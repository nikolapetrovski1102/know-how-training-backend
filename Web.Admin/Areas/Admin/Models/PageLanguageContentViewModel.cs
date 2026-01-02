namespace Web.Admin.Areas.Admin.Models
{
    public class PageLanguageContentViewModel
    {
        public byte LanguageId { get; set; }
        public string LanguageCode { get; set; } = string.Empty;
        public string LanguageName { get; set; } = string.Empty;

        public string? SeoTitle { get; set; }
        public string? SeoDescription { get; set; }

        public string? HeroTitle { get; set; }
        public string? HeroSubtitle { get; set; }
        public string? HeroCtaText { get; set; }
        public string? HeroCtaUrl { get; set; }

        public string ContentSectionsJson { get; set; } = "[]";
    }
}
