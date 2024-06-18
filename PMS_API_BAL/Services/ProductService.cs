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
        //public async Task<IEnumerable<AddProduct>> ProductList(SearchFilter searchFilter)
        //{
        //    List<Product> allproductList = await dbcontext.Products.Include(p => p.Category).Where(p => p.DeletedAt == null).OrderBy(p => p.Id).ToListAsync();
        //    allproductList = allproductList.Where(p => p.Name.ToLower().Contains(searchFilter.SearchName.ToLower()));
        //    var result = allproductList.Select(p => new AddProduct
        //    {
        //        ProductName = p.Name,
        //        ProductId = p.Id,
        //        CategoryName = p.Category.Name,
        //        CreatedDate = p.CreatedAt,
        //        ModiFiedDate = p.ModifiedAt,
        //        Description = p.Description,
        //        Price = p.Price,
        //        CategoryTag = p.Categorytag,
        //    }).ToList();
        //    return result;
        //}

        public async Task<PagedList<AddProduct>> ProductList(int pageNumber, int pageSize, SearchFilter searchFilter)
        {
            IQueryable<Product> query = dbcontext.Products.Include(p => p.Category).Where(p => p.DeletedAt == null);

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

        public async Task<int> TotalProducts()
        {
            int totalProducts = await dbcontext.Products.Include(p => p.Category).Where(p => p.DeletedAt == null).CountAsync();
            return totalProducts;
        }



        public async Task<AddProduct> AddProductView()
        {
            AddProduct addProduct = new AddProduct()
            {
                categories = await dbcontext.Categories.Where(c => c.DeletedAt == null).ToListAsync(),
            };
            return addProduct;
        }

        public async Task<bool> CheckProductInDb(string productName, int categoryId)
        {
            Product? product = await dbcontext.Products.FirstOrDefaultAsync(p => p.Name.ToLower() == productName.ToLower() && p.CategoryId == categoryId);
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
                    //UserId = userId
                };
                await dbcontext.Products.AddAsync(product);
                await dbcontext.SaveChangesAsync();

            }

        }

        public async Task<EditProduct> GetProduct(int id)
        {
            Product? product = await dbcontext.Products.FirstOrDefaultAsync(p => p.Id == id);
            if (product == null)
            {
                // Handle the case when the product is not found, e.g., return null or throw an exception.
                return null;
            }

            List<Category> categoryList = await dbcontext.Categories
                .Where(c => c.DeletedAt == null)
                .ToListAsync();

            List<CategoryList> categoryList1 = new List<CategoryList>();
            foreach (var category in categoryList)
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
                return await CheckProductNameExist(editProduct.ProductName, (int)editProduct.CategoryId);
            }
            return false;
        }

        public async Task<bool> CheckProductNameExist(string productName, int categoryId)
        {
            Product? product = await dbcontext.Products.FirstOrDefaultAsync(c => c.Name.ToLower() == productName.ToLower() && c.CategoryId == categoryId);
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
                        File.Delete(fileNameMain);
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



    }

}

