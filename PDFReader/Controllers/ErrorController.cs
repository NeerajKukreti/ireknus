using System.Web.Mvc;

namespace PDFReader.Controllers
{
    public class ErrorController : Controller
    {
        // GET: Error
        public ActionResult ServerError()
        {
            return View();
        }
        public ActionResult NotFound()
        {
            return View();
        }

    }
}