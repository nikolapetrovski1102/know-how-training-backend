using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Application.DTOs
{
    public class PageDto
    {
        public int PageId { get; set; }
        public string Slug { get; set; } = string.Empty;
        public bool IsPublished { get; set; }
        public string LanguageCode { get; set; } = string.Empty;
        public PageContentDto Content { get; set; } = new();
        public List<ProgramDto> Programs { get; set; } = new();
    }
}
