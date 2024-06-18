using PMS_API_DAL.Models;
using PMS_API_DAL.Models.CustomeModel;

namespace PMS_API_BAL.Interfaces
{
    public interface ICategory
    {
        public Task<PagedList<Category>> CategoryList(int pageNumber, int pageSize, SearchFilter searchFilter);
        public Task<int> totalCount();
        public Task AddCategoryInDb(CategoryDTO addCategory);
        public Task<bool> CheckCategoryNameInDb(string categoryName);
        public Task<bool> CheckCategoryCodeInDb(string categoryCode);
        public Task<Category> GetCategoryById(int id);
        public Task<bool> IsCategoryNameOrCodeExist(CategoryDTO editCategory, int currentCategoryId);
        public Task EditProduct(int id, CategoryDTO category);
        public Task<bool> CategoryCount(int categoryId);
        public Task DeleteCategory(int categoryId);
    }
}
