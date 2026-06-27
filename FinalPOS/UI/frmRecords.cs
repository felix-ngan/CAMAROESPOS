using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;
using FinalPOS.Domain;
using FinalPOS.Presentation;

namespace FinalPOS
{
    public partial class frmRecords : Form, IRecordsView
    {
        private readonly RecordsPresenter _presenter;
        private TextBox txtWalletSearch;
        private DateTimePicker dtWalletFrom;
        private DateTimePicker dtWalletTo;
        private DataGridView dgvCustomerWallets;
        private DataGridView dgvWalletTransactions;

        // Consolidation
        private DateTimePicker dtConsolidationFrom;
        private DateTimePicker dtConsolidationTo;
        private DataGridView dgvConsolidation;
        private Chart chartConsolidation;

        public frmRecords()
        {
            InitializeComponent();
            InitializeWalletTab();
            InitializeConsolidationTab();
            this.Load += (s, e) => {
                FinalPOS.UI.ThemeManager.ApplyTheme(this);
                FinalPOS.UI.LanguageManager.ApplyLanguage(this);
            };
            
            // Initialisation des dépôts et du présentateur
            string connStr = new DBConnection().MyConnection();
            var reportRepo = new Data.ReportRepository(connStr);
            var productRepo = new Data.ProductRepository(connStr);
            var stockRepo = new Data.StockRepository(connStr);
            
            _presenter = new RecordsPresenter(this, reportRepo, productRepo, stockRepo);
        }

