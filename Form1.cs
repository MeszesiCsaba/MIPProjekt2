using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Data.Entity;

namespace AdministratieMagazine
{
    public partial class Form1 : Form
    {
        private List<ProductGridRow> allProducts = new List<ProductGridRow>();
        private ToolStripTextBox searchBox;
        public Form1()
        {
            InitializeComponent();
            menuManageCategories.Click += menuManageCategories_Click;
            dataGridViewProducts.AutoGenerateColumns = true;
            Load += Form1_Load;
            buttonRefresh.Click += buttonRefresh_Click;
            menuAddNewProduct.Click += menuAddNewProduct_Click;
            menuAddStock.Click += menuAddStock_Click;
            dataGridViewProducts.CellDoubleClick += dataGridViewProducts_CellDoubleClick;
            menuViewSalesHistory.Click += menuViewSalesHistory_Click;
            searchBox = new ToolStripTextBox
            {
                Width = 180,
                ToolTipText = "Search by name, description or category"
            };

            var searchButton = new ToolStripButton("Search");
            var clearButton = new ToolStripButton("Clear");

            menuStripMain.Items.Add(new ToolStripLabel("Search:"));
            menuStripMain.Items.Add(searchBox);
            menuStripMain.Items.Add(searchButton);
            menuStripMain.Items.Add(clearButton);

            searchButton.Click += (sender, e) => ApplyProductFilter();

            clearButton.Click += (sender, e) =>
            {
                searchBox.Clear();
                ApplyProductFilter();
            };

            searchBox.KeyDown += (sender, e) =>
            {
                if (e.KeyCode == Keys.Enter)
                {
                    e.SuppressKeyPress = true;
                    ApplyProductFilter();
                }
            };
        }
        private void Form1_Load(object sender, System.EventArgs e)
        {
            LoadProductsAsync();
        }

        private void buttonRefresh_Click(object sender, System.EventArgs e)
        {
            LoadProductsAsync();
        }

        private void LoadProductsAsync()
        {
            Task.Run(() =>
            {
                try
                {
                    List<ProductGridRow> rows;

                    using (var database = new StoreContext())
                    {
                        var products = database.Products
                            .Include(product => product.Category)
                            .ToList();

                        rows = products.Select(product => new ProductGridRow
                        {
                            Id = product.Id,
                            Name = product.Name,
                            Description = product.Description,
                            DateAdded = product.DateAdded,
                            ExpirationDate = product.ExpirationDate,
                            Quantity = product.Quantity,
                            Category = product.Category.Name
                        }).ToList();
                    }

                    if (!IsDisposed && IsHandleCreated)
                    {
                        Invoke((System.Action)(() =>
                        {
                            allProducts = rows;
                            ApplyProductFilter();
                        }));
                    }
                }
                catch (System.Exception ex)
                {
                    if (!IsDisposed && IsHandleCreated)
                    {
                        Invoke((System.Action)(() =>
                        {
                            MessageBox.Show(
                                "Could not load products:\n" + ex.ToString(),
                                "Database error",
                                MessageBoxButtons.OK,
                                MessageBoxIcon.Error);
                        }));
                    }
                }
            });
        }

        private sealed class ProductGridRow
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public string Description { get; set; }
            public System.DateTime DateAdded { get; set; }
            public System.DateTime? ExpirationDate { get; set; }
            public int Quantity { get; set; }
            public string Category { get; set; }
        }

        private void menuManageCategories_Click(object sender, System.EventArgs e)
        {
            using (var form = new CategoryForm())
            {
                form.ShowDialog(this);
            }
        }
        private void menuAddNewProduct_Click(
            object sender,
            System.EventArgs e)
        {
            using (var form = new ProductForm())
            {
                if (form.ShowDialog(this) == DialogResult.OK)
                {
                    LoadProductsAsync();
                }
            }
        }

