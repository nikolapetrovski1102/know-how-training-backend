using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Domain.Entities.CoreModels
{
    public class DownloadItem
    {
        public int Id { get; set; }
        public string FileUrl { get; set; } = string.Empty;
        public int? FileSizeKb { get; set; }
        public int SortOrder { get; set; }
        public bool IsPublished { get; set; }
    }
}
