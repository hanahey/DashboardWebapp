namespace DashboardWebapp.Models
{
    using System;
    using System.Data.Entity;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Linq;

    public partial class DashboardContext : DbContext
    {
        public DashboardContext()
            : base("name=DashboardContext")
        {
        }

        public virtual DbSet<Category> Categories { get; set; }
        public virtual DbSet<Period> Periods { get; set; }
        public virtual DbSet<Person> People { get; set; }
        public virtual DbSet<RecurringTransaction> RecurringTransactions { get; set; }
        public virtual DbSet<Tracker> Trackers { get; set; }
        public virtual DbSet<Transaction> Transactions { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Category>()
                .Property(e => e.Name)
                .IsUnicode(false);

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
                .HasMany(e => e.Trackers)
                .WithRequired(e => e.Person)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<Person>()
                .HasMany(e => e.Transactions)
                .WithRequired(e => e.Person)
                .WillCascadeOnDelete(false);
        }

        public System.Data.Entity.DbSet<DashboardWebapp.Models.Item> Items { get; set; }
    }
}
