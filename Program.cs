using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Data.SQLite;
using System.IO;

namespace AdministratieMagazine
{
    internal static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            try
            {
                AppDomain.CurrentDomain.SetData(
                    "DataDirectory",
                    Application.StartupPath);

                Directory.SetCurrentDirectory(Application.StartupPath);

                InitializeDatabaseTables();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    ex.ToString(),
                    "Database initialization error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);

                return;
            }

            Application.Run(new Form1());
        }
        private static void InitializeDatabaseTables()
        {
            string databasePath = Path.Combine(Application.StartupPath, "store.db");

            string connectionString =
                "Data Source=" + databasePath + ";Version=3;";

            using (var connection = new SQLiteConnection(connectionString))
            {
                connection.Open();

                string[] commands =
                {
            @"CREATE TABLE IF NOT EXISTS Categories
              (
                  Id INTEGER PRIMARY KEY AUTOINCREMENT,
                  Name NVARCHAR(50) NOT NULL
              );",

            @"CREATE TABLE IF NOT EXISTS Products
              (
                  Id INTEGER PRIMARY KEY AUTOINCREMENT,
                  Name NVARCHAR(30) NOT NULL,
                  Description NVARCHAR(500) NULL,
                  DateAdded DATETIME NOT NULL,
                  ExpirationDate DATETIME NULL,
                  Quantity INTEGER NOT NULL,
                  CategoryId INTEGER NOT NULL,
                  FOREIGN KEY (CategoryId) REFERENCES Categories(Id)
              );",

            @"CREATE TABLE IF NOT EXISTS SalesHistory
              (
                  Id INTEGER PRIMARY KEY AUTOINCREMENT,
                  ProductId INTEGER NOT NULL,
                  Quantity INTEGER NOT NULL,
                  SoldAt DATETIME NOT NULL,
                  FOREIGN KEY (ProductId) REFERENCES Products(Id)
              );"
        };

                foreach (string sql in commands)
                {
                    using (var command = new SQLiteCommand(sql, connection))
                    {
                        command.ExecuteNonQuery();
                    }
                }
                EnsureSalesHistoryColumn(connection, "ProductName", "NVARCHAR(30)");
                EnsureSalesHistoryColumn(connection, "CategoryName", "NVARCHAR(50)");

                using (var command = new SQLiteCommand(
                      @"UPDATE SalesHistory
                      SET ProductName = COALESCE(
                      (SELECT Name FROM Products
                      WHERE Products.Id = SalesHistory.ProductId), ''),
                      CategoryName = COALESCE(
                      (SELECT Categories.Name
                      FROM Products
                      JOIN Categories ON Categories.Id = Products.CategoryId
                      WHERE Products.Id = SalesHistory.ProductId), '')
                      WHERE ProductName IS NULL OR CategoryName IS NULL;",
                    connection))
                {
                    command.ExecuteNonQuery();
                }
            }
        }
        private static void EnsureSalesHistoryColumn(
    SQLiteConnection connection,
    string columnName,
    string dataType)
        {
            bool exists = false;

            using (var command =
                new SQLiteCommand("PRAGMA table_info(SalesHistory);", connection))
            using (var reader = command.ExecuteReader())
            {
                while (reader.Read())
                {
                    if (string.Equals(
                        reader["name"].ToString(),
                        columnName,
                        StringComparison.OrdinalIgnoreCase))
                    {
                        exists = true;
                        break;
                    }
                }
            }

            if (!exists)
            {
                string sql = "ALTER TABLE SalesHistory ADD COLUMN "
                    + columnName + " " + dataType + ";";

                using (var command = new SQLiteCommand(sql, connection))
                {
                    command.ExecuteNonQuery();
                }
            }
        }
    }
}
