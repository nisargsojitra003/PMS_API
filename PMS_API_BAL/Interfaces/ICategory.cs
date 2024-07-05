using PMS_API_DAL.Models;
using PMS_API_DAL.Models.CustomeModel;

namespace PMS_API_BAL.Interfaces
{
    public interface ICategory
    {
        public Task<PagedList<Category>> CategoryList(int pageNumber, int pageSize, SearchFilter searchFilter);
        public Task<int> TotalCategoriesCount(SearchFilter searchFilter);
        public Task Create(CategoryDTO addCategory);
        public Task<bool> CheckCategoryIfAlreadyExist(CategoryDTO category);
        public Task<Category> GetCategoryById(int id);
        public Task<bool> IsCategoryNameOrCodeExist(CategoryDTO editCategory, int currentCategoryId, int userId);
        public Task Update(CategoryDTO category);
        public Task<bool> CategoryCount(int categoryId);
        public Task DeleteCategory(int categoryId);
        public Task CreateActivity(string description, int userId);
        public Task<string> CategoryName(int categoryId);
        public Task<int> CategoryUserid(int categoryId);
        public Task<bool> CheckIfCategoryExists(int categoryId);
        public Task<bool> CheckUsersCategory(int categoryId, int userId);
        public Task<bool> GetCategoryTypeById(int categoryId, int userId);
    }
}
