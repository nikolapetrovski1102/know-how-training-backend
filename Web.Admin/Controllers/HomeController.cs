using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Web.Admin.Models;
using Core.Application;
using Core.Application.Services;

namespace Web.Admin.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IPageService _pageService;

        public HomeController(ILogger<HomeController> logger, IPageService pageService)
        {
            _logger = logger;
            _pageService = pageService;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
