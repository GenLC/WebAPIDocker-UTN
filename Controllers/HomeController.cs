using Microsoft.AspNetCore.Mvc;

namespace WebAPIDockerUTN.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        [Route("/Home/Error")]
        public IActionResult Error()
        {
            var requestId = HttpContext.TraceIdentifier;
            ViewData["RequestId"] = requestId;
            return View();
        }


    }
}
