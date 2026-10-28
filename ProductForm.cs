using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace AdministratieMagazine
{
    public partial class ProductForm : Form
    {
        public ProductForm()
        {
            InitializeComponent();

            Load += ProductForm_Load;
            buttonSave.Click += buttonSave_Click;
        }

        private async void ProductForm_Load(object sender, System.EventArgs e)
        {
            await LoadCategoriesAsync();
        }

        private async Task LoadCategoriesAsync()
        {
            try
            {
                var categories = await Task.Run(() =>
                {
                    using (var database = new StoreContext())
                    {
                        return database.Categories
                            .OrderBy(category => category.Name)
                            .ToList();
                    }
                });

                comboBoxCategory.DataSource = categories;
                comboBoxCategory.DisplayMember = "Name";
                comboBoxCategory.ValueMember = "Id";

                buttonSave.Enabled = categories.Count > 0;

                if (categories.Count == 0)
                {
                    MessageBox.Show("Add a category before adding a product.");
                }
            }
            catch (System.Exception ex)
            {
                MessageBox.Show(
                    "Could not load categories:\n" + ex.Message,
                    "Database error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        private async void buttonSave_Click(object sender, System.EventArgs e)
        {
            string name = textBoxName.Text.Trim();
            string description = textBoxDescription.Text.Trim();

            if (name.Length == 0 || name.Length > 30)
            {
                MessageBox.Show("Enter a product name (1–30 characters).");
                return;
            }

            if (description.Length > 500)
            {
                MessageBox.Show("The description can be at most 500 characters.");
                return;
            }

            if (numericUpDownQuantity.Value < 1)
            {
                MessageBox.Show("Quantity must be at least 1.");
                return;
            }

            if (comboBoxCategory.SelectedValue == null)
            {
                MessageBox.Show("Select a category.");
                return;
            }

            int categoryId = System.Convert.ToInt32(comboBoxCategory.SelectedValue);

            try
            {
                buttonSave.Enabled = false;
                System.DateTime expirationDate = dateTimePickerExpiration.Value.Date;

                await Task.Run(() =>
                {
                    using (var database = new StoreContext())
                    {
                        var product = new Product
                        {
                            Name = name,
                            Description = description,
                            DateAdded = System.DateTime.Now,
                            ExpirationDate = expirationDate,
                            Quantity = System.Convert.ToInt32(numericUpDownQuantity.Value),
                            CategoryId = categoryId
                        };

                        database.Products.Add(product);
                        database.SaveChanges();
                    }
                });

                MessageBox.Show("Product saved.");
                DialogResult = DialogResult.OK;
                Close();
            }
            catch (System.Exception ex)
            {
                buttonSave.Enabled = true;

                MessageBox.Show(
                    "Could not save the product:\n" + ex.Message,
                    "Database error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }
    }
}
