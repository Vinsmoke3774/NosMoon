namespace OpenNos.DAL.EF.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class burgu : DbMigration
    {
        public override void Up()
        {
            AlterColumn("dbo.Item", "MaxCellon", c => c.Byte(nullable: false));
        }
        
        public override void Down()
        {
            AlterColumn("dbo.Item", "MaxCellon", c => c.Int(nullable: false));
        }
    }
}
