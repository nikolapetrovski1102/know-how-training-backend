using Core.Application.DTOs;
using Core.Application.Services;
using Microsoft.AspNetCore.Mvc;
using Web.Admin.Areas.Admin.Models;

namespace Web.Admin.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Route("Admin/[controller]/[action]")]
    public class PagesAdminController : Controller
    {
        private readonly IPageAdminService _pageAdminService;

        public PagesAdminController(IPageAdminService pageAdminService)
        {
            _pageAdminService = pageAdminService;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var pages = await _pageAdminService.GetAllPagesAsync();
            return View(pages); // Simple table with Edit links
        }

        [HttpGet("{id:int}")]
        public async Task<IActionResult> Edit(int id)
        {
            var dto = await _pageAdminService.GetPageForEditAsync(id);
            if (dto == null) return NotFound();

            var vm = new PageEditViewModel
            {
                PageId = dto.PageId,
                Slug = dto.Slug,
                IsPublished = dto.IsPublished,
                IsMenuItem = dto.IsMenuItem,
                MenuSortOrder = dto.MenuSortOrder,
                Languages = dto.Languages.Select(l => new PageLanguageContentViewModel
                {
                    LanguageId = l.LanguageId,
                    LanguageCode = l.LanguageCode,
                    LanguageName = l.LanguageName,
                    SeoTitle = l.SeoTitle,
                    SeoDescription = l.SeoDescription,
                    HeroTitle = l.HeroTitle,
                    HeroSubtitle = l.HeroSubtitle,
                    HeroCtaText = l.HeroCtaText,
                    HeroCtaUrl = l.HeroCtaUrl,
                    ContentSectionsJson = l.ContentSectionsJson
                }).ToList()
            };

            return View(vm);
        }

        [HttpPost("{id:int}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, PageEditViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var dto = new PageAdminDto
            {
                PageId = id,
                Slug = model.Slug,
                IsPublished = model.IsPublished,
                IsMenuItem = model.IsMenuItem,
                MenuSortOrder = model.MenuSortOrder,
                Languages = model.Languages.Select(l => new PageAdminLanguageDto
                {
                    LanguageId = l.LanguageId,
                    LanguageCode = l.LanguageCode,
                    LanguageName = l.LanguageName,
                    SeoTitle = l.SeoTitle,
                    SeoDescription = l.SeoDescription,
                    HeroTitle = l.HeroTitle,
                    HeroSubtitle = l.HeroSubtitle,
                    HeroCtaText = l.HeroCtaText,
                    HeroCtaUrl = l.HeroCtaUrl,
                    ContentSectionsJson = l.ContentSectionsJson
                }).ToList()
            };

            var success = await _pageAdminService.SavePageAsync(dto);

            if (success)
            {
                TempData["Success"] = "Page saved successfully!";
                return RedirectToAction(nameof(Edit), new { id });
            }
            else
            {
                ModelState.AddModelError("", "Failed to save page. Please try again.");
                return View(model);
            }
        }
    }
}
