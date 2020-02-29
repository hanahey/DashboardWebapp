namespace DashboardWebapp.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class initial : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Item",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(nullable: false, maxLength: 50),
                        Price = c.Double(nullable: false),
                        Store = c.String(nullable: false, maxLength: 50),
                        Quantity = c.Int(nullable: false),
                        Measurement = c.String(maxLength: 50),
                        PersonId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Person", t => t.PersonId)
                .Index(t => t.PersonId);
            
            CreateTable(
                "dbo.Item_Tag",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        ItemId = c.Int(nullable: false),
                        TagId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Tag", t => t.TagId)
                .ForeignKey("dbo.Item", t => t.ItemId)
                .Index(t => t.ItemId)
                .Index(t => t.TagId);

            CreateTable(
                "dbo.Tag",
                c => new
                {
                    Id = c.Int(nullable: false, identity: true),
                    Name = c.String(nullable: false, maxLength: 50, unicode: false),
                    PersonId = c.Int(nullable: false),
                })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Person", t => t.PersonId)
                .Index(t => t.PersonId);
            
            CreateTable(
                "dbo.Person",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Email = c.String(nullable: false, maxLength: 256),
                        FirstName = c.String(nullable: false, maxLength: 50),
                        LastName = c.String(nullable: false, maxLength: 50),
                        Gender = c.String(maxLength: 50, unicode: false),
                        DateOfBirth = c.DateTime(nullable: false),
                        UserId = c.String(nullable: false, maxLength: 128),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.Tracker",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(nullable: false, maxLength: 50),
                        GoalAmount = c.Double(nullable: false),
                        StartDate = c.DateTime(nullable: false),
                        GoalDate = c.DateTime(nullable: false),
                        EndDate = c.DateTime(),
                        PersonId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Person", t => t.PersonId)
                .Index(t => t.PersonId);
            
            CreateTable(
                "dbo.Transaction",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(nullable: false, maxLength: 50),
                        Amount = c.Double(nullable: false),
                        Date = c.DateTime(nullable: false),
                        Company = c.String(maxLength: 50),
                        RecurringTransactionId = c.Int(),
                        TrackerId = c.Int(),
                        PersonId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.RecurringTransaction", t => t.RecurringTransactionId)
                .ForeignKey("dbo.Tracker", t => t.TrackerId)
                .ForeignKey("dbo.Person", t => t.PersonId)
                .Index(t => t.RecurringTransactionId)
                .Index(t => t.TrackerId)
                .Index(t => t.PersonId);
            
            CreateTable(
                "dbo.RecurringTransaction",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(nullable: false, maxLength: 50),
                        StartDate = c.DateTime(nullable: false),
                        EndDate = c.DateTime(),
                        PeriodId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Period", t => t.PeriodId)
                .Index(t => t.PeriodId);
            
            CreateTable(
                "dbo.Period",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(nullable: false, maxLength: 50, unicode: false),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.Transaction_Tag",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        TransactionId = c.Int(nullable: false),
                        TagId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Transaction", t => t.TransactionId)
                .ForeignKey("dbo.Tag", t => t.TagId)
                .Index(t => t.TransactionId)
                .Index(t => t.TagId);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.Item_Tag", "ItemId", "dbo.Item");
            DropForeignKey("dbo.Transaction_Tag", "TagId", "dbo.Tag");
            DropForeignKey("dbo.Transaction", "PersonId", "dbo.Person");
            DropForeignKey("dbo.Tracker", "PersonId", "dbo.Person");
            DropForeignKey("dbo.Transaction_Tag", "TransactionId", "dbo.Transaction");
            DropForeignKey("dbo.Transaction", "TrackerId", "dbo.Tracker");
            DropForeignKey("dbo.Transaction", "RecurringTransactionId", "dbo.RecurringTransaction");
            DropForeignKey("dbo.RecurringTransaction", "PeriodId", "dbo.Period");
            DropForeignKey("dbo.Tag", "PersonId", "dbo.Person");
            DropForeignKey("dbo.Item", "PersonId", "dbo.Person");
            DropForeignKey("dbo.Item_Tag", "TagId", "dbo.Tag");
            DropIndex("dbo.Transaction_Tag", new[] { "TagId" });
            DropIndex("dbo.Transaction_Tag", new[] { "TransactionId" });
            DropIndex("dbo.RecurringTransaction", new[] { "PeriodId" });
            DropIndex("dbo.Transaction", new[] { "PersonId" });
            DropIndex("dbo.Transaction", new[] { "TrackerId" });
            DropIndex("dbo.Transaction", new[] { "RecurringTransactionId" });
            DropIndex("dbo.Tracker", new[] { "PersonId" });
            DropIndex("dbo.Tag", new[] { "Item_Id" });
            DropIndex("dbo.Tag", new[] { "PersonId" });
            DropIndex("dbo.Item_Tag", new[] { "TagId" });
            DropIndex("dbo.Item_Tag", new[] { "ItemId" });
            DropIndex("dbo.Item", new[] { "PersonId" });
            DropTable("dbo.Transaction_Tag");
            DropTable("dbo.Period");
            DropTable("dbo.RecurringTransaction");
            DropTable("dbo.Transaction");
            DropTable("dbo.Tracker");
            DropTable("dbo.Person");
            DropTable("dbo.Tag");
            DropTable("dbo.Item_Tag");
            DropTable("dbo.Item");
        }
    }
}
