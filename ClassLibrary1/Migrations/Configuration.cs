namespace EFDataAccess.Migrations
{
    using System;
    using System.Data.Entity;
    using System.Data.Entity.Migrations;
    using System.Linq;

    internal sealed class Configuration : DbMigrationsConfiguration<EFDataAccess.DBInvoiceSample>
    {
        public Configuration()
        {
            AutomaticMigrationsEnabled = false;
        }

        protected override void Seed(EFDataAccess.DBInvoiceSample context)
        {
            //  This method will be called after migrating to the latest version.

            //  You can use the DbSet<T>.AddOrUpdate() helper extension method 
            //  to avoid creating duplicate seed data. E.g.
            //
            //    context.People.AddOrUpdate(
            //      p => p.FullName,
            //      new Person { FullName = "Andrew Peters" },
            //      new Person { FullName = "Brice Lambson" },
            //      new Person { FullName = "Rowan Miller" }
            //    );
            //
            context.SalesPersons.AddOrUpdate(s => s.Name,
                        new SalesPerson { Name="Roger" },
                        new SalesPerson { Name="Alma" },
                        new SalesPerson { Name="Lenny" }
                );

            context.Products.AddOrUpdate(
                    new Product { Name = "Wrench", Price = 45.30, Qty = 100 },
                    new Product { Name = "Pipe cutter", Price = 120.30, Qty = 200},
                    new Product { Name = "Saw", Price = 50, Qty = 50}
                );
        }
    }
}
