using Microsoft.EntityFrameworkCore;
using PMS_API_BAL.Interfaces;
using PMS_API_DAL.DataContext;
using PMS_API_DAL.Models;
using PMS_API_DAL.Models.CustomeModel;

namespace PMS_API_BAL.Services
{
    public class ProductService : IProduct
    {
        private readonly ApplicationDbContext dbcontext;
        public ProductService(ApplicationDbContext context)
        {
            dbcontext = context;
        }


        public async Task<PagedList<AddProduct>> ProductList(int pageNumber, int pageSize, SearchFilter searchFilter)
        {
            IQueryable<Product> query = dbcontext.Products.Include(r => r.Category)
                .Where(p => (p.DeletedAt == null && p.UserId == searchFilter.userId))
                .OrderBy(p => p.Id)
                .AsQueryable();

            if (!string.IsNullOrEmpty(searchFilter.searchProduct))
            {
                query = query.Where(p => p.Name.ToLower().Contains(searchFilter.searchProduct.ToLower()));
            }
            if (!string.IsNullOrEmpty(searchFilter.searchCategoryTag))
            {
                if (!string.IsNullOrEmpty(searchFilter.searchProduct))
                {
                    query = query.Where(p => p.Categorytag.ToLower().Contains(searchFilter.searchCategoryTag.ToLower()) && p.Name.ToLower().Contains(searchFilter.searchProduct.ToLower()));
                }
                else
                {
                    query = query.Where(p => p.Categorytag.ToLower().Contains(searchFilter.searchCategoryTag.ToLower()));
                }
                pageNumber = 1;
            }
            if (!string.IsNullOrEmpty(searchFilter.searchDescription))
            {
                if (!string.IsNullOrEmpty(searchFilter.searchProduct) && (!string.IsNullOrEmpty(searchFilter.searchCategoryTag)))
                {
                    query = query.Where(p => p.Description.ToLower().Contains(searchFilter.searchDescription.ToLower()) && p.Name.ToLower().Contains(searchFilter.searchProduct.ToLower()) && p.Categorytag.ToLower().Contains(searchFilter.searchCategoryTag.ToLower()));
                }
                else
                {
                    query = query.Where(p => p.Description.ToLower().Contains(searchFilter.searchDescription.ToLower()));
                }
                pageNumber = 1;
            }

            if (searchFilter.searchCategory != 0)
            {
                query = query.Where(p => p.CategoryId == searchFilter.searchCategory);

            }

            if (query.Count() >= 2)
            {
                switch (searchFilter.sortTypeProduct)
                {
                    case 1:
                        query = query.OrderBy(p => p.Name);
                        break;
                    case 2:
                        query = query.OrderByDescending(p => p.Name);
                        break;
                    case 3:
                        query = query.OrderBy(p => p.Price);
                        break;
                    case 4:
                        query = query.OrderByDescending(p => p.Price);
                        break;
                    case 5:
                        query = query.OrderBy(p => p.Description);
                        break;
                    case 6:
                        query = query.OrderByDescending(p => p.Description);
                        break;
                    case 7:
                        query = query.OrderBy(p => p.CreatedAt);
                        break;
                    case 8:
                        query = query.OrderByDescending(p => p.CreatedAt);
                        break;
                    case 9:
                        query = query.OrderBy(p => p.ModifiedAt);
                        break;
                    case 10:
                        query = query.OrderByDescending(p => p.ModifiedAt);
                        break;
                    case 11:
                        query = query.OrderBy(p => p.Categorytag);
                        break;
                    case 12:
                        query = query.OrderByDescending(p => p.Categorytag);
                        break;
                    case 13:
                        query = query.OrderBy(p => p.Category.Name);
                        break;
                    case 14:
                        query = query.OrderByDescending(p => p.Category.Name);
                        break;
                    default:
                        break;
                }   
            }

            int totalCount = query.Count();

            List<AddProduct> productsList = await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(p => new AddProduct
                {
                    ProductId = p.Id,
                    CategoryId = p.CategoryId,
                    CreatedDate = p.CreatedAt,
                    ModiFiedDate = p.ModifiedAt,
                    ProductName = p.Name,
                    CategoryTag = p.Categorytag,
                    CategoryName = p.Category.Name,
                    Description = p.Description,
                    Price = p.Price
                })
                .ToListAsync();

            return new PagedList<AddProduct>(productsList, totalCount, pageNumber, pageSize);
        }

