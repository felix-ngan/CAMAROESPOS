using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using CamaroesPOS.Domain.Entities;

namespace CamaroesPOS.UI.Views
{
    public partial class ReceiptWindow : Window
    {
        private string _transNo;
        private IEnumerable<CartItem> _items;
        private double _totalAmount;
        private double _receivedAmount;
        private double _changeAmount;
        private string _paymentMode;
        private double _discountAmount;
        
        public ReceiptWindow(string transNo, IEnumerable<CartItem> items, double totalAmount, double receivedAmount, double changeAmount, string paymentMode, double discountAmount = 0)
        {
            InitializeComponent();
            
            _transNo = transNo;
            _items = items;
            _totalAmount = totalAmount;
            _receivedAmount = receivedAmount;
            _changeAmount = changeAmount;
            _paymentMode = paymentMode;
            _discountAmount = discountAmount;

            TxtTransNo.Text = transNo;
            TxtDate.Text = DateTime.Now.ToString("dd/MM/yyyy HH:mm");
            TxtMode.Text = $"Mode de Paiement : {paymentMode}";
            
            ItemsList.ItemsSource = items;
            
            if (_discountAmount > 0)
            {
                double subTotal = totalAmount + discountAmount;
                GridSubTotal.Visibility = Visibility.Visible;
                GridDiscount.Visibility = Visibility.Visible;
                TxtSubTotal.Text = $"{subTotal:N0} FCFA";
                TxtDiscount.Text = $"-{discountAmount:N0} FCFA";
            }
            
            TxtTotal.Text = $"{totalAmount:N0} FCFA";
            
            if (paymentMode == "Espèces")
            {
                TxtReceived.Text = $"{receivedAmount:N0} FCFA";
                TxtChange.Text = $"{changeAmount:N0} FCFA";
            }
            else
            {
                GridReceived.Visibility = Visibility.Collapsed;
                GridChange.Visibility = Visibility.Collapsed;
            }
        }

        private void BtnClose_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void BtnExportPdf_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var dialog = new Microsoft.Win32.SaveFileDialog
                {
                    Filter = "Fichiers PDF (*.pdf)|*.pdf",
                    DefaultExt = ".pdf",
                    FileName = $"Recu_{_transNo}_{DateTime.Now:yyyyMMdd}.pdf"
                };

                if (dialog.ShowDialog() == true)
                {
                    string cashier = Application.Current.Properties.Contains("CurrentUserName") 
                        ? Application.Current.Properties["CurrentUserName"].ToString() 
                        : "Caissier";
                        
                    var pdfService = new CamaroesPOS.UI.Services.PdfService();
                    pdfService.GenerateReceiptPdf(_transNo, cashier, _totalAmount, _receivedAmount, _changeAmount, "", _items, dialog.FileName, _discountAmount);
                    
                    MessageBox.Show("Le reçu a été exporté avec succès !", "Succès", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erreur lors de l'exportation PDF : {ex.Message}", "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnPrint_Click(object sender, RoutedEventArgs e)
        {
            PrintDialog printDialog = new PrintDialog();
            if (printDialog.ShowDialog() == true)
            {
                PrintArea.Measure(new Size(printDialog.PrintableAreaWidth, double.PositiveInfinity));
                PrintArea.Arrange(new Rect(new Point(0, 0), PrintArea.DesiredSize));
                
                printDialog.PrintVisual(PrintArea, "Reçu de Caisse CAMAROES POS");
            }
        }
    }
}
