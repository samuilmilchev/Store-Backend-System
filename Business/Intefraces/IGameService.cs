using Shared.DTOs;

namespace Business.Intefraces
{
    public interface IGameService
    {
        Task<List<TopPlatformDto>> GetTopPlatformsAsync();
        Task<List<SearchResultDto>> SearchGamesAsync(string term, int limit, int offset);
        Task<SearchResultDto> SearchGameByIdAsync(int id);
        Task<SearchResultDto> CreateGame(CreateProductDto productData);
        Task<SearchResultDto> UpdateGame(int id, UpdateProductDto productData);
        Task<bool> DeleteGame(int id);
        Task<RatingResponseDto> CreateRating(Guid userId, CreateRatingDto ratingData);
        Task DeleteRating(Guid userId, DeleteRatingDto deleteRatingData);
        Task<ProductListResultDto> ListGames(ProductQueryDto queryData);
    }
}
