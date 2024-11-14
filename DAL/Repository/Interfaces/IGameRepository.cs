using DAL.Entities;
using Shared.DTOs;

namespace DAL.Repository.Interfaces
{
    public interface IGameRepository
    {
        Task<List<TopPlatformDto>> GetTopPlatformsAsync();
        Task<List<SearchResultDto>> SearchGamesAsync(string term, int limit, int offset);
        Task<SearchResultDto> SearchGameByIdAsync(int id);
        Task<Product> FindGameById(int id);
        Task<SearchResultDto> CreateGame(CreateProductDto productData);
        Task<SearchResultDto> UpdateGame(int id, UpdateProductDto productData);
        Task<bool> DeleteGame(int id);
        Task<ProductRating> CreateRating(Product product, ProductRating rating);
        Task DeleteRating(Guid userId, ProductRating rating);
        Task<List<Product>> ListGames(ProductQueryDto queryData);
        List<ProductRating> GetRatings();
        List<Product> GetProducts();
    }
}
