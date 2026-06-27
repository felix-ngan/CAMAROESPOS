using System;
using System.Drawing;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;
using MetroFramework;
using MetroFramework.Controls;
using MetroFramework.Interfaces;

namespace FinalPOS.UI
{
    public enum ThemeMode
    {
        Light,
        Dark
    }

    public static class ThemeManager
    {
        public static ThemeMode CurrentTheme { get; set; } = ThemeMode.Light;

        // Custom Colors for Standard Controls - Modern Slate Palette
        public static Color DarkBg = Color.FromArgb(15, 23, 42);       // Slate-900
        public static Color DarkPanelBg = Color.FromArgb(30, 41, 59);  // Slate-800
        public static Color DarkText = Color.FromArgb(241, 245, 249);  // Slate-100
        public static Color DarkAccent = Color.FromArgb(59, 130, 246); // Blue-500
        public static Color DarkControlBg = Color.FromArgb(51, 65, 85);// Slate-700

        public static Color LightBg = Color.FromArgb(248, 250, 252);   // Slate-50
        public static Color LightPanelBg = Color.White;
        public static Color LightText = Color.FromArgb(15, 23, 42);    // Slate-900
        public static Color LightAccent = Color.FromArgb(37, 99, 235);  // Blue-600
        public static Color LightControlBg = Color.FromArgb(241, 245, 249); // Slate-100

        public static void ApplyTheme(Form form)
        {
            var metroTheme = CurrentTheme == ThemeMode.Dark ? MetroThemeStyle.Dark : MetroThemeStyle.Light;
            var bgColor = CurrentTheme == ThemeMode.Dark ? DarkBg : LightBg;
            var fgColor = CurrentTheme == ThemeMode.Dark ? DarkText : LightText;

            if (form is IMetroForm metroForm)
            {
                metroForm.Theme = metroTheme;
            }
            else
            {
                form.BackColor = bgColor;
                form.ForeColor = fgColor;
            }

            // Force Segoe UI font on Form
            form.Font = new Font("Segoe UI", 9.75F, FontStyle.Regular, GraphicsUnit.Point, 0);

            ApplyThemeToControls(form.Controls, metroTheme);
            form.Refresh();
        }

