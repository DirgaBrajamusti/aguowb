using MERCY.Web.FrontEnd.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace MERCY.Web.FrontEnd.Controllers
{
    public class SampleValidationApprovalvController : Controller
    {
        public ActionResult Index()
        {
            UserInterface userInterface = new UserInterface(false);
            string ui_Folder_ServerSide = userInterface.Folder_ServerSide;
            string ui_Folder_Client_Side = userInterface.Folder_ClientSide;

            return View(ui_Folder_ServerSide + @"SampleValidationApproval.cshtml");
        }
        public ActionResult ViewPage()
        {
            UserInterface userInterface = new UserInterface(false);
            string ui_Folder_ServerSide = userInterface.Folder_ServerSide;
            string ui_Folder_Client_Side = userInterface.Folder_ClientSide;

            return View(ui_Folder_ServerSide + @"SampleValidationApproval_ViewPage.cshtml");
        }

        public ActionResult ValidationPage()
        {
            UserInterface userInterface = new UserInterface(false);
            string ui_Folder_ServerSide = userInterface.Folder_ServerSide;
            string ui_Folder_Client_Side = userInterface.Folder_ClientSide;

            return View(ui_Folder_ServerSide + @"SampleValidationApproval_ValidationPage.cshtml");
        }
        public ActionResult ApprovalPage()
        {
            UserInterface userInterface = new UserInterface(false);
            string ui_Folder_ServerSide = userInterface.Folder_ServerSide;
            string ui_Folder_Client_Side = userInterface.Folder_ClientSide;

            return View(ui_Folder_ServerSide + @"SampleValidationApproval_ApprovalPage.cshtml");
        }
    }
}