        private void categoriesToolStripMenuItem_Click(object sender, EventArgs e)
        {

        }
        private async void menuAddStock_Click(
    object sender,
    System.EventArgs e)
        {
            if (dataGridViewProducts.CurrentRow == null)
            {
                MessageBox.Show("Select a product first.");
                return;
            }

            int productId = System.Convert.ToInt32(
                dataGridViewProducts.CurrentRow.Cells["Id"].Value);

            string input = Microsoft.VisualBasic.Interaction.InputBox(
                "Enter the quantity to add:",
                "Add stock",
                "1");

            if (!int.TryParse(input, out int amount) || amount < 1)
            {
                MessageBox.Show("Enter a whole number greater than zero.");
                return;
            }

            try
            {
                await Task.Run(() =>
                {
                    using (var database = new StoreContext())
                    {
                        var product = database.Products.Find(productId);

                        if (product == null)
                            throw new System.Exception("The selected product was not found.");

                        product.Quantity += amount;
                        database.SaveChanges();
                    }
                });

                LoadProductsAsync();
            }
            catch (System.Exception ex)
            {
                MessageBox.Show(
                    "Could not add stock:\n" + ex.Message,
                    "Database error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }
        private async void dataGridViewProducts_CellDoubleClick(
    object sender,
    DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0)
                return;

            var row = dataGridViewProducts.Rows[e.RowIndex]
                .DataBoundItem as ProductGridRow;

            if (row == null)
                return;

            string input = Microsoft.VisualBasic.Interaction.InputBox(
                "Enter the quantity to sell:",
                "Sell product",
                "1");

            if (!int.TryParse(input, out int amount) || amount < 1)
            {
                MessageBox.Show("Enter a whole number greater than zero.");
                return;
            }

            try
            {
                await Task.Run(() =>
                {
                    int remainingQuantity;

                    using (var database = new StoreContext())
                    {
                        var product = database.Products
                            .Include(item => item.Category)
                            .FirstOrDefault(item => item.Id == row.Id);

                        if (product == null)
                            throw new System.Exception("The product was not found.");

                        if (amount > product.Quantity)
                            throw new System.Exception(
                                "There is not enough stock. Available: "
                                + product.Quantity);

                        database.SalesHistory.Add(new SalesHistory
                        {
                            ProductId = product.Id,
                            ProductName = product.Name,
                            CategoryName = product.Category.Name,
                            Quantity = amount,
                            SoldAt = System.DateTime.Now
                        });

                        product.Quantity -= amount;
                        remainingQuantity = product.Quantity;
                        database.SaveChanges();
                    }

                    // Töröljük a terméket, ha elfogyott.
                    if (remainingQuantity == 0)
                    {
                        using (var database = new StoreContext())
                        {
                            // Az értékesítési előzmény a név pillanatképeit tárolja,
                            // ezért a termék törlésekor kikapcsoljuk az FK-ellenőrzést.
                            var connection = database.Database.Connection;
                            connection.Open();

                            using (var command = connection.CreateCommand())
                            {
                                command.CommandText = "PRAGMA foreign_keys = OFF;";
                                command.ExecuteNonQuery();
                            }

                            var product = database.Products.Find(row.Id);

                            if (product != null)
                            {
                                database.Products.Remove(product);
                                database.SaveChanges();
                            }
                        }
                    }
                });

                LoadProductsAsync();
                MessageBox.Show("Sale saved.");
            }
            catch (System.Exception ex)
            {
                MessageBox.Show(
                    "Could not complete the sale:\n" + ex.Message,
                    "Sale error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }
        private async void menuViewSalesHistory_Click(
    object sender,
    System.EventArgs e)
        {
            var historyForm = new Form
            {
                Text = "Sales history",
                Width = 800,
                Height = 450,
                StartPosition = FormStartPosition.CenterParent
            };

            var grid = new DataGridView
            {
                Dock = DockStyle.Fill,
                ReadOnly = true,
                AutoGenerateColumns = true,
                AllowUserToAddRows = false,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill
            };

            historyForm.Controls.Add(grid);

            try
            {
                var rows = await Task.Run(() =>
                {
                    using (var database = new StoreContext())
                    {
                        return database.SalesHistory
                            .OrderByDescending(sale => sale.SoldAt)
                            .Select(sale => new
                            {
                                sale.Id,
                                Product = sale.ProductName,
                                Category = sale.CategoryName,
                                sale.Quantity,
                                sale.SoldAt
                            })
                            .ToList();
                    }
                });

                grid.DataSource = rows;

                if (rows.Count == 0)
                {
                    MessageBox.Show("There are no sales yet.");
                    return;
                }

                historyForm.ShowDialog(this);
            }
            catch (System.Exception ex)
            {
                MessageBox.Show(
                    "Could not load sales history:\n" + ex.Message,
                    "Database error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }
        private void ApplyProductFilter()
        {
            string searchText = searchBox.Text.Trim();

            var filteredProducts = string.IsNullOrEmpty(searchText)
                ? allProducts
                : allProducts.Where(product =>
                    product.Name.IndexOf(
                        searchText,
                        StringComparison.OrdinalIgnoreCase) >= 0 ||
                    product.Description.IndexOf(
                        searchText,
                        StringComparison.OrdinalIgnoreCase) >= 0 ||
                    product.Category.IndexOf(
                        searchText,
                        StringComparison.OrdinalIgnoreCase) >= 0)
                  .ToList();

            dataGridViewProducts.DataSource = null;
            dataGridViewProducts.DataSource = filteredProducts;
        }
    }
}
