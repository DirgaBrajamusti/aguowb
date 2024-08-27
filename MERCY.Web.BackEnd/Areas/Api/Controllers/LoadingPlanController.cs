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
using System.Globalization;

namespace MERCY.Web.BackEnd.Areas.Api.Controllers
{
    public class LoadingPlanController : Controller
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
            string shipmen_type = OurUtility.ValueOf(Request, "shipment_Type").Equals("all") ? string.Empty : OurUtility.ValueOf(Request, "shipment_Type");
            string vessel = OurUtility.ValueOf(Request, "vessel").Equals("all") ? string.Empty : OurUtility.ValueOf(Request, "vessel");
            string buyer = OurUtility.ValueOf(Request, "buyer").Equals("all") ? string.Empty : OurUtility.ValueOf(Request, "buyer");
            string allDate = OurUtility.ValueOf(Request, "dateFrom") == string.Empty ? "1" : "0";
            string date = OurUtility.ValueOf(Request, "dateFrom");
            string id = OurUtility.ValueOf(Request, ".id") == string.Empty ? "-1" : OurUtility.ValueOf(Request, ".id");

            if (allDate == "0")
            {
                DateTime time = DateTime.ParseExact(date, "dd-MMM-yyyy", CultureInfo.CurrentCulture);
                date = time.ToString("yyyy-MM-dd");
            }

            string txt = OurUtility.ValueOf(Request, "txt").ToUpper();
            bool isAllText = string.IsNullOrEmpty(txt);

            // -- Actual code
            try
            {
                List<Model_Loading_Plan> notFilteredItems = new List<Model_Loading_Plan>();
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
                                                    , p.ETA as ETA_Vessel_MBR
                                                    , p.CreatedOn
                                                from Loading_Plan p
                                                inner join Site s on s.SiteId = p.SiteId 
                                                inner join UserSite us on us.SiteId = s.SiteId 
                                                inner join Company c on c.CompanyCode = p.Company 
                                                inner join UserCompany uc on uc.CompanyCode = c.CompanyCode and uc.UserId = us.UserId
                                                where p.SiteId = '1'
	                                                and uc.UserId = '{0}'
	                                                and p.Company LIKE '{1}%'
	                                                and p.Shipment_Type LIKE '{2}%'
	                                                and p.Vessel LIKE '{3}%'
	                                                and p.Buyer LIKE '{4}%'
	                                                and ('1'={5} or ( p.CreatedOn >= '{6}' and p.CreatedOn <= '{6} 23:59:59'))
	                                                and (p.RecordId = {7} OR {7} = -1)
                                                order by p.CreatedOn desc",
                            user.UserId,company_code, shipmen_type, vessel, buyer, allDate, date, id);

                        command.CommandText = sql;

                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                var recordId = OurUtility.ToInt64(OurUtility.ValueOf(reader, "RecordId"));
                                
                                item = new Model_Loading_Plan
                                {
                                    RecordId = recordId,
                                    Trip_No = OurUtility.ValueOf(reader, "Trip_No"),
                                    Site_Str = OurUtility.ValueOf(reader, "Site_Str"),
                                    Company = OurUtility.ValueOf(reader, "Company"),
                                    Shipment_Type = OurUtility.ValueOf(reader, "Shipment_Type"),
                                    Destination = OurUtility.ValueOf(reader, "Destination"),
                                    Remark = OurUtility.ValueOf(reader, "Remark"),
                                    ETA_Vessel_MBR = OurUtility.ToDateTime(OurUtility.ValueOf(reader, "ETA_Vessel_MBR")).Date.ToString("dd-MM-yyyy"),

                                    PlanBlendingLoading = string.Empty,

                                    CreatedOn = OurUtility.ToDateTime(OurUtility.ValueOf(reader, "CreatedOn"))
                                };

                                item = GetPortionBlending(item, item.RecordId);

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

