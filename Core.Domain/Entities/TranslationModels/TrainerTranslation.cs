using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Domain.Entities.TranslationModels
{
    public class TrainerTranslation
    {
        public int Id { get; set; }
        public int TrainerId { get; set; }
        public byte LanguageId { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string? Title { get; set; }
        public string? Bio { get; set; }
        public int? ExperienceYears { get; set; }
    }
}
