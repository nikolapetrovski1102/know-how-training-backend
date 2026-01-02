using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Domain.Entities.TranslationModels
{
    public class TestimonialTranslation
    {
        public int Id { get; set; }
        public int TestimonialId { get; set; }
        public byte LanguageId { get; set; }
        public string Quote { get; set; } = string.Empty;
        public string AuthorName { get; set; } = string.Empty;
    }
}
