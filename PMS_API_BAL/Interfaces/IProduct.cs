using PMS_API_DAL.Models.CustomeModel;

namespace PMS_API_BAL.Interfaces
{
    public interface IProduct
    {
        public Task<PagedList<AddProduct>> ProductList(int pageNumber, int pageSize, SearchFilter searchFilter);
        public Task<int> TotalProductsCounts(SearchFilter searchFilter);
        public Task<EditProduct> GetProduct(int id, int userId);
        public Task AddProductInDb(AddProductDTO addProduct);
        public Task EditProduct(int productId, AddProductDTO editProduct);
        public Task<bool> DeleteProduct(int productId);
        public Task<AddProduct> AddProductViewCategories(int userId);
        public Task<bool> CheckProductInDb(AddProductDTO addProduct);
        public Task<bool> CheckProductEditName(int productId, AddProductDTO editProduct);
        public Task<bool> CheckProductNameExist(string productName, int categoryId, int userId);
        public Task DeleteProductImage(int productId);
        public Task<DashboardData> UserDashboardData(int userId);
        public Task<string> ProductName(int productId);
        public Task<int> ProductUserid(int productId);
        public Task<bool> CheckProduct(int productId);
        public Task<bool> CheckUsersProduct(int productId, int userId);
        public Task<bool> CheckProductIfExists(AddProductDTO productDto);

    }
}
