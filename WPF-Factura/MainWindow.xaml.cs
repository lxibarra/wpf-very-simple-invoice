using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using EFDataAccess;

namespace WPF_Invoice
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        //Temp varible to hold the last found item
        private Product tmpProduct = null;
        //Array of Cart items 
        private List<Product> ShoppingCart;  
        public MainWindow()
        {
            InitializeComponent();
            //on the constructor of the class we create a new instance of the shooping cart
            ShoppingCart = new List<Product>();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            //Populates the combobox when the window loads
            DBInvoiceSample db = new DBInvoiceSample();
            cbSalesPersonel.ItemsSource = db.SalesPersons.ToList();
            cbSalesPersonel.DisplayMemberPath = "Name";
            cbSalesPersonel.SelectedValuePath = "Id";
            cbSalesPersonel.SelectedIndex = 0;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            
            //we first check if a product has been selected
            if (tmpProduct == null)
            {   
                //if not we call the search button method
                Button_Click_1(null, null);
                //we check again if the product was found
                if (tmpProduct == null)
                {
                    //if tmpProduct is empty (Product not found) we exit the procedure
                    MessageBox.Show("No product was selected", "No product", MessageBoxButton.OK,
                        MessageBoxImage.Exclamation);
                    //exit procedure
                    return;   
                }
            }

            

            //product quantity
            int qty;

            // we try to parse the number of the textbox if the number is invalid 
            int.TryParse(txtQty.Text, out qty);
            //if qty is 0 we assign 0 otherwise we assign the actual parsed value
            qty = qty == 0 ? 1 : qty;
            //really basic validation that checks inventory
            if (qty <= tmpProduct.Qty)
            {
                //we check if product is not already in the cart if it is we remove the old one
                ShoppingCart.RemoveAll(s => s.Id == tmpProduct.Id);
                //we add the product to the Cart
                ShoppingCart.Add(new Product()
                {
                    Id = tmpProduct.Id,
                    Name = tmpProduct.Name,
                    Price = tmpProduct.Price,
                    Qty = qty
                });
                
                //perform  query on Shopping Cart to select certain fields and perform subtotal operation 
                BindDataGrid();
                //<----------------------
                //cleanup variables
                tmpProduct = null;
                //once the products had been added we clear the textbox of code and quantity.
                TxtProdCode.Text = string.Empty;
                txtQty.Text = string.Empty;
                //clean up current product label
                cprod.Content = "Current product N/A";
            }
            else
            {
                MessageBox.Show("Not enough Inventory", "Inventory Error", MessageBoxButton.OK,
                    MessageBoxImage.Exclamation);
            }

        }

        //Adds the Shopping cart data to the grid
        private void BindDataGrid()
        {
            //we query the array cart and add a new calculated field Subtotal
            var cartItems = from s in ShoppingCart
                select new
                {
                    s.Id,
                    s.Name,
                    s.Qty,
                    s.Price,
                    SubTotal = s.Qty*s.Price
                };

            //refresh dataGridview-----------
            CartGrid.ItemsSource = null;
            CartGrid.ItemsSource = cartItems;

            //we add the total with sum(price) and apply a currency formating.
            lbtotal.Content = string.Format("Total: {0}", ShoppingCart.Sum(x => x.Price * x.Qty).ToString("C"));
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            //If a product code is not empty we search the database
            if (Regex.IsMatch(TxtProdCode.Text.Trim(), @"^\d+$"))
            {
                DBInvoiceSample db = new DBInvoiceSample();
                //parse the product code as int from the TextBox
                int id = int.Parse(TxtProdCode.Text);
                //We query the database for the product
                Product p = db.Products.SingleOrDefault(x => x.Id == id);
                if (p != null) //if product was found
                {
                    //store in a temp variable (if user clicks on add we will need this for the Array)
                    tmpProduct = p;
                    //We display the product information on a label 
                    cprod.Content = string.Format("ID: {0}, Name: {1}, Price: {2}, InStock (Qty): {3}", p.Id, p.Name, p.Price, p.Qty);
                }
                else
                {
                    //if product was not found we display a user notification window
                    MessageBox.Show("Product not found. (Only numbers allowed)", "Product code error", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                }
            }
        }

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            //we make sure there is at least one item in the cart and a sales person has been selected
            if (ShoppingCart.Count > 0 && cbSalesPersonel.SelectedIndex > -1)
            {
                //auto dispose after no longer in scope
                using (DBInvoiceSample db = new DBInvoiceSample())
                {
                    //All database transactions are considered 1 unit of work
                    using (var dbTransaction = db.Database.BeginTransaction())
                    {
                        try
                        {
                            //we create the invoice object
                            Invoice inv = new Invoice();
                            inv.SaleDate = DateTime.Now;
                            //assign sales person by querying the database using the Combobox selection
                            inv.SalesPerson =
                                db.SalesPersons.SingleOrDefault(s => s.Id == (int) cbSalesPersonel.SelectedValue);

                            //for each product in the shopping cart we query the database
                            foreach (var prod in ShoppingCart)
                            {
                                //get product record with id
                                Product p = db.Products.SingleOrDefault(i => i.Id == prod.Id);
                                //reduce inventory
                                int RemainingItems = p.Qty - prod.Qty >= 0 ? (p.Qty - prod.Qty) : p.Qty;
                                if (p.Qty == RemainingItems)
                                {
                                    MessageBox.Show(
                                        string.Format(
                                            "Unable to sell Product #{0} not enough inventory, Do want to continue?",
                                            p.Id),
                                        "Not Enough Inventory", MessageBoxButton.OK, MessageBoxImage.Asterisk); 
                                   
                                        //end transaction
                                        dbTransaction.Rollback();
                                        //exit procedure
                                        return;
                                }
                                else
                                {
                                    //If Qty is ok we sell the product
                                    p.Qty = RemainingItems;
                                    inv.SaleList.Add(p);
                                }
                                
                            }

                            //we add the generated invoice to the Invoice Entity (Table)
                            db.Invoices.Add(inv);
                            //Save Changed to the database
                            db.SaveChanges();
                            //Make the changes permanent 
                            dbTransaction.Commit();
                            //We restore the form with defaults
                            CleanUp();
                            //Show confirmation message to the user
                            MessageBox.Show(string.Format("Transaction #{0}  Saved", inv.InvoiceId), "Success", MessageBoxButton.OK,
                                MessageBoxImage.Information);
                        }
                        catch
                        {
                            //if an error is produced, we rollback everything
                            dbTransaction.Rollback();
                            //We notify the user of the error
                            MessageBox.Show("Transaction Error, unable to generate invoice", "Fatal Error", MessageBoxButton.OK,
                                MessageBoxImage.Error);
                        }
                    }
                }
            }
            else
            {
                MessageBox.Show("Please select at least one product and a Sales Person", "Data Error",
                    MessageBoxButton.OK, MessageBoxImage.Stop);
            }
        }

        //this method will clear/reset form values
        private void CleanUp()
        {
            //shopping cart = a new empty list
            ShoppingCart = new List<Product>();
            //Textboxes and labels are set to defaults
            TxtProdCode.Text = string.Empty;
            txtQty.Text = string.Empty;
            lbtotal.Content = "Total: $0.00";
            //DataGrid items are set to null
            CartGrid.ItemsSource = null;
            CartGrid.Items.Refresh();
            //Tmp variable is erased using null
            tmpProduct = null;

        }


        //fires on Grid item click (Button delete)
        private void ButtonBase_OnClick(object sender, RoutedEventArgs e)
        {
            //We ask the user if really wants to delete the item
            if (
                MessageBox.Show("Are you sure you want to remove this product from Cart", "Confirmation",
                    MessageBoxButton.OKCancel, MessageBoxImage.Question) == MessageBoxResult.OK)
            {
                //if Result is OK we get the Button that was click
                Button deleteButton = (Button) sender;
                //We get the record id binded using the commandParameter attribute {Binding Id}
                int id = (int) deleteButton.CommandParameter;
                //Remove the product from the Array
                ShoppingCart.RemoveAll(s => s.Id == id);
                //Update the DataGrid
                BindDataGrid(); 
            }
        }

        private void Button_Click_3(object sender, RoutedEventArgs e)
        {
            CleanUp();
        }
    }
}
