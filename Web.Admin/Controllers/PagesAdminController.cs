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
        public async Task<ActionResult> UpdatePage([FromRoute] string slug, [FromBody] UpdatePageRequestDTO request)
        {
            try
            {
                var language = string.IsNullOrWhiteSpace(request.Language) ? "en" : request.Language.Trim().ToLower();

                _logger.LogInformation("UpdatePage: slug={Slug}, PageId={PageId}, Language={Language}, Changes={Count}",
                    slug, request.PageId, language, request.Changes?.Count ?? 0);

                if (request.PageId == null)
                    return BadRequest(new { error = "PageId is required" });

                var pageAdmin = await _pageAdminService.GetPageForEditAsync(request.PageId.Value, language);
                if (pageAdmin == null)
                    return NotFound(new { error = $"Page not found (ID: {request.PageId})" });

                var langContent = pageAdmin.Languages.FirstOrDefault(l =>
                    l.LanguageCode.Equals(language, StringComparison.OrdinalIgnoreCase));

                if (langContent == null)
                {
                    langContent = await _pageAdminService.CreatePageLanguageFromTemplateAsync(
                        request.PageId.Value, language, pageAdmin.Languages.First().LanguageCode);

                    if (langContent == null)
                        return BadRequest(new { error = $"Failed to create language '{language}'" });

                    pageAdmin.Languages.Add(langContent);
                }

                // ✅ Apply all changes using the service
                _pageAdminService.ApplyChanges(langContent, request.Changes);

                // ✅ Save only the changed language
                var success = await _pageAdminService.SavePageLanguageAsync(request.PageId.Value, slug, langContent);

                if (!success)
                    return BadRequest(new { error = "Failed to save page" });

                _logger.LogInformation("Page {Slug} saved successfully for language {Language}", slug, language);

                return Ok(new { message = "Page saved successfully", language = language });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating page {Slug}", slug);
                return BadRequest(new { error = ex.Message });
            }
        }

    }
}
