namespace OpenNos.DAL.EF.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class matchmaking : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Participant",
                c => new
                    {
                        ParticipantId = c.Int(nullable: false, identity: true),
                        Name = c.String(),
                        Mean = c.Double(nullable: false),
                        StandardDeviation = c.Double(nullable: false),
                        Rating = c.Double(nullable: false),
                    })
                .PrimaryKey(t => t.ParticipantId);
            
        }
        
        public override void Down()
        {
            DropTable("dbo.Participant");
        }
    }
}
