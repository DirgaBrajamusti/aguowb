using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

using System.Data;
using System.Data.SqlClient;

using MERCY.Data.EntityFramework;
using MERCY.Web.BackEnd.Models;
using MERCY.Web.BackEnd.Helpers;
using MERCY.Web.BackEnd.Security;
using Permission = MERCY.Web.BackEnd.Security.Permission;

namespace MERCY.Web.BackEnd.Areas.Api.Controllers
{
    public class PermissionController : Controller
    {
        public JsonResult Index()
        {
            List<Model_View_Menu> result = new List<Model_View_Menu>();

            // Check Permission based on Current Url
            UserX user = new UserX(Request);
            Permission_Item permission_Item = null;

            Permission.Check_API(Request, user, ref permission_Item);
            if ( ! permission_Item.Is_View)
            {
                var msg = new { Success = false, Permission = permission_Item, Message = Permission.ERROR_PERMISSION_READ, MessageDetail = string.Empty, Version = Configuration.VERSION};
                return Json(msg, JsonRequestBehavior.AllowGet);
            }

            bool is_Show_User_Menu = OurUtility.ValueOf(Request, "u_menu").Equals("1");
            bool is_Show_User_Relation = OurUtility.ValueOf(Request, "u_relation").Equals("1");

            // -- Actual code
            try
            {
                int groupid = OurUtility.ToInt32(Request["groupid"]);

                using (MERCY_Ctx db = new MERCY_Ctx())
                {
                    using (var connection = new SqlConnection(db.Database.Connection.ConnectionString))
                    {
                        connection.Open();

                        SqlCommand command = connection.CreateCommand();

                        command.CommandText = string.Format(@"
                                                            select
                                                                p.groupid
                                                                , d.menuid
                                                                , d.menuname
                                                                , case when d.parentid <= 0 then d.menuname else '--- ' + d.menuname end name2
                                                                , d.url
                                                                , d.ordering
                                                                , d.parentid
                                                                , d.level
                                                                , case when IsNull(p.isview, -1) = -1 then 0 else p.isview end isview
                                                                , case when IsNull(p.isadd, -1) = -1 then 0 else p.isadd end isadd
                                                                , case when IsNull(p.isdelete, -1) = -1 then 0 else p.isdelete end isdelete
                                                                , case when IsNull(p.isupdate, -1) = -1 then 0 else p.isupdate end isupdate
                                                                , case when IsNull(p.isacknowledge, -1) = -1 then 0 else p.isacknowledge end isacknowledge
                                                                , case when IsNull(p.isapprove, -1) = -1 then 0 else p.isapprove end isapprove
                                                                , case when IsNull(p.isemail, -1) = -1 then 0 else p.isemail end isemail
                                                                , case when IsNull(p.isactive, -1) = -1 then 0 else p.isactive end isactive
                                                            from menu d
                                                                left join permission p on d.menuid = p.menuid and p.groupid = {0}
                                                            --where p.groupid = {0} or p.groupid is NULL
                                                            order by  d.level, d.ordering, d.menuname
                                                            ", groupid);

                        Model_View_Menu menu = null;
                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                menu = new Model_View_Menu
                                {
                                    MenuId = OurUtility.ToInt32(reader["menuid"].ToString()),
                                    MenuName = reader["menuname"].ToString(),
                                    Name2 = OurUtility.ToInt32(reader["parentid"].ToString()) <= 0 ? reader["menuname"].ToString() : "-- " + reader["menuname"].ToString(),
                                    Url = reader["url"].ToString(),
                                    Ordering = OurUtility.ToInt32(reader["ordering"].ToString()),
                                    ParentId = OurUtility.ToInt32(reader["parentid"].ToString()),
                                    Level = OurUtility.ToInt32(reader["level"].ToString()),

                                    isview = (reader["isview"].ToString().Equals("1") || reader["isview"].ToString().Equals("true")),
                                    isadd = (reader["isadd"].ToString().Equals("1") || reader["isadd"].ToString().Equals("true")),
                                    isdelete = (reader["isdelete"].ToString().Equals("1") || reader["isdelete"].ToString().Equals("true")),
                                    isupdate = (reader["isupdate"].ToString().Equals("1") || reader["isupdate"].ToString().Equals("true")),
                                    isacknowledge = (reader["isacknowledge"].ToString().Equals("1") || reader["isacknowledge"].ToString().Equals("true")),
                                    isapprove = (reader["isapprove"].ToString().Equals("1") || reader["isapprove"].ToString().Equals("true")),
                                    isemail = (reader["isemail"].ToString().Equals("1") || reader["isemail"].ToString().Equals("true")),
                                    isactive = (reader["isactive"].ToString().Equals("1") || reader["isactive"].ToString().Equals("true"))
                                };

                                result.Add(menu);
                            }
                        }

                        connection.Close();

                        string hdr_text = "hdr1";
                        result.ForEach(c =>
                        {
                            // reset
                            hdr_text = "hdr2";

                            result.ForEach(c_child =>
                            {
                                if (c_child.ParentId == c.MenuId)
                                {
                                    hdr_text = "hdr1";
                                }
                            });

                            c.hdr = hdr_text;
                        });
                    }
                }

            }
            catch {}

