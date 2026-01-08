using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Core.Application.DTOs;

namespace Core.Application.Services
{
    public interface IPageAdminService
    {
        Task<List<PageSummaryDto>> GetAllPagesAsync();
        Task<bool> SavePageAsync(PageAdminDto page);
        Task<PageAdminDto?> GetPageForEditAsync(int id, string lang);
        Task<PageAdminLanguageDto?> CreatePageLanguageFromTemplateAsync(int pageId, string targetLanguageCode, string sourceLanguageCode = "en");
        Task<bool> SavePageLanguageAsync(int pageId, string slug, PageAdminLanguageDto langContent);
        void ApplyChanges(PageAdminLanguageDto langContent, List<EditChange> changes);
    }
}
