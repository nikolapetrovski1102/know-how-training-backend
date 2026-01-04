using Microsoft.AspNetCore.Mvc;
using Core.Application.Services;
using Core.Application.DTOs;
using System.Text.Json;

namespace Web.API.Controllers
{
    [ApiController]
    [Route("api/admin")]
    public class AdminApiController : ControllerBase
    {
        private readonly IPageAdminService _pageAdminService;
        private readonly IPageService _pageService;
        private readonly ILogger<AdminApiController> _logger;

        public AdminApiController(
            IPageAdminService pageAdminService,
            IPageService pageService,
            ILogger<AdminApiController> logger)
        {
            _pageAdminService = pageAdminService;
            _pageService = pageService;
            _logger = logger;
        }

        /// <summary>
        /// GET /api/admin/pages - List all pages (for table)
        /// </summary>
        [HttpGet("pages")]
        public async Task<ActionResult<List<PageSummaryDto>>> GetAllPages()
        {
            try
            {
                var pages = await _pageAdminService.GetAllPagesAsync();
                return Ok(pages);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching all pages");
                return StatusCode(500, new { error = "Failed to load pages" });
            }
        }

        /// <summary>
        /// GET /api/admin/pages/{slug}?lang=en - Get page content by slug (for preview)
        /// Uses public PageService
        /// </summary>
        [HttpGet("pages/{slug}")]
        public async Task<ActionResult<PageDto>> GetPageBySlug(
            [FromRoute] string slug,
            [FromQuery] string lang = "en")
        {
            try
            {
                var page = await _pageService.GetPageBySlugAsync(slug, lang);

                if (page == null)
                {
                    return NotFound(new { error = $"Page '{slug}' not found" });
                }

                return Ok(page);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching page {Slug}", slug);
                return BadRequest(new { error = "Failed to load page" });
            }
        }

        /// <summary>
        /// GET /api/admin/pages/edit/{id} - Get page by ID for editing
        /// Uses PageAdminService
        /// </summary>
        [HttpGet("pages/edit/{id:int}")]
        public async Task<ActionResult<PageAdminDto>> GetPageForEdit(int id, [FromQuery] string lang)
        {
            try
            {
                var page = await _pageAdminService.GetPageForEditAsync(id, lang);

                if (page == null)
                {
                    return NotFound(new { error = $"Page with ID {id} not found" });
                }

                return Ok(page);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching page {Id} for editing", id);
                return BadRequest(new { error = "Failed to load page for editing" });
            }
        }

