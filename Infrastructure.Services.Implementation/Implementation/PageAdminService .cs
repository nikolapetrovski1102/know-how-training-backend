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
    public class PageAdminService : IPageAdminService
    {
        private readonly string _connectionString;
        private readonly ILogger<PageAdminService> _logger;
        private readonly ICacheService _cacheService;

        public PageAdminService(IConfiguration configuration, ILogger<PageAdminService> logger,ICacheService cacheService)        
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection")!;
            _logger = logger;
            _cacheService = cacheService;
        }

        public async Task<PageAdminLanguageDto?> CreatePageLanguageFromTemplateAsync(
            int pageId,
            string targetLanguageCode,
            string sourceLanguageCode = "en")
        {
            try
            {
                using var connection = new SqlConnection(_connectionString);
                await connection.OpenAsync();

                using var cmd = new SqlCommand("CreatePageLanguageFromTemplate", connection)
                {
                    CommandType = CommandType.StoredProcedure
                };

                cmd.Parameters.AddWithValue("@PageId", pageId);
                cmd.Parameters.AddWithValue("@TargetLanguageCode", targetLanguageCode);
                cmd.Parameters.AddWithValue("@SourceLanguageCode", sourceLanguageCode);

                using var reader = await cmd.ExecuteReaderAsync();

                if (!await reader.ReadAsync())
                {
                    _logger.LogError("Failed to create language template for PageId={PageId}, Language={Language}",
                        pageId, targetLanguageCode);
                    return null;
                }

                var langDto = new PageAdminLanguageDto
                {
                    Id = reader.GetInt32(reader.GetOrdinal("Id")),
                    PageId = reader.GetInt32(reader.GetOrdinal("PageId")),
                    LanguageId = reader.GetByte(reader.GetOrdinal("LanguageId")),
                    LanguageCode = reader.GetString(reader.GetOrdinal("LanguageCode")),
                    SeoTitle = reader.GetString(reader.GetOrdinal("SeoTitle")),
                    MenuTitle = reader.IsDBNull(reader.GetOrdinal("MenuTitle"))
                        ? null
                        : reader.GetString(reader.GetOrdinal("MenuTitle")),
                    SeoDescription = reader.IsDBNull(reader.GetOrdinal("SeoDescription"))
                        ? null
                        : reader.GetString(reader.GetOrdinal("SeoDescription")),
                    SeoKeywords = reader.IsDBNull(reader.GetOrdinal("SeoKeywords"))
                        ? null
                        : reader.GetString(reader.GetOrdinal("SeoKeywords")),
                    ContentSectionsJson = reader.IsDBNull(reader.GetOrdinal("Content"))
                        ? null
                        : reader.GetString(reader.GetOrdinal("Content"))
                };

                _logger.LogInformation("Created new language template: PageId={PageId}, Language={Language}, NewId={NewId}",
                    pageId, targetLanguageCode, langDto.Id);

                return langDto;
            }
            catch (SqlException ex)
            {
                _logger.LogError(ex, "SQL Error creating language template for PageId={PageId}, Language={Language}",
                    pageId, targetLanguageCode);
                throw;
            }
        }

        public async Task<List<PageSummaryDto>> GetAllPagesAsync()
        {
            var pages = new List<PageSummaryDto>();

            try
            {
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
                        Slug = reader.GetSafeString("Slug")!,
                        IsPublished = reader.GetBoolean("IsPublished"),
                        SortOrder = reader.GetInt32("SortOrder")
                    });
                }

                return pages;
            }
            catch (SqlException ex)
            {
                _logger.LogError(ex, "Error loading all pages");
                throw;
            }
        }

        public async Task<PageAdminDto?> GetPageForEditAsync(int id, string language)
        {
            try
            {
                using var connection = new SqlConnection(_connectionString);
                await connection.OpenAsync();

                using var cmd = new SqlCommand("GetPageById", connection)
                {
                    CommandType = CommandType.StoredProcedure
                };
                cmd.Parameters.AddWithValue("@Id", id);
                cmd.Parameters.AddWithValue("@LanguageCode", language);

                using var reader = await cmd.ExecuteReaderAsync();

                PageAdminDto? page = null;
                if (await reader.ReadAsync())
                {
                    // ✅ Get LanguageId from query
                    var languageId = GetLanguageId(reader.GetSafeString("LanguageCode")!);

                    page = new PageAdminDto
                    {
                        Id = reader.GetInt32("PageId"),
                        Slug = reader.GetSafeString("Slug")!,
                        IsPublished = reader.GetBoolean("IsPublished"),
                        IsMenuItem = false, // Load from Pages table if you have this column
                        SortOrder = 0,      // Load from Pages table if you have this column
                        Languages = new List<PageAdminLanguageDto>
                {
                    new PageAdminLanguageDto
                    {
                        LanguageId = languageId, // ✅ Now included
                        LanguageCode = reader.GetSafeString("LanguageCode")!,
                        SeoTitle = reader.GetSafeString("SeoTitle"),
                        SeoDescription = reader.GetSafeString("SeoDescription"),
                        HeroTitle = reader.GetSafeString("HeroTitle"),
                        HeroSubtitle = reader.GetSafeString("HeroSubtitle"),
                        HeroCtaText = reader.GetSafeString("HeroCtaText"),
                        HeroCtaUrl = reader.GetSafeString("HeroCtaUrl"),
                        ContentSectionsJson = reader.GetSafeString("ContentSections"),
                        OpenGraphTitle = reader.GetSafeString("OpenGraphTitle"),
                        OpenGraphDescription = reader.GetSafeString("OpenGraphDescription"),
                        OpenGraphImage = reader.GetSafeString("OpenGraphImage"),
                        IsPublished = reader.GetBoolean("IsPublished")
                    }
                },
                        Programs = new List<ProgramDto>()
                    };
                }

                if (page == null) return null;

                // Read programs (second result set)
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
                _logger.LogError(ex, "Error loading page {Id} for editing", id);
                throw;
            }
        }

        public async Task<bool> SavePageAsync(PageAdminDto page)
        {
            try
            {
                _logger.LogInformation("SavePageAsync called for page ID: {PageId}, Slug: {Slug}", page.Id, page.Slug);

                using var connection = new SqlConnection(_connectionString);
                await connection.OpenAsync();

                var languageContents = page.Languages.Select(lang => new
                {
                    LanguageId = lang.LanguageId,
                    SeoTitle = lang.SeoTitle,
                    SeoDescription = lang.SeoDescription,
                    HeroTitle = lang.HeroTitle,
                    HeroSubtitle = lang.HeroSubtitle,
                    HeroCtaText = lang.HeroCtaText,
                    HeroCtaUrl = lang.HeroCtaUrl,
                    ContentSections = lang.ContentSectionsJson ?? "[]",
                    IsPublished = lang.IsPublished
                }).ToList();

                var languageJson = JsonSerializer.Serialize(languageContents);

                _logger.LogDebug("Saving page with language data: {Json}", languageJson);

                using var cmd = new SqlCommand("Admin_SavePage", connection)
                {
                    CommandType = CommandType.StoredProcedure
                };

                cmd.Parameters.AddWithValue("@PageId", page.Id);
                cmd.Parameters.AddWithValue("@Slug", page.Slug);
                cmd.Parameters.AddWithValue("@IsPublished", page.IsPublished);
                cmd.Parameters.AddWithValue("@IsMenuItem", page.IsMenuItem ?? false);
                cmd.Parameters.AddWithValue("@MenuSortOrder", page.MenuSortOrder ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@SortOrder", page.SortOrder ?? 0);
                cmd.Parameters.AddWithValue("@LanguageContents", languageJson);

                _logger.LogInformation("Executing Admin_SavePage for PageId={PageId}", page.Id);

                var result = await cmd.ExecuteScalarAsync();

                _logger.LogInformation("Admin_SavePage returned: {Result}", result);

                if (result != null && Convert.ToInt32(result) == 1)
                {
                    _logger.LogInformation("Page {PageId} saved successfully", page.Id);

                    // ✅ Invalidate cache for this page (all languages)
                    InvalidatePageCache(page.Slug);

                    // ✅ Also invalidate navigation cache (menu might have changed)
                    InvalidateNavigationCache();

                    return true;
                }

                _logger.LogWarning("Page {PageId} save returned unexpected result: {Result}", page.Id, result);
                return false;
            }
            catch (SqlException ex)
            {
                _logger.LogError(ex, "SQL Error saving page {PageId}: {Message}\nProcedure: {Procedure}, Line: {Line}, Number: {Number}",
                    page.Id, ex.Message, ex.Procedure, ex.LineNumber, ex.Number);
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving page {PageId}: {Message}", page.Id, ex.Message);
                return false;
            }
        }

        #region Private Methods
        private int GetLanguageId(string languageCode)
        {
            return languageCode.ToLower() switch
            {
                "en" => 1,
                "mk" => 2,
                _ => 1
            };
        }
        private void InvalidatePageCache(string slug)
        {
            var pattern = CacheKeys.PagePattern(slug);
            _cacheService.RemoveByPattern(pattern);
            _logger.LogInformation("Invalidated cache for page: {Slug}", slug);
        }

        // ✅ Invalidate navigation cache for all languages
        private void InvalidateNavigationCache()
        {
            var pattern = CacheKeys.NavigationPattern();
            _cacheService.RemoveByPattern(pattern);
            _logger.LogInformation("Invalidated navigation cache");
        }
        #endregion

    }
}
