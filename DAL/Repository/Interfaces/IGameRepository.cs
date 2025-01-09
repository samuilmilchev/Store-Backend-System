using DAL.Entities;
using Shared.DTOs;

namespace DAL.Repository.Interfaces
{
    public interface IGameRepository
    {
        Task<List<TopPlatformDto>> GetTopPlatformsAsync();
        Task<List<Product>> SearchGamesAsync(string term, int limit, int offset);
        Task<Product> SearchGameByIdAsync(int id);
        Task<Product> CreateGame(Product product);
        Task<Product> UpdateGame(Product product);
        Task<bool> DeleteGame(Product product);
        Task<ProductRating> CreateRating(Product product, ProductRating rating);
        Task DeleteRating(Guid userId, ProductRating rating);
        Task<List<Product>> ListGames(ProductQueryDto queryData);
        List<ProductRating> GetRatings();
        Task<List<Product>> GetProducts();
    }
}
