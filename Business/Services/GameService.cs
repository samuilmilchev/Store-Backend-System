using Business.Exceptions;
using Business.Intefraces;
using DAL.Data;
using Microsoft.EntityFrameworkCore;
using Shared.DTOs;

namespace Business.Services
{
    public class GameService : IGameService
    {
        private readonly ApplicationDbContext _context;

        public GameService(ApplicationDbContext context)
        {
            _context = context;
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
            if (term == null || limit < 0 || offset < 0)
            {
                throw new MyApplicationException(ErrorStatus.InvalidData, "Invalid search term.");
            }

            return await _context.Products
                .Where(p => p.Name.Contains(term))
                .Skip(offset)
                .Take(limit)
                .Select(p => new SearchResultDto
                {
                    Id = p.Id,
                    Name = p.Name,
                    Platform = p.Platform.ToString(),
                    DateCreated = p.DateCreated,
                    TotalRating = p.TotalRating,
                    Price = p.Price
                })
                .ToListAsync();
        }
    }
}
