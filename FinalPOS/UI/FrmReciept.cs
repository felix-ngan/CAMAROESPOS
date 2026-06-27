using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Data.SqlClient;
using Microsoft.Reporting.WinForms;
using System.IO;
using System.Drawing.Imaging;
using QRCoder;

namespace FinalPOS
{
    public partial class FrmReciept : Form
    {
        SqlConnection cn = new SqlConnection();
        DBConnection dbcon = new DBConnection();
        frmPOS f;
        string store = "";
        string address = "";
        private List<string> _tempFiles = new List<string>();

        private Panel pnlHeader;
        private Label lblLang;
        private ComboBox cboLang;
        private string _pcash = "0";
        private string _pchange = "0";
        private string logoUri = "";
        private string qrUri = "";

        public FrmReciept(frmPOS frm)
        {
            InitializeComponent();
            this.Load += (s, e) => {
                FinalPOS.UI.ThemeManager.ApplyTheme(this);
                FinalPOS.UI.LanguageManager.ApplyLanguage(this);
            };
            cn = new SqlConnection(dbcon.MyConnection());
            f = frm;
            this.KeyPreview = true;
            this.FormClosed += FrmReciept_FormClosed;
            InitializeLanguageSelector();
        }

        private void InitializeLanguageSelector()
        {
            this.Size = new Size(480, 700);
            this.StartPosition = FormStartPosition.CenterScreen;

            pnlHeader = new Panel();
            pnlHeader.Height = 45;
            pnlHeader.Dock = DockStyle.Top;
            pnlHeader.BackColor = Color.FromArgb(41, 128, 185);

            lblLang = new Label();
            lblLang.Text = "Langue / Language :";
            lblLang.ForeColor = Color.White;
            lblLang.Font = new Font("Segoe UI", 10, FontStyle.Bold);
            lblLang.Location = new Point(10, 12);
            lblLang.AutoSize = true;

            cboLang = new ComboBox();
            cboLang.DropDownStyle = ComboBoxStyle.DropDownList;
            cboLang.Items.AddRange(new object[] { "Français", "English" });
            cboLang.SelectedIndex = FinalPOS.UI.LanguageManager.CurrentLanguage == "English" ? 1 : 0;
            cboLang.Location = new Point(150, 10);
            cboLang.Width = 120;
            cboLang.Font = new Font("Segoe UI", 10);

            cboLang.SelectedIndexChanged += (s, e) => {
                UpdateParametersForLanguage();
            };

            pnlHeader.Controls.Add(lblLang);
            pnlHeader.Controls.Add(cboLang);
            this.Controls.Add(pnlHeader);

            // Ensure reportViewer1 is filling the rest
            this.reportViewer1.Dock = DockStyle.Fill;
            pnlHeader.BringToFront();
        }

