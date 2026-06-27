using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using FinalPOS.Domain;

namespace FinalPOS.Data
{
    public class CartRepository : ICartRepository
    {
        private readonly string _connectionString;

        public CartRepository(string connectionString)
        {
            _connectionString = connectionString;
            AutoMigrateDatabase();
        }

        private void AutoMigrateDatabase()
        {
            try
            {
                using (var cn = new SqlConnection(_connectionString))
                {
                    cn.Open();
                    // Create tbl_CustomerWallets if it does not exist
                    string query = "IF OBJECT_ID('tbl_CustomerWallets', 'U') IS NULL " +
                                   "BEGIN " +
                                   "    CREATE TABLE tbl_CustomerWallets ( " +
                                   "        phone varchar(50) PRIMARY KEY, " +
                                   "        balance decimal(18,2) NOT NULL " +
                                   "    ) " +
                                   "END";
                    using (var cm = new SqlCommand(query, cn))
                    {
                        cm.ExecuteNonQuery();
                    }

                    // Create tbl_WalletTransactions if it does not exist
                    string queryTx = "IF OBJECT_ID('tbl_WalletTransactions', 'U') IS NULL " +
                                     "BEGIN " +
                                     "    CREATE TABLE tbl_WalletTransactions ( " +
                                     "        id INT IDENTITY(1,1) PRIMARY KEY, " +
                                     "        phone varchar(50) NOT NULL, " +
                                     "        transno varchar(50) NULL, " +
                                     "        type varchar(20) NOT NULL, " + // 'Credit' or 'Debit'
                                     "        amount decimal(18,2) NOT NULL, " +
                                     "        created_at datetime NOT NULL " +
                                     "    ) " +
                                     "END";
                    using (var cmTx = new SqlCommand(queryTx, cn))
                    {
                        cmTx.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("CartRepository auto-migration error: " + ex.Message);
            }
        }

        public IEnumerable<CartItem> GetPendingCartItems(string transNo)
        {
            var list = new List<CartItem>();
            using (var cn = new SqlConnection(_connectionString))
            {
                using (var cm = new SqlCommand("select c.id, c.pcode, p.pdesc, c.price, c.qty, c.disc, c.disc_per, c.total, c.sdate, c.cashier, c.status from tbl_Cart as c inner join tbl_Products as p on c.pcode = p.pcode where c.transno = @transno and c.status = 'Pending'", cn))
                {
                    cm.Parameters.AddWithValue("@transno", transNo);
                    cn.Open();
                    using (var dr = cm.ExecuteReader())
                    {
                        while (dr.Read())
                        {
                            list.Add(new CartItem
                            {
                                Id = Convert.ToInt32(dr["id"]),
                                ProductCode = dr["pcode"].ToString(),
                                ProductDescription = dr["pdesc"].ToString(),
                                Price = Convert.ToDouble(dr["price"]),
                                Qty = Convert.ToInt32(dr["qty"]),
                                DiscountAmount = dr["disc"] != DBNull.Value ? Convert.ToDouble(dr["disc"]) : 0.0,
                                DiscountPercent = dr["disc_per"] != DBNull.Value ? Convert.ToDouble(dr["disc_per"]) : 0.0,
                                Total = dr["total"] != DBNull.Value ? Convert.ToDouble(dr["total"]) : 0.0,
                                Date = Convert.ToDateTime(dr["sdate"]),
                                Cashier = dr["cashier"].ToString(),
                                Status = dr["status"].ToString()
                            });
                        }
                    }
                }
            }
            return list;
        }

        public string GetLastTransactionNo(string datePrefix)
        {
            using (var cn = new SqlConnection(_connectionString))
            {
                using (var cm = new SqlCommand("select top 1 transno from tbl_Cart where transno like @datePrefix order by id desc", cn))
                {
                    cm.Parameters.AddWithValue("@datePrefix", datePrefix + "%");
                    cn.Open();
                    object result = cm.ExecuteScalar();
                    return result?.ToString();
                }
            }
        }

        public CartItem GetPendingItem(string transNo, string pcode)
        {
            using (var cn = new SqlConnection(_connectionString))
            {
                using (var cm = new SqlCommand("select id, transno, pcode, price, qty, disc, disc_per, sdate, cashier, status from tbl_Cart where transno = @transno and pcode = @pcode and status = 'Pending'", cn))
                {
                    cm.Parameters.AddWithValue("@transno", transNo);
                    cm.Parameters.AddWithValue("@pcode", pcode);
                    cn.Open();
                    using (var dr = cm.ExecuteReader())
                    {
                        if (dr.Read())
                        {
                            return new CartItem
                            {
                                Id = Convert.ToInt32(dr["id"]),
                                TransactionNo = dr["transno"].ToString(),
                                ProductCode = dr["pcode"].ToString(),
                                Price = Convert.ToDouble(dr["price"]),
                                Qty = Convert.ToInt32(dr["qty"]),
                                DiscountAmount = dr["disc"] != DBNull.Value ? Convert.ToDouble(dr["disc"]) : 0.0,
                                DiscountPercent = dr["disc_per"] != DBNull.Value ? Convert.ToDouble(dr["disc_per"]) : 0.0,
                                Date = Convert.ToDateTime(dr["sdate"]),
                                Cashier = dr["cashier"].ToString(),
                                Status = dr["status"].ToString()
                            };
                        }
                    }
                }
            }
            return null;
        }

        public void AddToCart(string transNo, string pcode, double price, int qty, string cashier)
        {
            using (var cn = new SqlConnection(_connectionString))
            {
                using (var cm = new SqlCommand("insert into tbl_Cart(transno, pcode, price, qty, sdate, cashier, status, disc, disc_per) values (@transno, @pcode, @price, @qty, @sdate, @cashier, 'Pending', 0, 0)", cn))
                {
                    cm.Parameters.AddWithValue("@transno", transNo);
                    cm.Parameters.AddWithValue("@pcode", pcode);
                    cm.Parameters.AddWithValue("@price", price);
                    cm.Parameters.AddWithValue("@qty", qty);
                    cm.Parameters.AddWithValue("@sdate", DateTime.Now);
                    cm.Parameters.AddWithValue("@cashier", cashier);
                    cn.Open();
                    cm.ExecuteNonQuery();
                }
            }
        }

        public void UpdateCartQty(int id, int qty)
        {
            using (var cn = new SqlConnection(_connectionString))
            {
                using (var cm = new SqlCommand("update tbl_Cart set qty = @qty where id = @id", cn))
                {
                    cm.Parameters.AddWithValue("@qty", qty);
                    cm.Parameters.AddWithValue("@id", id);
                    cn.Open();
                    cm.ExecuteNonQuery();
                }
            }
        }

        public void UpdateCartQtyByPcode(string transNo, string pcode, int qty)
        {
            using (var cn = new SqlConnection(_connectionString))
            {
                using (var cm = new SqlCommand("update tbl_Cart set qty = @qty where transno = @transno and pcode = @pcode and status = 'Pending'", cn))
                {
                    cm.Parameters.AddWithValue("@qty", qty);
                    cm.Parameters.AddWithValue("@transno", transNo);
                    cm.Parameters.AddWithValue("@pcode", pcode);
                    cn.Open();
                    cm.ExecuteNonQuery();
                }
            }
        }

        public void DeleteCartItem(int id)
        {
            using (var cn = new SqlConnection(_connectionString))
            {
                using (var cm = new SqlCommand("delete from tbl_Cart where id = @id", cn))
                {
                    cm.Parameters.AddWithValue("@id", id);
                    cn.Open();
                    cm.ExecuteNonQuery();
                }
            }
        }

        public void ClearCart(string transNo)
        {
            using (var cn = new SqlConnection(_connectionString))
            {
                using (var cm = new SqlCommand("delete from tbl_Cart where transno = @transno and status = 'Pending'", cn))
                {
                    cm.Parameters.AddWithValue("@transno", transNo);
                    cn.Open();
                    cm.ExecuteNonQuery();
                }
            }
        }

        public void ApplyDiscount(int id, double discountAmount, double discountPercent)
        {
            using (var cn = new SqlConnection(_connectionString))
            {
                using (var cm = new SqlCommand("update tbl_Cart set disc = @disc, disc_per = @disc_per where id = @id", cn))
                {
                    cm.Parameters.AddWithValue("@disc", discountAmount);
                    cm.Parameters.AddWithValue("@disc_per", discountPercent);
                    cm.Parameters.AddWithValue("@id", id);
                    cn.Open();
                    cm.ExecuteNonQuery();
                }
            }
        }

        public void SettlePayment(string transNo, string cashier, IEnumerable<(int cartId, string pcode, int qty)> items, string customerPhone, double walletDeduction, double walletCredit)
        {
            using (var cn = new SqlConnection(_connectionString))
            {
                cn.Open();
                using (var transaction = cn.BeginTransaction())
                {
                    try
                    {
                        object storeIdVal = DBNull.Value;
                        object registerIdVal = DBNull.Value;

                        using (var cmStore = new SqlCommand("select setting_value from tbl_LocalSettings where setting_key = 'StoreId'", cn, transaction))
                        {
                            var res = cmStore.ExecuteScalar();
                            if (res != null && res != DBNull.Value && int.TryParse(res.ToString(), out int sId))
                                storeIdVal = sId;
                        }

                        using (var cmReg = new SqlCommand("select setting_value from tbl_LocalSettings where setting_key = 'RegisterId'", cn, transaction))
                        {
                            var res = cmReg.ExecuteScalar();
                            if (res != null && res != DBNull.Value && int.TryParse(res.ToString(), out int rId))
                                registerIdVal = rId;
                        }

                        foreach (var item in items)
                        {
                            // 1. Subtract qty from tbl_Products
                            using (var cmProduct = new SqlCommand("update tbl_Products set qty = qty - @qty where pcode = @pcode", cn, transaction))
                            {
                                cmProduct.Parameters.AddWithValue("@qty", item.qty);
                                cmProduct.Parameters.AddWithValue("@pcode", item.pcode);
                                cmProduct.ExecuteNonQuery();
                            }

                            // 2. Set status to 'Sold' in tbl_Cart, recording checkout time, store/register details and synced=0
                            using (var cmCart = new SqlCommand("update tbl_Cart set status = 'Sold', sdate = @sdate, store_id = @store_id, register_id = @register_id, synced = 0 where id = @id", cn, transaction))
                            {
                                cmCart.Parameters.AddWithValue("@sdate", DateTime.Now);
                                cmCart.Parameters.AddWithValue("@store_id", storeIdVal);
                                cmCart.Parameters.AddWithValue("@register_id", registerIdVal);
                                cmCart.Parameters.AddWithValue("@id", item.cartId);
                                cmCart.ExecuteNonQuery();
                            }
                        }

                        // 3. Handle Wallet operations
                        if (!string.IsNullOrEmpty(customerPhone))
                        {
                            int walletCount = 0;
                            using (var cmCount = new SqlCommand("select count(*) from tbl_CustomerWallets where phone = @phone", cn, transaction))
                            {
                                cmCount.Parameters.AddWithValue("@phone", customerPhone);
                                walletCount = Convert.ToInt32(cmCount.ExecuteScalar());
                            }

                            if (walletCount > 0)
                            {
                                using (var cmUpdate = new SqlCommand("update tbl_CustomerWallets set balance = balance - @deduction + @credit where phone = @phone", cn, transaction))
                                {
                                    cmUpdate.Parameters.AddWithValue("@deduction", walletDeduction);
                                    cmUpdate.Parameters.AddWithValue("@credit", walletCredit);
                                    cmUpdate.Parameters.AddWithValue("@phone", customerPhone);
                                    cmUpdate.ExecuteNonQuery();
                                }
                            }
                            else
                            {
                                double initialBalance = walletCredit - walletDeduction;
                                using (var cmInsert = new SqlCommand("insert into tbl_CustomerWallets (phone, balance) values (@phone, @balance)", cn, transaction))
                                {
                                    cmInsert.Parameters.AddWithValue("@phone", customerPhone);
                                    cmInsert.Parameters.AddWithValue("@balance", initialBalance);
                                    cmInsert.ExecuteNonQuery();
                                }
                            }

                            // Log transaction events in the wallet transactions audit table
                            if (walletDeduction > 0)
                            {
                                using (var cmLog = new SqlCommand("insert into tbl_WalletTransactions (phone, transno, type, amount, created_at, store_id, register_id, synced) values (@phone, @transno, 'Debit', @amount, @created_at, @store_id, @register_id, 0)", cn, transaction))
                                {
                                    cmLog.Parameters.AddWithValue("@phone", customerPhone);
                                    cmLog.Parameters.AddWithValue("@transno", transNo);
                                    cmLog.Parameters.AddWithValue("@amount", walletDeduction);
                                    cmLog.Parameters.AddWithValue("@created_at", DateTime.Now);
                                    cmLog.Parameters.AddWithValue("@store_id", storeIdVal);
                                    cmLog.Parameters.AddWithValue("@register_id", registerIdVal);
                                    cmLog.ExecuteNonQuery();
                                }
                            }
                            if (walletCredit > 0)
                            {
                                using (var cmLog = new SqlCommand("insert into tbl_WalletTransactions (phone, transno, type, amount, created_at, store_id, register_id, synced) values (@phone, @transno, 'Credit', @amount, @created_at, @store_id, @register_id, 0)", cn, transaction))
                                {
                                    cmLog.Parameters.AddWithValue("@phone", customerPhone);
                                    cmLog.Parameters.AddWithValue("@transno", transNo);
                                    cmLog.Parameters.AddWithValue("@amount", walletCredit);
                                    cmLog.Parameters.AddWithValue("@created_at", DateTime.Now);
                                    cmLog.Parameters.AddWithValue("@store_id", storeIdVal);
                                    cmLog.Parameters.AddWithValue("@register_id", registerIdVal);
                                    cmLog.ExecuteNonQuery();
                                }
                            }
                        }

                        transaction.Commit();
                    }
                    catch
                    {
                        transaction.Rollback();
                        throw;
                    }
                }
            }
        }

        public double GetWalletBalance(string phone)
        {
            if (string.IsNullOrEmpty(phone)) return 0.0;
            using (var cn = new SqlConnection(_connectionString))
            {
                using (var cm = new SqlCommand("select balance from tbl_CustomerWallets where phone = @phone", cn))
                {
                    cm.Parameters.AddWithValue("@phone", phone);
                    cn.Open();
                    object result = cm.ExecuteScalar();
                    if (result != null && result != DBNull.Value)
                    {
                        return Convert.ToDouble(result);
                    }
                }
            }
            return 0.0;
        }

        public void UpdateWalletBalance(string phone, double amount)
        {
            if (string.IsNullOrEmpty(phone)) return;
            using (var cn = new SqlConnection(_connectionString))
            {
                cn.Open();
                int count = 0;
                using (var cmCount = new SqlCommand("select count(*) from tbl_CustomerWallets where phone = @phone", cn))
                {
                    cmCount.Parameters.AddWithValue("@phone", phone);
                    count = Convert.ToInt32(cmCount.ExecuteScalar());
                }

                if (count > 0)
                {
                    using (var cmUpdate = new SqlCommand("update tbl_CustomerWallets set balance = balance + @amount where phone = @phone", cn))
                    {
                        cmUpdate.Parameters.AddWithValue("@amount", amount);
                        cmUpdate.Parameters.AddWithValue("@phone", phone);
                        cmUpdate.ExecuteNonQuery();
                    }
                }
                else
                {
                    using (var cmInsert = new SqlCommand("insert into tbl_CustomerWallets (phone, balance) values (@phone, @amount)", cn))
                    {
                        cmInsert.Parameters.AddWithValue("@phone", phone);
                        cmInsert.Parameters.AddWithValue("@amount", amount);
                        cmInsert.ExecuteNonQuery();
                    }
                }
            }
        }

        public double GetVatRate()
        {
            double vat = 0;
            using (var cn = new SqlConnection(_connectionString))
            {
                using (var cm = new SqlCommand("select top 1 vat from tbl_Vat", cn))
                {
                    cn.Open();
                    object result = cm.ExecuteScalar();
                    if (result != null && result != DBNull.Value)
                    {
                        vat = Convert.ToDouble(result);
                    }
                }
            }
            return vat;
        }
    }
}
