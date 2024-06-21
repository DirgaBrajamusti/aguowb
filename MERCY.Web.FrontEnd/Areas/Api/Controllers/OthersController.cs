using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

using MERCY.Web.FrontEnd.Helpers;

namespace MERCY.Web.FrontEnd.Areas.Api.Controllers
{
    public class OthersController : Controller
    {
        public ActionResult Index()
        {
            string view = Request[".v"];
            view = view.Replace(@"/", "").Replace(@"\", "");

            return View(view);
        }
    }
}