using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

using System.IO;
using System.Data;
using System.Data.SqlClient;

using MERCY.Data.EntityFramework;
using MERCY.Web.BackEnd.Models;
using MERCY.Web.BackEnd.Helpers;
using MERCY.Web.BackEnd.Security;
using Permission = MERCY.Web.BackEnd.Security.Permission;

namespace MERCY.Web.BackEnd.Areas.Api.Controllers
{
    public class UserController : Controller
    {
        public JsonResult Index()
        {
            string time_begin = DateTime.Now.ToString("dd-MMM-yyyy HH:mm:ss.ffff");
            string time_end = string.Empty;

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
                            from dt in db.Users
                            //where (isAll_Text || dt.LoginName.Contains(txt) || dt.FullName.Contains(txt) || dt.Title.Contains(txt))
                            orderby dt.LoginName
                            select new Model_View_User
                            {
                                UserId = dt.UserId
                                , LoginName = dt.LoginName
                                , FullName = dt.FullName
                                , Title = dt.Title
                                , Department = dt.Department
                                , Email = dt.Email
                                , IsActive= dt.IsActive
                                , Is_ActiveDirectory = dt.Is_ActiveDirectory
                            }
                        );

                var items = dataQuery.ToList();

                items.ForEach(c =>
                {
                    int i = 0;

                    var dataQuery_Sites =
                        (
                            from d in db.UserSites
                                join s in db.Sites on d.SiteId equals s.SiteId
                            where d.UserId == c.UserId
                            orderby s.SiteName
                            select new 
                            {
                                s.SiteName
                            }
                        );

                    var items_Sites = dataQuery_Sites.ToList();
                    string sites = string.Empty;
                    i = 0;
                    items_Sites.ForEach(s =>
                    {
                        i++;
                        sites += (i<=1?"":", ") + s.SiteName;
                    });
                    c.Sites = sites;

                    var dataQuery_Companies =
                        (
                            from d in db.UserCompanies
                                join s in db.Companies on d.CompanyCode equals s.CompanyCode
                            where d.UserId == c.UserId
                            orderby s.Name
                            select new
                            {
                                s.Name
                            }
                        );

                    var items_Companies = dataQuery_Companies.ToList();
                    string companies = string.Empty;
                    i = 0;
                    items_Companies.ForEach(s =>
                    {
                        i++;
                        companies += (i <= 1 ? "" : ", ") + s.Name;
                    });
                    c.Companies = companies;

                    var dataQuery_Groups =
                        (
                            from d in db.UserGroups
                            join s in db.Groups on d.GroupId equals s.GroupId
                            where d.UserId == c.UserId
                            orderby s.GroupName
                            select new
                            {
                                s.GroupName
                            }
                        );

                    var items_Groups = dataQuery_Groups.ToList();
                    string groups = string.Empty;
                    i = 0;
                    items_Groups.ForEach(s =>
                    {
                        i++;
                        groups += (i <= 1 ? "" : ", ") + s.GroupName;
                    });
                    c.Groups = groups;
                });

                if ( ! isAll_Text)
                {
                    var dataQuery2 =
                            (
                                from dt in items
                                where (isAll_Text || dt.LoginName.Contains(txt) || dt.FullName.Contains(txt) || dt.Title.Contains(txt)
                                            || dt.Sites.Contains(txt) || dt.Companies.Contains(txt) || dt.Groups.Contains(txt))
                                orderby dt.LoginName
                                select dt
                            );

                    items = dataQuery2.ToList();
                }

                //var draw = OurUtility.ValueOf(Request, "draw");
                //var recordsTotal = dataQuery.Count();
                /*
                // -- get Emails from Active Directory
                string server_DC_1 = Configuration.AD_Server;
                string server_DC_2 = Configuration.AD_Server_2;
                string loginName = Configuration.AD_User;
                string password = Configuration.AD_Password;

                try
                {
                    DirectoryEntry entry = new DirectoryEntry(server_DC_1, loginName, password);
                    DirectorySearcher searcher = new DirectorySearcher(entry);

                    items.ForEach(c =>
                    {
                        // skip this User
                        if ( ! c.Is_ActiveDirectory)
                        {
                            c.Email = string.Empty + " ini dari AD";
                        }
                        else
                        {
                            try
                            {            
                                searcher.Filter = "(sAMAccountName=" + c.LoginName + ")";
                                searcher.PropertiesToLoad.Add("mail");

                                SearchResult sr = searcher.FindOne();
                                if (sr != null)
                                {
                                    DirectoryEntry entryToFind = sr.GetDirectoryEntry();

                                    c.Email = entryToFind.Properties["mail"].Value as string;
                                }
                            }
                            catch {}
                        }
                    });
                }
                catch {}*/

