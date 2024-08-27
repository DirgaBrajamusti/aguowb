using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using System.Data;
using System.Data.SqlClient;

using MERCY.Data.EntityFramework;
using MERCY.Web.BackEnd.Helpers;
using MERCY.Web.BackEnd.Models;

using MERCY.Web.BackEnd.Areas.Api.Controllers;

namespace MERCY.Web.BackEnd.Security
{
    public class UserX : User
    {
        public const string MASK_PASSWORD = "*****";

        public int Login_Code { get; set; }
        public string Login_Message { get; set; }
        public string Login_Message2 { get; set; }
        public int DC { get; set; }

        private UserX() {}

        public UserX(HttpRequestBase p_request)
        {
            string msg = string.Empty;
            Check_Token(p_request, ref msg);
        }

        public UserX(HttpRequestBase p_request, HttpResponseBase p_response, string p_loginName, string p_password)
        {
            LoginX(p_request, p_response, p_loginName, p_password);
        }

        private bool Check(string p_loginName)
        {
            bool result = false;

            UserId = -1;

            try
            {
                using (MERCY_Ctx db = new MERCY_Ctx())
                {
                    var dataQuery =
                            (
                                from d in db.Users
                                where d.LoginName == p_loginName
                                select d
                            );

                    var data = dataQuery.SingleOrDefault();

                    UserId = data.UserId;
                    LoginName = data.LoginName;
                    FullName = data.FullName;
                    Title = data.Title;
                    Department = data.Department;
                    Email = data.Email;
                    Is_ActiveDirectory = data.Is_ActiveDirectory;
                    IsActive = data.IsActive;
                    Pwd_DB = data.Pwd_DB;

                    IsAdmin = data.IsAdmin;
                    IsCPL = data.IsCPL;
                    IsLabMaintenance = data.IsLabMaintenance;
                    UserInterface = data.UserInterface;
                    IsUserBiasa = data.IsUserBiasa;
                    IsUserLab = data.IsUserLab;
                    IsMasterData = data.IsMasterData;
                    IsConsumable = data.IsConsumable;

                    result = true;
                }
            }
            catch {}

            return result;
        }


