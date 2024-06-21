using MERCY.Web.FrontEnd.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace MERCY.Web.FrontEnd.Areas.Api.Controllers
{
    public class DataEntryController : Controller
    {
        public string Index()
        {
            OurUtility.Transparent_Layer_to_BackEnd(Request, Response);

            return String.Empty;
        }

        public string DataEntry()
        {
            OurUtility.Transparent_Layer_to_BackEnd(Request, Response);
            
            return String.Empty;
        }

        public string GetAvailableSchemes()
        {
            OurUtility.Transparent_Layer_to_BackEnd(Request, Response);
            
            return String.Empty;
        }

        public string GetAnalysisResults()
        {
            OurUtility.Transparent_Layer_to_BackEnd(Request, Response);

            return String.Empty;
        }

        public string UpdateAnalysisResult()
        {
            OurUtility.Transparent_Layer_to_BackEnd(Request, Response);

            return String.Empty;
        }

        public string GetCompletedJob()
        {
            OurUtility.Transparent_Layer_to_BackEnd(Request, Response);

            return String.Empty;
        }
    }
}