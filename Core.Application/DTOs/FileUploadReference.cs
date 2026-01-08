using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Application.DTOs
{
    public class FileUploadReference
    {
        public string SectionType { get; set; }
        public int ItemIndex { get; set; }
        public string PropertyName { get; set; }
        public string FileUrl { get; set; }
    }
}
