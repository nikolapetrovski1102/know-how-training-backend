using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Application.DTOs
{
    public class PageContentDto
    {
        public SeoDto Seo { get; set; } = new();
        public HeroDto? Hero { get; set; }
        public string Sections { get; set; }
        public OpenGraphDto OpenGraph { get; set; } = new();
    }
}
