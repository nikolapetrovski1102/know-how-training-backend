using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Application.DTOs
{
    public class PageUpdateRequestDto
    {
        public int PageId { get; set; }
        public string Language { get; set; } = string.Empty;
        public List<ContentChange> Changes { get; set; } = new();
    }

}
