using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using FinalPOS.Domain;

namespace FinalPOS.Data
{
    public class StockRepository : IStockRepository
    {
        private readonly string _connectionString;

        public StockRepository(string connectionString)
        {
            _connectionString = connectionString;
        }

        public IEnumerable<Stock> GetPendingStocksByRefNo(string refNo)
        {
            var list = new List<Stock>();
            using (var cn = new SqlConnection(_connectionString))
            {
                using (var cm = new SqlCommand("select id, refno, pcode, pdesc, qty, sdate, stockinby, status, vendor from ViewStocks where refno = @refno and status = 'Pending'", cn))
                {
                    cm.Parameters.AddWithValue("@refno", refNo);
                    cn.Open();
                    using (var dr = cm.ExecuteReader())
                    {
                        while (dr.Read())
                        {
                            list.Add(new Stock
                            {
                                Id = Convert.ToInt32(dr["id"]),
                                RefNo = dr["refno"].ToString(),
                                ProductCode = dr["pcode"].ToString(),
                                Description = dr["pdesc"].ToString(),
                                Qty = dr["qty"] != DBNull.Value ? Convert.ToInt32(dr["qty"]) : 0,
                                StockInDate = Convert.ToDateTime(dr["sdate"]),
                                StockInBy = dr["stockinby"].ToString(),
                                Status = dr["status"].ToString(),
                                VendorName = dr["vendor"].ToString()
                            });
                        }
                    }
                }
            }
            return list;
        }

        public IEnumerable<Stock> GetStockInHistory(DateTime dateFrom, DateTime dateTo)
        {
            var list = new List<Stock>();
            using (var cn = new SqlConnection(_connectionString))
            {
                using (var cm = new SqlCommand("select id, refno, pcode, pdesc, qty, sdate, stockinby, status, vendor from ViewStocks where cast(sdate as date) between @dateFrom and @dateTo and status = 'Done'", cn))
                {
                    cm.Parameters.AddWithValue("@dateFrom", dateFrom.ToString("yyyy-MM-dd"));
                    cm.Parameters.AddWithValue("@dateTo", dateTo.ToString("yyyy-MM-dd"));
                    cn.Open();
                    using (var dr = cm.ExecuteReader())
                    {
                        while (dr.Read())
                        {
                            list.Add(new Stock
                            {
                                Id = Convert.ToInt32(dr["id"]),
                                RefNo = dr["refno"].ToString(),
                                ProductCode = dr["pcode"].ToString(),
                                Description = dr["pdesc"].ToString(),
                                Qty = dr["qty"] != DBNull.Value ? Convert.ToInt32(dr["qty"]) : 0,
                                StockInDate = Convert.ToDateTime(dr["sdate"]),
                                StockInBy = dr["stockinby"].ToString(),
                                Status = dr["status"].ToString(),
                                VendorName = dr["vendor"].ToString()
                            });
                        }
                    }
                }
            }
            return list;
        }

        public void AddPendingStock(string refNo, string pcode, DateTime sdate, string stockInBy, int vendorId)
        {
            using (var cn = new SqlConnection(_connectionString))
            {
                using (var cm = new SqlCommand("insert into tbl_Stocks_In(refno, pcode, sdate, stockinby, vendorid, qty, status) values(@refno, @pcode, @sdate, @stockinby, @vendorid, 0, 'Pending')", cn))
                {
                    cm.Parameters.AddWithValue("@refno", refNo);
                    cm.Parameters.AddWithValue("@pcode", pcode);
                    cm.Parameters.AddWithValue("@sdate", sdate);
                    cm.Parameters.AddWithValue("@stockinby", stockInBy);
                    cm.Parameters.AddWithValue("@vendorid", vendorId);
                    cn.Open();
                    cm.ExecuteNonQuery();
                }
            }
        }

