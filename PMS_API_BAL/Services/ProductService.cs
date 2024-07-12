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
        private readonly IRepository<Product> repo;

        public ProductService(ApplicationDbContext context, IRepository<Product> _repo)
        {
            dbcontext = context;
            repo = _repo;
        }

        public async Task<PagedList<AddProduct>> ProductList(int pageNumber, int pageSize, SearchFilter searchFilter)
        {
            IQueryable<Product> query = dbcontext.Products.Include(r => r.Category)
                .Where(p => (!p.DeletedAt.HasValue && p.UserId == searchFilter.userId))
                .OrderBy(p => p.Id)
                .AsQueryable();

            if (!string.IsNullOrEmpty(searchFilter.searchProduct))
            {
                query = query.Where(p => p.Name.ToLower().Contains(searchFilter.searchProduct.ToLower()));
            }

            if (!string.IsNullOrEmpty(searchFilter.searchCategoryTag))
            {
                query = query.Where(c => c.Categorytag.ToLower().Trim().Contains(searchFilter.searchCategoryTag.ToLower()) &&
                (string.IsNullOrEmpty(searchFilter.searchProduct) || c.Name.ToLower().Trim().Contains(searchFilter.searchProduct.ToLower())));

                pageNumber = 1;
            }

            if (!string.IsNullOrEmpty(searchFilter.searchDescription))
            {
                query = query.Where(c => c.Description.ToLower().Trim().Contains(searchFilter.searchDescription.ToLower()) &&
                (string.IsNullOrEmpty(searchFilter.searchProduct) || c.Name.ToLower().Trim().Contains(searchFilter.searchProduct.ToLower()) &&
                (string.IsNullOrEmpty(searchFilter.searchCategoryTag) || c.Categorytag.ToLower().Trim().Contains(searchFilter.searchCategoryTag.ToLower()))));

                pageNumber = 1;
            }

            if (searchFilter.searchCategory != 0)
            {
                query = query.Where(p => p.CategoryId == searchFilter.searchCategory);

            }

            if (query.Count() >= 2 && searchFilter.sortTypeProduct != 0)
            {
                query = (ProductSortType)searchFilter.sortTypeProduct switch
                {
                    ProductSortType.NameAsc => query.OrderBy(p => p.Name),
                    ProductSortType.NameDesc => query.OrderByDescending(p => p.Name),
                    ProductSortType.PriceAsc => query.OrderBy(p => p.Price),
                    ProductSortType.PriceDesc => query.OrderByDescending(p => p.Price),
                    ProductSortType.DescriptionAsc => query.OrderBy(p => p.Description),
                    ProductSortType.DescriptionDesc => query.OrderByDescending(p => p.Description),
                    ProductSortType.CreatedAtAsc => query.OrderBy(p => p.CreatedAt),
                    ProductSortType.CreatedAtDesc => query.OrderByDescending(p => p.CreatedAt),
                    ProductSortType.ModifiedAtAsc => query.OrderBy(p => p.ModifiedAt),
                    ProductSortType.ModifiedAtDesc => query.OrderByDescending(p => p.ModifiedAt),
                    ProductSortType.CategoryTagAsc => query.OrderBy(p => p.Categorytag),
                    ProductSortType.CategoryTagDesc => query.OrderByDescending(p => p.Categorytag),
                    ProductSortType.CategoryNameAsc => query.OrderBy(p => p.Category.Name),
                    ProductSortType.CategoryNameDesc => query.OrderByDescending(p => p.Category.Name),
                    _ => query.AsQueryable(),
                };
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
                }).ToListAsync();

            return new PagedList<AddProduct>(productsList, totalCount, pageNumber, pageSize);
        }

        public async Task<int> TotalProductsCounts(SearchFilter searchFilter)
        {
            return await dbcontext.Products.Where(p => !p.DeletedAt.HasValue && p.UserId == searchFilter.userId).CountAsync();
        }

        public async Task<AddProduct> AddProductViewCategories(int userId)
        {
            AddProduct addProduct = new AddProduct()
            {
                categories = await dbcontext.Categories.Where(c => (!c.DeletedAt.HasValue && (c.UserId == userId || c.IsSystem)))
                                                       .OrderBy(c => c.Id).ToListAsync()
            };
            return addProduct;
        }

        public async Task<bool> CheckProductInDb(AddProductDTO addProduct)
        {
            return await dbcontext.Products.AnyAsync(p => p.Name.ToLower() == addProduct.ProductName.ToLower() &&
                                                     p.CategoryId == addProduct.CategoryId &&
                                                     p.UserId == addProduct.userId);
        }


        public async Task AddProductInDb(AddProductDTO addProduct)
        {
            if (addProduct.Fileupload != null)
            {
                string fileName = string.Empty;
                string filePath = string.Empty;
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
                await repo.AddAsyncAndSave(product);
            }
            else
            {
                Product product = new Product
                {
                    Name = addProduct.ProductName.Trim(),
                    CategoryId = (int)addProduct.CategoryId,
                    CreatedAt = DateTime.Now,
                    Categorytag = addProduct.CategoryTag,
                    Filename = null,
                    Price = addProduct.Price,
                    Description = addProduct.Description.Trim(),
                    UserId = addProduct.userId
                };
                await repo.AddAsyncAndSave(product);
            }

        }

        public async Task<EditProduct> GetProduct(int id, int userId)
        {
            Product? product = await dbcontext.Products.FirstOrDefaultAsync(p => p.Id == id);

            List<Category> categoryList = await dbcontext.Categories.Where(c => (!c.DeletedAt.HasValue && (c.UserId == userId || c.IsSystem)))
                .OrderBy(c => c.Id).ToListAsync();

            List<CategoryList> categoryList1 = new List<CategoryList>();

            foreach (Category category in categoryList)
            {
                categoryList1.Add(new CategoryList
                {
                    Id = category.Id,
                    Name = category.Name
                });
            }

            EditProduct getProduct = new EditProduct()
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

            return getProduct;
        }

        public async Task<bool> CheckProductEditName(int productId, AddProductDTO editProduct)
        {
            Product? product = await dbcontext.Products.FirstOrDefaultAsync(p => p.Id == productId) ?? null;

            if (product == null)
            {
                return false;
            }

            if (editProduct.ProductName.ToLower() != product.Name.ToLower() || editProduct.CategoryId != product.CategoryId)
            {
                return await CheckProductNameExist(editProduct.ProductName, (int)editProduct.CategoryId, (int)editProduct.userId);
            }
            return false;
        }

        public async Task<bool> CheckProductNameExist(string productName, int categoryId, int userId)
        {
            return await dbcontext.Products.AnyAsync(p => p.Name.ToLower() == productName.ToLower() && p.CategoryId == categoryId && p.UserId == userId);
        }

        public async Task EditProduct(AddProductDTO editProduct)
        {
            Product? product = await dbcontext.Products.FirstOrDefaultAsync(p => p.Id == editProduct.ProductId);

            if (editProduct.Fileupload != null)
            {
                if (product?.Filename != null)
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

                string fileName = string.Empty;
                string filePath = string.Empty;

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
                await repo.UpdateAsyncAndSave(product);
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

                    await repo.UpdateAsyncAndSave(product);
                }
            }
        }

        public async Task<bool> DeleteProduct(int productId)
        {
            Product? product = await dbcontext.Products.FirstOrDefaultAsync(p => p.Id == productId) ?? null;

            if (product?.DeletedAt == null)
            {
                product.DeletedAt = DateTime.Now;
                product.ModifiedAt = DateTime.Now;

                await repo.UpdateAsyncAndSave(product);

                return true;
            }
            return false;
        }


        public async Task<string> ProductName(int productId)
        {
            return (await dbcontext.Products.FirstOrDefaultAsync(p => p.Id == productId))?.Name ?? "";
        }

        public async Task<int> ProductUserid(int productId)
        {
            return (await dbcontext.Products.FirstOrDefaultAsync(p => p.Id == productId))?.UserId ?? 0;
        }

        public async Task DeleteProductImage(int productId)
        {
            Product? product = await dbcontext.Products.FirstOrDefaultAsync(p => p.Id == productId);

            if (product != null && product?.Filename != null)
            {
                string fileName = product.Filename;
                string filePath = Path.Combine(Directory.GetCurrentDirectory(), "UploadDocuments", fileName);

                if (File.Exists(filePath))
                {
                    File.Delete(filePath);
                }

                product.Filename = null;

                await repo.UpdateAsyncAndSave(product);
            }
        }

        public async Task<DashboardData> UserDashboardData(int userId)
        {
            DashboardData dashboardData = new DashboardData()
            {
                totalCategories = await dbcontext.Categories.Where(c => (!c.DeletedAt.HasValue && (c.UserId == userId || c.IsSystem))).CountAsync(),
                totalProducts = await dbcontext.Products.Where(p => (!p.DeletedAt.HasValue && p.UserId == userId)).CountAsync()
            };

            return dashboardData;
        }

        public async Task<bool> CheckProduct(int productId)
        {
            return await dbcontext.Products.AnyAsync(c => c.Id == productId && !c.DeletedAt.HasValue);
        }

        public async Task<bool> CheckUsersProduct(int productId, int userId)
        {
            return await dbcontext.Products.AnyAsync(c => c.Id == productId && c.UserId == userId);
        }

        public async Task<bool> CheckProductIfExists(AddProductDTO productDto)
        {
            return await dbcontext.Products.Where(p => !p.DeletedAt.HasValue).AnyAsync(p =>
                                   p.Name.ToLower() == productDto.ProductName.ToLower() &&
                                   p.CategoryId == productDto.CategoryId &&
                                   p.UserId == productDto.userId &&
                                   (!productDto.ProductId.HasValue || p.Id != productDto.ProductId.Value));
        }
    }
}