namespace DashboardWebapp.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class cascadeDelete : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.Item_Tag", "ItemId", "dbo.Item");
            DropForeignKey("dbo.Item_Tag", "TagId", "dbo.Tag");
            DropForeignKey("dbo.Transaction_Tag", "TagId", "dbo.Tag");
            DropForeignKey("dbo.Transaction_Tag", "TransactionId", "dbo.Transaction");
            AddForeignKey("dbo.Item_Tag", "ItemId", "dbo.Item", "Id", cascadeDelete: true);
            AddForeignKey("dbo.Item_Tag", "TagId", "dbo.Tag", "Id", cascadeDelete: true);
            AddForeignKey("dbo.Transaction_Tag", "TagId", "dbo.Tag", "Id", cascadeDelete: true);
            AddForeignKey("dbo.Transaction_Tag", "TransactionId", "dbo.Transaction", "Id", cascadeDelete: true);
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.Transaction_Tag", "TransactionId", "dbo.Transaction");
            DropForeignKey("dbo.Transaction_Tag", "TagId", "dbo.Tag");
            DropForeignKey("dbo.Item_Tag", "TagId", "dbo.Tag");
            DropForeignKey("dbo.Item_Tag", "ItemId", "dbo.Item");
            AddForeignKey("dbo.Transaction_Tag", "TransactionId", "dbo.Transaction", "Id");
            AddForeignKey("dbo.Transaction_Tag", "TagId", "dbo.Tag", "Id");
            AddForeignKey("dbo.Item_Tag", "TagId", "dbo.Tag", "Id");
            AddForeignKey("dbo.Item_Tag", "ItemId", "dbo.Item", "Id");
        }
    }
}