                                notFilteredItems.Add(item);
                            }
                        }

                        bool is_Show_Vessel_Buyer = OurUtility.ValueOf(Request, "show_Vessel_Buyer").Equals("1");
                        if (is_Show_Vessel_Buyer)
                        {
                            vessels = db.Loading_Plan
                                .Select(lr => lr.Vessel ?? string.Empty)
                                .Distinct()
                                .OrderBy(lrVessel => lrVessel)
                                .ToList();

                            buyers = db.Loading_Plan
                                .Select(lr => lr.Buyer ?? string.Empty)
                                .Distinct()
                                .OrderBy(lrBuyer => lrBuyer)
                                .ToList();
                        }

                        connection.Close();
                    }
                }

                var items = notFilteredItems.Where(x =>
                        isAllText
                        || (x.Destination?.ToUpper().Contains(txt) ?? false)
                        || (x.Trip_No?.ToUpper().Contains(txt) ?? false)
                        || (x.Product?.ToUpper().Contains(txt) ?? false)
                        || (x.Tug?.ToUpper().Contains(txt) ?? false))
                    .ToList();

                var result = new { Success = true, Permission = permission_Item, User = UserX.Information(user, is_Show_User_Menu, is_Show_User_Relation), Message = string.Empty, MessageDetail = string.Empty, Version = Configuration.VERSION, Items = items, Total = items.Count, Vessels = vessels, Buyers = buyers };
                return Json(result, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                var result = new { Success = false, Permission = permission_Item, ex.Message, MessageDetail = ex.StackTrace, Version = Configuration.VERSION, Items = string.Empty, Total = 0 };
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
            using (MERCY_Ctx db = new MERCY_Ctx())
            {
                try
                {

                    var refLoadingPlan = db.Loading_Plan.ToList();
                    string buyer = OurUtility.ValueOf(p_collection, "buyer");
                    string tripNo = OurUtility.ValueOf(p_collection, "tripNo");
                    bool isDataValid = true;

                    isDataValid = IsDataValid(OurUtility.ValueOf(p_collection, "Shipment_Type"), tripNo, buyer, refLoadingPlan);

                    if (!isDataValid)
                    {
                        var validationResult = new { Success = false, Permission = permission_Item, Message = "Data is not valid (already exist in database)", MessageDetail = string.Empty };
                        return Json(validationResult, JsonRequestBehavior.AllowGet);
                    }

                    var data = new Loading_Plan
                    {
                        SiteId = OurUtility.ToInt32(OurUtility.ValueOf(p_collection, "site")),
                        Company = OurUtility.ValueOf(p_collection, "company"),
                        Shipment_Type = OurUtility.ValueOf(p_collection, "Shipment_Type"),
                        Buyer = buyer,
                        Vessel = OurUtility.ValueOf(p_collection, "vessel")
                    };
                    if (OurUtility.ValueOf(p_collection, "ETA") != string.Empty)
                    {
                        data.ETA = OurUtility.ToDateTime(OurUtility.ValueOf(p_collection, "ETA"));
                    }
                    if (data.Vessel == "null") data.Vessel = string.Empty;
                    data.Destination = OurUtility.ValueOf(p_collection, "destination");
                    data.Trip_No = tripNo;
                    data.Remark = OurUtility.ValueOf(p_collection, "remark");

                    data.CreatedOn = DateTime.Now;
                    data.CreatedBy = user.UserId;
                    data.CreatedOn_Date_Only = data.CreatedOn.ToString("yyyy-MM-dd");

                    db.Loading_Plan.Add(data);
                    db.SaveChanges();

                    long id = data.RecordId;

                    var message = Create_Detail_Barge(p_collection, db, id, user, data.Shipment_Type, data.Vessel, buyer);
                    if (message == "Success")
                    {
                        var result = new { Success = true, Permission = permission_Item, Message = BaseConstants.MESSAGE_CREATE_SUCCESS, MessageDetail = string.Empty, Version = Configuration.VERSION, Data = id };
                        return Json(result, JsonRequestBehavior.AllowGet);
                    }
                    else
                    {
                        var validationResult = new { Success = false, Permission = permission_Item, Message = message, MessageDetail = string.Empty };
                        return Json(validationResult, JsonRequestBehavior.AllowGet);
                    }
                }
                catch (Exception ex)
                {
                    var result = new { Success = false, Permission = permission_Item, ex.Message, MessageDetail = string.Empty, Version = Configuration.VERSION, Items = string.Empty, Total = 0 };
                    return Json(result, JsonRequestBehavior.AllowGet);
                }

            }

        }

        [HttpGet]
        public JsonResult Get()
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

            long id = OurUtility.ToInt64(Request[".id"]);
            if (id <= 0)
            {
                // -- special purpose
                // this Id is only for Checking CurrentUser:Info\

                var result = new { Success = true, Permission = permission_Item, User = UserX.Information(user, is_Show_User_Menu, is_Show_User_Relation), Version = Configuration.VERSION };
                return Json(result, JsonRequestBehavior.AllowGet);
            }

            try
            {
                Model_Loading_Plan item = null;
                List<Model_Loading_Plan__Detail_Barge> detail_Barge_List = new List<Model_Loading_Plan__Detail_Barge>();
                Model_Loading_Plan__Detail_Barge detail_Barge = null;

                List<Model_Tunnel_Quality> blendingFormula_List = new List<Model_Tunnel_Quality>();
                Model_Tunnel_Quality blendingFormula = null;

                List<Model_Id_Text> products = new List<Model_Id_Text>();
                Model_Id_Text product = null;


                using (MERCY_Ctx db = new MERCY_Ctx())
                {

                    using (var connection = new SqlConnection(db.Database.Connection.ConnectionString))
                    {
                        connection.Open();

                        SqlCommand command = connection.CreateCommand();
                        SqlCommand command_Detail = connection.CreateCommand();
                        SqlCommand command_Quality_Plan = connection.CreateCommand();
                        SqlCommand command_Blending_Formula = connection.CreateCommand();

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
                                                    , p.ETA as ETA_Vessel_MBR
                                                    , p.CreatedOn
                                                from Loading_Plan p
                                                    , Site s
                                                    , Company c
                                                where p.RecordId = {0} and p.SiteId = s.SiteId
                                                    and p.Company = c.CompanyCode
                                                    ", id);

                        command.CommandText = sql;

                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                item = new Model_Loading_Plan
                                {
                                    RecordId = OurUtility.ToInt64(OurUtility.ValueOf(reader, "RecordId")),
                                    Trip_No = OurUtility.ValueOf(reader, "Trip_No"),
                                    SiteId = OurUtility.ToInt32(OurUtility.ValueOf(reader, "SiteId")),
                                    Site_Str = OurUtility.ValueOf(reader, "Site_Str"),
                                    Company = OurUtility.ValueOf(reader, "Company"),
                                    Shipment_Type = OurUtility.ValueOf(reader, "Shipment_Type"),
                                    Buyer = OurUtility.ValueOf(reader, "Buyer"),
                                    Vessel = OurUtility.ValueOf(reader, "Vessel"),
                                    Destination = OurUtility.ValueOf(reader, "Destination"),
                                    Remark = OurUtility.ValueOf(reader, "Remark"),
                                    ETA_Vessel_MBR = OurUtility.ToDateTime(OurUtility.ValueOf(reader, "ETA_Vessel_MBR")).Date.ToString("dd-MM-yyyy"),

                                    CreatedOn = OurUtility.ToDateTime(OurUtility.ValueOf(reader, "CreatedOn"))
                                };
                                item.CreatedOn_Str = OurUtility.DateFormat(item.CreatedOn, @"dd-MMM-yyyy HH:mm:ss");
                                item.CreatedOn_Str2 = OurUtility.DateFormat(item.CreatedOn, "dd-MMM-yyyy");

                                sql = string.Format(@"
                                                select RecordId, Tug, Barge, Trip_ID, Convert(varchar(20), EstimateStartLoading, 120) as EstimateStartLoading
                                                        , Quantity, Product
                                                from Loading_Plan_Detail_Barge
                                                where Loading_Plan_Id = {0}
                                                    ", item.RecordId);

                                command_Detail.CommandText = sql;

                                using (SqlDataReader reader_Detail = command_Detail.ExecuteReader())
                                {
                                    while (reader_Detail.Read())
                                    {
                                        detail_Barge = new Model_Loading_Plan__Detail_Barge
                                        {
                                            RecordId = OurUtility.ValueOf(reader_Detail, "RecordId"),
                                            Tug = OurUtility.ValueOf(reader_Detail, "Tug"),
                                            Barge = OurUtility.ValueOf(reader_Detail, "Barge"),
                                            TripID = OurUtility.ValueOf(reader_Detail, "Trip_ID"),
                                            EstimateStartLoading = OurUtility.ValueOf(reader_Detail, "EstimateStartLoading")
                                        };
                                        detail_Barge.EstimateStartLoading_D = OurUtility.ToDateTime(detail_Barge.EstimateStartLoading);
                                        detail_Barge.EstimateStartLoading_Str = OurUtility.DateFormat(detail_Barge.EstimateStartLoading_D, "dd-MMM-yyyy");
                                        detail_Barge.Product = OurUtility.ValueOf(reader_Detail, "Product");
                                        detail_Barge.Capacity = OurUtility.ValueOf(reader_Detail, "Quantity");
                                        detail_Barge.CV_ADB_plan = "0";
                                        detail_Barge.CV_ADB_actual = "0";
                                        detail_Barge.TS_plan = "0.00";
                                        detail_Barge.TS_actual = "0.00";
                                        detail_Barge.ASH_plan = "0.00";
                                        detail_Barge.ASH_actual = "0.00";
                                        detail_Barge.M_plan = "0.00";
                                        detail_Barge.M_actual = "0.00";
                                        detail_Barge.TM_plan = "0.00";
                                        detail_Barge.TM_actual = "0.00";

                                        try
                                        {
                                            sql = string.Format(@"
                                                                select top 1 RecordId
                                                                      ,Loading_Plan_Id
                                                                      ,Detail_Barge_Id
                                                                      ,TM_plan
                                                                      ,TM_actual
                                                                      ,M_plan
                                                                      ,M_actual
                                                                      ,ASH_plan
                                                                      ,ASH_actual
                                                                      ,TS_plan
                                                                      ,TS_actual
                                                                      ,CV_ADB_plan
                                                                      ,CV_ADB_actual
                                                                      ,CV_AR_plan
                                                                      ,CV_AR_actual
                                                                from Loading_Plan_Detail_Barge_Quality
                                                                where Loading_Plan_Id = {0}
                                                                      and Detail_Barge_Id = {1}
                                                                order by RecordId desc
                                                                ", item.RecordId, detail_Barge.RecordId);

                                            command_Quality_Plan.CommandText = sql;

                                            using (SqlDataReader reader_Quality_Plan = command_Quality_Plan.ExecuteReader())
                                            {
                                                if (reader_Quality_Plan.Read())
                                                {
                                                    detail_Barge.CV_ADB_plan = OurUtility.ValueOf(reader_Quality_Plan, "CV_ADB_plan");
                                                    detail_Barge.CV_ADB_actual = OurUtility.ValueOf(reader_Quality_Plan, "CV_ADB_actual");
                                                    detail_Barge.TS_plan = OurUtility.ValueOf(reader_Quality_Plan, "TS_plan");
                                                    detail_Barge.TS_actual = OurUtility.ValueOf(reader_Quality_Plan, "TS_actual");
                                                    detail_Barge.ASH_plan = OurUtility.ValueOf(reader_Quality_Plan, "ASH_plan");
                                                    detail_Barge.ASH_actual = OurUtility.ValueOf(reader_Quality_Plan, "ASH_actual");
                                                    detail_Barge.M_plan = OurUtility.ValueOf(reader_Quality_Plan, "M_plan");
                                                    detail_Barge.M_actual = OurUtility.ValueOf(reader_Quality_Plan, "M_actual");
                                                    detail_Barge.TM_plan = OurUtility.ValueOf(reader_Quality_Plan, "TM_plan");
                                                    detail_Barge.TM_actual = OurUtility.ValueOf(reader_Quality_Plan, "TM_actual");
                                                }
                                            }
                                        }
                                        catch { }

                                        detail_Barge_List.Add(detail_Barge);
                                    }
                                }

                                sql = string.Format(@"
                                                    select TunnelID
                                                          ,Tunnel
                                                          ,Stock
                                                          ,CV
                                                          ,TS
                                                          ,ASH
                                                          ,IM
                                                          ,TM
                                                          ,Portion
                                                          ,Detail_Barge_Id
                                                    from Loading_Plan_Detail_Barge_Blending_Formula
                                                    where Loading_Plan_Id = {0}
                                                    order by Detail_Barge_Id, Tunnel
                                                    ", item.RecordId);

                                command_Blending_Formula.CommandText = sql;

                                using (SqlDataReader reader_BlendingFormula = command_Blending_Formula.ExecuteReader())
                                {
                                    while (reader_BlendingFormula.Read())
                                    {
                                        blendingFormula = new Model_Tunnel_Quality
                                        {
                                            TunnelID = OurUtility.ValueOf(reader_BlendingFormula, "TunnelID"),
                                            Company_Code = "",
                                            LatestDate = "",
                                            DateToday = "",
                                            Tunnel = OurUtility.ValueOf(reader_BlendingFormula, "Tunnel"),
                                            Ton = OurUtility.ValueOf(reader_BlendingFormula, "Stock"),
                                            Portion = OurUtility.ValueOf(reader_BlendingFormula, "Portion"),
                                            CV = OurUtility.ValueOf(reader_BlendingFormula, "CV"),
                                            TS = OurUtility.ValueOf(reader_BlendingFormula, "TS"),
                                            ASH = OurUtility.ValueOf(reader_BlendingFormula, "ASH"),
                                            IM = OurUtility.ValueOf(reader_BlendingFormula, "IM"),
                                            TM = OurUtility.ValueOf(reader_BlendingFormula, "TM")
                                        };

                                        blendingFormula.CV_Str = string.Format("{0:N0}", OurUtility.Round(blendingFormula.CV, 0));
                                        blendingFormula.TS_Str = string.Format("{0:N2}", OurUtility.Round(blendingFormula.TS, 2));
                                        blendingFormula.ASH_Str = string.Format("{0:N2}", OurUtility.Round(blendingFormula.ASH, 2));
                                        blendingFormula.IM_Str = string.Format("{0:N2}", OurUtility.Round(blendingFormula.IM, 2));
                                        blendingFormula.TM_Str = string.Format("{0:N2}", OurUtility.Round(blendingFormula.TM, 2));

                                        blendingFormula.Detail_Barge_Id = OurUtility.ValueOf(reader_BlendingFormula, "Detail_Barge_Id");

                                        blendingFormula_List.Add(blendingFormula);
                                    }
                                }
                                sql = string.Format(@"
                                                select ProductName
                                                from Product
                                                where CompanyCode = '{0}'
                                                order by ProductName
                                                    ", item.Company);

                                command_Detail.CommandText = sql;

                                using (SqlDataReader reader_Detail = command_Detail.ExecuteReader())
                                {
                                    while (reader_Detail.Read())
                                    {
                                        product = new Model_Id_Text
                                        {
                                            id = OurUtility.ValueOf(reader_Detail, "ProductName"),
                                            text = OurUtility.ValueOf(reader_Detail, "ProductName")
                                        };

                                        products.Add(product);
                                    }
                                }
                            }
                        }

                        connection.Close();
                    }
                }

                var result = new { Success = true, Permission = permission_Item, User = UserX.Information(user, is_Show_User_Menu, is_Show_User_Relation), Message = string.Empty, MessageDetail = string.Empty, Version = Configuration.VERSION, Item = item, Detail_Barge_List = detail_Barge_List, Products = products, BlendingFormula = blendingFormula_List };
                return Json(result, JsonRequestBehavior.AllowGet);
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
                    bool isDataValid = true;

                    var dataQuery =
                        
                            from d in db.Loading_Plan
                            select d
                        ;

                    isDataValid = IsDataValid(OurUtility.ValueOf(p_collection, "Shipment_Type"), OurUtility.ValueOf(p_collection, "tripNo"), OurUtility.ValueOf(p_collection, "buyer"), dataQuery.ToList());

                    var data = dataQuery.Where(x => x.RecordId == id).SingleOrDefault();

                    string site = OurUtility.ValueOf(p_collection, "site");
                    data.SiteId = db.Sites.Where(x => x.SiteName == site).FirstOrDefault().SiteId;
                    data.Company = OurUtility.ValueOf(p_collection, "company");
                    data.Shipment_Type = OurUtility.ValueOf(p_collection, "Shipment_Type");
                    data.Buyer = OurUtility.ValueOf(p_collection, "buyer");
                    data.Vessel = OurUtility.ValueOf(p_collection, "vessel");
                    if (OurUtility.ValueOf(p_collection, "ETA") != string.Empty)
                    {
                        data.ETA = OurUtility.ToDateTime(OurUtility.ValueOf(p_collection, "ETA"));
                    }
                    data.Destination = OurUtility.ValueOf(p_collection, "destination");
                    data.Trip_No = OurUtility.ValueOf(p_collection, "tripNo");
                    data.Remark = OurUtility.ValueOf(p_collection, "remark");

                    data.LastModifiedOn = DateTime.Now;
                    data.LastModifiedBy = user.UserId;

                    var datas = db.Loading_Plan_Detail_Barge.Where(x => x.Loading_Plan_Id == id).ToList();
                    var qualityDatas = db.Loading_Plan_Detail_Barge_Quality.ToList();
                    var blendingFormulaDatas = db.Loading_Plan_Detail_Barge_Blending_Formula.ToList();
                    foreach (var item in datas)
                    {
                        var qualityList = qualityDatas.Where(x => x.Loading_Plan_Id == id && x.Detail_Barge_Id == item.RecordId).ToList();
                        var blendingFormulaList = blendingFormulaDatas.Where(x => x.Loading_Plan_Id == id && x.Detail_Barge_Id == item.RecordId).ToList();

                        db.Loading_Plan_Detail_Barge.Remove(item);
                        db.Loading_Plan_Detail_Barge_Quality.RemoveRange(qualityList);
                        db.Loading_Plan_Detail_Barge_Blending_Formula.RemoveRange(blendingFormulaList);
                        db.SaveChanges();
                    }

                    var message = Create_Detail_Barge(p_collection, db, id, user, data.Shipment_Type, data.Vessel, data.Buyer);
                    if (message == "Success")
                    {
                        var result = new { Success = true, Permission = permission_Item, Message = BaseConstants.MESSAGE_CREATE_SUCCESS, MessageDetail = string.Empty, Version = Configuration.VERSION, Data = id };
                        return Json(result, JsonRequestBehavior.AllowGet);
                    }
                    else
                    {
                        var validationResult = new { Success = false, Permission = permission_Item, Message = message, MessageDetail = string.Empty };
                        return Json(validationResult, JsonRequestBehavior.AllowGet);
                    }
                }
            }
            catch (Exception ex)
            {
                var result = new { Success = false, Permission = permission_Item, ex.Message, MessageDetail = string.Empty, Version = Configuration.VERSION, Items = string.Empty, Total = 0 };
                return Json(result, JsonRequestBehavior.AllowGet);
            }
        }

        public JsonResult DetailBargeList()
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
                string tripNo = string.Empty;
                string vessel = OurUtility.ValueOf(Request, "vessel");
                string buyer = OurUtility.ValueOf(Request, "buyer");

                List<Model_Loading_Plan__Detail_Barge> detail_Barge_List = new List<Model_Loading_Plan__Detail_Barge>();
                Model_Loading_Plan__Detail_Barge detail_Barge = null;

                using (MERCY_Ctx db = new MERCY_Ctx())
                {
                    using (var connection = new SqlConnection(db.Database.Connection.ConnectionString))
                    {
                        connection.Open();

                        SqlCommand command = connection.CreateCommand();
                        SqlCommand command2 = connection.CreateCommand();

                        string sql = string.Format(@"
                                                    select TripNo, Vessel
                                                    from UPLOAD_Barge_Quality_Plan_Header
                                                    -- latest uploaded
                                                    where CreatedOn_Date_Only = (select Max(CreatedOn_Date_Only) from UPLOAD_Barge_Quality_Plan_Header)
                                                        and Buyer = '{0}' AND (Vessel = '{1}' OR Vessel = 'TBN')
                                                    order by TripNo
                                                    ", buyer, vessel);
                        command.CommandText = sql;

                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            bool isMatchVessel = false;
                            if (reader.Read())
                            {
                                var vesselName = OurUtility.ValueOf(reader, "Vessel");

                                if (!isMatchVessel || vesselName == vessel)
                                {
                                    if (vesselName == vessel)
                                    {
                                        isMatchVessel = true;
                                    }

                                    tripNo = OurUtility.ValueOf(reader, "TripNo");
                                }
                            }
                        }

                        if (OurUtility.ValueOf(Request, "shipmentType") == OurUtility.Shipment_Type_Vessel)
                        {
                            sql = string.Format(@"
                                                    select RecordId
                                                        ,TEMPORARY
                                                        ,Site
                                                        ,Sheet
                                                        ,Convert(varchar(10), EstimateStartLoading, 120) EstimateStartLoading
                                                        ,Port_of_Loading
                                                        ,VesselName
                                                        ,TripID
                                                        ,TugBoat
                                                        ,Barge
                                                        ,Product
                                                        ,Capacity
                                                        ,FinalDestinantion
                                                        ,Remark
                                                        ,CreatedBy
                                                        ,CreatedOn
                                                        ,CreatedOn_Date_Only
                                                        ,LastModifiedBy
                                                        ,LastModifiedOn
                                                    from UPLOAD_Barge_Line_Up
                                                    -- latest uploaded
                                                    where CreatedOn_Date_Only = (select Max(CreatedOn_Date_Only) from UPLOAD_Barge_Line_Up)
                                                    and VesselName = '{0}'
                                                    order by RecordId
                                                    ", vessel);
                            command.CommandText = sql;

                            using (SqlDataReader reader = command.ExecuteReader())
                            {
                                while (reader.Read())
                                {
                                    detail_Barge = new Model_Loading_Plan__Detail_Barge
                                    {
                                        RecordId = OurUtility.ValueOf(reader, "RecordId"),
                                        Tug = OurUtility.ValueOf(reader, "TugBoat"),
                                        Barge = OurUtility.ValueOf(reader, "Barge"),
                                        TripID = OurUtility.ValueOf(reader, "TripID"),
                                        EstimateStartLoading = OurUtility.ValueOf(reader, "EstimateStartLoading")
                                    };
                                    detail_Barge.EstimateStartLoading_D = OurUtility.ToDateTime(detail_Barge.EstimateStartLoading);
                                    detail_Barge.EstimateStartLoading_Str = OurUtility.DateFormat(detail_Barge.EstimateStartLoading_D, "dd-MMM-yyyy");
                                    detail_Barge.Product = OurUtility.ValueOf(reader, "Product");
                                    detail_Barge.Capacity = OurUtility.ValueOf(reader, "Capacity");
                                    detail_Barge.CV_ADB_plan = "0";
                                    detail_Barge.CV_ADB_actual = "0";
                                    detail_Barge.TS_plan = "0.00";
                                    detail_Barge.TS_actual = "0.00";
                                    detail_Barge.ASH_plan = "0.00";
                                    detail_Barge.ASH_actual = "0.00";
                                    detail_Barge.M_plan = "0.00";
                                    detail_Barge.M_actual = "0.00";
                                    detail_Barge.TM_plan = "0.00";
                                    detail_Barge.TM_actual = "0.00";

                                    sql = string.Format(@"
                                                        select Detail.*
                                                        from 
                                                        (
	                                                        select RecordId, Sheet, TugName, BargeName, Product, CV_ADB_plan, CV_ADB_actual, TS_plan, TS_actual,
                                                                ASH_plan, ASH_actual, M_plan, M_actual, TM_plan, TM_actual
	                                                        from UPLOAD_Barge_Quality_Plan
	                                                        -- latest uploaded
	                                                        where CreatedOn_Date_Only = (select Max(CreatedOn_Date_Only) from UPLOAD_Barge_Quality_Plan_Header)
                                                        ) Detail
                                                        where Sheet in
                                                        (
	                                                        select Sheet
	                                                        from UPLOAD_Barge_Quality_Plan_Header
	                                                        -- latest uploaded
	                                                        where CreatedOn_Date_Only = (select Max(CreatedOn_Date_Only) from UPLOAD_Barge_Quality_Plan_Header)
		                                                        and Buyer = '{0}' AND (Vessel = '{1}' OR Vessel = 'TBN')
                                                        )
                                                        and Product = '{2}'
                                                        order by RecordId", buyer, vessel, detail_Barge.Product);

                                    command2.CommandText = sql;
                                    using (SqlDataReader reader2 = command2.ExecuteReader())
                                    {
                                        if (reader2.Read())
                                        {
                                            var tugName = OurUtility.ValueOf(reader2, "TugName");
                                            var bargeName = OurUtility.ValueOf(reader2, "BargeName");
                                            if (IsQualityPlanUpdated(detail_Barge) || (tugName == detail_Barge.Tug && bargeName == detail_Barge.Barge))
                                            {
                                                detail_Barge.CV_ADB_plan = string.Format("{0:N0}", OurUtility.Round(OurUtility.ValueOf(reader2, "CV_ADB_plan"), 0));
                                                detail_Barge.CV_ADB_actual = string.Format("{0:N0}", OurUtility.Round(OurUtility.ValueOf(reader2, "CV_ADB_actual"), 0));
                                                detail_Barge.TS_plan = OurUtility.ValueOf(reader2, "TS_plan");
                                                detail_Barge.TS_actual = OurUtility.ValueOf(reader2, "TS_actual");
                                                detail_Barge.ASH_plan = OurUtility.ValueOf(reader2, "ASH_plan");
                                                detail_Barge.ASH_actual = OurUtility.ValueOf(reader2, "ASH_actual");
                                                detail_Barge.M_plan = OurUtility.ValueOf(reader2, "M_plan");
                                                detail_Barge.M_actual = OurUtility.ValueOf(reader2, "M_actual");
                                                detail_Barge.TM_plan = OurUtility.ValueOf(reader2, "TM_plan");
                                                detail_Barge.TM_actual = OurUtility.ValueOf(reader2, "TM_actual");
                                            }
                                        }
                                    }

                                    detail_Barge_List.Add(detail_Barge);
                                }
                            }
                        }

                        connection.Close();
                    }
                }

                var result = new { Success = true, Permission = permission_Item, Message = string.Empty, MessageDetail = string.Empty, Version = Configuration.VERSION, TripNo = tripNo, Detail_Barge_List = detail_Barge_List };
                return Json(result, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                var result = new { Success = false, Permission = permission_Item, ex.Message, MessageDetail = string.Empty, Version = Configuration.VERSION, Items = string.Empty, Total = 0 };
                return Json(result, JsonRequestBehavior.AllowGet);
            }
        }

        public JsonResult Buyer()
        {
            // Check Permission based on Current Url
            UserX user = new UserX(Request);
            Permission_Item permission_Item = null;

            // -- Not necessary checking Permission
            //Permission.Check_API(Request, user, ref permission_Item);
            // -- just Logging User: is enough
            if (user.UserId <= 0)
            {
                var msg = new { Success = false, Permission = permission_Item, Message = Permission.ERROR_PERMISSION_READ + "[not Login]", MessageDetail = string.Empty, Version = Configuration.VERSION };
                return Json(msg, JsonRequestBehavior.AllowGet);
            }

            // -- Actual code
            try
            {
                List<Model_Loading_Plan__Buyer> buyers = new List<Model_Loading_Plan__Buyer>();
                Model_Loading_Plan__Buyer buyer = null;

                using (MERCY_Ctx db = new MERCY_Ctx())
                {
                    using (var connection = new SqlConnection(db.Database.Connection.ConnectionString))
                    {
                        connection.Open();

                        SqlCommand command = connection.CreateCommand();

                        string sql = string.Empty;

                        switch (OurUtility.ValueOf(Request, "shipmentType"))
                        {
                            case OurUtility.Shipment_Type_Vessel:
                                sql = string.Format(@"
                                                    select distinct BUYER as BuyerName
                                                    from UPLOAD_VLU
                                                    -- latest uploaded
                                                    where CreatedOn_Date_Only = (select Max(CreatedOn_Date_Only) from UPLOAD_VLU)
                                                    order by BUYER
                                                    ");
                                break;
                            case OurUtility.Shipment_Type_Direct:
                                sql = string.Format(@"
                                                    select distinct BuyerName
                                                    from DirectShipment
                                                    order by BuyerName 
                                                    ");
                                break;
                        }

                        command.CommandText = sql;

                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                buyer = new Model_Loading_Plan__Buyer
                                {
                                    BuyerName = OurUtility.ValueOf(reader, "BuyerName")
                                };
                                buyers.Add(buyer);
                            }
                        }

                        connection.Close();
                    }
                }

                var result = new { Success = true, Permission = permission_Item, Message = string.Empty, MessageDetail = string.Empty, Version = Configuration.VERSION, Items = buyers, Total = buyers.Count };
                return Json(result, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                var result = new { Success = false, Permission = permission_Item, ex.Message, MessageDetail = string.Empty, Version = Configuration.VERSION, Items = string.Empty, Total = 0 };
                return Json(result, JsonRequestBehavior.AllowGet);
            }
        }

        public JsonResult Vessel()
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
                List<VesselDetail> vessels = new List<VesselDetail>();

                using (MERCY_Ctx db = new MERCY_Ctx())
                {
                    using (var connection = new SqlConnection(db.Database.Connection.ConnectionString))
                    {
                        connection.Open();

                        SqlCommand command = connection.CreateCommand();

                        string sql = string.Empty;

                        switch (OurUtility.ValueOf(Request, "shipmentType"))
                        {
                            case OurUtility.Shipment_Type_Vessel:
                                sql = string.Format(@"
                                                    select distinct VESSEL, ETA
                                                    from UPLOAD_VLU
                                                    -- latest uploaded
                                                    where CreatedOn_Date_Only = (select Max(CreatedOn_Date_Only) from UPLOAD_VLU)
                                                    AND (BUYER = '{0}' OR '{0}' = '')
                                                    order by VESSEL
                                                    ", OurUtility.ValueOf(Request, "buyer"));
                                break;
                            case OurUtility.Shipment_Type_Direct:
                                break;
                        }

                        if (!string.IsNullOrEmpty(sql))
                        {
                            command.CommandText = sql;

                            using (SqlDataReader reader = command.ExecuteReader())
                            {
                                while (reader.Read())
                                {
                                    var vessel = new VesselDetail
                                    {
                                        VesselName = OurUtility.ValueOf(reader, "VESSEL"),
                                        ETA = OurUtility.ToDateTime(OurUtility.ValueOf(reader, "ETA")).Date.ToString("dd-MM-yyyy")
                                    };
                                    vessels.Add(vessel);
                                }
                            }
                        }

                        connection.Close();
                    }
                }

                var result = new { Success = true, Permission = permission_Item, Message = string.Empty, MessageDetail = string.Empty, Version = Configuration.VERSION, Items = vessels, Total = vessels.Count };
                return Json(result, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                var result = new { Success = false, Permission = permission_Item, ex.Message, MessageDetail = string.Empty, Version = Configuration.VERSION, Items = string.Empty, Total = 0 };
                return Json(result, JsonRequestBehavior.AllowGet);
            }
        }

        public JsonResult Destination()
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
                List<string> destinations = new List<string>();

                using (MERCY_Ctx db = new MERCY_Ctx())
                {
                    using (var connection = new SqlConnection(db.Database.Connection.ConnectionString))
                    {
                        connection.Open();

                        SqlCommand command = connection.CreateCommand();

                        string sql = string.Empty;

                        switch (OurUtility.ValueOf(Request, "shipmentType"))
                        {
                            case OurUtility.Shipment_Type_Vessel:
                                sql = string.Format(@"
                                                    select distinct DESTINATION
                                                    from UPLOAD_VLU
                                                    -- latest uploaded
                                                    where CreatedOn_Date_Only = (select Max(CreatedOn_Date_Only) from UPLOAD_VLU)
                                                    AND (BUYER = '{0}' OR '{0}' = '')
                                                    order by DESTINATION
                                                    ", OurUtility.ValueOf(Request, "buyer"));
                                break;
                            case OurUtility.Shipment_Type_Direct:
                                sql = string.Format(@"
                                                    select distinct Destination
                                                    from DirectShipment
                                                    where BuyerName = '{0}' 
                                                    ", OurUtility.ValueOf(Request, "buyer"));
                                break;
                        }

                        command.CommandText = sql;

                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                switch (OurUtility.ValueOf(Request, "shipmentType"))
                                {
                                    case OurUtility.Shipment_Type_Vessel:
                                        destinations.Add(OurUtility.ValueOf(reader, "DESTINATION"));
                                        break;
                                    case OurUtility.Shipment_Type_Direct:
                                        string[] xs = OurUtility.ValueOf(reader, "Destination").Split(',');
                                        foreach (string x in xs)
                                        {
                                            if (!destinations.Contains(x))
                                            {
                                                destinations.Add(x);
                                            }
                                        }
                                        break;
                                }
                            }
                        }

                        destinations.Sort();

                        connection.Close();
                    }
                }

                var result = new { Success = true, Permission = permission_Item, Message = string.Empty, MessageDetail = string.Empty, Version = Configuration.VERSION, Items = destinations, Total = destinations.Count };
                return Json(result, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                var result = new { Success = false, Permission = permission_Item, ex.Message, MessageDetail = string.Empty, Version = Configuration.VERSION, Items = string.Empty, Total = 0 };
                return Json(result, JsonRequestBehavior.AllowGet);
            }
        }

        public JsonResult Buyer_Direct()
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
                            
                                from d in db.DirectShipments
                                orderby d.BuyerName
                                select new Model_View_DirectShipment
                                {
                                    RecordId = d.RecordId,
                                    BuyerName = d.BuyerName,
                                    Destination = d.Destination,
                                    CV = d.CV,
                                    TS = d.TS,
                                    ASH = d.ASH,
                                    IM = d.IM,
                                    TM = d.TM
                                }
                            ;

                    var items = dataQuery.ToList();

                    items.ForEach(c =>
                    {
                        c.CV_Str = string.Format("{0:N0}", OurUtility.Round(c.CV, 0));
                        c.TS_Str = string.Format("{0:N2}", OurUtility.Round(c.TS, 2));
                        c.ASH_Str = string.Format("{0:N2}", OurUtility.Round(c.ASH, 2));
                        c.IM_Str = string.Format("{0:N2}", OurUtility.Round(c.IM, 2));
                        c.TM_Str = string.Format("{0:N2}", OurUtility.Round(c.TM, 2));
                    });

                    var result = new { Success = true, Permission = permission_Item, Message = string.Empty, MessageDetail = string.Empty, Version = Configuration.VERSION, Items = items };
                    return Json(result, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                var result = new { Success = false, Permission = permission_Item, ex.Message, MessageDetail = string.Empty, Version = Configuration.VERSION, Items = string.Empty, Total = 0 };
                return Json(result, JsonRequestBehavior.AllowGet);
            }
        }

        private string Create_Detail_Barge(FormCollection p_collection, MERCY_Ctx p_db, long p_loadingPlan, UserX p_executedBy, string p_Shipment_Type, string p_vessel, string p_buyer)
        {
            var result = "Success";
            int count = OurUtility.ToInt32(OurUtility.ValueOf(Request, "recordNumber"));

            string sql = string.Empty;
            Dictionary<long, string> quality_Plans = new Dictionary<long, string>();
            Dictionary<long, string> blending_Formulas = new Dictionary<long, string>();
            var transaction = p_db.Database.BeginTransaction();
            try
            {
                p_db.SaveChanges();
                for (int i = 1; i < count; i++)
                {

                    bool isDataValid = true;
                    var refLoadingPlanDetail = p_db.Loading_Plan_Detail_Barge.ToList();
                    string tripId = OurUtility.ValueOf(p_collection, "Trip_ID" + i.ToString());
                    string barge = OurUtility.ValueOf(p_collection, "Barge" + i.ToString());
                    string tug = OurUtility.ValueOf(p_collection, "Tug" + i.ToString());
                    string product = OurUtility.ValueOf(p_collection, "Product" + i.ToString());

                    if (barge == string.Empty)
                    {
                        return "Barge Name cannot be empty, please make sure every data is filled";
                    }
                    else if (tug == string.Empty)
                    {
                        return "Tug Name cannot be empty, please make sure every data is filled";
                    }
                    else if (product == string.Empty)
                    {
                        return "Product cannot be empty, please make sure every data is filled";
                    }

                    switch (p_Shipment_Type)
                    {
                        case OurUtility.Shipment_Type_Vessel:
                            if (refLoadingPlanDetail.Any(x => x.Trip_ID == tripId && x.Loading_Plan_Id == p_loadingPlan) && tripId != "")
                            {
                                isDataValid = false;
                            }
                            break;
                        case OurUtility.Shipment_Type_Direct:
                            if (refLoadingPlanDetail.Any(x => x.Barge == barge && x.Loading_Plan_Id == p_loadingPlan))
                            {
                                isDataValid = false;
                            }
                            break;
                    }

                    if (!isDataValid)
                    {
                        return "Data is not valid (already exist in database)";
                    }

                    var data = new Loading_Plan_Detail_Barge
                    {
                        Loading_Plan_Id = p_loadingPlan,
                        Tug = tug,
                        Barge = barge,
                        Trip_ID = tripId,
                        EstimateStartLoading = OurUtility.ToDateTime(OurUtility.ValueOf(p_collection, "EstimateStartLoading" + i.ToString())),
                        Quantity = OurUtility.ToInt32(OurUtility.ValueOf(p_collection, "Quantity" + i.ToString())),
                        Product = product
                    };
                    if (data.Product == "null") data.Product = string.Empty;

                    data.CreatedOn = DateTime.Now;
                    data.CreatedBy = p_executedBy.UserId;
                    data.CreatedOn_Date_Only = data.CreatedOn.ToString("yyyy-MM-dd");

                    p_db.Loading_Plan_Detail_Barge.Add(data);
                    p_db.SaveChanges();

                    quality_Plans.Add(data.RecordId, data.Product);
                    blending_Formulas.Add(data.RecordId, OurUtility.ValueOf(p_collection, "Value_Tunnel" + i.ToString()) + "#" + OurUtility.ValueOf(p_collection, "Value_Portion" + i.ToString()));
                }
                transaction.Commit();
            }
            catch(Exception ex)
            {
                transaction.Rollback();
                transaction.Dispose();

                return ex.Message;
            }

            List<Model_Tunnel_Quality> tunnel_Quality = Tunnel_Qualityx();

            using (var connection = new SqlConnection(p_db.Database.Connection.ConnectionString))
            {
                connection.Open();

                SqlCommand command = connection.CreateCommand();
                SqlCommand command_Insert = connection.CreateCommand();

                string m_TM_plan = "0.0";
                string m_TM_actual = "0.0";
                string m_M_plan = "0.0";
                string m_M_actual = "0.0";
                string m_ASH_plan = "0.0";
                string m_ASH_actual = "0.0";
                string m_TS_plan = "0.0";
                string m_TS_actual = "0.0";
                string m_CV_ADB_plan = "0.0";
                string m_CV_ADB_actual = "0.0";
                string m_CV_AR_plan = "0.0";
                string m_CV_AR_actual = "0.0";

                foreach (KeyValuePair<long, string> quality_Plan in quality_Plans)
                {
                    try
                    {
                        m_TM_plan = "0.0";
                        m_TM_actual = "0.0";
                        m_M_plan = "0.0";
                        m_M_actual = "0.0";
                        m_ASH_plan = "0.0";
                        m_ASH_actual = "0.0";
                        m_TS_plan = "0.0";
                        m_TS_actual = "0.0";
                        m_CV_ADB_plan = "0.0";
                        m_CV_ADB_actual = "0.0";
                        m_CV_AR_plan = "0.0";
                        m_CV_AR_actual = "0.0";

                        if (p_Shipment_Type.ToUpper() == OurUtility.Shipment_Type_Vessel.ToUpper())
                        {
                            // Vessel Shipment
                            // #191 and #132
                            sql = string.Format(@"
                                                select Detail.*
                                                from 
                                                (
	                                                select RecordId, Sheet, TugName, BargeName, Product, CV_ADB_plan, CV_ADB_actual, TS_plan, TS_actual
                                                            , ASH_plan, ASH_actual, M_plan, M_actual, TM_plan, TM_actual, CV_AR_plan, CV_AR_actual
	                                                from UPLOAD_Barge_Quality_Plan
	                                                -- latest uploaded
	                                                where CreatedOn_Date_Only = (select Max(CreatedOn_Date_Only) from UPLOAD_Barge_Quality_Plan_Header)
                                                ) Detail
                                                where Sheet in
                                                (
	                                                select Sheet
	                                                from UPLOAD_Barge_Quality_Plan_Header
	                                                -- latest uploaded
	                                                where CreatedOn_Date_Only = (select Max(CreatedOn_Date_Only) from UPLOAD_Barge_Quality_Plan_Header)
		                                                and Buyer = '{0}' AND (Vessel = '{1}' OR Vessel = 'TBN')
                                                )
                                                and Product = '{2}'
                                                order by RecordId
                                                ", p_buyer, p_vessel, quality_Plan.Value);

                            command.CommandText = sql;
                            using (SqlDataReader reader = command.ExecuteReader())
                            {
                                if (reader.Read())
                                {
                                    m_TM_plan = OurUtility.ValueOf(reader, "TM_plan").Replace(",",".");
                                    m_TM_actual = OurUtility.ValueOf(reader, "TM_actual").Replace(",", ".");
                                    m_M_plan = OurUtility.ValueOf(reader, "M_plan").Replace(",", ".");
                                    m_M_actual = OurUtility.ValueOf(reader, "M_actual").Replace(",", ".");
                                    m_ASH_plan = OurUtility.ValueOf(reader, "ASH_plan").Replace(",", ".");
                                    m_ASH_actual = OurUtility.ValueOf(reader, "ASH_actual").Replace(",", ".");
                                    m_TS_plan = OurUtility.ValueOf(reader, "TS_plan").Replace(",", ".");
                                    m_TS_actual = OurUtility.ValueOf(reader, "TS_actual").Replace(",", ".");
                                    m_CV_ADB_plan = OurUtility.ValueOf(reader, "CV_ADB_plan").Replace(",", ".");
                                    m_CV_ADB_actual = OurUtility.ValueOf(reader, "CV_ADB_actual").Replace(",", ".");
                                    m_CV_AR_plan = OurUtility.ValueOf(reader, "CV_AR_plan").Replace(",", ".");
                                    m_CV_AR_actual = OurUtility.ValueOf(reader, "CV_AR_actual").Replace(",", ".");
                                }
                            }
                        }
                        else
                        {
                            // Direct Shipment
                            // #193 and #161
                            sql = string.Format(@"
                                                select top 1 CV, TS, ASH, IM, TM from DirectShipment
                                                where BuyerName = '{0}'
                                                order by RecordId desc
                                                ", p_buyer);

                            command.CommandText = sql;
                            using (SqlDataReader reader = command.ExecuteReader())
                            {
                                if (reader.Read())
                                {
                                    m_TM_plan = OurUtility.ValueOf(reader, "TM").Replace(",", ".");
                                    m_M_plan = OurUtility.ValueOf(reader, "IM").Replace(",", ".");
                                    m_ASH_plan = OurUtility.ValueOf(reader, "ASH").Replace(",",".");
                                    m_TS_plan = OurUtility.ValueOf(reader, "TS").Replace(",", ".");
                                    m_CV_ADB_plan = OurUtility.ValueOf(reader, "CV").Replace(",", ".");
                                    m_CV_AR_plan = OurUtility.ValueOf(reader, "CV").Replace(",", ".");
                                }
                            }
                        }

                        command_Insert.CommandText = string.Format(@"insert into Loading_Plan_Detail_Barge_Quality(Loading_Plan_Id, Detail_Barge_Id
                                                                            , TM_plan, TM_actual, M_plan, M_actual, ASH_plan, ASH_actual, TS_plan, TS_actual
                                                                            ,  CV_ADB_plan, CV_ADB_actual, CV_AR_plan, CV_AR_actual)
                                                                      values({0}, {1}, {2}, {3}, {4}, {5}, {6}, {7}, {8}, {9}, {10}, {11}, {12}, {13})"
                                                                       , p_loadingPlan, quality_Plan.Key
                                                                       , m_TM_plan, m_TM_actual, m_M_plan, m_M_actual, m_ASH_plan, m_ASH_actual, m_TS_plan, m_TS_actual
                                                                            , m_CV_ADB_plan, m_CV_ADB_actual, m_CV_AR_plan, m_CV_AR_actual);

                        command_Insert.ExecuteNonQuery();
                    }
                    catch(Exception ex) {
                        return ex.Message;
                    }
                }

                foreach (KeyValuePair<long, string> blending_Formula in blending_Formulas)
                {
                    try
                    {
                        string[] temp = blending_Formula.Value.Split('#');

                        string[] value_Tunnels = temp[0].Split(',');
                        string[] value_Portions = temp[1].Split(',');
                        int length = value_Tunnels.Length;

                        for (int i = 0; i < length; i++)
                        {
                            if (string.IsNullOrEmpty(value_Tunnels[i])) continue;

                            //tunnel_Quality
                            var dataQuery =
                            
                                from d in tunnel_Quality
                                where d.TunnelID == value_Tunnels[i]
                                select d
                            ;

                            var tunnels = dataQuery.ToList();

                            foreach (var tunnel in tunnels)
                            {
                                command_Insert.CommandText = string.Format(@"insert into Loading_Plan_Detail_Barge_Blending_Formula(Loading_Plan_Id, Detail_Barge_Id
                                                                            , TunnelID, Tunnel, Stock, CV, TS, ASH, IM, TM, Portion, CreatedBy, CreatedOn)
                                                                      values({0}, {1}, {2}, '{3}', {4}, {5}, {6}, {7}, {8}, {9}, {10}, {11}, GetDate())"
                                                                       , p_loadingPlan, blending_Formula.Key
                                                                       , tunnel.TunnelID, tunnel.Tunnel, tunnel.Ton.Replace(",", "."), tunnel.CV.Replace(",", "."), tunnel.TS.Replace(",", ".")
                                                                       , tunnel.ASH.Replace(",", "."), tunnel.IM.Replace(",", "."), tunnel.TM.Replace(",", "."), value_Portions[i], p_executedBy.UserId);

                                command_Insert.ExecuteNonQuery();
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        return ex.Message;
                    }
                }

                connection.Close();
            }

            return result;
        }

        public JsonResult BlendingFormula()
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

            var result = new { Success = true, Permission = permission_Item, Message = string.Empty, MessageDetail = string.Empty, Version = Configuration.VERSION, Items = Tunnel_Qualityx() };
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        private List<Model_Tunnel_Quality> Tunnel_Qualityx()
        {
            List<Model_Tunnel_Quality> tunnel_Quality_List = new List<Model_Tunnel_Quality>();

            // -- Actual code
            try
            {
                Model_Tunnel_Quality tunnel_Quality = null;

                using (MERCY_Ctx db = new MERCY_Ctx())
                {
                    using (var connection = new SqlConnection(db.Database.Connection.ConnectionString))
                    {
                        connection.Open();

                        SqlCommand command = connection.CreateCommand();

                        string sql = string.Format(@"
                                                        WITH newest_data AS (
                                                            SELECT t.TunnelId, p.*, ROW_NUMBER() OVER (PARTITION BY Tunnel ORDER BY p.CreatedOn DESC) AS nd
                                                            FROM PortionBlending AS p
                                                            INNER JOIN Tunnel AS t on t.Name = p.Tunnel
                                                            WHERE t.IsActive = 1
                                                        )
                                                        SELECT * FROM newest_data WHERE nd = 1
                                                        order by Company, Tunnel;
                                                    ");
                        command.CommandText = sql;

                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                tunnel_Quality = new Model_Tunnel_Quality
                                {
                                    TunnelID = OurUtility.ValueOf(reader, "TunnelID"),
                                    Company_Code = OurUtility.ValueOf(reader, "Company"),
                                    LatestDate = OurUtility.ValueOf(reader, "CreatedOn"),
                                    DateToday = DateTime.Now.ToString("dd/MM/yyyy hh:mm:ss"),
                                    Tunnel = OurUtility.ValueOf(reader, "Tunnel"),
                                    Ton = OurUtility.ValueOf(reader, "Ton"),
                                    Portion = "0",
                                    CV = OurUtility.ValueOf(reader, "CV"),
                                    TS = OurUtility.ValueOf(reader, "TS"),
                                    ASH = OurUtility.ValueOf(reader, "ASH"),
                                    IM = OurUtility.ValueOf(reader, "IM"),
                                    TM = OurUtility.ValueOf(reader, "TM")
                                };

                                tunnel_Quality.CV_Str = string.Format("{0:N0}", OurUtility.Round(tunnel_Quality.CV, 0));
                                tunnel_Quality.TS_Str = string.Format("{0:N2}", OurUtility.Round(tunnel_Quality.TS, 2));
                                tunnel_Quality.ASH_Str = string.Format("{0:N2}", OurUtility.Round(tunnel_Quality.ASH, 2));
                                tunnel_Quality.IM_Str = string.Format("{0:N2}", OurUtility.Round(tunnel_Quality.IM, 2));
                                tunnel_Quality.TM_Str = string.Format("{0:N2}", OurUtility.Round(tunnel_Quality.TM, 2));

                                tunnel_Quality_List.Add(tunnel_Quality);
                            }
                        }

                        connection.Close();
                    }
                }
            }
            catch { }

            return tunnel_Quality_List;
        }

        private bool IsQualityPlanUpdated(Model_Loading_Plan__Detail_Barge detailBarge)
        {
            return detailBarge.CV_ADB_plan == "0"
                && detailBarge.CV_ADB_actual == "0"
                && detailBarge.TS_plan == "0.00"
                && detailBarge.TS_actual == "0.00"
                && detailBarge.ASH_plan == "0.00"
                && detailBarge.ASH_actual == "0.00"
                && detailBarge.M_plan == "0.00"
                && detailBarge.M_actual == "0.00"
                && detailBarge.TM_plan == "0.00"
                && detailBarge.TM_actual == "0.00";
        }

        private bool IsDataValid(string shipmentType, string tripNo, string buyer, List<Loading_Plan> loadingPlanList)
        {
            switch (shipmentType)
            {
                case OurUtility.Shipment_Type_Vessel:
                    if (loadingPlanList.Any(x => x.Trip_No == tripNo) && tripNo != "")
                    {
                        return false;
                    }
                    break;
                case OurUtility.Shipment_Type_Direct:
                    if (loadingPlanList.Any(x => x.Buyer == buyer && x.CreatedOn_Date_Only == DateTime.Now.Date.ToString("yyyy-MM-dd")))
                    {
                        return false;
                    }
                    break;
            }
            return true;
        }

        public class VesselDetail
        {
            public string VesselName { get; set; }
            public string ETA { get; set; }
        }

        internal static Model_Loading_Plan GetPortionBlending(Model_Loading_Plan item, long loadingPlanId)
        {
            using (MERCY_Ctx db = new MERCY_Ctx())
            {
                var blendingFormulas = db.Loading_Plan_Detail_Barge_Blending_Formula
                                    .Where(bf => bf.Loading_Plan_Id == loadingPlanId)
                                    .GroupBy(bf => bf.Detail_Barge_Id)
                                    .Select(x => new { Id = x.Key, Data = x.Select(bf => new { bf.Tunnel, bf.Portion }) })
                                    .ToList();
                
                foreach(var blendingPortion in blendingFormulas)
                {
                    int[] tn = new int[] { 0, 0, 0, 0, 0, 0 };
                    int[] btn = new int[] { 0, 0, 0, 0, 0, 0 };

                    foreach (var data in blendingPortion.Data)
                    {
                        switch (data.Tunnel)
                        {
                            case "TN1":
                                tn[0] = data.Portion;
                                break;
                            case "TN2":
                                tn[1] = data.Portion;
                                break;
                            case "TN3":
                                tn[2] = data.Portion;
                                break;
                            case "TN4":
                                tn[3] = data.Portion;
                                break;
                            case "TN5":
                                tn[4] = data.Portion;
                                break;
                            case "TN6":
                                tn[5] = data.Portion;
                                break;
                            case "BTN1":
                                btn[0] = data.Portion;
                                break;
                            case "BTN2":
                                btn[1] = data.Portion;
                                break;
                            case "BTN3":
                                btn[2] = data.Portion;
                                break;
                            case "BTN4":
                                btn[3] = data.Portion;
                                break;
                            case "BTN5":
                                btn[4] = data.Portion;
                                break;
                            case "BTN6":
                                btn[5] = data.Portion;
                                break;
                        }
                    }

                    item.TN1_Str += tn[0] == 0 ? @"&nbsp;" : string.Format(@"<div>{0}%</div>", tn[0]);
                    item.TN2_Str += tn[1] == 0 ? @"&nbsp;" : string.Format(@"<div>{0}%</div>", tn[1]);
                    item.TN3_Str += tn[2] == 0 ? @"&nbsp;" : string.Format(@"<div>{0}%</div>", tn[2]);
                    item.TN4_Str += tn[3] == 0 ? @"&nbsp;" : string.Format(@"<div>{0}%</div>", tn[3]);
                    item.TN5_Str += tn[4] == 0 ? @"&nbsp;" : string.Format(@"<div>{0}%</div>", tn[4]);
                    item.TN6_Str += tn[5] == 0 ? @"&nbsp;" : string.Format(@"<div>{0}%</div>", tn[5]);
                    item.BTN1_Str += btn[0] == 0 ? @"&nbsp;" : string.Format(@"<div>{0}%</div>", btn[0]);
                    item.BTN2_Str += btn[1] == 0 ? @"&nbsp;" : string.Format(@"<div>{0}%</div>", btn[1]);
                    item.BTN3_Str += btn[2] == 0 ? @"&nbsp;" : string.Format(@"<div>{0}%</div>", btn[2]);
                    item.BTN4_Str += btn[3] == 0 ? @"&nbsp;" : string.Format(@"<div>{0}%</div>", btn[3]);
                    item.BTN5_Str += btn[4] == 0 ? @"&nbsp;" : string.Format(@"<div>{0}%</div>", btn[4]);
                    item.BTN6_Str += btn[5] == 0 ? @"&nbsp;" : string.Format(@"<div>{0}%</div>", btn[5]);
                }
            }

            return item;
        }
    }
}