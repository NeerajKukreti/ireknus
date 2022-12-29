using PDFReader.Models;
using System.Web.Mvc;

namespace PDFReader.Controllers
{
    [Authorize]
    public class DashboardController : Controller
    {
        // GET: Dashboard
        public ActionResult Index()
        {
            return View("Dashboard", new DashboardModel());
        }
    }
}