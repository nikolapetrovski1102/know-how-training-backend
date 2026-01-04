using Microsoft.AspNetCore.Mvc;
using Core.Application.DTOs;
using Infrastructure.Data.Services;
using Core.Application.Services;

namespace Web.API.Controllers;

[ApiController]
[Route("api")]
public class NavigationController : ControllerBase
{
    private readonly INavigationService _navigationService;
    private readonly ILogger<NavigationController> _logger;

    public NavigationController(
        INavigationService navigationService,
        ILogger<NavigationController> logger)
    {
        _navigationService = navigationService;
        _logger = logger;
    }

    /// <summary>
    /// Get navigation menu items for the specified language
    /// </summary>
    /// <param name="lang">Language code (e.g., 'en', 'mk', 'de')</param>
    /// <returns>List of navigation items</returns>
    [HttpGet("navigation")]
    [ProducesResponseType(typeof(NavigationDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<NavigationDto>> GetNavigation([FromQuery] string lang = "en")
    {
        try
        {
            if (string.IsNullOrWhiteSpace(lang) || lang.Length > 10)
            {
                return BadRequest(new { error = "Invalid language code" });
            }

            var navigation = await _navigationService.GetNavigationAsync(lang);

            if (navigation.Items.Count == 0)
            {
                _logger.LogWarning("No navigation items found for language {Language}", lang);
            }

            return Ok(navigation);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting navigation for language {Language}", lang);
            return StatusCode(500, new { error = "Failed to load navigation" });
        }
    }
}
