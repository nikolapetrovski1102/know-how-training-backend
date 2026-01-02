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

namespace Infrastructure.Data.Services
{
    public class PageService : IPageService
    {
        private readonly string _connectionString;
        private readonly ILogger<PageService> _logger;

        public PageService(IConfiguration configuration, ILogger<PageService> logger)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection")
                ?? throw new ArgumentNullException("Connection string not found");
            _logger = logger;
        }

        public async Task<PageDto?> GetPageBySlugAsync(string slug, string languageCode)
        {
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

                PageDto? page = null;
                if (await reader.ReadAsync())
                {
                    page = new PageDto
                    {
                        PageId = reader.GetInt32("PageId"),
                        Slug = reader.GetSafeString("Slug")!,
                        IsPublished = reader.GetBoolean("IsPublished"),
                        LanguageCode = reader.GetSafeString("LanguageCode")!,
                        Content = new PageContentDto
                        {
                            Seo = new SeoDto
                            {
                                Title = reader.GetSafeString("SeoTitle"),
                                Description = reader.GetSafeString("SeoDescription"),
                                OpenGraphTitle = reader.GetSafeString("OpenGraphTitle"),
                                OpenGraphDescription = reader.GetSafeString("OpenGraphDescription"),
                                OpenGraphImage = reader.GetSafeString("OpenGraphImage")
                            },
                            Hero = !reader.IsDBNull("HeroTitle") ? new HeroDto
                            {
                                Title = reader.GetSafeString("HeroTitle")!,
                                Subtitle = reader.GetSafeString("HeroSubtitle"),
                                CtaText = reader.GetSafeString("HeroCtaText"),
                                CtaUrl = reader.GetSafeString("HeroCtaUrl"),
                                Image = reader.GetSafeString("HeroImage") // ✅ NEW
                            } : null,
                            Sections = JsonSerializer.Deserialize<List<ContentSectionDto>>(
                                reader.GetSafeString("ContentSections") ?? "[]") ?? new()
                        },
                        Programs = new List<ProgramDto>()
                    };
                }

                if (page == null) return null;

                // Read programs
                if (await reader.NextResultAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        page.Programs.Add(new ProgramDto
                        {
                            Id = reader.GetInt32("Id"),
                            Slug = reader.GetSafeString("Slug")!,
                            Title = reader.GetSafeString("Title")!,
                            ShortDescription = reader.GetSafeString("ShortDescription"),
                            ImageUrl = reader.GetSafeString("ImageUrl"),
                            DurationHours = reader.GetSafeDecimal("DurationHours"),
                            MaxParticipants = reader.GetSafeInt32("MaxParticipants"),
                            CategoryName = reader.GetSafeString("CategoryName")
                        });
                    }
                }

                return page;
            }
            catch (SqlException ex)
            {
                _logger.LogError(ex, "Error loading page by slug {Slug}", slug);
                throw;
            }
        }

        public async Task SeedHomePageAsync()
        {
            await Task.CompletedTask;
        }
    }
}
