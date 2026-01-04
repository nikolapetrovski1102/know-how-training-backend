using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Application.DTOs
{
    public class PageDto
    {
        // Page info
        public int Id { get; set; }
        public string Slug { get; set; } = string.Empty;
        public bool IsPublished { get; set; }
        public string LanguageCode { get; set; } = string.Empty;

        // SEO
        public string SeoTitle { get; set; } = string.Empty;
        public string? SeoDescription { get; set; }

        // Hero section
        public string? HeroTitle { get; set; }
        public string? HeroSubtitle { get; set; }
        public string? HeroCtaText { get; set; }
        public string? HeroCtaUrl { get; set; }
        public string? HeroImage { get; set; }

        // Content
        public string? ContentSectionsJson { get; set; }

        // OpenGraph
        public string? OpenGraphTitle { get; set; }
        public string? OpenGraphDescription { get; set; }
        public string? OpenGraphImage { get; set; }

        // Related programs (from second result set)
        public List<ProgramDto>? Programs { get; set; }
    }
}
