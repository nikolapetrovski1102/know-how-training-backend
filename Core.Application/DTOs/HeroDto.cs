using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Application.DTOs
{
    public class HeroDto
    {
        public string? Title { get; set; }
        public string? Subtitle { get; set; }
        public string? CtaText { get; set; }
        public string? CtaUrl { get; set; }
        public string? BackgroundImage { get; set; }
    }
}
