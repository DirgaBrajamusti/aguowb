using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

using MERCY.Web.FrontEnd.Helpers;

namespace MERCY.Web.FrontEnd.Areas.Api.Controllers
{
    public class Upload_LECOController : Controller
    {
        public string CV()
        {
            OurUtility.Transparent_Layer_to_BackEnd(Request, Response);

            return string.Empty;
        }

        public string TS()
        {
            OurUtility.Transparent_Layer_to_BackEnd(Request, Response);

            return string.Empty;
        }

        public string TGA()
        {
            OurUtility.Transparent_Layer_to_BackEnd(Request, Response);

            return string.Empty;
        }
    }
}