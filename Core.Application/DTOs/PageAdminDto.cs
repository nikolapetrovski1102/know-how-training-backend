using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Application.DTOs
{
    public class PageAdminDto
    {
        public int PageId { get; set; }
        public string Slug { get; set; } = string.Empty;
        public bool IsPublished { get; set; }
        public bool IsMenuItem { get; set; }
        public int? MenuSortOrder { get; set; }

        public List<PageAdminLanguageDto> Languages { get; set; } = new();
    }
}
