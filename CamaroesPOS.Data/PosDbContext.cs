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
        public DbSet<Expense> Expenses { get; set; }
        public DbSet<CashShift> CashShifts { get; set; }
        public DbSet<AuditEntry> AuditEntries { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                string dbPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "CamaroesPOS", "camaroes_pos.db");
                var dir = Path.GetDirectoryName(dbPath);
                if (!string.IsNullOrEmpty(dir))
                {
                    Directory.CreateDirectory(dir);
                }
                optionsBuilder.UseSqlite($"Data Source={dbPath}");
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Product>(e =>
            {
                e.HasKey(p => p.ProductCode);
                e.HasIndex(p => p.Barcode);
            });

            modelBuilder.Entity<CustomerWallet>().HasKey(w => w.Phone);
            modelBuilder.Entity<User>().HasKey(u => u.Username);
            modelBuilder.Entity<Store>().HasKey(s => s.Id);
            modelBuilder.Entity<StoreRevenue>().HasNoKey();

            modelBuilder.Entity<SoldItem>(e =>
            {
                e.HasIndex(s => s.TransNo);
                e.HasIndex(s => s.SoldDate);
            });

            modelBuilder.Entity<WalletTransaction>(e =>
            {
                e.HasIndex(w => w.Phone);
                e.HasIndex(w => w.TransNo);
            });

            modelBuilder.Entity<AuditEntry>(e =>
            {
                e.HasIndex(a => a.Timestamp);
            });
        }
    }
}