        /// <summary>
        /// POST /api/admin/pages/{slug} - Save page edits
        /// </summary>
        [HttpPost("pages/{slug}")]
        public async Task<ActionResult> UpdatePage(
            [FromRoute] string slug,
            [FromBody] UpdatePageRequest request)
        {
            try
            {
                // Normalize language: default to 'en' if null/empty
                var language = string.IsNullOrWhiteSpace(request.Language) ? "en" : request.Language.Trim().ToLower();

                _logger.LogInformation("UpdatePage: slug={Slug}, PageId={PageId}, Language={Language}, Changes={Count}",
                    slug, request.PageId, language, request.Changes?.Count ?? 0);

                // Log each change for debugging
                foreach (var change in request.Changes ?? new List<EditChange>())
                {
                    _logger.LogDebug("Change received: {Path} = {Value}", change.Path, change.Edited);
                }

                if (request.PageId == null)
                    return BadRequest(new { error = "PageId is required" });

                var pageAdmin = await _pageAdminService.GetPageForEditAsync(request.PageId.Value, language);
                if (pageAdmin == null)
                    return NotFound(new { error = $"Page not found (ID: {request.PageId})" });

                var langContent = pageAdmin.Languages.FirstOrDefault(l =>
                    l.LanguageCode.Equals(language, StringComparison.OrdinalIgnoreCase));

                if (langContent == null)
                {
                    _logger.LogInformation("Language '{Language}' not found for page {PageId}, creating from template",
                        language, request.PageId);

                    try
                    {
                        langContent = await _pageAdminService.CreatePageLanguageFromTemplateAsync(
                            request.PageId.Value,
                            language,
                            pageAdmin.Languages.First().LanguageCode
                        );

                        if (langContent == null)
                        {
                            return BadRequest(new
                            {
                                error = $"Failed to create language '{language}' for page {request.PageId}. " +
                                        "Ensure the source language (English) exists for this page."
                            });
                        }

                        pageAdmin.Languages.Add(langContent);

                        _logger.LogInformation("Successfully created language '{Language}' for page {PageId}",
                            language, request.PageId);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error creating language '{Language}' for page {PageId}",
                            language, request.PageId);
                        return BadRequest(new
                        {
                            error = $"Failed to create language '{language}': {ex.Message}"
                        });
                    }
                }

                // Apply all changes
                foreach (var change in request.Changes ?? new List<EditChange>())
                {
                    ApplyChange(langContent, change);
                }

                // Save to database
                var success = await _pageAdminService.SavePageAsync(pageAdmin);
                if (!success)
                    return BadRequest(new { error = "Failed to save page" });

                _logger.LogInformation("Page {Slug} (ID: {PageId}) saved successfully for language {Language}",
                    slug, request.PageId, language);

                return Ok(new
                {
                    message = "Page saved successfully",
                    language = language
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating page {Slug}: {Message}", slug, ex.Message);
                return BadRequest(new { error = ex.Message });
            }
        }

        #region Apply Changes
        private void ApplyChange(PageAdminLanguageDto langContent, EditChange change)
        {
            _logger.LogDebug("Applying change: {Path} = '{Edited}'", change.Path, change.Edited);

            var pathParts = change.Path.Split('.');

            switch (pathParts[0])
            {
                case "hero":
                    ApplyHeroChange(langContent, pathParts, change.Edited);
                    break;
                case "seo":
                    ApplySeoChange(langContent, pathParts, change.Edited);
                    break;
                case "intro":
                    ApplyIntroChange(langContent, pathParts, change.Edited);
                    break;
                case "stats":
                    ApplyStatsChange(langContent, pathParts, change.Edited);
                    break;
                case "sections":
                    ApplySectionsChange(langContent, pathParts, change.Edited);
                    break;
                default:
                    _logger.LogWarning("Unknown change path: {Path}", change.Path);
                    break;
            }
        }

        private void ApplyHeroChange(PageAdminLanguageDto langContent, string[] pathParts, string value)
        {
            if (pathParts.Length < 2) return;

            switch (pathParts[1])
            {
                case "title":
                    langContent.HeroTitle = value;
                    _logger.LogDebug("Updated HeroTitle to: {Value}", value);
                    break;
                case "subtitle":
                    langContent.HeroSubtitle = value;
                    _logger.LogDebug("Updated HeroSubtitle to: {Value}", value);
                    break;
                case "ctaText":
                    langContent.HeroCtaText = value;
                    break;
                case "ctaUrl":
                    langContent.HeroCtaUrl = value;
                    break;
                case "image":
                    langContent.HeroImage = value;
                    _logger.LogInformation("Updated hero image to: {Image}", value);
                    break;
                default:
                    _logger.LogWarning("Unknown hero field: {Field}", pathParts[1]);
                    break;
            }
        }

        private void ApplySeoChange(PageAdminLanguageDto langContent, string[] pathParts, string value)
        {
            if (pathParts.Length < 2) return;

            switch (pathParts[1])
            {
                case "title":
                    langContent.SeoTitle = value;
                    break;
                case "description":
                    langContent.SeoDescription = value;
                    break;
                case "openGraphTitle":
                    langContent.OpenGraphTitle = value;
                    break;
                case "openGraphDescription":
                    langContent.OpenGraphDescription = value;
                    break;
                case "openGraphImage":
                    langContent.OpenGraphImage = value;
                    break;
                default:
                    _logger.LogWarning("Unknown SEO field: {Field}", pathParts[1]);
                    break;
            }
        }

        private void ApplyIntroChange(PageAdminLanguageDto langContent, string[] pathParts, string value)
        {
            if (pathParts.Length < 2) return;

            _logger.LogDebug("Applying intro change: {Field} = {Value}", pathParts[1], value);

            // Parse sections JSON and update intro-card
            var sections = ParseSections(langContent.ContentSectionsJson);

            var introSection = sections.FirstOrDefault(s =>
                string.Equals(s.Type, "intro-card", StringComparison.OrdinalIgnoreCase));

            if (introSection == null)
            {
                _logger.LogWarning("Intro section not found, creating new one");
                introSection = new ContentSectionDto { Type = "intro-card" };
                sections.Add(introSection);
            }

            switch (pathParts[1])
            {
                case "greeting":
                    introSection.Greeting = value;
                    break;
                case "name":
                    introSection.Name = value;
                    break;
                case "title":
                    introSection.Title = value;
                    break;
                case "description":
                    introSection.Description = value;
                    break;
                default:
                    _logger.LogWarning("Unknown intro field: {Field}", pathParts[1]);
                    break;
            }

            langContent.ContentSectionsJson = JsonSerializer.Serialize(sections, new JsonSerializerOptions
            {
                WriteIndented = false
            });

            _logger.LogDebug("Updated ContentSectionsJson: {Json}", langContent.ContentSectionsJson);
        }

        private void ApplyStatsChange(PageAdminLanguageDto langContent, string[] pathParts, string value)
        {
            // Expected format: stats.items (full array) OR stats.items.0.value OR stats.title
            if (pathParts.Length < 2) return;

            _logger.LogDebug("Applying stats change: {Path} = {Value}", string.Join(".", pathParts), value);

            var sections = ParseSections(langContent.ContentSectionsJson);

            var statsSection = sections.FirstOrDefault(s =>
                string.Equals(s.Type, "stats", StringComparison.OrdinalIgnoreCase));

            if (statsSection == null)
            {
                _logger.LogWarning("Stats section not found, creating new one");
                statsSection = new ContentSectionDto
                {
                    Type = "stats",
                    Items = new List<object>()
                };
                sections.Add(statsSection);
            }

            // Handle stats.title
            if (pathParts.Length == 2 && pathParts[1] == "title")
            {
                statsSection.Title = value;
                langContent.ContentSectionsJson = JsonSerializer.Serialize(sections, new JsonSerializerOptions
                {
                    WriteIndented = false
                });
                _logger.LogDebug("Updated stats title");
                return;
            }

            // Handle stats.items (FULL ARRAY REPLACEMENT)
            if (pathParts.Length == 2 && pathParts[1] == "items")
            {
                try
                {
                    // Deserialize the entire items array
                    var newItems = JsonSerializer.Deserialize<List<Dictionary<string, object>>>(value);

                    if (newItems != null)
                    {
                        statsSection.Items = newItems;
                        _logger.LogInformation("Updated entire stats items array with {Count} items", newItems.Count);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to deserialize stats items array: {Value}", value);
                }

                langContent.ContentSectionsJson = JsonSerializer.Serialize(sections, new JsonSerializerOptions
                {
                    WriteIndented = false
                });
                return;
            }

            // Handle stats.items.{index}.{property} (INDIVIDUAL ITEM PROPERTY)
            if (pathParts.Length == 4 && pathParts[1] == "items" && int.TryParse(pathParts[2], out int statIndex))
            {
                if (statsSection.Items == null)
                {
                    statsSection.Items = new List<object>();
                }

                // Deserialize items as JsonElement first, then convert to dictionary
                var itemsList = new List<Dictionary<string, object>>();

                if (statsSection.Items is JsonElement jsonElement && jsonElement.ValueKind == JsonValueKind.Array)
                {
                    itemsList = JsonSerializer.Deserialize<List<Dictionary<string, object>>>(jsonElement.GetRawText())
                        ?? new List<Dictionary<string, object>>();
                }
                else if (statsSection.Items is List<object> objList)
                {
                    foreach (var item in objList)
                    {
                        if (item is JsonElement je)
                        {
                            var dict = JsonSerializer.Deserialize<Dictionary<string, object>>(je.GetRawText());
                            if (dict != null) itemsList.Add(dict);
                        }
                        else if (item is Dictionary<string, object> dict)
                        {
                            itemsList.Add(dict);
                        }
                    }
                }
                else if (statsSection.Items is IEnumerable<Dictionary<string, object>> dictList)
                {
                    itemsList = dictList.ToList();
                }

                // Ensure the item exists at the index
                while (itemsList.Count <= statIndex)
                {
                    itemsList.Add(new Dictionary<string, object>());
                }

                var propertyName = char.ToUpper(pathParts[3][0]) + pathParts[3].Substring(1); // Capitalize first letter
                itemsList[statIndex][propertyName] = value;

                _logger.LogDebug("Updated stats item {Index}.{Property} to: {Value}", statIndex, propertyName, value);

                statsSection.Items = itemsList;
            }

            langContent.ContentSectionsJson = JsonSerializer.Serialize(sections, new JsonSerializerOptions
            {
                WriteIndented = false
            });

            _logger.LogDebug("Updated ContentSectionsJson");
        }

        private void ApplySectionsChange(PageAdminLanguageDto langContent, string[] pathParts, string value)
        {
            if (pathParts.Length < 3 || !int.TryParse(pathParts[1], out int sectionIndex)) return;

            // Parse sections JSON and update section
            var sections = ParseSections(langContent.ContentSectionsJson);

            if (sectionIndex < sections.Count)
            {
                var section = sections[sectionIndex];

                switch (pathParts[2])
                {
                    case "title":
                        section.Title = value;
                        break;
                    case "subtitle":
                        section.Subtitle = value;
                        break;
                }
            }

            langContent.ContentSectionsJson = JsonSerializer.Serialize(sections, new JsonSerializerOptions
            {
                WriteIndented = false
            });
        }

        private List<ContentSectionDto> ParseSections(string? sectionsJson)
        {
            if (string.IsNullOrWhiteSpace(sectionsJson))
                return new List<ContentSectionDto>();

            try
            {
                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                };

                return JsonSerializer.Deserialize<List<ContentSectionDto>>(sectionsJson, options)
                    ?? new List<ContentSectionDto>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to parse sections JSON: {Json}", sectionsJson);
                return new List<ContentSectionDto>();
            }
        }
        #endregion
    }

    public class UpdatePageRequest
    {
        public int? PageId { get; set; }
        public string Language { get; set; } = "en";
        public List<EditChange> Changes { get; set; } = new();
    }

    public class EditChange
    {
        public string Path { get; set; } = string.Empty;
        public string Original { get; set; } = string.Empty;
        public string Edited { get; set; } = string.Empty;
    }
}
