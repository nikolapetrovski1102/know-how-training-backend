using Core.Application.DTOs;
using Core.Application.Services;
using Microsoft.AspNetCore.Mvc;

namespace Web.Admin.Controller_APIs
{
    // Web.Api/Controllers/PagesAdminController.cs
    [ApiController]
    [Route("api/old/")]
    public class PagesAdminController : Controller
    {
        private readonly IPageAdminService _pageAdminService;

        public PagesAdminController(IPageAdminService pageAdminService)
        {
            this._pageAdminService = pageAdminService;
        }

        [HttpGet("admin/pages")]
        public async Task<ActionResult<List<PageSummaryDto>>> GetPagesList()
        {
            var pages = await _pageAdminService.GetAllPagesAsync();
            return Ok(pages);
        }

        [HttpGet("admin/pages/{id}")]
        public async Task<ActionResult<PageAdminDto>> GetPage(int id)
        {
            var dto = await _pageAdminService.GetPageForEditAsync(id, "en");
            return Ok(dto);
        }

        [HttpPost("admin/pages/{id}")]
        public async Task<ActionResult> UpdatePage(int id, PageAdminDto dto)
        {
            var success = await _pageAdminService.SavePageAsync(dto);
            return success ? Ok() : BadRequest("Save failed");
        }

    }
}
