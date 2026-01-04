namespace Infrastructure.Services;

public static class CacheKeys
{
    private const string PagePrefix = "page";
    private const string NavigationPrefix = "nav";

    // ✅ Cache key format: "page:{slug}:{language}"
    public static string Page(string slug, string language = "en")
        => $"{PagePrefix}:{slug.ToLowerInvariant()}:{language.ToLowerInvariant()}";

    // ✅ Pattern to remove all language versions of a page: "page:{slug}"
    public static string PagePattern(string slug)
        => $"{PagePrefix}:{slug.ToLowerInvariant()}";

    // ✅ Cache key for navigation: "nav:{language}"
    public static string Navigation(string language = "en")
        => $"{NavigationPrefix}:{language.ToLowerInvariant()}";

    // ✅ Pattern to remove all navigation cache
    public static string NavigationPattern()
        => NavigationPrefix;
}
