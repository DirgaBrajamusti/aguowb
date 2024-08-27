using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using MERCY.Web.BackEnd.Models;
using MERCY.Web.BackEnd.Helpers;
using MERCY.Web.BackEnd.Security;
using Permission = MERCY.Web.BackEnd.Security.Permission;

namespace MERCY.Web.BackEnd.Areas.Api.Controllers
{
    public class ProductController : Controller
    {
        public JsonResult Index()
        {
            var controllerB = new ExternalDataController();
            controllerB.InitializeController(this.Request.RequestContext);

            return controllerB.GetProduct();
        }

        public JsonResult Get_ddl_All()
        {
            var controllerB = new ExternalDataController();
            controllerB.InitializeController(this.Request.RequestContext);

            return controllerB.GetProduct();
        }

        public JsonResult Get_ddl()
        {
            var controller_Company = new CompanyController();
            controller_Company.InitializeController(this.Request.RequestContext);

            if (controller_Company.Is_from_BigData(OurUtility.ValueOf(Request, "company")))
            {
                // this is from BigData
                var controllerB = new ExternalDataController();
                controllerB.InitializeController(this.Request.RequestContext);

                return controllerB.GetProduct();
            }

            // Load data from Local Database
            var controller_Product = new X_ProductController();
            controller_Product.InitializeController(this.Request.RequestContext);

            return controller_Product.GetProduct();
        }

        public JsonResult Get()
        {

            var controller_Company = new CompanyController();
            controller_Company.InitializeController(this.Request.RequestContext);

            if (controller_Company.Is_from_BigData(OurUtility.ValueOf(Request, "company")))
            {
                // this is from BigData
                var controllerB = new ExternalDataController();
                controllerB.InitializeController(this.Request.RequestContext);

                return controllerB.GetProduct_byName();
            }

            // Load data from Local Database
            var controller_Product = new X_ProductController();
            controller_Product.InitializeController(this.Request.RequestContext);

            return controller_Product.GetProduct_byName();
        }

        public JsonResult GetByCompany()
        {
            // Check Permission based on Current Url
            UserX user = new UserX(Request);
            Permission_Item permission_Item = null;

            // -- Not necessary checking Permission
            //Permission.Check_API(Request, user, ref permission_Item);
            // -- just Logging User: is enough
            if (user.UserId <= 0)
            {
                var msg = new { Success = false, Permission = permission_Item, Message = Permission.ERROR_PERMISSION_READ + " [not Login]", MessageDetail = string.Empty, Version = Configuration.VERSION };
                return Json(msg, JsonRequestBehavior.AllowGet);
            }

            try
            {
                string company = Request["c"];

                Configuration config = new Configuration();
                string productss = config.Excel_Product_TCM;

                switch (company)
                {
                    case "TCM":
                        productss = config.Excel_Product_TCM;
                        break;
                    case "BEK":
                        productss = config.Excel_Product_BEK;
                        break;
                }

                List<string> products = productss.Split(',').ToList();
                products.Sort();

                var result = new { Success = true, Permission = permission_Item, Message = string.Empty, MessageDetail = string.Empty, Version = Configuration.VERSION, Items = products, Total = products.Count };
                return Json(result, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                var result = new { Success = false, Permission = permission_Item, ex.Message, MessageDetail = string.Empty, Version = Configuration.VERSION, Items = string.Empty, Total = 0 };
                return Json(result, JsonRequestBehavior.AllowGet);
            }
        }
    }
}