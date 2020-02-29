namespace DashboardWebapp.Models
{
    using System;
    using System.Data.Entity;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Linq;

    public partial class DataContext : DbContext
    {
        public DataContext()
            : base("name=DataContext")
        {
        }

        public virtual DbSet<Item> Items { get; set; }
        public virtual DbSet<Item_Tag> ItemTags { get; set; }
        public virtual DbSet<Period> Periods { get; set; }
        public virtual DbSet<Person> People { get; set; }
        public virtual DbSet<RecurringTransaction> RecurringTransactions { get; set; }
        public virtual DbSet<Tag> Tags { get; set; }
        public virtual DbSet<Tracker> Trackers { get; set; }
        public virtual DbSet<Transaction> Transactions { get; set; }
        public virtual DbSet<Transaction_Tag> TransactionTags { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Item>()
                .HasMany(e => e.ItemTags)
                .WithRequired(e => e.Item)
                .WillCascadeOnDelete(true);

            modelBuilder.Entity<Period>()
                .Property(e => e.Name)
                .IsUnicode(false);

            modelBuilder.Entity<Period>()
                .HasMany(e => e.RecurringTransactions)
                .WithRequired(e => e.Period)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<Person>()
                .Property(e => e.Gender)
                .IsUnicode(false);

            modelBuilder.Entity<Person>()
                .HasMany(e => e.Items)
                .WithRequired(e => e.Person)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<Person>()
                .HasMany(e => e.Tags)
                .WithRequired(e => e.Person)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<Person>()
                .HasMany(e => e.Trackers)
                .WithRequired(e => e.Person)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<Person>()
                .HasMany(e => e.Transactions)
                .WithRequired(e => e.Person)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<Tag>()
                .Property(e => e.Name)
                .IsUnicode(false);

            modelBuilder.Entity<Tag>()
                .HasMany(e => e.Item_Tag)
                .WithRequired(e => e.Tag)
                .WillCascadeOnDelete(true);

            modelBuilder.Entity<Tag>()
                .HasMany(e => e.Transaction_Tag)
                .WithRequired(e => e.Tag)
                .WillCascadeOnDelete(true);

            modelBuilder.Entity<Transaction>()
                .HasMany(e => e.TransactionTags)
                .WithRequired(e => e.Transaction)
                .WillCascadeOnDelete(true);
        }
    }
}
