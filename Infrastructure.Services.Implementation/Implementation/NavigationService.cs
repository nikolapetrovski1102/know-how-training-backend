using Core.Application.Services;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Data;
using Core.Application.DTOs;

namespace Infrastructure.Services.Implementation
{
    public class NavigationService : INavigationService
    {
        private readonly string _connectionString;
        private readonly ILogger<NavigationService> _logger;
        private readonly ICacheService _cacheService;

        public NavigationService(IConfiguration configuration, ILogger<NavigationService> logger, ICacheService cacheService)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection")!;
            _logger = logger;
            _cacheService = cacheService;
        }

        public async Task<NavigationDto> GetNavigationAsync(string languageCode)
        {
            var cacheKey = CacheKeys.Navigation(languageCode);

            var navigation = await _cacheService.GetOrCreateAsync(
                cacheKey,
                async () => await FetchNavigationFromDatabaseAsync(languageCode),
                TimeSpan.FromHours(24)
            );

            return navigation ?? new NavigationDto();
        }

        private async Task<NavigationDto> FetchNavigationFromDatabaseAsync(string languageCode)
        {
            _logger.LogInformation("Fetching navigation from database for language {Language}", languageCode);

            try
            {
                using var connection = new SqlConnection(_connectionString);
                await connection.OpenAsync();

                using var cmd = new SqlCommand("GetNavigation", connection)
                {
                    CommandType = CommandType.StoredProcedure
                };

                cmd.Parameters.AddWithValue("@LanguageCode", languageCode);

                var items = new List<NavItemDto>();

                using var reader = await cmd.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    items.Add(new NavItemDto
                    {
                        Slug = reader.GetString(reader.GetOrdinal("Slug")),
                        Title = reader.GetString(reader.GetOrdinal("Title"))
                    });
                }

                _logger.LogInformation("Loaded {Count} navigation items for language {Language}",
                    items.Count, languageCode);

                return new NavigationDto { Items = items };
            }
            catch (SqlException ex)
            {
                _logger.LogError(ex, "Error loading navigation for language {Language}", languageCode);
                throw;
            }
        }

    }
}
