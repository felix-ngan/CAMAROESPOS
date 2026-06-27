using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using FinalPOS.Domain;

namespace FinalPOS.Data
{
    public class ReportRepository : IReportRepository
    {
        private readonly string _connectionString;

        public ReportRepository(string connectionString)
        {
            _connectionString = connectionString;
        }

        public IEnumerable<string> GetCashiers()
        {
            var list = new List<string>();
            using (var cn = new SqlConnection(_connectionString))
            {
                using (var cm = new SqlCommand("select username from tbl_Users where role like 'Cashier' order by username", cn))
                {
                    cn.Open();
                    using (var dr = cm.ExecuteReader())
                    {
                        while (dr.Read())
                        {
                            list.Add(dr["username"].ToString());
                        }
                    }
                }
            }
            return list;
        }

        public IEnumerable<SoldItem> GetSoldItems(DateTime from, DateTime to, string cashier)
        {
            var list = new List<SoldItem>();
            using (var cn = new SqlConnection(_connectionString))
            {
                string sql;
                bool filterCashier = !string.IsNullOrEmpty(cashier) && cashier != "Tous les Caissiers";

                if (filterCashier)
                {
                    sql = @"select c.id, c.transno, c.pcode , p.pdesc, c.price, c.qty, c.disc , c.total, c.sdate, c.cashier 
                            from tbl_Cart as c 
                            inner join tbl_Products as p on c.pcode = p.pcode 
                            where status like 'Sold' and sdate between @from and @to and cashier like @cashier
                            order by c.sdate desc";
                }
                else
                {
                    sql = @"select c.id, c.transno, c.pcode , p.pdesc, c.price, c.qty, c.disc , c.total, c.sdate, c.cashier 
                            from tbl_Cart as c 
                            inner join tbl_Products as p on c.pcode = p.pcode 
                            where status like 'Sold' and sdate between @from and @to
                            order by c.sdate desc";
                }

                using (var cm = new SqlCommand(sql, cn))
                {
                    cm.Parameters.AddWithValue("@from", from);
                    cm.Parameters.AddWithValue("@to", to);
                    if (filterCashier)
                    {
                        cm.Parameters.AddWithValue("@cashier", cashier);
                    }

                    cn.Open();
                    using (var dr = cm.ExecuteReader())
                    {
                        while (dr.Read())
                        {
                            list.Add(new SoldItem
                            {
                                Id = Convert.ToInt32(dr["id"]),
                                TransNo = dr["transno"].ToString(),
                                ProductCode = dr["pcode"].ToString(),
                                Description = dr["pdesc"].ToString(),
                                Price = Convert.ToDouble(dr["price"]),
                                Qty = Convert.ToInt32(dr["qty"]),
                                Discount = Convert.ToDouble(dr["disc"]),
                                Total = Convert.ToDouble(dr["total"]),
                                SoldDate = Convert.ToDateTime(dr["sdate"]),
                                Cashier = dr["cashier"].ToString()
                            });
                        }
                    }
                }
            }
            return list;
        }

        public IEnumerable<SoldItem> GetTopSellingByAmount(DateTime from, DateTime to)
        {
            var list = new List<SoldItem>();
            using (var cn = new SqlConnection(_connectionString))
            {
                string sql = @"select top 10 pcode , pdesc, isnull(sum(qty),0) as qty, isnull(sum(total),0) as total  
                               from ViewSoldItems 
                               where sdate between @from and @to and status like 'Sold' 
                               group by pcode, pdesc 
                               order by total desc";

                using (var cm = new SqlCommand(sql, cn))
                {
                    cm.Parameters.AddWithValue("@from", from);
                    cm.Parameters.AddWithValue("@to", to);

                    cn.Open();
                    using (var dr = cm.ExecuteReader())
                    {
                        while (dr.Read())
                        {
                            list.Add(new SoldItem
                            {
                                ProductCode = dr["pcode"].ToString(),
                                Description = dr["pdesc"].ToString(),
                                Qty = Convert.ToInt32(dr["qty"]),
                                Total = Convert.ToDouble(dr["total"])
                            });
                        }
                    }
                }
            }
            return list;
        }

