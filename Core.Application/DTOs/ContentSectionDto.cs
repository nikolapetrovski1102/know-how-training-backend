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
        public object? Items { get; set; } // Can be List<object> or JsonElement
        public string Greeting { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Layout { get; set; }
        public string BackgroundColor { get; set; }
        public int Columns { get; set; }
        public string? ImageUrl { get; set; }
        public string? Alignment { get; set; }
        public int? StyleId { get; set; }
    }
}
