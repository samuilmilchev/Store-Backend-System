using Shared.DTOs;

namespace DAL.Repository.Interfaces
{
    public interface IGameRepository
    {
        Task<List<TopPlatformDto>> GetTopPlatformsAsync();
        Task<List<SearchResultDto>> SearchGamesAsync(string term, int limit, int offset);
        Task<SearchResultDto> SearchGameByIdAsync(int id);
        Task<SearchResultDto> CreateProduct(CreateProductDto productData);
        Task<SearchResultDto> UpdateProduct(int id, UpdateProductDto productData);
        Task<bool> DeleteProduct(int id);
    }
}
