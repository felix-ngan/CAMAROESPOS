using System;
using System.Data.SqlClient;
using FinalPOS.Domain;

namespace FinalPOS.Data
{
    public class StoreRepository : IStoreRepository
    {
        private readonly string _connectionString;

        public StoreRepository(string connectionString)
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
                    // Add logo column to tbl_Store if it does not exist yet
                    string query = "IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('tbl_Store') AND name = 'logo') " +
                                   "ALTER TABLE tbl_Store ADD logo varbinary(max) NULL";
                    using (var cm = new SqlCommand(query, cn))
                    {
                        cm.ExecuteNonQuery();
                    }

                    // Add momo_code column if it does not exist
                    string queryMomo = "IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('tbl_Store') AND name = 'momo_code') " +
                                       "ALTER TABLE tbl_Store ADD momo_code varchar(50) NULL";
                    using (var cmMomo = new SqlCommand(queryMomo, cn))
                    {
                        cmMomo.ExecuteNonQuery();
                    }

                    // Add orange_code column if it does not exist
                    string queryOrange = "IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('tbl_Store') AND name = 'orange_code') " +
                                         "ALTER TABLE tbl_Store ADD orange_code varchar(50) NULL";
                    using (var cmOrange = new SqlCommand(queryOrange, cn))
                    {
                        cmOrange.ExecuteNonQuery();
                    }

                    // Create tbl_LocalSettings if it does not exist
                    string queryLocalSettings = "IF OBJECT_ID('tbl_LocalSettings', 'U') IS NULL " +
                                                "BEGIN " +
                                                "    CREATE TABLE tbl_LocalSettings ( " +
                                                "        setting_key VARCHAR(50) PRIMARY KEY, " +
                                                "        setting_value VARCHAR(MAX) NULL " +
                                                "    ) " +
                                                "END";
                    using (var cmLS = new SqlCommand(queryLocalSettings, cn))
                    {
                        cmLS.ExecuteNonQuery();
                    }

                    // Add store_id, register_id, synced to tbl_Cart if they do not exist
                    string queryCartStoreId = "IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('tbl_Cart') AND name = 'store_id') " +
                                              "ALTER TABLE tbl_Cart ADD store_id INT NULL";
                    using (var cmCS = new SqlCommand(queryCartStoreId, cn)) { cmCS.ExecuteNonQuery(); }

                    string queryCartRegisterId = "IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('tbl_Cart') AND name = 'register_id') " +
                                                 "ALTER TABLE tbl_Cart ADD register_id INT NULL";
                    using (var cmCR = new SqlCommand(queryCartRegisterId, cn)) { cmCR.ExecuteNonQuery(); }

                    string queryCartSynced = "IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('tbl_Cart') AND name = 'synced') " +
                                             "ALTER TABLE tbl_Cart ADD synced BIT NOT NULL DEFAULT 0";
                    using (var cmCSyn = new SqlCommand(queryCartSynced, cn)) { cmCSyn.ExecuteNonQuery(); }

                    // Add store_id, register_id, synced to tbl_WalletTransactions if they do not exist
                    string queryWTStoreId = "IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('tbl_WalletTransactions') AND name = 'store_id') " +
                                            "ALTER TABLE tbl_WalletTransactions ADD store_id INT NULL";
                    using (var cmWTS = new SqlCommand(queryWTStoreId, cn)) { cmWTS.ExecuteNonQuery(); }

                    string queryWTRegisterId = "IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('tbl_WalletTransactions') AND name = 'register_id') " +
                                               "ALTER TABLE tbl_WalletTransactions ADD register_id INT NULL";
                    using (var cmWTR = new SqlCommand(queryWTRegisterId, cn)) { cmWTR.ExecuteNonQuery(); }

