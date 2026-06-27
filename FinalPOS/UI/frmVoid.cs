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

namespace FinalPOS
{
    public partial class frmVoid : Form
    {
        SqlConnection cn = new SqlConnection();
        SqlCommand cm = new SqlCommand();
        SqlDataReader dr;
        DBConnection dbcon = new DBConnection();
        frmCancelDetails f;
        public frmVoid(frmCancelDetails frm)
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

        private void btnVoid_Click(object sender, EventArgs e)
        {
            try
            {
                if(txtPassword.Text != string.Empty)
                {
                    string user;
                    cn.Open();
                    cm = new SqlCommand("select * from tbl_Users where username = @username  and password = @password", cn);
                    cm.Parameters.AddWithValue("@username", txtUsername.Text);
                    cm.Parameters.AddWithValue("@password", txtPassword.Text);
                    dr = cm.ExecuteReader();
                    dr.Read();
                    if(dr.HasRows)
                    {
                        user = dr["username"].ToString();
                        dr.Close();
                        cn.Close();

                       SaveCancelOrder(user);
                        if(f.cboAction.Text == "Oui")
                        {
                            UpdateData("update tbl_Products set qty = qty + " + int.Parse(f.txtCancelQty.Text) + "where pcode = '" + f.txtpcode.Text + "'  ");
                        }

                        UpdateData("update tbl_Cart set qty = qty - " + int.Parse(f.txtCancelQty.Text) + "where id like '"+f.txtID.Text+"'   ");

                         MessageBox.Show("La transaction a été annulée avec succès.", "Annuler la Commande", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        this.Dispose();
                        f.RefreshList();
                        f.Dispose();
                    }
                    dr.Close();
                    cn.Close();
                }
            }
            catch(Exception ex)
            {
                cn.Close();
                 MessageBox.Show(ex.Message, "Avertissement", MessageBoxButtons.OK , MessageBoxIcon.Warning );
            }

           
        }
        public void SaveCancelOrder(string user)
        {
            cn.Open();
            cm = new SqlCommand("insert into tbl_Cancel (transno, pcode, price, qty, sdate, voidby, cancelledby, reason,action) values (@transno, @pcode, @price, @qty, @sdate, @voidby, @cancelledby, @reason, @action) ",cn);
            cm.Parameters.AddWithValue("transno", f.txtTransno.Text);
            cm.Parameters.AddWithValue("pcode", f.txtpcode.Text);
            cm.Parameters.AddWithValue("price", double.Parse(f.txtPrice.Text.Replace(" FCFA", "").Replace(" ", "").Trim(), System.Globalization.NumberStyles.Any));
            cm.Parameters.AddWithValue("qty", int.Parse(f.txtCancelQty.Text));
            cm.Parameters.AddWithValue("sdate", DateTime.Now);
            cm.Parameters.AddWithValue("voidby", user);
            cm.Parameters.AddWithValue("cancelledby", f.txtCancel.Text);
            cm.Parameters.AddWithValue("reason", f.txtReasons.Text);
            cm.Parameters.AddWithValue("action", f.cboAction.Text);

            cm.ExecuteNonQuery();

            cn.Close();
        }

        public void UpdateData(string sql)
        {
            cn.Open();
            cm = new SqlCommand(sql,cn);
            cm.ExecuteNonQuery();
            cn.Close();
        }
    }
}
