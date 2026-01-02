using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Application.DTOs
{
    public class PageDto
    {
        public string Slug { get; set; } = string.Empty;
        public PageContentDto Content { get; set; } = new();
        public List<ProgramDto> Programs { get; set; } = new();
        public List<TrainerDto> Trainers { get; set; } = new();
        public List<TestimonialDto> Testimonials { get; set; } = new();
        public List<CompanyReferenceDto> References { get; set; } = new();
        public List<GalleryItemDto> Gallery { get; set; } = new();
    }
}
