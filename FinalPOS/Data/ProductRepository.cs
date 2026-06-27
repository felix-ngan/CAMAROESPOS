using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using FinalPOS.Domain;

namespace FinalPOS.Data
{
    public class ProductRepository : IProductRepository
    {
        private readonly string _connectionString;

        public ProductRepository(string connectionString)
        {
            _connectionString = connectionString;
        }

        public IEnumerable<Product> Search(string searchText)
        {
            var list = new List<Product>();
            using (var cn = new SqlConnection(_connectionString))
            {
                string sql = "Select p.pcode, p.barcode, p.pdesc, b.brand, c.category, p.price, p.qty, p.reorder from tbl_Products as p inner join tbl_Brand as b on b.id = p.bid inner join tbl_category as c on c.id = p.cid";
                if (!string.IsNullOrEmpty(searchText))
                {
                    sql += " where p.barcode like @search or p.pdesc like @search";
                }
                sql += " order by p.pdesc";

                using (var cm = new SqlCommand(sql, cn))
                {
                    if (!string.IsNullOrEmpty(searchText))
                    {
                        cm.Parameters.AddWithValue("@search", "%" + searchText + "%");
                    }
                    cn.Open();
                    using (var dr = cm.ExecuteReader())
                    {
                        while (dr.Read())
                        {
                            list.Add(new Product
                            {
                                ProductCode = dr["pcode"].ToString(),
                                Barcode = dr["barcode"].ToString(),
                                Description = dr["pdesc"].ToString(),
                                BrandName = dr["brand"].ToString(),
                                CategoryName = dr["category"].ToString(),
                                Price = Convert.ToDouble(dr["price"]),
                                Qty = Convert.ToInt32(dr["qty"]),
                                Reorder = Convert.ToInt32(dr["reorder"])
                            });
                        }
                    }
                }
            }
            return list;
        }

        public void Add(Product product)
        {
            using (var cn = new SqlConnection(_connectionString))
            {
                using (var cm = new SqlCommand("INSERT INTO tbl_Products (pcode, barcode, pdesc, bid, cid, price, reorder, qty) VALUES(@pcode, @barcode, @pdesc, @bid, @cid, @price, @reorder, 0)", cn))
                {
                    cm.Parameters.AddWithValue("@pcode", product.ProductCode);
                    cm.Parameters.AddWithValue("@barcode", product.Barcode);
                    cm.Parameters.AddWithValue("@pdesc", product.Description);
                    cm.Parameters.AddWithValue("@bid", product.BrandId);
                    cm.Parameters.AddWithValue("@cid", product.CategoryId);
                    cm.Parameters.AddWithValue("@price", product.Price);
                    cm.Parameters.AddWithValue("@reorder", product.Reorder);
                    cn.Open();
                    cm.ExecuteNonQuery();
                }
            }
        }

        public void Update(Product product)
        {
            using (var cn = new SqlConnection(_connectionString))
            {
                using (var cm = new SqlCommand("UPDATE tbl_Products SET barcode = @barcode, pdesc = @pdesc, bid = @bid, cid = @cid, price = @price, reorder = @reorder where pcode = @pcode", cn))
                {
                    cm.Parameters.AddWithValue("@pcode", product.ProductCode);
                    cm.Parameters.AddWithValue("@barcode", product.Barcode);
                    cm.Parameters.AddWithValue("@pdesc", product.Description);
                    cm.Parameters.AddWithValue("@bid", product.BrandId);
                    cm.Parameters.AddWithValue("@cid", product.CategoryId);
                    cm.Parameters.AddWithValue("@price", product.Price);
                    cm.Parameters.AddWithValue("@reorder", product.Reorder);
                    cn.Open();
                    cm.ExecuteNonQuery();
                }
            }
        }

        public void Delete(string pcode)
        {
            using (var cn = new SqlConnection(_connectionString))
            {
                using (var cm = new SqlCommand("delete from tbl_Products where pcode = @pcode", cn))
                {
                    cm.Parameters.AddWithValue("@pcode", pcode);
                    cn.Open();
                    cm.ExecuteNonQuery();
                }
            }
        }

