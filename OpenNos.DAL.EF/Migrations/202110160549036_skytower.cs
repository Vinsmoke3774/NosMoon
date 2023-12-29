namespace OpenNos.DAL.EF.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class skytower : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.CharacterSkyTower",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        CharacterId = c.Long(nullable: false),
                        Timer = c.Byte(nullable: false),
                        Round = c.Byte(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Character", t => t.CharacterId, cascadeDelete: true)
                .Index(t => t.CharacterId);
            
            AddColumn("dbo.Character", "SkyTowerLevel", c => c.Int(nullable: false));
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.CharacterSkyTower", "CharacterId", "dbo.Character");
            DropIndex("dbo.CharacterSkyTower", new[] { "CharacterId" });
            DropColumn("dbo.Character", "SkyTowerLevel");
            DropTable("dbo.CharacterSkyTower");
        }
    }
}
