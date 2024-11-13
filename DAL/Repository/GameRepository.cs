using AutoMapper;
using Business.Exceptions;
using DAL.Data;
using DAL.Entities;
using DAL.Repository.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Shared.DTOs;

namespace DAL.Repository
{
    public class GameRepository : IGameRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;

        public GameRepository(ApplicationDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<List<TopPlatformDto>> GetTopPlatformsAsync()
        {
            var topPlatforms = await _context.Products
                .GroupBy(p => p.Platform)
                .OrderByDescending(g => g.Count())
                .Take(3)
                .Select(g => new TopPlatformDto
                {
                    Platform = g.Key.ToString(),
                    ProductCount = g.Count()
                })
                .ToListAsync();

            return topPlatforms;
        }

        public async Task<List<SearchResultDto>> SearchGamesAsync(string term, int limit, int offset)
        {
            if (term.IsNullOrEmpty())
            {
                throw new MyApplicationException(ErrorStatus.InvalidData, "Invalid input.");
            }

            var products = await _context.Products
                .Where(p => !p.IsDeleted)
                .Where(p => p.Name.Contains(term))
                .Skip(offset)
                .Take(limit)
                .ToListAsync();

            if (products == null)
            {
                throw new MyApplicationException(ErrorStatus.NotFound, $"Games with term {term} was not found.");
            }

            return _mapper.Map<List<SearchResultDto>>(products);
        }

        public async Task<SearchResultDto> SearchGameByIdAsync(int id)
        {
            if (id <= 0 || id > _context.Products.Count())
            {
                throw new MyApplicationException(ErrorStatus.InvalidData, "Invalid input.");
            }

            var product = await _context.Products
                .Where(p => !p.IsDeleted)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (product == null)
            {
                throw new MyApplicationException(ErrorStatus.NotFound, $"Product with id {id} does not exist.");
            }

            return _mapper.Map<SearchResultDto>(product);
        }

        public async Task<SearchResultDto> CreateGame(CreateProductDto productData)
        {
            var product = _mapper.Map<Product>(productData);

            product.Logo = productData.LogoUrl;
            product.Background = productData.BackgroundUrl;

            _context.Products.Add(product);
            _context.SaveChanges();

            return _mapper.Map<SearchResultDto>(product);
        }

        public async Task<SearchResultDto> UpdateGame(int id, UpdateProductDto productData)
        {
            var product = await _context.Products
               .Where(p => !p.IsDeleted)
               .FirstOrDefaultAsync(p => p.Id == id);

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

            _context.Products.Update(product);
            _context.SaveChanges();

            return _mapper.Map<SearchResultDto>(product);
        }

        public async Task<bool> DeleteGame(int id)
        {
            var product = await _context.Products
                .Where(x => !x.IsDeleted)
               .FirstOrDefaultAsync(p => p.Id == id);

            if (product == null)
            {
                throw new MyApplicationException(ErrorStatus.NotFound, $"Product with id {id} was not found.");
            }

            product.IsDeleted = true;

            _context.Products.Update(product);
            _context.SaveChanges();

            return true;
        }

        public async Task<RatingResponseDto> CreateRating(Guid userId, CreateRatingDto ratingData)
        {
            var product = await _context.Products
                .Where(x => !x.IsDeleted)
                .Include(x => x.Ratings)
               .FirstOrDefaultAsync(p => p.Id == ratingData.ProductId);

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

            product.Ratings.Add(rating);
            product.TotalRating = product.Ratings.Sum(x => x.Rating) / product.Ratings.Count;

            _context.Ratings.Add(rating);
            _context.SaveChanges();

            return _mapper.Map<RatingResponseDto>(rating);
        }

        public async Task DeleteRating(Guid userId, DeleteRatingDto deleteRatingData)
        {
            var rating = _context.Ratings
                .FirstOrDefault(x => x.UserId == userId && x.ProductId == deleteRatingData.ProductId);

            if (rating == null)
            {
                throw new MyApplicationException(ErrorStatus.NotFound, $"Rating was not found.");
            }

            var product = _context.Products
                .Include(x => x.Ratings)
                .FirstOrDefault(x => x.Id == deleteRatingData.ProductId);

            product.TotalRating = product.Ratings.Sum(x => x.Rating) / product.Ratings.Count;

            _context.Ratings.Remove(rating);
            _context.SaveChanges();
        }

        public async Task<ProductListResultDto> ListGames(ProductQueryDto queryData)
        {
            var productsQuery = _context.Products
                .Where(p => !p.IsDeleted);

            if (!string.IsNullOrEmpty(queryData.Genre))
            {
                productsQuery = productsQuery.Where(p => p.Genre == queryData.Genre);
            }

            if (queryData.Age.HasValue)
            {
                productsQuery = productsQuery.Where(p => (int)p.Rating >= queryData.Age);
            }

            productsQuery = queryData.SortBy.ToLower() switch
            {
                "price" => queryData.SortDirection.ToLower() == "asc"
                    ? productsQuery.OrderBy(p => p.Price)
                    : productsQuery.OrderByDescending(p => p.Price),
                _ => queryData.SortDirection.ToLower() == "asc"
                    ? productsQuery.OrderBy(p => p.TotalRating)
                    : productsQuery.OrderByDescending(p => p.TotalRating),
            };

            int totalItems = await productsQuery.CountAsync();

            var products = await productsQuery
                .Skip((queryData.Page - 1) * queryData.PageSize)
                .Take(queryData.PageSize)
                .ToListAsync();

            var responseProducts = _mapper.Map<List<SearchResultDto>>(products);

            return new ProductListResultDto
            {
                Products = responseProducts,
                TotalItems = totalItems,
                Page = queryData.Page,
                PageSize = queryData.PageSize
            };
        }
    }
}
