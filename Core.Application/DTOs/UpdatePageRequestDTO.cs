using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Application.DTOs
{
    public class UpdatePageRequestDTO
    {
        public int? PageId { get; set; }
        public string Language { get; set; } = "en";
        public List<EditChange> Changes { get; set; } = new();
    }
}
