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
    public class LoadingActualController : Controller
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

            string site = OurUtility.ValueOf(Request, "site");
            int site_i = OurUtility.ToInt32(site);
            string shipmentType = OurUtility.ValueOf(Request, "shipmentType");
            string dateFrom = OurUtility.ValueOf(Request, "dateFrom");
            string dateTo = OurUtility.ValueOf(Request, "dateTo");

            bool is_site_ALL = (string.IsNullOrEmpty(site) || site == "all");
            bool is_shipmentType_ALL = (string.IsNullOrEmpty(shipmentType) || shipmentType == "all");

            bool isAll_Text = true;
            string txt = Request["txt"];
            isAll_Text = string.IsNullOrEmpty(txt);

            // -- Actual code
            try
            {
                DateTime dateFrom_O = DateTime.Now;
                DateTime dateTo_O = DateTime.Now.AddDays(1);

                try
                {
                    dateFrom_O = DateTime.Parse(dateFrom);
                }
                catch {}
                try
                {
                    dateTo_O = DateTime.Parse(dateTo).AddDays(1);
                }
                catch {}

                using (MERCY_Ctx db = new MERCY_Ctx())
                {
                    var dataQuery =
                            (
                                from d in db.Loading_Actual
                                    join s in db.Sites on d.SiteId equals s.SiteId
                                where (is_site_ALL || d.SiteId == site_i)
                                        && (is_shipmentType_ALL || d.Shipment_Type == shipmentType)
                                        && d.Arrival_Time >= dateFrom_O
                                        && d.Arrival_Time < dateTo_O
                                        && (isAll_Text || d.No_Ref_Report.Contains(txt)
                                                    || d.No_Services_Trip.Contains(txt)
                                                    || d.TugName.Contains(txt)
                                                    || d.Barge_Name.Contains(txt))
                                orderby d.RecordId
                                select new Model_View_Loading_Actual
                                {
                                    RecordId = d.RecordId
                                    , SiteId = d.SiteId
                                    , CreatedOn = d.CreatedOn
                                    , Status = d.Status
                                    , SiteName = s.SiteName
                                    , No_Ref_Report = d.No_Ref_Report
                                    , No_Services_Trip = d.No_Services_Trip
                                    , TugName = d.TugName
                                    , Barge_Name = d.Barge_Name
                                    , Route = d.Route
                                    , Load_Type = d.Load_Type
                                    , Arrival_Time = d.Arrival_Time
                                    , Departure = d.Departure
                                    , Coal_Quality_Spec = d.Coal_Quality_Spec
                                    , Cargo_Loaded = ""
                                    , Shipment_Type = d.Shipment_Type
                                }
                            );

                    var items = dataQuery.ToList();
                    try
                    {
                        items.ForEach(c =>
                        {
                            c.RequestedOn_Str = OurUtility.DateFormat(c.CreatedOn, @"dd-MMM-yyyy HH:mm");
                            c.Arrival_Str = OurUtility.DateFormat(c.Arrival_Time, @"dd-MMM-yyyy HH:mm");
                            c.Departure_Str = OurUtility.DateFormat(c.Departure, @"dd-MMM-yyyy HH:mm");
                        });
                    }
                    catch {}

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
                    var data = new Loading_Actual
                    {
                        TugName = OurUtility.ValueOf(p_collection, "tug"),
                        RequestId = OurUtility.ToInt32(OurUtility.ValueOf(p_collection, "Request_Id"))
                    };
                    data.Shipment_Type = Shipment_Type(db, data.RequestId);
                    data.No_Ref_Report = OurUtility.ValueOf(p_collection, "ref_report");
                    data.SiteId = OurUtility.ToInt32(OurUtility.ValueOf(p_collection, "site"));
                    data.No_Services_Trip = OurUtility.ValueOf(p_collection, "service");
                    data.Barge_Name = OurUtility.ValueOf(p_collection, "barge");
                    data.Barge_Size = OurUtility.ToInt32(OurUtility.ValueOf(p_collection, "size"));
                    data.Route = OurUtility.ValueOf(p_collection, "route");
                    data.Load_Type = OurUtility.ValueOf(p_collection, "load_type");

                    data.Arrival_Time = OurUtility.ToDateTime(OurUtility.ValueOf(p_collection, "Arrival_Time"));
                    data.Initial_Draft = OurUtility.ToDateTime(OurUtility.ValueOf(p_collection, "Initial_Draft"));
                    data.Anchor_Up = OurUtility.ToDateTime(OurUtility.ValueOf(p_collection, "Anchor_Up"));
                    data.Berthed_Time = OurUtility.ToDateTime(OurUtility.ValueOf(p_collection, "Berthed_Time"));
                    data.Commenced_Loading = OurUtility.ToDateTime(OurUtility.ValueOf(p_collection, "Commenced_Loading"));
                    data.Completed_Loading = OurUtility.ToDateTime(OurUtility.ValueOf(p_collection, "Completed_Loading"));
                    data.Unberthing = OurUtility.ToDateTime(OurUtility.ValueOf(p_collection, "Unberthing"));
                    data.Departure = OurUtility.ToDateTime(OurUtility.ValueOf(p_collection, "Departure"));

                    data.Coal_Quality_Spec = OurUtility.ValueOf(p_collection, "coal_quality");
                    data.Delay_Cause_of_Barge_Changing = OurUtility.ValueOf(p_collection, "delay_cause");
                    data.Surveyor_Name = OurUtility.ValueOf(p_collection, "surveyor");
                    data.Weather_Condition = OurUtility.ValueOf(p_collection, "weather");
                    data.Water_Level_During_Loading = OurUtility.ToDecimal(OurUtility.ValueOf(p_collection, "Water_Level_During_Loading"));
                    data.Daily_Water_Level = OurUtility.ToDecimal(OurUtility.ValueOf(p_collection, "Daily_Water_Level"));
                    data.Water_Level_at_Jetty = OurUtility.ToDecimal(OurUtility.ValueOf(p_collection, "Water_Level_at_Jetty"));

                    data.Status = "Draft";

                    data.CreatedOn = DateTime.Now;
                    data.CreatedBy = user.UserId;

                    db.Loading_Actual.Add(data);
                    db.SaveChanges();

                    long id = data.RecordId;

                    Process_Cargo_Loaded(db, user, id, OurUtility.ValueOf(p_collection, "Detail_CargoLoaded"));
                    Process_Fuel_Consumption(db, user, id, OurUtility.ValueOf(p_collection, "Detail_FUEL_CONSUMPTION"));
                    Process_Draft_Survey(db, user, id, OurUtility.ValueOf(p_collection, "Detail_DRAFT_SURVEY"));
                    Process_Barge_Survey(db, user, id, OurUtility.ValueOf(p_collection, "Detail_BARGE_SURVEY"));
                    Process_Coal_Temperature(db, user, id, OurUtility.ValueOf(p_collection, "Detail_COAL_TEMPERATURE"));
                    Process_Attachements(db, user, id, p_collection);

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
            if ( ! permission_Item.Is_Edit)
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
                            from d in db.Loading_Actual
                            where d.RecordId == id
                            select d
                        );

                    var data = dataQuery.SingleOrDefault();

                    if (data.Status != "Draft")
                    {
                        var resultx = new { Success = false, Permission = permission_Item, Message = "Status: Final, data can't be modified!", MessageDetail = string.Empty, Version = Configuration.VERSION, Items = string.Empty, Total = 0 };
                        return Json(resultx, JsonRequestBehavior.AllowGet);
                    }

                    data.TugName = OurUtility.ValueOf(p_collection, "tug");
                    data.No_Ref_Report = OurUtility.ValueOf(p_collection, "ref_report");
                    data.SiteId = OurUtility.ToInt32(OurUtility.ValueOf(p_collection, "site"));
                    data.No_Services_Trip = OurUtility.ValueOf(p_collection, "service");
                    data.Barge_Name = OurUtility.ValueOf(p_collection, "barge");
                    data.Barge_Size = OurUtility.ToInt32(OurUtility.ValueOf(p_collection, "size"));
                    data.Route = OurUtility.ValueOf(p_collection, "route");
                    data.Load_Type = OurUtility.ValueOf(p_collection, "load_type");

                    data.Arrival_Time = OurUtility.ToDateTime(OurUtility.ValueOf(p_collection, "Arrival_Time"));
                    data.Initial_Draft = OurUtility.ToDateTime(OurUtility.ValueOf(p_collection, "Initial_Draft"));
                    data.Anchor_Up = OurUtility.ToDateTime(OurUtility.ValueOf(p_collection, "Anchor_Up"));
                    data.Berthed_Time = OurUtility.ToDateTime(OurUtility.ValueOf(p_collection, "Berthed_Time"));
                    data.Commenced_Loading = OurUtility.ToDateTime(OurUtility.ValueOf(p_collection, "Commenced_Loading"));
                    data.Completed_Loading = OurUtility.ToDateTime(OurUtility.ValueOf(p_collection, "Completed_Loading"));
                    data.Unberthing = OurUtility.ToDateTime(OurUtility.ValueOf(p_collection, "Unberthing"));
                    data.Departure = OurUtility.ToDateTime(OurUtility.ValueOf(p_collection, "Departure"));

                    data.Coal_Quality_Spec = OurUtility.ValueOf(p_collection, "coal_quality");
                    data.Delay_Cause_of_Barge_Changing = OurUtility.ValueOf(p_collection, "delay_cause");
                    data.Surveyor_Name = OurUtility.ValueOf(p_collection, "surveyor");
                    data.Weather_Condition = OurUtility.ValueOf(p_collection, "weather");
                    data.Water_Level_During_Loading = OurUtility.ToDecimal(OurUtility.ValueOf(p_collection, "Water_Level_During_Loading"));
                    data.Daily_Water_Level = OurUtility.ToDecimal(OurUtility.ValueOf(p_collection, "Daily_Water_Level"));
                    data.Water_Level_at_Jetty = OurUtility.ToDecimal(OurUtility.ValueOf(p_collection, "Water_Level_at_Jetty"));

                    //data.Status = ;

                    data.LastModifiedOn = DateTime.Now;
                    data.LastModifiedBy = user.UserId;

                    db.SaveChanges();

                    Process_Cargo_Loaded(db, user, id, OurUtility.ValueOf(p_collection, "Detail_CargoLoaded"));
                    Process_Fuel_Consumption(db, user, id, OurUtility.ValueOf(p_collection, "Detail_FUEL_CONSUMPTION"));
                    Process_Draft_Survey(db, user, id, OurUtility.ValueOf(p_collection, "Detail_DRAFT_SURVEY"));
                    Process_Barge_Survey(db, user, id, OurUtility.ValueOf(p_collection, "Detail_BARGE_SURVEY"));
                    Process_Coal_Temperature(db, user, id, OurUtility.ValueOf(p_collection, "Detail_COAL_TEMPERATURE"));
                    Process_Attachements(db, user, id, p_collection);

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


        [HttpGet]
        public JsonResult Get()
        {
            // Check Permission based on Current Url
            UserX user = new UserX(Request);
            Permission_Item permission_Item = null;

            Permission.Check_API(Request, user, ref permission_Item);
            /*if ( ! permission_Item.Is_View)
            {
                var msg = new { Success = false, Permission = permission_Item, Message = Permission.ERROR_PERMISSION_READ, MessageDetail = string.Empty, Version = Configuration.VERSION };
                return Json(msg, JsonRequestBehavior.AllowGet);
            }*/

            bool is_Show_User_Menu = OurUtility.ValueOf(Request, "u_menu").Equals("1");
            bool is_Show_User_Relation = OurUtility.ValueOf(Request, "u_relation").Equals("1");

            long id = OurUtility.ToInt64(Request[".id"]);

            List<Model_Id_Text> ref_Tunnels = null;
            List<Model_Id_Text> ref_Barge_Survey_Condition = null;
            List<Model_Id_Text> ref_Draft_Survey = null;
            List<Model_Id_Text> ref_Fuel_Rob = null;
            List<Model_Default_Actual_Barge_Loading> default_Actual_Barge_Loadings = new List<Model_Default_Actual_Barge_Loading>();

            try
            {
                using (MERCY_Ctx db = new MERCY_Ctx())
                {
                    var data_Tunnels =
                        (
                            from d in db.Tunnels
                            orderby d.Name
                            select new Model_Id_Text
                            {
                                id = d.TunnelId.ToString()
                                , text = d.Name
                            }
                        );
                    ref_Tunnels = data_Tunnels.ToList();

                    var data_Barge_Survey_Condition =
                        (
                            from d in db.Barge_Survey_Condition
                            orderby d.Barge_Survey_Condition_Name
                            select new Model_Id_Text
                            {
                                id = d.RecordId.ToString()
                                , text = d.Barge_Survey_Condition_Name
                            }
                        );
                    ref_Barge_Survey_Condition = data_Barge_Survey_Condition.ToList();

                    var data_Draft_Survey =
                        (
                            from d in db.Draft_Survey
                            orderby d.Draft_Survey_Name
                            select new Model_Id_Text
                            {
                                id = d.RecordId.ToString()
                                , text = d.Draft_Survey_Name
                            }
                        );
                    ref_Draft_Survey = data_Draft_Survey.ToList();

                    var data_Fuel_Rob =
                        (
                            from d in db.Fuel_Rob
                            orderby d.Fuel_Rob_Name
                            select new Model_Id_Text
                            {
                                id = d.RecordId.ToString()
                                , text = d.Fuel_Rob_Name
                            }
                        );
                    ref_Fuel_Rob = data_Fuel_Rob.ToList();

                    if (id <= 0)
                    {
                        using (var connection = new SqlConnection(db.Database.Connection.ConnectionString))
                        {
                            connection.Open();

                            SqlCommand command = connection.CreateCommand();
                            SqlCommand command_Detail = connection.CreateCommand();

                            string sql = string.Empty;

                            sql = string.Format(@"
                                                select distinct d.Request_Id, 1 as No_Ref_Report, d.Tug, d.Barge, d.Trip_ID, d.Quantity as Barge_Size, d.Product as Coal_Quality
                                                    , 'Transshipment' as LoadType, 'JBG' as Route
                                                from Loading_Request_Loading_Plan_Detail_Barge d
                                                where d.Tug not in
                                                (
                                                    select TugName from Loading_Actual
                                                )
                                                    and CreatedOn_Date_Only = (select Max(CreatedOn_Date_Only) from Loading_Request_Loading_Plan_Detail_Barge)
                                                order by d.Tug
                                                ");

                            command.CommandText = sql;

                            Model_Default_Actual_Barge_Loading item = null;
                            using (SqlDataReader reader = command.ExecuteReader())
                            {
                                while (reader.Read())
                                {
                                    item = new Model_Default_Actual_Barge_Loading
                                    {
                                        Request_Id = OurUtility.ValueOf(reader, "Request_Id"),
                                        Tug_Name = OurUtility.ValueOf(reader, "Tug"),
                                        No_Service_Trip = OurUtility.ValueOf(reader, "Trip_ID"),
                                        No_Ref_Report = OurUtility.ValueOf(reader, "No_Ref_Report"),
                                        BargeName = OurUtility.ValueOf(reader, "Barge"),
                                        BargeSize = OurUtility.ValueOf(reader, "Barge_Size"),
                                        Route = OurUtility.ValueOf(reader, "Route"),
                                        LoadType = OurUtility.ValueOf(reader, "LoadType"),
                                        Coal_Quality = OurUtility.ValueOf(reader, "Coal_Quality")
                                    };

                                    default_Actual_Barge_Loadings.Add(item);
                                }
                            }
                        }
                    }
                }
            }
            catch {}

            if (id <= 0)
            {
                // -- special purpose
                // this Id is only for Checking CurrentUser:Info

                var result = new { Success = true, Permission = permission_Item, User = UserX.Information(user, is_Show_User_Menu, is_Show_User_Relation), Version = Configuration.VERSION
                                    , Ref_Tunnels = ref_Tunnels, Ref_Barge_Survey_Condition = ref_Barge_Survey_Condition, Ref_Draft_Survey = ref_Draft_Survey, Ref_Fuel_Rob = ref_Fuel_Rob
                                    , Default_Actual = default_Actual_Barge_Loadings
                };
                return Json(result, JsonRequestBehavior.AllowGet);
            }

            try
            {
                using (MERCY_Ctx db = new MERCY_Ctx())
                {
                    var dataQuery =
                            (
                                from d in db.Loading_Actual
                                where d.RecordId == id
                                select new Model_View_Loading_Actual_Detail
                                {
                                    TugName = d.TugName
                                    , RequestId = d.RequestId
                                    , No_Ref_Report = d.No_Ref_Report
                                    , SiteId = d.SiteId
                                    , No_Services_Trip = d.No_Services_Trip
                                    , Barge_Name = d.Barge_Name
                                    , Barge_Size = d.Barge_Size
                                    , Route = d.Route
                                    , Load_Type = d.Load_Type
                                    , Arrival_Time = d.Arrival_Time
                                    , Initial_Draft = d.Initial_Draft
                                    , Anchor_Up = d.Anchor_Up
                                    , Berthed_Time = d.Berthed_Time
                                    , Commenced_Loading = d.Commenced_Loading
                                    , Completed_Loading = d.Completed_Loading
                                    , Unberthing = d.Unberthing
                                    , Departure = d.Departure
                                    , Coal_Quality_Spec = d.Coal_Quality_Spec
                                    , Delay_Cause_of_Barge_Changing = d.Delay_Cause_of_Barge_Changing
                                    , Surveyor_Name = d.Surveyor_Name
                                    , Weather_Condition = d.Weather_Condition
                                    , Water_Level_During_Loading = d.Water_Level_During_Loading
                                    , Daily_Water_Level = d.Daily_Water_Level
                                    , Water_Level_at_Jetty = d.Water_Level_at_Jetty
                                    , Status = d.Status
                                }
                            );

                    var item = dataQuery.SingleOrDefault();
                    if (item == null)
                    {
                        var result_NotFound = new { Success = false, Permission = permission_Item, User = UserX.Information(user, is_Show_User_Menu, is_Show_User_Relation), Message = "Id: " + id.ToString() + " is not found", MessageDetail = string.Empty, Version = Configuration.VERSION };
                        return Json(result_NotFound, JsonRequestBehavior.AllowGet);
                    }

                    item.Arrival_Str = OurUtility.DateFormat(item.Arrival_Time, @"dd-MMM-yyyy - HH:mm");
                    item.Initial_Draft_Str = OurUtility.DateFormat(item.Initial_Draft, @"dd-MMM-yyyy - HH:mm");
                    item.Anchor_Up_Str = OurUtility.DateFormat(item.Anchor_Up, @"dd-MMM-yyyy - HH:mm");
                    item.Berthed_Time_Str = OurUtility.DateFormat(item.Berthed_Time, @"dd-MMM-yyyy - HH:mm");
                    item.Commenced_Loading_Str = OurUtility.DateFormat(item.Commenced_Loading, @"dd-MMM-yyyy - HH:mm");
                    item.Completed_Loading_Str = OurUtility.DateFormat(item.Completed_Loading, @"dd-MMM-yyyy - HH:mm");
                    item.Unberthing_Str = OurUtility.DateFormat(item.Unberthing, @"dd-MMM-yyyy - HH:mm");
                    item.Departure_Str = OurUtility.DateFormat(item.Departure, @"dd-MMM-yyyy - HH:mm");

                    item.Arrival_Str2 = OurUtility.DateFormat(item.Arrival_Time, @"HH:mm");
                    item.Initial_Draft_Str2 = OurUtility.DateFormat(item.Initial_Draft, @"HH:mm");
                    item.Anchor_Up_Str2 = OurUtility.DateFormat(item.Anchor_Up, @"HH:mm");
                    item.Berthed_Time_Str2 = OurUtility.DateFormat(item.Berthed_Time, @"HH:mm");
                    item.Commenced_Loading_Str2 = OurUtility.DateFormat(item.Commenced_Loading, @"HH:mm");
                    item.Completed_Loading_Str2 = OurUtility.DateFormat(item.Completed_Loading, @"HH:mm");
                    item.Unberthing_Str2 = OurUtility.DateFormat(item.Unberthing, @"HH:mm");
                    item.Departure_Str2 = OurUtility.DateFormat(item.Departure, @"HH:mm");

                    var dataQuery_Cargo_Loaded =
                            (
                                from d in db.Loading_Actual_Cargo_Loaded
                                join t in db.Tunnels on d.TunnelId equals t.TunnelId
                                where d.ActualId == id
                                orderby t.Name
                                select new Model_View_Loading_Actual_Cargo_Loaded
                                {
                                    RecordId = d.RecordId
                                    , ActualId = d.ActualId
                                    , TunnelId = d.TunnelId
                                    , TunnelName = t.Name
                                    , Belt = d.Belt
                                    , Draft = d.Draft
                                }
                            );

                    var items_Cargo_Loaded = dataQuery_Cargo_Loaded.ToList();

                    var dataQuery_Fuel_Consumption =
                            (
                                from d in db.Loading_Actual_Fuel_Consumption
                                join f in db.Fuel_Rob on d.Fuel_Rob_Id equals f.RecordId
                                where d.ActualId == id
                                orderby f.Fuel_Rob_Name
                                select new Model_View_Loading_Actual_Fuel_Consumption
                                {
                                    RecordId = d.RecordId
                                    , Fuel_Rob_Id = d.Fuel_Rob_Id
                                    , Fuel_Rob_Name = f.Fuel_Rob_Name
                                    , Total_Consumption = d.Total_Consumption
                                }
                            );

                    var items_Fuel_Consumption = dataQuery_Fuel_Consumption.ToList();

                    var dataQuery_Draft_Survey =
                            (
                                from d in db.Loading_Actual_Draft_Survey
                                join s in db.Draft_Survey on d.Draft_Survey_Id equals s.RecordId
                                where d.ActualId == id
                                orderby s.Draft_Survey_Name
                                select new Model_View_Loading_Actual_Draft_Survey
                                {
                                    RecordId = d.RecordId
                                    , ActualId = d.ActualId
                                    , Draft_Survey_Id = d.Draft_Survey_Id
                                    , Draft_Survey_Name = s.Draft_Survey_Name
                                    , Initial = d.Initial
                                    , Final = d.Final
                                }
                            );

                    var items_Draft_Survey = dataQuery_Draft_Survey.ToList();

                    var dataQuery_Barge_Survey_Condition =
                            (
                                from d in db.Loading_Actual_Barge_Survey_Condition
                                join s in db.Barge_Survey_Condition on d.Barge_Survey_Condition_Id equals s.RecordId
                                where d.ActualId == id
                                orderby s.Barge_Survey_Condition_Name
                                select new Model_View_Loading_Actual_Barge_Survey_Condition
                                {
                                    RecordId = d.RecordId
                                    , ActualId = d.ActualId
                                    , Barge_Survey_Condition_Id = d.Barge_Survey_Condition_Id
                                    , Barge_Survey_Name = s.Barge_Survey_Condition_Name
                                    , Condition = d.Condition
                                    , Remark = d.Remark
                                }
                            );

                    var items_Barge_Survey_Condition = dataQuery_Barge_Survey_Condition.ToList();

                    var dataQuery_Coal_Temperature =
                            (
                                from d in db.Loading_Actual_Coal_Temperature
                                where d.ActualId == id
                                orderby d.RecordId
                                select new
                                {
                                    d.RecordId
                                    , d.ActualId
                                    , d.Temperature
                                }
                            );

                    var items_Coal_Temperature = dataQuery_Coal_Temperature.ToList();

                    var dataQuery_Attachments =
                            (
                                from d in db.Loading_Actual_Attachments
                                where d.ActualId == id
                                orderby d.FileName
                                select new
                                {
                                    d.RecordId
                                    , d.ActualId
                                    , d.FileName
                                    , d.FileName2
                                }
                            );

                    var items_Attachments = dataQuery_Attachments.ToList();

                    var result = new { Success = true, Permission = permission_Item, User = UserX.Information(user, is_Show_User_Menu, is_Show_User_Relation), Message = string.Empty, MessageDetail = string.Empty, Version = Configuration.VERSION
                                        , Item = item, Cargo_Loaded = items_Cargo_Loaded, Fuel_Consumption = items_Fuel_Consumption
                                        , Draft_Survey = items_Draft_Survey, Barge_Survey_Condition = items_Barge_Survey_Condition, Coal_Temperature = items_Coal_Temperature, Attachments = items_Attachments
                                        , Ref_Tunnels = ref_Tunnels, Ref_Barge_Survey_Condition = ref_Barge_Survey_Condition, Ref_Draft_Survey = ref_Draft_Survey, Ref_Fuel_Rob = ref_Fuel_Rob
                    };
                                        
                    return Json(result, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                var result = new { Success = false, Permission = permission_Item, ex.Message, MessageDetail = string.Empty, Version = Configuration.VERSION, Items = string.Empty, Total = 0 };
                return Json(result, JsonRequestBehavior.AllowGet);
            }
        }

        private void Process_Cargo_Loaded(MERCY_Ctx p_db, UserX p_executedBy, long p_actual, string p_cargo_Loaded)
        {
            string recordId_str = string.Empty;
            string tunnel = string.Empty;
            string belt = string.Empty;
            string draft = string.Empty;

            int recordId = 0;

            try
            {
                string[] raw_data = p_cargo_Loaded.Split(new[] { "+++" }, StringSplitOptions.None);
                string part_data = raw_data[0];
                string[] records = part_data.Split(new[] { "###" }, StringSplitOptions.RemoveEmptyEntries);

                foreach (string record in records)
                {
                    string[] items = record.Split(new[] { "xxx" }, StringSplitOptions.RemoveEmptyEntries);

                    recordId_str = items[0];
                    recordId = OurUtility.ToInt32(recordId_str);
                    if (recordId == 0) continue;

                    tunnel = items[1];
                    belt = items[2];
                    draft = items[3];

                    try
                    {
                        if (recordId < 0)
                        {
                            // -- Add Record
                            var data = new Loading_Actual_Cargo_Loaded
                            {
                                ActualId = p_actual,
                                TunnelId = OurUtility.ToInt32(tunnel),
                                Belt = OurUtility.ToDecimal(belt),
                                Draft = OurUtility.ToDecimal(draft),

                                CreatedOn = DateTime.Now,
                                CreatedBy = p_executedBy.UserId
                            };

                            p_db.Loading_Actual_Cargo_Loaded.Add(data);
                            p_db.SaveChanges();
                        }
                        else
                        {
                            // -- Edit Record
                            var dataQuery =
                                   (
                                       from d in p_db.Loading_Actual_Cargo_Loaded
                                       where d.RecordId == recordId
                                       select d
                                   );

                            var data = dataQuery.SingleOrDefault();
                            if (data == null) continue;

                            data.ActualId = p_actual;
                            data.TunnelId = OurUtility.ToInt32(tunnel);
                            data.Belt = OurUtility.ToDecimal(belt);
                            data.Draft = OurUtility.ToDecimal(draft);

                            data.LastModifiedOn = DateTime.Now;
                            data.LastModifiedBy = p_executedBy.UserId;

                            p_db.SaveChanges();
                        }
                    }
                    catch {}
                }
            }
            catch {}

            try
            {
                // data Deleted
                string[] raw_data = p_cargo_Loaded.Split(new[] { "+++" }, StringSplitOptions.None);
                string part_deleted = raw_data[1];
                string[] deleted_ids = part_deleted.Split(',');
                foreach (string idx in deleted_ids)
                {
                    if (string.IsNullOrEmpty(idx.Trim())) continue;

                    recordId = OurUtility.ToInt32(idx.Trim());
                    try
                    {
                        Loading_Actual_Cargo_Loaded data = p_db.Loading_Actual_Cargo_Loaded.Find(recordId);
                        p_db.Loading_Actual_Cargo_Loaded.Remove(data);
                        p_db.SaveChanges();
                    }
                    catch {}
                }
            }
            catch {}
        }

        private void Process_Fuel_Consumption(MERCY_Ctx p_db, UserX p_executedBy, long p_actual, string p_fuel)
        {
            string recordId_str = string.Empty;
            string rob = string.Empty;
            string liter = string.Empty;

            int recordId = 0;

            try
            {
                string[] raw_data = p_fuel.Split(new[] { "+++" }, StringSplitOptions.None);
                string part_data = raw_data[0];
                string[] records = part_data.Split(new[] { "###" }, StringSplitOptions.RemoveEmptyEntries);

                foreach (string record in records)
                {
                    string[] items = record.Split(new[] { "xxx" }, StringSplitOptions.RemoveEmptyEntries);

                    recordId_str = items[0];
                    recordId = OurUtility.ToInt32(recordId_str);
                    if (recordId == 0) continue;

                    rob = items[1];
                    liter = items[2];

                    try
                    {
                        if (recordId < 0)
                        {
                            // -- Add Record
                            var data = new Loading_Actual_Fuel_Consumption
                            {
                                ActualId = p_actual,
                                Fuel_Rob_Id = OurUtility.ToInt32(rob),
                                Total_Consumption = OurUtility.ToInt32(liter),

                                CreatedOn = DateTime.Now,
                                CreatedBy = p_executedBy.UserId
                            };

                            p_db.Loading_Actual_Fuel_Consumption.Add(data);
                            p_db.SaveChanges();
                        }
                        else
                        {
                            // -- Edit Record
                            var dataQuery =
                                   (
                                       from d in p_db.Loading_Actual_Fuel_Consumption
                                       where d.RecordId == recordId
                                       select d
                                   );

                            var data = dataQuery.SingleOrDefault();
                            if (data == null) continue;

                            data.ActualId = p_actual;
                            data.Fuel_Rob_Id = OurUtility.ToInt32(rob);
                            data.Total_Consumption = OurUtility.ToInt32(liter);

                            data.LastModifiedOn = DateTime.Now;
                            data.LastModifiedBy = p_executedBy.UserId;

                            p_db.SaveChanges();
                        }
                    }
                    catch {}
                }
            }
            catch {}

            try
            {
                // data Deleted
                string[] raw_data = p_fuel.Split(new[] { "+++" }, StringSplitOptions.None);
                string part_deleted = raw_data[1];
                string[] deleted_ids = part_deleted.Split(',');
                foreach (string idx in deleted_ids)
                {
                    if (string.IsNullOrEmpty(idx.Trim())) continue;

                    recordId = OurUtility.ToInt32(idx.Trim());
                    try
                    {
                        Loading_Actual_Fuel_Consumption data = p_db.Loading_Actual_Fuel_Consumption.Find(recordId);
                        p_db.Loading_Actual_Fuel_Consumption.Remove(data);
                        p_db.SaveChanges();
                    }
                    catch {}
                }
            }
            catch {}
        }

        private void Process_Draft_Survey(MERCY_Ctx p_db, UserX p_executedBy, long p_actual, string p_draft_survey)
        {
            string recordId_str = string.Empty;
            string survey = string.Empty;
            string initial = string.Empty;
            string final = string.Empty;

            int recordId = 0;

            try
            {
                string[] raw_data = p_draft_survey.Split(new[] { "+++" }, StringSplitOptions.None);
                string part_data = raw_data[0];
                string[] records = part_data.Split(new[] { "###" }, StringSplitOptions.RemoveEmptyEntries);

                foreach (string record in records)
                {
                    string[] items = record.Split(new[] { "xxx" }, StringSplitOptions.RemoveEmptyEntries);

                    recordId_str = items[0];
                    recordId = OurUtility.ToInt32(recordId_str);
                    if (recordId == 0) continue;

                    survey = items[1];
                    initial = items[2];
                    final = items[3];

                    try
                    {
                        if (recordId < 0)
                        {
                            // -- Add Record
                            var data = new Loading_Actual_Draft_Survey
                            {
                                ActualId = p_actual,
                                Draft_Survey_Id = OurUtility.ToInt32(survey),
                                Initial = OurUtility.ToDecimal(initial),
                                Final = OurUtility.ToDecimal(final),

                                CreatedOn = DateTime.Now,
                                CreatedBy = p_executedBy.UserId
                            };

                            p_db.Loading_Actual_Draft_Survey.Add(data);
                            p_db.SaveChanges();
                        }
                        else
                        {
                            // -- Edit Record
                            var dataQuery =
                                   (
                                       from d in p_db.Loading_Actual_Draft_Survey
                                       where d.RecordId == recordId
                                       select d
                                   );

                            var data = dataQuery.SingleOrDefault();
                            if (data == null) continue;

                            data.ActualId = p_actual;
                            data.Draft_Survey_Id = OurUtility.ToInt32(survey);
                            data.Initial = OurUtility.ToDecimal(initial);
                            data.Final = OurUtility.ToDecimal(final);

                            data.LastModifiedOn = DateTime.Now;
                            data.LastModifiedBy = p_executedBy.UserId;

                            p_db.SaveChanges();
                        }
                    }
                    catch {}
                }
            }
            catch {}

            try
            {
                // data Deleted
                string[] raw_data = p_draft_survey.Split(new[] { "+++" }, StringSplitOptions.None);
                string part_deleted = raw_data[1];
                string[] deleted_ids = part_deleted.Split(',');
                foreach (string idx in deleted_ids)
                {
                    if (string.IsNullOrEmpty(idx.Trim())) continue;

                    recordId = OurUtility.ToInt32(idx.Trim());
                    try
                    {
                        Loading_Actual_Draft_Survey data = p_db.Loading_Actual_Draft_Survey.Find(recordId);
                        p_db.Loading_Actual_Draft_Survey.Remove(data);
                        p_db.SaveChanges();
                    }
                    catch {}
                }
            }
            catch {}
        }

        private void Process_Barge_Survey(MERCY_Ctx p_db, UserX p_executedBy, long p_actual, string p_draft_survey)
        {
            string recordId_str = string.Empty;
            string survey = string.Empty;
            string condition = string.Empty;
            string remark = string.Empty;

            int recordId = 0;

            try
            {
                string[] raw_data = p_draft_survey.Split(new[] { "+++" }, StringSplitOptions.None);
                string part_data = raw_data[0];
                string[] records = part_data.Split(new[] { "###" }, StringSplitOptions.RemoveEmptyEntries);

                foreach (string record in records)
                {
                    string[] items = record.Split(new[] { "xxx" }, StringSplitOptions.RemoveEmptyEntries);

                    recordId_str = items[0];
                    recordId = OurUtility.ToInt32(recordId_str);
                    if (recordId == 0) continue;

                    survey = items[1];
                    condition = items[2];
                    remark = items[3];

                    try
                    {
                        if (recordId < 0)
                        {
                            // -- Add Record
                            var data = new Loading_Actual_Barge_Survey_Condition
                            {
                                ActualId = p_actual,
                                Barge_Survey_Condition_Id = OurUtility.ToInt32(survey),
                                Condition = condition,
                                Remark = remark,

                                CreatedOn = DateTime.Now,
                                CreatedBy = p_executedBy.UserId
                            };

                            p_db.Loading_Actual_Barge_Survey_Condition.Add(data);
                            p_db.SaveChanges();
                        }
                        else
                        {
                            // -- Edit Record
                            var dataQuery =
                                   (
                                       from d in p_db.Loading_Actual_Barge_Survey_Condition
                                       where d.RecordId == recordId
                                       select d
                                   );

                            var data = dataQuery.SingleOrDefault();
                            if (data == null) continue;

                            data.ActualId = p_actual;
                            data.Barge_Survey_Condition_Id = OurUtility.ToInt32(survey);
                            data.Condition = condition;
                            data.Remark = remark;

                            data.LastModifiedOn = DateTime.Now;
                            data.LastModifiedBy = p_executedBy.UserId;

                            p_db.SaveChanges();
                        }
                    }
                    catch {}
                }
            }
            catch {}

            try
            {
                // data Deleted
                string[] raw_data = p_draft_survey.Split(new[] { "+++" }, StringSplitOptions.None);
                string part_deleted = raw_data[1];
                string[] deleted_ids = part_deleted.Split(',');
                foreach (string idx in deleted_ids)
                {
                    if (string.IsNullOrEmpty(idx.Trim())) continue;

                    recordId = OurUtility.ToInt32(idx.Trim());
                    try
                    {
                        Loading_Actual_Barge_Survey_Condition data = p_db.Loading_Actual_Barge_Survey_Condition.Find(recordId);
                        p_db.Loading_Actual_Barge_Survey_Condition.Remove(data);
                        p_db.SaveChanges();
                    }
                    catch {}
                }
            }
            catch {}
        }

        private void Process_Coal_Temperature(MERCY_Ctx p_db, UserX p_executedBy, long p_actual, string p_draft_survey)
        {
            string recordId_str = string.Empty;
            string no = string.Empty;
            string temperature = string.Empty;

            int recordId = 0;

            try
            {
                string[] raw_data = p_draft_survey.Split(new[] { "+++" }, StringSplitOptions.None);
                string part_data = raw_data[0];
                string[] records = part_data.Split(new[] { "###" }, StringSplitOptions.RemoveEmptyEntries);

                foreach (string record in records)
                {
                    string[] items = record.Split(new[] { "xxx" }, StringSplitOptions.RemoveEmptyEntries);

                    recordId_str = items[0];
                    recordId = OurUtility.ToInt32(recordId_str);
                    if (recordId == 0) continue;

                    no = items[1];
                    temperature = items[2];

                    try
                    {
                        if (recordId < 0)
                        {
                            // -- Add Record
                            var data = new Loading_Actual_Coal_Temperature
                            {
                                ActualId = p_actual,
                                Temperature = OurUtility.ToDecimal(temperature),

                                CreatedOn = DateTime.Now,
                                CreatedBy = p_executedBy.UserId
                            };

                            p_db.Loading_Actual_Coal_Temperature.Add(data);
                            p_db.SaveChanges();
                        }
                        else
                        {
                            // -- Edit Record
                            var dataQuery =
                                   (
                                       from d in p_db.Loading_Actual_Coal_Temperature
                                       where d.RecordId == recordId
                                       select d
                                   );

                            var data = dataQuery.SingleOrDefault();
                            if (data == null) continue;

                            data.ActualId = p_actual;
                            data.Temperature = OurUtility.ToDecimal(temperature);

                            data.LastModifiedOn = DateTime.Now;
                            data.LastModifiedBy = p_executedBy.UserId;

                            p_db.SaveChanges();
                        }
                    }
                    catch {}
                }
            }
            catch {}

            try
            {
                // data Deleted
                string[] raw_data = p_draft_survey.Split(new[] { "+++" }, StringSplitOptions.None);
                string part_deleted = raw_data[1];
                string[] deleted_ids = part_deleted.Split(',');
                foreach (string idx in deleted_ids)
                {
                    if (string.IsNullOrEmpty(idx.Trim())) continue;

                    recordId = OurUtility.ToInt32(idx.Trim());
                    try
                    {
                        Loading_Actual_Coal_Temperature data = p_db.Loading_Actual_Coal_Temperature.Find(recordId);
                        p_db.Loading_Actual_Coal_Temperature.Remove(data);
                        p_db.SaveChanges();
                    }
                    catch {}
                }
            }
            catch {}
        }

        private void Process_Attachements(MERCY_Ctx p_db, UserX p_executedBy, long p_actual, FormCollection p_collection)
        {
            int file_Counter = OurUtility.ToInt32(OurUtility.ValueOf(p_collection, "file_Counter"));

            string fileName = string.Empty;
            string fileName2 = string.Empty;
            string msg = string.Empty;

            for (int i=1;i<=file_Counter;i++)
            {
                OurUtility.Upload(Request, "fileInput_Upload_" + i.ToString(), UploadFolder, ref fileName, ref fileName2, ref msg);

                try
                {
                    // -- Add Record
                    var data = new Loading_Actual_Attachments
                    {
                        ActualId = p_actual,
                        FileName = fileName,
                        FileName2 = fileName2,

                        CreatedOn = DateTime.Now,
                        CreatedBy = p_executedBy.UserId
                    };

                    p_db.Loading_Actual_Attachments.Add(data);
                    p_db.SaveChanges();
                }
                catch {}
            }

            try
            {
                // data Deleted
                string part_deleted = OurUtility.ValueOf(p_collection, "file_Deleted");
                string[] deleted_ids = part_deleted.Split(',');
                int recordId = 0;
                foreach (string idx in deleted_ids)
                {
                    if (string.IsNullOrEmpty(idx.Trim())) continue;

                    recordId = OurUtility.ToInt32(idx.Trim());
                    try
                    {
                        Loading_Actual_Attachments data = p_db.Loading_Actual_Attachments.Find(recordId);
                        p_db.Loading_Actual_Attachments.Remove(data);
                        p_db.SaveChanges();
                    }
                    catch {}
                }
            }
            catch {}
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

        public JsonResult Finalize(FormCollection p_collection)
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
                long id = OurUtility.ToInt64(Request[".id"]);

                using (MERCY_Ctx db = new MERCY_Ctx())
                {
                    var dataQuery =
                        (
                            from d in db.Loading_Actual
                            where d.RecordId == id
                            select d
                        );

                    var data = dataQuery.SingleOrDefault();

                    if (data.Status != "Draft")
                    {
                        var resultx = new { Success = false, Permission = permission_Item, Message = "Status: already Final!", MessageDetail = string.Empty, Version = Configuration.VERSION, Items = string.Empty, Total = 0 };
                        return Json(resultx, JsonRequestBehavior.AllowGet);
                    }

                    data.Status = "Finalized";

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

        private static string Shipment_Type(MERCY_Ctx p_db, int p_requestId)
        {
            string result = string.Empty;

            try
            {
                var dataQuery =
                        (
                            from d in p_db.Loading_Request_Loading_Plan
                            where d.Request_Id == p_requestId
                            select d
                        );

                result = dataQuery.Take(1).First().Shipment_Type;
            }
            catch {}

            return result;
        }
    }
}