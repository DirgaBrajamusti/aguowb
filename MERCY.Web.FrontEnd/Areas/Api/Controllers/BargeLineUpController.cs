using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

using MERCY.Web.FrontEnd.Helpers;

namespace MERCY.Web.FrontEnd.Areas.Api.Controllers
{
    public class BargeLineUpController : Controller
    {
        public string Index()
        {
            OurUtility.Transparent_Layer_to_BackEnd(Request, Response);

            return string.Empty;
        }

        public string ChooseFile()
        {
            OurUtility.Transparent_Layer_to_BackEnd(Request, Response);

            return string.Empty;
        }

        public string ParsingContent()
        {
            OurUtility.Transparent_Layer_to_BackEnd(Request, Response);

            return string.Empty;
        }

        public string DisplayContent()
        {
            OurUtility.Transparent_Layer_to_BackEnd(Request, Response);

            return string.Empty;
        }

        public string Save()
        {
            OurUtility.Transparent_Layer_to_BackEnd(Request, Response);

            return string.Empty;
        }

        public string File()
        {
            OurUtility.Transparent_Layer_to_BackEnd(Request, Response);

            return string.Empty;
        }

        public string GetByVessel()
        {
            OurUtility.Transparent_Layer_to_BackEnd(Request, Response);

            return string.Empty;
        }
    }
}