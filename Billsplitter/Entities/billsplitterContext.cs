using System;
using Billsplitter.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace Billsplitter.Entities
{
    public partial class billsplitterContext : DbContext
    {
//        public billsplitterContext()
//        {
//        }

        public billsplitterContext(DbContextOptions<billsplitterContext> options)
            : base(options)
        {
        }

        public virtual DbSet<ProductCategories> ProductCategories { get; set; }
        public virtual DbSet<Currencies> Currencies { get; set; }
        public virtual DbSet<Efmigrationshistory> Efmigrationshistory { get; set; }
        public virtual DbSet<Groups> Groups { get; set; }
        public virtual DbSet<GroupsUsers> GroupsUsers { get; set; }
        public virtual DbSet<HaveToBuyList> HaveToBuyList { get; set; }
        public virtual DbSet<Measures> Measures { get; set; }
        public virtual DbSet<Products> Products { get; set; }
        public virtual DbSet<PurchaseMembers> PurchaseMembers { get; set; }
        public virtual DbSet<Purchases> Purchases { get; set; }
        public virtual DbSet<RepeatingPurchases> RepeatingPurchases { get; set; }
        public virtual DbSet<Users> Users { get; set; }
        public virtual DbSet<Transactions> Transactions { get; set; }
        public DbQuery<GroupMoney> GroupMoney { get; set; }
        public DbQuery<ProductStatistics> ProductStatistics { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
//#warning To protect potentially sensitive information in your connection string, you should move it out of source code. See http://go.microsoft.com/fwlink/?LinkId=723263 for guidance on storing connection strings.
                optionsBuilder.UseMySql("server=localhost;userid=billsplitter;pwd=rvwvvCArx4Pm8PfD;port=3306;database=billsplitter;sslmode=none;");
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            
            modelBuilder.Entity<ProductCategories>(entity =>
            {
                entity.ToTable("product_categories");

                entity.Property(e => e.Id).HasColumnType("int(11)");

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasColumnType("varchar(255)");
                
                entity.Property(e => e.Color)
                    .IsRequired()
                    .HasColumnType("varchar(255)");
            });
            
            modelBuilder.Entity<Currencies>(entity =>
            {
                entity.ToTable("currencies");

                entity.Property(e => e.Id).HasColumnType("int(11)");

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasColumnType("varchar(255)");
            });

            modelBuilder.Entity<Efmigrationshistory>(entity =>
            {
                entity.HasKey(e => e.MigrationId);

                entity.ToTable("__efmigrationshistory");

                entity.Property(e => e.MigrationId).HasColumnType("varchar(95)");

                entity.Property(e => e.ProductVersion)
                    .IsRequired()
                    .HasColumnType("varchar(32)");
            });

            modelBuilder.Entity<Groups>(entity =>
            {
                entity.ToTable("groups");

                entity.HasIndex(e => e.CreatedByUserId)
                    .HasName("GroupCreatedByUserId");

                entity.HasIndex(e => e.CurrencyId)
                    .HasName("GroupCurrencyId");

                entity.Property(e => e.Id).HasColumnType("int(11)");

                entity.Property(e => e.CreatedByUserId).HasColumnType("int(11)");

                entity.Property(e => e.CurrencyId).HasColumnType("int(11)");

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasColumnType("varchar(255)");

                entity.Property(e => e.PhotoUrl)
                    .IsRequired()
                    .HasColumnType("varchar(255)");

                entity.HasOne(d => d.CreatedByUser)
                    .WithMany(p => p.Groups)
                    .HasForeignKey(d => d.CreatedByUserId)
                    .HasConstraintName("GroupCreatedByUserId");

                entity.HasOne(d => d.Currency)
                    .WithMany(p => p.Groups)
                    .HasForeignKey(d => d.CurrencyId)
                    .HasConstraintName("GroupCurrencyId");
            });

            modelBuilder.Entity<GroupsUsers>(entity =>
            {
                entity.ToTable("groups_users");

                entity.HasIndex(e => e.GroupId)
                    .HasName("GroupsUsersGroupId");

                entity.HasIndex(e => e.UserId)
                    .HasName("GroupsUsersUserId");

                entity.Property(e => e.Id).HasColumnType("int(11)");

                entity.Property(e => e.GroupId).HasColumnType("int(11)");

                entity.Property(e => e.UserId).HasColumnType("int(11)");

                entity.HasOne(d => d.Group)
                    .WithMany(p => p.GroupsUsers)
                    .HasForeignKey(d => d.GroupId)
                    .HasConstraintName("GroupsUsersGroupId");

                entity.HasOne(d => d.User)
                    .WithMany(p => p.GroupsUsers)
                    .HasForeignKey(d => d.UserId)
                    .HasConstraintName("GroupsUsersUserId");
            });
            
            modelBuilder.Entity<Transactions>(entity =>
            {
                entity.ToTable("transactions");

                entity.HasIndex(e => e.GroupId)
                    .HasName("TransactionGroupId");

                entity.HasIndex(e => e.PayerId)
                    .HasName("TransactionPayerId");
                
                entity.HasIndex(e => e.ReceiverId)
                    .HasName("TransactionReceiverId");

                entity.Property(e => e.Id).HasColumnType("int(11)");

                entity.Property(e => e.GroupId).HasColumnType("int(11)");

                entity.Property(e => e.PayerId).HasColumnType("int(11)");
                
                entity.Property(e => e.ReceiverId).HasColumnType("int(11)");

                entity.HasOne(d => d.Group)
                    .WithMany(p => p.Transactions)
                    .HasForeignKey(d => d.GroupId)
                    .HasConstraintName("TransactionGroupId");

                entity.HasOne(d => d.Payer)
                    .WithMany(p => p.Outcomes)
                    .HasForeignKey(d => d.PayerId)
                    .HasConstraintName("TransactionPayerId");
                
                entity.HasOne(d => d.Receiver)
                    .WithMany(p => p.Incomes)
                    .HasForeignKey(d => d.ReceiverId)
                    .HasConstraintName("TransactionReceiverId");
                
                entity.Property(e => e.CreatedAt)
                    .HasColumnType("timestamp")
                    .HasDefaultValueSql("'CURRENT_TIMESTAMP'");
            });

            modelBuilder.Entity<HaveToBuyList>(entity =>
            {
                entity.ToTable("have_to_buy_list");

                entity.HasIndex(e => e.AddedByUserId)
                    .HasName("HaveToBuyListAddedByUserId");

                entity.HasIndex(e => e.GroupId)
                    .HasName("HaveToBuyListGroupId");

                entity.HasIndex(e => e.ProductId)
                    .HasName("HaveToBuyListProductId");

                entity.Property(e => e.Id).HasColumnType("int(11)");

                entity.Property(e => e.AddedByUserId).HasColumnType("int(11)");

                entity.Property(e => e.Amount).HasColumnType("decimal(10,3)");

                entity.Property(e => e.Comment).HasColumnType("varchar(500)");

                entity.Property(e => e.GroupId).HasColumnType("int(11)");

                entity.Property(e => e.ProductId).HasColumnType("int(11)");

                entity.HasOne(d => d.AddedByUser)
                    .WithMany(p => p.HaveToBuyList)
                    .HasForeignKey(d => d.AddedByUserId)
                    .HasConstraintName("HaveToBuyListAddedByUserId");

                entity.HasOne(d => d.Group)
                    .WithMany(p => p.HaveToBuyList)
                    .HasForeignKey(d => d.GroupId)
                    .HasConstraintName("HaveToBuyListGroupId");

                entity.HasOne(d => d.Product)
                    .WithMany(p => p.HaveToBuyList)
                    .HasForeignKey(d => d.ProductId)
                    .HasConstraintName("HaveToBuyListProductId");
            });

            modelBuilder.Entity<Measures>(entity =>
            {
                entity.ToTable("measures");

                entity.Property(e => e.Id).HasColumnType("int(11)");

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasColumnType("varchar(255)");
            });

            modelBuilder.Entity<Products>(entity =>
            {
                entity.ToTable("products");

                entity.HasIndex(e => e.AddedByUserId)
                    .HasName("ProductsAddedByUserId");
                
                entity.HasIndex(e => e.CategoryId)
                    .HasName("ProductCategoryId");

                entity.HasIndex(e => e.GroupId)
                    .HasName("ProductGroupId");

                entity.HasIndex(e => e.MeasureId)
                    .HasName("MeasureId");

                entity.Property(e => e.Id).HasColumnType("int(11)");

                entity.Property(e => e.AddedByUserId).HasColumnType("int(11)");

                entity.Property(e => e.BarCode)
                    .IsRequired()
                    .HasColumnType("varchar(500)");

                entity.Property(e => e.CreatedAt)
                    .HasColumnType("timestamp")
                    .HasDefaultValueSql("'CURRENT_TIMESTAMP'");

                entity.Property(e => e.GroupId).HasColumnType("int(11)");

                entity.Property(e => e.MeasureId).HasColumnType("int(11)");

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasColumnType("varchar(255)");

                entity.Property(e => e.PhotoUrl)
                    .IsRequired()
                    .HasColumnType("varchar(255)");

                entity.Property(e => e.CategoryId).HasColumnType("int(11)");

                entity.HasOne(d => d.AddedByUser)
                    .WithMany(p => p.Products)
                    .HasForeignKey(d => d.AddedByUserId)
                    .HasConstraintName("ProductsAddedByUserId");
                
                entity.HasOne(d => d.Category)
                    .WithMany(p => p.Products)
                    .HasForeignKey(d => d.CategoryId)
                    .HasConstraintName("ProductCategoryId");

                entity.HasOne(d => d.Group)
                    .WithMany(p => p.Products)
                    .HasForeignKey(d => d.GroupId)
                    .HasConstraintName("ProductGroupId");

                entity.HasOne(d => d.Measure)
                    .WithMany(p => p.Products)
                    .HasForeignKey(d => d.MeasureId)
                    .HasConstraintName("MeasureId");
            });

            modelBuilder.Entity<PurchaseMembers>(entity =>
            {
                entity.ToTable("purchase_members");

                entity.HasIndex(e => e.PurchaseId)
                    .HasName("PurchaseMembersPurchaseId");

                entity.HasIndex(e => e.UserId)
                    .HasName("PurchaseMembersUserId");

                entity.Property(e => e.Id).HasColumnType("int(11)");

                entity.Property(e => e.IsPaid)
                    .HasColumnType("bool")
                    .HasDefaultValueSql("0");

                entity.Property(e => e.PurchaseId).HasColumnType("int(11)");

                entity.Property(e => e.UserId).HasColumnType("int(11)");

                entity.HasOne(d => d.Purchase)
                    .WithMany(p => p.PurchaseMembers)
                    .HasForeignKey(d => d.PurchaseId)
                    .HasConstraintName("PurchaseMembersPurchaseId");

                entity.HasOne(d => d.User)
                    .WithMany(p => p.PurchaseMembers)
                    .HasForeignKey(d => d.UserId)
                    .HasConstraintName("PurchaseMembersUserId");
            });

            modelBuilder.Entity<Purchases>(entity =>
            {
                entity.ToTable("purchases");

                entity.HasIndex(e => e.GroupId)
                    .HasName("PurchaseGroupId");

                entity.HasIndex(e => e.PaidByUserId)
                    .HasName("PurchasePaidByUserId");

                entity.HasIndex(e => e.ProductId)
                    .HasName("PurchaseProductId");

                entity.Property(e => e.Id).HasColumnType("int(11)");

//                entity.Property(e => e.Amount).HasColumnType("decimal(10,3)");

//                entity.Property(e => e.Comment).HasColumnType("varchar(500)");

                entity.Property(e => e.CreatedAt)
                    .HasColumnType("timestamp")
                    .HasDefaultValueSql("'CURRENT_TIMESTAMP'");

                entity.Property(e => e.GroupId).HasColumnType("int(11)");

                entity.Property(e => e.IsComplete)
                    .HasColumnType("boolean")
                    .HasDefaultValueSql("0");
                
                entity.Property(e => e.Show)
                    .HasColumnType("boolean")
                    .HasDefaultValueSql("1");
                
                entity.Property(e => e.Date)
                    .HasColumnType("date");

                entity.Property(e => e.PaidByUserId).HasColumnType("int(11)");

                entity.Property(e => e.Price).HasColumnType("decimal(15,2)");

                entity.Property(e => e.ProductId).HasColumnType("int(11)");

//                entity.Property(e => e.Title)
//                    .IsRequired()
//                    .HasColumnType("varchar(255)");

                entity.HasOne(d => d.Group)
                    .WithMany(p => p.Purchases)
                    .HasForeignKey(d => d.GroupId)
                    .HasConstraintName("PurchaseGroupId");

                entity.HasOne(d => d.PaidByUser)
                    .WithMany(p => p.Purchases)
                    .HasForeignKey(d => d.PaidByUserId)
                    .HasConstraintName("PurchasePaidByUserId");

                entity.HasOne(d => d.Product)
                    .WithMany(p => p.Purchases)
                    .HasForeignKey(d => d.ProductId)
                    .HasConstraintName("PurchaseProductId");
            });

            modelBuilder.Entity<RepeatingPurchases>(entity =>
            {
                entity.ToTable("repeating_purchases");

                entity.HasIndex(e => e.AddedByUserId)
                    .HasName("RepeatingPurchasesListAddedByUseriId");

                entity.HasIndex(e => e.GroupId)
                    .HasName("RepeatingPurchasesGroupId");

                entity.HasIndex(e => e.ProductId)
                    .HasName("RepeatingPurchasesProductId");

                entity.Property(e => e.Id).HasColumnType("int(11)");

                entity.Property(e => e.AddedByUserId).HasColumnType("int(11)");

                entity.Property(e => e.Amount).HasColumnType("decimal(10,3)");

                entity.Property(e => e.Comment).HasColumnType("varchar(500)");

                entity.Property(e => e.CreatedAt)
                    .HasColumnType("timestamp")
                    .HasDefaultValueSql("'CURRENT_TIMESTAMP'");

                entity.Property(e => e.GroupId).HasColumnType("int(11)");

                entity.Property(e => e.LastAddedDate).HasColumnType("date");

                entity.Property(e => e.ProductId).HasColumnType("int(11)");

                entity.Property(e => e.RepeatPeriod).HasColumnType("int(11)");

                entity.HasOne(d => d.AddedByUser)
                    .WithMany(p => p.RepeatingPurchases)
                    .HasForeignKey(d => d.AddedByUserId)
                    .HasConstraintName("RepeatingPurchasesListAddedByUseriId");

                entity.HasOne(d => d.Group)
                    .WithMany(p => p.RepeatingPurchases)
                    .HasForeignKey(d => d.GroupId)
                    .HasConstraintName("RepeatingPurchasesGroupId");

                entity.HasOne(d => d.Product)
                    .WithMany(p => p.RepeatingPurchases)
                    .HasForeignKey(d => d.ProductId)
                    .HasConstraintName("RepeatingPurchasesProductId");
            });

            modelBuilder.Entity<Users>(entity =>
            {
                entity.ToTable("users");

                entity.Property(e => e.Id).HasColumnType("int(11)");

                entity.Property(e => e.CreatedAt)
                    .HasColumnType("timestamp")
                    .HasDefaultValueSql("'CURRENT_TIMESTAMP'");

                entity.Property(e => e.Email)
                    .IsRequired()
                    .HasColumnType("varchar(255)");

                entity.Property(e => e.EmailVerificationCode).HasColumnType("varchar(255)");

                entity.Property(e => e.FullName)
                    .IsRequired()
                    .HasColumnType("varchar(255)");
                
                entity.Property(e => e.GoogleId)
                    .HasColumnType("varchar(255)");
                
                entity.Property(e => e.FacebookId)
                    .HasColumnType("varchar(255)");

                entity.Property(e => e.Password)
                    .IsRequired()
                    .HasColumnType("varchar(255)");

                entity.Property(e => e.PasswordResetCode)
                    .HasColumnType("varchar(255)")
                    .HasDefaultValueSql("NULL");

                entity.Property(e => e.PhotoUrl).HasColumnType("varchar(255)");
            });
        }
    }
}
