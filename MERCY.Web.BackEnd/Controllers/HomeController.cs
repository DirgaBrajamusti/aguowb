using System;
using System.Web.Mvc;

using MERCY.Web.BackEnd.Helpers;

namespace MERCY.Web.BackEnd.Controllers
{
    public class HomeController : Controller
    {
        // by Default, tampilkan saja "Version: FrontEnd"
        public string Index()
        {
            return "MERCY, <strong>BackEnd</strong> : " + Configuration.VERSION + ".<br/><br/>DateTime on Server is: " + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");
        }
    }
}