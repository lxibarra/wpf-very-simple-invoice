using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EFDataAccess
{
    public class Invoice
    {
        public Invoice ()
        {
            this.SalesPerson = new SalesPerson();
            this.SaleList = new List<Product>();
        }

        public int InvoiceId { get; set; }

        public virtual SalesPerson SalesPerson { get; set; }

        public DateTime SaleDate { get; set; }

        public virtual List<Product> SaleList { get; set; } 

    }
}
