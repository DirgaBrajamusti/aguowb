using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

using MERCY.Web.FrontEnd.Helpers;

namespace MERCY.Web.FrontEnd.Controllers
{
    public class LoadingRequestvController : Controller
    {
        public ActionResult Index()
        {
            UserInterface userInterface = new UserInterface(false);
            string ui_Folder_ServerSide = userInterface.Folder_ServerSide;
            string ui_Folder_Client_Side = userInterface.Folder_ClientSide;

            return View(ui_Folder_ServerSide + @"LoadingRequest.cshtml");
        }

        public ActionResult Index2()
        {
            UserInterface userInterface = new UserInterface(false);
            string ui_Folder_ServerSide = userInterface.Folder_ServerSide;
            string ui_Folder_Client_Side = userInterface.Folder_ClientSide;

            return View(ui_Folder_ServerSide + @"LoadingRequest2.cshtml");
        }

        public ActionResult Form()
        {
            UserInterface userInterface = new UserInterface(false);
            string ui_Folder_ServerSide = userInterface.Folder_ServerSide;
            string ui_Folder_Client_Side = userInterface.Folder_ClientSide;

            return View(ui_Folder_ServerSide + @"LoadingRequest_Form.cshtml");
        }

        public ActionResult Preview()
        {
            UserInterface userInterface = new UserInterface(false);
            string ui_Folder_ServerSide = userInterface.Folder_ServerSide;
            string ui_Folder_Client_Side = userInterface.Folder_ClientSide;

            return View(ui_Folder_ServerSide + @"LoadingRequest_Preview.cshtml");
        }
    }
}