using AutoMapper;
using Business.Exceptions;
using Business.Intefraces;
using DAL.Entities;
using DAL.Repository.Interfaces;
using Microsoft.IdentityModel.Tokens;
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
            if (term.IsNullOrEmpty())
            {
                throw new MyApplicationException(ErrorStatus.InvalidData, "Invalid input.");
            }

            var products = await _gameRepository.SearchGamesAsync(term, limit, offset);

            return _mapper.Map<List<SearchResultDto>>(products);
        }

        public async Task<SearchResultDto> SearchGameByIdAsync(int id)
        {
            if (id <= 0 || id > _gameRepository.GetProducts().Count())
            {
                throw new MyApplicationException(ErrorStatus.InvalidData, "Invalid input.");
            }

            var product = await _gameRepository.SearchGameByIdAsync(id);

            return _mapper.Map<SearchResultDto>(product);
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

            var product = _mapper.Map<Product>(productData);

            product.Logo = productData.LogoUrl;
            product.Background = productData.BackgroundUrl;

            await _gameRepository.CreateGame(product);

            return _mapper.Map<SearchResultDto>(product);
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

            var product = await _gameRepository.SearchGameByIdAsync(id);

            if (product == null)
            {
                throw new MyApplicationException(ErrorStatus.NotFound, $"Product with id {id} does not exist.");
            }

            _mapper.Map(productData, product);

            if (productData.LogoUrl != null)
            {
                product.Logo = productData.LogoUrl;
            }

            if (productData.BackgroundUrl != null)
            {
                product.Background = productData.BackgroundUrl;
            }

            await _gameRepository.UpdateGame(product);

            return _mapper.Map<SearchResultDto>(product);
        }

        public async Task<bool> DeleteGame(int id)
        {
            var product = await _gameRepository.SearchGameByIdAsync(id);

            if (product == null)
            {
                throw new MyApplicationException(ErrorStatus.NotFound, $"Product with id {id} was not found.");
            }

            return await _gameRepository.DeleteGame(product);
        }

        public async Task<RatingResponseDto> CreateRating(Guid userId, CreateRatingDto ratingData)
        {
            var product = await _gameRepository.SearchGameByIdAsync(ratingData.ProductId);

            if (product == null)
            {
                throw new MyApplicationException(ErrorStatus.NotFound, $"Product with id {ratingData.ProductId} was not found.");
            }

            if (product.Ratings.Any(x => x.UserId == userId))
            {
                throw new MyApplicationException(ErrorStatus.InvalidOperation, ("You have already review this product."));
            }

            ProductRating rating = _mapper.Map<ProductRating>(ratingData);
            rating.UserId = userId;

            product.TotalRating = CalculateTotalRatingCreate(product, rating);

            var result = await _gameRepository.CreateRating(product, rating);

            return _mapper.Map<RatingResponseDto>(rating);
        }

        public async Task DeleteRating(Guid userId, DeleteRatingDto deleteRatingData)
        {
            var allRatings = _gameRepository.GetRatings();

            var rating = allRatings.FirstOrDefault(x => x.UserId == userId && x.ProductId == deleteRatingData.ProductId);

            if (rating == null)
            {
                throw new MyApplicationException(ErrorStatus.NotFound, $"Rating was not found.");
            }

            var product = _gameRepository.GetProducts()
                .FirstOrDefault(x => x.Id == deleteRatingData.ProductId);

            if (product == null)
            {
                throw new MyApplicationException(ErrorStatus.NotFound, $"Product with id {deleteRatingData.ProductId} does not exist.");
            }

            product.TotalRating = CalculateTotalRatingDelete(product, rating);

            await _gameRepository.DeleteRating(userId, rating);
        }

        public async Task<ProductListResultDto> ListGames(ProductQueryDto queryData)
        {
            var listGames = await _gameRepository.ListGames(queryData);

            var responseProducts = _mapper.Map<List<SearchResultDto>>(listGames);

            return new ProductListResultDto
            {
                Products = responseProducts,
                TotalItems = listGames.Count(),
                Page = queryData.Page,
                PageSize = queryData.PageSize
            };
        }

        private double CalculateTotalRatingCreate(Product product, ProductRating rating)
        {
            var sumOfRatings = product.Ratings.Sum(x => x.Rating);
            sumOfRatings += rating.Rating;
            var countRatings = product.Ratings.Count();
            countRatings++;

            return sumOfRatings / countRatings;
        }

        private double CalculateTotalRatingDelete(Product product, ProductRating rating)
        {
            var sumOfRatings = product.Ratings.Sum(x => x.Rating);
            sumOfRatings -= rating.Rating;
            var countRatings = product.Ratings.Count();
            countRatings--;

            return sumOfRatings / countRatings;
        }
    }
}