        private void UpdateParametersForLanguage()
        {
            if (string.IsNullOrEmpty(logoUri) || string.IsNullOrEmpty(qrUri))
            {
                // Report is not fully loaded yet
                return;
            }

            string lang = cboLang.SelectedItem.ToString();
            
            string labelVatable, labelVat, labelDiscount, labelTotal, labelCash, labelChange, labelThankYou, labelTransaction;
            
            if (lang == "English")
            {
                labelVatable = "Subtotal";
                labelVat = "VAT";
                labelDiscount = "Discount";
                labelTotal = "Net Total";
                labelCash = "Amount Received";
                labelChange = "Change";
                labelThankYou = "THANK YOU FOR YOUR VISIT";
                labelTransaction = "Invoice No: " + f.lblTransno.Text;
            }
            else
            {
                labelVatable = "Hors-taxe";
                labelVat = "TVA";
                labelDiscount = "Remise";
                labelTotal = "Total Net";
                labelCash = "Montant Reçu";
                labelChange = "Monnaie Rendue";
                labelThankYou = "MERCI POUR VOTRE VISITE";
                labelTransaction = "Facture N°: " + f.lblTransno.Text;
            }

            ReportParameter pVatable = new ReportParameter("pVatable", f.lblVatable.Text);
            ReportParameter pVat = new ReportParameter("pVat", f.lblVAT.Text);
            ReportParameter pDiscount = new ReportParameter("pDiscount", f.lblDiscount.Text);
            ReportParameter pTotal = new ReportParameter("pTotal", f.lblDisplayTotal.Text);
            ReportParameter pCash = new ReportParameter("pCash", _pcash);
            ReportParameter pChange = new ReportParameter("pChange", _pchange);
            ReportParameter pStore = new ReportParameter("pStore", store);
            ReportParameter pAddress = new ReportParameter("pAddress", address);
            ReportParameter pTransaction = new ReportParameter("pTransaction", labelTransaction);
            ReportParameter pCashier = new ReportParameter("pCashier", f.lblUser.Text);
            ReportParameter pLogoPath = new ReportParameter("pLogoPath", logoUri);
            ReportParameter pQrPath = new ReportParameter("pQrPath", qrUri);

            ReportParameter pLableVatable = new ReportParameter("pLabelVatable", labelVatable);
            ReportParameter pLableVat = new ReportParameter("pLabelVat", labelVat);
            ReportParameter pLableDiscount = new ReportParameter("pLabelDiscount", labelDiscount);
            ReportParameter pLableTotal = new ReportParameter("pLabelTotal", labelTotal);
            ReportParameter pLableCash = new ReportParameter("pLabelCash", labelCash);
            ReportParameter pLableChange = new ReportParameter("pLabelChange", labelChange);
            ReportParameter pLableThankYou = new ReportParameter("pLabelThankYou", labelThankYou);

            reportViewer1.LocalReport.SetParameters(new ReportParameter[] {
                pVatable, pVat, pDiscount, pTotal, pCash, pChange, pStore, pAddress, pTransaction, pCashier, pLogoPath, pQrPath,
                pLableVatable, pLableVat, pLableDiscount, pLableTotal, pLableCash, pLableChange, pLableThankYou
            });

            reportViewer1.RefreshReport();
        }

        private string GetTransparentImage()
        {
            string path = Path.Combine(Path.GetTempPath(), "empty_temp_" + Guid.NewGuid().ToString() + ".png");
            try
            {
                using (var bmp = new Bitmap(1, 1))
                {
                    bmp.SetPixel(0, 0, Color.Transparent);
                    bmp.Save(path, ImageFormat.Png);
                }
                _tempFiles.Add(path);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Error generating transparent img: " + ex.Message);
            }
            return path;
        }

