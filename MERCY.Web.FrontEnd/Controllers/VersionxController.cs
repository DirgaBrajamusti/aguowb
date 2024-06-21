using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

using MERCY.Web.FrontEnd.Helpers;

namespace MERCY.Web.FrontEnd.Controllers
{
    public class VersionxController : Controller
    {
        // Version: ambil nilai dari BackEnd
        public string Index()
        {
            OurUtility.Transparent_Layer_to_BackEnd(Request, Response);

            // dummy statement, just to make function "Happy" with return statement
            return string.Empty;
        }
    }
}