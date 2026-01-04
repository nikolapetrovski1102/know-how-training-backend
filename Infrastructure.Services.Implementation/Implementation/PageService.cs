using System;
using System.Collections.Generic;
using System.Data;
using System.Text.Json;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Core.Application.DTOs;
using Core.Application.Services;
using Core.Application.Helpers;
using Infrastructure.Services;

namespace Infrastructure.Data.Services
{
    public class PageService : IPageService
    {
        private readonly string _connectionString;
        private readonly ILogger<PageService> _logger;
        private readonly ICacheService _cacheService;

        public PageService(IConfiguration configuration, ILogger<PageService> logger, ICacheService cacheService)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection")
                ?? throw new ArgumentNullException("Connection string not found");
            _logger = logger;
            _cacheService = cacheService;
        }
        public async Task<PageDto?> GetPageBySlugAsync(string slug, string languageCode = "en")
        {
            var cacheKey = CacheKeys.Page(slug, languageCode);

            var page = await _cacheService.GetOrCreateAsync(
                cacheKey,
                async () => await FetchPageFromDatabaseAsync(slug, languageCode),
                TimeSpan.FromMinutes(30)
            );

            if (page != null)
            {
                _logger.LogInformation("Retrieved page {Slug} for language {Language}", slug, languageCode);
            }
            else
            {
                _logger.LogWarning("Page {Slug} not found for language {Language}", slug, languageCode);
            }

            return page;
        }

        private async Task<PageDto?> FetchPageFromDatabaseAsync(string slug, string languageCode)
        {
            _logger.LogInformation("Fetching page {Slug} from database for language {Language}", slug, languageCode);

            try
            {
                using var connection = new SqlConnection(_connectionString);
                await connection.OpenAsync();

                using var cmd = new SqlCommand("GetPageBySlug", connection)
                {
                    CommandType = CommandType.StoredProcedure
                };

                cmd.Parameters.AddWithValue("@Slug", slug);
                cmd.Parameters.AddWithValue("@LanguageCode", languageCode);

                using var reader = await cmd.ExecuteReaderAsync();

                // Read first result set (Page data)
                if (!await reader.ReadAsync())
                {
                    return null;
                }

                var pageDto = new PageDto
                {
                    Id = reader.GetInt32(reader.GetOrdinal("PageId")),
                    Slug = reader.GetString(reader.GetOrdinal("Slug")),
                    IsPublished = reader.GetBoolean(reader.GetOrdinal("IsPublished")),
                    LanguageCode = reader.GetString(reader.GetOrdinal("LanguageCode")),

                    SeoTitle = reader.GetString(reader.GetOrdinal("SeoTitle")),
                    SeoDescription = reader.IsDBNull(reader.GetOrdinal("SeoDescription"))
                        ? null
                        : reader.GetString(reader.GetOrdinal("SeoDescription")),

                    HeroTitle = reader.IsDBNull(reader.GetOrdinal("HeroTitle"))
                        ? null
                        : reader.GetString(reader.GetOrdinal("HeroTitle")),
                    HeroSubtitle = reader.IsDBNull(reader.GetOrdinal("HeroSubtitle"))
                        ? null
                        : reader.GetString(reader.GetOrdinal("HeroSubtitle")),
                    HeroCtaText = reader.IsDBNull(reader.GetOrdinal("HeroCtaText"))
                        ? null
                        : reader.GetString(reader.GetOrdinal("HeroCtaText")),
                    HeroCtaUrl = reader.IsDBNull(reader.GetOrdinal("HeroCtaUrl"))
                        ? null
                        : reader.GetString(reader.GetOrdinal("HeroCtaUrl")),
                    HeroImage = reader.IsDBNull(reader.GetOrdinal("HeroImage"))
                        ? null
                        : reader.GetString(reader.GetOrdinal("HeroImage")),

                    ContentSectionsJson = reader.IsDBNull(reader.GetOrdinal("ContentSections"))
                        ? null
                        : reader.GetString(reader.GetOrdinal("ContentSections")),

                    OpenGraphTitle = reader.IsDBNull(reader.GetOrdinal("OpenGraphTitle"))
                        ? null
                        : reader.GetString(reader.GetOrdinal("OpenGraphTitle")),
                    OpenGraphDescription = reader.IsDBNull(reader.GetOrdinal("OpenGraphDescription"))
                        ? null
                        : reader.GetString(reader.GetOrdinal("OpenGraphDescription")),
                    OpenGraphImage = reader.IsDBNull(reader.GetOrdinal("OpenGraphImage"))
                        ? null
                        : reader.GetString(reader.GetOrdinal("OpenGraphImage"))
                };

                // ✅ Read second result set (Programs) with safe type conversion
                if (await reader.NextResultAsync())
                {
                    var programs = new List<ProgramDto>();

                    while (await reader.ReadAsync())
                    {
                        programs.Add(new ProgramDto
                        {
                            Id = reader.GetInt32(reader.GetOrdinal("Id")),
                            Slug = reader.GetString(reader.GetOrdinal("Slug")),
                            Title = reader.GetString(reader.GetOrdinal("Title")),
                            ShortDescription = reader.IsDBNull(reader.GetOrdinal("ShortDescription"))
                                ? null
                                : reader.GetString(reader.GetOrdinal("ShortDescription")),
                            ImageUrl = reader.IsDBNull(reader.GetOrdinal("ImageUrl"))
                                ? null
                                : reader.GetString(reader.GetOrdinal("ImageUrl")),

                            // ✅ FIX: Safe conversion from DECIMAL to INT
                            DurationHours = reader.IsDBNull(reader.GetOrdinal("DurationHours"))
                                ? null
                                : Convert.ToInt32(reader.GetValue(reader.GetOrdinal("DurationHours"))),

                            MaxParticipants = reader.IsDBNull(reader.GetOrdinal("MaxParticipants"))
                                ? null
                                : Convert.ToInt32(reader.GetValue(reader.GetOrdinal("MaxParticipants"))),

                            CategoryName = reader.IsDBNull(reader.GetOrdinal("CategoryName"))
                                ? null
                                : reader.GetString(reader.GetOrdinal("CategoryName"))
                        });
                    }

                    pageDto.Programs = programs;
                }

                return pageDto;
            }
            catch (SqlException ex)
            {
                _logger.LogError(ex, "SQL error fetching page {Slug} for language {Language}", slug, languageCode);
                throw;
            }
        }

        public async Task SeedHomePageAsync()
        {
            await Task.CompletedTask;
        }
    }
}
