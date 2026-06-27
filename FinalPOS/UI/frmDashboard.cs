using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Data.SqlClient;
using System.Windows.Forms.DataVisualization.Charting;

namespace FinalPOS
{
    public partial class frmDashboard : Form
    {
        SqlConnection cn;
        DBConnection db = new DBConnection();

        public frmDashboard()
        {
            InitializeComponent();
            this.Load += (s, e) => {
                FinalPOS.UI.ThemeManager.ApplyTheme(this);
                FinalPOS.UI.LanguageManager.ApplyLanguage(this);
            };
            cn = new SqlConnection();
            cn.ConnectionString = db.MyConnection();

            // Set transparent backgrounds for dashboard summary elements and hide separators
            foreach (Panel card in new Panel[] { panel2, panel3, panel4, panel5 })
            {
                card.Paint += Panel_Paint;
                foreach (Control ctrl in card.Controls)
                {
                    ctrl.BackColor = Color.Transparent;
                    if (ctrl is GroupBox gb)
                    {
                        gb.BackColor = Color.Transparent;
                        gb.Visible = false; // Hide legacy line separator
                    }
                }
            }

            LoadChart();
        }

        private void frmDashboard_Resize(object sender, EventArgs e)
        {
            panel1.Left = (this.Width - panel1.Width) / 2;
        }

        private void panel2_Paint(object sender, PaintEventArgs e)
        {
        }

        private void Panel_Paint(object sender, PaintEventArgs e)
        {
            Panel p = (Panel)sender;
            Color color1 = Color.FromArgb(59, 130, 246);
            Color color2 = Color.FromArgb(147, 51, 234);

            if (p.Name == "panel2") // Daily Sales
            {
                color1 = Color.FromArgb(244, 63, 94); // Rose
                color2 = Color.FromArgb(124, 58, 237); // Violet
            }
            else if (p.Name == "panel4") // Product Line
            {
                color1 = Color.FromArgb(37, 99, 235); // Blue
                color2 = Color.FromArgb(6, 182, 212); // Cyan
            }
            else if (p.Name == "panel3") // Stock on Hand
            {
                color1 = Color.FromArgb(245, 158, 11); // Amber
                color2 = Color.FromArgb(239, 68, 68); // Red-Orange
            }
            else if (p.Name == "panel5") // Critical Items
            {
                color1 = Color.FromArgb(13, 148, 136); // Teal
                color2 = Color.FromArgb(16, 185, 129); // Emerald
            }

            using (var brush = new LinearGradientBrush(p.ClientRectangle, color1, color2, 45F))
            {
                e.Graphics.FillRectangle(brush, p.ClientRectangle);
            }

            // Draw a subtle border
            using (var pen = new Pen(Color.FromArgb(50, 255, 255, 255), 1.5f))
            {
                e.Graphics.DrawRectangle(pen, 0, 0, p.Width - 1, p.Height - 1);
            }
        }

        public void LoadChart()
        {
            try
            {
                cn.Open();
                // Query sales by month of the current year (fallback to all time by month if needed)
                string query = "SELECT DateName(month, sdate) as Month, isnull(sum(total),0.0) as Total FROM tbl_Cart WHERE status = 'Sold' GROUP BY Month(sdate), DateName(month, sdate) ORDER BY Month(sdate)";
                SqlDataAdapter da = new SqlDataAdapter(query, cn);
                DataSet ds = new DataSet();

                da.Fill(ds, "Sales");
                chart1.DataSource = ds.Tables["Sales"];
                Series series1 = chart1.Series["Series1"];
                series1.ChartType = SeriesChartType.SplineArea;
                series1.Name = "SALES";

                var chart = chart1;
                chart.Series[series1.Name].XValueMember = "Month";
                chart.Series[series1.Name].YValueMembers = "Total";
                chart.Series[0].IsValueShownAsLabel = false; // Hide ugly text labels on every point

                // Modernizing chart looks
                series1.Color = Color.FromArgb(40, 37, 99, 235); // 40/255 opacity
                series1.BorderColor = Color.FromArgb(37, 99, 235);
                series1.BorderWidth = 3;
                series1.MarkerStyle = MarkerStyle.Circle;
                series1.MarkerSize = 6;
                series1.MarkerColor = Color.White;
                series1.MarkerBorderColor = Color.FromArgb(37, 99, 235);
                series1.MarkerBorderWidth = 2;
                series1.ShadowOffset = 1;

                cn.Close();
            }
            catch (Exception)
            {
                if (cn.State == ConnectionState.Open) cn.Close();
            }
        }
    }
}