        // Code :1, User exist in Active Directroy
        //      Status: ok, Authentication in Active Directory is success
        //      Status: failed, Authentication failed
        // Code :2, User exist in Database
        //      Status: ok, Authentication in Database is success
        //      Status: failed, Authentication failed
        // Code : -1, User is NOT exist in User Table
        // Code : -2, The user name or password is incorrect
        // Code : -3, {User ActiveDirectory} The user name or password is incorrect 
        // Code : -4, {User ActiveDirectory} User is NOT exist in Active Directroy
        // Code : -5, The user is Not Active
        // Code : -6, {User Database} The user name or password is incorrect 
        // Code : -10, Unknown
        // Code : -11, LoginName is empty
        // Code : -20, The user name or password is incorrect 
        // Code : -21, New user is created
        // Code : -22, Failed when trying to create User
        // Code : -23, Can't create user. UserName is not valid in ActiveDirectory
        internal void LoginX(HttpRequestBase p_request, HttpResponseBase p_response, string p_loginName, string p_password)
        {
            string time_begin = DateTime.Now.ToString("dd-MMM-yyyy HH:mm:ss.ffff");

            string status = "failed";
            string msg = string.Empty;

            string msg2 = string.Empty;
            string token = string.Empty;

            UserId = 0;
            Login_Code = 0;
            Login_Message = string.Empty;
            Login_Message2 = string.Empty;

            //Logout();

            if (string.IsNullOrEmpty(p_loginName))
            {
                Login_Message = "LoginName is empty";

                return;
            }

            try
            {
                p_loginName = p_loginName.ToLower().Trim();

                // -- check in User Table
                if (Check(p_loginName))
                {
                    // -- User is exist in User Table
                    if ( ! IsActive)
                    {
                        // "Mark it"
                        UserId = -1 * UserId;

                        Login_Code = -5;
                        Login_Message = "The user is Not Active.";
                    }
                    else if ( ! Is_ActiveDirectory)
                    {
                        if (Pwd_DB == OurUtility.Sha256_Hash(p_password))
                        {
                            token = OurUtility.Sha256_Hash(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.ffff"));
                            status = "ok";
                        }
                        else
                        {
                            Login_Code = -6;
                            Login_Message = "The user name or password is incorrect.";

                            // reset value
                            Reset();
                        }
                    }
                    else
                    {
                        int _DC = 0;
                        if (ActiveDirectory.Authenticate(p_loginName, p_password, ref _DC, ref msg))
                        {
                            token = OurUtility.Sha256_Hash(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.ffff"));
                            status = "ok";
                        }
                        else
                        {
                            Login_Code = -3;
                            Login_Message = "The user name or password is incorrect.";
                            Login_Message2 = msg;

                            // reset value
                            Reset();
                        }

                        DC = _DC;
                    }
                }
                else
                {
                    // -- User is NOT exist in User Table
                    Login_Code = -1;
                    Login_Message = "User is NOT exist in User Table.";

                    int _DC = 0;
                    string _name = string.Empty;
                    string _title = string.Empty;
                    string _department = string.Empty;
                    string _email = string.Empty;

                    if (ActiveDirectory.Info_Himself(p_loginName, p_password, ref _DC, ref _name, ref _title, ref _department, ref _email, ref msg))
                    {
                        int _id = 0;
                        string _message = string.Empty;
                        if (UserController.Create(0, p_loginName, _name, _title, _department, _email, ref _id, ref _message))
                        {
                            Login_Code = -21;
                            Login_Message = "You're successfully registered, please contact MERCY System Admin to get your privileges";
                        }
                        else
                        {
                            Login_Code = -22;
                            Login_Message = "Failed when trying to create User";
                        }
                    }
                    else
                    {
                        Login_Code = -20;
                        Login_Message = msg; // "Can't create user. UserName is not valid in ActiveDirectory";
                    }

                    DC = _DC;
                }
            }
            catch (Exception ex)
            {
                Login_Code = -10; // Unknown
                Login_Message = ex.Message;

                // reset value
                Reset();
            }

            if (status == "ok")
            {
                //TODO: FormsAuthentication.SetAuthCookie(p_loginName.ToLower(), true);

                //FormsAuthentication.RedirectFromLoginPage(loginName, false);

                string ipAddress = OurUtility.Get_Header(p_request, OurUtility.END_USER_IP_ADDRESS);
                Log_Activity(p_loginName, token, true, p_request, ipAddress);
            }
            else
            {
                // Clear all
                //Logout();
            }

            OurUtility.Set_Response_Header(p_response, OurUtility.MERCY_TOKEN, token);
        }

        public static string Get_Token(HttpRequestBase p_request)
        {
            return OurUtility.Get_Header(p_request, OurUtility.MERCY_TOKEN);
        }

        private bool Check_Token(HttpRequestBase p_request, ref string p_message)
        {
            bool result = false;

            string token = Get_Token(p_request);

            p_message = string.Empty;

            try
            {
                using (MERCY_Ctx db = new MERCY_Ctx())
                {
                    var dataQuery =
                            (
                                from d in db.Users
                                join a in db.User_Activity on d.LoginName equals a.LoginName
                                where a.Token == token
                                select d
                            );

                    var data = dataQuery.SingleOrDefault();

                    LoginName = data.LoginName;
                    UserId = data.UserId;
                    FullName = data.FullName;
                    Title = data.Title;
                    Department = data.Department;
                    Email = data.Email;
                    Is_ActiveDirectory = data.Is_ActiveDirectory;
                    IsActive = data.IsActive;
                    IsAdmin = data.IsAdmin;
                    IsCPL = data.IsCPL;
                    IsLabMaintenance = data.IsLabMaintenance;
                    UserInterface = data.UserInterface;
                    IsUserBiasa = data.IsUserBiasa;
                    IsUserLab = data.IsUserLab;
                    IsMasterData = data.IsMasterData;
                    IsConsumable = data.IsConsumable;

                    result = true;
                }
            }
            catch (Exception ex)
            {
                p_message = ex.Message;
            }

            return result;
        }

        private void Reset()
        {
            LoginName = string.Empty;
            UserId = 0;
            FullName = string.Empty;
            Title = string.Empty;
            Department = string.Empty;
            Email = string.Empty;
            IsActive = false;
            Is_ActiveDirectory = false;
            UserInterface = string.Empty;
            Pwd_DB = string.Empty;

            IsAdmin = false;
            IsCPL = false;
            IsLabMaintenance = false;
            IsUserBiasa = false;
            IsUserLab = false;
            IsMasterData = false;
            IsConsumable = false;
        }

        private static bool Log_Activity(string p_user, string p_token, bool p_forceNew, HttpRequestBase p_request, string p_ipAddress)
        {
            bool result = false;

            try
            {
                string userAgent = p_request.UserAgent;

                if (p_user == "Guest") return result;
                p_user = p_user.ToLower();

                using (MERCY_Ctx db = new MERCY_Ctx())
                {
                    User_Activity data = new User_Activity
                    {
                        LoginName = p_user,
                        Token = p_token,
                        CreatedOn = DateTime.Now,
                        IsActive = true
                    };

                    if (string.IsNullOrEmpty(userAgent)) userAgent = string.Empty;

                    if (userAgent.Length > 250)
                    {
                        userAgent = userAgent.Substring(0, 250);
                    }
                    data.UserAgent = userAgent;
                    data.IPAddress_of_EndUser = p_ipAddress;

                    data.LastActivity = DateTime.Now;

                    db.User_Activity.Add(data);
                    db.SaveChanges();

                    result = true;
                }
            }
            catch (Exception)
            {
                //result = ex.Message;
            }

            return result;
        }

        public static void Log_Activity(string p_token)
        {
            try
            {
                using (MERCY_Ctx db = new MERCY_Ctx())
                {
                    var dataQuery =
                        (
                            from d in db.User_Activity
                            where d.Token == p_token
                            select d
                        );

                    var data = dataQuery.SingleOrDefault();

                    data.IsActive = false;
                    data.LastActivity = DateTime.Now;

                    //db.sessions.Remove(data);
                    db.SaveChanges();
                }
            }
            catch { }
        }

        public static object Relations(MERCY_Ctx p_db, int p_userId)
        {
            List<Model_View_Site2> items_Sites = new List<Model_View_Site2>();
            List<Model_View_Company2> items_Companies = new List<Model_View_Company2>();
            List<Model_View_Group2> items_Groups = new List<Model_View_Group2>();

            try
            {
                var dataQuery_Sites =
                            (
                                from d in p_db.UserSites
                                join s in p_db.Sites on d.SiteId equals s.SiteId
                                where d.UserId == p_userId
                                orderby s.SiteName
                                select new Model_View_Site2
                                {
                                    SiteId = d.SiteId
                                    , SiteName = s.SiteName
                                }
                            );

                items_Sites = dataQuery_Sites.ToList();

                var dataQuery_Companies =
                         (
                             from d in p_db.UserCompanies
                             join c in p_db.Companies on d.CompanyCode equals c.CompanyCode
                             where d.UserId == p_userId
                             orderby c.Name
                             select new Model_View_Company2
                             {
                                 CompanyCode = d.CompanyCode
                                 , CompanyName = c.Name
                                 , SiteId = c.SiteId
                             }
                         );

                items_Companies = dataQuery_Companies.ToList();

                var dataQuery_Groups =
                        (
                            from d in p_db.UserGroups
                            join g in p_db.Groups on d.GroupId equals g.GroupId
                            where d.UserId == p_userId
                            orderby g.GroupName
                            select new Model_View_Group2
                            {
                                GroupId = d.GroupId
                                , GroupName = g.GroupName
                            }
                        );

                items_Groups = dataQuery_Groups.ToList();
            }
            catch { }

            var result = new
            {

                Sites = items_Sites
                , Companies = items_Companies
                , Groups = items_Groups
            };

            return result;
        }

        public static List<Model_View_Menu> Menu(SqlConnection p_db, int p_userId)
        {
            List<Model_View_Menu> items = new List<Model_View_Menu>();

            try
            {
                SqlCommand command = p_db.CreateCommand();

                command.CommandText = string.Format(@"
                                                            select distinct
                                                                d.menuid
                                                                , d.menuname
                                                                , d.url
                                                                , d.ordering
                                                                , d.parentid
                                                                , d.level
                                                            from menu d
                                                                , permission p
                                                                --, group 
                                                                , usergroup ug
                                                            where ug.userid = {0}
                                                                and d.MenuId = p.MenuId
                                                                and p.GroupId = ug.GroupId
                                                                and (p.isview = 1 or p.isadd = 1 or p.isdelete = 1 or p.isupdate = 1 or p.isactive = 1)
                                                            --order by  d.level, d.ordering, d.menuname
                                                            ", p_userId);

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
                            Level = OurUtility.ToInt32(reader["level"].ToString())
                        };

                        items.Add(menu);
                    }
                }

                command.CommandText = string.Format(@"
                                                            select d2.*
                                                            from menu d2
                                                            where d2.menuid in
                                                            (
                                                                select d.parentid
                                                                from menu d
                                                                    , permission p
                                                                    --, group 
                                                                    , usergroup ug
                                                                where ug.userid = {0}
                                                                    and d.MenuId = p.MenuId
                                                                    and p.GroupId = ug.GroupId
                                                                    and (p.isview = 1 or p.isadd = 1 or p.isdelete = 1 or p.isupdate = 1 or p.isactive = 1)
                                                            )
                                                            and d2.menuid not in
                                                            (
                                                                select d.menuid
                                                                from menu d
                                                                    , permission p
                                                                    --, group 
                                                                    , usergroup ug
                                                                where ug.userid = {0}
                                                                    and d.MenuId = p.MenuId
                                                                    and p.GroupId = ug.GroupId
                                                                    and (p.isview = 1 or p.isadd = 1 or p.isdelete = 1 or p.isupdate = 1 or p.isactive = 1)
                                                            )
                                                            ", p_userId);

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
                            Level = OurUtility.ToInt32(reader["level"].ToString())
                        };

                        items.Add(menu);
                    }
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

                var dataQuery = from d in items
                                where d.MenuId > 0
                                orderby d.Level, d.Ordering, d.MenuName
                                select d;

                items = dataQuery.ToList();
            }
            catch { }

