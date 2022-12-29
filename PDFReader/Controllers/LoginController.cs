using Dapper;
using PDFReader.Models;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using System.Web.Security;

namespace PDFReader.Controllers
{
    public class OverridableAuthorize : AuthorizeAttribute
    {
        public override void OnAuthorization(AuthorizationContext filterContext)
        {
            var authorized = base.AuthorizeCore(filterContext.HttpContext);
            if (!authorized)
            {
                // The user is not authorized => no need to go any further
            }
            base.OnAuthorization(filterContext);
        }
    }

    public class LoginController : Controller
    {
        // GET: Login
        public ActionResult Index()
        {
            return View("Login", new LoginModel());
        }

        [HttpPost]
        public async Task<ActionResult> Index(LoginModel Login)
        {
            using (IDbConnection db = new SqlConnection(Connection.MyConnection()))
            {
                DynamicParameters ob = new DynamicParameters();
                ob.Add("USER_CONTACT", Login.USER_CONTACT);
                ob.Add("USER_PASSWORD", Login.USER_PASSWORD);

                var xx = await db.QueryAsync<LoginModel>("SP_LOGIN", ob, commandType: CommandType.StoredProcedure);
                Login = xx.FirstOrDefault();

                if (Login.StatusCode > 0)
                {
                    FormsAuthentication.SetAuthCookie(Login.USER_CONTACT, true);
                    Session.Add("User", Login);
                    Session.Add("Role", "User");

                    return RedirectToAction("index", "Dashboard");
                }
                else
                {
                    return View("Login", Login);
                }
            }
        }
    }
}