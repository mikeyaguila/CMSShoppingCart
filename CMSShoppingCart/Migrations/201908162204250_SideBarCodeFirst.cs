namespace CMSShoppingCart.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class SideBarCodeFirst : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Sidebar",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Body = c.String(),
                    })
                .PrimaryKey(t => t.Id);
            
        }
        
        public override void Down()
        {
            DropTable("dbo.Sidebar");
        }
    }
}
