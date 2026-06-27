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
using FinalPOS.Domain;

namespace FinalPOS
{
    public partial class frmChangePassword : Form
    {
        SqlConnection cn = new SqlConnection();
        SqlCommand cm = new SqlCommand();
        DBConnection dbcon = new DBConnection();
        frmPOS f;
        public frmChangePassword(frmPOS frm)
        {
            InitializeComponent();
            this.Load += (s, e) => {
                FinalPOS.UI.ThemeManager.ApplyTheme(this);
                FinalPOS.UI.LanguageManager.ApplyLanguage(this);
            };
            cn = new SqlConnection(dbcon.MyConnection());
            f = frm;
        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {
            this.Dispose();
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            try
            {
                string _oldpass = dbcon.GetPassword(f.lblUser.Text);
                if (!PasswordHasher.VerifyPassword(txtOld.Text, _oldpass))
                {
                    MessageBox.Show("L'ancien mot de passe ne correspond pas !", "ERREUR", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
                else if(txtNew.Text != txtConfNew.Text)
                {
                    MessageBox.Show("Les nouveaux mots de passe ne correspondent pas !", "ERREUR", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
                else
                {
                    if (MessageBox.Show("Voulez-vous modifier le mot de passe ?", "Confirmer", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                    {
                        string newHashed = PasswordHasher.HashPassword(txtNew.Text);
                        cn.Open();
                        cm = new SqlCommand(" update tbl_Users set password = @password  where username = @username",cn);
                        cm.Parameters.AddWithValue("@password", newHashed);
                        cm.Parameters.AddWithValue("@username", f.lblUser.Text);
                        cm.ExecuteNonQuery();
                        cn.Close();
                        
                        MessageBox.Show("Mot de passe modifié avec succès.", "SUCCÈS", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        this.Dispose();
                    }
                }
            }
            catch(Exception ex)
            {
                cn.Close();
                MessageBox.Show(ex.Message, "ERREUR", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }
    }
}
