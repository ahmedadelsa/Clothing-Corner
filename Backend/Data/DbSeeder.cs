using ShopHub.API.Models;

namespace ShopHub.API.Data
{
    public static class DbSeeder
    {
        public static void Seed(AppDbContext context)
        {
            if (!context.Categories.Any())
            {
                var categories = new List<Category>
                {
                    new Category { Name = "Electronics" },
                    new Category { Name = "Clothing" },
                    new Category { Name = "Books" }
                };

                context.Categories.AddRange(categories);
                context.SaveChanges();
            }

            if (!context.Products.Any())
            {
                var products = new List<Product>
                {
                    new Product
                    {
                        Name = "iPhone 14",
                        Description = "Apple smartphone",
                        Price = 1500,
                        StockQuantity = 10,
                        ImageUrl = "iphone.jpg",
                        CategoryId = 1
                    },
                    new Product
                    {
                        Name = "T-Shirt",
                        Description = "Cotton shirt",
                        Price = 25,
                        StockQuantity = 50,
                        ImageUrl = "shirt.jpg",
                        CategoryId = 2
                    },
                    new Product
                    {
                        Name = "C# Book",
                        Description = "Programming book",
                        Price = 40,
                        StockQuantity = 20,
                        ImageUrl = "book.jpg",
                        CategoryId = 3
                    }
                };

                context.Products.AddRange(products);
                context.SaveChanges();
            }
        }
    }
}