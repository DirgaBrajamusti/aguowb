using MERCY.Web.FrontEnd.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace MERCY.Web.FrontEnd.Areas.Api.Controllers
{
    public class SessionController : Controller
    {
        public string ClaimSession()
        {
            OurUtility.Transparent_Layer_to_BackEnd(Request, Response);

            return String.Empty;
        }

        public string CloseSession()
        {
            OurUtility.Transparent_Layer_to_BackEnd(Request, Response);

            return String.Empty;
        }
    }
}