using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using FinalPOS.Domain;

namespace FinalPOS.Data
{
    public class CategoryRepository : ICategoryRepository
    {
        private readonly string _connectionString;

        public CategoryRepository(string connectionString)
        {
            _connectionString = connectionString;
        }

        public IEnumerable<Category> GetAll()
        {
            var list = new List<Category>();
            using (var cn = new SqlConnection(_connectionString))
            {
                using (var cm = new SqlCommand("select id, category from tbl_category order by category", cn))
                {
                    cn.Open();
                    using (var dr = cm.ExecuteReader())
                    {
                        while (dr.Read())
                        {
                            list.Add(new Category
                            {
                                Id = Convert.ToInt32(dr["id"]),
                                CategoryName = dr["category"].ToString()
                            });
                        }
                    }
                }
            }
            return list;
        }

        public void Add(Category category)
        {
            using (var cn = new SqlConnection(_connectionString))
            {
                using (var cm = new SqlCommand("INSERT INTO tbl_category(category) VALUES (@category)", cn))
                {
                    cm.Parameters.AddWithValue("@category", category.CategoryName);
                    cn.Open();
                    cm.ExecuteNonQuery();
                }
            }
        }

        public void Update(Category category)
        {
            using (var cn = new SqlConnection(_connectionString))
            {
                using (var cm = new SqlCommand("update tbl_category set category = @category where id = @id", cn))
                {
                    cm.Parameters.AddWithValue("@category", category.CategoryName);
                    cm.Parameters.AddWithValue("@id", category.Id);
                    cn.Open();
                    cm.ExecuteNonQuery();
                }
            }
        }

        public void Delete(int id)
        {
            using (var cn = new SqlConnection(_connectionString))
            {
                using (var cm = new SqlCommand("delete from tbl_category where id = @id", cn))
                {
                    cm.Parameters.AddWithValue("@id", id);
                    cn.Open();
                    cm.ExecuteNonQuery();
                }
            }
        }

        public bool Exists(string categoryName, int? excludeId = null)
        {
            using (var cn = new SqlConnection(_connectionString))
            {
                string sql = "SELECT COUNT(*) FROM tbl_category WHERE category = @categoryName";
                if (excludeId.HasValue)
                {
                    sql += " AND id <> @excludeId";
                }
                using (var cm = new SqlCommand(sql, cn))
                {
                    cm.Parameters.AddWithValue("@categoryName", categoryName);
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
