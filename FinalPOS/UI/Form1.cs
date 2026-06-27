using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Data.SqlClient;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Data.Common;
using Tulpep.NotificationWindow;
using FinalPOS.UI;

namespace FinalPOS
{
    public partial class Form1 : MetroFramework.Forms.MetroForm
    {
        SqlConnection cn = new SqlConnection();
        SqlCommand cm = new SqlCommand();
        DBConnection dbcon = new DBConnection();
        SqlDataReader dr;
        public string _pass , _username;   
        private bool _isSyncing = false;
        private System.Windows.Forms.Timer _autoSyncTimer;

        public Form1()
        {
            InitializeComponent();
            cn = new SqlConnection(dbcon.MyConnection());
            
            // Programmatically add Theme toggle and apply theme
            AddThemeToggle();
            AddLanguageSelector();
            AddSyncControls();

            NotifyCriticalItems();
            MyDashboard();
        }

        private void AddThemeToggle()
        {
            System.Windows.Forms.Label lblTheme = new System.Windows.Forms.Label();
            lblTheme.Text = "Mode Sombre";
            lblTheme.Size = new Size(110, 20);
            lblTheme.Location = new Point(45, 10);
            lblTheme.ForeColor = Color.White;
            lblTheme.Font = new Font("Segoe UI", 10F, FontStyle.Bold);

            MetroFramework.Controls.MetroToggle themeToggle = new MetroFramework.Controls.MetroToggle();
            themeToggle.Name = "themeToggle";
            themeToggle.Size = new Size(80, 20);
            themeToggle.Location = new Point(160, 10);
            themeToggle.Checked = ThemeManager.CurrentTheme == ThemeMode.Dark;
            themeToggle.CheckedChanged += (s, ev) => {
                ThemeManager.CurrentTheme = themeToggle.Checked ? ThemeMode.Dark : ThemeMode.Light;
                ThemeManager.ApplyTheme(this);
                lblTheme.ForeColor = Color.White;
                
                // Theme the active subform if any
                if (panel3.Controls.Count > 0 && panel3.Controls[0] is Form activeForm)
                {
                    ThemeManager.ApplyTheme(activeForm);
                }
            };

            panel2.Controls.Add(lblTheme);
            panel2.Controls.Add(themeToggle);

            // Apply default theme on start
            ThemeManager.ApplyTheme(this);
        }

        private void AddLanguageSelector()
        {
            // Keep panel4 at its original designer size/position so the Logout button remains fully visible.
            // Instead, resize and reposition the top section elements to make room.
            pictureBox1.Location = new Point(97, 60);
            pictureBox1.Size = new Size(90, 90);
            
            lblName.Location = new Point(lblName.Location.X, 155);
            lblRole.Location = new Point(lblRole.Location.X, 155);

            System.Windows.Forms.Label lblLang = new System.Windows.Forms.Label();
            lblLang.Name = "lblLang";
            lblLang.Text = "Langue";
            lblLang.Size = new Size(110, 20);
            lblLang.Location = new Point(45, 33);
            lblLang.ForeColor = Color.White;
            lblLang.Font = new Font("Segoe UI", 10F, FontStyle.Bold);

            System.Windows.Forms.ComboBox cboLang = new System.Windows.Forms.ComboBox();
            cboLang.Name = "cboLang";
            cboLang.Size = new Size(80, 20);
            cboLang.Location = new Point(160, 30);
            cboLang.DropDownStyle = ComboBoxStyle.DropDownList;
            cboLang.Items.AddRange(new object[] { "Français", "English" });
            cboLang.SelectedIndex = LanguageManager.CurrentLanguage == "English" ? 1 : 0;
            cboLang.Font = new Font("Segoe UI", 9F);

            cboLang.SelectedIndexChanged += (s, ev) => {
                LanguageManager.CurrentLanguage = cboLang.SelectedItem.ToString();
                ApplyCurrentLanguage();
            };

            panel2.Controls.Add(lblLang);
            panel2.Controls.Add(cboLang);
            ApplyCurrentLanguage();
        }

