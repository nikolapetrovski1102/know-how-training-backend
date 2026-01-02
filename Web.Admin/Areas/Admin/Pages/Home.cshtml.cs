using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using Web.Admin.Models;
using Microsoft.AspNetCore.Mvc;
using Areas.Admin.Models;
using Areas.Admin.Data;

namespace Areas.Admin.Pages.Pages
{
    public class HomeModel : PageModel
    {
        private readonly Areas.Admin.Data.PageContext _context;

        [BindProperty]
        public HomePageModel Page { get; set; } = new();
        [BindProperty(SupportsGet = true)]
        public string CurrentLang { get; set; } = LanguageCode.MK;
        
        public HomeModel(Areas.Admin.Data.PageContext context)
        {
            _context = context;
        }

        public async Task OnGetAsync(string? lang = LanguageCode.MK)
        {
            CurrentLang = lang ?? LanguageCode.MK;

            var entity = await _context.Pages
                .FirstOrDefaultAsync(p => p.Slug == "home" && p.Language == CurrentLang);

            if (entity?.ContentJson != null)
            {
                Page.ContentByLang[CurrentLang] = JsonSerializer.Deserialize<PageContent>(entity.ContentJson)
                    ?? new PageContent();
            }
        }

        public async Task<IActionResult> OnPostAsync(string lang)
        {
            CurrentLang = lang ?? LanguageCode.MK;

            var entity = await _context.Pages
                .FirstOrDefaultAsync(p => p.Slug == "home" && p.Language == CurrentLang);

            if (entity == null)
            {
                entity = new PageEntity
                {
                    Slug = "home",
                    Language = CurrentLang
                };
                _context.Pages.Add(entity);
            }

            entity.ContentJson = JsonSerializer.Serialize(Page.ContentByLang[CurrentLang]);
            entity.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            TempData["Success"] = "Home page saved successfully!";
            return RedirectToPage(new { lang });
        }
    }
}
