using Microsoft.EntityFrameworkCore;
using PMS_API_BAL.Interfaces;
using PMS_API_DAL.DataContext;
using PMS_API_DAL.Models;
using PMS_API_DAL.Models.CustomeModel;

namespace PMS_API_BAL.Services
{
    public class CategoryService : ICategory
    {
        private readonly ApplicationDbContext dbcontext;
        public CategoryService(ApplicationDbContext context)
        {
            dbcontext = context;
        }

        public async Task<PagedList<Category>> CategoryList(int pageNumber, int pageSize, SearchFilter searchFilter)
        {
            IQueryable<Category> query = dbcontext.Categories
                            .Where(c => (c.DeletedAt == null && (c.UserId == searchFilter.userId || (c.UserId == null && c.IsSystem == true))))
                            .OrderByDescending(c => c.IsSystem == true).ThenBy(c => c.Id)
                            .AsQueryable();
            if (!string.IsNullOrEmpty(searchFilter.SearchName))
            {
                query = query.Where(c => c.Name.ToLower().Trim().Contains(searchFilter.SearchName.ToLower()));
                pageNumber = 1;
            }
            if (!string.IsNullOrEmpty(searchFilter.SearchCode))
            {
                if (!string.IsNullOrEmpty(searchFilter.SearchName))
                {
                    query = query.Where(c => c.Code.ToLower().Trim().Contains(searchFilter.SearchCode.ToLower()) && c.Name.ToLower().Trim().Contains(searchFilter.SearchName.ToLower()));
                }
                else
                {
                    query = query.Where(c => c.Code.ToLower().Trim().Contains(searchFilter.SearchCode.ToLower()));
                }
                pageNumber = 1;
            }
            if (!string.IsNullOrEmpty(searchFilter.description))
            {
                if (!string.IsNullOrEmpty(searchFilter.SearchName) && (!string.IsNullOrEmpty(searchFilter.SearchCode)))
                {
                    query = query.Where(c => c.Code.ToLower().Trim().Contains(searchFilter.SearchCode.ToLower()) && c.Name.ToLower().Trim().Contains(searchFilter.SearchName.ToLower()) && c.Description.ToLower().Trim().Contains(searchFilter.description.ToLower()));
                }
                else
                {
                    query = query.Where(c => c.Description.ToLower().Trim().Contains(searchFilter.description.ToLower()));
                }
                pageNumber = 1;
            }

            if (query.Count() >= 2)
            {
                switch (searchFilter.sortType)
                {
                    case 1:
                        query = query.OrderBy(c => c.Name);
                        break;
                    case 2:
                        query = query.OrderByDescending(c => c.Name);
                        break;
                    case 3:
                        query = query.OrderBy(c => c.Code);
                        break;
                    case 4:
                        query = query.OrderByDescending(c => c.Code);
                        break;
                    case 5:
                        query = query.OrderBy(c => c.CreatedAt);
                        break;
                    case 6:
                        query = query.OrderByDescending(c => c.CreatedAt);
                        break;
                    case 7:
                        query = query.OrderBy(c => c.ModifiedAt);
                        break;
                    case 8:
                        query = query.OrderByDescending(c => c.ModifiedAt);
                        break;
                    case 9:
                        query = query.OrderBy(c => c.Description);
                        break;
                    case 10:
                        query = query.OrderByDescending(c => c.Description);
                        break;
                    default:
                        query = query.AsQueryable();
                        break;
                }
            }

            int totalCount = query.Count();

            List<Category> categoryList = await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(p => new Category
                {
                    Id = p.Id,
                    Description = p.Description,
                    CreatedAt = p.CreatedAt,
                    ModifiedAt = p.ModifiedAt,
                    Name = p.Name,
                    Code = p.Code,
                    IsSystem = p.IsSystem
                })
                .ToListAsync();
            return new PagedList<Category>(categoryList, totalCount, pageNumber, pageSize);
        }

        public async Task<int> totalCount(SearchFilter searchFilter)
        {
            int totalCount = await dbcontext.Categories
                            .Where(c => (c.DeletedAt == null && (c.UserId == searchFilter.userId || (c.UserId == null && c.IsSystem == true))))
                            .OrderByDescending(c => c.IsSystem == true).ThenBy(c => c.Id)
                            .CountAsync();
            return totalCount;
        }

        public async Task AddCategoryInDb(CategoryDTO addCategory)
        {
            Category category = new()
            {
                Name = addCategory.Name,
                Code = addCategory.Code,
                CreatedAt = DateTime.Now,
                IsSystem = false,
                Description = addCategory.Description,
                UserId = addCategory.UserId

            };
            await dbcontext.Categories.AddAsync(category);
            await dbcontext.SaveChangesAsync();
        }

        public async Task<bool> CheckCategoryNameInDb(string categoryName, int userId)
        {
            return await dbcontext.Categories
                .AnyAsync(c => (c.Name.ToLower().Trim() == categoryName.ToLower().Trim() && c.UserId == userId) ||
                               (c.Name.ToLower().Trim() == categoryName.ToLower().Trim() && c.IsSystem == true));
        }

        public async Task<bool> CheckCategoryCodeInDb(string categoryCode, int userId)
        {
            return await dbcontext.Categories
                .AnyAsync(c => (c.Code.ToLower().Trim() == categoryCode.ToLower().Trim() && c.UserId == userId) ||
                               (c.Code.ToLower().Trim() == categoryCode.ToLower().Trim() && c.IsSystem == true));
        }

        public async Task<Category> GetCategoryById(int id)
        {
            Category? category = await dbcontext.Categories.FirstOrDefaultAsync(c => c.Id == id);

            Category getCategory = new Category()
            {
                Id = category.Id,
                Name = category.Name,
                Code = category.Code,
                Description = category.Description
            };
            return getCategory;
        }

        public async Task<bool> IsCategoryNameOrCodeExist(CategoryDTO editCategory, int currentCategoryId, int userId)
        {
            return await dbcontext.Categories.AnyAsync(c =>
                (c.Name.ToLower() == editCategory.Name.ToLower() || c.Code.ToLower() == editCategory.Code.ToLower()) &&
                c.Id != currentCategoryId && (c.UserId == userId || c.IsSystem == true));
        }

        public async Task EditProduct(int id, CategoryDTO category)
        {
            var mainCategory = await dbcontext.Categories.FirstOrDefaultAsync(c => c.Id == id);

            if (mainCategory != null)
            {
                mainCategory.Name = category.Name;
                mainCategory.Code = category.Code;
                mainCategory.Description = category.Description;
                mainCategory.ModifiedAt = DateTime.Now;

                dbcontext.Categories.Update(mainCategory);
                await dbcontext.SaveChangesAsync();
            }
        }

        public async Task<bool> CategoryCount(int categoryId)
        {
            Category? category = await dbcontext.Categories.FirstOrDefaultAsync(c => c.Id == categoryId);
            List<Product> productList = await dbcontext.Products.Where(p => (p.CategoryId == categoryId && p.DeletedAt == null)).ToListAsync();
            if (productList.Count() == 0)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public async Task DeleteCategory(int categoryId)
        {
            Category? category = await dbcontext.Categories.FirstOrDefaultAsync(c => c.Id == categoryId);
            if (category != null)
            {
                category.DeletedAt = DateTime.Now;
                dbcontext.Categories.Update(category);
                await dbcontext.SaveChangesAsync();
            }
        }
    }
}