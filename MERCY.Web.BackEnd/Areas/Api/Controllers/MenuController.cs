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
    public class MenuController : Controller
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
            using (MERCY_Ctx db = new MERCY_Ctx())
            {
                var dataQuery =
                        (
                            from d in db.Menus
                                join p in db.Menus on d.ParentId equals p.MenuId into ps
                            from p in ps.DefaultIfEmpty()
                            orderby d.Level, d.Ordering, d.MenuName
                            select new
                            {
                                d.MenuId
                                , d.MenuName
                                , d.Description
                                , d.Url
                                , d.Logo
                                , d.Ordering
                                , d.ParentId
                                , ParentName = p == null ? "" : p.MenuName
                                , Name2 = d.ParentId <= 0 ? d.MenuName : "--- " + d.MenuName
                                , d.Level
                                , d.IsActive
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

        [HttpGet]
        public JsonResult Get()
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

            int id = OurUtility.ToInt32(Request[".id"]);
            if (id <= 0)
            {
                // -- special purpose
                // this Id is only for Checking CurrentUser:Info

                var result = new { Success = true, Permission = permission_Item, User = UserX.Information(user, is_Show_User_Menu, is_Show_User_Relation), Version = Configuration.VERSION };
                return Json(result, JsonRequestBehavior.AllowGet);
            }

            try
            {
                using (MERCY_Ctx db = new MERCY_Ctx())
                {
                    var dataQuery =
                            (
                                from d in db.Menus
                                join p in db.Menus on d.ParentId equals p.MenuId into ps
                                from p in ps.DefaultIfEmpty()
                                where d.MenuId == id
                                orderby d.Level, d.Ordering, d.MenuName
                                select new
                                {
                                    d.MenuId
                                    , d.MenuName
                                    , d.Description
                                    , d.Url
                                    , d.Logo
                                    , d.Ordering
                                    , d.ParentId
                                    , ParentName = p == null ? "" : p.MenuName
                                    , Name2 = d.ParentId <= 0 ? d.MenuName : "--- " + d.MenuName
                                    , d.Level
                                    , d.IsActive
                                    , d.CreatedOn
                                    , d.CreatedBy
                                    , d.LastModifiedOn
                                    , d.LastModifiedBy
                                }
                            );

                    var item = dataQuery.SingleOrDefault();
                    if (item == null)
                    {
                        var result_NotFound = new { Success = false, Permission = permission_Item, User = UserX.Information(user, is_Show_User_Menu, is_Show_User_Relation), Message = "Id: " + id.ToString() + " is not found", MessageDetail = string.Empty, Version = Configuration.VERSION};
                        return Json(result_NotFound, JsonRequestBehavior.AllowGet);
                    }
                    
                    var result = new { Success = true, Permission = permission_Item, User = UserX.Information(user, is_Show_User_Menu, is_Show_User_Relation), Item = item, Message = string.Empty, MessageDetail = string.Empty, Version = Configuration.VERSION };
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

            using (MERCY_Ctx db = new MERCY_Ctx())
            {
                var dataQuery =
                        (
                            from d in db.Menus
                            orderby d.Level, d.Ordering
                            select new
                            {
                                d.MenuId
                                , d.MenuName
                            }
                        );

                var items = dataQuery.ToList();

                var result = new { Success = true, Permission = permission_Item, Message = string.Empty, MessageDetail = string.Empty, Version = Configuration.VERSION, Items = items, Total = items.Count };
                return Json(result, JsonRequestBehavior.AllowGet);
            }
        }

        public JsonResult GetParents_ddl()
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

            using (MERCY_Ctx db = new MERCY_Ctx())
            {
                var dataQuery =
                        (
                            from d in db.Menus
                            where d.ParentId == 0
                            orderby d.Ordering
                            select new
                            {
                                d.MenuId
                                , d.MenuName
                            }
                        );

                var items = dataQuery.ToList();

                var result = new { Success = true, Permission = permission_Item, Message = string.Empty, MessageDetail = string.Empty, Version = Configuration.VERSION, Items = items, Total = items.Count };
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
                    var data = new Menu();

                    int ordering = OurUtility.ToInt32(OurUtility.ValueOf(p_collection, "ordering"));
                    int level = ordering * 10;
                    int parent = OurUtility.ToInt32(OurUtility.ValueOf(p_collection, "parentid"));

                    data.MenuName = OurUtility.ValueOf(p_collection, "menuname").Trim();
                    data.Description = OurUtility.ValueOf(p_collection, "description").Trim();
                    data.Url = OurUtility.ValueOf(p_collection, "url").Trim();
                    data.Logo = OurUtility.ValueOf(p_collection, "logo").Trim();
                    data.Ordering = ordering;
                    data.ParentId = parent;
                    data.Level = level;

                    if (parent != 0)
                    {
                        var dataQuery_Parent =
                            (
                                from d in db.Menus
                                where d.MenuId == parent
                                select d
                            );

                        var parent_Record = dataQuery_Parent.SingleOrDefault();
                        level = parent_Record.Level.Value + ordering;

                        data.Level = level;
                    }

                    data.IsActive = OurUtility.ValueOf(p_collection, "isactive").Equals("1");
                    data.CreatedOn = DateTime.Now;
                    data.CreatedBy = user.UserId;

                    db.Menus.Add(data);

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
                int id = OurUtility.ToInt32(Request[".id"]);

                using (MERCY_Ctx db = new MERCY_Ctx())
                {
                    var dataQuery =
                            (
                                from d in db.Menus
                                where d.MenuId == id
                                select d
                            );

                    var data = dataQuery.SingleOrDefault();

                    int ordering = OurUtility.ToInt32(OurUtility.ValueOf(p_collection, "ordering"));
                    int level = ordering * 10;
                    int parent = OurUtility.ToInt32(OurUtility.ValueOf(p_collection, "parentid"));

                    data.MenuName = OurUtility.ValueOf(p_collection, "menuname").Trim();
                    data.Description = OurUtility.ValueOf(p_collection, "description").Trim();
                    data.Url = OurUtility.ValueOf(p_collection, "url").Trim();
                    data.Logo = OurUtility.ValueOf(p_collection, "logo").Trim();
                    data.Ordering = ordering;
                    data.ParentId = parent;
                    data.Level = level;

                    if (parent != 0)
                    {
                        var dataQuery_Parent =
                            (
                                from d in db.Menus
                                where d.MenuId == parent
                                select d
                            );

                        var parent_Record = dataQuery_Parent.SingleOrDefault();
                        level = parent_Record.Level.Value + ordering;

                        data.Level = level;
                    }

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
                int id = OurUtility.ToInt32(Request[".id"]);

                using (MERCY_Ctx db = new MERCY_Ctx())
                {
                    var dataQuery =
                            (
                                from d in db.Menus
                                where d.MenuId == id
                                select d
                            );

                    var data = dataQuery.SingleOrDefault();

                    db.Menus.Remove(data);
                    db.SaveChanges();

                    //DeletedData.Add(db, "menu", id.Value);

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

        // Delete - for POST
        /*[HttpPost]
        public JsonResult DeleteP(FormCollection p_collection)
        {
            int id = OurUtility.ToInt32(p_collection["menuid"]);

            return Delete(id);
        }*/

        [HttpPost]
        public JsonResult SetActive(FormCollection p_collection)
        {
            // Check Permission based on Current Url
            UserX user = new UserX(Request);
            Permission_Item permission_Item = null;

            Permission.Check_API(Request, user, ref permission_Item);
            if ( ! permission_Item.Is_Active)
            {
                var msg = new { Success = false, Permission = permission_Item, Message = Permission.ERROR_PERMISSION_SET_ISACTIVE, MessageDetail = string.Empty, Version = Configuration.VERSION };
                return Json(msg, JsonRequestBehavior.AllowGet);
            }

            // -- Actual code
            try
            {
                int id = OurUtility.ToInt32(Request[".id"]);

                using (MERCY_Ctx db = new MERCY_Ctx())
                {
                    var dataQuery =
                            (
                                from d in db.Menus
                                where d.MenuId == id
                                select d
                            );

                    var data = dataQuery.SingleOrDefault();

                    data.IsActive = OurUtility.ValueOf(p_collection, "isactive").Equals("1");
                    data.LastModifiedOn = DateTime.Now;
                    data.LastModifiedBy = user.UserId;

                    db.SaveChanges();

                    var result = new { Success = true, Permission = permission_Item, Message = string.Empty, MessageDetail = string.Empty, Version = Configuration.VERSION};
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