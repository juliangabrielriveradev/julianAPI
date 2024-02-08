


using Microsoft.AspNetCore.Mvc;

namespace julianapi.Controllers
{
    
    [ApiController]
    [Route("[controller]")]
   
    public class HomeController : Controller
    {
         [HttpGet]
        public IActionResult Index(){
            return View();
        }

    }
    }