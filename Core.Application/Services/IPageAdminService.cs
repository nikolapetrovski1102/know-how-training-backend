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
        Task<PageAdminDto?> GetPageForEditAsync(int pageId);
        Task<bool> SavePageAsync(PageAdminDto page);
    }
}
