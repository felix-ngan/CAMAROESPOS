using System;
using System.Collections.Generic;
using System.Data.SqlClient;

namespace FinalPOS.Data
{
    internal class SaleSyncItem
    {
        public int Id { get; set; }
        public string TransNo { get; set; }
        public string ProductCode { get; set; }
        public double Price { get; set; }
        public int Qty { get; set; }
        public double DiscPer { get; set; }
        public double Disc { get; set; }
        public double Total { get; set; }
        public DateTime SDate { get; set; }
        public string Status { get; set; }
        public string Cashier { get; set; }
        public object StoreId { get; set; }
        public object RegisterId { get; set; }
    }

    internal class WalletTxSyncItem
    {
        public int Id { get; set; }
        public string Phone { get; set; }
        public string TransNo { get; set; }
        public string Type { get; set; }
        public double Amount { get; set; }
        public DateTime CreatedAt { get; set; }
        public object StoreId { get; set; }
        public object RegisterId { get; set; }
    }

    internal class UserSyncItem
    {
        public string Username { get; set; }
        public string Password { get; set; }
        public string Role { get; set; }
        public string Name { get; set; }
        public string IsActive { get; set; }
    }

    public class SyncService
    {
        private readonly string _localConnectionString;

        public SyncService(string localConnectionString)
        {
            _localConnectionString = localConnectionString;
        }

