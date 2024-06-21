using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

using MERCY.Web.FrontEnd.Helpers;

namespace MERCY.Web.FrontEnd.Controllers
{
    public class FilevController : Controller
    {
        public ActionResult Index()
        {
            UserInterface userInterface = new UserInterface(false);
            string ui_Folder_ServerSide = userInterface.Folder_ServerSide;
            string ui_Folder_Client_Side = userInterface.Folder_ClientSide;

            return View(ui_Folder_ServerSide + @"File.cshtml");
        }

        public ActionResult Form()
        {
            UserInterface userInterface = new UserInterface(false);
            string ui_Folder_ServerSide = userInterface.Folder_ServerSide;

            switch (OurUtility.ValueOf(Request, ".type"))
            {
                case OurUtility.UPLOAD_Sampling_ROM:
                    return View(ui_Folder_ServerSide + @"File_Preview_Sampling_ROM.cshtml");
                case OurUtility.UPLOAD_Geology_Pit_Monitoring:
                    return View(ui_Folder_ServerSide + @"File_Preview_Geology_Pit_Monitoring.cshtml");
                case OurUtility.UPLOAD_Geology_Explorasi:
                    return View(ui_Folder_ServerSide + @"File_Preview_Geology_Explorasi.cshtml");
                case OurUtility.UPLOAD_BARGE_LOADING:
                    return View(ui_Folder_ServerSide + @"File_Preview_Barge_Loading.cshtml");
                case OurUtility.UPLOAD_CRUSHING_PLANT:
                    return View(ui_Folder_ServerSide + @"File_Preview_Crushing_Plant.cshtml");
                case OurUtility.UPLOAD_BargeLineUp:
                    return View(ui_Folder_ServerSide + @"File_Preview_Barge_LineUp.cshtml");
                case OurUtility.UPLOAD_BargeQualityPlan:
                    return View(ui_Folder_ServerSide + @"File_Preview_Barge_QualityPlan.cshtml");
                case OurUtility.UPLOAD_SampleDetail:
                    return View(ui_Folder_ServerSide + @"File_Preview_Sample_Detail.cshtml");
                case OurUtility.UPLOAD_HAC:
                    return View(ui_Folder_ServerSide + @"File_Preview_HAC.cshtml");
                default:
                    return View(ui_Folder_ServerSide + @"File_Preview_Sampling_ROM.cshtml");
            }
        }
    }
}