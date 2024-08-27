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
    public class HopperController : Controller
    {
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
            isAll_Company = string.IsNullOrEmpty(company) || company == "all";

            int id = OurUtility.ToInt32(OurUtility.ValueOf(Request, "id"));
            bool isAllId = id == 0;

            bool isAllStatus = string.IsNullOrEmpty(OurUtility.ValueOf(Request, "isActive"));
            bool isActive = OurUtility.ValueOf(Request, "isActive").Equals("true");

            // -- Actual code
            try
            {
                using (MERCY_Ctx db = new MERCY_Ctx())
                {
                    var dataQuery =
                            (
                                from d in db.Hoppers
                                    join u in db.Users on d.CreatedBy equals u.UserId
                                    join c in db.Companies on d.CompanyCode equals c.CompanyCode
                                where (isAll_Company || d.CompanyCode == company)
                                    && (isAllId || d.HopperId == id)
                                    && (isAllStatus || d.IsActive == isActive)
                                    && (isAll_Text
                                        || d.CompanyCode.Contains(txt)
                                        || d.Name.Contains(txt))
                                orderby d.IsActive descending, d.CompanyCode
                                select new Model_View_Hopper
                                {
                                    HopperId = d.HopperId
                                    , CompanyCode = d.CompanyCode
                                    , IsActive = d.IsActive
                                    , Name = d.Name
                                    , CreatedOn = d.CreatedOn
                                    , FullName = u.FullName
                                    , CompanyName = c.Name
                                }
                            );

                    var items = dataQuery.ToList();
                    try
                    {
                        items.ForEach(c =>
                        {
                            c.CreatedOn_Str = OurUtility.DateFormat(c.CreatedOn, @"dd-MMM-yyyy HH:mm:ss");
                            c.CreatedOn_Str2 = OurUtility.DateFormat(c.CreatedOn, "dd-MMM-yyyy");
                        });
                    }
                    catch {}

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
                    var data = new Hopper
                    {
                        CompanyCode = OurUtility.ValueOf(p_collection, "company"),
                        Name = OurUtility.ValueOf(p_collection, "name"),
                        IsActive = OurUtility.ValueOf(p_collection, "isactive").Equals("1"),

                        CreatedOn = DateTime.Now,
                        CreatedBy = user.UserId
                    };

                    db.Hoppers.Add(data);
                    db.SaveChanges();

                    var result = new { Success = true, Permission = permission_Item, Message = BaseConstants.MESSAGE_CREATE_SUCCESS, MessageDetail = string.Empty, Version = Configuration.VERSION};
                    return Json(result, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                var result = new { Success = false, Permission = permission_Item, ex.Message, MessageDetail = string.Empty, Version = Configuration.VERSION, Items = string.Empty, Total = 0 };
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
                            from d in db.Hoppers
                            where d.HopperId == id
                            select d
                        );

                    var data = dataQuery.SingleOrDefault();

                    data.Name = OurUtility.ValueOf(p_collection, "name");
                    data.IsActive = OurUtility.ValueOf(p_collection, "isactive").Equals("1");

                    data.LastModifiedOn = DateTime.Now;
                    data.LastModifiedBy = user.UserId;

                    db.SaveChanges();

                    var result = new { Success = true, Permission = permission_Item, Message = BaseConstants.MESSAGE_UPDATE_SUCCESS, MessageDetail = string.Empty, Version = Configuration.VERSION};
                    return Json(result, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                var result = new { Success = false, Permission = permission_Item, ex.Message, MessageDetail = string.Empty, Version = Configuration.VERSION, Items = string.Empty, Total = 0 };
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
                            from d in db.Hoppers
                            where d.HopperId == id
                            select d
                        );

                    var data = dataQuery.SingleOrDefault();
                    db.Hoppers.Remove(data);

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
                                from d in db.Tunnels
                                where d.IsActive
                                orderby d.Name
                                select new
                                {
                                    id = d.CompanyCode
                                    , text = d.Name
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
    }
}