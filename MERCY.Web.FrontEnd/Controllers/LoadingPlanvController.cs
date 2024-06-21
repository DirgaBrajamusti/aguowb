using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

using MERCY.Web.FrontEnd.Helpers;

namespace MERCY.Web.FrontEnd.Controllers
{
    public class LoadingPlanvController : Controller
    {
        public ActionResult Index()
        {
            UserInterface userInterface = new UserInterface(false);
            string ui_Folder_ServerSide = userInterface.Folder_ServerSide;
            string ui_Folder_Client_Side = userInterface.Folder_ClientSide;

            return View(ui_Folder_ServerSide + @"LoadingPlan.cshtml");
        }

        public ActionResult Index2()
        {
            UserInterface userInterface = new UserInterface(false);
            string ui_Folder_ServerSide = userInterface.Folder_ServerSide;
            string ui_Folder_Client_Side = userInterface.Folder_ClientSide;

            return View(ui_Folder_ServerSide + @"LoadingPlan2.cshtml");
        }
        public ActionResult Form()
        {
            UserInterface userInterface = new UserInterface(false);
            string ui_Folder_ServerSide = userInterface.Folder_ServerSide;
            string ui_Folder_Client_Side = userInterface.Folder_ClientSide;

            return View(ui_Folder_ServerSide + @"LoadingPlan_Form.cshtml");
        }

        public ActionResult Form2()
        {
            UserInterface userInterface = new UserInterface(false);
            string ui_Folder_ServerSide = userInterface.Folder_ServerSide;
            string ui_Folder_Client_Side = userInterface.Folder_ClientSide;

            return View(ui_Folder_ServerSide + @"LoadingPlan_Form2.cshtml");
        }
    }
}