        public IEnumerable<SoldItem> GetTopSellingByQty(DateTime from, DateTime to)
        {
            var list = new List<SoldItem>();
            using (var cn = new SqlConnection(_connectionString))
            {
                string sql = @"select top 10 pcode , pdesc, isnull(sum(qty),0) as qty, isnull(sum(total),0) as total  
                               from ViewSoldItems 
                               where sdate between @from and @to and status like 'Sold' 
                               group by pcode, pdesc 
                               order by qty desc";

                using (var cm = new SqlCommand(sql, cn))
                {
                    cm.Parameters.AddWithValue("@from", from);
                    cm.Parameters.AddWithValue("@to", to);

                    cn.Open();
                    using (var dr = cm.ExecuteReader())
                    {
                        while (dr.Read())
                        {
                            list.Add(new SoldItem
                            {
                                ProductCode = dr["pcode"].ToString(),
                                Description = dr["pdesc"].ToString(),
                                Qty = Convert.ToInt32(dr["qty"]),
                                Total = Convert.ToDouble(dr["total"])
                            });
                        }
                    }
                }
            }
            return list;
        }

        public IEnumerable<SoldItem> GetSoldItemsGrouped(DateTime from, DateTime to)
        {
            var list = new List<SoldItem>();
            using (var cn = new SqlConnection(_connectionString))
            {
                string sql = @"select c.pcode, p.pdesc, c.price, sum(c.qty) as tot_qty, sum(c.disc) as tot_disc, sum(c.total) as total 
                               from tbl_Cart as c 
                               inner join tbl_Products as p on c.pcode = p.pcode 
                               where status like 'Sold' and sdate between @from and @to 
                               group by c.pcode , p.pdesc, c.price
                               order by total desc";

                using (var cm = new SqlCommand(sql, cn))
                {
                    cm.Parameters.AddWithValue("@from", from);
                    cm.Parameters.AddWithValue("@to", to);

                    cn.Open();
                    using (var dr = cm.ExecuteReader())
                    {
                        while (dr.Read())
                        {
                            list.Add(new SoldItem
                            {
                                ProductCode = dr["pcode"].ToString(),
                                Description = dr["pdesc"].ToString(),
                                Price = Convert.ToDouble(dr["price"]),
                                Qty = Convert.ToInt32(dr["tot_qty"]),
                                Discount = Convert.ToDouble(dr["tot_disc"]),
                                Total = Convert.ToDouble(dr["total"])
                            });
                        }
                    }
                }
            }
            return list;
        }

        public double GetTotalRevenue(DateTime from, DateTime to)
        {
            using (var cn = new SqlConnection(_connectionString))
            {
                string sql = "select isnull(sum(total),0) as total from tbl_Cart where status like 'Sold' and sdate between @from and @to";
                using (var cm = new SqlCommand(sql, cn))
                {
                    cm.Parameters.AddWithValue("@from", from);
                    cm.Parameters.AddWithValue("@to", to);

                    cn.Open();
                    var res = cm.ExecuteScalar();
                    return res != DBNull.Value ? Convert.ToDouble(res) : 0.0;
                }
            }
        }

        public IEnumerable<CancelledOrder> GetCancelledOrders(DateTime from, DateTime to)
        {
            var list = new List<CancelledOrder>();
            using (var cn = new SqlConnection(_connectionString))
            {
                string sql = "select transno, pcode, pdesc, price, qty, total, sdate, voidby, cancelledby, reason, action from CancelledOrder where sdate between @from and @to order by sdate desc";
                using (var cm = new SqlCommand(sql, cn))
                {
                    cm.Parameters.AddWithValue("@from", from);
                    cm.Parameters.AddWithValue("@to", to);

                    cn.Open();
                    using (var dr = cm.ExecuteReader())
                    {
                        while (dr.Read())
                        {
                            list.Add(new CancelledOrder
                            {
                                Id = 0,
                                TransNo = dr["transno"].ToString(),
                                ProductCode = dr["pcode"].ToString(),
                                Description = dr["pdesc"].ToString(),
                                Price = Convert.ToDouble(dr["price"]),
                                Qty = Convert.ToInt32(dr["qty"]),
                                Total = Convert.ToDouble(dr["total"]),
                                SoldDate = Convert.ToDateTime(dr["sdate"]),
                                VoidBy = dr["voidby"].ToString(),
                                CancelledBy = dr["cancelledby"].ToString(),
                                Reason = dr["reason"].ToString(),
                                Action = dr["action"].ToString()
                            });
                        }
                    }
                }
            }
            return list;
        }

