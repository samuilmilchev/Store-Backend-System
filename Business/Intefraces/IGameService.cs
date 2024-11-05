using Shared.DTOs;

namespace Business.Intefraces
{
    public interface IGameService
    {
        Task<List<TopPlatformDto>> GetTopPlatformsAsync();
        Task<List<SearchResultDto>> SearchGamesAsync(string term, int limit, int offset);
    }
}
