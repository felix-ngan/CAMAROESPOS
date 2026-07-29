using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using CamaroesPOS.Domain.Entities;

namespace CamaroesPOS.UI.Services
{
    public class PdfService
    {
        public void GenerateReceiptPdf(string transactionNo, string cashier, double totalAmount, double receivedAmount, double changeAmount, string customerPhone, IEnumerable<CartItem> items, string path)
        {
            Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A5);
                    page.Margin(20);
                    page.PageColor(Colors.White);
                    page.DefaultTextStyle(x => x.FontSize(10));

                    page.Header().Element(c => ComposeReceiptHeader(c, transactionNo, cashier, customerPhone));
                    page.Content().Element(c => ComposeReceiptContent(c, items, totalAmount, receivedAmount, changeAmount));
                    page.Footer().Element(ComposeReceiptFooter);
                });
            })
            .GeneratePdf(path);
        }

        private void ComposeReceiptHeader(IContainer container, string transactionNo, string cashier, string customerPhone)
        {
            container.Column(column =>
            {
                column.Item().AlignCenter().Text("CAMAROES POS").FontSize(20).SemiBold().FontColor(Colors.Blue.Darken2);
                column.Item().AlignCenter().Text("Reçu de Caisse").FontSize(14).SemiBold();
                column.Item().PaddingTop(10).Row(row =>
                {
                    row.RelativeItem().Column(col =>
                    {
                        col.Item().Text($"Transaction : {transactionNo}");
                        col.Item().Text($"Date : {DateTime.Now:dd/MM/yyyy HH:mm}");
                        col.Item().Text($"Caissier : {cashier}");
                        if (!string.IsNullOrEmpty(customerPhone))
                        {
                            col.Item().Text($"Client (Tél) : {customerPhone}");
                        }
                    });
                });
                column.Item().PaddingVertical(5).LineHorizontal(1).LineColor(Colors.Grey.Lighten2);
            });
        }

        private void ComposeReceiptContent(IContainer container, IEnumerable<CartItem> items, double totalAmount, double receivedAmount, double changeAmount)
        {
            container.PaddingVertical(10).Column(column =>
            {
                column.Item().Table(table =>
                {
                    table.ColumnsDefinition(columns =>
                    {
                        columns.RelativeColumn(3); // Description
                        columns.RelativeColumn(1); // Qty
                        columns.RelativeColumn(1); // Price
                        columns.RelativeColumn(1); // Total
                    });

                    table.Header(header =>
                    {
                        header.Cell().Text("Article").SemiBold();
                        header.Cell().AlignRight().Text("Qté").SemiBold();
                        header.Cell().AlignRight().Text("Prix").SemiBold();
                        header.Cell().AlignRight().Text("Total").SemiBold();
                        
                        header.Cell().ColumnSpan(4).PaddingVertical(5).BorderBottom(1).BorderColor(Colors.Grey.Lighten2);
                    });

                    foreach (var item in items)
                    {
                        table.Cell().Text(item.ProductDescription);
                        table.Cell().AlignRight().Text(item.Qty.ToString());
                        table.Cell().AlignRight().Text($"{item.Price:N0}");
                        table.Cell().AlignRight().Text($"{item.Total:N0}");
                    }
                });

                column.Item().PaddingVertical(10).LineHorizontal(1).LineColor(Colors.Grey.Lighten2);
                
                column.Item().AlignRight().Text($"Total : {totalAmount:N0} FCFA").FontSize(14).Bold();
                if (receivedAmount > 0)
                {
                    column.Item().AlignRight().Text($"Reçu : {receivedAmount:N0} FCFA");
                    column.Item().AlignRight().Text($"Rendu : {changeAmount:N0} FCFA");
                }
            });
        }

        private void ComposeReceiptFooter(IContainer container)
        {
            container.AlignCenter().Text("Merci de votre visite !").Italic();
        }

        public void GenerateSalesReportPdf(DateTime startDate, DateTime endDate, double totalSales, double totalDiscounts, int totalTransactions, IEnumerable<SoldItem> soldItems, string path)
        {
            Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(30);
                    page.PageColor(Colors.White);
                    page.DefaultTextStyle(x => x.FontSize(11));

                    page.Header().Element(c => ComposeReportHeader(c, startDate, endDate));
                    page.Content().Element(c => ComposeReportContent(c, soldItems, totalSales, totalDiscounts, totalTransactions));
                    page.Footer().Element(ComposeReportFooter);
                });
            })
            .GeneratePdf(path);
        }

        private void ComposeReportHeader(IContainer container, DateTime startDate, DateTime endDate)
        {
            container.Row(row =>
            {
                row.RelativeItem().Column(column =>
                {
                    column.Item().Text("Rapport des Ventes").FontSize(24).SemiBold().FontColor(Colors.Blue.Darken2);
                    
                    var dateRange = startDate.Date == endDate.Date 
                        ? $"Date : {startDate:dd/MM/yyyy}" 
                        : $"Période : {startDate:dd/MM/yyyy} - {endDate:dd/MM/yyyy}";
                        
                    column.Item().Text(dateRange).FontSize(14);
                });
                row.ConstantItem(100).AlignRight().Text($"Généré le {DateTime.Now:dd/MM/yyyy}");
            });
        }

        private void ComposeReportContent(IContainer container, IEnumerable<SoldItem> items, double totalSales, double totalDiscounts, int totalTransactions)
        {
            container.PaddingVertical(20).Column(column =>
            {
                // Summary section
                column.Item().Background(Colors.Grey.Lighten4).Padding(10).Row(row =>
                {
                    row.RelativeItem().Column(col =>
                    {
                        col.Item().Text("Chiffre d'Affaires").SemiBold();
                        col.Item().Text($"{totalSales:N0} FCFA").FontSize(16).Bold().FontColor(Colors.Blue.Darken2);
                    });
                    row.RelativeItem().Column(col =>
                    {
                        col.Item().Text("Réductions Accordées").SemiBold();
                        col.Item().Text($"{totalDiscounts:N0} FCFA").FontSize(16).Bold().FontColor(Colors.Orange.Darken1);
                    });
                    row.RelativeItem().Column(col =>
                    {
                        col.Item().Text("Total Transactions").SemiBold();
                        col.Item().Text($"{totalTransactions}").FontSize(16).Bold();
                    });
                });

                column.Item().PaddingVertical(20).Text("Détails des Ventes").FontSize(16).SemiBold();

                // Details table
                column.Item().Table(table =>
                {
                    table.ColumnsDefinition(columns =>
                    {
                        columns.RelativeColumn(2); // Date
                        columns.RelativeColumn(2); // Trans No
                        columns.RelativeColumn(3); // Article
                        columns.RelativeColumn(1); // Qty
                        columns.RelativeColumn(1); // Prix
                        columns.RelativeColumn(1); // Total
                    });

                    table.Header(header =>
                    {
                        header.Cell().Text("Date").SemiBold();
                        header.Cell().Text("N° Trans.").SemiBold();
                        header.Cell().Text("Article").SemiBold();
                        header.Cell().AlignRight().Text("Qté").SemiBold();
                        header.Cell().AlignRight().Text("Prix").SemiBold();
                        header.Cell().AlignRight().Text("Total").SemiBold();

                        header.Cell().ColumnSpan(6).PaddingVertical(5).BorderBottom(1).BorderColor(Colors.Grey.Lighten2);
                    });

                    foreach (var item in items)
                    {
                        table.Cell().Text(item.SoldDate.ToString("dd/MM/yy HH:mm"));
                        table.Cell().Text(item.TransNo);
                        table.Cell().Text(item.Description);
                        table.Cell().AlignRight().Text(item.Qty.ToString());
                        table.Cell().AlignRight().Text($"{item.Price:N0}");
                        table.Cell().AlignRight().Text($"{item.Total:N0}");
                    }
                });
            });
        }

        private void ComposeReportFooter(IContainer container)
        {
            container.AlignCenter().Text(x =>
            {
                x.Span("Page ");
                x.CurrentPageNumber();
                x.Span(" sur ");
                x.TotalPages();
            });
        }
    }
}
