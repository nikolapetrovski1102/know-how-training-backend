using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Domain.Entities.CoreModels
{
    public class Page
    {
        public int Id { get; set; }
        public string Slug { get; set; } = string.Empty;
        public int SortOrder { get; set; }
        public bool IsPublished { get; set; }
        public bool IsMenuItem { get; set; }
        public int? MenuSortOrder { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}
