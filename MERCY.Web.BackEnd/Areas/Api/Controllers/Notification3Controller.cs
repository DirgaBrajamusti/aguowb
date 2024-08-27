using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

using System.Data;
using System.Data.SqlClient;

using MERCY.Data.EntityFramework;
using MERCY.Web.BackEnd.Models;
using MERCY.Web.BackEnd.Helpers;

namespace MERCY.Web.BackEnd.Areas.Api.Controllers
{
    public class Notification3Controller : Controller
    {
        public void InitializeController(System.Web.Routing.RequestContext context)
        {
            base.Initialize(context);
        }

        public string Index()
        {
            return (new TestController()).Index();
        }

        public string TemplateFolder
        {
            get
            {
                string result = @"c:\temp";

                try
                {
                    result = Server.MapPath("~") + @"\templates";
                    result = result.Replace(@"\\", @"\");
                }
                catch { }

                return result;
            }
        }

        private string UploadFolder
        {
            get
            {
                string result = @"c:\temp";

                try
                {
                    result = Server.MapPath("~") + @"\" + Configuration.FolderUpload + @"\";
                    result = result.Replace(@"\\", @"\");
                }
                catch { }

                return result;
            }
        }

        internal static List<Model_View_UserList_access_Menu> UserList_access_Menu(string p_menu_Name, ref string p_company)
        {
            List<Model_View_UserList_access_Menu> result = new List<Model_View_UserList_access_Menu>();

            try
            {
                using (MERCY_Ctx db = new MERCY_Ctx())
                {
                    using (var connection = new SqlConnection(db.Database.Connection.ConnectionString))
                    {
                        connection.Open();

                        result = UserList_access_Menu(connection, p_menu_Name, p_company);

                        connection.Close();
                    }
                }
            }
            catch {}

            return result;
        }

        internal static List<Model_View_UserList_access_Menu> UserList_access_Menu(SqlConnection p_connection, string p_menu_Name, string p_company, int p_site = 0)
        {
            List<Model_View_UserList_access_Menu> result = new List<Model_View_UserList_access_Menu>();

            try
            {
                SqlCommand command = p_connection.CreateCommand();

                command.CommandText = string.Format(@"
                                                        select DISTINCT u.userid, u.loginname, u.FullName, u.is_activedirectory, u.Email
                                                            , g.groupid, g.groupname
                                                            , p.isview, p.isadd, p.isdelete, p.isupdate, p.isactive, p.isemail, uc.CompanyCode, us.SiteId
                                                        FROM menu m
                                                            , permission p
                                                            , {0}group{1} g
                                                            , usergroup ug
                                                            , {0}user{1} u
															, UserCompany uc
                                                            , UserSite us
                                                        WHERE(m.menuname = '{2}')
                                                            AND m.menuid = p.menuid
                                                            AND p.groupid = g.groupid
                                                            --AND g.groupid = ug.groupid
                                                            AND p.groupid = ug.groupid
                                                            AND ug.userid = u.userid
                                                            AND uc.userid = u.userid
                                                            AND us.userid = u.userid
															AND (uc.CompanyCode = '{3}' OR '{3}' = '')
                                                            AND (us.SiteId = {4} OR {4} = 0)
                                                            AND u.isactive = 1
                                                        order BY u.loginname
                                                        ", "[", "]", p_menu_Name, p_company, p_site);

                Model_View_UserList_access_Menu item = null;

                using (SqlDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        item = new Model_View_UserList_access_Menu
                        {
                            UserId = OurUtility.ValueOf(reader, "userid"),
                            LoginName = OurUtility.ValueOf(reader, "loginname"),
                            FullName = OurUtility.ValueOf(reader, "FullName"),
                            Is_ActiveDirectory = OurUtility.ValueOf(reader, "is_activedirectory").ToLower().Equals("true"),
                            GroupId = OurUtility.ValueOf(reader, "groupid"),
                            GroupName = OurUtility.ValueOf(reader, "groupname"),
                            Is_View = OurUtility.ValueOf(reader, "isview").ToLower().Equals("true"),
                            Is_Add = OurUtility.ValueOf(reader, "isadd").ToLower().Equals("true"),
                            Is_Delete = OurUtility.ValueOf(reader, "isdelete").ToLower().Equals("true"),
                            Is_Update = OurUtility.ValueOf(reader, "isupdate").ToLower().Equals("true"),
                            Is_Active = OurUtility.ValueOf(reader, "isactive").ToLower().Equals("true"),
                            Is_Email = OurUtility.ValueOf(reader, "isemail").ToLower().Equals("true"),
                            Email = OurUtility.ValueOf(reader, "email")
                        };
                        if(p_company == string.Empty)
                        {
                            p_company = OurUtility.ValueOf(reader, "CompanyCode");
                        }
                      

                        // -- Calculation for Is_View
                        item.Is_View = item.Is_View || item.Is_Add || item.Is_Delete || item.Is_Update || item.Is_Active;

                        result.Add(item);
                    }
                }
            }
            catch {}

            return result;
        }

        internal static List<Model_View_UserList_access_Menu> UserList_access_Menu(string p_menu_Name, string p_menu_Access
                                                                                    , string p_id, string p_uploaded_file_type
                                                                                    , ref string p_uploaded_date, ref string p_company
                                                                                    , ref string p_fileName, ref string p_link)
        {
            List<Model_View_UserList_access_Menu> result = new List<Model_View_UserList_access_Menu>();
            List<Model_View_UserList_access_Menu> result2 = new List<Model_View_UserList_access_Menu>();

            // Tidak usah di "Reset"
            /*
            p_uploaded_date = string.Empty;
            p_company = string.Empty;
            p_fileName = string.Empty;
            p_link = string.Empty;*/

            try
            {
                string tableName_file_type = string.Empty;

                switch (p_uploaded_file_type)
                {
                    case OurUtility.UPLOAD_Sampling_ROM:
                        tableName_file_type = "TEMPORARY_Sampling_ROM_Header";
                        break;
                    case OurUtility.UPLOAD_Geology_Pit_Monitoring:
                        tableName_file_type = "TEMPORARY_Geology_Pit_Monitoring_Header";
                        break;
                    case OurUtility.UPLOAD_Geology_Explorasi:
                        tableName_file_type = "TEMPORARY_Geology_Explorasi_Header";
                        break;
                    case OurUtility.UPLOAD_BARGE_LOADING:
                        tableName_file_type = "TEMPORARY_BARGE_LOADING_Header";
                        break;
                    case OurUtility.UPLOAD_CRUSHING_PLANT:
                        tableName_file_type = "TEMPORARY_CRUSHING_PLANT_Header";
                        break;
                    case OurUtility.UPLOAD_HAC:
                        tableName_file_type = "TEMPORARY_HAC_Header";
                        break;
                }

                using (MERCY_Ctx db = new MERCY_Ctx())
                {
                    using (var connection = new SqlConnection(db.Database.Connection.ConnectionString))
                    {
                        connection.Open();

                        SqlCommand command = connection.CreateCommand();

                        command.CommandText = string.Format(@"
                                                                select f.RecordId
                                                                    , f.CreatedBy, f.FileName, f.Link
                                                                    , Convert(varchar(16), f.CreatedOn, 120) CreatedOn_Str
                                                                    , x.Company
                                                                from TEMPORARY_File f
                                                                    , {2} x
                                                                where f.RecordId = {0}
                                                                    --and f.FileType = '{1}'
                                                                    and x.File_Physical = {0}
                                                                    and (x.Company = '{3}' OR '{3}' = '')
                                                                ", p_id, p_uploaded_file_type, tableName_file_type, p_company);

                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                p_uploaded_date = OurUtility.ValueOf(reader, "CreatedOn_Str");
                                p_company = OurUtility.ValueOf(reader, "Company");
                                p_fileName = OurUtility.ValueOf(reader, "FileName");
                                p_link = OurUtility.ValueOf(reader, "Link");
                            }
                        }

                        result = UserList_access_Menu(connection, p_menu_Name, p_company);

                        connection.Close();
                    }
                }

                switch (p_menu_Access.ToUpper())
                {
                    case "VIEW":
                        result = result.Where(d => d.Is_View).ToList();
                        break;
                    case "ADD":
                        result = result.Where(d => d.Is_Add).ToList();
                        break;
                    case "DELETE":
                        result = result.Where(d => d.Is_Delete).ToList();
                        break;
                    case "UPDATE":
                        result = result.Where(d => d.Is_Update).ToList();
                        break;
                    case "EDIT": // Edit == Update
                        result = result.Where(d => d.Is_Update).ToList();
                        break;
                    case "ISACTIVE":
                        result = result.Where(d => d.Is_Active).ToList();
                        break;
                    default:
                        result = result.Where(d => d.Is_View && d.Is_Add && d.Is_Delete && d.Is_Update).ToList();
                        break;
                }

                string ids = string.Empty;
                foreach (Model_View_UserList_access_Menu item in result)
                {
                    if ( ! ids.Contains(item.UserId + ","))
                    {
                        result2.Add(item);

                        ids += item.UserId + ",";
                    }
                }
            }
            catch {}

            return result2;
        }

        internal static List<Model_View_UserList_access_Menu> UserList_access_Menu(string p_menu_Name, string p_menu_Access, string p_company, int p_site = 0)
        {
            List<Model_View_UserList_access_Menu> result = new List<Model_View_UserList_access_Menu>();
            List<Model_View_UserList_access_Menu> result2 = new List<Model_View_UserList_access_Menu>();

            try
            {
                using (MERCY_Ctx db = new MERCY_Ctx())
                {
                    using (var connection = new SqlConnection(db.Database.Connection.ConnectionString))
                    {
                        connection.Open();

                        result = UserList_access_Menu(connection, p_menu_Name, p_company, p_site);

                        connection.Close();
                    }
                }

                switch (p_menu_Access.ToUpper())
                {
                    case "VIEW":
                        result = result.Where(d => d.Is_View).ToList();
                        break;
                    case "ADD":
                        result = result.Where(d => d.Is_Add).ToList();
                        break;
                    case "DELETE":
                        result = result.Where(d => d.Is_Delete).ToList();
                        break;
                    case "UPDATE":
                        result = result.Where(d => d.Is_Update).ToList();
                        break;
                    case "EDIT": // Edit == Update
                        result = result.Where(d => d.Is_Update).ToList();
                        break;
                    case "ISACTIVE":
                        result = result.Where(d => d.Is_Active).ToList();
                        break;
                    case "EMAIL":
                        result = result.Where(d => d.Is_Email && d.Is_Active).ToList();
                        break;
                    default:
                        result = result.Where(d => d.Is_View && d.Is_Add && d.Is_Delete && d.Is_Update).ToList();
                        break;
                }

                string ids = string.Empty;
                foreach (Model_View_UserList_access_Menu item in result)
                {
                    if (!ids.Contains(item.UserId + ","))
                    {
                        result2.Add(item);

                        ids += item.UserId + ",";
                    }
                }
            }
            catch {}

            return result2;
        }

        internal static string EmailAddress(List<Model_View_UserList_access_Menu> userList_access_Menu, bool is_Original_Email = false)
        {
            string result = string.Empty;
            string separator = string.Empty;

            if (is_Original_Email)
            {
                foreach (Model_View_UserList_access_Menu d in userList_access_Menu)
                {
                    result += separator +  d.Email ;
                    separator = ",";
                }
            }
            else
            {
                foreach (Model_View_UserList_access_Menu d in userList_access_Menu)
                {
                    result += separator + d.LoginName + "@banpuindo.co.id";
                    separator = ",";
                }
            }

            return result;
        }

        internal static string EmailAddress_and_Name(List<Model_View_UserList_access_Menu> userList_access_Menu)
        {
            string result = string.Empty;
            string separator = string.Empty;

            foreach (Model_View_UserList_access_Menu d in userList_access_Menu)
            {
                result += separator + d.FullName + " [" + d.LoginName + "@banpuindo.co.id]";
                separator = ",";
            }

            return result;
        }

        internal static List<Model_View_UserList_access_Menu> UserListApproveAndEditTunnel(string p_menu_Name, string permission, string p_company)
        {
            List<Model_View_UserList_access_Menu> result = new List<Model_View_UserList_access_Menu>();

            try
            {
                using (MERCY_Ctx db = new MERCY_Ctx())
                {
                    using (var connection = new SqlConnection(db.Database.Connection.ConnectionString))
                    {
                        connection.Open();

                        result = UserListApproveAndEditTunnel(connection, p_menu_Name, permission,  p_company);

                        connection.Close();
                    }
                }
            }
            catch { }

            return result;
        }

        internal static List<Model_View_UserList_access_Menu> UserListApproveAndEditTunnel(SqlConnection p_connection, string p_menu_Name, string permission, string p_company)
        {
            List<Model_View_UserList_access_Menu> result = new List<Model_View_UserList_access_Menu>();

            try
            {
                SqlCommand command = p_connection.CreateCommand();

                command.CommandText = string.Format(@"
                                                        select DISTINCT u.userid, u.loginname, u.FullName, u.is_activedirectory
                                                            , g.groupid, g.groupname
                                                            , p.isview, p.isadd, p.isdelete, p.isupdate, p.isactive
                                                        FROM menu m
                                                            , permission p
                                                            , {0}group{1} g
                                                            , usergroup ug
                                                            , {0}user{1} u
															, UserCompany uc
                                                        WHERE(m.menuname = '{2}')
                                                            AND m.menuid = p.menuid
                                                            AND p.groupid = g.groupid
                                                            --AND g.groupid = ug.groupid
                                                            AND p.groupid = ug.groupid
                                                            AND ug.userid = u.userid
                                                            AND uc.userid = u.userid
															AND p.{3} = 1 
                                                            AND (uc.CompanyCode = '{4}' OR '{4}' = '' )
                                                            AND u.isactive = 1
                                                        order BY u.loginname
                                                        ", "[", "]", p_menu_Name, permission, p_company);

                Model_View_UserList_access_Menu item = null;

                using (SqlDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        item = new Model_View_UserList_access_Menu
                        {
                            UserId = OurUtility.ValueOf(reader, "userid"),
                            LoginName = OurUtility.ValueOf(reader, "loginname"),
                            FullName = OurUtility.ValueOf(reader, "FullName"),
                            Is_ActiveDirectory = OurUtility.ValueOf(reader, "is_activedirectory").ToLower().Equals("true"),
                            GroupId = OurUtility.ValueOf(reader, "groupid"),
                            GroupName = OurUtility.ValueOf(reader, "groupname"),
                            Is_View = OurUtility.ValueOf(reader, "isview").ToLower().Equals("true"),
                            Is_Add = OurUtility.ValueOf(reader, "isadd").ToLower().Equals("true"),
                            Is_Delete = OurUtility.ValueOf(reader, "isdelete").ToLower().Equals("true"),
                            Is_Update = OurUtility.ValueOf(reader, "isupdate").ToLower().Equals("true"),
                            Is_Active = OurUtility.ValueOf(reader, "isactive").ToLower().Equals("true")
                        };

                        // -- Calculation for Is_View
                        item.Is_View = item.Is_View || item.Is_Add || item.Is_Delete || item.Is_Update || item.Is_Active;

                        result.Add(item);
                    }
                }
            }
            catch { }

            return result;
        }

        internal static List<Model_View_UserList_access_Menu> UserListValidateAnalysisResult(string p_menu_Name, string permission, string p_company)
        {
            List<Model_View_UserList_access_Menu> result = new List<Model_View_UserList_access_Menu>();

            try
            {
                using (MERCY_Ctx db = new MERCY_Ctx())
                {
                    using (var connection = new SqlConnection(db.Database.Connection.ConnectionString))
                    {
                        connection.Open();

                        result = UserListValidateAnalysisResult(connection, p_menu_Name, permission, p_company);

                        connection.Close();
                    }
                }
            }
            catch { }

            return result;
        }

        internal static List<Model_View_UserList_access_Menu> UserListValidateAnalysisResult(SqlConnection p_connection, string p_menu_Name, string permission, string p_company)
        {
            List<Model_View_UserList_access_Menu> result = new List<Model_View_UserList_access_Menu>();

            try
            {
                SqlCommand command = p_connection.CreateCommand();

                command.CommandText = string.Format(@"
                                                        select DISTINCT u.userid, u.loginname, u.FullName, u.is_activedirectory
                                                            , g.groupid, g.groupname
                                                            , p.isview, p.isadd, p.isdelete, p.isupdate, p.isactive, p.isacknowledge, p.isapprove, p.isemail
                                                        FROM menu m
                                                            , permission p
                                                            , {0}group{1} g
                                                            , usergroup ug
                                                            , {0}user{1} u
															, UserCompany uc
                                                        WHERE(m.menuname = '{2}')
                                                            AND m.menuid = p.menuid
                                                            AND p.groupid = g.groupid
                                                            --AND g.groupid = ug.groupid
                                                            AND p.groupid = ug.groupid
                                                            AND ug.userid = u.userid
                                                            AND uc.userid = u.userid
															AND p.{3} = 1 
                                                            AND (uc.CompanyCode = '{4}' OR '{4}' = '' )
                                                            AND u.isactive = 1
                                                        order BY u.loginname
                                                        ", "[", "]", p_menu_Name, permission, p_company);

                Model_View_UserList_access_Menu item = null;

                using (SqlDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        item = new Model_View_UserList_access_Menu
                        {
                            UserId = OurUtility.ValueOf(reader, "userid"),
                            LoginName = OurUtility.ValueOf(reader, "loginname"),
                            FullName = OurUtility.ValueOf(reader, "FullName"),
                            Is_ActiveDirectory = OurUtility.ValueOf(reader, "is_activedirectory").ToLower().Equals("true"),
                            GroupId = OurUtility.ValueOf(reader, "groupid"),
                            GroupName = OurUtility.ValueOf(reader, "groupname"),
                            Is_View = OurUtility.ValueOf(reader, "isview").ToLower().Equals("true"),
                            Is_Add = OurUtility.ValueOf(reader, "isadd").ToLower().Equals("true"),
                            Is_Delete = OurUtility.ValueOf(reader, "isdelete").ToLower().Equals("true"),
                            Is_Update = OurUtility.ValueOf(reader, "isupdate").ToLower().Equals("true"),
                            Is_Active = OurUtility.ValueOf(reader, "isactive").ToLower().Equals("true"),
                            Is_Acknowledge = OurUtility.ValueOf(reader, "isacknowledge").ToLower().Equals("true"),
                            Is_Approve = OurUtility.ValueOf(reader, "isapprove").ToLower().Equals("true"),
                            Is_Email = OurUtility.ValueOf(reader, "isemail").ToLower().Equals("true")
                        };

                        // -- Calculation for Is_View
                        item.Is_View = item.Is_View || item.Is_Add || item.Is_Delete || item.Is_Update || item.Is_Active;

                        result.Add(item);
                    }
                }
            }
            catch { }

            return result;
        }   
        internal static List<Model_View_UserList_access_Menu> UserListApprovalAnalysisResult(string p_menu_Name, string permission, string p_company)
        {
            List<Model_View_UserList_access_Menu> result = new List<Model_View_UserList_access_Menu>();

            try
            {
                using (MERCY_Ctx db = new MERCY_Ctx())
                {
                    using (var connection = new SqlConnection(db.Database.Connection.ConnectionString))
                    {
                        connection.Open();

                        result = UserListApprovalAnalysisResult(connection, p_menu_Name, permission, p_company);

                        connection.Close();
                    }
                }
            }
            catch { }

            return result;
        }

        internal static List<Model_View_UserList_access_Menu> UserListApprovalAnalysisResult(SqlConnection p_connection, string p_menu_Name, string permission, string p_company)
        {
            List<Model_View_UserList_access_Menu> result = new List<Model_View_UserList_access_Menu>();

            try
            {
                SqlCommand command = p_connection.CreateCommand();

                command.CommandText = string.Format(@"
                                                        select DISTINCT u.userid, u.loginname, u.FullName, u.is_activedirectory
                                                            , g.groupid, g.groupname
                                                            , p.isview, p.isadd, p.isdelete, p.isupdate, p.isactive, p.isacknowledge, p.isapprove, p.isemail
                                                        FROM menu m
                                                            , permission p
                                                            , {0}group{1} g
                                                            , usergroup ug
                                                            , {0}user{1} u
															, UserCompany uc
                                                        WHERE(m.menuname = '{2}')
                                                            AND m.menuid = p.menuid
                                                            AND p.groupid = g.groupid
                                                            --AND g.groupid = ug.groupid
                                                            AND p.groupid = ug.groupid
                                                            AND ug.userid = u.userid
                                                            AND uc.userid = u.userid
															AND p.{3} = 1
                                                            AND (uc.CompanyCode = '{4}' OR '{4}' = '' )
                                                            AND u.isactive = 1
                                                        order BY u.loginname
                                                        ", "[", "]", p_menu_Name, permission, p_company);

                Model_View_UserList_access_Menu item = null;

                using (SqlDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        item = new Model_View_UserList_access_Menu
                        {
                            UserId = OurUtility.ValueOf(reader, "userid"),
                            LoginName = OurUtility.ValueOf(reader, "loginname"),
                            FullName = OurUtility.ValueOf(reader, "FullName"),
                            Is_ActiveDirectory = OurUtility.ValueOf(reader, "is_activedirectory").ToLower().Equals("true"),
                            GroupId = OurUtility.ValueOf(reader, "groupid"),
                            GroupName = OurUtility.ValueOf(reader, "groupname"),
                            Is_View = OurUtility.ValueOf(reader, "isview").ToLower().Equals("true"),
                            Is_Add = OurUtility.ValueOf(reader, "isadd").ToLower().Equals("true"),
                            Is_Delete = OurUtility.ValueOf(reader, "isdelete").ToLower().Equals("true"),
                            Is_Update = OurUtility.ValueOf(reader, "isupdate").ToLower().Equals("true"),
                            Is_Active = OurUtility.ValueOf(reader, "isactive").ToLower().Equals("true"),
                            Is_Acknowledge = OurUtility.ValueOf(reader, "isacknowledge").ToLower().Equals("true"),
                            Is_Approve = OurUtility.ValueOf(reader, "isapprove").ToLower().Equals("true"),
                            Is_Email = OurUtility.ValueOf(reader, "isemail").ToLower().Equals("true")
                        };

                        // -- Calculation for Is_View
                        item.Is_View = item.Is_View || item.Is_Add || item.Is_Delete || item.Is_Update || item.Is_Active;

                        result.Add(item);
                    }
                }
            }
            catch { }

            return result;
        }
    }

}