using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Domain.Entities.CoreModels
{
    public class CompanyReference
    {
        public int Id { get; set; }
        public string LogoUrl { get; set; } = string.Empty;
        public string? WebsiteUrl { get; set; }
        public int SortOrder { get; set; }
        public bool IsPublished { get; set; }
    }
}