            var result2 = new { Success = true, Permission = permission_Item, User = UserX.Information(user, is_Show_User_Menu, is_Show_User_Relation), Message = string.Empty, MessageDetail = string.Empty, Version = Configuration.VERSION, Items = result, Total = result.Count };
            return Json(result2, JsonRequestBehavior.AllowGet);
        }

        public JsonResult Index2()
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

            bool is_Show_User_Menu = Request["u_menu"].Equals("1");
            bool is_Show_User_Relation = Request["u_relation"].Equals("1");

            // -- Actual code
            try
            {
                int groupid = OurUtility.ToInt32(Request["group"]);

                using (MERCY_Ctx db = new MERCY_Ctx())
                {
                    var dataQuery =
                            (
                                from d in db.Menus
                                    join p in db.Permissions on d.MenuId equals p.MenuId into ps
                                from p in ps.DefaultIfEmpty()
                                where p.GroupId == groupid
                                orderby d.Level, d.Ordering
                                select new Model_View_Menu
                                {
                                    MenuId = d.MenuId
                                    , MenuName = d.MenuName
                                    , Name2 = d.ParentId <= 0 ? d.MenuName : "--- " + d.MenuName
                                    , Url = d.Url
                                    , Ordering = d.Ordering
                                    , ParentId = d.ParentId
                                    , Level = d.Level
                                    , isview = p != null && p.IsView
                                    , isadd = p != null && p.IsAdd
                                    , isdelete = p != null && p.IsDelete
                                    , isupdate = p != null && p.IsUpdate
                                    , isactive = p != null && p.IsActive
                                }
                            );

                    var items = dataQuery.ToList();
                    if (items.Count <= 0)
                    {
                        // if and only if: "Left Join" doesn't work
                        var dataQuery2 =
                            (
                                from d in db.Menus
                                orderby d.Level, d.Ordering
                                select new Model_View_Menu
                                {
                                    MenuId = d.MenuId
                                    , MenuName = d.MenuName
                                    , Name2 = d.ParentId <= 0 ? d.MenuName : "--- " + d.MenuName
                                    , Url = d.Url
                                    , Ordering = d.Ordering
                                    , ParentId = d.ParentId
                                    , Level = d.Level
                                    , isview = false
                                    , isadd = false
                                    , isdelete = false
                                    , isupdate = false
                                    , isactive = false
                                }
                            );

                        items = dataQuery2.ToList();
                    }

                    string hdr_text = "hdr1";
                    items.ForEach(c =>
                    {
                        // reset
                        hdr_text = "hdr2";

                        items.ForEach(c_child =>
                        {
                            if (c_child.ParentId == c.MenuId)
                            {
                                hdr_text = "hdr1";
                            }
                        });

                        c.hdr = hdr_text;
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

        [HttpPost]
        public JsonResult Save(FormCollection p_collection)
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
                int groupId = Convert.ToInt32(p_collection["p_groupid"]);
                string chkMenu = string.Empty;
                string parentId = string.Empty;
                string perm_View = string.Empty;
                string perm_Add = string.Empty;
                string perm_Delete = string.Empty;
                string perm_Edit = string.Empty;
                string perm_IsActive = string.Empty;

                using (MERCY_Ctx db = new MERCY_Ctx())
                {
                    Delete_Permission_by_Group(db, groupId);

                    int max_Data_Processed = 100;

                    for (int i = 0; i <= max_Data_Processed; i++)
                    {
                        chkMenu = Request["chkMenu" + i.ToString()];
                        if (string.IsNullOrEmpty(Request["chkMenu" + i.ToString()]))
                        {
                            // stop indicator
                            break;
                        }

                        try
                        {
                            var data = new MERCY.Data.EntityFramework.Permission
                            {
                                GroupId = groupId,
                                MenuId = OurUtility.ToInt32(Request["chkMenu" + i.ToString()]),
                                IsView = Request["chkItem" + i.ToString() + "View"].Equals("1"),
                                IsAdd = Request["chkItem" + i.ToString() + "Add"].Equals("1"),
                                IsDelete = Request["chkItem" + i.ToString() + "Delete"].Equals("1"),
                                IsUpdate = Request["chkItem" + i.ToString() + "Edit"].Equals("1"),
                                IsActive = Request["chkItem" + i.ToString() + "IsActive"].Equals("1"),
                                IsAcknowledge = Request["chkItem" + i.ToString() + "Acknowledge"].Equals("1"),
                                IsApprove = Request["chkItem" + i.ToString() + "Approve"].Equals("1"),
                                IsEmail = Request["chkItem" + i.ToString() + "Email"].Equals("1"),


                                CreatedOn = DateTime.Now,
                                CreatedBy = user.UserId
                            };

                            db.Permissions.Add(data);
                            db.SaveChanges();
                        }
                        catch (Exception)
                        {
                            //result += OurUtility.FlattenException(ex2);
                        }
                    }

                    //TODO: Add from Menu that are not still not Exists in Table Permission
                    var result = new { Success = true, Permission = permission_Item, Message = BaseConstants.MESSAGE_CREATE_SUCCESS, MessageDetail = string.Empty, Version = Configuration.VERSION};
                    return Json(result, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                var result = new { Success = false, Permission = permission_Item, ex.Message, MessageDetail = string.Empty, Version = Configuration.VERSION, Data = 0 };
                return Json(result, JsonRequestBehavior.AllowGet);
            }
        }

        private static void Delete_Permission_by_Group(MERCY_Ctx p_db, int p_group)
        {
            try
            {
                bool isDelete = false;
                var items = p_db.Permissions.Where(d => d.GroupId == p_group);
                foreach (var item in items)
                {
                    p_db.Permissions.Remove(item);
                    isDelete = true;

                    //DeletedData.Add(p_db, "permission", item.PermissionId);
                }

                if (isDelete) p_db.SaveChanges();
            }
            catch {}
        }

        public JsonResult GetByUser()
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

            // -- Actual code
            int userId = OurUtility.ToInt32(OurUtility.ValueOf(Request, "u"));

            using (MERCY_Ctx db = new MERCY_Ctx())
            {
                var dataQuery =
                        (
                            from d in db.Menus
                                join p in db.Permissions on d.MenuId equals p.MenuId
                                //join g in db.Groups on p.GroupId equals g.GroupId
                                join ug in db.UserGroups on p.GroupId equals ug.GroupId
                            where ug.UserId == userId
                                && (p.IsView || p.IsAdd || p.IsDelete || p.IsUpdate || p.IsActive)
                            orderby d.Level, d.Ordering
                            select new
                            {
                                d.MenuId
                                , d.MenuName
                                , Name2 = d.ParentId <= 0 ? d.MenuName : "--- " + d.MenuName
                                , d.Url
                                , d.Ordering
                                , d.ParentId
                                , d.Level
                                , p.IsView
                                , p.IsAdd
                                , p.IsDelete
                                , p.IsUpdate
                                , p.IsActive
                                , p.GroupId
                            }
                        );

                var items = dataQuery.ToList();

                var result = new { Success = true, Permission = permission_Item, Message = string.Empty, MessageDetail = string.Empty, Version = Configuration.VERSION, Items = items, Total = items.Count };
                return Json(result, JsonRequestBehavior.AllowGet);
            }
        }

        public JsonResult Menu()
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

            // -- Actual code
            using (MERCY_Ctx db = new MERCY_Ctx())
            {
                var dataQuery =
                        (
                            from d in db.Menus
                                join p in db.Permissions on d.MenuId equals p.MenuId
                                //join g in db.Groups on p.GroupId equals g.GroupId
                                join ug in db.UserGroups on p.GroupId equals ug.GroupId
                            where ug.UserId == user.UserId
                                && (p.IsView || p.IsAdd || p.IsDelete || p.IsUpdate || p.IsActive)
                            orderby d.Level, d.Ordering
                            select new
                            {
                                d.MenuId
                                , d.MenuName
                                , Name2 = d.ParentId <= 0 ? d.MenuName : "--- " + d.MenuName
                                , Url = d.Url == "" ? "#" : d.Url
                                , d.Ordering
                                , d.ParentId
                                , d.Level
                            }
                        );

                var items = dataQuery.ToList();

                var result = new { Success = true, Permission = permission_Item, Message = string.Empty, MessageDetail = string.Empty, Version = Configuration.VERSION, Items = items, Total = items.Count };
                return Json(result, JsonRequestBehavior.AllowGet);
            }
        }

        public JsonResult Group_ddl()
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

            bool is_Show_User_Menu = Request["u_menu"].Equals("1");
            bool is_Show_User_Relation = Request["u_relation"].Equals("1");

            return Json(GroupX.Get_ddl(permission_Item, user, is_Show_User_Menu, is_Show_User_Relation), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult Group_Create(FormCollection p_collection)
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
        public JsonResult Group_Edit(FormCollection p_collection)
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
    }
}