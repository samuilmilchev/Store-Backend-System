using AutoMapper;
using Business.Exceptions;
using DAL.Data;
using DAL.Entities;
using DAL.Repository.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Shared.DTOs;
using Shared.Enums;

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
                .Include(p => p.Ratings)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (product == null)
            {
                throw new MyApplicationException(ErrorStatus.NotFound, $"Product with id {id} does not exist.");
            }

            return _mapper.Map<SearchResultDto>(product);
        }

        public async Task<Product> FindGameById(int id)
        {
            if (id <= 0 || id > _context.Products.Count())
            {
                throw new MyApplicationException(ErrorStatus.InvalidData, "Invalid input.");
            }

            var product = await _context.Products
                .Where(p => !p.IsDeleted)
                .Include(p => p.Ratings)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (product == null)
            {
                throw new MyApplicationException(ErrorStatus.NotFound, $"Product with id {id} does not exist.");
            }

            return product;
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

        public async Task<ProductRating> CreateRating(Product product, ProductRating rating)
        {
            product.Ratings.Add(rating);

            _context.Ratings.Add(rating);
            _context.SaveChanges();

            return rating;
        }

        public async Task DeleteRating(Guid userId, ProductRating rating)
        {
            _context.Ratings.Remove(rating);
            _context.SaveChanges();
        }

        public async Task<List<Product>> ListGames(ProductQueryDto queryData)
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

            productsQuery = queryData.SortBy switch
            {
                SortBy.Price => queryData.SortDirection == SortDirection.Asc
                    ? productsQuery.OrderBy(p => p.Price)
                    : productsQuery.OrderByDescending(p => p.Price),
                _ => queryData.SortDirection == SortDirection.Asc
                    ? productsQuery.OrderBy(p => p.TotalRating)
                    : productsQuery.OrderByDescending(p => p.TotalRating),
            };

            int totalItems = await productsQuery.CountAsync();

            return await productsQuery
                .Skip((queryData.Page - 1) * queryData.PageSize)
                .Take(queryData.PageSize)
                .ToListAsync();
        }

        public List<ProductRating> GetRatings()
        {
            var ratings = _context.Ratings.ToList();

            return ratings;
        }

        public List<Product> GetProducts()
        {
            var products = _context.Products
                .Where(p => !p.IsDeleted)
                .Include(p => p.Ratings)
                .ToList();

            return products;
        }
    }
}
