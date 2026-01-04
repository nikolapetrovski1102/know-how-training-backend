using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Application.DTOs
{
    public class NavigationDto
    {
        public List<NavItemDto> Items { get; set; } = new();
    }
}
