using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Application.DTOs
{
    public class SeoDto
    {
        public string? Title { get; set; }
        public string? Description { get; set; }
        public string? OpenGraphTitle { get; set; }
        public string? OpenGraphDescription { get; set; }
        public string? OpenGraphImage { get; set; }
    }
}