        private static void ApplyThemeToControls(Control.ControlCollection controls, MetroThemeStyle metroTheme)
        {
            var isDark = metroTheme == MetroThemeStyle.Dark;
            var bgColor = isDark ? DarkBg : LightBg;
            var panelColor = isDark ? DarkPanelBg : LightPanelBg;
            var fgColor = isDark ? DarkText : LightText;
            var controlBg = isDark ? DarkControlBg : LightControlBg;

            foreach (Control ctrl in controls)
            {
                // Enforce Segoe UI font style
                if (!(ctrl is IMetroControl))
                {
                    var existingStyle = ctrl.Font != null ? ctrl.Font.Style : FontStyle.Regular;
                    var existingSize = ctrl.Font != null ? ctrl.Font.Size : 9.75F;
                    ctrl.Font = new Font("Segoe UI", existingSize, existingStyle);
                }

                if (ctrl is MetroTextBox metroTxt)
                {
                    metroTxt.Theme = metroTheme;
                    metroTxt.UseCustomBackColor = true;
                    metroTxt.UseCustomForeColor = true;
                    metroTxt.BackColor = controlBg;
                    metroTxt.ForeColor = fgColor;
                    metroTxt.Font = new Font("Segoe UI", 10.5F, FontStyle.Regular);
                    metroTxt.WaterMarkFont = new Font("Segoe UI", 10.5F, FontStyle.Italic);
                }
                else if (ctrl is IMetroControl metroCtrl)
                {
                    metroCtrl.Theme = metroTheme;
                }
                else
                {
                    // Regular Windows Forms Control
                    if (ctrl is Panel panel)
                    {
                        if (panel.Name.Contains("panel1") || panel.Name.Contains("panel2") || panel.Name.Contains("panel4") || panel.Name.Contains("leftPanel") || panel.Name.Contains("Menu"))
                        {
                            panel.BackColor = isDark ? Color.FromArgb(21, 21, 21) : Color.FromArgb(15, 23, 42);
                            panel.ForeColor = Color.White;
                        }
                        else if (panel.Parent != null && panel.Parent.Name == "panel1" && panel.Parent.Parent is frmDashboard)
                        {
                            // Dashboard summary cards: keep original colored backgrounds, but force white text
                            panel.ForeColor = Color.White;
                        }
                        else
                        {
                            panel.BackColor = panelColor;
                            panel.ForeColor = fgColor;
                        }
                    }
                    else if (ctrl is DataGridView dgv)
                    {
                        dgv.BackgroundColor = panelColor;
                        dgv.ForeColor = fgColor;
                        dgv.GridColor = isDark ? Color.FromArgb(51, 65, 85) : Color.FromArgb(226, 232, 240);
                        dgv.DefaultCellStyle.BackColor = panelColor;
                        dgv.DefaultCellStyle.ForeColor = fgColor;
                        
                        // Modern Selection Colors
                        dgv.DefaultCellStyle.SelectionBackColor = isDark ? Color.FromArgb(37, 99, 235) : Color.FromArgb(219, 234, 254);
                        dgv.DefaultCellStyle.SelectionForeColor = isDark ? Color.White : Color.FromArgb(15, 23, 42);
                        
                        // Header Styling (Segoe UI Bold, Slate headers, taller heights)
                        dgv.ColumnHeadersDefaultCellStyle.BackColor = isDark ? Color.FromArgb(51, 65, 85) : Color.FromArgb(241, 245, 249);
                        dgv.ColumnHeadersDefaultCellStyle.ForeColor = fgColor;
                        dgv.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 9.5F, FontStyle.Bold);
                        dgv.ColumnHeadersHeight = 35;
                        dgv.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
                        dgv.EnableHeadersVisualStyles = false;

                        // Modern Row formatting (Height = 30-35px, horizontal borders only)
                        dgv.RowTemplate.Height = 32;
                        foreach (DataGridViewRow row in dgv.Rows)
                        {
                            row.Height = 32;
                        }
                        dgv.CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal;
                        dgv.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
                    }
                    else if (ctrl is Label lbl)
                    {
                        if (lbl.Parent != null && (lbl.Parent.Name.Contains("panel1") || lbl.Parent.Name.Contains("panel2") || lbl.Parent.Name.Contains("panel4") || lbl.Parent.Name.Contains("leftPanel") || lbl.Parent.Name.Contains("Menu")))
                        {
                            lbl.ForeColor = Color.White;
                        }
                        else
                        {
                            lbl.ForeColor = fgColor;
                        }
                    }
                    else if (ctrl is Button btn)
                    {
                        btn.FlatStyle = FlatStyle.Flat;
                        
                        // Check if it is a dashboard/sidebar menu button to preserve custom styles
                        if (btn.Parent != null && (btn.Parent.Name.Contains("panel1") || btn.Parent.Name.Contains("panel2") || btn.Parent.Name.Contains("panel4") || btn.Parent.Name.Contains("leftPanel") || btn.Parent.Name.Contains("Menu")))
                        {
                            // Keep sidebar styling (dark / blue background)
                            btn.FlatAppearance.BorderSize = 0;
                            btn.BackColor = Color.Transparent;
                            btn.ForeColor = Color.White;
                            btn.FlatAppearance.MouseOverBackColor = isDark ? Color.FromArgb(51, 65, 85) : Color.FromArgb(30, 41, 59);
                        }
                        else
                        {
                            btn.FlatAppearance.BorderSize = 0;
                            string txtUpper = btn.Text?.ToUpper() ?? "";
                            
                            // Contextual coloring for primary/secondary actions
                            if (txtUpper.Contains("ENREGISTRER") || txtUpper.Contains("SAVE") || txtUpper.Contains("PAYER") || txtUpper.Contains("PAY"))
                            {
                                btn.BackColor = Color.FromArgb(16, 185, 129); // Emerald Green
                            }
                            else if (txtUpper.Contains("ANNULER") || txtUpper.Contains("CANCEL") || txtUpper.Contains("EFFACER") || txtUpper.Contains("CLEAR") || txtUpper.Contains("SUPPRIMER") || txtUpper.Contains("DELETE"))
                            {
                                btn.BackColor = Color.FromArgb(239, 68, 68); // Red
                            }
                            else if (txtUpper.Contains("METTRE") || txtUpper.Contains("UPDATE") || txtUpper.Contains("MODIFIER") || txtUpper.Contains("EDIT"))
                            {
                                btn.BackColor = Color.FromArgb(59, 130, 246); // Electric Blue
                            }
                            else
                            {
                                btn.BackColor = isDark ? Color.FromArgb(51, 65, 85) : Color.FromArgb(37, 99, 235);
                            }
                            
                            btn.ForeColor = Color.White;
                            
                            // Hover effect calculation
                            btn.FlatAppearance.MouseOverBackColor = Color.FromArgb(
                                Math.Max(0, Math.Min(255, btn.BackColor.R - 20)),
                                Math.Max(0, Math.Min(255, btn.BackColor.G - 20)),
                                Math.Max(0, Math.Min(255, btn.BackColor.B - 20))
                            );
                        }
                    }
                    else if (ctrl is TextBox txt)
                    {
                        txt.BackColor = controlBg;
                        txt.ForeColor = fgColor;
                        txt.BorderStyle = BorderStyle.FixedSingle;
                    }
                    else if (ctrl is ComboBox cbo)
                    {
                        cbo.BackColor = controlBg;
                        cbo.ForeColor = fgColor;
                    }
                    else if (ctrl is Chart chart)
                    {
                        chart.BackColor = panelColor;
                        chart.BorderlineColor = panelColor;
                        if (chart.ChartAreas.Count > 0)
                        {
                            chart.ChartAreas[0].BackColor = panelColor;
                            chart.ChartAreas[0].AxisX.LineColor = fgColor;
                            chart.ChartAreas[0].AxisY.LineColor = fgColor;
                            chart.ChartAreas[0].AxisX.LabelStyle.ForeColor = fgColor;
                            chart.ChartAreas[0].AxisY.LabelStyle.ForeColor = fgColor;
                            chart.ChartAreas[0].AxisX.MajorGrid.LineColor = isDark ? Color.FromArgb(51, 65, 85) : Color.FromArgb(241, 245, 249);
                            chart.ChartAreas[0].AxisY.MajorGrid.LineColor = isDark ? Color.FromArgb(51, 65, 85) : Color.FromArgb(241, 245, 249);
                        }
                        if (chart.Legends.Count > 0)
                        {
                            chart.Legends[0].BackColor = panelColor;
                            chart.Legends[0].ForeColor = fgColor;
                        }
                    }
                }

                if (ctrl.Controls.Count > 0)
                {
                    ApplyThemeToControls(ctrl.Controls, metroTheme);
                }
            }
        }
    }
}
