namespace OpenNos.DAL.EF.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class bpIdRemove : DbMigration
    {
        public override void Up()
        {
            DropColumn("dbo.BattlePassItem", "AlreadyTaken");
        }
        
        public override void Down()
        {
            AddColumn("dbo.BattlePassItem", "AlreadyTaken", c => c.Boolean(nullable: false));
        }
    }
}
