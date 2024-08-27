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
    public class GroupController : Controller
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

            // -- Actual code
            using (MERCY_Ctx db = new MERCY_Ctx())
            {
                var dataQuery =
                        (
                            from d in db.Groups
                                join u in db.Users on d.CreatedBy equals u.UserId
                            where (isAll_Text || d.GroupName.Contains(txt))
                            orderby d.IsActive descending, d.GroupName
                            select new Model_View_Group
                            {
                                GroupId = d.GroupId
                                , IsActive = d.IsActive
                                , GroupName = d.GroupName
                                , CreatedOn = d.CreatedOn
                                , FullName = u.FullName
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

            return Json(GroupX.Get_ddl(permission_Item), JsonRequestBehavior.AllowGet);
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

            return Json(GroupX.Create(p_collection, permission_Item, user.UserId), JsonRequestBehavior.AllowGet);
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

            return Json(GroupX.Edit(p_collection, permission_Item, user.UserId, OurUtility.ToInt32(Request[".id"])), JsonRequestBehavior.AllowGet);
        }

        // Delete - for GET
        public JsonResult Delete()
        {
            // Check Permission based on Current Url
            UserX user = new UserX(Request);
            Permission_Item permission_Item = null;

            Permission.Check_API(Request, user, ref permission_Item);
            if ( !permission_Item.Is_Delete)
            {
                var msg = new { Success = false, Permission = permission_Item, Message = Permission.ERROR_PERMISSION_DELETE, MessageDetail = string.Empty, Version = Configuration.VERSION };
                return Json(msg, JsonRequestBehavior.AllowGet);
            }

            return Json(GroupX.Delete(permission_Item, OurUtility.ToInt32(Request[".id"])), JsonRequestBehavior.AllowGet);
        }

        // Delete - for POST
        /*[HttpPost]
        public JsonResult DeleteP(FormCollection p_collection)
        {
            int id = OurUtility.ToInt32(p_collection["groupid"]);

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
                                from d in db.Groups
                                where d.GroupId == id
                                select d
                            );

                    var data = dataQuery.SingleOrDefault();

                    data.IsActive = OurUtility.ValueOf(p_collection, "isactive").Equals("1");
                    data.LastModifiedOn = DateTime.Now;
                    data.LastModifiedBy = user.UserId;

                    db.SaveChanges();

                    var result = new { Success = true, Permission = permission_Item, Message = string.Empty, MessageDetail = string.Empty, Version = Configuration.VERSION };
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

    public class GroupX
    {
        public static object Get_ddl(Permission_Item p_permission_Item)
        {
            using (MERCY_Ctx db = new MERCY_Ctx())
            {
                var dataQuery =
                        (
                            from d in db.Groups
                            where d.IsActive
                            orderby d.GroupName
                            select new
                            {
                                id = d.GroupId
                                , text = d.GroupName
                            }
                        );

                var items = dataQuery.ToList();

                return new { Success = true, Permission = p_permission_Item, Message = string.Empty, MessageDetail = string.Empty, Version = Configuration.VERSION, Items = items, Total = items.Count };
            }
        }

        public static object Get_ddl(Permission_Item p_permission_Item, UserX p_user, bool p_is_Show_User_Menu, bool p_is_Show_User_Relation)
        {
            using (MERCY_Ctx db = new MERCY_Ctx())
            {
                var dataQuery =
                        (
                            from d in db.Groups
                            where d.IsActive
                            orderby d.GroupName
                            select new
                            {
                                id = d.GroupId
                                , text = d.GroupName
                            }
                        );

                var items = dataQuery.ToList();

                return new { Success = true, Permission = p_permission_Item, User = UserX.Information(p_user, p_is_Show_User_Menu, p_is_Show_User_Relation), Message = string.Empty, MessageDetail = string.Empty, Version = Configuration.VERSION, Items = items, Total = items.Count };
            }
        }

        public static object Create(FormCollection p_collection, Permission_Item p_permission_Item, int p_executedBy)
        {
            try
            {
                using (MERCY_Ctx db = new MERCY_Ctx())
                {
                    var data = new Group
                    {
                        GroupName = p_collection["name"],

                        IsActive = OurUtility.ValueOf(p_collection, "isactive").Equals("1"),
                        CreatedOn = DateTime.Now,
                        CreatedBy = p_executedBy
                    };

                    db.Groups.Add(data);
                    db.SaveChanges();

                    int id = data.GroupId;

                    return new { Success = true, Permission = p_permission_Item, Message = BaseConstants.MESSAGE_CREATE_SUCCESS, MessageDetail = string.Empty, Version = Configuration.VERSION, Data = id };
                }
            }
            catch (Exception ex)
            {
                return new { Success = false, Permission = p_permission_Item, ex.Message, MessageDetail = string.Empty, Version = Configuration.VERSION, Items = string.Empty, Total = 0 };
            }
        }

        public static object Edit(FormCollection p_collection, Permission_Item p_permission_Item, int p_executedBy, int p_id)
        {
            try
            {
                using (MERCY_Ctx db = new MERCY_Ctx())
                {
                    var dataQuery =
                            (
                                from d in db.Groups
                                where d.GroupId == p_id
                                select d
                            );

                    var data = dataQuery.SingleOrDefault();

                    data.GroupName = p_collection["name"];

                    data.IsActive = OurUtility.ValueOf(p_collection, "isactive").Equals("1");
                    data.LastModifiedOn = DateTime.Now;
                    data.LastModifiedBy = p_executedBy;

                    db.SaveChanges();

                    return new { Success = true, Permission = p_permission_Item, Message = BaseConstants.MESSAGE_UPDATE_SUCCESS, MessageDetail = string.Empty, Version = Configuration.VERSION, Data = p_id };
                }
            }
            catch (Exception ex)
            {
                return new { Success = false, Permission = p_permission_Item, ex.Message, MessageDetail = string.Empty, Version = Configuration.VERSION, Items = string.Empty, Total = 0 };
            }
        }

        public static object Delete(Permission_Item p_permission_Item, int p_id)
        {
            try
            {
                using (MERCY_Ctx db = new MERCY_Ctx())
                {
                    var dataQuery =
                            (
                                from d in db.Groups
                                where d.GroupId == p_id
                                select d
                            );

                    var data = dataQuery.SingleOrDefault();

                    db.Groups.Remove(data);
                    db.SaveChanges();

                    //DeletedData.Add(db, "group", id.Value);

                    return new { Success = true, Permission = p_permission_Item, Message = BaseConstants.MESSAGE_DELETE_SUCCESS, MessageDetail = string.Empty, Version = Configuration.VERSION, Data = p_id };
                }
            }
            catch (Exception ex)
            {
                return new { Success = false, Permission = p_permission_Item, ex.Message, MessageDetail = string.Empty, Version = Configuration.VERSION, Items = string.Empty, Total = 0 };
            }
        }

        // Delete - for POST
        /*[HttpPost]
        public static object DeleteP(FormCollection p_collection)
        {
            int id = OurUtility.ToInt32(p_collection["groupid"]);

            return Delete(id);
        }*/
    }
}