using Core.Application.DTOs;
using Core.Application.Services;
using Microsoft.AspNetCore.Mvc;

namespace Web.Admin.Controllers
{
    // Web.Api/Controllers/PagesController.cs
    [ApiController]
    [Route("api/[controller]")]
    public class PagesController : ControllerBase
    {
        private readonly IPageService _pageService;
        private readonly ILogger<PagesController> _logger;

        public PagesController(IPageService pageService, ILogger<PagesController> logger)
        {
            _pageService = pageService;
            _logger = logger;
        }

        /// <summary>
        /// Get page by slug
        /// </summary>
        [HttpGet("{slug}")]
        [ProducesResponseType(typeof(PageDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<PageDto>> Get(
            [FromRoute] string slug,
            [FromQuery] string lang = "en")
        {
            try
            {
                var page = await _pageService.GetPageBySlugAsync(slug, lang);

                if (page == null)
                {
                    _logger.LogWarning("Page not found: {Slug} (Language: {Language})", slug, lang);
                    return NotFound(new { error = $"Page '{slug}' not found for language '{lang}'" });
                }

                return Ok(page);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching page {Slug} (Language: {Language})", slug, lang);
                return StatusCode(500, new { error = "Failed to load page" });
            }
        }

    }


}
