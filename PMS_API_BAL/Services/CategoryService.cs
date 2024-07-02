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
                            .Where(c => (!c.DeletedAt.HasValue && (c.UserId == searchFilter.userId || c.IsSystem)))
                            .OrderByDescending(c => c.IsSystem)
                            .ThenBy(c => c.Id).AsQueryable();

            if (!string.IsNullOrEmpty(searchFilter.SearchName))
            {
                query = query.Where(c => c.Name.ToLower().Trim().Contains(searchFilter.SearchName.ToLower()));
                pageNumber = 1;
            }

            if (!string.IsNullOrEmpty(searchFilter.SearchCode))
            {
                query = query.Where(c => c.Code.ToLower().Trim().Contains(searchFilter.SearchCode.ToLower()) &&
                (string.IsNullOrEmpty(searchFilter.SearchName) || c.Name.ToLower().Trim().Contains(searchFilter.SearchName.ToLower())));

                pageNumber = 1;
            }

            if (!string.IsNullOrEmpty(searchFilter.description))
            {
                query = query.Where(c => c.Description.ToLower().Trim().Contains(searchFilter.description.ToLower()) &&
                (string.IsNullOrEmpty(searchFilter.SearchName) || c.Name.ToLower().Trim().Contains(searchFilter.SearchName.ToLower()) &&
                (string.IsNullOrEmpty(searchFilter.SearchCode) || c.Code.ToLower().Trim().Contains(searchFilter.SearchCode.ToLower()))));

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
                }).ToListAsync();

            return new PagedList<Category>(categoryList, totalCount, pageNumber, pageSize);
        }

        public async Task<int> TotalCategoriesCount(SearchFilter searchFilter)
        {
            return await dbcontext.Categories.Where(c => (!c.DeletedAt.HasValue && (c.UserId == searchFilter.userId || c.IsSystem == true))).CountAsync();
        }

        public async Task AddCategoryInDb(CategoryDTO category)
        {
            Category newCategory = new Category()
            {
                Name = category.Name,
                Code = category.Code,
                CreatedAt = DateTime.Now,
                IsSystem = false,
                Description = category.Description,
                UserId = category.UserId
            };

            await dbcontext.Categories.AddAsync(newCategory);
            await dbcontext.SaveChangesAsync();
        }

        public async Task<bool> CheckCategoryIfAlreayExist(CategoryDTO category)
        {
            return await dbcontext.Categories
                .AnyAsync(c => (c.Name.ToLower().Trim() == category.Name.ToLower().Trim() && (c.UserId == category.UserId || c.IsSystem)) ||
                               (c.Code.ToLower().Trim() == category.Code.ToLower().Trim() && (c.UserId == category.UserId || c.IsSystem)));
        }

        public async Task<bool> CheckCategoryCodeInDb(CategoryDTO category)
        {
            return await dbcontext.Categories
                .AnyAsync(c => (c.Code.ToLower().Trim() == category.Code.ToLower().Trim() && (c.UserId == category.UserId || c.IsSystem)));
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
                c.Id != currentCategoryId && (c.UserId == userId || c.IsSystem));
        }

        public async Task EditCategory(int id, CategoryDTO category)
        {
            Category? mainCategory = await dbcontext.Categories.FirstOrDefaultAsync(c => c.Id == id);

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
            int productListCount = await dbcontext.Products.Where(p => (p.CategoryId == categoryId && p.DeletedAt == null)).CountAsync();
            return productListCount == 0 ? true : false;
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

        public async Task<string> CategoryName(int categoryId)
        {
            Category? category = await dbcontext.Categories.FirstOrDefaultAsync(c => c.Id == categoryId);
            return category?.Name ?? string.Empty;
        }

        public async Task<int> CategoryUserid(int categoryId)
        {
            Category? category = await dbcontext.Categories.FirstOrDefaultAsync(c => c.Id == categoryId);
            return category?.UserId ?? 0;
        }


        public async Task CreateActivity(string description, int userId)
        {
            UserActivity userActivity = new UserActivity()
            {
                CreatedAt = DateTime.Now,
                Description = description,
                UserId = userId,
            };

            await dbcontext.AddAsync(userActivity);
            await dbcontext.SaveChangesAsync();
        }

        public async Task<bool> CheckCategory(int categoryId)
        {
            return await dbcontext.Categories.AnyAsync(c => c.Id == categoryId);
        }

        public async Task<bool> CheckUsersCategory(int categoryId, int userId)
        {
            Category? category = await dbcontext.Categories.FirstOrDefaultAsync(c => c.Id == categoryId);

            int? categoryUserId = category?.UserId;

            return categoryUserId != userId ? true : false;
        }

        public async Task<bool> GetCategoryTypeById(int categoryId, int userId)
        {
            Category? category = await dbcontext.Categories.FirstOrDefaultAsync(c => c.Id == categoryId);
            AspNetUser? aspNetUser = await dbcontext.AspNetUsers.FirstOrDefaultAsync(a => a.Id == userId);

            if (category == null || aspNetUser == null)
            {
                return false;
            }

            return (category.IsSystem && aspNetUser.Role == nameof(RoleEnum.Admin));
        }
    }
}