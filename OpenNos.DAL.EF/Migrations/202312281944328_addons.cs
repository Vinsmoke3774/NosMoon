namespace OpenNos.DAL.EF.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class addons : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Item", "ItemInstance_Id", c => c.Guid());
            AlterColumn("dbo.Item", "MaxCellon", c => c.Int(nullable: false));
            CreateIndex("dbo.Item", "ItemInstance_Id");
            AddForeignKey("dbo.Item", "ItemInstance_Id", "dbo.ItemInstance", "Id");
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.Item", "ItemInstance_Id", "dbo.ItemInstance");
            DropIndex("dbo.Item", new[] { "ItemInstance_Id" });
            AlterColumn("dbo.Item", "MaxCellon", c => c.Byte(nullable: false));
            DropColumn("dbo.Item", "ItemInstance_Id");
        }
    }
}
