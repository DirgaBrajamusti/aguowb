using System;
using System.Linq;
using System.Web.Mvc;

using MERCY.Data.EntityFramework;
using MERCY.Web.BackEnd.Models;
using MERCY.Web.BackEnd.Helpers;
using MERCY.Web.BackEnd.Security;
using Permission = MERCY.Web.BackEnd.Security.Permission;

namespace MERCY.Web.BackEnd.Areas.Api.Controllers
{
    public class X_ProductController : Controller
    {
        public void InitializeController(System.Web.Routing.RequestContext context)
        {
            base.Initialize(context);
        }

        public JsonResult Index()
        {
            // Check Permission based on Current Url
            UserX user = new UserX(Request);
            Permission_Item permission_Item = null;

            Permission.Check_API(Request, user, ref permission_Item);
            if ( ! permission_Item.Is_View)
            {
                var msg = new { Success = false, Permission = permission_Item, Message = Permission.ERROR_PERMISSION_READ, MessageDetail = string.Empty, Version = Configuration.VERSION };
                return Json(msg, JsonRequestBehavior.AllowGet);
            }

            bool is_Show_User_Menu = OurUtility.ValueOf(Request, "u_menu").Equals("1");
            bool is_Show_User_Relation = OurUtility.ValueOf(Request, "u_relation").Equals("1");

            bool isAll_Text = true;
            string txt = OurUtility.ValueOf(Request, "txt");
            isAll_Text = string.IsNullOrEmpty(txt);

            bool isAll_Company = true;
            string company = OurUtility.ValueOf(Request, "company");
            isAll_Company = (string.IsNullOrEmpty(company) || company == "all");

            // -- Actual code
            try
            {
                using (MERCY_Ctx db = new MERCY_Ctx())
                {
                    var dataQuery =
                            (
                                from d in db.Products
                                    join c in db.Companies on d.CompanyCode equals c.CompanyCode
                                where (isAll_Text || d.ProductName.Contains(txt))
                                        && (isAll_Company || d.CompanyCode == company)
                                        && d.IsActive
                                orderby d.ProductName
                                select new Model_View_Product
                                {
                                    ProductId = d.ProductId
                                    , ProductName = d.ProductName
                                    , CV = d.CV
                                    , TS = d.TS
                                    , ASH = d.ASH
                                    , IM = d.IM
                                    , TM = d.TM
                                    , CompanyCode = c.CompanyCode
                                    , CompanyName = c.Name
                                }
                            );

                    var items = dataQuery.ToList();

                    items.ForEach(c =>
                    {
                        c.CV_Str = string.Format("{0:N0}", OurUtility.Round(c.CV, 0));
                        c.TS_Str = string.Format("{0:N2}", OurUtility.Round(c.TS, 2));
                        c.ASH_Str = string.Format("{0:N2}", OurUtility.Round(c.ASH, 2));
                        c.IM_Str = string.Format("{0:N2}", OurUtility.Round(c.IM, 2));
                        c.TM_Str = string.Format("{0:N2}", OurUtility.Round(c.TM, 2));
                    });

                    var result = new { Success = true, Permission = permission_Item, User = UserX.Information(user, is_Show_User_Menu, is_Show_User_Relation), Message = string.Empty, MessageDetail = string.Empty, Version = Configuration.VERSION, Items = items, Total = items.Count };
                    return Json(result, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                var result = new { Success = false, Permission = permission_Item, ex.Message, MessageDetail = string.Empty, Version = Configuration.VERSION, Items = string.Empty, Total = 0 };
                return Json(result, JsonRequestBehavior.AllowGet);
            }
        }

        public JsonResult Create(FormCollection p_collection)
        {
            // Check Permission based on Current Url
            UserX user = new UserX(Request);
            Permission_Item permission_Item = null;

            Permission.Check_API(Request, user, ref permission_Item);
            if ( ! permission_Item.Is_Add)
            {
                var msg = new { Success = false, Permission = permission_Item, Message = Permission.ERROR_PERMISSION_ADD, MessageDetail = string.Empty, Version = Configuration.VERSION };
                return Json(msg, JsonRequestBehavior.AllowGet);
            }

            // -- Actual code
            try
            {
                using (MERCY_Ctx db = new MERCY_Ctx())
                {
                    var data = new Product
                    {
                        CompanyCode = OurUtility.ValueOf(p_collection, "company"),
                        ProductName = OurUtility.ValueOf(p_collection, "name"),
                        CV = OurUtility.ToDecimal(OurUtility.ValueOf(p_collection, "cv")),
                        TS = OurUtility.ToDecimal(OurUtility.ValueOf(p_collection, "ts")),
                        ASH = OurUtility.ToDecimal(OurUtility.ValueOf(p_collection, "ash")),
                        IM = OurUtility.ToDecimal(OurUtility.ValueOf(p_collection, "im")),
                        TM = OurUtility.ToDecimal(OurUtility.ValueOf(p_collection, "tm")),

                        IsActive = true,

                        CreatedOn = DateTime.Now,
                        CreatedBy = user.UserId
                    };

                    db.Products.Add(data);
                    db.SaveChanges();

                    var result = new { Success = true, Permission = permission_Item, Message = BaseConstants.MESSAGE_CREATE_SUCCESS, MessageDetail = string.Empty, Version = Configuration.VERSION};
                    return Json(result, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                string message = ex.Message;
                string messageDetail = ex.ToString();
                if (messageDetail.Contains("Violation of UNIQUE KEY constraint"))
                {
                    message = "Data is not valid (duplicate Product Name)";
                }

                var result = new { Success = false, Permission = permission_Item, Message = message, MessageDetail = string.Empty, Version = Configuration.VERSION};
                return Json(result, JsonRequestBehavior.AllowGet);
            }
        }

        public JsonResult Edit(FormCollection p_collection)
        {
            // Check Permission based on Current Url
            UserX user = new UserX(Request);
            Permission_Item permission_Item = null;

            Permission.Check_API(Request, user, ref permission_Item);
            if ( ! permission_Item.Is_Edit)
            {
                var msg = new { Success = false, Permission = permission_Item, Message = Permission.ERROR_PERMISSION_EDIT, MessageDetail = string.Empty, Version = Configuration.VERSION };
                return Json(msg, JsonRequestBehavior.AllowGet);
            }

            // -- Actual code
            try
            {
                int id = OurUtility.ToInt32(OurUtility.ValueOf(Request, ".id"));

                using (MERCY_Ctx db = new MERCY_Ctx())
                {
                    var dataQuery =
                        (
                            from d in db.Products
                            where d.ProductId == id
                            select d
                        );

                    var data = dataQuery.SingleOrDefault();

                    data.ProductName = OurUtility.ValueOf(p_collection, "name");
                    data.CV = OurUtility.ToDecimal(OurUtility.ValueOf(p_collection, "cv"));
                    data.TS = OurUtility.ToDecimal(OurUtility.ValueOf(p_collection, "ts"));
                    data.ASH = OurUtility.ToDecimal(OurUtility.ValueOf(p_collection, "ash"));
                    data.IM = OurUtility.ToDecimal(OurUtility.ValueOf(p_collection, "im"));
                    data.TM = OurUtility.ToDecimal(OurUtility.ValueOf(p_collection, "tm"));

                    data.LastModifiedOn = DateTime.Now;
                    data.LastModifiedBy = user.UserId;

                    db.SaveChanges();

                    var result = new { Success = true, Permission = permission_Item, Message = BaseConstants.MESSAGE_UPDATE_SUCCESS, MessageDetail = string.Empty, Version = Configuration.VERSION};
                    return Json(result, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                string message = ex.Message;
                string messageDetail = ex.ToString();
                if (messageDetail.Contains("Violation of UNIQUE KEY constraint"))
                {
                    message = "Data is not valid (duplicate Product Name)";
                }

                var result = new { Success = false, Permission = permission_Item, Message = message, MessageDetail = string.Empty, Version = Configuration.VERSION };
                return Json(result, JsonRequestBehavior.AllowGet);
            }
        }

        public JsonResult Delete()
        {
            // Check Permission based on Current Url
            UserX user = new UserX(Request);
            Permission_Item permission_Item = null;

            Permission.Check_API(Request, user, ref permission_Item);
            if ( ! permission_Item.Is_Delete)
            {
                var msg = new { Success = false, Permission = permission_Item, Message = Permission.ERROR_PERMISSION_DELETE, MessageDetail = string.Empty, Version = Configuration.VERSION };
                return Json(msg, JsonRequestBehavior.AllowGet);
            }

            // -- Actual code
            try
            {
                int id = OurUtility.ToInt32(OurUtility.ValueOf(Request, ".id"));

                using (MERCY_Ctx db = new MERCY_Ctx())
                {
                    var dataQuery =
                        (
                            from d in db.Products
                            where d.ProductId == id
                            select d
                        );

                    var data = dataQuery.SingleOrDefault();
                    db.Products.Remove(data);

                    db.SaveChanges();

                    var result = new { Success = true, Permission = permission_Item, Message = BaseConstants.MESSAGE_DELETE_SUCCESS, MessageDetail = string.Empty, Version = Configuration.VERSION};
                    return Json(result, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                var result = new { Success = false, Permission = permission_Item, ex.Message, MessageDetail = string.Empty, Version = Configuration.VERSION, Items = string.Empty, Total = 0 };
                return Json(result, JsonRequestBehavior.AllowGet);
            }
        }

        public JsonResult Get_ddl()
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

            // -- Actual code
            try
            {
                using (MERCY_Ctx db = new MERCY_Ctx())
                {
                    var dataQuery =
                            (
                                from d in db.Products
                                where d.IsActive
                                orderby d.ProductName
                                select new
                                {
                                    d.ProductId
                                    , d.ProductName
                                }
                            );

                    var items = dataQuery.ToList();
                    var result = new { Success = true, Permission = permission_Item, Message = string.Empty, MessageDetail = string.Empty, Version = Configuration.VERSION, Items = items, Total = items.Count };
                    return Json(result, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                var result = new { Success = false, Permission = permission_Item, ex.Message, MessageDetail = string.Empty, Version = Configuration.VERSION, Items = string.Empty, Total = 0 };
                return Json(result, JsonRequestBehavior.AllowGet);
            }
        }

        public JsonResult Get_ddl2()
        {
            // Check Permission based on Current Url
            UserX user = new UserX(Request);
            Permission_Item permission_Item = null;

            Permission.Check_API(Request, user, ref permission_Item);
            /*if ( ! permission_Item.Is_View)
            {
                var msg = new { Success = false, Permission = permission_Item, Message = Permission.ERROR_PERMISSION_READ, MessageDetail = string.Empty, Version = Configuration.VERSION };
                return Json(msg, JsonRequestBehavior.AllowGet);
            }*/

            // -- Actual code
            DateTime now = DateTime.Now;

            string company = Request["company"];
            bool isAllCompany = (string.IsNullOrEmpty(company) || company == "all");

            try
            {
                using (MERCY_Ctx db = new MERCY_Ctx())
                {
                    var dataQueryX =
                            (
                                from d in db.Products
                                where (isAllCompany || d.CompanyCode == company) && d.IsActive
                                orderby d.ProductName
                                select new
                                {
                                    id = d.ProductName
                                    , text = d.ProductName
                                }
                            );

                    var items = dataQueryX.Distinct().ToList();

                    var result = new { Success = true, Permission = permission_Item, Message = string.Empty, MessageDetail = string.Empty, Version = Configuration.VERSION, Items = items, Total = items.Count };
                    return Json(result, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                var result = new { Success = false, Permission = permission_Item, ex.Message, MessageDetail = string.Empty, Version = Configuration.VERSION, Items = string.Empty, Total = 0 };
                return Json(result, JsonRequestBehavior.AllowGet);
            }
        }

        public JsonResult GetProduct()
        {
            // Check Permission based on Current Url
            UserX user = new UserX(Request);
            Permission_Item permission_Item = null;

            Permission.Check_API(Request, user, ref permission_Item);
/*            if (!permission_Item.Is_View)
            {
                var msg = new { Success = false, Permission = permission_Item, Message = Permission.ERROR_PERMISSION_READ, MessageDetail = string.Empty, Version = Configuration.VERSION };
                return Json(msg, JsonRequestBehavior.AllowGet);
            }*/
            // -- Actual code
            DateTime now = DateTime.Now;
            string company = Request["company"];
            try
            {
                using (MERCY_Ctx db = new MERCY_Ctx())
                {
                    var dataQueryX =
                            (
                                from d in db.Products
                                where d.CompanyCode == company
                                orderby d.ProductName
                                select new Model_Product
                                {
                                    id = d.ProductId
                                    , Product_name = d.ProductName
                                    , FirstDate = null
                                }
                            );

                    var items = dataQueryX.Distinct().ToList();

                    var result = new { Success = true, Permission = permission_Item, Message = string.Empty, MessageDetail = string.Empty, Version = Configuration.VERSION, Items = items, Total = items.Count };
                    return Json(result, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                var result = new { Success = false, Permission = permission_Item, ex.Message, MessageDetail = string.Empty, Version = Configuration.VERSION, Items = string.Empty, Total = 0 };
                return Json(result, JsonRequestBehavior.AllowGet);
            }
        }

        public JsonResult GetProduct_byName()
        {
            // Check Permission based on Current Url
            UserX user = new UserX(Request);
            Permission_Item permission_Item = null;

            Permission.Check_API(Request, user, ref permission_Item);
            /*if (!permission_Item.Is_View)
            {
                var msg = new { Success = false, Permission = permission_Item, Message = Permission.ERROR_PERMISSION_READ, MessageDetail = string.Empty, Version = Configuration.VERSION };
                return Json(msg, JsonRequestBehavior.AllowGet);
            }*/

            // -- Actual code
            string productName = Request["productName"];

            try
            {
                using (MERCY_Ctx db = new MERCY_Ctx())
                {
                    var dataQueryX =
                            (
                                from d in db.Products
                                where d.ProductName == productName
                                orderby d.ProductId descending
                                select new Model_View_Local_Product
                                {
                                    id = d.ProductId
                                    , Product_name = d.ProductName
                                    , FirstDate = ""
                                    , Year = ""
                                    , months = ""
                                    , CV = d.CV
                                    , TS = d.TS
                                    , ASH = d.ASH
                                    , TM = d.TM
                                    , IM = d.IM
                                }
                            );

                    var items = dataQueryX.ToList();

                    items.ForEach(c =>
                    {
                        c.CV_Str = string.Format("{0:N0}", c.CV);
                        c.TS_Str = string.Format("{0:N2}", c.TS);
                        c.ASH_Str = string.Format("{0:N2}", c.ASH);
                        c.IM_Str = string.Format("{0:N2}", c.IM);
                        c.TM_Str = string.Format("{0:N2}", c.TM);
                    });

                    var item = items.Take(1);

                    var result = new { Success = true, Permission = permission_Item, Message = string.Empty, MessageDetail = string.Empty, Version = Configuration.VERSION, Item = item };
                    return Json(result, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                var result = new { Success = false, Permission = permission_Item, ex.Message, MessageDetail = string.Empty, Version = Configuration.VERSION, Items = string.Empty, Total = 0 };
                return Json(result, JsonRequestBehavior.AllowGet);
            }
        }
    }
}