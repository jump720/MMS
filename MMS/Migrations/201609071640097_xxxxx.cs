namespace MMS.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class xxxxx : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.TipoGasto",
                c => new
                    {
                        TipoGastoID = c.String(nullable: false, maxLength: 3),
                        TipoGastoDesc = c.String(nullable: false, maxLength: 100),
                    })
                .PrimaryKey(t => t.TipoGastoID);
            
        }
        
        public override void Down()
        {
            DropTable("dbo.TipoGasto");
        }
    }
}
