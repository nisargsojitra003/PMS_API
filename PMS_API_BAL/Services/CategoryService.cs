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
        private readonly IRepository<Category> repo;
        public CategoryService(ApplicationDbContext context, IRepository<Category> _repo)
        {
            dbcontext = context;
            repo = _repo;
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

            if (query.Count() >= 2 && searchFilter.sortType != 0)
            {
                query = (CategorySortType)searchFilter.sortType switch
                {
                    CategorySortType.NameAsc => query.OrderBy(c => c.Name),
                    CategorySortType.NameDesc => query.OrderByDescending(c => c.Name),
                    CategorySortType.CodeAsc => query.OrderBy(c => c.Code),
                    CategorySortType.CodeDesc => query.OrderByDescending(c => c.Code),
                    CategorySortType.CreatedAtAsc => query.OrderBy(c => c.CreatedAt),
                    CategorySortType.CreatedAtDesc => query.OrderByDescending(c => c.CreatedAt),
                    CategorySortType.ModifiedAtAsc => query.OrderBy(c => c.ModifiedAt),
                    CategorySortType.ModifiedAtDesc => query.OrderByDescending(c => c.ModifiedAt),
                    CategorySortType.DescriptionAsc => query.OrderBy(c => c.Description),
                    CategorySortType.DescriptionDesc => query.OrderByDescending(c => c.Description),
                    _ => query.AsQueryable(),
                };
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
            return await dbcontext.Categories.Where(c => (!c.DeletedAt.HasValue && (c.UserId == searchFilter.userId || c.IsSystem))).CountAsync();
        }

        public async Task Create(CategoryDTO category)
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
            await repo.AddAsyncAndSave(newCategory);
        }

        public async Task<bool> CheckCategoryIfAlreadyExist(CategoryDTO category)
        {
            return await dbcontext.Categories.AnyAsync(c => (c.UserId == category.UserId || c.IsSystem) && c.Id != category.Id &&
                        (c.Name.ToLower().Trim() == category.Name.ToLower().Trim() || c.Code.ToLower().Trim() == category.Code.ToLower().Trim()));
        }


        public async Task<Category> GetCategoryById(int id)
        {
            Category? category = await dbcontext.Categories.FirstOrDefaultAsync(c => c.Id == id);
            return category;
        }

        public async Task<bool> IsCategoryNameOrCodeExist(CategoryDTO category, int currentCategoryId, int userId)
        {
            return await dbcontext.Categories.AnyAsync(c =>
                (c.Name.ToLower() == category.Name.ToLower() || c.Code.ToLower() == category.Code.ToLower()) &&
                c.Id != currentCategoryId && (c.UserId == userId || c.IsSystem));
        }

        public async Task Update(CategoryDTO category)
        {
            Category? originalCategory = await dbcontext.Categories.FirstOrDefaultAsync(c => c.Id == category.Id);

            if (originalCategory != null)
            {
                originalCategory.Name = category.Name;
                originalCategory.Code = category.Code;
                originalCategory.Description = category.Description;
                originalCategory.ModifiedAt = DateTime.Now;

                await repo.UpdateAsyncAndSave(originalCategory);
            }
        }

        public async Task<bool> CategoryCount(int categoryId)
        {
            return await dbcontext.Products.AnyAsync(p => (p.CategoryId == categoryId && !p.DeletedAt.HasValue));
        }

        public async Task DeleteCategory(int categoryId)
        {
            Category? category = await dbcontext.Categories.FirstOrDefaultAsync(c => c.Id == categoryId);
            if (category != null)
            {
                category.DeletedAt = DateTime.Now;
                //dbcontext.Categories.Update(category);
                //await dbcontext.SaveChangesAsync();
                await repo.UpdateAsyncAndSave(category);
            }
        }

        public async Task<string> CategoryName(int categoryId)
        {
            return (await dbcontext.Categories.FirstOrDefaultAsync(p => p.Id == categoryId))?.Name ?? "";
        }

        public async Task<int> CategoryUserid(int categoryId)
        {
            return (await dbcontext.Categories.FirstOrDefaultAsync(p => p.Id == categoryId))?.UserId ?? 0;
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

        public async Task<bool> CheckIfCategoryExists(int categoryId)
        {
            return await dbcontext.Categories.AnyAsync(c => (c.Id == categoryId && !c.DeletedAt.HasValue));
        }

        public async Task<bool> CheckUsersCategory(int categoryId, int userId)
        {
            return await dbcontext.Categories.AnyAsync(c => (c.Id == categoryId && c.UserId == userId));
        }

        public async Task<bool> GetCategoryTypeById(int categoryId, int userId)
        {
            bool categoryExists = await dbcontext.Categories.AnyAsync(c => c.Id == categoryId && c.IsSystem);
            bool userExistsAndIsAdmin = await dbcontext.AspNetUsers.AnyAsync(a => a.Id == userId && a.Role == nameof(RoleEnum.Admin));

            return categoryExists && userExistsAndIsAdmin;
        }

    }
}