            return items;
        }

        public static object Information(UserX p_userd, bool p_Is_ShowMenu, bool p_Is_ShowRelation)
        {
            object relations = null;
            List<Model_View_Menu> menus = null;

            if (p_Is_ShowMenu || p_Is_ShowRelation)
            {
                using (MERCY_Ctx db = new MERCY_Ctx())
                {
                    if (p_Is_ShowRelation)
                    {
                        relations = UserX.Relations(db, p_userd.UserId);
                    }

                    if (p_Is_ShowMenu)
                    {
                        using (var connection = new SqlConnection(db.Database.Connection.ConnectionString))
                        {
                            connection.Open();

                            menus = UserX.Menu(connection, p_userd.UserId);

                            connection.Close();

                        }
                    }
                }
            }

            var result = new
            {
                Success = true
                ,
                p_userd.UserId
                ,
                p_userd.LoginName
                , Name = p_userd.FullName
                ,
                p_userd.Title
                ,
                p_userd.Department
                ,
                p_userd.Email
                ,
                p_userd.UserInterface

                , Menus = menus
                , Relations = relations

                , Time = DateTime.Now.ToString("dd-MMM-yyyy HH:mm:ss.ffff")
                , Version = Configuration.VERSION
            };

            return result;
        }

        public static object Information(UserX p_userd)
        {
            return Information(p_userd, true, true);
        }
    }
}