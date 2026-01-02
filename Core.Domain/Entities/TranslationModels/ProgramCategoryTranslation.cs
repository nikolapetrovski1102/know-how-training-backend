using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Domain.Entities.TranslationModels
{
    public class ProgramCategoryTranslation
    {
        public int Id { get; set; }
        public int CategoryId { get; set; }
        public byte LanguageId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
    }
}
