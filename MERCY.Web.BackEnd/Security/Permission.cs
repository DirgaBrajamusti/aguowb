using System.Linq;
using System.Web;

using MERCY.Data.EntityFramework;
using MERCY.Web.BackEnd.Models;

namespace MERCY.Web.BackEnd.Security
{
    public class Permission
    {
        public const string ERROR_PERMISSION_LOGIN = "You must be logged in.";
        public const string ERROR_PERMISSION_READ = "Permission is not enough. Read access is needed.";
        public const string ERROR_PERMISSION_ADD = "Permission is not enough. Add access is needed.";
        public const string ERROR_PERMISSION_EDIT = "Permission is not enough. Edit access is needed.";
        public const string ERROR_PERMISSION_DELETE = "Permission is not enough. Delete access is needed.";
        public const string ERROR_PERMISSION_IS_ACKNOWLEDGE = "Permission is not enough. Acknowledge access is needed.";
        public const string ERROR_PERMISSION_SET_ISACTIVE = "Permission is not enough. Set IsActive access is needed.";

        public static bool Check(HttpRequestBase p_request, string p_url, UserX p_user, ref Permission_Item p_permission_Item)
        {
            bool result = true;

            // initialization
            p_permission_Item = new Permission_Item
            {
                Is_View = false,
                Is_Add = false,
                Is_Delete = false,
                Is_Edit = false,
                Is_Active = false,
                Is_Acknowledge = false,
                Is_Approve = false,
                Is_Email = false
            };

            try
            {
                using (MERCY_Ctx db = new MERCY_Ctx())
                {
                    var dataQuery =
                            (
                                from d in db.Menus
                                    join p in db.Permissions on d.MenuId equals p.MenuId
                                    //join g in db.groups on p.groupid equals g.groupid
                                    join ug in db.UserGroups on p.GroupId equals ug.GroupId
                                where ug.UserId == p_user.UserId
                                    && (p.IsView || p.IsAdd || p.IsDelete || p.IsUpdate || p.IsAcknowledge || p.IsApprove || p.IsEmail || p.IsActive)
                                    && d.Url == p_url
                                orderby d.Level, d.Ordering
                                select new
                                {
                                    d.MenuId
                                    , p.IsView
                                    , p.IsAdd
                                    , p.IsDelete
                                    , p.IsUpdate
                                    , p.IsActive
                                    , p.IsAcknowledge
                                    , p.IsApprove
                                    , p.IsEmail
                                }
                            );

                    var list = dataQuery.ToList();

                    int count = list.Count();
                    for (int i = 0; i < count; i++)
                    {
                        if (list[i].IsView)
                        {
                            p_permission_Item.Is_View = true;
                        }
                        if (list[i].IsAdd)
                        {
                            p_permission_Item.Is_Add = true;
                        }
                        if (list[i].IsDelete)
                        {
                            p_permission_Item.Is_Delete = true;
                        }
                        if (list[i].IsUpdate)
                        {
                            p_permission_Item.Is_Edit = true;
                        }
                        if (list[i].IsAcknowledge)
                        {
                            p_permission_Item.Is_Acknowledge = true;
                        }
                        if (list[i].IsApprove)
                        {
                            p_permission_Item.Is_Approve = true;
                        }
                        if (list[i].IsEmail)
                        {
                            p_permission_Item.Is_Email = true;
                        }
                        if (list[i].IsActive)
                        {
                            p_permission_Item.Is_Active = true;
                        }
                    }

                    result = true;
                }
            }
            catch {}

            // -- Calculation for Is_View
            p_permission_Item.Is_View = (p_permission_Item.Is_View 
                                            || p_permission_Item.Is_Add
                                            || p_permission_Item.Is_Delete
                                            || p_permission_Item.Is_Edit
                                            || p_permission_Item.Is_Acknowledge
                                            || p_permission_Item.Is_Approve
                                            || p_permission_Item.Is_Email
                                            || p_permission_Item.Is_Active);

            return result;
        }

        public static bool Check_API(HttpRequestBase p_request, UserX p_user, ref Permission_Item p_permission_Item)
        {
            string url_Menu = "*****";

            try
            {
                string url_API = p_request.Url.AbsolutePath.ToLower();

                using (MERCY_Ctx db = new MERCY_Ctx())
                {
                    var dataQuery =
                            (
                                from ap in db.Apiis
                                join mn in db.Menus on ap.MenuId equals mn.MenuId
                                where ap.Url == url_API
                                select new
                                {
                                    url_Menu = mn.Url
                                }
                            );

                    var data = dataQuery.SingleOrDefault();

                    url_Menu = data.url_Menu;
                }
            }
            catch { }

            // Check Permission based on Current Url
            return Check(p_request, url_Menu, p_user, ref p_permission_Item);
        }
    }
}