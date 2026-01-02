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

        public PagesController(IPageService pageService)
        {
            _pageService = pageService;
        }

        [HttpGet("{slug}")]
        public async Task<ActionResult<Core.Application.DTOs.ApiResponse<Core.Application.DTOs.PageDto>>> Get(
            string slug, [FromQuery] string lang = "en")
        {
            try
            {
                var page = await _pageService.GetPageBySlugAsync(slug, lang);
                if (page == null)
                    return NotFound(new Core.Application.DTOs.ApiResponse<Core.Application.DTOs.PageDto>
                    {
                        Success = false,
                        Message = "Page not found"
                    });

                return Ok(new Core.Application.DTOs.ApiResponse<Core.Application.DTOs.PageDto>
                {
                    Success = true,
                    Data = page
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new Core.Application.DTOs.ApiResponse<Core.Application.DTOs.PageDto>
                {
                    Success = false,
                    Message = ex.Message
                });
            }
        }

    }


}
