using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

using MERCY.Data.EntityFramework;
using MERCY.Web.BackEnd.Models;
using MERCY.Web.BackEnd.Helpers;
using MERCY.Web.BackEnd.Security;
using Permission = MERCY.Web.BackEnd.Security.Permission;

using System.Data;
using System.Data.SqlClient;

namespace MERCY.Web.BackEnd.Areas.Api.Controllers
{
    public class LoadingRequestController : Controller
    {
        public JsonResult Index()
        {
            // Check Permission based on Current Url
            UserX user = new UserX(Request);
            Permission_Item permission_Item = null;

            Permission.Check_API(Request, user, ref permission_Item);
            if (!permission_Item.Is_View)
            {
                var msg = new { Success = false, Permission = permission_Item, Message = Permission.ERROR_PERMISSION_READ, MessageDetail = string.Empty, Version = Configuration.VERSION };
                return Json(msg, JsonRequestBehavior.AllowGet);
            }

            bool is_Show_User_Menu = OurUtility.ValueOf(Request, "u_menu").Equals("1");
            bool is_Show_User_Relation = OurUtility.ValueOf(Request, "u_relation").Equals("1");
            string company_code = OurUtility.ValueOf(Request, "company").Equals("all") ? string.Empty : OurUtility.ValueOf(Request, "company");
            string site = OurUtility.ValueOf(Request, "site").Equals("all") ? string.Empty : OurUtility.ValueOf(Request, "site");
            var createdOn = OurUtility.ValueOf(Request, "dateFrom").Equals("all") ? string.Empty : OurUtility.ValueOf(Request, "dateFrom");
            string createdOnString = createdOn == "" ? "" : OurUtility.DateFormat(createdOn, "yyyy-MM-dd");
            string ShipmentType = OurUtility.ValueOf(Request, "shipment_Type").Equals("all") ? string.Empty : OurUtility.ValueOf(Request, "shipment_Type");
            string vessel = OurUtility.ValueOf(Request, "vessel").Equals("all") ? string.Empty : OurUtility.ValueOf(Request, "vessel");
            string buyer = OurUtility.ValueOf(Request, "buyer").Equals("all") ? string.Empty : OurUtility.ValueOf(Request, "buyer");
            string id = OurUtility.ValueOf(Request, ".id") == string.Empty ? "-1" : OurUtility.ValueOf(Request, ".id");

            string txt = OurUtility.ValueOf(Request, "txt").ToUpper();
            bool isAllText = string.IsNullOrEmpty(txt);

            // -- Actual code
            try
            {
                List<Model_Loading_Plan> items = new List<Model_Loading_Plan>();
                Model_Loading_Plan item = null;
                List<string> vessels = new List<string>();
                List<string> buyers = new List<string>();

                using (MERCY_Ctx db = new MERCY_Ctx())
                {
                    using (var connection = new SqlConnection(db.Database.Connection.ConnectionString))
                    {
                        connection.Open();

                        SqlCommand command_Request = connection.CreateCommand();
                        SqlCommand command = connection.CreateCommand();
                        SqlCommand command_Detail = connection.CreateCommand();

                        string sql = string.Empty;

                        sql = string.Format(@"
                            select r.RecordId as Request_Id
                                , r.CreatedOn as SubmittedOn
                                , u.FullName as SubmittedBy
                            from Loading_Request r, 
                                [User] u
                            where r.CreatedBy = u.UserId
                                and (r.RecordId = {0} OR {0} = -1)
                                and ((CONVERT (date, r.CreatedOn) = '{1}') or ('{1}' = ''))
                            order by r.RecordId desc",
                            id,
                            createdOnString);

                        command_Request.CommandText = sql;

                        using (SqlDataReader reader_Request = command_Request.ExecuteReader())
                        {
                            int request_Count = 0;
                            int previous_Request = request_Count;

                            while (reader_Request.Read())
                            {
                                request_Count++;

                                sql = string.Format(@"
                                                select p.RecordId
                                                    , p.Loading_Plan_Id
                                                    , p.SiteId
                                                    , s.SiteName as Site_Str
                                                    , p.Company
                                                    , p.Shipment_Type
                                                    , p.Buyer
                                                    , p.Vessel
                                                    , p.Destination
                                                    , p.Trip_No
                                                    , p.Remark
                                                    , '' as ETA_Vessel_MBR
                                                    , p.CreatedOn
                                                from Loading_Request_Loading_Plan p
                                                    , Site s
                                                    , Company c
                                                where p.Request_Id = {0}
                                                    and p.SiteId = s.SiteId
                                                    and p.Company = c.CompanyCode
                                                    and ((c.CompanyCode = '{1}') or ('{1}' = ''))
                                                    and ((s.SiteName = '{2}') or ('{2}' = ''))
                                                    and ((p.Shipment_Type = '{3}') or ('{3}' = ''))
                                                    and ((p.Vessel = '{4}') or ('{4}' = ''))
                                                    and ((p.Buyer = '{5}') or ('{5}' = ''))
                                                order by p.Loading_Plan_Id
                                                    ", OurUtility.ValueOf(reader_Request, "Request_Id"), company_code, site, ShipmentType, vessel, buyer);

                                command.CommandText = sql;

                                using (SqlDataReader reader = command.ExecuteReader())
                                {
                                    while (reader.Read())
                                    {
                                        item = new Model_Loading_Plan
                                        {
                                            Request_Id = OurUtility.ValueOf(reader_Request, "Request_Id"),
                                            SubmittedOn = OurUtility.ToDateTime(OurUtility.ValueOf(reader_Request, "SubmittedOn"))
                                        };
                                        item.SubmittedOn_Str2 = OurUtility.DateFormat(item.SubmittedOn, "dd-MMM-yyyy");
                                        item.SubmittedOn_Str = string.Empty;
                                        item.Submitted_By = string.Empty;

                                        if (previous_Request != request_Count)
                                        {
                                            item.SubmittedOn_Str = OurUtility.DateFormat(item.SubmittedOn, @"dd-MMM-yyyy HH:mm:ss");

                                            item.Submitted_By = OurUtility.ValueOf(reader_Request, "SubmittedBy");
                                        }

                                        item.RecordId = OurUtility.ToInt64(OurUtility.ValueOf(reader, "RecordId"));
                                        item.Loading_Plan_Id = OurUtility.ValueOf(reader, "Loading_Plan_Id");
                                        item.Trip_No = OurUtility.ValueOf(reader, "Trip_No");
                                        item.Site_Str = OurUtility.ValueOf(reader, "Site_Str");
                                        item.Company = OurUtility.ValueOf(reader, "Company");
                                        item.Shipment_Type = OurUtility.ValueOf(reader, "Shipment_Type");
                                        item.Destination = OurUtility.ValueOf(reader, "Destination");
                                        item.Remark = OurUtility.ValueOf(reader, "Remark");

                                        item.PlanBlendingLoading = string.Empty;

                                        item.CreatedOn = OurUtility.ToDateTime(OurUtility.ValueOf(reader, "CreatedOn"));
                                        item.CreatedOn_Str = OurUtility.DateFormat(item.CreatedOn, @"dd-MMM-yyyy HH:mm:ss");
                                        item.CreatedOn_Str2 = OurUtility.DateFormat(item.CreatedOn, "dd-MMM-yyyy");

                                        sql = string.Format(@"
                                                select Tug, Barge, Convert(varchar(20), EstimateStartLoading, 120) as EstimateStartLoading
                                                        , Quantity, Product
                                                from Loading_Request_Loading_Plan_Detail_Barge
                                                where Request_Id = {0} and Loading_Plan_Id = {1}
                                                order by RecordId
                                                    ", item.Request_Id, item.Loading_Plan_Id);

                                        command_Detail.CommandText = sql;

                                        using (SqlDataReader reader_Detail = command_Detail.ExecuteReader())
                                        {
                                            while (reader_Detail.Read())
                                            {
                                                item.Tug += string.Format(@"<div>- {0}</div>", OurUtility.ValueOf(reader_Detail, "Tug"));
                                                item.Barge += string.Format(@"<div>- {0}</div>", OurUtility.ValueOf(reader_Detail, "Barge"));
                                                item.Estimate_Start_Loading += string.Format(@"<div>{0}</div>", OurUtility.DateFormat(OurUtility.ValueOf(reader_Detail, "EstimateStartLoading"), "dd-MMM-yyyy"));
                                                item.Estimate_Quantity += string.Format(@"<div>{0}</div>", OurUtility.ValueOf(reader_Detail, "Quantity"));
                                                item.Product += string.Format(@"<div>- {0}</div>", OurUtility.ValueOf(reader_Detail, "Product"));
                                            }
                                        }

                                        item = LoadingPlanController.GetPortionBlending(item, Convert.ToInt64(item.Loading_Plan_Id));

                                        items.Add(item);

                                        previous_Request = request_Count;
                                    }
                                }
                            }
                        }

                        bool is_Show_Vessel_Buyer = OurUtility.ValueOf(Request, "show_Vessel_Buyer").Equals("1");
                        if (is_Show_Vessel_Buyer)
                        {
                            vessels = db.Loading_Request_Loading_Plan
                                .Select(lr => lr.Vessel ?? string.Empty)
                                .Distinct()
                                .OrderBy(lrVessel => lrVessel)
                                .ToList();

                            buyers = db.Loading_Request_Loading_Plan
                                .Select(lr => lr.Buyer ?? string.Empty)
                                .Distinct()
                                .OrderBy(lrBuyer => lrBuyer)
                                .ToList();
                        }

                        connection.Close();
                    }
                }

                var filteredItems = items.Where(x =>
                        isAllText
                        || (x.Tug?.ToUpper().Contains(txt) ?? false)
                        || (x.Barge?.ToUpper().Contains(txt) ?? false)
                        || (x.Product?.ToUpper().Contains(txt) ?? false))
                    .ToList();

                var result = new { Success = true, Permission = permission_Item, User = UserX.Information(user, is_Show_User_Menu, is_Show_User_Relation), Message = string.Empty, MessageDetail = string.Empty, Version = Configuration.VERSION, Items = filteredItems, Total = filteredItems.Count, Vessels = vessels, Buyers = buyers };
                return Json(result, JsonRequestBehavior.AllowGet);
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
            if (!permission_Item.Is_Add)
            {
                var msg = new { Success = false, Permission = permission_Item, Message = Permission.ERROR_PERMISSION_ADD, MessageDetail = string.Empty, Version = Configuration.VERSION };
                return Json(msg, JsonRequestBehavior.AllowGet);
            }

            // -- Actual code
            try
            {
                using (MERCY_Ctx db = new MERCY_Ctx())
                {
                    var data = new Site
                    {
                        SiteName = OurUtility.ValueOf(p_collection, "name"),
                        IsActive = OurUtility.ValueOf(p_collection, "isactive").Equals("1"),

                        CreatedOn = DateTime.Now,
                        CreatedBy = user.UserId
                    };

                    db.Sites.Add(data);
                    db.SaveChanges();

                    long id = data.SiteId;

                    var result = new { Success = true, Permission = permission_Item, Message = BaseConstants.MESSAGE_CREATE_SUCCESS, MessageDetail = string.Empty, Version = Configuration.VERSION, Data = id };
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
            if (!permission_Item.Is_Edit)
            {
                var msg = new { Success = false, Permission = permission_Item, Message = Permission.ERROR_PERMISSION_EDIT, MessageDetail = string.Empty, Version = Configuration.VERSION };
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

                    data.SiteName = OurUtility.ValueOf(p_collection, "name");
                    data.IsActive = OurUtility.ValueOf(p_collection, "isactive").Equals("1");

                    data.LastModifiedOn = DateTime.Now;
                    data.LastModifiedBy = user.UserId;

                    db.SaveChanges();

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

        public JsonResult Plan()
        {
            // Check Permission based on Current Url
            UserX user = new UserX(Request);
            Permission_Item permission_Item = null;

            Permission.Check_API(Request, user, ref permission_Item);
            if (!permission_Item.Is_View)
            {
                var msg = new { Success = false, Permission = permission_Item, Message = Permission.ERROR_PERMISSION_READ, MessageDetail = string.Empty, Version = Configuration.VERSION };
                return Json(msg, JsonRequestBehavior.AllowGet);
            }

            string CreatedOnFilter = DateTime.Now.AddDays(-30).ToString("yyyy-MM-dd");

            // -- Actual code
            try
            {
                List<Model_Loading_Plan> items = new List<Model_Loading_Plan>();
                Model_Loading_Plan item = null;

                using (MERCY_Ctx db = new MERCY_Ctx())
                {
                    using (var connection = new SqlConnection(db.Database.Connection.ConnectionString))
                    {
                        connection.Open();

                        SqlCommand command = connection.CreateCommand();
                        SqlCommand command_Detail = connection.CreateCommand();

                        string sql = string.Empty;

                        sql = string.Format(@"
                                                select p.RecordId
                                                    , p.Shipment_Type
                                                    , p.Buyer
                                                    , p.Vessel
                                                    , p.CreatedOn
                                                    , u.FullName
                                                from Loading_Plan p
                                                    JOIN [User] u ON p.CreatedBy = u.UserId
                                                    LEFT JOIN Loading_Request_Loading_Plan rp ON rp.Loading_Plan_Id = p.RecordId 
                                                where p.CreatedBy = u.UserId
                                                    and (CONVERT (date, p.CreatedOn) >= CONVERT (date, '{0}'))
                                                    and rp.RecordId is null
                                                order by p.RecordId
                                                    ", CreatedOnFilter);

                        command.CommandText = sql;

                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                item = new Model_Loading_Plan
                                {
                                    RecordId = OurUtility.ToInt64(OurUtility.ValueOf(reader, "RecordId")),

                                    FullName = OurUtility.ValueOf(reader, "FullName"),
                                    Shipment_Type = OurUtility.ValueOf(reader, "Shipment_Type"),
                                    Buyer = OurUtility.ValueOf(reader, "Buyer"),
                                    Vessel = OurUtility.ValueOf(reader, "Vessel"),

                                    CreatedOn = OurUtility.ToDateTime(OurUtility.ValueOf(reader, "CreatedOn"))
                                };
                                item.CreatedOn_Str = OurUtility.DateFormat(item.CreatedOn, @"dd-MMM-yyyy HH:mm:ss");
                                item.CreatedOn_Str2 = OurUtility.DateFormat(item.CreatedOn, "dd-MMM-yyyy");

                                items.Add(item);
                            }
                        }

                        connection.Close();
                    }
                }

                var result = new { Success = true, Permission = permission_Item, Message = string.Empty, MessageDetail = string.Empty, Version = Configuration.VERSION, Items = items, Total = items.Count };
                return Json(result, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                var result = new { Success = false, Permission = permission_Item, ex.Message, MessageDetail = string.Empty, Version = Configuration.VERSION, Items = string.Empty, Total = 0 };
                return Json(result, JsonRequestBehavior.AllowGet);
            }
        }

        public JsonResult Preview()
        {
            // Check Permission based on Current Url
            UserX user = new UserX(Request);
            Permission_Item permission_Item = null;

            Permission.Check_API(Request, user, ref permission_Item);
            if (!permission_Item.Is_View)
            {
                var msg = new { Success = false, Permission = permission_Item, Message = Permission.ERROR_PERMISSION_READ, MessageDetail = string.Empty, Version = Configuration.VERSION };
                return Json(msg, JsonRequestBehavior.AllowGet);
            }

            bool is_Show_User_Menu = OurUtility.ValueOf(Request, "u_menu").Equals("1");
            bool is_Show_User_Relation = OurUtility.ValueOf(Request, "u_relation").Equals("1");

            string selected = OurUtility.ValueOf(Request, "selected");

            // -- Actual code
            try
            {
                List<Model_Loading_Plan> items = new List<Model_Loading_Plan>();
                Model_Loading_Plan item = null;
                List<string> vessels = new List<string>();
                List<string> buyers = new List<string>();

                using (MERCY_Ctx db = new MERCY_Ctx())
                {
                    using (var connection = new SqlConnection(db.Database.Connection.ConnectionString))
                    {
                        connection.Open();

                        SqlCommand command = connection.CreateCommand();
                        SqlCommand command_Detail = connection.CreateCommand();

                        string sql = string.Empty;

                        sql = string.Format(@"
                                                select p.RecordId
                                                    , p.SiteId
                                                    , s.SiteName as Site_Str
                                                    , p.Company
                                                    , p.Shipment_Type
                                                    , p.Buyer
                                                    , p.Vessel
                                                    , p.Destination
                                                    , p.Trip_No
                                                    , p.Remark
                                                    , '' as ETA_Vessel_MBR
                                                    , p.CreatedOn
                                                from Loading_Plan p
                                                    , Site s
                                                    , Company c
                                                where p.RecordId in ({0})
                                                    and p.SiteId = s.SiteId
                                                    and p.Company = c.CompanyCode
                                                order by p.CreatedOn
                                                    ", selected);

                        command.CommandText = sql;

                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                item = new Model_Loading_Plan
                                {
                                    RecordId = OurUtility.ToInt64(OurUtility.ValueOf(reader, "RecordId")),
                                    Trip_No = OurUtility.ValueOf(reader, "Trip_No"),
                                    Site_Str = OurUtility.ValueOf(reader, "Site_Str"),
                                    Company = OurUtility.ValueOf(reader, "Company"),
                                    Shipment_Type = OurUtility.ValueOf(reader, "Shipment_Type"),
                                    Destination = OurUtility.ValueOf(reader, "Destination"),
                                    Remark = OurUtility.ValueOf(reader, "Remark"),

                                    PlanBlendingLoading = string.Empty,

                                    CreatedOn = OurUtility.ToDateTime(OurUtility.ValueOf(reader, "CreatedOn"))
                                };
                                item.CreatedOn_Str = OurUtility.DateFormat(item.CreatedOn, @"dd-MMM-yyyy HH:mm:ss");
                                item.CreatedOn_Str2 = OurUtility.DateFormat(item.CreatedOn, "dd-MMM-yyyy");

                                sql = string.Format(@"
                                                select Tug, Barge, Convert(varchar(20), EstimateStartLoading, 120) as EstimateStartLoading
                                                        , Quantity, Product
                                                from Loading_Plan_Detail_Barge
                                                where Loading_Plan_Id = {0}
                                                order by RecordId
                                                    ", item.RecordId);

                                command_Detail.CommandText = sql;

                                using (SqlDataReader reader_Detail = command_Detail.ExecuteReader())
                                {
                                    while (reader_Detail.Read())
                                    {
                                        item.Tug += string.Format(@"<div>- {0}</div>", OurUtility.ValueOf(reader_Detail, "Tug"));
                                        item.Barge += string.Format(@"<div>- {0}</div>", OurUtility.ValueOf(reader_Detail, "Barge"));
                                        item.Estimate_Start_Loading += string.Format(@"<div>{0}</div>", OurUtility.DateFormat(OurUtility.ValueOf(reader_Detail, "EstimateStartLoading"), "dd-MMM-yyyy"));
                                        item.Estimate_Quantity += string.Format(@"<div>{0}</div>", OurUtility.ValueOf(reader_Detail, "Quantity"));
                                        item.Product += string.Format(@"<div>- {0}</div>", OurUtility.ValueOf(reader_Detail, "Product"));
                                    }
                                }

                                item = LoadingPlanController.GetPortionBlending(item, Convert.ToInt64(item.RecordId));

                                items.Add(item);
                            }
                        }

                        connection.Close();
                    }
                }

                var result = new { Success = true, Permission = permission_Item, User = UserX.Information(user, is_Show_User_Menu, is_Show_User_Relation), Message = string.Empty, MessageDetail = string.Empty, Version = Configuration.VERSION, Items = items, Total = items.Count, Vessels = vessels, Buyers = buyers };
                return Json(result, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                var result = new { Success = false, Permission = permission_Item, ex.Message, MessageDetail = string.Empty, Version = Configuration.VERSION, Items = string.Empty, Total = 0 };
                return Json(result, JsonRequestBehavior.AllowGet);
            }
        }

        public JsonResult Submit(FormCollection p_collection)
        {
            // Check Permission based on Current Url
            UserX user = new UserX(Request);
            Permission_Item permission_Item = null;

            Permission.Check_API(Request, user, ref permission_Item);
            if (!permission_Item.Is_Add)
            {
                var msg = new { Success = false, Permission = permission_Item, Message = Permission.ERROR_PERMISSION_ADD, MessageDetail = string.Empty, Version = Configuration.VERSION };
                return Json(msg, JsonRequestBehavior.AllowGet);
            }

            string selected = OurUtility.ValueOf(Request, "selected");

            // -- Actual code
            try
            {
                using (MERCY_Ctx db = new MERCY_Ctx())
                {
                    long id = OurUtility.ToInt64(Request["selected"]);
                    long[] splittedSelected = selected.Split(',').Select(str => long.Parse(str)).ToArray();
                    var existingDataQuery = (from p in db.Loading_Request_Loading_Plan
                                             where splittedSelected.Contains(p.Loading_Plan_Id)
                                             select p);
                    var existingData = existingDataQuery.FirstOrDefault();
                    if (existingData != null)
                    {
                        var msg = new { Success = false, Permission = permission_Item, Message = "Loading request for that Loading Plan already exist", MessageDetail = string.Empty, Version = Configuration.VERSION };
                        return Json(msg, JsonRequestBehavior.AllowGet);
                    }
                    else
                    {
                        var data = new Loading_Request
                        {
                            CreatedOn = DateTime.Now,
                            CreatedBy = user.UserId
                        };
                        data.CreatedOn_Date_Only = data.CreatedOn.ToString("yyyy-MM-dd");

                        db.Loading_Request.Add(data);
                        db.SaveChanges();

                        long request_Id = data.RecordId;

                        using (var connection = new SqlConnection(db.Database.Connection.ConnectionString))
                        {
                            connection.Open();

                            SqlCommand command = connection.CreateCommand();
                            SqlCommand command_Insert = connection.CreateCommand();

                            string sql = string.Empty;

                            sql = string.Format(@"
                                                select p.RecordId
                                                from Loading_Plan p
                                                where p.RecordId in ({0})
                                                order by p.RecordId
                                                    ", selected);

                            command.CommandText = sql;

                            using (SqlDataReader reader = command.ExecuteReader())
                            {
                                while (reader.Read())
                                {
                                    command_Insert.CommandText = string.Format(@"
                                    insert into Loading_Request_Loading_Plan(Request_Id
                                            ,Loading_Plan_Id
                                            ,SiteId
                                            ,Company
                                            ,Shipment_Type
                                            ,Buyer
                                            ,Vessel
                                            ,Destination
                                            ,Trip_No
                                            ,Remark
                                            ,CreatedBy
                                            ,CreatedOn
                                            ,CreatedOn_Date_Only
                                            ,LastModifiedBy
                                            ,LastModifiedOn)
                                    select {0}
                                            ,p.RecordId
                                            ,p.SiteId
                                            ,p.Company
                                            ,p.Shipment_Type
                                            ,p.Buyer
                                            ,p.Vessel
                                            ,p.Destination
                                            ,p.Trip_No
                                            ,p.Remark
                                            ,p.CreatedBy
                                            ,p.CreatedOn
                                            ,p.CreatedOn_Date_Only
                                            ,p.LastModifiedBy
                                            ,p.LastModifiedOn
                                    from Loading_Plan p
                                    where p.RecordId = {1}", request_Id, OurUtility.ValueOf(reader, "RecordId"));
                                    command_Insert.ExecuteNonQuery();

                                    command_Insert.CommandText = string.Format(@"
                                    insert into Loading_Request_Loading_Plan_Detail_Barge(
                                            Request_Id
                                            ,Loading_Plan_Id
                                            ,Tug
                                            ,Barge
                                            ,Trip_ID
                                            ,EstimateStartLoading
                                            ,Quantity
                                            ,Product
                                            ,CreatedBy
                                            ,CreatedOn
                                            ,CreatedOn_Date_Only
                                            ,LastModifiedBy
                                            ,LastModifiedOn
                                    )
                                    select {0}
                                            ,p.Loading_Plan_Id
                                            ,p.Tug
                                            ,p.Barge
                                            ,p.Trip_ID
                                            ,p.EstimateStartLoading
                                            ,p.Quantity
                                            ,p.Product
                                            ,p.CreatedBy
                                            ,p.CreatedOn
                                            ,p.CreatedOn_Date_Only
                                            ,p.LastModifiedBy
                                            ,p.LastModifiedOn
                                    from Loading_Plan_Detail_Barge p
                                    where p.Loading_Plan_Id = {1}", request_Id, OurUtility.ValueOf(reader, "RecordId"));
                                    command_Insert.ExecuteNonQuery();
                                }
                            }

                            connection.Close();
                        }

                        var result = new { Success = true, Permission = permission_Item, Message = BaseConstants.MESSAGE_CREATE_SUCCESS, MessageDetail = string.Empty, Version = Configuration.VERSION, Data = request_Id };
                        return Json(result, JsonRequestBehavior.AllowGet);
                    }
                }
            }
            catch (Exception ex)
            {
                var result = new { Success = false, Permission = permission_Item, ex.Message, MessageDetail = string.Empty, Version = Configuration.VERSION, Items = string.Empty, Total = 0 };
                return Json(result, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpGet]
        public JsonResult GetDetailBarge()
        {
            // Check Permission based on Current Url
            UserX user = new UserX(Request);
            Permission_Item permission_Item = null;

            Permission.Check_API(Request, user, ref permission_Item);
            if (!permission_Item.Is_View)
            {
                var msg = new { Success = false, Permission = permission_Item, Message = Permission.ERROR_PERMISSION_READ, MessageDetail = string.Empty, Version = Configuration.VERSION };
                return Json(msg, JsonRequestBehavior.AllowGet);
            }

            bool is_Show_User_Menu = OurUtility.ValueOf(Request, "u_menu").Equals("1");
            bool is_Show_User_Relation = OurUtility.ValueOf(Request, "u_relation").Equals("1");

            List<Model_View_Loading_Request_Loading_Plan> items = new List<Model_View_Loading_Request_Loading_Plan>();

            var config = new Configuration();
            var nthLoadingRequest = Convert.ToInt32(config.LOADING_REQUEST_NTH_DAY);

            try
            {
                using (MERCY_Ctx db = new MERCY_Ctx())
                {
                    using (var connection = new SqlConnection(db.Database.Connection.ConnectionString))
                    {
                        connection.Open();

                        SqlCommand command = connection.CreateCommand();

                        string sql = string.Empty;

                        sql = string.Format(@"
                            SELECT DISTINCT d.RecordId,
	                            d.Request_Id,
	                            d.Loading_Plan_Id,
                                1 as No_Ref_Report,
                                d.Tug,
                                d.Barge,
                                d.Trip_ID,
                                d.Quantity as Barge_Size,
                                d.Product as Coal_Quality, 
                                lrlp.Shipment_Type + ' Shipment' as LoadType,
                                'JBG' as Route
                            FROM Loading_Request_Loading_Plan_Detail_Barge d
                            JOIN Loading_Request_Loading_Plan lrlp on d.Request_Id = lrlp.Request_Id AND d.Loading_Plan_Id = lrlp.Loading_Plan_Id 
                            JOIN (
                                SELECT Tug, MAX(CreatedOn) as CreatedOn
                                FROM Loading_Request_Loading_Plan_Detail_Barge
                                WHERE CreatedOn_Date_Only BETWEEN '{0}' AND '{1}' -- BETWEEN H - N AND H
                                GROUP BY Tug
                            ) AS d2 ON d.Tug = d2.Tug AND d.CreatedOn = d2.CreatedOn
                            ORDER BY d.Tug;",
                            DateTime.Now.AddDays(nthLoadingRequest * -1).Date.ToString("yyyy-MM-dd"),
                            DateTime.Now.Date.ToString("yyyy-MM-dd"));

                        command.CommandText = sql;

                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                var item = new Model_View_Loading_Request_Loading_Plan
                                {
                                    RecordId = OurUtility.ToInt64(OurUtility.ValueOf(reader, "RecordId")),
                                    Request_Id = OurUtility.ToInt64(OurUtility.ValueOf(reader, "Request_Id")),
                                    Loading_Plan_Id = OurUtility.ToInt64(OurUtility.ValueOf(reader, "Loading_Plan_Id")),
                                    No_Ref_Report = OurUtility.ToInt32(OurUtility.ValueOf(reader, "No_Ref_Report")),
                                    Tug = OurUtility.ValueOf(reader, "Tug"),
                                    Barge = OurUtility.ValueOf(reader, "Barge"),
                                    Trip_ID = OurUtility.ValueOf(reader, "Trip_ID"),
                                    Barge_Size = OurUtility.ToInt32(OurUtility.ValueOf(reader, "Barge_Size")),
                                    Quantity = OurUtility.ToInt32(OurUtility.ValueOf(reader, "Barge_Size")),
                                    Coal_Quality = OurUtility.ValueOf(reader, "Coal_Quality"),
                                    Product = OurUtility.ValueOf(reader, "Coal_Quality"),
                                    Shipment_Type = OurUtility.ValueOf(reader, "LoadType"),
                                    Route = OurUtility.ValueOf(reader, "Route"),
                                };

                                items.Add(item);
                            }
                        }

                        connection.Close();
                    }

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
    }
}