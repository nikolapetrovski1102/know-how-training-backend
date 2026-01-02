using Microsoft.AspNetCore.Mvc;
using Core.Application.Services;
using Core.Application.DTOs;

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
        public async Task<ActionResult<PageAdminDto>> GetPageForEdit(int id)
        {
            try
            {
                var page = await _pageAdminService.GetPageForEditAsync(id);

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
                _logger.LogInformation("UpdatePage called for slug: {Slug}, PageId: {PageId}", slug, request.PageId);

                if (request.PageId == null)
                {
                    return BadRequest(new { error = "PageId is required" });
                }

                var pageAdmin = await _pageAdminService.GetPageForEditAsync(request.PageId.Value);

                if (pageAdmin == null)
                {
                    return NotFound(new { error = $"Page not found (ID: {request.PageId})" });
                }

                _logger.LogInformation("Loaded page {PageId}: {Slug}, Languages: {Count}",
                    pageAdmin.Id, pageAdmin.Slug, pageAdmin.Languages.Count);

                // Apply inline edits
                var langContent = pageAdmin.Languages.FirstOrDefault(l => l.LanguageCode == request.Language);
                if (langContent == null)
                {
                    return BadRequest(new { error = $"Language '{request.Language}' not found for this page" });
                }

                _logger.LogInformation("Applying {Count} changes", request.Changes?.Count ?? 0);

                foreach (var change in request.Changes ?? new List<EditChange>())
                {
                    _logger.LogDebug("Applying change: {Path} = '{Edited}'", change.Path, change.Edited);
                    ApplyChange(langContent, change);
                }

                var success = await _pageAdminService.SavePageAsync(pageAdmin);

                if (!success)
                {
                    return BadRequest(new { error = "Failed to save page - SavePageAsync returned false" });
                }

                _logger.LogInformation("Page {Slug} (ID: {PageId}) saved successfully", slug, request.PageId);
                return Ok(new { message = "Page saved successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating page {Slug}: {Message}\nStack: {Stack}",
                    slug, ex.Message, ex.StackTrace);

                // ✅ Return actual error details
                return BadRequest(new
                {
                    error = "Failed to save page",
                    details = ex.Message,
                    innerError = ex.InnerException?.Message,
                    type = ex.GetType().Name
                });
            }
        }

        private void ApplyChange(PageAdminLanguageDto langContent, EditChange change)
        {
            var pathParts = change.Path.Split('.');

            switch (pathParts[0])
            {
                case "hero":
                    if (pathParts.Length > 1)
                    {
                        switch (pathParts[1])
                        {
                            case "title":
                                langContent.HeroTitle = change.Edited;
                                _logger.LogDebug("Updated HeroTitle to: {Title}", change.Edited);
                                break;
                            case "subtitle":
                                langContent.HeroSubtitle = change.Edited;
                                break;
                            case "ctaText":
                                langContent.HeroCtaText = change.Edited;
                                break;
                        }
                    }
                    break;
                case "seo":
                    if (pathParts.Length > 1)
                    {
                        switch (pathParts[1])
                        {
                            case "title":
                                langContent.SeoTitle = change.Edited;
                                break;
                            case "description":
                                langContent.SeoDescription = change.Edited;
                                break;
                        }
                    }
                    break;
                default:
                    _logger.LogWarning("Unknown change path: {Path}", change.Path);
                    break;
            }
        }
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