        private void InitializeWalletTab()
        {
            // 1. Create the TabPage
            MetroFramework.Controls.MetroTabPage tabPageWallet = new MetroFramework.Controls.MetroTabPage();
            tabPageWallet.Text = "Portefeuilles";
            tabPageWallet.Name = "tabPageWallet";

            // 2. Search Panel
            Panel pnlSearch = new Panel();
            pnlSearch.Dock = DockStyle.Top;
            pnlSearch.Height = 50;

            Label lblSearch = new Label();
            lblSearch.Text = "Rechercher téléphone :";
            lblSearch.Location = new Point(10, 15);
            lblSearch.AutoSize = true;
            lblSearch.Font = new Font("Segoe UI", 9.5F, FontStyle.Bold);

            txtWalletSearch = new TextBox();
            txtWalletSearch.Location = new Point(160, 12);
            txtWalletSearch.Width = 150;
            txtWalletSearch.Font = new Font("Segoe UI", 10F);
            txtWalletSearch.TextChanged += (s, e) => {
                _presenter.LoadCustomerWallets();
                _presenter.LoadWalletTransactions();
            };

            Label lblFrom = new Label();
            lblFrom.Text = "Du :";
            lblFrom.Location = new Point(330, 15);
            lblFrom.AutoSize = true;
            lblFrom.Font = new Font("Segoe UI", 9.5F, FontStyle.Bold);

            dtWalletFrom = new DateTimePicker();
            dtWalletFrom.Location = new Point(365, 12);
            dtWalletFrom.Width = 110;
            dtWalletFrom.Format = DateTimePickerFormat.Short;

            Label lblTo = new Label();
            lblTo.Text = "Au :";
            lblTo.Location = new Point(485, 15);
            lblTo.AutoSize = true;
            lblTo.Font = new Font("Segoe UI", 9.5F, FontStyle.Bold);

            dtWalletTo = new DateTimePicker();
            dtWalletTo.Location = new Point(515, 12);
            dtWalletTo.Width = 110;
            dtWalletTo.Format = DateTimePickerFormat.Short;

            Button btnLoad = new Button();
            btnLoad.Text = "Charger";
            btnLoad.Location = new Point(640, 10);
            btnLoad.Width = 90;
            btnLoad.Height = 28;
            btnLoad.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            btnLoad.Click += (s, e) => {
                _presenter.LoadCustomerWallets();
                _presenter.LoadWalletTransactions();
            };

            pnlSearch.Controls.Add(lblSearch);
            pnlSearch.Controls.Add(txtWalletSearch);
            pnlSearch.Controls.Add(lblFrom);
            pnlSearch.Controls.Add(dtWalletFrom);
            pnlSearch.Controls.Add(lblTo);
            pnlSearch.Controls.Add(dtWalletTo);
            pnlSearch.Controls.Add(btnLoad);

            // 3. Grid Container (Split Panel or TableLayoutPanel)
            TableLayoutPanel layoutGrid = new TableLayoutPanel();
            layoutGrid.Dock = DockStyle.Fill;
            layoutGrid.ColumnCount = 2;
            layoutGrid.RowCount = 1;
            layoutGrid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 40F)); // Wallets list
            layoutGrid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 60F)); // Transaction history

            // 4. Left Grid (Wallets)
            Panel pnlLeft = new Panel();
            pnlLeft.Dock = DockStyle.Fill;
            Label lblLeftTitle = new Label();
            lblLeftTitle.Text = "Soldes des Portefeuilles";
            lblLeftTitle.Dock = DockStyle.Top;
            lblLeftTitle.Height = 25;
            lblLeftTitle.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            lblLeftTitle.TextAlign = ContentAlignment.MiddleLeft;

            dgvCustomerWallets = new DataGridView();
            dgvCustomerWallets.Dock = DockStyle.Fill;
            dgvCustomerWallets.AllowUserToAddRows = false;
            dgvCustomerWallets.AllowUserToDeleteRows = false;
            dgvCustomerWallets.ReadOnly = true;
            dgvCustomerWallets.RowHeadersVisible = false;
            dgvCustomerWallets.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgvCustomerWallets.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dgvCustomerWallets.Columns.Add("colNum", "#");
            dgvCustomerWallets.Columns.Add("colPhone", "Numéro Client");
            dgvCustomerWallets.Columns.Add("colBalance", "Solde Porte-monnaie");
            dgvCustomerWallets.Columns[0].Width = 30;

            pnlLeft.Controls.Add(dgvCustomerWallets);
            pnlLeft.Controls.Add(lblLeftTitle);

            // 5. Right Grid (Transactions)
            Panel pnlRight = new Panel();
            pnlRight.Dock = DockStyle.Fill;
            Label lblRightTitle = new Label();
            lblRightTitle.Text = "Historique des Transactions";
            lblRightTitle.Dock = DockStyle.Top;
            lblRightTitle.Height = 25;
            lblRightTitle.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            lblRightTitle.TextAlign = ContentAlignment.MiddleLeft;

            dgvWalletTransactions = new DataGridView();
            dgvWalletTransactions.Dock = DockStyle.Fill;
            dgvWalletTransactions.AllowUserToAddRows = false;
            dgvWalletTransactions.AllowUserToDeleteRows = false;
            dgvWalletTransactions.ReadOnly = true;
            dgvWalletTransactions.RowHeadersVisible = false;
            dgvWalletTransactions.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgvWalletTransactions.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dgvWalletTransactions.Columns.Add("colTxNum", "#");
            dgvWalletTransactions.Columns.Add("colTxDate", "Date & Heure");
            dgvWalletTransactions.Columns.Add("colTxPhone", "Téléphone");
            dgvWalletTransactions.Columns.Add("colTxNo", "Facture");
            dgvWalletTransactions.Columns.Add("colTxType", "Opération");
            dgvWalletTransactions.Columns.Add("colTxAmt", "Montant");
            dgvWalletTransactions.Columns[0].Width = 30;
            dgvWalletTransactions.Columns[1].Width = 120;

            pnlRight.Controls.Add(dgvWalletTransactions);
            pnlRight.Controls.Add(lblRightTitle);

            // Add panels to layout grid
            layoutGrid.Controls.Add(pnlLeft, 0, 0);
            layoutGrid.Controls.Add(pnlRight, 1, 0);

            // Add search and grids to tab page
            tabPageWallet.Controls.Add(layoutGrid);
            tabPageWallet.Controls.Add(pnlSearch);

            // Add tab page to tab control
            metroTabControl1.TabPages.Add(tabPageWallet);
            
            // Set margins/padding if needed
            layoutGrid.Padding = new Padding(5);
        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {
            this.Dispose();
        }

        // --- Propriétés d'interface de IRecordsView ---

        // Top Selling
        public string TopSellingFilter
        {
            get => cboTopSelect.Text;
            set => cboTopSelect.Text = value;
        }

        public DateTime TopSellingDateFrom
        {
            get => dt1.Value;
            set => dt1.Value = value;
        }

        public DateTime TopSellingDateTo
        {
            get => dt2.Value;
            set => dt2.Value = value;
        }

        public void LoadTopSellingItems(IEnumerable<SoldItem> items)
        {
            dataGridView1.Rows.Clear();
            int i = 0;
            foreach (var item in items)
            {
                i++;
                dataGridView1.Rows.Add(
                    i, 
                    item.ProductCode, 
                    item.Description, 
                    item.Qty, 
                    item.Total.ToString("#,##0") + " FCFA"
                );
            }
        }

        public void LoadTopSellingChart(IEnumerable<SoldItem> items, string yValueMember)
        {
            chart1.Series[0].Points.Clear();
            chart1.Series[0].ChartType = SeriesChartType.Doughnut;
            chart1.Series[0].Name = "MEILLEURES VENTES";
            chart1.Series[0].XValueMember = "pcode";
            chart1.Series[0]["PieLabelStyle"] = "Outside";
            chart1.Series[0].BorderColor = System.Drawing.Color.Gray;
            chart1.Series[0].IsValueShownAsLabel = true;

            foreach (var item in items)
            {
                double value = (yValueMember == "qty") ? item.Qty : item.Total;
                int pointIndex = chart1.Series[0].Points.AddXY(item.ProductCode, value);
                chart1.Series[0].Points[pointIndex].Label = value.ToString("#,##0");
            }
        }

        // Sold Items
        public DateTime SoldItemsDateFrom
        {
            get => date1.Value;
            set => date1.Value = value;
        }

        public DateTime SoldItemsDateTo
        {
            get => date2.Value;
            set => date2.Value = value;
        }

        public void LoadSoldItemsGrouped(IEnumerable<SoldItem> items, double totalAmount)
        {
            dataGridView2.Rows.Clear();
            int i = 0;
            foreach (var item in items)
            {
                i++;
                dataGridView2.Rows.Add(
                    i, 
                    item.ProductCode, 
                    item.Description, 
                    item.Price.ToString("#,##0") + " FCFA", 
                    item.Qty, 
                    item.Discount, 
                    item.Total
                );
            }
            lblTotal.Text = totalAmount.ToString("#,##0") + " FCFA";
        }

        // Inventory
        public void LoadInventory(IEnumerable<Product> items)
        {
            dataGridView4.Rows.Clear();
            int i = 0;
            foreach (var prod in items)
            {
                i++;
                dataGridView4.Rows.Add(
                    i, 
                    prod.ProductCode, 
                    prod.Barcode, 
                    prod.Description, 
                    prod.BrandName, 
                    prod.CategoryName, 
                    prod.Price, 
                    prod.Reorder, 
                    prod.Qty
                );
            }
        }

        // Critical Items
        public void LoadCriticalItems(IEnumerable<Product> items)
        {
            dataGridView3.Rows.Clear();
            int i = 0;
            foreach (var prod in items)
            {
                i++;
                dataGridView3.Rows.Add(
                    i, 
                    prod.ProductCode, 
                    prod.Barcode, 
                    prod.Description, 
                    prod.BrandName, 
                    prod.CategoryName, 
                    prod.Price, 
                    prod.Reorder, 
                    prod.Qty
                );
            }
        }

        // Stock In History
        public DateTime StockInDateFrom
        {
            get => dateTimePicker4.Value;
            set => dateTimePicker4.Value = value;
        }

        public DateTime StockInDateTo
        {
            get => dateTimePicker3.Value;
            set => dateTimePicker3.Value = value;
        }

        public void LoadStockInHistory(IEnumerable<Stock> items)
        {
            dataGridView6.Rows.Clear();
            int i = 0;
            foreach (var st in items)
            {
                i++;
                dataGridView6.Rows.Add(
                    i, 
                    st.Id, 
                    st.RefNo, 
                    st.ProductCode, 
                    st.Description, 
                    st.Qty, 
                    st.StockInDate.ToShortDateString(), 
                    st.StockInBy
                );
            }
        }

        // Cancelled Orders
        public DateTime CancelledOrdersDateFrom
        {
            get => dateTimePicker1.Value;
            set => dateTimePicker1.Value = value;
        }

        public DateTime CancelledOrdersDateTo
        {
            get => dateTimePicker2.Value;
            set => dateTimePicker2.Value = value;
        }

        public void LoadCancelledOrders(IEnumerable<CancelledOrder> items)
        {
            dataGridView5.Rows.Clear();
            int i = 0;
            foreach (var ord in items)
            {
                i++;
                dataGridView5.Rows.Add(
                    i, 
                    ord.TransNo, 
                    ord.ProductCode, 
                    ord.Description, 
                    ord.Price, 
                    ord.Qty, 
                    ord.Total, 
                    ord.SoldDate.ToString(), 
                    ord.VoidBy, 
                    ord.CancelledBy, 
                    ord.Reason, 
                    ord.Action
                );
            }
        }
        // Customer Wallets & Change Management
        public string WalletSearchPhone
        {
            get => txtWalletSearch.Text;
            set => txtWalletSearch.Text = value;
        }

        public void LoadCustomerWallets(IEnumerable<CustomerWallet> wallets)
        {
            dgvCustomerWallets.Rows.Clear();
            int i = 0;
            foreach (var wallet in wallets)
            {
                i++;
                dgvCustomerWallets.Rows.Add(
                    i,
                    wallet.Phone,
                    wallet.Balance.ToString("#,##0") + " FCFA"
                );
            }
        }

        // Wallet Transactions
        public DateTime WalletDateFrom
        {
            get => dtWalletFrom.Value;
            set => dtWalletFrom.Value = value;
        }

        public DateTime WalletDateTo
        {
            get => dtWalletTo.Value;
            set => dtWalletTo.Value = value;
        }

        public void LoadWalletTransactions(IEnumerable<WalletTransaction> transactions)
        {
            dgvWalletTransactions.Rows.Clear();
            int i = 0;
            foreach (var tx in transactions)
            {
                i++;
                dgvWalletTransactions.Rows.Add(
                    i,
                    tx.CreatedAt.ToString("yyyy-MM-dd HH:mm:ss"),
                    tx.Phone,
                    tx.TransNo,
                    tx.Type == "Credit" ? "Épargne" : "Débit",
                    tx.Amount.ToString("#,##0") + " FCFA"
                );
            }
        }

        // --- Gestionnaires d'événements UI ---

        // Meilleures ventes - Charger
        private void linkLabel7_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            if (string.IsNullOrEmpty(cboTopSelect.Text))
            {
                MessageBox.Show("Veuillez sélectionner un tri.", "AVERTISSEMENT", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                return;
            }
            _presenter.LoadTopSelling();
        }

        // Meilleures ventes - Imprimer
        private void linkLabel5_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            frmInventoryReport f = new frmInventoryReport();
            string d1 = dt1.Value.ToString("yyyy-MM-dd 00:00:00");
            string d2 = dt2.Value.ToString("yyyy-MM-dd 23:59:59");
            if (cboTopSelect.Text == "TRIER PAR QTÉ")
            {
                f.LoadTopSelling("select top 10 pcode , pdesc, sum(qty) as qty from ViewSoldItems where sdate between '" + d1 + "' and '" + d2 + "' and status like 'Sold' group by pcode, pdesc order by qty desc ", "Du : " + dt1.Value.ToShortDateString() + " Au : " + dt2.Value.ToShortDateString() , "TRIER PAR QUANTITÉ");
            }
            else if (cboTopSelect.Text == "TRIER PAR MONTANT TOTAL")
            {
                f.LoadTopSelling("select top 10 pcode , pdesc, isnull(sum(qty),0) as qty, isnull(sum(total),0) as total  from ViewSoldItems where sdate between '" + d1 + "' and '" + d2 + "' and status like 'Sold' group by pcode, pdesc order by total desc ", "Du: " + dt1.Value.ToShortDateString() + " Au: " + dt2.Value.ToShortDateString() , "TRIER PAR VENTES TOTALES" );
            }
            f.ShowDialog();
        }

        // Articles Vendus - Charger
        private void linkLabel8_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            try
            {
                _presenter.LoadSoldItemsGrouped();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Avertissement", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        // Articles Vendus - Imprimer
        private void linkLabel6_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            frmInventoryReport f = new frmInventoryReport();
            string d1 = date1.Value.ToString("yyyy-MM-dd 00:00:00");
            string d2 = date2.Value.ToString("yyyy-MM-dd 23:59:59");
            f.LoadSoldItems(" select c.pcode, p.pdesc, c.price, sum(c.qty) as tot_qty, sum(c.disc) as tot_disc, sum(c.total) as total from tbl_Cart as c inner join tbl_Products as p on c.pcode = p.pcode where status like 'Sold' and sdate between '" + d1 + "' and '" + d2 + "' group by c.pcode , p.pdesc,c.price   ", "Du : " + date1.Value.ToShortDateString() + " Au : " + date2.Value.ToShortDateString());
            f.ShowDialog();
        }

        // Articles Vendus - Graphique
        private void linkLabel9_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            frmChart f = new frmChart();
            f.lblTitle.Text = "ARTICLES VENDUS [" + date1.Value.ToShortDateString() + " - " + date2.Value.ToShortDateString() + "]";
            f.LoadChartSold("select p.pdesc, sum(c.total) as total from tbl_Cart as c inner join tbl_Products as p on c.pcode = p.pcode where status like 'Sold' and sdate between '" + date1.Value.ToString("yyyy-MM-dd 00:00:00") + "' and '" + date2.Value.ToString("yyyy-MM-dd 23:59:59") + "' group by p.pdesc order by total desc");
            f.ShowDialog();
        }

        // Inventaire - Charger (méthode de chargement appelée de l'extérieur ou via d'autres événements)
        public void LoadInventory()
        {
            _presenter.LoadInventory();
        }

        // Méthodes de compatibilité pour Form1.cs
        public void CancelledOrders()
        {
            _presenter.LoadCancelledOrders();
        }

        public void LoadStockInHistory()
        {
            _presenter.LoadStockInHistory();
        }

        public void LoadRecords()
        {
            _presenter.LoadTopSelling();
            _presenter.LoadCustomerWallets();
            _presenter.LoadWalletTransactions();
        }

        // Inventaire - Imprimer
        private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            frmInventoryReport frm = new frmInventoryReport();
            frm.LoadReport();
            frm.ShowDialog();
        }

        // Articles Critiques - Charger
        public void LoadCriticalItems()
        {
            try
            {
                _presenter.LoadCriticalItems();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Avertissement", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        // Stock In History - Charger
        private void linkLabel10_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            _presenter.LoadStockInHistory();
        }

        // Stock In History - Imprimer
        private void linkLabel3_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            frmInventoryReport frm = new frmInventoryReport();
            string param = "Période couverte : " + dateTimePicker4.Value.ToShortDateString() + " - " + dateTimePicker3.Value.ToShortDateString();
            frm.LoadStocksInReports("select * from ViewStocks where cast(sdate as date) between '" + dateTimePicker4.Value.ToString("yyyy-MM-dd") + "' and '" + dateTimePicker3.Value.ToString("yyyy-MM-dd") + "' and status like 'Done'", param);
            frm.ShowDialog();
        }

        // Cancelled Orders - Charger
        private void linkLabel11_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            _presenter.LoadCancelledOrders();
        }

        // Cancelled Orders - Imprimer
        private void linkLabel2_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            frmInventoryReport f = new frmInventoryReport();
            string param = "Période couverte : " + dateTimePicker1.Value.ToShortDateString() + " - " + dateTimePicker2.Value.ToShortDateString();
            f.LoadCancelledOrders("select * from CancelledOrder where sdate between '" + dateTimePicker1.Value.ToString("yyyy-MM-dd 00:00:00") + "' and '" + dateTimePicker2.Value.ToString("yyyy-MM-dd 23:59:59") + "' ", param);
            f.ShowDialog();
        }

        private void cboTopSelect_KeyPress(object sender, KeyPressEventArgs e)
        {
            e.Handled = true;
        }

        // Événements inutilisés à conserver
        private void panel7_Paint(object sender, PaintEventArgs e) { }
        private void panel2_Paint(object sender, PaintEventArgs e) { }
        private void btnLoad_Click(object sender, EventArgs e) { }
        private void button1_Click(object sender, EventArgs e) { }
        private void button2_Click(object sender, EventArgs e) { }
        private void dataGridView2_CellContentClick(object sender, DataGridViewCellEventArgs e) { }

        // --- Consolidation Implementation ---
        public DateTime ConsolidationDateFrom
        {
            get => dtConsolidationFrom.Value;
            set => dtConsolidationFrom.Value = value;
        }

        public DateTime ConsolidationDateTo
        {
            get => dtConsolidationTo.Value;
            set => dtConsolidationTo.Value = value;
        }

        public void LoadConsolidation(IEnumerable<StoreRevenue> revenues)
        {
            dgvConsolidation.Rows.Clear();
            chartConsolidation.Series[0].Points.Clear();
            chartConsolidation.Series[0].Name = "Chiffre d'affaires par Boutique";

            foreach (var rev in revenues)
            {
                string storeLabel = rev.StoreId.HasValue ? "Boutique " + rev.StoreId.Value : "Inconnu";
                string lastActiveStr = rev.LastActivity.HasValue ? rev.LastActivity.Value.ToString("yyyy-MM-dd HH:mm:ss") : "Jamais";
                dgvConsolidation.Rows.Add(
                    rev.StoreId.HasValue ? rev.StoreId.Value.ToString() : "Inconnu",
                    rev.Revenue.ToString("#,##0") + " FCFA",
                    lastActiveStr
                );

                int pointIndex = chartConsolidation.Series[0].Points.AddXY(storeLabel, rev.Revenue);
                chartConsolidation.Series[0].Points[pointIndex].Label = rev.Revenue.ToString("#,##0") + " FCFA";
            }
        }

        private string GetCentralConnectionString()
        {
            try
            {
                string connStr = new DBConnection().MyConnection();
                using (var cn = new System.Data.SqlClient.SqlConnection(connStr))
                {
                    using (var cm = new System.Data.SqlClient.SqlCommand("select setting_value from tbl_LocalSettings where setting_key = 'CentralConnectionString'", cn))
                    {
                        cn.Open();
                        object result = cm.ExecuteScalar();
                        return result?.ToString() ?? "";
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("GetCentralConnectionString error: " + ex.Message);
                return "";
            }
        }

        private void InitializeConsolidationTab()
        {
            // 1. Create the TabPage
            MetroFramework.Controls.MetroTabPage tabPageConsolidation = new MetroFramework.Controls.MetroTabPage();
            tabPageConsolidation.Text = "Consolidation";
            tabPageConsolidation.Name = "tabPageConsolidation";

            // 2. Search Panel
            Panel pnlSearch = new Panel();
            pnlSearch.Dock = DockStyle.Top;
            pnlSearch.Height = 50;

            Label lblFrom = new Label();
            lblFrom.Text = "Du :";
            lblFrom.Name = "lblConsolidationFrom";
            lblFrom.Location = new Point(10, 15);
            lblFrom.AutoSize = true;
            lblFrom.Font = new Font("Segoe UI", 9.5F, FontStyle.Bold);

            dtConsolidationFrom = new DateTimePicker();
            dtConsolidationFrom.Location = new Point(45, 12);
            dtConsolidationFrom.Width = 110;
            dtConsolidationFrom.Format = DateTimePickerFormat.Short;

            Label lblTo = new Label();
            lblTo.Text = "Au :";
            lblTo.Name = "lblConsolidationTo";
            lblTo.Location = new Point(170, 15);
            lblTo.AutoSize = true;
            lblTo.Font = new Font("Segoe UI", 9.5F, FontStyle.Bold);

            dtConsolidationTo = new DateTimePicker();
            dtConsolidationTo.Location = new Point(200, 12);
            dtConsolidationTo.Width = 110;
            dtConsolidationTo.Format = DateTimePickerFormat.Short;

            Button btnLoad = new Button();
            btnLoad.Text = "Charger";
            btnLoad.Name = "btnConsolidationLoad";
            btnLoad.Location = new Point(325, 10);
            btnLoad.Width = 90;
            btnLoad.Height = 28;
            btnLoad.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            btnLoad.Click += (s, e) => {
                string centralConnStr = GetCentralConnectionString();
                if (string.IsNullOrWhiteSpace(centralConnStr))
                {
                    MessageBox.Show("La chaîne de connexion centrale n'est pas configurée dans les paramètres de la boutique.", "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }
                _presenter.LoadConsolidation(centralConnStr);
            };

            pnlSearch.Controls.Add(lblFrom);
            pnlSearch.Controls.Add(dtConsolidationFrom);
            pnlSearch.Controls.Add(lblTo);
            pnlSearch.Controls.Add(dtConsolidationTo);
            pnlSearch.Controls.Add(btnLoad);

            // 3. Grid & Chart Container
            TableLayoutPanel layoutGrid = new TableLayoutPanel();
            layoutGrid.Dock = DockStyle.Fill;
            layoutGrid.ColumnCount = 2;
            layoutGrid.RowCount = 1;
            layoutGrid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 45F)); // Table list
            layoutGrid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 55F)); // Chart comparison

            // 4. Left Grid (Consolidation)
            Panel pnlLeft = new Panel();
            pnlLeft.Dock = DockStyle.Fill;
            Label lblLeftTitle = new Label();
            lblLeftTitle.Text = "Chiffre d'affaires par Boutique";
            lblLeftTitle.Name = "lblConsolidationLeftTitle";
            lblLeftTitle.Dock = DockStyle.Top;
            lblLeftTitle.Height = 25;
            lblLeftTitle.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            lblLeftTitle.TextAlign = ContentAlignment.MiddleLeft;

            dgvConsolidation = new DataGridView();
            dgvConsolidation.Dock = DockStyle.Fill;
            dgvConsolidation.AllowUserToAddRows = false;
            dgvConsolidation.AllowUserToDeleteRows = false;
            dgvConsolidation.ReadOnly = true;
            dgvConsolidation.RowHeadersVisible = false;
            dgvConsolidation.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgvConsolidation.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            
            // Add columns
            var colId = new DataGridViewTextBoxColumn();
            colId.Name = "colStoreId";
            colId.HeaderText = "Boutique ID";
            var colRev = new DataGridViewTextBoxColumn();
            colRev.Name = "colRevenue";
            colRev.HeaderText = "Chiffre d'affaires";
            var colActivity = new DataGridViewTextBoxColumn();
            colActivity.Name = "colLastActivity";
            colActivity.HeaderText = "Dernière Vente";
            dgvConsolidation.Columns.AddRange(new DataGridViewColumn[] { colId, colRev, colActivity });

            pnlLeft.Controls.Add(dgvConsolidation);
            pnlLeft.Controls.Add(lblLeftTitle);

            // 5. Right Chart (Consolidation)
            Panel pnlRight = new Panel();
            pnlRight.Dock = DockStyle.Fill;
            
            chartConsolidation = new Chart();
            chartConsolidation.Dock = DockStyle.Fill;
            
            ChartArea chartArea = new ChartArea("ConsolidationArea");
            chartConsolidation.ChartAreas.Add(chartArea);
            
            Series series = new Series("Revenues");
            series.ChartType = SeriesChartType.Column;
            series.XValueType = ChartValueType.String;
            series.YValueType = ChartValueType.Double;
            series.IsValueShownAsLabel = true;
            series.Font = new Font("Segoe UI", 8.5F, FontStyle.Bold);
            chartConsolidation.Series.Add(series);
            
            Legend legend = new Legend("ConsolidationLegend");
            legend.Docking = Docking.Top;
            chartConsolidation.Legends.Add(legend);
            
            pnlRight.Controls.Add(chartConsolidation);

            // Add panels to layout grid
            layoutGrid.Controls.Add(pnlLeft, 0, 0);
            layoutGrid.Controls.Add(pnlRight, 1, 0);

            // Add search and grids to tab page
            tabPageConsolidation.Controls.Add(layoutGrid);
            tabPageConsolidation.Controls.Add(pnlSearch);

            // Add tab page to tab control
            metroTabControl1.TabPages.Add(tabPageConsolidation);
            
            layoutGrid.Padding = new Padding(5);
        }
    }
}
