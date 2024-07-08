namespace PMS_API_BAL.Interfaces
{
    public interface IRepository<T> where T : class
    {
        public Task SaveChangesAsync();
        public Task AddAsyncAndSave(T model);
        public Task UpdateAsyncAndSave(T model);
        public Task<T> GetById(int id);
    }
}