using CarvedRockFitness.Models;
namespace CarvedRockFitness.Repositories;

public interface IProductRepository
{
    Task<IEnumerable<Product?>> GetAllAsync();
    Task<Product?> GetByIdAsync(int id);
    Task<IEnumerable<Product?>> GetByCategoryAsync(string? category);
}