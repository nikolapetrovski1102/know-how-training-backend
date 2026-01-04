using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using Core.Application.Services;

namespace Infrastructure.Services;

public class CacheWarmerService : IHostedService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<CacheWarmerService> _logger;

    public CacheWarmerService(
        IServiceProvider serviceProvider,
        ILogger<CacheWarmerService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("🔥 Starting cache warming...");

        try
        {
            using var scope = _serviceProvider.CreateScope();

            var pageService = scope.ServiceProvider.GetRequiredService<IPageService>();
            var navigationService = scope.ServiceProvider.GetRequiredService<INavigationService>();

            // ✅ Define pages to warm (most visited)
            var pagesToWarm = new[]
            {
                "home",
                "about",
                "programs",
                "contact"
            };

            // ✅ Define languages to warm
            var languages = new[] { "en", "mk" };

            var startTime = DateTime.UtcNow;
            var warmedCount = 0;

            // ✅ Warm navigation cache for all languages
            foreach (var lang in languages)
            {
                if (cancellationToken.IsCancellationRequested) break;

                try
                {
                    await navigationService.GetNavigationAsync(lang);
                    warmedCount++;
                    _logger.LogDebug("✅ Warmed navigation cache: {Language}", lang);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "⚠️ Failed to warm navigation cache for {Language}", lang);
                }
            }

            // ✅ Warm page cache for all languages
            foreach (var slug in pagesToWarm)
            {
                if (cancellationToken.IsCancellationRequested) break;

                foreach (var lang in languages)
                {
                    try
                    {
                        var page = await pageService.GetPageBySlugAsync(slug, lang);
                        if (page != null)
                        {
                            warmedCount++;
                            _logger.LogDebug("✅ Warmed page cache: {Slug} ({Language})", slug, lang);
                        }
                        else
                        {
                            _logger.LogDebug("⏭️ Skipped (not found): {Slug} ({Language})", slug, lang);
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "⚠️ Failed to warm cache for {Slug} ({Language})", slug, lang);
                    }
                }
            }

            var duration = DateTime.UtcNow - startTime;
            _logger.LogInformation("🔥 Cache warming completed: {Count} entries in {Duration}ms",
                warmedCount, duration.TotalMilliseconds);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Cache warming failed");
        }
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Cache warmer service stopped");
        return Task.CompletedTask;
    }
}
