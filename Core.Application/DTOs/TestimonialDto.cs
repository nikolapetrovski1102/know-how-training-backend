using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Application.DTOs
{
    public class TestimonialDto
    {
        public int Id { get; set; }
        public string Quote { get; set; } = string.Empty;
        public string AuthorName { get; set; } = string.Empty;
        public string? AuthorPosition { get; set; }
        public string? AuthorImageUrl { get; set; }
        public string? CompanyLogoUrl { get; set; }
    }
}
