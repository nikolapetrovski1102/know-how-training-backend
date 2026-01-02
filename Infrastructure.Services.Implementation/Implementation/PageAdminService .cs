using Core.Application.DTOs;
using Core.Application.Services;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System.Data;
using System.Text.Json;

namespace Infrastructure.Services.Services
{
    public class PageAdminService : IPageAdminService
    {
        private readonly string _connectionString;

        public PageAdminService(IConfiguration config)
        {
            _connectionString = config.GetConnectionString("DefaultConnection")!;
        }

        public async Task<List<PageSummaryDto>> GetAllPagesAsync()
        {
            var pages = new List<PageSummaryDto>();

            using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();

            using var cmd = new SqlCommand("Admin_GetAllPages", connection)
            {
                CommandType = CommandType.StoredProcedure
            };

            using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                pages.Add(new PageSummaryDto
                {
                    Id = reader.GetInt32("Id"),
                    Slug = reader.GetString("Slug"),
                    IsPublished = reader.GetBoolean("IsPublished")
                });
            }

            return pages;
        }

        public async Task<PageAdminDto?> GetPageForEditAsync(int pageId)
        {
            var page = new PageAdminDto { PageId = pageId };

            using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();

            using var cmd = new SqlCommand("Admin_GetPageForEdit", connection)
            {
                CommandType = CommandType.StoredProcedure
            };
            cmd.Parameters.AddWithValue("@PageId", pageId);

            using var reader = await cmd.ExecuteReaderAsync();

            // First result: Page metadata
            if (await reader.ReadAsync())
            {
                page.Slug = reader.GetString("Slug");
                page.IsPublished = reader.GetBoolean("IsPublished");
                page.IsMenuItem = reader.GetBoolean("IsMenuItem");
                page.MenuSortOrder = reader.IsDBNull("MenuSortOrder") ? null : reader.GetInt32("MenuSortOrder");
            }
            else
            {
                return null;
            }

            // Second result: Language contents
            await reader.NextResultAsync();
            while (await reader.ReadAsync())
            {
                page.Languages.Add(new PageAdminLanguageDto
                {
                    LanguageId = reader.GetByte("LanguageId"),
                    LanguageCode = reader.GetString("Code"),
                    LanguageName = reader.GetString("Name"),
                    SeoTitle = reader.IsDBNull("SeoTitle") ? null : reader.GetString("SeoTitle"),
                    SeoDescription = reader.IsDBNull("SeoDescription") ? null : reader.GetString("SeoDescription"),
                    HeroTitle = reader.IsDBNull("HeroTitle") ? null : reader.GetString("HeroTitle"),
                    HeroSubtitle = reader.IsDBNull("HeroSubtitle") ? null : reader.GetString("HeroSubtitle"),
                    HeroCtaText = reader.IsDBNull("HeroCtaText") ? null : reader.GetString("HeroCtaText"),
                    HeroCtaUrl = reader.IsDBNull("HeroCtaUrl") ? null : reader.GetString("HeroCtaUrl"),
                    ContentSectionsJson = reader.GetString("ContentSections")
                });
            }

            return page;
        }

        public async Task<bool> SavePageAsync(PageAdminDto page)
        {
            using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();

            // Create JSON for language contents
            var languageJson = JsonSerializer.Serialize(page.Languages.Select(l => new
            {
                l.LanguageId,
                l.SeoTitle,
                l.SeoDescription,
                l.HeroTitle,
                l.HeroSubtitle,
                l.HeroCtaText,
                l.HeroCtaUrl,
                l.ContentSectionsJson,
                IsPublished = page.IsPublished  // Sync page publish state
            }));

            using var cmd = new SqlCommand("Admin_SavePage", connection)
            {
                CommandType = CommandType.StoredProcedure
            };
            cmd.Parameters.AddWithValue("@PageId", page.PageId);
            cmd.Parameters.AddWithValue("@Slug", page.Slug);
            cmd.Parameters.AddWithValue("@IsPublished", page.IsPublished);
            cmd.Parameters.AddWithValue("@IsMenuItem", page.IsMenuItem);
            cmd.Parameters.AddWithValue("@MenuSortOrder", (object?)page.MenuSortOrder ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@SortOrder", page.MenuSortOrder ?? 999);
            cmd.Parameters.AddWithValue("@LanguageContents", languageJson);

            var result = await cmd.ExecuteScalarAsync();
            return result != null && (int)result == 1;
        }
    }
}
