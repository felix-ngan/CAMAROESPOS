
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using FinalPOS.Domain;

namespace FinalPOS.Data
{
    public class BrandRepository : IBrandRepository
    {
        private readonly string _connectionString;

        public BrandRepository(string connectionString)
        {
            _connectionString = connectionString;
        }

        public IEnumerable<Brand> GetAll()
        {
            var list = new List<Brand>();
            using (var cn = new SqlConnection(_connectionString))
            {
                using (var cm = new SqlCommand("select id, brand from tbl_Brand order by brand", cn))
                {
                    cn.Open();
                    using (var dr = cm.ExecuteReader())
                    {
                        while (dr.Read())
                        {
                            list.Add(new Brand
                            {
                                Id = Convert.ToInt32(dr["id"]),
                                BrandName = dr["brand"].ToString()
                            });
                        }
                    }
                }
            }
            return list;
        }

        public void Add(Brand brand)
        {
            using (var cn = new SqlConnection(_connectionString))
            {
                using (var cm = new SqlCommand("INSERT INTO tbl_Brand(Brand) VALUES (@brand)", cn))
                {
                    cm.Parameters.AddWithValue("@brand", brand.BrandName);
                    cn.Open();
                    cm.ExecuteNonQuery();
                }
            }
        }

        public void Update(Brand brand)
        {
            using (var cn = new SqlConnection(_connectionString))
            {
                using (var cm = new SqlCommand("update tbl_Brand set brand = @brand where id = @id", cn))
                {
                    cm.Parameters.AddWithValue("@brand", brand.BrandName);
                    cm.Parameters.AddWithValue("@id", brand.Id);
                    cn.Open();
                    cm.ExecuteNonQuery();
                }
            }
        }

        public void Delete(int id)
        {
            using (var cn = new SqlConnection(_connectionString))
            {
                using (var cm = new SqlCommand("delete from tbl_Brand where id = @id", cn))
                {
                    cm.Parameters.AddWithValue("@id", id);
                    cn.Open();
                    cm.ExecuteNonQuery();
                }
            }
        }

        public bool Exists(string brandName, int? excludeId = null)
        {
            using (var cn = new SqlConnection(_connectionString))
            {
                string sql = "SELECT COUNT(*) FROM tbl_Brand WHERE brand = @brandName";
                if (excludeId.HasValue)
                {
                    sql += " AND id <> @excludeId";
                }
                using (var cm = new SqlCommand(sql, cn))
                {
                    cm.Parameters.AddWithValue("@brandName", brandName);
                    if (excludeId.HasValue)
                    {
                        cm.Parameters.AddWithValue("@excludeId", excludeId.Value);
                    }
                    cn.Open();
                    int count = Convert.ToInt32(cm.ExecuteScalar());
                    return count > 0;
                }
            }
        }
    }
}