        private void ApplyCurrentLanguage()
        {
            LanguageManager.ApplyLanguage(this);
            
            // Translate language and theme label manually since they are added dynamically
            var lblLang = panel2.Controls["lblLang"] as System.Windows.Forms.Label;
            var lblTheme = panel2.Controls.Cast<Control>().FirstOrDefault(c => c.Text == "Mode Sombre" || c.Text == "Dark Mode") as System.Windows.Forms.Label;
            
            var btnSync = panel4.Controls["btnSync"] as System.Windows.Forms.Button;
            var lblSyncStatus = panel4.Controls["lblSyncStatus"] as System.Windows.Forms.Label;

            if (LanguageManager.CurrentLanguage == "English")
            {
                if (lblLang != null) lblLang.Text = "Language";
                if (lblTheme != null) lblTheme.Text = "Dark Mode";
                if (btnSync != null && btnSync.Text != "  Syncing...") btnSync.Text = "  Synchronize";
                if (lblSyncStatus != null)
                {
                    if (lblSyncStatus.Text.StartsWith("Dernière synchro :"))
                    {
                        lblSyncStatus.Text = lblSyncStatus.Text.Replace("Dernière synchro :", "Last Sync:");
                    }
                    else if (lblSyncStatus.Text == "Dernière synchro : Jamais")
                    {
                        lblSyncStatus.Text = "Last Sync: Never";
                    }
                }
            }
            else
            {
                if (lblLang != null) lblLang.Text = "Langue";
                if (lblTheme != null) lblTheme.Text = "Mode Sombre";
                if (btnSync != null && btnSync.Text != "  Synchronize") btnSync.Text = "  Synchroniser";
                if (lblSyncStatus != null)
                {
                    if (lblSyncStatus.Text.StartsWith("Last Sync:"))
                    {
                        lblSyncStatus.Text = lblSyncStatus.Text.Replace("Last Sync:", "Dernière synchro :");
                    }
                    else if (lblSyncStatus.Text == "Last Sync: Never")
                    {
                        lblSyncStatus.Text = "Dernière synchro : Jamais";
                    }
                }
            }

            // Apply translation to active subform
            if (panel3.Controls.Count > 0 && panel3.Controls[0] is Form activeForm)
            {
                LanguageManager.ApplyLanguage(activeForm);
                if (activeForm is frmDashboard)
                {
                    MyDashboard();
                }
            }
        }

        private void LoadSubForm(Form frm)
        {
            panel3.Controls.Clear();
            frm.TopLevel = false;
            frm.FormBorderStyle = FormBorderStyle.None;
            frm.Dock = DockStyle.Fill;
            panel3.Controls.Add(frm);
            ThemeManager.ApplyTheme(frm);
            LanguageManager.ApplyLanguage(frm);
            frm.BringToFront();
            frm.Show();
        }
        
        public void NotifyCriticalItems()
        {
            string critical = "";
            cn.Open();
            cm = new SqlCommand("select count(*) from  ViewCriticalItems  ", cn);

            int i = 0;
            string count = cm.ExecuteScalar().ToString();
            cn.Close();

            cn.Open();
            cm = new SqlCommand("select * from  ViewCriticalItems  ", cn);
            dr = cm.ExecuteReader();
            while(dr.Read())
            {
                i++;
                critical += i+ ".  "  + dr["pdesc"].ToString() + Environment.NewLine;
            }
            dr.Close();
            cn.Close();

            PopupNotifier popup = new PopupNotifier();
            popup.Image = Properties.Resources.cancel__2_;
            popup.TitleText = count + " ARTICLE(S) CRITIQUE(S)";
            popup.ContentText = critical;
            popup.Popup();
        }

        private void StocksButton_Click(object sender, EventArgs e)
        {
            frmStockIn frm = new frmStockIn();
            LoadSubForm(frm);
        }

        private void POSButton_Click(object sender, EventArgs e)
        {
            //frmPOS frm = new frmPOS();
            //frm.ShowDialog();
        }

        private void ManageBrandButton_Click(object sender, EventArgs e)
        {
            frmBrandList frm = new frmBrandList();
            LoadSubForm(frm);
        }

        private void UserSettingsButton_Click(object sender, EventArgs e)
        {
            frmUserAccounts frm = new frmUserAccounts(this);
            frm.txtUsername.Text = _username;
            LoadSubForm(frm);
        }

        private void RecordsButton_Click(object sender, EventArgs e)
        {
            frmRecords frm = new frmRecords();
            frm.LoadCriticalItems();
            frm.CancelledOrders();
            frm.LoadStockInHistory();
            frm.LoadInventory();
            frm.LoadRecords();
            LoadSubForm(frm);
        }

        private void ManageProductsButton_Click(object sender, EventArgs e)
        {
            frmProductList frm = new frmProductList();
            LoadSubForm(frm);
        }

        private void ManageCategoryButton_Click(object sender, EventArgs e)
        {
            frmCateogoryList frm = new frmCateogoryList();
            frm.LoadCategory();
            LoadSubForm(frm);
        }

        private void SystemSettingButton_Click(object sender, EventArgs e)
        {
            frmStore frm = new frmStore();
            ThemeManager.ApplyTheme(frm);
            LanguageManager.ApplyLanguage(frm);
            frm.LoadRecords();
            frm.ShowDialog();           
        }

