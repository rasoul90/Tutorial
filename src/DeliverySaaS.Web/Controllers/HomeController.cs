using Microsoft.AspNetCore.Mvc;

namespace DeliverySaaS.Web.Controllers;

public class HomeController : Controller
{
    public IActionResult Index()
    {
        var token = HttpContext.Session.GetString("access_token") ?? Request.Cookies["access_token"];
        if (string.IsNullOrWhiteSpace(token))
        {
            return RedirectToAction("Login", "Account");
        }

        return View();
    }
}