        public async Task<List<UserActivity>> UserActivityList(int userId)
        {
            var activityList = await dbcontext.UserActivities.Where(u => u.UserId == userId).OrderByDescending(u => u.CreatedAt).ToListAsync();
            return activityList;
        }

        public async Task<int> TotalProducts(SearchFilter searchFilter)
        {
            int totalProducts = await dbcontext.Products.Where(p => p.DeletedAt == null && p.UserId == searchFilter.userId).CountAsync();

            return totalProducts;
        }

        public async Task<AddProduct> AddProductView(int userId)
        {
            List<Category> categoryList = await dbcontext.Categories
               .Where(c => (c.DeletedAt == null && (c.UserId == userId || (c.UserId == null && c.IsSystem == true))))
               .OrderBy(c => c.Id)
               .ToListAsync();
            AddProduct addProduct = new AddProduct()
            {
                categories = categoryList
            };
            return addProduct;
        }

        public async Task<bool> CheckProductInDb(string productName, int categoryId, int userId)
        {
            Product? product = await dbcontext.Products.FirstOrDefaultAsync(p => p.Name.ToLower() == productName.ToLower() && p.CategoryId == categoryId && p.UserId == userId);
            return product != null ? true : false;
        }

        public async Task<EditProduct> EditProductView()
        {
            EditProduct addProduct = new EditProduct()
            {
                CategoryList = await dbcontext.Categories.Where(c => c.DeletedAt == null).ToListAsync(),
            };
            return addProduct;
        }

        public async Task AddProductInDb(AddProductDTO addProduct)
        {
            if (addProduct != null)
            {
                string fileName = "";
                string filePath = "";
                fileName = $"{Guid.NewGuid().ToString()}_{addProduct.Fileupload.FileName}";
                filePath = Path.Combine(Directory.GetCurrentDirectory(), "UploadDocuments", fileName);

                string folderPath = Path.Combine(Directory.GetCurrentDirectory(), "UploadDocuments");
                if (!Directory.Exists(folderPath))
                {
                    Directory.CreateDirectory(folderPath);
                }

                using (FileStream stream = new FileStream(filePath, FileMode.Create))
                {
                    addProduct.Fileupload.CopyTo(stream);
                }

                Product product = new Product
                {
                    Name = addProduct.ProductName.Trim(),
                    CategoryId = (int)addProduct.CategoryId,
                    CreatedAt = DateTime.Now,
                    Categorytag = addProduct.CategoryTag,
                    Filename = fileName,
                    Price = addProduct.Price,
                    Description = addProduct.Description.Trim(),
                    UserId = addProduct.userId
                };
                await dbcontext.Products.AddAsync(product);
                await dbcontext.SaveChangesAsync();

            }

        }

        public async Task<EditProduct> GetProduct(int id, int userId)
        {
            Product? product = await dbcontext.Products.FirstOrDefaultAsync(p => p.Id == id);
            if (product == null)
            {
                // Handle the case when the product is not found, e.g., return null or throw an exception.
                return null;
            }

            List<Category> categoryList = await dbcontext.Categories
               .Where(c => (c.DeletedAt == null && (c.UserId == userId || (c.UserId == null && c.IsSystem == true))))
               .OrderBy(c => c.Id)
               .ToListAsync();

            List<CategoryList> categoryList1 = new List<CategoryList>();
            foreach (Category category in categoryList)
            {
                categoryList1.Add(new CategoryList
                {
                    Id = category.Id,
                    Name = category.Name
                });
            }

            EditProduct addProduct = new EditProduct()
            {
                ProductId = product.Id,
                ProductName = product.Name,
                categories = categoryList1,
                ModiFiedDate = DateTime.Now,
                CategoryTag = product.Categorytag,
                FileName = product.Filename,
                Description = product.Description,
                Price = product.Price,
                CategoryId = product.CategoryId,
                CategoryName = (await dbcontext.Categories.FirstOrDefaultAsync(c => c.Id == product.CategoryId))?.Name
            };

            return addProduct;
        }

        public async Task<bool> CheckProductEditName(int productId, EditProductDTO editProduct)
        {
            // Check if the new name exists in the same category for the same user
            Product? product = await dbcontext.Products.FirstOrDefaultAsync(p => p.Id == productId);
            if (product == null)
            {
                return false; // or handle appropriately
            }

            if (editProduct.ProductName.ToLower() != product.Name.ToLower() || editProduct.CategoryId != product.CategoryId)
            {
                return await CheckProductNameExist(editProduct.ProductName, (int)editProduct.CategoryId, (int)editProduct.userId);
            }
            return false;
        }

