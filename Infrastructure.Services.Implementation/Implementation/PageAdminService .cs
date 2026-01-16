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
        private List<FileUploadReference> _pendingFileUploads = new List<FileUploadReference>();


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
                        IsPublished = reader.GetBoolean("IsPublished"),
                        HeroImage = reader.GetSafeString("HeroImage")
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

        public async Task<bool> SavePageLanguageAsync(int pageId, string slug, PageAdminLanguageDto langContent)
        {
            try
            {
                _logger.LogInformation("SavePageLanguageAsync: PageId={PageId}, LanguageId={LanguageId}",
                    pageId, langContent.LanguageId);

                using var connection = new SqlConnection(_connectionString);
                await connection.OpenAsync();

                using var cmd = new SqlCommand("Admin_SavePageLanguage", connection)
                {
                    CommandType = CommandType.StoredProcedure
                };

                // ✅ Pass scalar parameters directly - no JSON parsing needed!
                cmd.Parameters.AddWithValue("@PageId", pageId);
                cmd.Parameters.AddWithValue("@LanguageId", langContent.LanguageId);
                cmd.Parameters.AddWithValue("@SeoTitle", (object?)langContent.SeoTitle ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@SeoDescription", (object?)langContent.SeoDescription ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@HeroTitle", (object?)langContent.HeroTitle ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@HeroSubtitle", (object?)langContent.HeroSubtitle ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@HeroImage", (object?)langContent.HeroImage ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@HeroCtaText", (object?)langContent.HeroCtaText ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@HeroCtaUrl", (object?)langContent.HeroCtaUrl ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@IsPublished", langContent.IsPublished);

                cmd.Parameters.AddWithValue("@ContentSections",
                    (object?)langContent.ContentSectionsJson ?? DBNull.Value);

                var result = await cmd.ExecuteScalarAsync();

                if (result != null && Convert.ToInt32(result) == 1)
                {
                    if (_pendingFileUploads.Any())
                    {
                        await InsertFileUploadsAsync(pageId, langContent.LanguageCode);

                        _logger.LogInformation("Inserted {Count} file uploads for Page {PageId}",
                            _pendingFileUploads.Count, pageId);
                    }

                    _logger.LogInformation("Page language saved: PageId={PageId}, LanguageId={LanguageId}",
                        pageId, langContent.LanguageId);

                    // Invalidate cache
                    InvalidatePageCache(slug);
                    InvalidateNavigationCache();

                    return true;
                }

                return false;
            }
            catch (SqlException ex)
            {
                _logger.LogError(ex, "SQL Error saving page language: PageId={PageId}, LanguageId={LanguageId}",
                    pageId, langContent.LanguageId);
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving page language: PageId={PageId}, LanguageId={LanguageId}",
                    pageId, langContent.LanguageId);
                return false;
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

        public void ApplyChanges(PageAdminLanguageDto langContent, List<EditChange> changes)
        {
            // ✅ Parse ContentSections JSON only once
            List<ContentSectionDto>? sections = null;
            bool sectionsModified = false;

            var jsonPaths = new HashSet<string>
        {
            "sections", "intro", "stats", "videos", "coaches",
            "resources", "testimonials", "faq", "programs", "featured-programs"
        };

            var scalarPaths = new HashSet<string> { "hero", "seo", "images" };

            foreach (var change in changes)
            {
                var pathParts = change.Path.Split('.');
                var category = pathParts[0];

                if (jsonPaths.Contains(category))
                {
                    sections ??= ParseSections(langContent.ContentSectionsJson);
                    ApplyJsonChange(sections, pathParts, change.Edited);
                    sectionsModified = true;
                }
                else if (scalarPaths.Contains(category))
                {
                    ApplyScalarChange(langContent, category, pathParts, change.Edited);
                }
                else
                {
                    _logger.LogWarning("Unknown change category: {Category}", category);
                }
            }

            // ✅ Serialize JSON only once if modified
            if (sectionsModified && sections != null)
            {
                langContent.ContentSectionsJson = JsonSerializer.Serialize(sections,
                    new JsonSerializerOptions
                    {
                        WriteIndented = false,
                        DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
                    });
            }
        }

        #region Private Methods
        private void ApplyScalarChange(PageAdminLanguageDto langContent, string category, string[] pathParts, string value)
        {
            var field = pathParts.Length > 1 ? pathParts[1] : "";

            switch (category)
            {
                case "hero":
                    ApplyHeroChange(langContent, field, value);
                    if (field.ToLower() == "image" && IsFileUpload(value))
                        TrackFileUpload("hero", -1, "HeroImage", value);
                    break;
                case "seo":
                    ApplySeoChange(langContent, field, value);
                    if (field.ToLower() == "opengraphimage" && IsFileUpload(value))
                        TrackFileUpload("seo", -1, "OpenGraphImage", value);
                    break;
                case "images":
                    ApplyImagesChange(langContent, field, value);
                    if (IsFileUpload(value))
                        TrackFileUpload("images", -1, field, value);
                    break;
            }
        }


        private void ApplyJsonChange(List<ContentSectionDto> sections, string[] pathParts, string value)
        {
            var sectionType = pathParts[0];

            if (sectionType == "sections")
            {
                ApplySectionsChangeOptimized(sections, pathParts, value);
                return;
            }

            ApplySectionUpdateByType(sections, pathParts, value);
        }

        private void ApplySectionUpdateByType(List<ContentSectionDto> sections, string[] pathParts, string value)
        {
            var sectionType = pathParts[0];

            var typeMapping = new Dictionary<string, string[]>
    {
        { "intro", new[] { "intro-card" } },
        { "stats", new[] { "stats" } },
        { "videos", new[] { "videos" } },
        { "coaches", new[] { "coaches" } },
        { "resources", new[] { "resources" } },
        { "testimonials", new[] { "testimonials" } },
        { "faq", new[] { "faq" } },
        { "programs", new[] { "programs", "featured-programs" } },
        { "featured-programs", new[] { "featured-programs" } }
    };

            ContentSectionDto? section = null;
            if (typeMapping.TryGetValue(sectionType, out var possibleTypes))
            {
                section = sections.FirstOrDefault(s => possibleTypes.Contains(s.Type));
            }

            if (section == null)
            {
                _logger.LogWarning("{SectionType} section not found, creating new one", sectionType);
                section = new ContentSectionDto
                {
                    Type = typeMapping.ContainsKey(sectionType) ? typeMapping[sectionType][0] : sectionType
                };
                sections.Add(section);
            }

            // ✅ LOG THE PATH FOR DEBUGGING
            _logger.LogDebug("ApplySectionUpdateByType: SectionType={Type}, PathParts=[{Parts}], PathLength={Length}",
                sectionType, string.Join(", ", pathParts), pathParts.Length);

            // Handle different path lengths
            if (pathParts.Length == 2)
            {
                // stats.title OR stats.items (full replacement)
                ApplySectionField(section, pathParts[1], value);
            }
            else if (pathParts.Length == 4 && pathParts[1] == "items" && int.TryParse(pathParts[2], out int itemIndex))
            {
                // stats.items.0.value (individual item field)
                _logger.LogDebug("Updating item field: itemIndex={Index}, field={Field}", itemIndex, pathParts[3]);
                ApplySectionItemField(section, itemIndex, pathParts[3], value);
            }
            else
            {
                _logger.LogWarning("Unhandled path pattern: {Path}", string.Join(".", pathParts));
            }
        }

        private void ApplySectionField(ContentSectionDto section, string field, string value)
        {
            _logger.LogDebug("ApplySectionField: Section={Type}, Field={Field}, ValueLength={Length}",
                section.Type, field, value?.Length ?? 0);

            switch (field.ToLower())
            {
                case "title":
                    section.Title = value;
                    break;
                case "subtitle":
                    section.Subtitle = value;
                    break;
                case "description":
                    section.Description = value;
                    break;
                case "greeting":
                    section.Greeting = value;
                    break;
                case "name":
                    section.Name = value;
                    break;
                case "layout":
                    section.Layout = value;
                    break;
                case "backgroundcolor":
                    section.BackgroundColor = value;
                    break;
                case "columns":
                    if (int.TryParse(value, out int cols))
                        section.Columns = cols;
                    break;

                // ✅ CRITICAL FIX: Handle full items array replacement
                case "items":
                    try
                    {
                        _logger.LogDebug("Attempting to deserialize items array for section {Type}", section.Type);

                        var items = JsonSerializer.Deserialize<List<Dictionary<string, object>>>(value,
                            new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                        if (items != null)
                        {
                            section.Items = items;
                            _logger.LogInformation("✅ Replaced items array in {Type} section with {Count} items",
                                section.Type, items.Count);
                        }
                        else
                        {
                            _logger.LogWarning("Deserialized items is null for section {Type}", section.Type);
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Failed to deserialize items array for section {Type}: {Value}",
                            section.Type, value?.Substring(0, Math.Min(200, value?.Length ?? 0)));
                    }
                    break;

                default:
                    _logger.LogWarning("Unknown section field: {Field} for section type {Type}", field, section.Type);
                    break;
            }

            _logger.LogDebug("Updated {Type} section field {Field}", section.Type, field);
        }

        private void ApplySectionItemField(ContentSectionDto section, int itemIndex, string field, string value)
        {
            var itemsList = NormalizeItemsList(section.Items);

            while (itemsList.Count <= itemIndex)
                itemsList.Add(new Dictionary<string, object>());

            var fieldMapping = new Dictionary<string, string>
    {
        { "value", "Value" }, { "label", "Label" },
        { "url", "Url" }, { "thumbnail", "Thumbnail" },
        { "bio", "Bio" }, { "image", "Image" }, { "expertise", "Expertise" },
        { "fileurl", "FileUrl" }, { "filetype", "FileType" },
        { "quote", "Quote" }, { "author", "Author" }, { "role", "Role" }, { "company", "Company" },
        { "question", "Question" }, { "answer", "Answer" },
        { "title", "Title" }, { "description", "Description" }, { "name", "Name" }
    };

            var fieldKey = field.ToLower();
            var propertyName = fieldMapping.ContainsKey(fieldKey)
                ? fieldMapping[fieldKey]
                : char.ToUpper(field[0]) + field.Substring(1);

            itemsList[itemIndex][propertyName] = value;
            section.Items = itemsList;

            // ✅ Track file uploads
            if (IsFileUpload(value))
            {
                var fileFields = new[] { "image", "thumbnail", "fileurl" };
                if (fileFields.Contains(fieldKey))
                {
                    TrackFileUpload(section.Type, itemIndex, propertyName, value);
                    _logger.LogDebug("Tracked file upload: {Type}.{Index}.{Property} = {Url}",
                        section.Type, itemIndex, propertyName, value);
                }
            }

            _logger.LogDebug("Updated {Type} section, item {Index}.{Property}", section.Type, itemIndex, propertyName);
        }

        private void ApplyHeroChange(PageAdminLanguageDto content, string field, string value)
        {
            if (string.IsNullOrEmpty(field)) return;

            switch (field.ToLower())
            {
                case "title": content.HeroTitle = value; break;
                case "subtitle": content.HeroSubtitle = value; break;
                case "ctatext": content.HeroCtaText = value; break;
                case "ctaurl": content.HeroCtaUrl = value; break;
                case "image": content.HeroImage = value; break;
                default: _logger.LogWarning("Unknown hero field: {Field}", field); break;
            }
        }

        private void ApplySeoChange(PageAdminLanguageDto content, string field, string value)
        {
            if (string.IsNullOrEmpty(field)) return;

            switch (field.ToLower())
            {
                case "title": content.SeoTitle = value; break;
                case "description": content.SeoDescription = value; break;
                case "opengraphtitle": content.OpenGraphTitle = value; break;
                case "opengraphdescription": content.OpenGraphDescription = value; break;
                case "opengraphimage": content.OpenGraphImage = value; break;
                default: _logger.LogWarning("Unknown SEO field: {Field}", field); break;
            }
        }

        private void ApplyImagesChange(PageAdminLanguageDto content, string field, string value)
        {
            if (string.IsNullOrEmpty(field)) return;

            switch (field.ToLower())
            {
                case "hero": content.HeroImage = value; break;
                case "background": content.BackgroundImage = value; break;
                case "testimonial1": content.TestimonialImage1 = value; break;
                case "testimonial2": content.TestimonialImage2 = value; break;
                default: _logger.LogWarning("Unknown images field: {Field}", field); break;
            }
        }

        private void ApplySectionsChangeOptimized(List<ContentSectionDto> sections, string[] pathParts, string value)
        {
            if (pathParts.Length == 1 && pathParts[0] == "sections")
            {
                try
                {
                    var newSections = JsonSerializer.Deserialize<List<ContentSectionDto>>(value,
                        new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                    if (newSections != null)
                    {
                        sections.Clear();
                        sections.AddRange(newSections);
                        _logger.LogInformation("Replaced entire sections array with {Count} sections", newSections.Count);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to deserialize sections array");
                }
                return;
            }

            if (pathParts.Length < 2 || !int.TryParse(pathParts[1], out int sectionIndex))
                return;

            if (sectionIndex >= sections.Count)
            {
                _logger.LogWarning("Section index {Index} out of range (total: {Count})", sectionIndex, sections.Count);
                return;
            }

            var section = sections[sectionIndex];

            if (pathParts.Length == 3)
            {
                ApplySectionField(section, pathParts[2], value);
            }
            else if (pathParts.Length == 5 && pathParts[2] == "items" && int.TryParse(pathParts[3], out int itemIndex))
            {
                ApplySectionItemField(section, itemIndex, pathParts[4], value);
            }
            else if (pathParts.Length == 3 && pathParts[2] == "items")
            {
                try
                {
                    var items = JsonSerializer.Deserialize<List<Dictionary<string, object>>>(value);
                    section.Items = items;
                    _logger.LogInformation("Replaced items in section {Index} with {Count} items",
                        sectionIndex, items?.Count ?? 0);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to deserialize items for section {Index}", sectionIndex);
                }
            }
        }

        private List<Dictionary<string, object>> NormalizeItemsList(object? items)
        {
            if (items == null)
                return new List<Dictionary<string, object>>();

            if (items is JsonElement jsonElement && jsonElement.ValueKind == JsonValueKind.Array)
            {
                return JsonSerializer.Deserialize<List<Dictionary<string, object>>>(jsonElement.GetRawText())
                    ?? new List<Dictionary<string, object>>();
            }

            if (items is IEnumerable<Dictionary<string, object>> dictList)
            {
                return dictList.ToList();
            }

            if (items is IEnumerable<object> objList)
            {
                var result = new List<Dictionary<string, object>>();
                foreach (var item in objList)
                {
                    if (item is JsonElement je)
                    {
                        var dict = JsonSerializer.Deserialize<Dictionary<string, object>>(je.GetRawText());
                        if (dict != null) result.Add(dict);
                    }
                    else if (item is Dictionary<string, object> dict)
                    {
                        result.Add(dict);
                    }
                }
                return result;
            }

            try
            {
                var json = JsonSerializer.Serialize(items);
                return JsonSerializer.Deserialize<List<Dictionary<string, object>>>(json)
                    ?? new List<Dictionary<string, object>>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to normalize items list");
                return new List<Dictionary<string, object>>();
            }
        }

        private List<ContentSectionDto> ParseSections(string? sectionsJson)
        {
            if (string.IsNullOrWhiteSpace(sectionsJson))
                return new List<ContentSectionDto>();

            try
            {
                return JsonSerializer.Deserialize<List<ContentSectionDto>>(sectionsJson,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true })
                    ?? new List<ContentSectionDto>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to parse sections JSON");
                return new List<ContentSectionDto>();
            }
        }

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

        private void InvalidateNavigationCache()
        {
            var pattern = CacheKeys.NavigationPattern();
            _cacheService.RemoveByPattern(pattern);
            _logger.LogInformation("Invalidated navigation cache");
        }

        private bool IsFileUpload(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return false;

            return value.StartsWith("/uploads/", StringComparison.OrdinalIgnoreCase) ||
                   value.Contains("/uploads/");
        }

        private void TrackFileUpload(string sectionType, int itemIndex, string propertyName, string fileUrl)
        {
            if (!IsFileUpload(fileUrl))
                return;

            _pendingFileUploads.Add(new FileUploadReference
            {
                SectionType = sectionType,
                ItemIndex = itemIndex,
                PropertyName = propertyName,
                FileUrl = fileUrl
            });
        }

        private async Task InsertFileUploadsAsync(int pageId, string languageCode)
        {
            if (!_pendingFileUploads.Any())
                return;

            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();

                foreach (var fileRef in _pendingFileUploads)
                {
                    try
                    {
                        using (var cmd = new SqlCommand("sp_InsertPageContentFileUpload", connection))
                        {
                            cmd.CommandType = CommandType.StoredProcedure;

                            cmd.Parameters.AddWithValue("@PageContentId", pageId);
                            cmd.Parameters.AddWithValue("@ContentSectionType", fileRef.SectionType ?? (object)DBNull.Value);
                            cmd.Parameters.AddWithValue("@ItemIndex", fileRef.ItemIndex);
                            cmd.Parameters.AddWithValue("@PropertyName", fileRef.PropertyName ?? (object)DBNull.Value);
                            cmd.Parameters.AddWithValue("@FileUrl", fileRef.FileUrl);
                            cmd.Parameters.AddWithValue("@UploadedBy", "system");
                            cmd.Parameters.AddWithValue("@UploadDate", DateTime.Now);

                            await cmd.ExecuteScalarAsync();
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Failed to insert file upload: {Url}", fileRef.FileUrl);
                    }
                }
            }
        }

        #endregion

    }
}
