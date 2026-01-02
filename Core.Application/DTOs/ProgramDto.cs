using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Application.DTOs
{
    public class ProgramDto
    {
        public int Id { get; set; }
        public string Slug { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string? ShortDescription { get; set; }
        public string? ImageUrl { get; set; }
        public string? PdfOutlineUrl { get; set; }
        public decimal? DurationHours { get; set; }
        public int? MaxParticipants { get; set; }
        public decimal? PriceRangeMin { get; set; }
        public decimal? PriceRangeMax { get; set; }
        public List<TrainerDto> Trainers { get; set; } = new();
    }
}
