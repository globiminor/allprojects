using KapreoloKartenWebApp.Models;
using System.Web.Mvc;

namespace KapreoloKartenWebApp.Controllers
{
	public class HomeController : Controller
  {
    public ActionResult Index(string mapName)
    {
      return View(new MapModel { MapName = mapName});
    }

    public ActionResult About()
    {
      ViewBag.Message = "Your application description page.";

      return View();
    }

    public ActionResult Contact()
    {
      ViewBag.Message = "Your contact page.";

      return View();
    }
  }
}