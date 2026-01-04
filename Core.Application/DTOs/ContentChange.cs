using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Application.DTOs
{
    public class ContentChange
    {
        public string Path { get; set; } = string.Empty;
        public string Original { get; set; } = string.Empty;
        public string Edited { get; set; } = string.Empty;
    }
}
