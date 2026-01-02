using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Areas.Admin.Models
{
    public static class LanguageCode
    {
        public const string MK = "mk";
        public const string EN = "en";
    }

    public class SeoMetadata
    {
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string? OgTitle { get; set; }
        public string? OgDescription { get; set; }
        public string? OgImageUrl { get; set; }
        public string? OgType { get; set; } = "website";
    }

    public class Cta
    {
        public string Label { get; set; } = string.Empty;
        public string Url { get; set; } = string.Empty;
        public string? Variant { get; set; }
    }

    public class Hero
    {
        public string Title { get; set; } = string.Empty;
        public string? Subtitle { get; set; }
        public string? Eyebrow { get; set; }
        public Cta? PrimaryCta { get; set; }
        public Cta? SecondaryCta { get; set; }
        public string? BackgroundImageUrl { get; set; }
    }

    public class StatItem
    {
        public string Label { get; set; } = string.Empty;
        public string Value { get; set; } = string.Empty;
    }

    public class LogoItem
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string Name { get; set; } = string.Empty;
        public string LogoUrl { get; set; } = string.Empty;
    }

    public class Program
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string Title { get; set; } = string.Empty;
        public string Slug { get; set; } = string.Empty;
        public string CategoryName { get; set; } = string.Empty;
        public string CategorySlug { get; set; } = string.Empty;
        public string ShortDescription { get; set; } = string.Empty;
        public string LongDescription { get; set; } = string.Empty;
        public string? Duration { get; set; }
        public string? TargetAudience { get; set; }
        public string? PdfOutlineUrl { get; set; }
    }

    public class Testimonial
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string Quote { get; set; } = string.Empty;
        public string PersonName { get; set; } = string.Empty;
        public string? PersonTitle { get; set; }
        public string? CompanyName { get; set; }
        public string? CompanyLogoUrl { get; set; }
    }

    public class ContactFormField
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string Name { get; set; } = string.Empty;
        public string Label { get; set; } = string.Empty;
        public string Type { get; set; } = "text";
        public bool Required { get; set; }
        public List<ContactFormOption> Options { get; set; } = new();
    }

    public class ContactFormOption
    {
        public string Value { get; set; } = string.Empty;
        public string Label { get; set; } = string.Empty;
    }

    public class ContactFormConfig
    {
        public List<ContactFormField> Fields { get; set; } = new();
        public string SubmitLabel { get; set; } = string.Empty;
        public string SuccessMessage { get; set; } = string.Empty;
        public string ErrorMessage { get; set; } = string.Empty;
        public string Endpoint { get; set; } = string.Empty;
    }

    public abstract class PageModelBase
    {
        public string Slug { get; set; } = string.Empty;
        public Dictionary<string, PageContent> ContentByLang { get; set; } = new()
        {
            { LanguageCode.MK, new PageContent() },
            { LanguageCode.EN, new PageContent() }
        };
    }

    // ✅ HOME PAGE MODEL - NOW EXISTS
    public class HomePageModel : PageModelBase
    {
        public HomePageModel()
        {
            Slug = "home";
        }
        public ProgramsOverviewSection? ProgramsSection { get; set; }
    }

    // ✅ COACHING PAGE MODEL - NOW EXISTS  
    public class CoachingPageModel : PageModelBase
    {
        public CoachingPageModel()
        {
            Slug = "coaching";
        }
        public List<string> ProcessSteps { get; set; } = new();
        public string? Duration { get; set; }
        public string? TargetAudience { get; set; }
    }

    public class PageContent
    {
        public SeoMetadata Seo { get; set; } = new();
        public Hero? Hero { get; set; }
        public List<SectionBase> Sections { get; set; } = new();
        public ContactFormConfig? ContactForm { get; set; }
    }

    public class SectionBase
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string Type { get; set; } = string.Empty;
        public string? Title { get; set; }
        public string? Subtitle { get; set; }
        public int SortOrder { get; set; }
    }

    public class ProgramsOverviewSection : SectionBase
    {
        public List<Program> Programs { get; set; } = new();
        public string DetailsCtaLabel { get; set; } = string.Empty;
        public string RequestOfferCtaLabel { get; set; } = string.Empty;
    }

    public class SocialProofSection : SectionBase
    {
        public List<LogoItem> Logos { get; set; } = new();
        public List<StatItem> Stats { get; set; } = new();
    }
}
