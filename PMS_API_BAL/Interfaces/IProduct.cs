using PMS_API_DAL.Models.CustomeModel;

namespace PMS_API_BAL.Interfaces
{
    public interface IProduct
    {
        public Task<IEnumerable<AddProduct>> ProductList();
        public Task<EditProduct> GetProduct(int id);
        public Task AddProductInDb(AddProductDTO addProduct);
        public Task EditProduct(int productId, EditProductDTO editProduct);
        public Task DeleteProduct(int productId);
        public Task<AddProduct> AddProductView();
        public Task<EditProduct> EditProductView();
        public Task<bool> CheckProductInDb(string productName, int categoryId);
        public Task<bool> CheckProductEditName(int productId, EditProductDTO editProduct);
        public Task<bool> CheckProductNameExist(string productName, int categoryId);
    }
}