        public void CompleteStockIn(int id, string pcode, int qty)
        {
            using (var cn = new SqlConnection(_connectionString))
            {
                cn.Open();
                using (var transaction = cn.BeginTransaction())
                {
                    try
                    {
                        // 1. Update product qty
                        using (var cmProduct = new SqlCommand("update tbl_Products set qty = qty + @qty where pcode = @pcode", cn, transaction))
                        {
                            cmProduct.Parameters.AddWithValue("@qty", qty);
                            cmProduct.Parameters.AddWithValue("@pcode", pcode);
                            cmProduct.ExecuteNonQuery();
                        }

                        // 2. Update stock entry status and qty
                        using (var cmStock = new SqlCommand("update tbl_Stocks_In set qty = isnull(qty, 0) + @qty, status = 'Done' where id = @id", cn, transaction))
                        {
                            cmStock.Parameters.AddWithValue("@qty", qty);
                            cmStock.Parameters.AddWithValue("@id", id);
                            cmStock.ExecuteNonQuery();
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

        public void DeletePendingStock(int id)
        {
            using (var cn = new SqlConnection(_connectionString))
            {
                using (var cm = new SqlCommand("delete from tbl_Stocks_In where id = @id", cn))
                {
                    cm.Parameters.AddWithValue("@id", id);
                    cn.Open();
                    cm.ExecuteNonQuery();
                }
            }
        }

        public IEnumerable<Vendor> GetVendors()
        {
            var list = new List<Vendor>();
            using (var cn = new SqlConnection(_connectionString))
            {
                using (var cm = new SqlCommand("select id, vendor, address, contactperson, telephone, email from tbl_Vendor order by vendor", cn))
                {
                    cn.Open();
                    using (var dr = cm.ExecuteReader())
                    {
                        while (dr.Read())
                        {
                            list.Add(new Vendor
                            {
                                Id = Convert.ToInt32(dr["id"]),
                                VendorName = dr["vendor"].ToString(),
                                Address = dr["address"].ToString(),
                                ContactPerson = dr["contactperson"].ToString(),
                                Phone = dr["telephone"].ToString(),
                                Email = dr["email"].ToString()
                            });
                        }
                    }
                }
            }
            return list;
        }

        public Vendor GetVendorByName(string vendorName)
        {
            using (var cn = new SqlConnection(_connectionString))
            {
                using (var cm = new SqlCommand("select id, vendor, address, contactperson, telephone, email from tbl_Vendor where vendor = @vendorName", cn))
                {
                    cm.Parameters.AddWithValue("@vendorName", vendorName);
                    cn.Open();
                    using (var dr = cm.ExecuteReader())
                    {
                        if (dr.Read())
                        {
                            return new Vendor
                            {
                                Id = Convert.ToInt32(dr["id"]),
                                VendorName = dr["vendor"].ToString(),
                                Address = dr["address"].ToString(),
                                ContactPerson = dr["contactperson"].ToString(),
                                Phone = dr["telephone"].ToString(),
                                Email = dr["email"].ToString()
                            };
                        }
                    }
                }
            }
            return null;
        }

        public void AdjustStock(string referenceNo, string pcode, int qty, string action, string remarks, DateTime sdate, string user)
        {
            using (var cn = new SqlConnection(_connectionString))
            {
                cn.Open();
                using (var transaction = cn.BeginTransaction())
                {
                    try
                    {
                        // 1. Update product quantity
                        string updateSql;
                        if (action == "RETIRER DU STOCK")
                        {
                            updateSql = "update tbl_Products set qty = qty - @qty where pcode = @pcode";
                        }
                        else if (action == "AJOUTER AU STOCK")
                        {
                            updateSql = "update tbl_Products set qty = qty + @qty where pcode = @pcode";
                        }
                        else
                        {
                            throw new ArgumentException("Action d'ajustement non valide : " + action);
                        }

                        using (var cmProduct = new SqlCommand(updateSql, cn, transaction))
                        {
                            cmProduct.Parameters.AddWithValue("@qty", qty);
                            cmProduct.Parameters.AddWithValue("@pcode", pcode);
                            cmProduct.ExecuteNonQuery();
                        }

                        // 2. Insert into tbl_Adjustment
                        using (var cmAdj = new SqlCommand("insert into tbl_Adjustment(referenceno, pcode, qty, action, remarks, sdate, [user]) values (@referenceno, @pcode, @qty, @action, @remarks, @sdate, @user)", cn, transaction))
                        {
                            cmAdj.Parameters.AddWithValue("@referenceno", referenceNo);
                            cmAdj.Parameters.AddWithValue("@pcode", pcode);
                            cmAdj.Parameters.AddWithValue("@qty", qty);
                            cmAdj.Parameters.AddWithValue("@action", action);
                            cmAdj.Parameters.AddWithValue("@remarks", remarks);
                            cmAdj.Parameters.AddWithValue("@sdate", sdate.ToString("yyyy-MM-dd"));
                            cmAdj.Parameters.AddWithValue("@user", user);
                            cmAdj.ExecuteNonQuery();
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
    }
}
