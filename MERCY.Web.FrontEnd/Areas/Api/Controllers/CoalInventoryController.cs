using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using MERCY.Web.FrontEnd.Helpers;

namespace MERCY.Web.FrontEnd.Areas.Api.Controllers
{
    public class CoalInventoryController : Controller
    {
        public string Index()
        {
            OurUtility.Transparent_Layer_to_BackEnd(Request, Response);
            
            return String.Empty;
        }

        public string Create()
        {
            OurUtility.Transparent_Layer_to_BackEnd(Request, Response);

            return String.Empty;
        }

        public string Edit()
        {
            OurUtility.Transparent_Layer_to_BackEnd(Request, Response);
            
            return String.Empty;
        }

        public string GetRomLocation()
        {
            OurUtility.Transparent_Layer_to_BackEnd(Request, Response);

            return String.Empty;
        }

        public string GetRomLocationDetail()
        {
            OurUtility.Transparent_Layer_to_BackEnd(Request, Response);
            
            return String.Empty;
        }

        public string GetById()
        {
            OurUtility.Transparent_Layer_to_BackEnd(Request, Response);
            
            return String.Empty;
        }

        public string Attachment()
        {
            OurUtility.Transparent_Layer_to_BackEnd(Request, Response);
            
            return String.Empty;
        }

        public string Finalize()
        {
            OurUtility.Transparent_Layer_to_BackEnd(Request, Response);

            return String.Empty;
        }
    }
}