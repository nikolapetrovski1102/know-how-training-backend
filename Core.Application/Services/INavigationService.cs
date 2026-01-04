using Core.Application.DTOs;

namespace Core.Application.Services
{
    public interface INavigationService
    {
        Task<NavigationDto> GetNavigationAsync(string languageCode);
    }
}
