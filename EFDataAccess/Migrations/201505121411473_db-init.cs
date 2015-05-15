namespace EFDataAccess.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class dbinit : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Invoices",
                c => new
                    {
                        InvoiceId = c.Int(nullable: false, identity: true),
                        SaleDate = c.DateTime(nullable: false),
                        SalesPersonId_Id = c.Int(),
                    })
                .PrimaryKey(t => t.InvoiceId)
                .ForeignKey("dbo.SalesPersons", t => t.SalesPersonId_Id)
                .Index(t => t.SalesPersonId_Id);
            
            CreateTable(
                "dbo.Products",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(),
                        Price = c.Double(nullable: false),
                        Qty = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.SalesPersons",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.ProductInvoices",
                c => new
                    {
                        Product_Id = c.Int(nullable: false),
                        Invoice_InvoiceId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => new { t.Product_Id, t.Invoice_InvoiceId })
                .ForeignKey("dbo.Products", t => t.Product_Id, cascadeDelete: true)
                .ForeignKey("dbo.Invoices", t => t.Invoice_InvoiceId, cascadeDelete: true)
                .Index(t => t.Product_Id)
                .Index(t => t.Invoice_InvoiceId);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.Invoices", "SalesPersonId_Id", "dbo.SalesPersons");
            DropForeignKey("dbo.ProductInvoices", "Invoice_InvoiceId", "dbo.Invoices");
            DropForeignKey("dbo.ProductInvoices", "Product_Id", "dbo.Products");
            DropIndex("dbo.ProductInvoices", new[] { "Invoice_InvoiceId" });
            DropIndex("dbo.ProductInvoices", new[] { "Product_Id" });
            DropIndex("dbo.Invoices", new[] { "SalesPersonId_Id" });
            DropTable("dbo.ProductInvoices");
            DropTable("dbo.SalesPersons");
            DropTable("dbo.Products");
            DropTable("dbo.Invoices");
        }
    }
}
