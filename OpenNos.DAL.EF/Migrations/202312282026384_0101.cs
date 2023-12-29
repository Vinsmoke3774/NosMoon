namespace OpenNos.DAL.EF.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class _0101 : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.ItemInstance", "ItemVNum", "dbo.Item");
            AddColumn("dbo.ItemInstance", "Item_VNum", c => c.Short());
            CreateIndex("dbo.ItemInstance", "Item_VNum");
            AddForeignKey("dbo.ItemInstance", "ItemVNum", "dbo.Item", "VNum", cascadeDelete: true);
            AddForeignKey("dbo.ItemInstance", "Item_VNum", "dbo.Item", "VNum");
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.ItemInstance", "Item_VNum", "dbo.Item");
            DropForeignKey("dbo.ItemInstance", "ItemVNum", "dbo.Item");
            DropIndex("dbo.ItemInstance", new[] { "Item_VNum" });
            DropColumn("dbo.ItemInstance", "Item_VNum");
            AddForeignKey("dbo.ItemInstance", "ItemVNum", "dbo.Item", "VNum");
        }
    }
}
