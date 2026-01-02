using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Domain.Entities.TranslationModels
{
    public class ProgramTranslation
    {
        public int Id { get; set; }
        public int ProgramId { get; set; }
        public byte LanguageId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? ShortDescription { get; set; }
        public string? FullDescription { get; set; }
        public string? TargetAudience { get; set; }
        public string? LearningOutcomes { get; set; }
    }
}