                //return Json(new { draw = draw, recordsFiltered = recordsTotal, recordsTotal = recordsTotal, data = data }, JsonRequestBehavior.AllowGet);
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

            bool is_Show_User_Menu = Request["u_menu"].Equals("1");
            bool is_Show_User_Relation = Request["u_relation"].Equals("1");

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
                                from d in db.Users
                                where d.UserId == id
                                select new 
                                {
                                    d.UserId
                                    , d.LoginName
                                    , d.FullName
                                    , d.Title
                                    , d.Department
                                    , d.Email
                                    , d.IsActive
                                    , d.Is_ActiveDirectory
                                    , P = UserX.MASK_PASSWORD
                                }
                            );

                    var item = dataQuery.SingleOrDefault();
                    if (item == null)
                    {
                        var result_NotFound = new { Success = false, Permission = permission_Item, User = UserX.Information(user, is_Show_User_Menu, is_Show_User_Relation), Message = "Id: " + id.ToString() + " is not found", MessageDetail = string.Empty, Version = Configuration.VERSION};
                        return Json(result_NotFound, JsonRequestBehavior.AllowGet);
                    }
                    
                    var relations = UserX.Relations(db, item.UserId);

                    var result = new { Success = true, Permission = permission_Item, User = UserX.Information(user, is_Show_User_Menu, is_Show_User_Relation), Item = item, Relations = relations, Message = string.Empty, MessageDetail = string.Empty, Version = Configuration.VERSION };
                    return Json(result, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                var result = new { Success = false, Permission = permission_Item, ex.Message, MessageDetail = string.Empty, Version = Configuration.VERSION, Items = string.Empty, Total = 0 };
                return Json(result, JsonRequestBehavior.AllowGet);
            }
        }

        [AllowAnonymous, HttpPost]
        public JsonResult Login(FormCollection p_collection)
        {
            // login: POST
            string loginName = OurUtility.ValueOf(p_collection, "u");
            string password = OurUtility.ValueOf(p_collection, "p");

            UserX user = new UserX(Request, Response, loginName, password);

            if (user.UserId <= 0)
            {
                var msg = new { Success = false, user.DC, Message = user.Login_Message, Code = user.Login_Code, Message2 = user.Login_Message2, Version = Configuration.VERSION };
                return Json(msg, JsonRequestBehavior.AllowGet);
            }

            var result = new
            {
                Success = true
                , user.UserId
                , user.LoginName
                , Name = user.FullName
                , user.Title
                , user.Department
                , user.Email
                , user.UserInterface

                , user.DC

                , Time = DateTime.Now.ToString("dd-MMM-yyyy HH:mm:ss.ffff")
                , Version = Configuration.VERSION
            };

            return Json(result, JsonRequestBehavior.AllowGet);
        }

        [AllowAnonymous, HttpGet]
        public JsonResult Login()
        {
            // login: GET
            string loginName = OurUtility.ValueOf(Request, "u");
            string password = OurUtility.ValueOf(Request, "p");

            UserX user = new UserX(Request, Response, loginName, password);

            if (user.UserId <= 0)
            {
                var msg = new { Success = false, user.DC, Message = user.Login_Message, Code = user.Login_Code, Message2 = user.Login_Message2, Version = Configuration.VERSION };
                return Json(msg, JsonRequestBehavior.AllowGet);
            }

            var info = UserX.Information(user, false, false);

            return Json(info, JsonRequestBehavior.AllowGet);
        }

        [AllowAnonymous]
        public JsonResult Logout()
        {
            string time_begin = DateTime.Now.ToString("dd-MMM-yyyy HH:mm:ss.ffff");

            try
            {
                // -- FBA
                //TODO: FormsAuthentication.SignOut();

                // -- All Cookies
                ExpireAllCookies();

                // -- Session
                //Session.Clear();

                // -- Table
                UserX.Log_Activity(UserX.Get_Token(Request));
            }
            catch {}

            // Print Execution "Timing"
            OurUtility.Set_Response_PrintTiming(Response, time_begin);

            var result = new
            {
                Success = true
                , Time = DateTime.Now.ToString("dd-MMM-yyyy HH:mm:ss.ffff")
                , Version = Configuration.VERSION
            };

            return Json(result, JsonRequestBehavior.AllowGet);
        }

        private void ExpireAllCookies()
        {
            try
            {
                if (System.Web.HttpContext.Current != null)
                {
                    int cookieCount = System.Web.HttpContext.Current.Request.Cookies.Count;
                    for (var i = 0; i < cookieCount; i++)
                    {
                        var cookie = System.Web.HttpContext.Current.Request.Cookies[i];
                        if (cookie != null)
                        {
                            var expiredCookie = new HttpCookie(cookie.Name)
                            {
                                Expires = DateTime.Now.AddDays(-1),
                                Domain = cookie.Domain
                            };
                            System.Web.HttpContext.Current.Response.Cookies.Add(expiredCookie); // overwrite it
                        }
                    }

                    // clear cookies server side
                    System.Web.HttpContext.Current.Request.Cookies.Clear();
                }
            }
            catch {}
        }

        public JsonResult Info()
        {
            string time_begin = DateTime.Now.ToString("dd-MMM-yyyy HH:mm:ss.ffff");

            bool is_Show_User_Menu = OurUtility.ValueOf(Request, "u_menu").Equals("1");
            bool is_Show_User_Relation = OurUtility.ValueOf(Request, "u_relation").Equals("1");

            UserX user = new UserX(Request);
            
            string find_user = OurUtility.ValueOf(Request, "u");

            if (string.IsNullOrEmpty(find_user))
            {
                // Print Execution "Timing"
                OurUtility.Set_Response_PrintTiming(Response, time_begin);

                Permission_Item permission_Item = null;
                string check_page = OurUtility.ValueOf(Request, "check_page");
                if ( ! string.IsNullOrEmpty(check_page))
                {
                    Permission.Check(Request, check_page, user, ref permission_Item);
                }

                var data = new { Success = true, Permission = permission_Item, User = UserX.Information(user, is_Show_User_Menu, is_Show_User_Relation), Version = Configuration.VERSION};

                return Json(data, JsonRequestBehavior.AllowGet);
            }

            // -- This is for other purposes
            
            /*if (_userId <= 0)
            {
                var msg2 = new { Success = false, Message = PermissionController.ERROR_PERMISSION_READ + " [not Login]", MessageDetail = string.Empty, Version = Configuration.VERSION };
                return Json(msg2, JsonRequestBehavior.AllowGet);
            }*/

            // - ambil UserInfo dari ActiveDirectory
            int _DC = 0;
            string _name = string.Empty;
            string _title = string.Empty;
            string _department = string.Empty;
            string _email = string.Empty;
            string msg = string.Empty;
            bool success = false;

            string find_dc = OurUtility.ValueOf(Request, "dc");

            if (string.IsNullOrEmpty(find_dc))
            {
                success = ActiveDirectory.Info(find_user, ref _DC, ref _name, ref _title, ref _department, ref _email, ref msg);
            }
            else
            {
                _DC = OurUtility.ToInt32(find_dc);
                success = ActiveDirectory.Info(find_user, _DC, ref msg);
            }

            var result = new
            {
                Success = success
                , LoginName = find_user
                , Name = _name
                , Title = _title
                , Department = _department
                , Email = _email
                , DC = _DC

                , Message = msg
                , Time = DateTime.Now.ToString("dd-MMM-yyyy HH:mm:ss.ffff")
                , Version = Configuration.VERSION
            };

            // Print Execution "Timing"
            OurUtility.Set_Response_PrintTiming(Response, time_begin);

            return Json(result, JsonRequestBehavior.AllowGet);
        }

        public JsonResult FindAll()
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

            string user_find = OurUtility.ValueOf(Request, "u");
            string message = string.Empty;
            int _DC = 0;
            IList<string> users = ActiveDirectory.FindAll(user_find, ref _DC, ref message);

            var msg2 = new { Success = (users != null), DC = _DC, Users = users, Message = message, MessageDetail = string.Empty
                            , Time = DateTime.Now.ToString("dd-MMM-yyyy HH:mm:ss.ffff")
                            , Version = Configuration.VERSION };

            return Json(msg2, JsonRequestBehavior.AllowGet);
        }
                
        public ActionResult Picture(string id)
        {
            //string id = Request["id"];

            string dir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"/Picture");
            var path = string.Empty;

            try
            {
                // little bit more safe
                id = id.Replace(@"/", "");
                id = id.Replace(@"\", "");

                path = Path.Combine(dir, id + ".png");
                if (System.IO.File.Exists(path))
                {
                    return base.File(path, "image/jpeg");
                }
                else
                {
                    path = Path.Combine(dir, "1.png");
                    return base.File(path, "image/jpeg");
                }
            }
            catch
            {
                path = Path.Combine(dir, "1.png");
                return base.File(path, "image/jpeg");
            }
        }

        public JsonResult Activities()
        {
            bool is_Show_User_Menu = OurUtility.ValueOf(Request, "u_menu").Equals("1");
            bool is_Show_User_Relation = OurUtility.ValueOf(Request, "u_relation").Equals("1");

            string time_begin = DateTime.Now.ToString("dd-MMM-yyyy HH:mm:ss.ffff");
            string time_end = string.Empty;

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
                            from dt in db.User_Activity
                            join u in db.Users on dt.LoginName equals u.LoginName
                            orderby dt.RecordId descending
                            select new Model_View_User
                            {
                                RecordId = dt.RecordId
                                , UserId = u.UserId
                                , LoginName = u.LoginName
                                , FullName = u.FullName
                                , Title = u.Title
                                , Department = u.Department
                                , Email = u.Email
                                , CreatedOn = dt.CreatedOn
                                , LastActivity = dt.LastActivity
                            }
                        );
                
                var items = dataQuery.Take(50).ToList().OrderByDescending(x => x.RecordId).ToList();

                try
                {
                    items.ForEach(c =>
                    {
                        c.CreatedOn_Str = OurUtility.DateFormat(c.CreatedOn, "dd-MMM-yyyy HH:mm:ss.ffff");
                        c.LastActivity_Str = OurUtility.DateFormat(c.CreatedOn, "dd-MMM-yyyy HH:mm:ss.ffff");
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

            // -- Actual code
            try
            {
                using (MERCY_Ctx db = new MERCY_Ctx())
                {
                    var dataQuery =
                            (
                                from d in db.Users
                                orderby d.FullName
                                select new
                                {
                                    id = d.UserId
                                    , text = d.FullName
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

        public JsonResult GetM_ddl()
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
                                from d in db.Users
                                where d.IsMasterData
                                orderby d.FullName
                                select new
                                {
                                    id = d.UserId
                                    , text = d.FullName
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

        public JsonResult GetMtn_ddl()
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
                                from d in db.Users
                                where d.IsLabMaintenance
                                orderby d.FullName
                                select new
                                {
                                    id = d.UserId
                                    , text = d.FullName
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

            string loginName = OurUtility.ValueOf(p_collection, "LoginName");
            string password = OurUtility.ValueOf(p_collection, "p");
            string fullName = OurUtility.ValueOf(p_collection, "FullName");
            string title = OurUtility.ValueOf(p_collection, "Title");
            string department = OurUtility.ValueOf(p_collection, "Department");
            string email = OurUtility.ValueOf(p_collection, "Email");
            bool is_activedirectory = OurUtility.ValueOf(p_collection, "is_activedirectory").Equals("1");
            bool isactive = OurUtility.ValueOf(p_collection, "isactive").Equals("1");
            string sites = OurUtility.ValueOf(p_collection, "Sites");
            string companies = OurUtility.ValueOf(p_collection, "Companies");
            string groups = OurUtility.ValueOf(p_collection, "Groups");

            string message = string.Empty;
            int id = 0;
            int _DC = 0;
            if (is_activedirectory)
            {
                if ( ! ActiveDirectory.Info(loginName, ref _DC, ref fullName, ref title, ref department, ref email, ref message))
                {
                    var result = new { Success = false, Permission = permission_Item, DC = _DC, Message = "User not found in Active Directory", MessageDetail = message, Version = Configuration.VERSION };
                    return Json(result, JsonRequestBehavior.AllowGet);
                }
            }

            if (Create(user.UserId, loginName, password, fullName, title, department, email, is_activedirectory, isactive, sites, companies, groups, ref id, ref message))
            {
                var result = new { Success = true, Permission = permission_Item, Message = BaseConstants.MESSAGE_CREATE_SUCCESS, MessageDetail = string.Empty, Version = Configuration.VERSION, Data = id };
                return Json(result, JsonRequestBehavior.AllowGet);
            }
            else
            {
                var result = new { Success = false, Permission = permission_Item, Message = message, MessageDetail = string.Empty, Version = Configuration.VERSION};
                return Json(result, JsonRequestBehavior.AllowGet);
            }
        }

        internal static bool Create(int p_executedBy, string p_loginName
                                    , string p_name, string p_title, string p_department, string p_email
                                    , ref int p_id, ref string p_message)
        {
            bool is_activedirectory = true;
            bool isactive = false;

            p_id = -1;
            p_message = string.Empty;

            return Create(p_executedBy, p_loginName, UserX.MASK_PASSWORD, p_name, p_title, p_department, p_email
                            , is_activedirectory, isactive
                            , string.Empty, string.Empty, string.Empty, ref p_id, ref p_message);
        }

        internal static bool Create(int p_executedBy, string p_loginName, string p_password, string p_fullName, string p_title, string p_department, string p_email
                            , bool p_is_activedirectory, bool p_isactive
                            , string p_sites, string p_companies, string p_groups, ref int p_id, ref string p_message)
        {
            bool result = false;

            p_message = string.Empty;
            p_id = -1;

            try
            {
                using (MERCY_Ctx db = new MERCY_Ctx())
                {
                    var data = new User
                    {
                        Is_ActiveDirectory = p_is_activedirectory
                    };
                    if (data.Is_ActiveDirectory)
                    {
                        // we don't store Password for User:ActiveDirectory
                        data.Pwd_DB = UserX.MASK_PASSWORD;
                    }
                    else
                    {
                        data.Pwd_DB = OurUtility.Sha256_Hash(p_password);
                    }

                    data.LoginName = p_loginName;
                    data.IsActive = p_isactive;
                    data.FullName = p_fullName;
                    data.Title = p_title;

                    data.Department = p_department;
                    data.Email = p_email;
                    data.UserInterface = string.Empty;

                    data.IsAdmin = false;
                    data.IsConsumable = false;
                    data.IsCPL = false;
                    data.IsLabMaintenance = false;
                    data.IsMasterData = false;
                    data.IsReport = false;
                    data.IsUserBiasa = false;
                    data.IsUserLab = false;

                    data.CreatedOn = DateTime.Now;
                    data.CreatedBy = p_executedBy;

                    db.Users.Add(data);
                    db.SaveChanges();

                    p_id = data.UserId;

                    // only process -- if Necessary
                    if (string.IsNullOrEmpty(p_sites) && string.IsNullOrEmpty(p_companies) && string.IsNullOrEmpty(p_groups)) {}
                    else
                    {
                        try
                        {
                            using (var connection = new SqlConnection(db.Database.Connection.ConnectionString))
                            {
                                connection.Open();

                                string msgx = string.Empty;
                                User_Site_Add(connection, p_id, p_sites, p_executedBy, ref msgx);
                                User_Company_Add(connection, p_id, p_companies, p_executedBy, ref msgx);
                                User_Group_Add(connection, p_id, p_groups, p_executedBy, ref msgx);

                                connection.Close();
                            }
                        }
                        catch {}
                    }

                    result = true;
                }
            }
            catch (Exception ex)
            {
                p_message = ex.Message;
            }

            return result;
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
                int id = OurUtility.ToInt32(Request[".id"]);
                string msg = string.Empty;

                string loginName = OurUtility.ValueOf(p_collection, "LoginName");
                string password = OurUtility.ValueOf(p_collection, "p");
                string fullName = OurUtility.ValueOf(p_collection, "FullName");
                string title = OurUtility.ValueOf(p_collection, "Title");
                string department = OurUtility.ValueOf(p_collection, "Department");
                string email = OurUtility.ValueOf(p_collection, "Email");
                bool is_activedirectory = OurUtility.ValueOf(p_collection, "is_activedirectory").Equals("1");
                bool isactive = OurUtility.ValueOf(p_collection, "isactive").Equals("1");
                string sites = OurUtility.ValueOf(p_collection, "Sites");
                string companies = OurUtility.ValueOf(p_collection, "Companies");
                string groups = OurUtility.ValueOf(p_collection, "Groups");

                using (MERCY_Ctx db = new MERCY_Ctx())
                {
                    var dataQuery =
                        (
                            from d in db.Users
                            where d.UserId == id
                            select d
                        );

                    var data = dataQuery.SingleOrDefault();

                    data.Is_ActiveDirectory = is_activedirectory;
                    if (data.Is_ActiveDirectory)
                    {
                        // Reset information of User
                        // -- get from ActiveDirectory
                        int _DC = 0;
                        if ( ! ActiveDirectory.Info(loginName, ref _DC, ref fullName, ref title, ref department, ref email, ref msg))
                        {
                            var result2 = new { Success = false, Permission = permission_Item, DC = _DC, Message = "User not found in Active Directory", MessageDetail = msg, Version = Configuration.VERSION };
                            return Json(result2, JsonRequestBehavior.AllowGet);
                        }

                        // we don't store Password for User:ActiveDirectory
                        data.Pwd_DB = UserX.MASK_PASSWORD;
                    }
                    else
                    {
                        // change only, if necessary
                        if (password == UserX.MASK_PASSWORD)
                        {
                            // tidak melakukan perubahan Password
                        }
                        else
                        {
                            // lakukan perubahan Password
                            data.Pwd_DB = OurUtility.Sha256_Hash(password);
                        }
                    }

                    data.LoginName = loginName;
                    data.IsActive = isactive;
                    data.FullName = fullName;
                    data.Title = title;

                    data.Department = department;
                    data.Email = email;
                    data.UserInterface = string.Empty;

                    data.IsAdmin = false;
                    data.IsConsumable = false;
                    data.IsCPL = false;
                    data.IsLabMaintenance = false;
                    data.IsMasterData = false;
                    data.IsReport = false;
                    data.IsUserBiasa = false;
                    data.IsUserLab = false;

                    data.LastModifiedOn = DateTime.Now;
                    data.LastModifiedBy = user.UserId;

                    db.SaveChanges();

                    try
                    {
                        using (var connection = new SqlConnection(db.Database.Connection.ConnectionString))
                        {
                            connection.Open();

                            User_Site_Delete(connection, id, ref msg);
                            User_Company_Delete(connection, id, ref msg);
                            User_Group_Delete(connection, id, ref msg);

                            User_Site_Add(connection, id, sites, user.UserId, ref msg);
                            User_Company_Add(connection, id, companies, user.UserId, ref msg);
                            User_Group_Add(connection, id, groups, user.UserId, ref msg);

                            connection.Close();
                        }
                    }
                    catch {}

                    var result = new { Success = true, Permission = permission_Item, Message = BaseConstants.MESSAGE_UPDATE_SUCCESS, MessageDetail = string.Empty, Version = Configuration.VERSION, Data = id };
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
                long id = OurUtility.ToInt64(Request[".id"]);

                using (MERCY_Ctx db = new MERCY_Ctx())
                {
                    var dataQuery =
                        (
                            from d in db.Sites
                            where d.SiteId == id
                            select d
                        );

                    var data = dataQuery.SingleOrDefault();
                    db.Sites.Remove(data);

                    db.SaveChanges();

                    var result = new { Success = true, Permission = permission_Item, Message = BaseConstants.MESSAGE_DELETE_SUCCESS, MessageDetail = string.Empty, Version = Configuration.VERSION, Data = id };
                    return Json(result, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                var result = new { Success = false, Permission = permission_Item, ex.Message, MessageDetail = string.Empty, Version = Configuration.VERSION, Items = string.Empty, Total = 0 };
                return Json(result, JsonRequestBehavior.AllowGet);
            }
        }

        internal static bool User_Site_Add(SqlConnection p_db, int p_userId, string p_sites, int p_executedBy, ref string p_message)
        {
            bool result = false;

            p_message = string.Empty;

            try
            {
                p_sites = System.Uri.UnescapeDataString(p_sites);
                p_sites = System.Uri.UnescapeDataString(p_sites);
                p_sites = System.Web.HttpUtility.HtmlDecode(p_sites);

                string[] sites = p_sites.Split(',');
                string sql = string.Empty;
                SqlCommand command = p_db.CreateCommand();

                foreach (string site in sites)
                {
                    try
                    {
                        sql = string.Format(@"insert into UserSite(UserId, SiteId, CreatedBy, CreatedOn) values({0}, {1}, {2}, GetDate())", p_userId, site, p_executedBy);
                        command.CommandText = sql;
                        command.ExecuteNonQuery();
                    }
                    catch {}
                }

                result = true;
            }
            catch {}

            return result;
        }

        internal static bool User_Company_Add(SqlConnection p_db, int p_userId, string p_companies, int p_executedBy, ref string p_message)
        {
            bool result = false;

            p_message = string.Empty;

            try
            {
                p_companies = System.Uri.UnescapeDataString(p_companies);
                p_companies = System.Uri.UnescapeDataString(p_companies);
                p_companies = System.Web.HttpUtility.HtmlDecode(p_companies);

                string[] companies = p_companies.Split(',');
                string sql = string.Empty;
                SqlCommand command = p_db.CreateCommand();

                foreach (string company in companies)
                {
                    try
                    {
                        sql = string.Format(@"insert into UserCompany(UserId, CompanyCode, CreatedBy, CreatedOn) values({0}, '{1}', {2}, GetDate())", p_userId, company, p_executedBy);
                        command.CommandText = sql;
                        command.ExecuteNonQuery();
                    }
                    catch {}
                }

                result = true;
            }
            catch {}

            return result;
        }

        internal static bool User_Group_Add(SqlConnection p_db, int p_userId, string p_groups, int p_executedBy, ref string p_message)
        {
            bool result = false;

            p_message = string.Empty;

            try
            {
                p_groups = System.Uri.UnescapeDataString(p_groups);
                p_groups = System.Uri.UnescapeDataString(p_groups);
                p_groups = System.Web.HttpUtility.HtmlDecode(p_groups);

                string[] groups = p_groups.Split(',');
                string sql = string.Empty;
                SqlCommand command = p_db.CreateCommand();

                foreach (string group in groups)
                {
                    try
                    {
                        sql = string.Format(@"insert into UserGroup(UserId, GroupId, CreatedBy, CreatedOn) values({0}, {1}, {2}, GetDate())", p_userId, group, p_executedBy);
                        command.CommandText = sql;
                        command.ExecuteNonQuery();
                    }
                    catch {}
                }

                result = true;
            }
            catch {}

            return result;
        }
        
        internal static bool User_Site_Delete(SqlConnection p_db, int p_userId, ref string p_message)
        {
            bool result = false;

            p_message = string.Empty;

            try
            {
                SqlCommand command = p_db.CreateCommand();
                string sql = string.Format(@"delete from UserSite where UserId = {0}", p_userId);
                command.CommandText = sql;
                command.ExecuteNonQuery();

                result = true;
            }
            catch {}

            return result;
        }

        internal static bool User_Company_Delete(SqlConnection p_db, int p_userId, ref string p_message)
        {
            bool result = false;

            p_message = string.Empty;

            try
            {
                SqlCommand command = p_db.CreateCommand();
                string sql = string.Format(@"delete from UserCompany where UserId = {0}", p_userId);
                command.CommandText = sql;
                command.ExecuteNonQuery();

                result = true;
            }
            catch {}

            return result;
        }

        internal static bool User_Group_Delete(SqlConnection p_db, int p_userId, ref string p_message)
        {
            bool result = false;

            p_message = string.Empty;

            try
            {
                SqlCommand command = p_db.CreateCommand();
                string sql = string.Format(@"delete from UserGroup where UserId = {0}", p_userId);
                command.CommandText = sql;
                command.ExecuteNonQuery();

                result = true;
            }
            catch {}

            return result;
        }
    }
}