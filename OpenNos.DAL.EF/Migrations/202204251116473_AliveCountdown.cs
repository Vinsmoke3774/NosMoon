namespace OpenNos.DAL.EF.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AliveCountdown : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Character", "AliveCountdown", c => c.Double(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.Character", "AliveCountdown");
        }
    }
}
