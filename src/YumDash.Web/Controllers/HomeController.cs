using Microsoft.AspNetCore.Mvc;
using YumDash.Web.ViewModels;

namespace YumDash.Web.Controllers;

public class HomeController : Controller
{
    public IActionResult Index()
    {
        return View();
    }

    public IActionResult Error()
    {
        return View(new ErrorViewModel
        {
            RequestId = HttpContext.TraceIdentifier
        });
    }
}
