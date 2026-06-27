using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.SqlClient;
using System.Windows.Forms;
using Microsoft.Reporting.WinForms;

namespace FinalPOS
{
    public partial class frmSoldIReport : Form
    {
        SqlConnection cn = new SqlConnection();
        SqlCommand cm = new SqlCommand();
        DBConnection dbcon = new DBConnection();
        frmSoldItems f;
        public frmSoldIReport(frmSoldItems frm)
        {
            InitializeComponent();
            this.Load += (s, e) => {
                FinalPOS.UI.ThemeManager.ApplyTheme(this);
                FinalPOS.UI.LanguageManager.ApplyLanguage(this);
            };
            cn = new SqlConnection(dbcon.MyConnection());
            f = frm;
        }

        private void frmSoldIReport_Load(object sender, EventArgs e)
        {


            this.reportViewer2.RefreshReport();
        }

        

        public void LoadReport()
        {
            try
            {
                ReportDataSource rptDS;
                this.reportViewer2.LocalReport.ReportPath = Application.StartupPath + @"\Reports\Report2.rdlc";
                this.reportViewer2.LocalReport.DataSources.Clear();

                DataSet1 ds = new DataSet1();
                SqlDataAdapter da = new SqlDataAdapter();

                cn.Open();
                SqlCommand cmd;
                if (f.cboCashier.Text == "Tous les Caissiers")
                {
                    cmd = new SqlCommand("select c.id, c.transno, c.pcode , p.pdesc, c.price, c.qty, c.disc as discount , c.total from tbl_Cart as c inner join tbl_Products as p on c.pcode = p.pcode where status like 'Sold' and sdate between @dt1 and @dt2 ", cn);
                }
                else
                {
                    cmd = new SqlCommand("select c.id, c.transno, c.pcode , p.pdesc, c.price, c.qty, c.disc as discount , c.total from tbl_Cart as c inner join tbl_Products as p on c.pcode = p.pcode where status like 'Sold' and sdate between @dt1 and @dt2 and cashier like @cashier ", cn);
                    cmd.Parameters.AddWithValue("@cashier", f.cboCashier.Text);
                }
                cmd.Parameters.AddWithValue("@dt1", f.dt1.Value.Date);
                cmd.Parameters.AddWithValue("@dt2", f.dt2.Value.Date.AddDays(1).AddTicks(-1));

                da.SelectCommand = cmd;
                da.Fill(ds.Tables["dtSoldReport"]);
                cn.Close();

                ReportParameter pDate = new ReportParameter("pDate", "Période du : " + f.dt1.Value.ToShortDateString() + " Au " + f.dt2.Value.ToShortDateString());
                ReportParameter pCashier = new ReportParameter("pCashier", "Caissier: " + f.cboCashier.Text);
                ReportParameter pHeader = new ReportParameter("pHeader","RAPPORT DES VENTES");

                reportViewer2.LocalReport.SetParameters(pDate);
                reportViewer2.LocalReport.SetParameters(pCashier);
                reportViewer2.LocalReport.SetParameters(pHeader);

                rptDS = new ReportDataSource("DataSet1", ds.Tables["dtSoldReport"]);
                reportViewer2.LocalReport.DataSources.Add(rptDS);
                reportViewer2.SetDisplayMode(Microsoft.Reporting.WinForms.DisplayMode.PrintLayout);
                reportViewer2.ZoomMode = ZoomMode.Percent;
                reportViewer2.ZoomPercent = 50;
            }
            catch (Exception ex)
            {
                cn.Close();
                MessageBox.Show(ex.Message);
            }
        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {
            this.Dispose();
        }
    }
}
