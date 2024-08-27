using System.Web.Mvc;

namespace MERCY.Web.BackEnd.Controllers
{
    public class VersionxController : Controller
    {
        // Version: BackEnd
        public string Index()
        {
            return (new HomeController()).Index();
        }
    }
}