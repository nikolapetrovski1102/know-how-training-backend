using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Domain.Entities.TranslationModels
{
    public class PageContent
    {
        public int Id { get; set; }
        public int PageId { get; set; }
        public byte LanguageId { get; set; }
        public string? SeoTitle { get; set; }
        public string? SeoDescription { get; set; }
        public string? SeoKeywords { get; set; }
        public string? HeroTitle { get; set; }
        public string? HeroSubtitle { get; set; }
        public string? HeroCtaText { get; set; }
        public string? HeroCtaUrl { get; set; }
        public string ContentSections { get; set; } = string.Empty;
        public string? OpenGraphTitle { get; set; }
        public string? OpenGraphDescription { get; set; }
        public string? OpenGraphImage { get; set; }
        public bool IsPublished { get; set; }
        public DateTime? PublishedAt { get; set; }
    }
}
