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
        private readonly IImagesService _imagesService;

        public GameService(IGameRepository gameRepository, IMapper mapper, IImagesService imagesService)
        {
            _gameRepository = gameRepository;
            _mapper = mapper;
            _imagesService = imagesService;
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

        public async Task<SearchResultDto> SearchGameByIdAsync(int id)
        {
            return await _gameRepository.SearchGameByIdAsync(id);
        }

        public async Task<SearchResultDto> CreateGame(CreateProductDto productData)
        {
            if (productData.Logo != null || productData.Background != null)
            {
                var logo = await _imagesService.UploadImageAsync(productData.Logo);
                var background = await _imagesService.UploadImageAsync(productData.Background);

                productData.LogoUrl = logo.Url.ToString();
                productData.BackgroundUrl = background.Url.ToString();
            }

            return await _gameRepository.CreateGame(productData);
        }

        public async Task<SearchResultDto> UpdateGame(int id, UpdateProductDto productData)
        {
            if (productData.Logo != null)
            {
                var logo = await _imagesService.UploadImageAsync(productData.Logo);

                productData.LogoUrl = logo.Url.ToString();
            }

            if (productData.Background != null)
            {
                var background = await _imagesService.UploadImageAsync(productData.Background);

                productData.BackgroundUrl = background.Url.ToString();
            }

            return await _gameRepository.UpdateGame(id, productData);
        }

        public async Task<bool> DeleteGame(int id)
        {
            return await _gameRepository.DeleteGame(id);
        }

        public async Task<RatingResponseDto> CreateRating(Guid userId, CreateRatingDto ratingData)
        {
            return await _gameRepository.CreateRating(userId, ratingData);
        }

        public async Task DeleteRating(Guid userId, DeleteRatingDto deleteRatingData)
        {
            await _gameRepository.DeleteRating(userId, deleteRatingData);
        }

        public async Task<ProductListResultDto> ListGames(ProductQueryDto queryData)
        {
            return await _gameRepository.ListGames(queryData);
        }
    }
}
