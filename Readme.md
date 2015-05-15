#Installation#

1. Download or Clone this repository
1. Create an empty database with the name you desire.
1. Open visual studio and then the nuget console tool and run install packages.config. Make sure you install packages for both projects WPF-Invoice and EFDataAccess.
1. Configure the app.config connection string to your needs. Add your database name, server and credentials.
1. On the nuget console run the command update-database. This will run the migrations and seeders (Only 3 records on SalesPersons table and 3 on Products Table)
1. Compile and run the application.
