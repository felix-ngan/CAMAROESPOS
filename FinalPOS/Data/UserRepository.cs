using System;
using System.Data;
using System.Data.SqlClient;
using FinalPOS.Domain;

namespace FinalPOS.Data
{
    public class UserRepository : IUserRepository
    {
        private readonly DBConnection _dbcon;

        public UserRepository()
        {
            _dbcon = new DBConnection();
            AutoMigrateDatabase();
        }

        private void AutoMigrateDatabase()
        {
            try
            {
                using (var cn = new SqlConnection(_dbcon.MyConnection()))
                {
                    cn.Open();
                    string query = "DECLARE @IsNull bit " +
                                   "SELECT @IsNull = is_nullable FROM sys.columns WHERE object_id = OBJECT_ID('tbl_Users') AND name = 'password' " +
                                   "IF @IsNull = 1 " +
                                   "    ALTER TABLE tbl_Users ALTER COLUMN password varchar(255) NULL " +
                                   "ELSE " +
                                   "    ALTER TABLE tbl_Users ALTER COLUMN password varchar(255) NOT NULL";
                    using (var cm = new SqlCommand(query, cn))
                    {
                        cm.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("UserRepository auto-migration error: " + ex.Message);
            }
        }

        public User Authenticate(string username, string password)
        {
            User user = null;
            using (SqlConnection cn = new SqlConnection(_dbcon.MyConnection()))
            {
                string query = "SELECT username, role, name, password, isactive FROM tbl_Users WHERE username = @username";
                using (SqlCommand cm = new SqlCommand(query, cn))
                {
                    cm.Parameters.Add("@username", SqlDbType.VarChar).Value = username;

                    cn.Open();
                    using (SqlDataReader dr = cm.ExecuteReader())
                    {
                        if (dr.Read())
                        {
                            string storedPassword = dr["password"].ToString();
                            if (PasswordHasher.VerifyPassword(password, storedPassword))
                            {
                                user = new User
                                {
                                    Username = dr["username"].ToString(),
                                    Role = dr["role"].ToString(),
                                    Name = dr["name"].ToString(),
                                    Password = storedPassword,
                                    IsActive = bool.Parse(dr["isactive"].ToString())
                                };
                            }
                        }
                    }
                }
            }

            // Auto-upgrade password if verified but not yet hashed
            if (user != null && !user.Password.StartsWith("PBKDF2$"))
            {
                try
                {
                    string newHashedPassword = PasswordHasher.HashPassword(password);
                    UpdatePassword(user.Username, newHashedPassword);
                    user.Password = newHashedPassword;
                }
                catch
                {
                    // Fail silently, login remains successful
                }
            }

            return user;
        }

        public string GetPassword(string username)
        {
            string password = "";
            using (SqlConnection cn = new SqlConnection(_dbcon.MyConnection()))
            {
                string query = "SELECT password FROM tbl_Users WHERE username = @username";
                using (SqlCommand cm = new SqlCommand(query, cn))
                {
                    cm.Parameters.Add("@username", SqlDbType.VarChar).Value = username;

                    cn.Open();
                    object result = cm.ExecuteScalar();
                    if (result != null && result != DBNull.Value)
                    {
                        password = result.ToString();
                    }
                }
            }
            return password;
        }

        public void AddUser(string username, string password, string role, string name)
        {
            string hashedPassword = PasswordHasher.HashPassword(password);
            using (SqlConnection cn = new SqlConnection(_dbcon.MyConnection()))
            {
                using (SqlCommand cm = new SqlCommand("insert into tbl_Users (username, password, role, name, isactive) values (@username, @password, @role, @name, 'True')", cn))
                {
                    cm.Parameters.AddWithValue("@username", username);
                    cm.Parameters.AddWithValue("@password", hashedPassword);
                    cm.Parameters.AddWithValue("@role", role);
                    cm.Parameters.AddWithValue("@name", name);
                    cn.Open();
                    cm.ExecuteNonQuery();
                }
            }
        }

        public void UpdatePassword(string username, string password)
        {
            string hashedPassword = password.StartsWith("PBKDF2$") ? password : PasswordHasher.HashPassword(password);
            using (SqlConnection cn = new SqlConnection(_dbcon.MyConnection()))
            {
                using (SqlCommand cm = new SqlCommand("update tbl_Users set password = @password where username = @username", cn))
                {
                    cm.Parameters.AddWithValue("@password", hashedPassword);
                    cm.Parameters.AddWithValue("@username", username);
                    cn.Open();
                    cm.ExecuteNonQuery();
                }
            }
        }

        public bool GetUserActivationStatus(string username)
        {
            using (SqlConnection cn = new SqlConnection(_dbcon.MyConnection()))
            {
                using (SqlCommand cm = new SqlCommand("select isactive from tbl_Users where username = @username", cn))
                {
                    cm.Parameters.AddWithValue("@username", username);
                    cn.Open();
                    object result = cm.ExecuteScalar();
                    if (result != null && result != DBNull.Value)
                    {
                        return bool.Parse(result.ToString());
                    }
                }
            }
            return false;
        }

        public bool UserExists(string username)
        {
            using (SqlConnection cn = new SqlConnection(_dbcon.MyConnection()))
            {
                using (SqlCommand cm = new SqlCommand("select count(*) from tbl_Users where username = @username", cn))
                {
                    cm.Parameters.AddWithValue("@username", username);
                    cn.Open();
                    int count = Convert.ToInt32(cm.ExecuteScalar());
                    return count > 0;
                }
            }
        }

        public void UpdateUserActivationStatus(string username, bool isActive)
        {
            using (SqlConnection cn = new SqlConnection(_dbcon.MyConnection()))
            {
                using (SqlCommand cm = new SqlCommand("update tbl_Users set isactive = @isactive where username = @username", cn))
                {
                    cm.Parameters.AddWithValue("@isactive", isActive.ToString());
                    cm.Parameters.AddWithValue("@username", username);
                    cn.Open();
                    cm.ExecuteNonQuery();
                }
            }
        }

        public void BackupDatabase(string backupPath)
        {
            string connStr = _dbcon.MyConnection();
            var builder = new SqlConnectionStringBuilder(connStr);
            string dbName = builder.InitialCatalog;

            using (SqlConnection cn = new SqlConnection(connStr))
            {
                string sanitizedPath = backupPath.Replace("'", "''");
                string query = $"BACKUP DATABASE [{dbName}] TO DISK = '{sanitizedPath}' WITH FORMAT, STATS = 10";
                using (SqlCommand cm = new SqlCommand(query, cn))
                {
                    cn.Open();
                    cm.ExecuteNonQuery();
                }
            }
        }
    }
}