        private string GetCentralConnectionString()
        {
            try
            {
                using (var cn = new SqlConnection(_localConnectionString))
                {
                    using (var cm = new SqlCommand("select setting_value from tbl_LocalSettings where setting_key = 'CentralConnectionString'", cn))
                    {
                        cn.Open();
                        object result = cm.ExecuteScalar();
                        return result?.ToString() ?? "";
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("GetCentralConnectionString error: " + ex.Message);
                return "";
            }
        }

        public void Synchronize(Action<string> logCallback = null)
        {
            string centralConnStr = GetCentralConnectionString();
            if (string.IsNullOrWhiteSpace(centralConnStr))
            {
                logCallback?.Invoke("Chaîne de connexion centrale non configurée.");
                return;
            }

            logCallback?.Invoke("Début de la synchronisation...");

            try
            {
                // 1. Sync Sales
                int salesSynced = SyncSales(centralConnStr);
                logCallback?.Invoke($"{salesSynced} vente(s) synchronisée(s).");

                // 2. Sync Wallet Transactions
                int txsSynced = SyncWalletTransactions(centralConnStr);
                logCallback?.Invoke($"{txsSynced} transaction(s) de portefeuille synchronisée(s).");

                // 3. Pull Products
                int productsPulled = PullProducts(centralConnStr);
                logCallback?.Invoke($"{productsPulled} produit(s) synchronisé(s) depuis le serveur.");

                // 4. Pull Users
                int usersPulled = PullUsers(centralConnStr);
                logCallback?.Invoke($"{usersPulled} utilisateur(s) synchronisé(s) depuis le serveur.");

                // 5. Update last sync time
                using (var cn = new SqlConnection(_localConnectionString))
                {
                    cn.Open();
                    using (var cm = new SqlCommand(
                        "IF EXISTS (SELECT 1 FROM tbl_LocalSettings WHERE setting_key = 'LastSyncDateTime') " +
                        "UPDATE tbl_LocalSettings SET setting_value = @val WHERE setting_key = 'LastSyncDateTime' " +
                        "ELSE INSERT INTO tbl_LocalSettings (setting_key, setting_value) VALUES ('LastSyncDateTime', @val)", cn))
                    {
                        cm.Parameters.AddWithValue("@val", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                        cm.ExecuteNonQuery();
                    }
                }

                logCallback?.Invoke("Synchronisation terminée avec succès.");
            }
            catch (Exception ex)
            {
                logCallback?.Invoke($"Erreur de synchronisation : {ex.Message}");
                throw;
            }
        }

        private int SyncSales(string centralConnStr)
        {
            var salesList = new List<SaleSyncItem>();
            using (var cnLocal = new SqlConnection(_localConnectionString))
            {
                cnLocal.Open();
                using (var cmLocal = new SqlCommand("SELECT id, transno, pcode, price, qty, disc_per, disc, total, sdate, status, cashier, store_id, register_id FROM tbl_Cart WHERE status = 'Sold' AND (synced = 0 OR synced IS NULL)", cnLocal))
                {
                    using (var dr = cmLocal.ExecuteReader())
                    {
                        while (dr.Read())
                        {
                            salesList.Add(new SaleSyncItem
                            {
                                Id = Convert.ToInt32(dr["id"]),
                                TransNo = dr["transno"].ToString(),
                                ProductCode = dr["pcode"].ToString(),
                                Price = Convert.ToDouble(dr["price"]),
                                Qty = Convert.ToInt32(dr["qty"]),
                                DiscPer = dr["disc_per"] != DBNull.Value ? Convert.ToDouble(dr["disc_per"]) : 0.0,
                                Disc = dr["disc"] != DBNull.Value ? Convert.ToDouble(dr["disc"]) : 0.0,
                                Total = dr["total"] != DBNull.Value ? Convert.ToDouble(dr["total"]) : 0.0,
                                SDate = Convert.ToDateTime(dr["sdate"]),
                                Status = dr["status"].ToString(),
                                Cashier = dr["cashier"].ToString(),
                                StoreId = dr["store_id"] != DBNull.Value ? dr["store_id"] : DBNull.Value,
                                RegisterId = dr["register_id"] != DBNull.Value ? dr["register_id"] : DBNull.Value
                            });
                        }
                    }
                }
            }

            int count = 0;
            if (salesList.Count == 0) return 0;

            using (var cnCentral = new SqlConnection(centralConnStr))
            {
                cnCentral.Open();
                foreach (var item in salesList)
                {
                    using (var txCentral = cnCentral.BeginTransaction())
                    {
                        try
                        {
                            string checkQuery = "SELECT COUNT(*) FROM tbl_Cart WHERE transno = @transno AND pcode = @pcode AND (store_id = @store_id OR (store_id IS NULL AND @store_id IS NULL)) AND (register_id = @register_id OR (register_id IS NULL AND @register_id IS NULL))";
                            int exists = 0;
                            using (var cmCheck = new SqlCommand(checkQuery, cnCentral, txCentral))
                            {
                                cmCheck.Parameters.AddWithValue("@transno", item.TransNo);
                                cmCheck.Parameters.AddWithValue("@pcode", item.ProductCode);
                                cmCheck.Parameters.Add("@store_id", System.Data.SqlDbType.Int).Value = item.StoreId;
                                cmCheck.Parameters.Add("@register_id", System.Data.SqlDbType.Int).Value = item.RegisterId;
                                exists = Convert.ToInt32(cmCheck.ExecuteScalar());
                            }

                            if (exists == 0)
                            {
                                string insertQuery = "INSERT INTO tbl_Cart (transno, pcode, price, qty, disc_per, disc, total, sdate, status, cashier, store_id, register_id, synced) " +
                                                     "VALUES (@transno, @pcode, @price, @qty, @disc_per, @disc, @total, @sdate, @status, @cashier, @store_id, @register_id, 1)";
                                using (var cmInsert = new SqlCommand(insertQuery, cnCentral, txCentral))
                                {
                                    cmInsert.Parameters.AddWithValue("@transno", item.TransNo);
                                    cmInsert.Parameters.AddWithValue("@pcode", item.ProductCode);
                                    cmInsert.Parameters.AddWithValue("@price", item.Price);
                                    cmInsert.Parameters.AddWithValue("@qty", item.Qty);
                                    cmInsert.Parameters.AddWithValue("@disc_per", item.DiscPer);
                                    cmInsert.Parameters.AddWithValue("@disc", item.Disc);
                                    cmInsert.Parameters.AddWithValue("@total", item.Total);
                                    cmInsert.Parameters.AddWithValue("@sdate", item.SDate);
                                    cmInsert.Parameters.AddWithValue("@status", item.Status);
                                    cmInsert.Parameters.AddWithValue("@cashier", item.Cashier);
                                    cmInsert.Parameters.Add("@store_id", System.Data.SqlDbType.Int).Value = item.StoreId;
                                    cmInsert.Parameters.Add("@register_id", System.Data.SqlDbType.Int).Value = item.RegisterId;
                                    cmInsert.ExecuteNonQuery();
                                }
                            }

                            txCentral.Commit();

                            // Mark as synced locally
                            using (var cnLocal = new SqlConnection(_localConnectionString))
                            {
                                cnLocal.Open();
                                using (var cmUpdateLocal = new SqlCommand("UPDATE tbl_Cart SET synced = 1 WHERE id = @id", cnLocal))
                                {
                                    cmUpdateLocal.Parameters.AddWithValue("@id", item.Id);
                                    cmUpdateLocal.ExecuteNonQuery();
                                }
                            }

                            count++;
                        }
                        catch
                        {
                            txCentral.Rollback();
                            throw;
                        }
                    }
                }
            }
            return count;
        }

        private int SyncWalletTransactions(string centralConnStr)
        {
            var txList = new List<WalletTxSyncItem>();
            using (var cnLocal = new SqlConnection(_localConnectionString))
            {
                cnLocal.Open();
                using (var cmLocal = new SqlCommand("SELECT id, phone, transno, type, amount, created_at, store_id, register_id FROM tbl_WalletTransactions WHERE synced = 0 OR synced IS NULL", cnLocal))
                {
                    using (var dr = cmLocal.ExecuteReader())
                    {
                        while (dr.Read())
                        {
                            txList.Add(new WalletTxSyncItem
                            {
                                Id = Convert.ToInt32(dr["id"]),
                                Phone = dr["phone"].ToString(),
                                TransNo = dr["transno"].ToString(),
                                Type = dr["type"].ToString(),
                                Amount = Convert.ToDouble(dr["amount"]),
                                CreatedAt = Convert.ToDateTime(dr["created_at"]),
                                StoreId = dr["store_id"] != DBNull.Value ? dr["store_id"] : DBNull.Value,
                                RegisterId = dr["register_id"] != DBNull.Value ? dr["register_id"] : DBNull.Value
                            });
                        }
                    }
                }
            }

            int count = 0;
            if (txList.Count == 0) return 0;

            using (var cnCentral = new SqlConnection(centralConnStr))
            {
                cnCentral.Open();
                foreach (var item in txList)
                {
                    using (var txCentral = cnCentral.BeginTransaction())
                    {
                        try
                        {
                            // 1. Check if transaction already exists in central
                            string checkQuery = "SELECT COUNT(*) FROM tbl_WalletTransactions WHERE transno = @transno AND phone = @phone AND type = @type AND (store_id = @store_id OR (store_id IS NULL AND @store_id IS NULL)) AND (register_id = @register_id OR (register_id IS NULL AND @register_id IS NULL))";
                            int exists = 0;
                            using (var cmCheck = new SqlCommand(checkQuery, cnCentral, txCentral))
                            {
                                cmCheck.Parameters.AddWithValue("@transno", item.TransNo);
                                cmCheck.Parameters.AddWithValue("@phone", item.Phone);
                                cmCheck.Parameters.AddWithValue("@type", item.Type);
                                cmCheck.Parameters.Add("@store_id", System.Data.SqlDbType.Int).Value = item.StoreId;
                                cmCheck.Parameters.Add("@register_id", System.Data.SqlDbType.Int).Value = item.RegisterId;
                                exists = Convert.ToInt32(cmCheck.ExecuteScalar());
                            }

                            if (exists == 0)
                            {
                                // 2. Insert transaction in central
                                string insertQuery = "INSERT INTO tbl_WalletTransactions (phone, transno, type, amount, created_at, store_id, register_id, synced) " +
                                                     "VALUES (@phone, @transno, @type, @amount, @created_at, @store_id, @register_id, 1)";
                                using (var cmInsert = new SqlCommand(insertQuery, cnCentral, txCentral))
                                {
                                    cmInsert.Parameters.AddWithValue("@phone", item.Phone);
                                    cmInsert.Parameters.AddWithValue("@transno", item.TransNo);
                                    cmInsert.Parameters.AddWithValue("@type", item.Type);
                                    cmInsert.Parameters.AddWithValue("@amount", item.Amount);
                                    cmInsert.Parameters.AddWithValue("@created_at", item.CreatedAt);
                                    cmInsert.Parameters.Add("@store_id", System.Data.SqlDbType.Int).Value = item.StoreId;
                                    cmInsert.Parameters.Add("@register_id", System.Data.SqlDbType.Int).Value = item.RegisterId;
                                    cmInsert.ExecuteNonQuery();
                                }

                                // 3. Update central Customer Wallet balance
                                string checkWallet = "SELECT COUNT(*) FROM tbl_CustomerWallets WHERE phone = @phone";
                                int walletExists = 0;
                                using (var cmCheckWallet = new SqlCommand(checkWallet, cnCentral, txCentral))
                                {
                                    cmCheckWallet.Parameters.AddWithValue("@phone", item.Phone);
                                    walletExists = Convert.ToInt32(cmCheckWallet.ExecuteScalar());
                                }

                                if (walletExists == 0)
                                {
                                    using (var cmInsertWallet = new SqlCommand("INSERT INTO tbl_CustomerWallets (phone, balance) VALUES (@phone, 0)", cnCentral, txCentral))
                                    {
                                        cmInsertWallet.Parameters.AddWithValue("@phone", item.Phone);
                                        cmInsertWallet.ExecuteNonQuery();
                                    }
                                }

                                string updateWallet = "";
                                if (item.Type.Equals("Credit", StringComparison.OrdinalIgnoreCase))
                                {
                                    updateWallet = "UPDATE tbl_CustomerWallets SET balance = balance + @amount WHERE phone = @phone";
                                }
                                else if (item.Type.Equals("Debit", StringComparison.OrdinalIgnoreCase))
                                {
                                    updateWallet = "UPDATE tbl_CustomerWallets SET balance = balance - @amount WHERE phone = @phone";
                                }

                                if (!string.IsNullOrEmpty(updateWallet))
                                {
                                    using (var cmUpdateWallet = new SqlCommand(updateWallet, cnCentral, txCentral))
                                    {
                                        cmUpdateWallet.Parameters.AddWithValue("@amount", item.Amount);
                                        cmUpdateWallet.Parameters.AddWithValue("@phone", item.Phone);
                                        cmUpdateWallet.ExecuteNonQuery();
                                    }
                                }
                            }

                            txCentral.Commit();

                            // Mark as synced locally
                            using (var cnLocal = new SqlConnection(_localConnectionString))
                            {
                                cnLocal.Open();
                                using (var cmUpdateLocal = new SqlCommand("UPDATE tbl_WalletTransactions SET synced = 1 WHERE id = @id", cnLocal))
                                {
                                    cmUpdateLocal.Parameters.AddWithValue("@id", item.Id);
                                    cmUpdateLocal.ExecuteNonQuery();
                                }
                            }

                            count++;
                        }
                        catch
                        {
                            txCentral.Rollback();
                            throw;
                        }
                    }
                }
            }
            return count;
        }

        private int PullProducts(string centralConnStr)
        {
            int count = 0;
            using (var cnCentral = new SqlConnection(centralConnStr))
            {
                cnCentral.Open();
                using (var cmCentral = new SqlCommand("SELECT pcode, barcode, pdesc, bid, cid, price, qty, reorder FROM tbl_Products", cnCentral))
                {
                    using (var dr = cmCentral.ExecuteReader())
                    {
                        using (var cnLocal = new SqlConnection(_localConnectionString))
                        {
                            cnLocal.Open();
                            while (dr.Read())
                            {
                                string pcode = dr["pcode"].ToString();
                                string barcode = dr["barcode"].ToString();
                                string pdesc = dr["pdesc"].ToString();
                                object bid = dr["bid"];
                                object cid = dr["cid"];
                                double price = Convert.ToDouble(dr["price"]);
                                int qty = Convert.ToInt32(dr["qty"]);
                                int reorder = Convert.ToInt32(dr["reorder"]);

                                // Check if product exists locally
                                bool exists = false;
                                using (var cmCheck = new SqlCommand("SELECT COUNT(*) FROM tbl_Products WHERE pcode = @pcode", cnLocal))
                                {
                                    cmCheck.Parameters.AddWithValue("@pcode", pcode);
                                    exists = Convert.ToInt32(cmCheck.ExecuteScalar()) > 0;
                                }

                                if (exists)
                                {
                                    // Update details, do NOT overwrite local stock qty
                                    string updateQuery = "UPDATE tbl_Products SET barcode = @barcode, pdesc = @pdesc, bid = @bid, cid = @cid, price = @price, reorder = @reorder WHERE pcode = @pcode";
                                    using (var cmUpdate = new SqlCommand(updateQuery, cnLocal))
                                    {
                                        cmUpdate.Parameters.AddWithValue("@barcode", barcode);
                                        cmUpdate.Parameters.AddWithValue("@pdesc", pdesc);
                                        cmUpdate.Parameters.AddWithValue("@bid", bid ?? DBNull.Value);
                                        cmUpdate.Parameters.AddWithValue("@cid", cid ?? DBNull.Value);
                                        cmUpdate.Parameters.AddWithValue("@price", price);
                                        cmUpdate.Parameters.AddWithValue("@reorder", reorder);
                                        cmUpdate.Parameters.AddWithValue("@pcode", pcode);
                                        cmUpdate.ExecuteNonQuery();
                                    }
                                }
                                else
                                {
                                    // New product, insert all fields
                                    string insertQuery = "INSERT INTO tbl_Products (pcode, barcode, pdesc, bid, cid, price, qty, reorder) VALUES (@pcode, @barcode, @pdesc, @bid, @cid, @price, @qty, @reorder)";
                                    using (var cmInsert = new SqlCommand(insertQuery, cnLocal))
                                    {
                                        cmInsert.Parameters.AddWithValue("@pcode", pcode);
                                        cmInsert.Parameters.AddWithValue("@barcode", barcode);
                                        cmInsert.Parameters.AddWithValue("@pdesc", pdesc);
                                        cmInsert.Parameters.AddWithValue("@bid", bid ?? DBNull.Value);
                                        cmInsert.Parameters.AddWithValue("@cid", cid ?? DBNull.Value);
                                        cmInsert.Parameters.AddWithValue("@price", price);
                                        cmInsert.Parameters.AddWithValue("@qty", qty);
                                        cmInsert.Parameters.AddWithValue("@reorder", reorder);
                                        cmInsert.ExecuteNonQuery();
                                    }
                                }
                                count++;
                            }
                        }
                    }
                }
            }
            return count;
        }

        private int PullUsers(string centralConnStr)
        {
            var userList = new List<UserSyncItem>();
            try
            {
                using (var cnCentral = new SqlConnection(centralConnStr))
                {
                    cnCentral.Open();
                    using (var cmCentral = new SqlCommand("SELECT username, password, role, name, isactive FROM tbl_Users", cnCentral))
                    {
                        using (var dr = cmCentral.ExecuteReader())
                        {
                            while (dr.Read())
                            {
                                userList.Add(new UserSyncItem
                                {
                                    Username = dr["username"].ToString(),
                                    Password = dr["password"].ToString(),
                                    Role = dr["role"].ToString(),
                                    Name = dr["name"].ToString(),
                                    IsActive = dr["isactive"].ToString()
                                });
                            }
                        }
                    }
                }

                int count = 0;
                if (userList.Count == 0) return 0;

                using (var cnLocal = new SqlConnection(_localConnectionString))
                {
                    cnLocal.Open();
                    foreach (var user in userList)
                    {
                        bool exists = false;
                        using (var cmCheck = new SqlCommand("SELECT COUNT(*) FROM tbl_Users WHERE username = @username", cnLocal))
                        {
                            cmCheck.Parameters.AddWithValue("@username", user.Username);
                            exists = Convert.ToInt32(cmCheck.ExecuteScalar()) > 0;
                        }

                        if (exists)
                        {
                            using (var cmUpdate = new SqlCommand("UPDATE tbl_Users SET password = @password, role = @role, name = @name, isactive = @isactive WHERE username = @username", cnLocal))
                            {
                                cmUpdate.Parameters.AddWithValue("@password", user.Password);
                                cmUpdate.Parameters.AddWithValue("@role", user.Role);
                                cmUpdate.Parameters.AddWithValue("@name", user.Name);
                                cmUpdate.Parameters.AddWithValue("@isactive", user.IsActive);
                                cmUpdate.Parameters.AddWithValue("@username", user.Username);
                                cmUpdate.ExecuteNonQuery();
                            }
                        }
                        else
                        {
                            using (var cmInsert = new SqlCommand("INSERT INTO tbl_Users (username, password, role, name, isactive) VALUES (@username, @password, @role, @name, @isactive)", cnLocal))
                            {
                                cmInsert.Parameters.AddWithValue("@username", user.Username);
                                cmInsert.Parameters.AddWithValue("@password", user.Password);
                                cmInsert.Parameters.AddWithValue("@role", user.Role);
                                cmInsert.Parameters.AddWithValue("@name", user.Name);
                                cmInsert.Parameters.AddWithValue("@isactive", user.IsActive);
                                cmInsert.ExecuteNonQuery();
                            }
                        }
                        count++;
                    }
                }
                return count;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("PullUsers error: " + ex.Message);
                throw;
            }
        }
    }
}
