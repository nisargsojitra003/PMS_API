using Microsoft.EntityFrameworkCore;
using PMS_API_BAL.Interfaces;
using PMS_API_DAL.DataContext;

namespace PMS_API_BAL.Services
{
    public class Repository<T> : IRepository<T> where T : class
    {
        private readonly ApplicationDbContext _dbContext;
        private DbSet<T> entities;
        public Repository(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
            entities = dbContext.Set<T>();
        }

        public async Task SaveChangesAsync()
        {
            try
            {
                await _dbContext.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error occured while saving entity, {ex.Message}");
            }
        }

        public async Task AddAsyncAndSave(T model)
        {
            try
            {
                await _dbContext.AddAsync(model);
                await _dbContext.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error occured while saving entity, {ex.Message}");
            }
        }

        public async Task UpdateAsyncAndSave(T model)
        {
            try
            {
                _dbContext.Update(model);
                await _dbContext.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error occured while updating entity, {ex.Message}");
            }
        }

        public async Task<T> GetById(int id)
        {
            try
            {
                T? entity = await _dbContext.Set<T>().FindAsync(id);
                return entity;
            }
            catch(Exception ex)
            {
                Console.WriteLine($"Error occured while getting entity, {ex.Message}");
                throw;
            }
        }
    }
}