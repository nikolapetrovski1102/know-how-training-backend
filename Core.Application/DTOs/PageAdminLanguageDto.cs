using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Application.DTOs
{
    public class PageAdminLanguageDto
    {
        public int LanguageId { get; set; }
        public string LanguageCode { get; set; } = string.Empty;
        public string? SeoTitle { get; set; }
        public string? SeoDescription { get; set; }
        public string? HeroTitle { get; set; }
        public string? HeroSubtitle { get; set; }
        public string? HeroCtaText { get; set; }
        public string? HeroCtaUrl { get; set; }
        public string? ContentSectionsJson { get; set; }
        public string? OpenGraphTitle { get; set; }
        public string? OpenGraphDescription { get; set; }
        public string? OpenGraphImage { get; set; }
        public bool IsPublished { get; set; } = true;
    }
}
