using AutoMapper;
using Business.Intefraces;
using DAL.Repository.Interfaces;
using Shared.DTOs;

namespace Business.Services
{
    public class GameService : IGameService
    {
        private readonly IGameRepository _gameRepository;
        private readonly IMapper _mapper;
        public GameService(IGameRepository gameRepository, IMapper mapper)
        {
            _gameRepository = gameRepository;
            _mapper = mapper;
        }

        public async Task<List<TopPlatformDto>> GetTopPlatformsAsync()
        {
            var topPlatforms = await _gameRepository.GetTopPlatformsAsync();

            return topPlatforms;
        }

        public async Task<List<SearchResultDto>> SearchGamesAsync(string term, int limit, int offset)
        {
            return await _gameRepository.SearchGamesAsync(term, limit, offset);
        }
    }
}
