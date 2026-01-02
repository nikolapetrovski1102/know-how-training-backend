using Core.Application.DTOs;
using Core.Domain.Entities.CoreModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Application.Services
{
    public interface IPageService
    {
        Task<PageDto?> GetPageBySlugAsync(string slug, string languageCode);
        Task SeedHomePageAsync();
    }
}
