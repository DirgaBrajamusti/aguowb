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
    public class ApiiController : Controller
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

            // -- Actual code
            int menuid = Convert.ToInt32(Request["menu"]);

            using (MERCY_Ctx db = new MERCY_Ctx())
            {
                var dataQuery =
                        (
                            from d in db.Apiis
                            where d.MenuId == menuid
                            orderby d.ApiName, d.Url
                            select new
                            {
                                d.ApiId
                                , d.ApiName
                                , d.Url
                                , d.MenuId
                                , d.CreatedOn
                                , d.CreatedBy
                                , d.LastModifiedOn
                                , d.LastModifiedBy
                            }
                        );

                var items = dataQuery.ToList();

                var result = new { Success = true, Permission = permission_Item, User = UserX.Information(user, is_Show_User_Menu, is_Show_User_Relation), Message = string.Empty, MessageDetail = string.Empty, Version = Configuration.VERSION, Items = items, Total = items.Count };
                return Json(result, JsonRequestBehavior.AllowGet);
            }
        }

        public JsonResult All()
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

            bool is_Show_User_Menu = OurUtility.ValueOf(Request, "u_menu").Equals("1");
            bool is_Show_User_Relation = OurUtility.ValueOf(Request, "u_relation").Equals("1");

            // -- Actual code
            using (MERCY_Ctx db = new MERCY_Ctx())
            {
                var dataQuery =
                        (
                            from d in db.Menus
                                join a in db.Apiis on d.MenuId equals a.MenuId
                            orderby d.Level, d.Ordering, d.MenuName, a.ApiName, a.Url
                            select new
                            {
                                d.MenuId
                                , d.MenuName
                                ,  d.Ordering
                                , d.ParentId
                                , Name2 = d.ParentId <= 0 ? d.MenuName : "--- " + d.MenuName
                                , d.Level
                                , a.ApiId
                                , a.ApiName
                                , a.Url
                                , a.CreatedOn
                                , a.CreatedBy
                                , a.LastModifiedOn
                                , a.LastModifiedBy
                            }
                        );

                var items = dataQuery.ToList();

                var result = new { Success = true, Permission = permission_Item, User = UserX.Information(user, is_Show_User_Menu, is_Show_User_Relation), Message = string.Empty, MessageDetail = string.Empty, Version = Configuration.VERSION, Items = items, Total = items.Count };
                return Json(result, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
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
                    var data = new Apii
                    {
                        MenuId = Convert.ToInt32(Request["menu"]),
                        ApiName = p_collection["apiname"].Trim(),
                        Url = p_collection["url"].Trim(),

                        CreatedOn = DateTime.Now,
                        CreatedBy = user.UserId
                    };

                    db.Apiis.Add(data);
                    db.SaveChanges();

                    var result = new { Success = true, Permission = permission_Item, Message = BaseConstants.MESSAGE_CREATE_SUCCESS, MessageDetail = string.Empty, Version = Configuration.VERSION, Data = data.ApiId };
                    return Json(result, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                var result = new { Success = false, Permission = permission_Item, ex.Message, MessageDetail = string.Empty, Version = Configuration.VERSION, Data = 0 };
                return Json(result, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
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
                int id = Convert.ToInt32(p_collection["apiid"]);

                using (MERCY_Ctx db = new MERCY_Ctx())
                {
                    var dataQuery =
                            (
                                from d in db.Apiis
                                where d.ApiId == id
                                select d
                            );

                    var data = dataQuery.SingleOrDefault();

                    data.MenuId = Convert.ToInt32(Request["menu"]);
                    data.ApiName = p_collection["apiname"].Trim();
                    data.Url = p_collection["url"].Trim();

                    data.LastModifiedOn = DateTime.Now;
                    data.LastModifiedBy = user.UserId;

                    db.SaveChanges();

                    var result = new { Success = true, Permission = permission_Item, Message = BaseConstants.MESSAGE_UPDATE_SUCCESS, MessageDetail = string.Empty, Version = Configuration.VERSION, Data = id };
                    return Json(result, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                var result = new { Success = false, Permission = permission_Item, ex.Message, MessageDetail = string.Empty, Version = Configuration.VERSION, Data = 0 };
                return Json(result, JsonRequestBehavior.AllowGet);
            }
        }

        // Delete - for POST
        [HttpPost]
        public JsonResult DeleteX(FormCollection p_collection)
        {
            int id = OurUtility.ToInt32(p_collection["apiid"]);

            return Delete();
        }

        // Delete - for GET
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
                int id = Convert.ToInt32(Request["apiid"]);

                using (MERCY_Ctx db = new MERCY_Ctx())
                {
                    var dataQuery =
                            (
                                from d in db.Apiis
                                where d.ApiId == id
                                select d
                            );

                    var data = dataQuery.SingleOrDefault();

                    db.Apiis.Remove(data);
                    db.SaveChanges();

                    //DeletedData.Add(db, "apii", id.Value);

                    var result = new { Success = true, Permission = permission_Item, Message = BaseConstants.MESSAGE_DELETE_SUCCESS, MessageDetail = string.Empty, Version = Configuration.VERSION, Data = id };
                    return Json(result, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                var result = new { Success = false, Permission = permission_Item, ex.Message, MessageDetail = string.Empty, Version = Configuration.VERSION, Data = 0 };
                return Json(result, JsonRequestBehavior.AllowGet);
            }
        }
    }
}