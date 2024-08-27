using System;
using System.Collections.Generic;
using System.Web.Mvc;

using MERCY.Data.EntityFramework;
using MERCY.Web.BackEnd.Helpers;
using MERCY.Web.BackEnd.Models;
using MERCY.Web.BackEnd.Security;
using Permission = MERCY.Web.BackEnd.Security.Permission;
using System.Data.SqlClient;

namespace MERCY.Web.BackEnd.Areas.Api.Controllers
{
    public class Search_IDController : Controller
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

            string idx = OurUtility.ValueOf(Request, "idx");

            // -- Actual code
            try
            {
                List<Model_View_ID> items = new List<Model_View_ID>();
                if (string.IsNullOrEmpty(idx))
                {
                    var resultX = new { Success = true, Permission = permission_Item, User = UserX.Information(user, is_Show_User_Menu, is_Show_User_Relation), Message = string.Empty, MessageDetail = string.Empty, Version = Configuration.VERSION, Items = items, Total = items.Count };
                    return Json(resultX, JsonRequestBehavior.AllowGet);
                }

                string where_Sampling_ROM = string.Empty;
                string where_Geology_Pit_Monitoring = string.Empty;
                string where_Geology_Explorasi = string.Empty;
                string where_BARGE_LOADING = string.Empty;
                string where_CRUSHING_PLANT = string.Empty;
                string separator = string.Empty;
                string[] idx_s = idx.Split(' ');
                string x = string.Empty;
                foreach (string id in idx_s)
                {
                    x = id.Trim();
                    if (string.IsNullOrEmpty(x)) continue;

                    where_Sampling_ROM += separator + "Lab_ID like '%" + x + "%'";
                    where_Geology_Pit_Monitoring += separator + "Lab_ID like '%" + x + "%'";
                    where_Geology_Explorasi += separator + "Lab_ID like '%" + x + "%'";
                    where_BARGE_LOADING += separator + "ID_Number like '%" + x + "%'";
                    where_CRUSHING_PLANT += separator + "Sample_ID like '%" + x + "%'";


                    separator = " or ";
                }

                using (MERCY_Ctx db = new MERCY_Ctx())
                {
                    using (var connection = new SqlConnection(db.Database.Connection.ConnectionString))
                    {
                        connection.Open();

                        SqlCommand command = connection.CreateCommand();

                        command.CommandText = string.Format(@"
                                                            select top 100 ID_, CreatedOn, LastModifiedOn, Company, Type_
                                                            from
                                                            (
                                                                select 'Sampling_ROM' as Type_
	                                                                    , Lab_ID as ID_
	                                                                    , Company
	                                                                    ,Convert(varchar(19), CreatedOn, 120) as CreatedOn
                                                                        ,case IsNull(LastModifiedOn, 1) 
                                                                            when 1 then ''
                                                                            else Convert(varchar(19), LastModifiedOn, 120) end as LastModifiedOn
                                                                from UPLOAD_Sampling_ROM
                                                                where {0}

                                                                union

                                                                select 'Geology_Pit_Monitoring' as Type_
	                                                                    , Lab_ID as ID_
	                                                                    , Company
	                                                                    ,Convert(varchar(19), CreatedOn, 120) as CreatedOn
                                                                        ,case IsNull(LastModifiedOn, 1) 
                                                                            when 1 then ''
                                                                            else Convert(varchar(19), LastModifiedOn, 120) end as LastModifiedOn
                                                                from UPLOAD_Geology_Pit_Monitoring
                                                                where {1}

                                                                union

                                                                select 'Geology_Explorasi' as Type_
	                                                                    , Lab_ID as ID_
	                                                                    , Company
	                                                                    ,Convert(varchar(19), CreatedOn, 120) as CreatedOn
                                                                        ,case IsNull(LastModifiedOn, 1) 
                                                                            when 1 then ''
                                                                            else Convert(varchar(19), LastModifiedOn, 120) end as LastModifiedOn
                                                                from UPLOAD_Geology_Explorasi
                                                                where {2}

                                                                union

                                                                select 'BARGE_LOADING' as Type_
	                                                                    , ID_Number as ID_
	                                                                    , Company
	                                                                    ,Convert(varchar(19), CreatedOn, 120) as CreatedOn
                                                                        ,case IsNull(LastModifiedOn, 1) 
                                                                            when 1 then ''
                                                                            else Convert(varchar(19), LastModifiedOn, 120) end as LastModifiedOn
                                                                from UPLOAD_BARGE_LOADING
                                                                where {3}

                                                                union

                                                                select 'CRUSHING_PLANT' as Type_
	                                                                    , Sample_ID as ID_
	                                                                    , Company
	                                                                    ,Convert(varchar(19), CreatedOn, 120) as CreatedOn
                                                                        ,case IsNull(LastModifiedOn, 1) 
                                                                            when 1 then ''
                                                                            else Convert(varchar(19), LastModifiedOn, 120) end as LastModifiedOn
                                                                from UPLOAD_CRUSHING_PLANT
                                                                where {4}
                                                            ) x
                                                            order by ID_
                                                            ", where_Sampling_ROM
                                                            , where_Geology_Pit_Monitoring
                                                            , where_Geology_Explorasi
                                                            , where_BARGE_LOADING
                                                            , where_CRUSHING_PLANT);

                        Model_View_ID item = null;
                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                item = new Model_View_ID
                                {
                                    RecordId = "-1",
                                    Id = OurUtility.ValueOf(reader, "ID_"),
                                    CreatedOn = OurUtility.ValueOf(reader, "CreatedOn"),
                                    LastModifiedOn = OurUtility.ValueOf(reader, "LastModifiedOn"),
                                    Type = OurUtility.ValueOf(reader, "Type_"),
                                    Company = OurUtility.ValueOf(reader, "Company")
                                };

                                item.CreatedOn_Str = OurUtility.DateFormat(item.CreatedOn, "dd-MMM-yyyy HH:mm:ss");
                                item.LastModifiedOn_Str = string.Empty;
                                if ( ! string.IsNullOrEmpty(item.LastModifiedOn))
                                {
                                    item.LastModifiedOn_Str = OurUtility.DateFormat(item.LastModifiedOn, "dd-MMM-yyyy HH:mm:ss");
                                }

                                items.Add(item);
                            }
                        }

                        connection.Close();
                    }
                }

                var result = new { Success = true, Permission = permission_Item, User = UserX.Information(user, is_Show_User_Menu, is_Show_User_Relation), Message = string.Empty, MessageDetail = string.Empty, Version = Configuration.VERSION, Items = items, Total = items.Count};
                return Json(result, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                var result = new { Success = false, Permission = permission_Item, ex.Message, MessageDetail = string.Empty, Version = Configuration.VERSION, Items = string.Empty, Total = 0 };
                return Json(result, JsonRequestBehavior.AllowGet);
            }
        }
    }
}
