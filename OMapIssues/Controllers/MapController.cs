using System.Web.Mvc;

namespace OMapIssues.Controllers
{
  public class MapController : Controller
  {
    public ActionResult Navigate()
    {
      ViewBag.Message = "Your application description page.";

      return View();
    }

  }
}