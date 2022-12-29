using System.Web.Mvc;

namespace PDFReader.Controllers
{
    public class ProfileController : Controller
    {
        // GET: Profile
        [Authorize]
        public ActionResult Index()
        {
            return View("Profile");
        }
    }
}