        public async Task<bool> CheckProductNameExist(string productName, int categoryId, int userId)
        {
            Product? product = await dbcontext.Products.FirstOrDefaultAsync(c => c.Name.ToLower() == productName.ToLower() && c.CategoryId == categoryId && c.UserId == userId);
            return product != null;
        }

        public async Task EditProduct(int productId, EditProductDTO editProduct)
        {
            Product? product = await dbcontext.Products.FirstOrDefaultAsync(p => p.Id == productId);

            if (editProduct.Fileupload != null)
            {
                if (product.Filename != null)
                {
                    string fileNameMain = product.Filename;
                    string filePath1Main = Path.Combine(Directory.GetCurrentDirectory(), "UploadDocuments", fileNameMain);

                    if (File.Exists(filePath1Main))
                    {
                        File.Delete(filePath1Main);
                    }

                    product.Filename = null;
                    dbcontext.Products.Update(product);
                    await dbcontext.SaveChangesAsync();
                }
                string fileName = "";
                string filePath = "";
                fileName = $"{Guid.NewGuid().ToString()}_{editProduct.Fileupload.FileName}";
                filePath = Path.Combine(Directory.GetCurrentDirectory(), "UploadDocuments", fileName);

                string folderPath = Path.Combine(Directory.GetCurrentDirectory(), "UploadDocuments");
                if (!Directory.Exists(folderPath))
                {
                    Directory.CreateDirectory(folderPath);
                }

                using (FileStream stream = new FileStream(filePath, FileMode.Create))
                {
                    editProduct.Fileupload.CopyTo(stream);
                }

                if (product != null)
                {
                    product.Name = editProduct.ProductName.Trim();
                    product.CategoryId = (int)editProduct.CategoryId;
                    product.ModifiedAt = DateTime.Now;
                    product.Categorytag = editProduct.CategoryTag;
                    product.Filename = fileName;
                    product.Price = editProduct.Price;
                    product.Description = editProduct.Description.Trim();
                }
                dbcontext.Products.Update(product);
                await dbcontext.SaveChangesAsync();
            }
            else
            {
                if (product != null)
                {
                    product.Name = editProduct.ProductName.Trim();
                    product.CategoryId = (int)editProduct.CategoryId;
                    product.ModifiedAt = DateTime.Now;
                    product.Categorytag = editProduct.CategoryTag;
                    product.Filename = product.Filename;
                    product.Price = editProduct.Price;
                    product.Description = editProduct.Description.Trim();

                    dbcontext.Products.Update(product);
                    await dbcontext.SaveChangesAsync();
                }
            }
        }

        public async Task DeleteProduct(int productId)
        {
            Product? product = await dbcontext.Products.FirstOrDefaultAsync(p => p.Id == productId);

            if (product != null)
            {
                product.DeletedAt = DateTime.Now;
                product.ModifiedAt = DateTime.Now;
                dbcontext.Products.Update(product);
                await dbcontext.SaveChangesAsync();
            }
        }


        public async Task<string> ProductName(int productId)
        {
            Product? product = await dbcontext.Products.FirstOrDefaultAsync(c => c.Id == productId);
            string name = product.Name;
            return name;
        }

        public async Task<int> ProductUserid(int productId)
        {
            Product? product = await dbcontext.Products.FirstOrDefaultAsync(c => c.Id == productId);
            int userId = (int)product.UserId;
            return userId;
        }

        public async Task DeleteProductImage(int productId)
        {
            Product? product = await dbcontext.Products.FirstOrDefaultAsync(p => p.Id == productId);
            if (product.Filename != null)
            {
                product.Filename = null;
                dbcontext.Products.Update(product);
                await dbcontext.SaveChangesAsync();
            }
        }

        public async Task<TotalCount> totalCount(int userId)
        {
            List<Category> categoryList = await dbcontext.Categories
               .Where(c => (c.DeletedAt == null && (c.UserId == userId || (c.UserId == null && c.IsSystem == true))))
               .OrderBy(c => c.Id)
               .ToListAsync();
            TotalCount totalCount = new TotalCount()
            {
                totalCategories = categoryList.Count(),
                totalProducts = await dbcontext.Products.Where(p => (p.DeletedAt == null && p.UserId == userId)).CountAsync()
            };
            return totalCount;
        }

    }
}