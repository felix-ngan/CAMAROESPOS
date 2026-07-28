using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using CamaroesPOS.Domain.Entities;

namespace CamaroesPOS.UI.Views
{
    public partial class ReceiptWindow : Window
    {
        public ReceiptWindow(string transNo, IEnumerable<CartItem> items, double totalAmount, double receivedAmount, double changeAmount, string paymentMode)
        {
            InitializeComponent();
            
            TxtTransNo.Text = transNo;
            TxtDate.Text = DateTime.Now.ToString("dd/MM/yyyy HH:mm");
            TxtMode.Text = $"Mode de Paiement : {paymentMode}";
            
            ItemsList.ItemsSource = items;
            
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

        private void BtnPrint_Click(object sender, RoutedEventArgs e)
        {
            PrintDialog printDialog = new PrintDialog();
            if (printDialog.ShowDialog() == true)
            {
                // Measure and arrange the print area to ensure it prints correctly
                PrintArea.Measure(new Size(printDialog.PrintableAreaWidth, double.PositiveInfinity));
                PrintArea.Arrange(new Rect(new Point(0, 0), PrintArea.DesiredSize));
                
                printDialog.PrintVisual(PrintArea, "Reçu de Caisse CAMAROES POS");
            }
        }
    }
}
