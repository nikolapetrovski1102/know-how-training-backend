using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Domain.Entities.TranslationModels
{
    public class DownloadItemTranslation
    {
        public int Id { get; set; }
        public int DownloadId { get; set; }
        public byte LanguageId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
    }
}
