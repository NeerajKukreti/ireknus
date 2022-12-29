using System.Collections.Generic;
using System.Web.Mvc;

namespace PDFReader.Controllers
{
    public class abc
    {
        public string xx { get; set; }
    }

    public class TestController : Controller
    {
        public ActionResult Index()
        {
            viewModel viewModel = new viewModel
            {
                abcs = new List<abc> { new abc { xx = "1" }, new abc { xx = "2" } }
            };
            //return View(viewModel);
            return View(new List<abc> { new abc { xx = "1" }, new abc { xx = "2" } });
        }

        public ActionResult Index1(List<abc> list)
        {
            return View();
        }
    }

    public class viewModel
    {
        public List<abc> abcs { get; set; }
    }
}