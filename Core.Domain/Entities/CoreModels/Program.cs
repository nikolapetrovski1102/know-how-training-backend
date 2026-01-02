using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Domain.Entities.CoreModels
{
    public class Program
    {
        public int Id { get; set; }
        public int? CategoryId { get; set; }
        public string Slug { get; set; } = string.Empty;
        public decimal? DurationHours { get; set; }
        public int? MaxParticipants { get; set; }
        public decimal? PriceRangeMin { get; set; }
        public decimal? PriceRangeMax { get; set; }
        public string? ImageUrl { get; set; }
        public string? PdfOutlineUrl { get; set; }
        public int SortOrder { get; set; }
        public bool IsPublished { get; set; }
    }
}