        public void LoadReport(string pcash, string pchange)
        {
            try
            {
                this.reportViewer1.LocalReport.DataSources.Clear();
                
                this.reportViewer1.LocalReport.ReportPath = Application.StartupPath + @"\Reports\Report1.rdlc";
                ReportDataSource rptDatasource;

                DataSet1 ds = new DataSet1();
                SqlDataAdapter da = new SqlDataAdapter();
              
                cn.Open();
                
                // Garanti que les colonnes existent avant de les requêter
                string checkQuery = "IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('tbl_Store') AND name = 'logo') " +
                                    "ALTER TABLE tbl_Store ADD logo varbinary(max) NULL; " +
                                    "IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('tbl_Store') AND name = 'momo_code') " +
                                    "ALTER TABLE tbl_Store ADD momo_code varchar(50) NULL; " +
                                    "IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('tbl_Store') AND name = 'orange_code') " +
                                    "ALTER TABLE tbl_Store ADD orange_code varchar(50) NULL;";
                using (var cmMigrate = new SqlCommand(checkQuery, cn))
                {
                    cmMigrate.ExecuteNonQuery();
                }

                string momoCode = "";
                string orangeCode = "";
                byte[] logoBytes = null;
                // Charger dynamiquement les infos de la boutique
                using (var cmStore = new SqlCommand("select store, address, logo, momo_code, orange_code from tbl_Store", cn))
                {
                    using (var drStore = cmStore.ExecuteReader())
                    {
                        if (drStore.Read())
                        {
                            store = drStore["store"].ToString();
                            address = drStore["address"].ToString();
                            if (drStore["logo"] != DBNull.Value)
                            {
                                logoBytes = (byte[])drStore["logo"];
                            }
                            if (drStore["momo_code"] != DBNull.Value)
                            {
                                momoCode = drStore["momo_code"].ToString();
                            }
                            if (drStore["orange_code"] != DBNull.Value)
                            {
                                orangeCode = drStore["orange_code"].ToString();
                            }
                        }
                    }
                }

                da.SelectCommand = new SqlCommand("select c.id, c.transno, c.pcode , c.price, c.qty, c.disc, c.total, c.sdate, status, p.pdesc from tbl_Cart as c inner join tbl_Products as p on c.pcode = p.pcode where transno like '"+ f.lblTransno.Text + "' ", cn);
                da.Fill(ds.Tables["dtSold"]);
                cn.Close();

                // Enable external images for custom Logo and QR Code
                reportViewer1.LocalReport.EnableExternalImages = true;

                // Setup Logo URL
                if (logoBytes != null && logoBytes.Length > 0)
                {
                    try
                    {
                        string logoTempPath = Path.Combine(Path.GetTempPath(), "logo_temp_" + Guid.NewGuid().ToString() + ".png");
                        File.WriteAllBytes(logoTempPath, logoBytes);
                        _tempFiles.Add(logoTempPath);
                        logoUri = new Uri(logoTempPath).AbsoluteUri;
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine("Error saving logo: " + ex.Message);
                        logoUri = new Uri(GetTransparentImage()).AbsoluteUri;
                    }
                }
                else
                {
                    logoUri = new Uri(GetTransparentImage()).AbsoluteUri;
                }

                _pcash = pcash;
                _pchange = pchange;

                // Setup QR Code URL
                try
                {
                    // Clean total amount to get numeric total
                    string rawTotal = f.lblDisplayTotal.Text;
                    string numericTotal = "";
                    foreach (char c in rawTotal)
                    {
                        if (char.IsDigit(c))
                        {
                            numericTotal += c;
                        }
                    }

                    string qrText = $"FACTURE: {f.lblTransno.Text} | TOTAL: {numericTotal} FCFA";

                    if (!string.IsNullOrEmpty(momoCode))
                    {
                        qrText += $"\nMoMo: *126*4*{momoCode}*{numericTotal}#";
                    }
                    if (!string.IsNullOrEmpty(orangeCode))
                    {
                        qrText += $"\nOM: *150*47*{orangeCode}*{numericTotal}#";
                    }

                    string qrTempPath = Path.Combine(Path.GetTempPath(), "qr_temp_" + Guid.NewGuid().ToString() + ".png");
                    using (var qrGenerator = new QRCodeGenerator())
                    using (var qrCodeData = qrGenerator.CreateQrCode(qrText, QRCodeGenerator.ECCLevel.Q))
                    using (var qrCode = new QRCode(qrCodeData))
                    using (var qrCodeImage = qrCode.GetGraphic(30))
                    {
                        qrCodeImage.Save(qrTempPath, ImageFormat.Png);
                    }
                    _tempFiles.Add(qrTempPath);
                    qrUri = new Uri(qrTempPath).AbsoluteUri;
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine("Error generating QR: " + ex.Message);
                    qrUri = new Uri(GetTransparentImage()).AbsoluteUri;
                }

                rptDatasource = new ReportDataSource("DataSet1", ds.Tables["dtSold"]);
                reportViewer1.LocalReport.DataSources.Add(rptDatasource);
                
                UpdateParametersForLanguage();

                reportViewer1.SetDisplayMode(Microsoft.Reporting.WinForms.DisplayMode.PrintLayout);
                reportViewer1.ZoomMode = ZoomMode.PageWidth;
            }
            catch (Exception ex)
            {
                cn.Close();
                MessageBox.Show(ex.Message);
            }
        }

        private void dtSoldBindingSource_CurrentChanged(object sender, EventArgs e)
        {
        }

        private void FrmReciept_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
            {
                this.Dispose();
            }
        }

        private void FrmReciept_Load(object sender, EventArgs e)
        {
            this.reportViewer1.RefreshReport();
        }

        private void FrmReciept_FormClosed(object sender, FormClosedEventArgs e)
        {
            foreach (var path in _tempFiles)
            {
                try
                {
                    if (File.Exists(path))
                    {
                        File.Delete(path);
                    }
                }
                catch
                {
                    // Ignore clean-up failures
                }
            }
        }
    }
}
