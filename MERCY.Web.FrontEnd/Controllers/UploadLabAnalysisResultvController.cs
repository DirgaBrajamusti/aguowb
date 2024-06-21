using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

using MERCY.Web.FrontEnd.Helpers;

namespace MERCY.Web.FrontEnd.Controllers
{
    public class UploadLabAnalysisResultvController : Controller
    {
        public ActionResult Index()
        {
            UserInterface userInterface = new UserInterface(false);
            string ui_Folder_ServerSide = userInterface.Folder_ServerSide;
            string ui_Folder_Client_Side = userInterface.Folder_ClientSide;

            switch (OurUtility.ValueOf(Request, ".type"))
            {
                case OurUtility.UPLOAD_Sampling_ROM:
                    return View(ui_Folder_ServerSide + @"UploadLabAnalysisResult_Sampling_ROM.cshtml");
                case OurUtility.UPLOAD_Geology_Pit_Monitoring:
                    return View(ui_Folder_ServerSide + @"UploadLabAnalysisResult_Geology_Pit_Monitoring.cshtml");
                case OurUtility.UPLOAD_Geology_Explorasi:
                    return View(ui_Folder_ServerSide + @"UploadLabAnalysisResult_Geology_Explorasi.cshtml");
                case OurUtility.UPLOAD_BARGE_LOADING:
                    return View(ui_Folder_ServerSide + @"UploadLabAnalysisResult_Barge_Loading.cshtml");
                case OurUtility.UPLOAD_CRUSHING_PLANT:
                    return View(ui_Folder_ServerSide + @"UploadLabAnalysisResult_Crushing_Plant.cshtml");
                case OurUtility.UPLOAD_HAC:
                    return View(ui_Folder_ServerSide + @"UploadLabAnalysisResult_HAC.cshtml");
            }

            return View(ui_Folder_ServerSide + @"UploadLabAnalysisResult_Sampling_ROM.cshtml");
        }
    }
}