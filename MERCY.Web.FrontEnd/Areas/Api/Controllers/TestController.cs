using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace MERCY.Web.FrontEnd.Areas.Api.Controllers
{
    public class TestController : Controller
    {
        public string Index()
        {
            string dateTime_FrontEnd = DateTime.Now.ToString("yyyy - MM - dd HH: mm: ss.fff");

            return "DateTime on<strong> FrontEnd</strong> is: " + dateTime_FrontEnd;
        }
    }
}