using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Domain.Entities.CoreModels
{
    public class ContactSubmission
    {
        public int Id { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? Phone { get; set; }
        public string? CompanyName { get; set; }
        public string? CompanySize { get; set; }
        public string? ProgramInterest { get; set; }
        public string? Message { get; set; }
        public string? Language { get; set; }
        public string? IpAddress { get; set; }
        public string? UserAgent { get; set; }
        public DateTime SubmittedAt { get; set; }
        public bool IsRead { get; set; }
    }
}
