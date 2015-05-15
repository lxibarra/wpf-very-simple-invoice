namespace EFDataAccess.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class manytomanyupdate : DbMigration
    {
        public override void Up()
        {
            RenameColumn(table: "dbo.Invoices", name: "SalesPersonId_Id", newName: "SalesPerson_Id");
            RenameIndex(table: "dbo.Invoices", name: "IX_SalesPersonId_Id", newName: "IX_SalesPerson_Id");
        }
        
        public override void Down()
        {
            RenameIndex(table: "dbo.Invoices", name: "IX_SalesPerson_Id", newName: "IX_SalesPersonId_Id");
            RenameColumn(table: "dbo.Invoices", name: "SalesPerson_Id", newName: "SalesPersonId_Id");
        }
    }
}
