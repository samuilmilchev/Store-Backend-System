using AutoMapper;
using DAL.Data;
using DAL.Repository.Interfaces;
using Microsoft.EntityFrameworkCore;
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
            var products = await _context.Products
                .Where(p => p.Name.Contains(term))
                .Skip(offset)
                .Take(limit)
                .ToListAsync();

            return _mapper.Map<List<SearchResultDto>>(products);
        }
    }
}
