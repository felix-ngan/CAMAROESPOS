using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using FinalPOS.Domain;

namespace FinalPOS.Data
{
    public class VendorRepository : IVendorRepository
    {
        private readonly string _connectionString;

        public VendorRepository(string connectionString)
        {
            _connectionString = connectionString;
        }

        public IEnumerable<Vendor> GetAll()
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

        public void Add(Vendor vendor)
        {
            using (var cn = new SqlConnection(_connectionString))
            {
                using (var cm = new SqlCommand("insert into tbl_Vendor (vendor, address, contactperson, telephone, email) VALUES (@vendor, @address, @contactperson, @telephone, @email)", cn))
                {
                    cm.Parameters.AddWithValue("@vendor", vendor.VendorName);
                    cm.Parameters.AddWithValue("@address", vendor.Address);
                    cm.Parameters.AddWithValue("@contactperson", vendor.ContactPerson);
                    cm.Parameters.AddWithValue("@telephone", vendor.Phone);
                    cm.Parameters.AddWithValue("@email", vendor.Email);
                    cn.Open();
                    cm.ExecuteNonQuery();
                }
            }
        }

        public void Update(Vendor vendor)
        {
            using (var cn = new SqlConnection(_connectionString))
            {
                // Fixed SQL bug by adding WHERE id = @id to update single record
                using (var cm = new SqlCommand("update tbl_Vendor set vendor = @vendor , address =@address , contactperson = @contactperson, telephone = @telephone, email = @email where id = @id", cn))
                {
                    cm.Parameters.AddWithValue("@vendor", vendor.VendorName);
                    cm.Parameters.AddWithValue("@address", vendor.Address);
                    cm.Parameters.AddWithValue("@contactperson", vendor.ContactPerson);
                    cm.Parameters.AddWithValue("@telephone", vendor.Phone);
                    cm.Parameters.AddWithValue("@email", vendor.Email);
                    cm.Parameters.AddWithValue("@id", vendor.Id);
                    cn.Open();
                    cm.ExecuteNonQuery();
                }
            }
        }

        public void Delete(int id)
        {
            using (var cn = new SqlConnection(_connectionString))
            {
                using (var cm = new SqlCommand("delete from tbl_Vendor where id = @id", cn))
                {
                    cm.Parameters.AddWithValue("@id", id);
                    cn.Open();
                    cm.ExecuteNonQuery();
                }
            }
        }

        public bool Exists(string vendorName, int? excludeId = null)
        {
            using (var cn = new SqlConnection(_connectionString))
            {
                string sql = "SELECT COUNT(*) FROM tbl_Vendor WHERE vendor = @vendorName";
                if (excludeId.HasValue)
                {
                    sql += " AND id <> @excludeId";
                }
                using (var cm = new SqlCommand(sql, cn))
                {
                    cm.Parameters.AddWithValue("@vendorName", vendorName);
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
