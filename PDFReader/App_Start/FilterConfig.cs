using System.Web.Mvc;

namespace PDFReader
{
    public class FilterConfig
    {
        public static void RegisterGlobalFilters(GlobalFilterCollection filters)
        {
            // some new changes added
            filters.Add(new HandleErrorAttribute());
        }
    }
}