                    string queryWTSynced = "IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('tbl_WalletTransactions') AND name = 'synced') " +
                                           "ALTER TABLE tbl_WalletTransactions ADD synced BIT NOT NULL DEFAULT 0";
                    using (var cmWTSyn = new SqlCommand(queryWTSynced, cn)) { cmWTSyn.ExecuteNonQuery(); }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Database auto-migration error: " + ex.Message);
            }
        }

        public Store GetStoreDetails()
        {
            using (var cn = new SqlConnection(_connectionString))
            {
                using (var cm = new SqlCommand("select store, address, logo, momo_code, orange_code from tbl_Store", cn))
                {
                    cn.Open();
                    using (var dr = cm.ExecuteReader())
                    {
                        if (dr.Read())
                        {
                            byte[] logoData = null;
                            if (dr["logo"] != DBNull.Value)
                            {
                                logoData = (byte[])dr["logo"];
                            }
                            return new Store
                            {
                                StoreName = dr["store"].ToString(),
                                Address = dr["address"].ToString(),
                                Logo = logoData,
                                MomoCode = dr["momo_code"] != DBNull.Value ? dr["momo_code"].ToString() : "",
                                OrangeCode = dr["orange_code"] != DBNull.Value ? dr["orange_code"].ToString() : ""
                            };
                        }
                    }
                }
            }
            return null;
        }

        public void SaveStoreDetails(Store store)
        {
            using (var cn = new SqlConnection(_connectionString))
            {
                cn.Open();
                int count = 0;
                using (var cmCount = new SqlCommand("select count(*) from tbl_Store", cn))
                {
                    count = Convert.ToInt32(cmCount.ExecuteScalar());
                }

                if (count > 0)
                {
                    using (var cmUpdate = new SqlCommand("update tbl_Store set store = @store, address = @address, logo = @logo, momo_code = @momo_code, orange_code = @orange_code", cn))
                    {
                        cmUpdate.Parameters.AddWithValue("@store", store.StoreName);
                        cmUpdate.Parameters.AddWithValue("@address", store.Address);
                        cmUpdate.Parameters.Add("@logo", System.Data.SqlDbType.VarBinary).Value = (object)store.Logo ?? DBNull.Value;
                        cmUpdate.Parameters.AddWithValue("@momo_code", (object)store.MomoCode ?? DBNull.Value);
                        cmUpdate.Parameters.AddWithValue("@orange_code", (object)store.OrangeCode ?? DBNull.Value);
                        cmUpdate.ExecuteNonQuery();
                    }
                }
                else
                {
                    using (var cmInsert = new SqlCommand("insert into tbl_Store (store, address, logo, momo_code, orange_code) values (@store, @address, @logo, @momo_code, @orange_code)", cn))
                    {
                        cmInsert.Parameters.AddWithValue("@store", store.StoreName);
                        cmInsert.Parameters.AddWithValue("@address", store.Address);
                        cmInsert.Parameters.Add("@logo", System.Data.SqlDbType.VarBinary).Value = (object)store.Logo ?? DBNull.Value;
                        cmInsert.Parameters.AddWithValue("@momo_code", (object)store.MomoCode ?? DBNull.Value);
                        cmInsert.Parameters.AddWithValue("@orange_code", (object)store.OrangeCode ?? DBNull.Value);
                        cmInsert.ExecuteNonQuery();
                    }
                }
            }
        }

        public string GetLocalSetting(string key)
        {
            try
            {
                using (var cn = new SqlConnection(_connectionString))
                {
                    using (var cm = new SqlCommand("select setting_value from tbl_LocalSettings where setting_key = @key", cn))
                    {
                        cm.Parameters.AddWithValue("@key", key);
                        cn.Open();
                        object result = cm.ExecuteScalar();
                        return result != null && result != DBNull.Value ? result.ToString() : "";
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("GetLocalSetting error: " + ex.Message);
                return "";
            }
        }

        public void SaveLocalSetting(string key, string value)
        {
            try
            {
                using (var cn = new SqlConnection(_connectionString))
                {
                    cn.Open();
                    int count = 0;
                    using (var cmCount = new SqlCommand("select count(*) from tbl_LocalSettings where setting_key = @key", cn))
                    {
                        cmCount.Parameters.AddWithValue("@key", key);
                        count = Convert.ToInt32(cmCount.ExecuteScalar());
                    }

                    if (count > 0)
                    {
                        using (var cmUpdate = new SqlCommand("update tbl_LocalSettings set setting_value = @value where setting_key = @key", cn))
                        {
                            cmUpdate.Parameters.AddWithValue("@value", (object)value ?? DBNull.Value);
                            cmUpdate.Parameters.AddWithValue("@key", key);
                            cmUpdate.ExecuteNonQuery();
                        }
                    }
                    else
                    {
                        using (var cmInsert = new SqlCommand("insert into tbl_LocalSettings (setting_key, setting_value) values (@key, @value)", cn))
                        {
                            cmInsert.Parameters.AddWithValue("@key", key);
                            cmInsert.Parameters.AddWithValue("@value", (object)value ?? DBNull.Value);
                            cmInsert.ExecuteNonQuery();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("SaveLocalSetting error: " + ex.Message);
            }
        }
    }
}