        public bool Exists(string pcode)
        {
            using (var cn = new SqlConnection(_connectionString))
            {
                using (var cm = new SqlCommand("SELECT COUNT(*) FROM tbl_Products WHERE pcode = @pcode", cn))
                {
                    cm.Parameters.AddWithValue("@pcode", pcode);
                    cn.Open();
                    int count = Convert.ToInt32(cm.ExecuteScalar());
                    return count > 0;
                }
            }
        }

        public IEnumerable<string> GetBrandNames()
        {
            var list = new List<string>();
            using (var cn = new SqlConnection(_connectionString))
            {
                using (var cm = new SqlCommand("select brand from tbl_Brand order by brand", cn))
                {
                    cn.Open();
                    using (var dr = cm.ExecuteReader())
                    {
                        while (dr.Read())
                        {
                            list.Add(dr[0].ToString());
                        }
                    }
                }
            }
            return list;
        }

        public IEnumerable<string> GetCategoryNames()
        {
            var list = new List<string>();
            using (var cn = new SqlConnection(_connectionString))
            {
                using (var cm = new SqlCommand("select category from tbl_category order by category", cn))
                {
                    cn.Open();
                    using (var dr = cm.ExecuteReader())
                    {
                        while (dr.Read())
                        {
                            list.Add(dr[0].ToString());
                        }
                    }
                }
            }
            return list;
        }

        public int GetBrandIdByName(string brandName)
        {
            using (var cn = new SqlConnection(_connectionString))
            {
                using (var cm = new SqlCommand("Select id from tbl_Brand where brand = @brandName", cn))
                {
                    cm.Parameters.AddWithValue("@brandName", brandName);
                    cn.Open();
                    object result = cm.ExecuteScalar();
                    return result != null && result != DBNull.Value ? Convert.ToInt32(result) : 0;
                }
            }
        }

        public int GetCategoryIdByName(string categoryName)
        {
            using (var cn = new SqlConnection(_connectionString))
            {
                using (var cm = new SqlCommand("Select id from tbl_category where category = @categoryName", cn))
                {
                    cm.Parameters.AddWithValue("@categoryName", categoryName);
                    cn.Open();
                    object result = cm.ExecuteScalar();
                    return result != null && result != DBNull.Value ? Convert.ToInt32(result) : 0;
                }
            }
        }

        public IEnumerable<Product> GetCriticalProducts()
        {
            var list = new List<Product>();
            using (var cn = new SqlConnection(_connectionString))
            {
                using (var cm = new SqlCommand("select pcode, barcode, pdesc, brand, category, price, qty, reorder from ViewCriticalItems order by pdesc", cn))
                {
                    cn.Open();
                    using (var dr = cm.ExecuteReader())
                    {
                        while (dr.Read())
                        {
                            list.Add(new Product
                            {
                                ProductCode = dr["pcode"].ToString(),
                                Barcode = dr["barcode"].ToString(),
                                Description = dr["pdesc"].ToString(),
                                BrandName = dr["brand"].ToString(),
                                CategoryName = dr["category"].ToString(),
                                Price = Convert.ToDouble(dr["price"]),
                                Qty = Convert.ToInt32(dr["qty"]),
                                Reorder = Convert.ToInt32(dr["reorder"])
                            });
                        }
                    }
                }
            }
            return list;
        }

        public Product GetByBarcode(string barcode)
        {
            using (var cn = new SqlConnection(_connectionString))
            {
                string sql = "Select p.pcode, p.barcode, p.pdesc, b.brand, c.category, p.price, p.qty, p.reorder from tbl_Products as p inner join tbl_Brand as b on b.id = p.bid inner join tbl_category as c on c.id = p.cid where p.barcode = @barcode";
                using (var cm = new SqlCommand(sql, cn))
                {
                    cm.Parameters.AddWithValue("@barcode", barcode);
                    cn.Open();
                    using (var dr = cm.ExecuteReader())
                    {
                        if (dr.Read())
                        {
                            return new Product
                            {
                                ProductCode = dr["pcode"].ToString(),
                                Barcode = dr["barcode"].ToString(),
                                Description = dr["pdesc"].ToString(),
                                BrandName = dr["brand"].ToString(),
                                CategoryName = dr["category"].ToString(),
                                Price = Convert.ToDouble(dr["price"]),
                                Qty = Convert.ToInt32(dr["qty"]),
                                Reorder = Convert.ToInt32(dr["reorder"])
                            };
                        }
                    }
                }
            }
            return null;
        }
    }
}
