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
    public partial class CategoryForm : Form
    {
        public CategoryForm()
        {
            InitializeComponent();
            Load += CategoryForm_Load;
            buttonAddCategory.Click += buttonAddCategory_Click;
        }
        private async void CategoryForm_Load(object sender, System.EventArgs e)
        {
            await LoadCategoriesAsync();
        }

        private async void buttonAddCategory_Click(
            object sender,
            System.EventArgs e)
        {
            string name = textBoxCategoryName.Text.Trim();

            if (name.Length == 0)
            {
                MessageBox.Show("Enter a category name.");
                return;
            }

            try
            {
                bool added = await Task.Run(() =>
                {
                    using (var database = new StoreContext())
                    {
                        bool alreadyExists = database.Categories
                            .Any(category => category.Name == name);

                        if (alreadyExists)
                            return false;

                        database.Categories.Add(new ProductCategory
                        {
                            Name = name
                        });

                        database.SaveChanges();
                        return true;
                    }
                });

                if (!added)
                {
                    MessageBox.Show("That category already exists.");
                    return;
                }

                textBoxCategoryName.Clear();
                await LoadCategoriesAsync();
            }
            catch (System.Exception ex)
            {
                MessageBox.Show(
                    "Could not save the category:\n" + ex.Message,
                    "Database error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        private async Task LoadCategoriesAsync()
        {
            try
            {
                var names = await Task.Run(() =>
                {
                    using (var database = new StoreContext())
                    {
                        return database.Categories
                            .OrderBy(category => category.Name)
                            .Select(category => category.Name)
                            .ToList();
                    }
                });

                listBoxCategories.DataSource = null;
                listBoxCategories.DataSource = names;
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
    }
}