        private void panel3_Paint(object sender, PaintEventArgs e)
        {

        }

        private void Logoutbtn_Click(object sender, EventArgs e)
        {
            this.Dispose();
            frmSecurity frm = new frmSecurity();
            ThemeManager.ApplyTheme(frm);
            frm.Show();
        }

        private void btnSaleHistory_Click(object sender, EventArgs e)
        {
            frmSoldItems frm = new frmSoldItems();
            ThemeManager.ApplyTheme(frm);
            LanguageManager.ApplyLanguage(frm);
            frm.ShowDialog();
        }

        private void panel1_Paint(object sender, PaintEventArgs e)
        {

        }

        private void DashboardButton_Click(object sender, EventArgs e)
        {
            MyDashboard();
        }

        public void MyDashboard()
        {
            frmDashboard frm = new frmDashboard();
            frm.lblDailySales.Text = dbcon.DailySales().ToString("#,##0") + " FCFA";
            frm.lblProductLine.Text = dbcon.ProductLine().ToString("#,##0");
            frm.lblStockOnHand.Text = dbcon.StockOnHand().ToString("#,##0");
            frm.lblCriticalItems.Text = dbcon.Critical().ToString("#,##0");
            LoadSubForm(frm);
        }

        private void btnVendor_Click(object sender, EventArgs e)
        {
            frmVendorList frm = new frmVendorList();
            frm.LoadRecords();
            LoadSubForm(frm);
        }

        private void btnStockAdjustment_Click(object sender, EventArgs e)
        {
            frmAdjustment frm = new frmAdjustment(this);
            ThemeManager.ApplyTheme(frm);
            LanguageManager.ApplyLanguage(frm);
            frm.LoadRecords();
            frm.txtUser.Text = lblName.Text;
            frm.RefrenceNo();
            frm.ShowDialog();
        }

        private async void RunBackgroundSync(System.Windows.Forms.Label lblSyncStatus)
        {
            if (_isSyncing) return;
            _isSyncing = true;

            try
            {
                bool isEnglish = LanguageManager.CurrentLanguage == "English";
                lblSyncStatus.Text = isEnglish ? "Auto-syncing..." : "Synchro auto en cours...";
                lblSyncStatus.ForeColor = Color.LightBlue;

                var syncService = new FinalPOS.Data.SyncService(dbcon.MyConnection());
                await Task.Run(() => {
                    syncService.Synchronize(msg => {
                        System.Diagnostics.Debug.WriteLine("AutoSync: " + msg);
                    });
                });

                UpdateSyncStatusLabel(lblSyncStatus);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Background sync failed: " + ex.Message);
                bool isEnglish = LanguageManager.CurrentLanguage == "English";
                lblSyncStatus.Text = isEnglish ? "Sync Error (Offline)" : "Erreur synchro (Hors-ligne)";
                lblSyncStatus.ForeColor = Color.OrangeRed;
            }
            finally
            {
                _isSyncing = false;
            }
        }

