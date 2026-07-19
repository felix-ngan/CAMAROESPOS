using Microsoft.EntityFrameworkCore;
using CamaroesPOS.Domain.Entities;
using System.IO;
using System;

namespace CamaroesPOS.Data
{
    public class PosDbContext : DbContext
    {
        public DbSet<Brand> Brands { get; set; }
        public DbSet<CancelledOrder> CancelledOrders { get; set; }
        public DbSet<CartItem> CartItems { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<CustomerWallet> CustomerWallets { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<SoldItem> SoldItems { get; set; }
        public DbSet<Stock> Stocks { get; set; }
        public DbSet<Store> Stores { get; set; }
        public DbSet<StoreRevenue> StoreRevenues { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<Vendor> Vendors { get; set; }
        public DbSet<WalletTransaction> WalletTransactions { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                // La base de données SQLite sera créée dans le dossier des données de l'application (AppData) ou le dossier local
                string dbPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "CamaroesPOS", "camaroes_pos.db");
                
                // S'assurer que le dossier existe
                Directory.CreateDirectory(Path.GetDirectoryName(dbPath));
                
                optionsBuilder.UseSqlite($"Data Source={dbPath}");
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            
            // Configuration de Fluent API si nécessaire
            // Par exemple, définir des clés primaires spécifiques si elles ne respectent pas la convention Id
        }
    }
}
