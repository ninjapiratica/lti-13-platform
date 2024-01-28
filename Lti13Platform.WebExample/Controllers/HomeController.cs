global using NP.Lti13Platform.Core;
using Microsoft.AspNetCore.Mvc;

namespace NP.Lti13Platform.WebExample.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }
    }
}
