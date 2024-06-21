using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

using System.Net;

using MERCY.Web.FrontEnd.Helpers;

namespace MERCY.Web.FrontEnd.Controllers
{
    public class VersionController : Controller
    {
        // Version: FrontEnd
        public string Index()
        {
            return "MERCY, <strong>FrontEnd</strong> : " + Configuration.VERSION + ".<br/><br/>DateTime on Server is: " + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");
        }
    }
}