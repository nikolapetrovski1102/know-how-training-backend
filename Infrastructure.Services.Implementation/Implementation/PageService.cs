using Core.Application.Services;
using Core.Application.DTOs;
using Microsoft.Data.SqlClient;
using System.Text.Json;
using Microsoft.Extensions.Configuration;

namespace Infrastructure.Services.Implementation
{
    public class PageService : IPageService
    {
        private readonly string _connectionString;

        public PageService(IConfiguration config)
        {
            _connectionString = config.GetConnectionString("DefaultConnection")!;
        }

        public async Task<PageDto?> GetPageBySlugAsync(string slug, string languageCode)
        {
            using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();

            using var cmd = new SqlCommand("GetPageBySlug", connection)
            {
                CommandType = System.Data.CommandType.StoredProcedure
            };
            cmd.Parameters.AddWithValue("@Slug", slug);
            cmd.Parameters.AddWithValue("@LanguageCode", languageCode);

            using var reader = await cmd.ExecuteReaderAsync();

            if (await reader.ReadAsync())
            {
                var page = new PageDto
                {
                    Slug = reader["Slug"].ToString()!,
                    Content = new PageContentDto
                    {
                        Seo = new Core.Application.DTOs.SeoDto
                        {
                            Title = reader["SeoTitle"]?.ToString(),
                            Description = reader["SeoDescription"]?.ToString()
                        },
                        Hero = reader["HeroTitle"] != DBNull.Value ? new Core.Application.DTOs.HeroDto
                        {
                            Title = reader["HeroTitle"].ToString()!,
                            Subtitle = reader["HeroSubtitle"].ToString()!,
                            CtaText = reader["HeroCtaText"].ToString()!,
                            CtaUrl = reader["HeroCtaUrl"].ToString()!
                        } : null,
                        Sections = JsonSerializer.Deserialize<List<Core.Application.DTOs.ContentSectionDto>>(
                            reader["ContentSections"].ToString() ?? "[]") ?? new()
                    }
                };

                page.Programs = await LoadPageProgramsAsync(connection, slug, languageCode);
                return page;
            }

            return null;
        }

        private async Task<List<Core.Application.DTOs.ProgramDto>> LoadPageProgramsAsync(
            SqlConnection connection, string slug, string languageCode)
        {
            var programs = new List<Core.Application.DTOs.ProgramDto>();

            using var cmd = new SqlCommand("GetPageBySlug", connection)
            {
                CommandType = System.Data.CommandType.StoredProcedure
            };
            cmd.Parameters.AddWithValue("@Slug", slug);
            cmd.Parameters.AddWithValue("@LanguageCode", languageCode);

            using var reader = await cmd.ExecuteReaderAsync();

            while (await reader.NextResultAsync())
            {
                while (await reader.ReadAsync())
                {
                    programs.Add(new Core.Application.DTOs.ProgramDto
                    {
                        Id = (int)reader["Id"],
                        Slug = reader["Slug"].ToString()!,
                        Title = reader["Title"].ToString()!,
                        ShortDescription = reader["ShortDescription"]?.ToString(),
                        ImageUrl = reader["ImageUrl"]?.ToString()
                    });
                }
            }
            return programs;
        }

        public async Task SeedHomePageAsync()
        {
            using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();

            using var cmd = new SqlCommand(@"
                -- Your seed SQL here
                INSERT INTO Pages (Slug, IsPublished) VALUES ('home', 1);
            ", connection);

            await cmd.ExecuteNonQueryAsync();
        }
    }
}
