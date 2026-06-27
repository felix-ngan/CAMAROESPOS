using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.SqlClient;
using System.Data.Common;

namespace FinalPOS
{
    public class DBConnection
    {
        SqlConnection cn = new SqlConnection();
        SqlCommand cm = new SqlCommand();
        SqlDataReader dr;
        private double dailysales;
        private int productline;
        private string con;
        private int stockonhand;
        private int critical;
        public string MyConnection()
        {
            con = @"Data Source=DESKTOP-9EH20OI\SQLEXPRESS01;Initial Catalog=NewOne;Integrated Security=True";
            return con;
        }
        public double GetVal()
        {
            double vat = 0;
            cn.ConnectionString = MyConnection();
            cn.Open();
            cm = new SqlCommand("select * from tbl_Vat", cn);
            dr = cm.ExecuteReader();
            while (dr.Read())
            {
                vat = Double.Parse(dr["vat"].ToString());
            }
            dr.Close();
            cn.Close();

            return vat;
        }


        public double DailySales()
        {
            cn = new SqlConnection();
            cn.ConnectionString = MyConnection();
            cn.Open();
            cm = new SqlCommand("select isnull (sum(total),0) as total from tbl_Cart where sdate between @sdate1 and @sdate2 and status like 'Sold'   ", cn);
            cm.Parameters.Add("@sdate1", System.Data.SqlDbType.Date).Value = DateTime.Today;
            cm.Parameters.Add("@sdate2", System.Data.SqlDbType.Date).Value = DateTime.Today;
            object result = cm.ExecuteScalar();
            dailysales = result != null && result != DBNull.Value ? double.Parse(result.ToString()) : 0.0;

           cn.Close();
            return dailysales;
        }

        public double ProductLine()
        {
            cn = new SqlConnection();
            cn.ConnectionString = con;
            cn.Open();
            cm = new SqlCommand("select count(*)  from tbl_Products   ", cn);
            productline = int.Parse(cm.ExecuteScalar().ToString());

            cn.Close();
            return productline;
        }

        public double StockOnHand()
        {
            cn = new SqlConnection();
            cn.ConnectionString = con;
            cn.Open();
            cm = new SqlCommand("select isnull(sum(qty),0)  as qty  from tbl_Products   ", cn);
            stockonhand = int.Parse(cm.ExecuteScalar().ToString());

            cn.Close();
            return stockonhand;
        }

        public double Critical()
        {
            cn = new SqlConnection();
            cn.ConnectionString = con;
            cn.Open();
            cm = new SqlCommand("select count(*) from ViewCriticalItems  ", cn);
            critical = int.Parse(cm.ExecuteScalar().ToString());

            cn.Close();
            return critical;
        }

        public string GetPassword(string user)
        {
            string password="";
            cn.ConnectionString = MyConnection();
            cn.Open();
            cm = new SqlCommand("select * from tbl_Users where username = @username", cn);
            cm.Parameters.AddWithValue("username", user);
            dr = cm.ExecuteReader();
            dr.Read();
            if(dr.HasRows)
            {
                password = dr["password"].ToString();
            }
       
           
            dr.Close();
            cn.Close();


            return password;
        }
       
    }
}
