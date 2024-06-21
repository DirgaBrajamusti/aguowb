﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

using MERCY.Web.FrontEnd.Helpers;

namespace MERCY.Web.FrontEnd.Controllers
{
    public class UnitMeasurementsvController : Controller
    {
        public ActionResult Index()
        {
            UserInterface userInterface = new UserInterface(false);
            string ui_Folder_ServerSide = userInterface.Folder_ServerSide;
            string ui_Folder_Client_Side = userInterface.Folder_ClientSide;

            return View(ui_Folder_ServerSide + @"UnitMeasurements.cshtml");
        }

        public ActionResult Bulk()
        {
            UserInterface userInterface = new UserInterface(false);
            string ui_Folder_ServerSide = userInterface.Folder_ServerSide;
            string ui_Folder_Client_Side = userInterface.Folder_ClientSide;

            return View(ui_Folder_ServerSide + @"UnitMeasurements_Bulk.cshtml");
        }
    }
}