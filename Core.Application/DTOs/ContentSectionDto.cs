using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Application.DTOs
{
    public class ContentSectionDto
    {
        public string Type { get; set; } = string.Empty; // "programs", "stats", "testimonials", etc.
        public string? Title { get; set; }
        public string? Subtitle { get; set; }
        public object? Items { get; set; } // ProgramSummaryDto[], StatDto[], etc.
    }
}
