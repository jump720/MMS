namespace MMS.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class InitialCreate : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Canal",
                c => new
                    {
                        CanalID = c.String(nullable: false, maxLength: 3),
                        CanalDesc = c.String(nullable: false, maxLength: 100),
                    })
                .PrimaryKey(t => t.CanalID);
            
        }
        
        public override void Down()
        {
            DropTable("dbo.Canal");
        }
    }
}
