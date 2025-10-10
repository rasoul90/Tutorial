using Microsoft.AspNetCore.Mvc;

namespace HRGov.HRApp.Controllers;

public class HomeController : Controller
{
    public IActionResult Error()
    {
        return View();
    }
}
