using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Domain.Entities.TranslationModels
{
    public class GalleryItemTranslation
    {
        public int Id { get; set; }
        public int GalleryItemId { get; set; }
        public byte LanguageId { get; set; }
        public string? Caption { get; set; }
    }
}