        private void AddSyncControls()
        {
            // Compact existing buttons in panel4 to free up space at the bottom
            var buttons = panel4.Controls.Cast<Control>()
                .Where(c => c is Button && c.Name != "btnSync")
                .OrderBy(c => c.Location.Y)
                .ToList();

            int currentY = 10;
            foreach (var btn in buttons)
            {
                btn.Location = new Point(btn.Location.X, currentY);
                currentY += 33; // 33px height per button (0 gap)
            }

            // Create Sync Button inside panel4
            System.Windows.Forms.Button btnSync = new System.Windows.Forms.Button();
            btnSync.Name = "btnSync";
            btnSync.Text = LanguageManager.CurrentLanguage == "English" ? "  Synchronize" : "  Synchroniser";
            btnSync.Size = new Size(260, 24);
            btnSync.Location = new Point(12, 409);
            btnSync.FlatStyle = FlatStyle.Flat;
            btnSync.FlatAppearance.BorderSize = 0;
            btnSync.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            btnSync.ForeColor = Color.White;
            btnSync.BackColor = Color.FromArgb(59, 130, 246); // Electric Blue
            btnSync.TextAlign = ContentAlignment.MiddleCenter;
            
            // Mouse over color
            btnSync.FlatAppearance.MouseOverBackColor = Color.FromArgb(37, 99, 235);

            // Create Sync Status Label inside panel4
            System.Windows.Forms.Label lblSyncStatus = new System.Windows.Forms.Label();
            lblSyncStatus.Name = "lblSyncStatus";
            lblSyncStatus.Text = LanguageManager.CurrentLanguage == "English" ? "Last Sync: Never" : "Dernière synchro : Jamais";
            lblSyncStatus.Size = new Size(260, 15);
            lblSyncStatus.Location = new Point(12, 434);
            lblSyncStatus.ForeColor = Color.DarkGray;
            lblSyncStatus.Font = new Font("Segoe UI", 7.5F, FontStyle.Regular);
            lblSyncStatus.TextAlign = ContentAlignment.TopCenter;

            // Add to panel4
            panel4.Controls.Add(btnSync);
            panel4.Controls.Add(lblSyncStatus);

            // Fetch last sync date from Local Settings and update label
            UpdateSyncStatusLabel(lblSyncStatus);

            btnSync.Click += async (s, ev) => {
                if (_isSyncing) return;
                _isSyncing = true;

                bool isEnglish = LanguageManager.CurrentLanguage == "English";
                btnSync.Enabled = false;
                btnSync.Text = isEnglish ? "  Syncing..." : "  Synchronisation...";
                btnSync.BackColor = Color.FromArgb(107, 114, 128); // Gray-500
                lblSyncStatus.Text = isEnglish ? "Sync in progress..." : "Synchronisation en cours...";
                lblSyncStatus.ForeColor = Color.LightBlue;

                try
                {
                    var syncService = new FinalPOS.Data.SyncService(dbcon.MyConnection());
                    
                    // Run async in a task
                    await Task.Run(() => {
                        syncService.Synchronize(msg => {
                            System.Diagnostics.Debug.WriteLine("Sync: " + msg);
                        });
                    });

                    // Success
                    UpdateSyncStatusLabel(lblSyncStatus);
                    string successMsg = isEnglish ? "Synchronization succeeded with central database!" : "Synchronisation réussie avec la base centrale !";
                    string successTitle = isEnglish ? "Sync Success" : "Succès Sync";
                    MessageBox.Show(successMsg, successTitle, MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    lblSyncStatus.Text = isEnglish ? "Sync Error" : "Erreur de synchronisation";
                    lblSyncStatus.ForeColor = Color.Red;
                    string errMsg = isEnglish ? "Error during sync: " : "Erreur lors de la synchronisation : ";
                    string errTitle = isEnglish ? "Sync Error" : "Erreur Sync";
                    MessageBox.Show(errMsg + ex.Message, errTitle, MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                finally
                {
                    _isSyncing = false;
                    btnSync.Enabled = true;
                    btnSync.Text = isEnglish ? "  Synchronize" : "  Synchroniser";
                    btnSync.BackColor = Color.FromArgb(59, 130, 246);
                }
            };

            // Setup Auto-Sync Timer
            _autoSyncTimer = new System.Windows.Forms.Timer();
            _autoSyncTimer.Interval = 180000; // 3 minutes
            _autoSyncTimer.Tick += (s, ev) => RunBackgroundSync(lblSyncStatus);
            _autoSyncTimer.Start();

            // Hook FormClosing to clean up timer
            this.FormClosing += (s, ev) => {
                if (_autoSyncTimer != null)
                {
                    _autoSyncTimer.Stop();
                    _autoSyncTimer.Dispose();
                }
            };

            // Run background sync once on startup after 5 seconds to sync any offline transactions
            Task.Delay(5000).ContinueWith(t => {
                try {
                    this.BeginInvoke(new Action(() => RunBackgroundSync(lblSyncStatus)));
                } catch { }
            });
        }

        private void UpdateSyncStatusLabel(System.Windows.Forms.Label label)
        {
            try
            {
                using (var cnSetting = new SqlConnection(dbcon.MyConnection()))
                {
                    cnSetting.Open();
                    using (var cmSetting = new SqlCommand("select setting_value from tbl_LocalSettings where setting_key = 'LastSyncDateTime'", cnSetting))
                    {
                        var res = cmSetting.ExecuteScalar();
                        bool isEnglish = LanguageManager.CurrentLanguage == "English";
                        if (res != null && res != DBNull.Value && !string.IsNullOrWhiteSpace(res.ToString()))
                        {
                            label.Text = (isEnglish ? "Last Sync: " : "Dernière synchro : ") + res.ToString();
                            label.ForeColor = Color.GreenYellow;
                        }
                        else
                        {
                            label.Text = isEnglish ? "Last Sync: Never" : "Dernière synchro : Jamais";
                            label.ForeColor = Color.DarkGray;
                        }
                    }
                }
            }
            catch
            {
                label.Text = LanguageManager.CurrentLanguage == "English" ? "Last Sync: Unknown" : "Dernière synchro : Inconnue";
                label.ForeColor = Color.Red;
            }
        }
    }
}