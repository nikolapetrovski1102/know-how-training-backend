using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Application.DTOs
{
    public class TrainerDto
    {
        public int Id { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string? Title { get; set; }
        public string? Bio { get; set; }
        public int? ExperienceYears { get; set; }
        public string? ImageUrl { get; set; }
        public string? LinkedinUrl { get; set; }
    }
}
