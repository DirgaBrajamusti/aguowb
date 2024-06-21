using MERCY.Web.FrontEnd.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace MERCY.Web.FrontEnd.Controllers
{
    public class SampleAnalysisvController : Controller
    {
        public ActionResult Index()
        {
            UserInterface userInterface = new UserInterface(false);
            string ui_Folder_ServerSide = userInterface.Folder_ServerSide;
            string ui_Folder_Client_Side = userInterface.Folder_ClientSide;

            return View(ui_Folder_ServerSide + @"SampleAnalysis.cshtml");
        }

        public ActionResult InputAnalysis()
        {
            UserInterface userInterface = new UserInterface(false);
            string ui_Folder_ServerSide = userInterface.Folder_ServerSide;
            string ui_Folder_Client_Side = userInterface.Folder_ClientSide;

            return View(ui_Folder_ServerSide + @"InputAnalysis.cshtml");
        }

        public ActionResult InputAnalysisExpand()
        {
            UserInterface userInterface = new UserInterface(false);
            string ui_Folder_ServerSide = userInterface.Folder_ServerSide;
            string ui_Folder_Client_Side = userInterface.Folder_ClientSide;

            return View(ui_Folder_ServerSide + @"InputAnalysisExpand.cshtml");
        }
    }
}