using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

using MERCY.Web.FrontEnd.Helpers;

namespace MERCY.Web.FrontEnd.Areas.Api.Controllers
{
    public class UserController : Controller
    {
        public string Index()
        {
            OurUtility.Transparent_Layer_to_BackEnd(Request, Response);

            return string.Empty;
        }

        [AllowAnonymous, HttpPost]
        public string Login(FormCollection p_collection)
        {
            OurUtility.Transparent_Layer_to_BackEnd(Request, Response);

            return string.Empty;
        }

        [AllowAnonymous, HttpGet]
        public string Login()
        {
            OurUtility.Transparent_Layer_to_BackEnd(Request, Response);

            return string.Empty;
        }

        public string Info()
        {
            OurUtility.Transparent_Layer_to_BackEnd(Request, Response);

            return string.Empty;
        }

        public string Logout()
        {
            OurUtility.Transparent_Layer_to_BackEnd(Request, Response);

            return string.Empty;
        }

        public ActionResult Picture(string id)
        {
            OurUtility.Transparent_Layer_to_BackEnd(Request, Response);

            return null;
        }

        public ActionResult Activities()
        {
            OurUtility.Transparent_Layer_to_BackEnd(Request, Response);

            return null;
        }

        public string Get_ddl()
        {
            OurUtility.Transparent_Layer_to_BackEnd(Request, Response);

            return string.Empty;
        }

        public string GetM_ddl()
        {
            OurUtility.Transparent_Layer_to_BackEnd(Request, Response);

            return string.Empty;
        }

        public string GetMtn_ddl()
        {
            OurUtility.Transparent_Layer_to_BackEnd(Request, Response);

            return string.Empty;
        }

        public string Create()
        {
            OurUtility.Transparent_Layer_to_BackEnd(Request, Response);

            return string.Empty;
        }

        public string Edit()
        {
            OurUtility.Transparent_Layer_to_BackEnd(Request, Response);

            return string.Empty;
        }

        public string Delete()
        {
            OurUtility.Transparent_Layer_to_BackEnd(Request, Response);

            return string.Empty;
        }

        public string Get()
        {
            OurUtility.Transparent_Layer_to_BackEnd(Request, Response);

            return string.Empty;
        }

        public string FindAll()
        {
            OurUtility.Transparent_Layer_to_BackEnd(Request, Response);

            return string.Empty;
        }
    }
}