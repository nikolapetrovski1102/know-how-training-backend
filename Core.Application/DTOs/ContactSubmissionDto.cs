using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Application.DTOs
{
    public class ContactSubmissionDto
    {
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? Phone { get; set; }
        public string? CompanyName { get; set; }
        public string? CompanySize { get; set; }
        public string? ProgramInterest { get; set; }
        public string? Message { get; set; }
        public string Language { get; set; } = "en";
    }
}
