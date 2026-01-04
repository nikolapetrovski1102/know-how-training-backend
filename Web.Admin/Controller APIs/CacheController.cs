using Microsoft.AspNetCore.Mvc;
using Infrastructure.Services;
using Microsoft.Extensions.Logging;
using Core.Application.Services;

namespace Web.API.Controllers;

[ApiController]
[Route("api/admin/cache")]
public class CacheController : ControllerBase
{
    private readonly ICacheService _cacheService;
    private readonly ILogger<CacheController> _logger;

    public CacheController(ICacheService cacheService, ILogger<CacheController> logger)
    {
        _cacheService = cacheService;
        _logger = logger;
    }

    /// <summary>
    /// Clear all cache entries
    /// </summary>
    /// <remarks>
    /// This will clear ALL cached pages and navigation.
    /// Use with caution - it will cause cache misses until data is re-cached.
    /// </remarks>
    [HttpDelete("clear")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public ActionResult ClearAllCache()
        {
        _logger.LogWarning("🗑️ Clearing all cache via API request from {IP}", HttpContext.Connection.RemoteIpAddress);

        _cacheService.Clear();

        _logger.LogInformation("✅ All cache cleared successfully");

        return Ok(new
        {
            success = true,
            message = "All cache cleared successfully",
            timestamp = DateTime.UtcNow
        });
    }

    /// <summary>
    /// Clear cache for a specific page (all languages)
    /// </summary>
    /// <param name="slug">Page slug (e.g., "home", "about")</param>
    [HttpDelete("page/{slug}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public ActionResult ClearPageCache([FromRoute] string slug)
    {
        if (string.IsNullOrWhiteSpace(slug))
        {
            return BadRequest(new { error = "Slug is required" });
        }

        _logger.LogInformation("🗑️ Clearing cache for page: {Slug}", slug);

        var pattern = CacheKeys.PagePattern(slug);
        _cacheService.RemoveByPattern(pattern);

        _logger.LogInformation("✅ Cache cleared for page: {Slug}", slug);

        return Ok(new
        {
            success = true,
            message = $"Cache cleared for page: {slug}",
            pattern = pattern,
            timestamp = DateTime.UtcNow
        });
    }

    /// <summary>
    /// Clear cache for a specific page and language
    /// </summary>
    /// <param name="slug">Page slug</param>
    /// <param name="lang">Language code (e.g., "en", "mk")</param>
    [HttpDelete("page/{slug}/{lang}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public ActionResult ClearPageLanguageCache(
        [FromRoute] string slug,
        [FromRoute] string lang)
    {
        if (string.IsNullOrWhiteSpace(slug))
        {
            return BadRequest(new { error = "Slug is required" });
        }

        if (string.IsNullOrWhiteSpace(lang))
        {
            return BadRequest(new { error = "Language is required" });
        }

        _logger.LogInformation("🗑️ Clearing cache for page: {Slug}, language: {Lang}", slug, lang);

        var cacheKey = CacheKeys.Page(slug, lang);
        _cacheService.Remove(cacheKey);

        _logger.LogInformation("✅ Cache cleared for page: {Slug} ({Lang})", slug, lang);

        return Ok(new
        {
            success = true,
            message = $"Cache cleared for page: {slug} ({lang})",
            cacheKey = cacheKey,
            timestamp = DateTime.UtcNow
        });
    }

    /// <summary>
    /// Clear navigation cache (all languages)
    /// </summary>
    [HttpDelete("navigation")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public ActionResult ClearNavigationCache()
    {
        _logger.LogInformation("🗑️ Clearing navigation cache");

        var pattern = CacheKeys.NavigationPattern();
        _cacheService.RemoveByPattern(pattern);

        _logger.LogInformation("✅ Navigation cache cleared");

        return Ok(new
        {
            success = true,
            message = "Navigation cache cleared successfully",
            pattern = pattern,
            timestamp = DateTime.UtcNow
        });
    }

    /// <summary>
    /// Clear navigation cache for specific language
    /// </summary>
    /// <param name="lang">Language code</param>
    [HttpDelete("navigation/{lang}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public ActionResult ClearNavigationLanguageCache([FromRoute] string lang)
    {
        if (string.IsNullOrWhiteSpace(lang))
        {
            return BadRequest(new { error = "Language is required" });
        }

        _logger.LogInformation("🗑️ Clearing navigation cache for language: {Lang}", lang);

        var cacheKey = CacheKeys.Navigation(lang);
        _cacheService.Remove(cacheKey);

        _logger.LogInformation("✅ Navigation cache cleared for language: {Lang}", lang);

        return Ok(new
        {
            success = true,
            message = $"Navigation cache cleared for language: {lang}",
            cacheKey = cacheKey,
            timestamp = DateTime.UtcNow
        });
    }

    /// <summary>
    /// Warm cache (pre-load frequently accessed pages)
    /// </summary>
    /// <remarks>
    /// This endpoint manually triggers cache warming.
    /// Useful after clearing cache or deploying updates.
    /// </remarks>
    [HttpPost("warm")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult> WarmCache(
        [FromServices] IPageService pageService,
        [FromServices] INavigationService navigationService)
    {
        _logger.LogInformation("🔥 Manual cache warming triggered");

        var startTime = DateTime.UtcNow;
        var warmedCount = 0;
        var errors = new List<string>();

        var pagesToWarm = new[] { "home", "about", "programs", "contact" };
        var languages = new[] { "en", "mk" };

        // Warm navigation
        foreach (var lang in languages)
        {
            try
            {
                await navigationService.GetNavigationAsync(lang);
                warmedCount++;
                _logger.LogDebug("✅ Warmed navigation: {Lang}", lang);
            }
            catch (Exception ex)
            {
                errors.Add($"Navigation ({lang}): {ex.Message}");
                _logger.LogWarning(ex, "⚠️ Failed to warm navigation for {Lang}", lang);
            }
        }

        // Warm pages
        foreach (var slug in pagesToWarm)
        {
            foreach (var lang in languages)
            {
                try
                {
                    var page = await pageService.GetPageBySlugAsync(slug, lang);
                    if (page != null)
                    {
                        warmedCount++;
                        _logger.LogDebug("✅ Warmed page: {Slug} ({Lang})", slug, lang);
                    }
                }
                catch (Exception ex)
                {
                    errors.Add($"Page {slug} ({lang}): {ex.Message}");
                    _logger.LogWarning(ex, "⚠️ Failed to warm cache for {Slug} ({Lang})", slug, lang);
                }
            }
        }

        var duration = DateTime.UtcNow - startTime;

        _logger.LogInformation("🔥 Cache warming completed: {Count} entries in {Duration}ms",
            warmedCount, duration.TotalMilliseconds);

        return Ok(new
        {
            success = true,
            message = "Cache warming completed",
            warmedCount = warmedCount,
            duration = duration.TotalMilliseconds,
            errors = errors.Count > 0 ? errors : null,
            timestamp = DateTime.UtcNow
        });
    }

    /// <summary>
    /// Get cache statistics (if implemented)
    /// </summary>
    [HttpGet("stats")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public ActionResult GetCacheStats()
    {
        // Note: This is a placeholder. Implement actual stats tracking if needed.
        _logger.LogDebug("Cache stats requested");

        return Ok(new
        {
            message = "Cache statistics not yet implemented",
            note = "Consider implementing IMemoryCache statistics tracking",
            timestamp = DateTime.UtcNow
        });
    }
}
