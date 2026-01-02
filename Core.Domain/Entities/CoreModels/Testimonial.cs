using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Domain.Entities.CoreModels
{
    public class Testimonial
    {
        public int Id { get; set; }
        public string? AuthorImageUrl { get; set; }
        public string? AuthorPosition { get; set; }
        public string? CompanyLogoUrl { get; set; }
        public int SortOrder { get; set; }
        public bool IsPublished { get; set; }
    }
}
