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

        private void ApplySectionsChange(PageAdminLanguageDto langContent, string[] pathParts, string value)
        {
            var sectionsList = new List<ContentSectionDto>();
            if (pathParts.Length == 1 && pathParts[0] == "sections")
            {
                try
                {
                    // Validate JSON and assign directly
                    sectionsList = JsonSerializer.Deserialize<List<ContentSectionDto>>(value, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });

                    if (sectionsList != null)
                    {
                        langContent.ContentSectionsJson = JsonSerializer.Serialize(sectionsList, new JsonSerializerOptions
                        {
                            WriteIndented = false
                        });
                        _logger.LogInformation("Replaced entire sections array with {Count} sections", sectionsList.Count);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to deserialize sections array: {Value}", value);
                }
                return;
            }

            // Handle individual section updates: sections.0.title, sections.1.items, etc.
            if (pathParts.Length < 2 || !int.TryParse(pathParts[1], out int sectionIndex)) return;

            var sections = ParseSections(langContent.ContentSectionsJson);

            if (sectionIndex >= sections.Count)
            {
                _logger.LogWarning("Section index {Index} out of range (total: {Count})", sectionIndex, sections.Count);
                return;
            }

            var section = sections[sectionIndex];

            if (pathParts.Length == 3)
            {
                // Simple field update: sections.0.title, sections.0.subtitle
                var fieldName = pathParts[2];

                switch (fieldName.ToLower())
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
                    case "items":
                        // Full items array replacement for this section
                        try
                        {
                            var items = JsonSerializer.Deserialize<List<Dictionary<string, object>>>(value);
                            section.Items = items;
                            _logger.LogInformation("Replaced items array in section {Index} with {Count} items", sectionIndex, items?.Count ?? 0);
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, "Failed to deserialize items array for section {Index}", sectionIndex);
                        }
                        break;
                    default:
                        _logger.LogWarning("Unknown section field: {Field}", fieldName);
                        break;
                }
            }
            else if (pathParts.Length >= 4 && pathParts[2] == "items")
            {
                // Item-level update: sections.0.items.0.value
                if (int.TryParse(pathParts[3], out int itemIndex) && pathParts.Length == 5)
                {
                    if (section.Items == null)
                        section.Items = new List<object>();

                    // Deserialize items as dictionary list
                    var itemsList = new List<Dictionary<string, object>>();

                    if (section.Items is JsonElement jsonElement && jsonElement.ValueKind == JsonValueKind.Array)
                    {
                        itemsList = JsonSerializer.Deserialize<List<Dictionary<string, object>>>(jsonElement.GetRawText())
                            ?? new List<Dictionary<string, object>>();
                    }
                    else if (section.Items is List<object> objList)
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
                    else if (section.Items is IEnumerable<Dictionary<string, object>> dictList)
                    {
                        itemsList = dictList.ToList();
                    }

                    // Ensure item exists
                    while (itemsList.Count <= itemIndex)
                    {
                        itemsList.Add(new Dictionary<string, object>());
                    }

                    var propertyName = char.ToUpper(pathParts[4][0]) + pathParts[4].Substring(1);
                    itemsList[itemIndex][propertyName] = value;

                    _logger.LogDebug("Updated section {SectionIndex}, item {ItemIndex}.{Property} to: {Value}",
                        sectionIndex, itemIndex, propertyName, value);

                    section.Items = itemsList;
                }
            }

            langContent.ContentSectionsJson = JsonSerializer.Serialize(sections, new JsonSerializerOptions
            {
                WriteIndented = false
            });

            _logger.LogDebug("Updated ContentSectionsJson");
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
