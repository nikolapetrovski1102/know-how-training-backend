using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Domain.Entities.TranslationModels
{
    public class CompanyReferenceTranslation
    {
        public int Id { get; set; }
        public int CompanyId { get; set; }
        public byte LanguageId { get; set; }
        public string CompanyName { get; set; } = string.Empty;
    }
}