        public IEnumerable<CustomerWallet> GetCustomerWallets(string searchPhone)
        {
            var list = new List<CustomerWallet>();
            using (var cn = new SqlConnection(_connectionString))
            {
                string sql = "select phone, balance from tbl_CustomerWallets";
                if (!string.IsNullOrEmpty(searchPhone))
                {
                    sql += " where phone like @phone";
                }
                sql += " order by balance desc";

                using (var cm = new SqlCommand(sql, cn))
                {
                    if (!string.IsNullOrEmpty(searchPhone))
                    {
                        cm.Parameters.AddWithValue("@phone", "%" + searchPhone + "%");
                    }

                    cn.Open();
                    using (var dr = cm.ExecuteReader())
                    {
                        while (dr.Read())
                        {
                            list.Add(new CustomerWallet
                            {
                                Phone = dr["phone"].ToString(),
                                Balance = Convert.ToDouble(dr["balance"])
                            });
                        }
                    }
                }
            }
            return list;
        }

        public IEnumerable<WalletTransaction> GetWalletTransactions(DateTime from, DateTime to, string searchPhone)
        {
            var list = new List<WalletTransaction>();
            using (var cn = new SqlConnection(_connectionString))
            {
                // Verify if the table exists first (just in case the auto-migration hasn't run yet or fails)
                string checkTableQuery = "IF OBJECT_ID('tbl_WalletTransactions', 'U') IS NULL SELECT 0 ELSE SELECT 1";
                using (var checkCmd = new SqlCommand(checkTableQuery, cn))
                {
                    cn.Open();
                    int exists = (int)checkCmd.ExecuteScalar();
                    cn.Close();
                    if (exists == 0)
                    {
                        return list; // Return empty list if table doesn't exist yet
                    }
                }

                string sql = "select id, phone, transno, type, amount, created_at from tbl_WalletTransactions where created_at between @from and @to";
                if (!string.IsNullOrEmpty(searchPhone))
                {
                    sql += " and phone like @phone";
                }
                sql += " order by created_at desc";

                using (var cm = new SqlCommand(sql, cn))
                {
                    cm.Parameters.AddWithValue("@from", from);
                    cm.Parameters.AddWithValue("@to", to);
                    if (!string.IsNullOrEmpty(searchPhone))
                    {
                        cm.Parameters.AddWithValue("@phone", "%" + searchPhone + "%");
                    }

                    cn.Open();
                    using (var dr = cm.ExecuteReader())
                    {
                        while (dr.Read())
                        {
                            list.Add(new WalletTransaction
                            {
                                Id = Convert.ToInt32(dr["id"]),
                                Phone = dr["phone"].ToString(),
                                TransNo = dr["transno"].ToString(),
                                Type = dr["type"].ToString(),
                                Amount = Convert.ToDouble(dr["amount"]),
                                CreatedAt = Convert.ToDateTime(dr["created_at"])
                            });
                        }
                    }
                }
            }
            return list;
        }

        public IEnumerable<StoreRevenue> GetStoreRevenueConsolidated(DateTime from, DateTime to, string centralConnStr)
        {
            var list = new List<StoreRevenue>();
            if (string.IsNullOrWhiteSpace(centralConnStr))
            {
                return list;
            }

            using (var cn = new SqlConnection(centralConnStr))
            {
                string sql = @"select store_id, sum(total) as revenue, max(sdate) as last_activity 
                               from tbl_Cart 
                               where status = 'Sold' and sdate between @from and @to
                               group by store_id
                               order by store_id";

                using (var cm = new SqlCommand(sql, cn))
                {
                    cm.Parameters.AddWithValue("@from", from);
                    cm.Parameters.AddWithValue("@to", to);

                    try
                    {
                        cn.Open();
                        using (var dr = cm.ExecuteReader())
                        {
                            while (dr.Read())
                            {
                                int? storeId = dr["store_id"] != DBNull.Value ? Convert.ToInt32(dr["store_id"]) : (int?)null;
                                double revenue = dr["revenue"] != DBNull.Value ? Convert.ToDouble(dr["revenue"]) : 0.0;
                                DateTime? lastActivity = dr["last_activity"] != DBNull.Value ? Convert.ToDateTime(dr["last_activity"]) : (DateTime?)null;
                                list.Add(new StoreRevenue
                                {
                                    StoreId = storeId,
                                    Revenue = revenue,
                                    LastActivity = lastActivity
                                });
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine("GetStoreRevenueConsolidated error: " + ex.Message);
                    }
                }
            }
            return list;
        }
    }
}
