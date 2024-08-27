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
    public class TunnelManagementController : Controller
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

            string dateFrom = OurUtility.ValueOf(Request, "dateFrom");
            string dateTo = OurUtility.ValueOf(Request, "dateTo");

            // -- Actual code
            try
            {
                List<Model_View_TunnelManagement> items = new List<Model_View_TunnelManagement>();

                using (MERCY_Ctx db = new MERCY_Ctx())
                {
                    using (var connection = new SqlConnection(db.Database.Connection.ConnectionString))
                    {
                        connection.Open();

                        SqlCommand command = connection.CreateCommand();

                        command.CommandText = string.Format(@"
                                                                select s.SiteName, h.*
                                                                from HaulingRequest_Detail_PortionBlending h
	                                                                left join Company c on h.Company = c.CompanyCode
	                                                                left join Site s on c.SiteId = s.SiteId
                                                                where RecordIdx in
	                                                                (
		                                                                select Max(RecordIdx)
		                                                                from HaulingRequest_Detail_PortionBlending
		                                                                where BlendingDate >= '{0}'
                                                                            and BlendingDate <= '{1}'
		                                                                group by BlendingDate, Shift, Company
	                                                                )
                                                                order by BlendingDate desc, Shift, Company
                                                                ", dateFrom, dateTo);

                        // -- getting PortionBlending
                        Model_View_TunnelManagement record = null;
                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                record = new Model_View_TunnelManagement
                                {
                                    RecordIdx = OurUtility.ToInt64(reader["RecordIdx"].ToString()),
                                    BlendingDate_Str = OurUtility.DateFormat(reader["BlendingDate"].ToString(), "dd-MMM-yyyy"),
                                    Shift = OurUtility.ToInt32(reader["Shift"].ToString()),
                                    Site = reader["SiteName"].ToString(),
                                    Company = reader["Company"].ToString(),
                                    Source = string.Empty,
                                    Hopper = reader["Hopper"].ToString(),
                                    Tunnel = reader["Tunnel"].ToString(),
                                    Product = reader["Product"].ToString(),
                                    Quality = string.Empty,
                                    Tunnel_Actual_Text = string.Empty,
                                    Tunnel_Actual_Time = string.Empty,
                                    Remark = OurUtility.Handle_Enter(reader["Remark"].ToString()),
                                    NoHauling = reader["NoHauling"].ToString(),
                                    RecordId_Snapshot = reader["RecordId_Snapshot"].ToString()
                                };

                                items.Add(record);
                            }
                        }

                        command.CommandText = string.Format(@"
                                                                select d.*
                                                                from HaulingRequest_Detail_PortionBlending_Details d
                                                                , 
                                                                (
	                                                                select h.HaulingRequest, h.RecordId_Snapshot
                                                                    from HaulingRequest_Detail_PortionBlending h
                                                                    where RecordIdx in
	                                                                    (
		                                                                    select Max(RecordIdx)
		                                                                    from HaulingRequest_Detail_PortionBlending
		                                                                    where BlendingDate = '{0}'
		                                                                    group by BlendingDate, Shift, Company
	                                                                    )
                                                                ) Sub_Query
                                                                where d.HaulingRequest = Sub_Query.HaulingRequest
                                                                      and d.PortionBlending = Sub_Query.RecordId_Snapshot
                                                                ", dateFrom);

                        // -- getting PortionBlending Details
                        List<Model_View_TunnelManagement_ROM> items_ROM = new List<Model_View_TunnelManagement_ROM>();
                        Model_View_TunnelManagement_ROM record_ROM = null;
                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                record_ROM = new Model_View_TunnelManagement_ROM
                                {
                                    Block = reader["Block"].ToString(),
                                    ROM_Name = reader["ROM_Name"].ToString(),
                                    ROM_ID = reader["ROM_ID"].ToString(),
                                    CV = reader["CV"].ToString(),
                                    TS = reader["TS"].ToString(),
                                    ASH = reader["ASH"].ToString(),
                                    IM = reader["IM"].ToString(),
                                    TM = reader["TM"].ToString(),
                                    Ton = reader["Ton"].ToString(),
                                    Portion = OurUtility.ToInt32(reader["Portion"].ToString()),
                                    PortionBlending = reader["PortionBlending"].ToString()
                                };

                                items_ROM.Add(record_ROM);
                            }
                        }

                        items.ForEach(c =>
                        {
                            double detail_CV = 0.0;
                            double detail_TS = 0.0;
                            double detail_ASH = 0.0;

                            c.Source = string.Empty;
                            c.Tunnel_Actual_Text = string.Empty;
                            c.Tunnel_Actual_Time = string.Empty;
                            c.Tunnel_Actual_Remark = string.Empty;

                            try
                            {
                                var dataQuery =
                                    (
                                        from rom in items_ROM
                                        where rom.PortionBlending == c.RecordId_Snapshot
                                        select new
                                        {
                                            Name = rom.Block + " " + rom.ROM_Name
                                            , rom.Portion
                                            , rom.CV
                                            , rom.TS
                                            , rom.ASH
                                        }
                                    );

                                var roms = dataQuery.ToList().OrderBy(x => x.Name).ToList();

                                string separator = string.Empty;
                                roms.ForEach(c4 =>
                                {
                                    c.Source += string.Format(@"<div style='margin-bottom:5px;'>{0} ({1} %)</div>", c4.Name, c4.Portion);

                                    //Kalkulasi Quality, dengan strategi seperti berikut:
                                    //   - pastikan bahwa semua data selalu di Round terlebih dahulu
                                    //   - baru kemudian dilakukan Kalkulasi Penjumlahan
                                    detail_CV += ((OurUtility.Round(OurUtility.ToDouble(c4.CV), 2) * c4.Portion) / 100.0);
                                    detail_TS += ((OurUtility.Round(OurUtility.ToDouble(c4.TS), 2) * c4.Portion) / 100.0);
                                    detail_ASH += ((OurUtility.Round(OurUtility.ToDouble(c4.ASH), 2) * c4.Portion) / 100.0);
                                });

                                // kembali dilakukan Rounding, agar konsisten
                                detail_CV = OurUtility.Round(detail_CV, 0);
                                detail_TS = OurUtility.Round(detail_TS, 2);
                                detail_ASH = OurUtility.Round(detail_ASH, 2);

                                c.Quality = string.Format(@"<div style='margin-bottom:5px;'>CV={0:N0}</div>
                                                            <div style='margin-bottom:5px;'>TS={1:N2}</div>
                                                            <div style='margin-bottom:5px;'>ASH={2:N2}</div>"
                                                        , detail_CV, detail_TS, detail_ASH);

                                command.CommandText = string.Format(@"
                                                                    select d.RecordId
                                                                            , d.TunnelId
                                                                            , Convert(varchar(16), [Time], 120) Time_str
                                                                            , d.Remark
                                                                            , t.Name
                                                                    from Tunnel_Actual d, Tunnel t
                                                                    where d.TunnelId = t.TunnelId
                                                                        and d.HaulingRequest_Reference = {0}
                                                                    order by d.RecordId
                                                                    ", c.RecordIdx);

                                string time = string.Empty;

                                using (SqlDataReader reader = command.ExecuteReader())
                                {
                                    while (reader.Read())
                                    {
                                        time = OurUtility.Substring(reader["Time_str"].ToString(), 11).Trim();

                                        c.Tunnel_Actual_Text += string.Format(@"<div style='height:14px;margin-bottom:5px;'>{0}</div>", reader["Name"].ToString());
                                        c.Tunnel_Actual_Time += string.Format(@"<div style='height:14px;margin-bottom:5px;'>{0}</div>", time);
                                        c.Tunnel_Actual_Remark += string.Format(@"<div style='height:14px;margin-bottom:5px;'>{0}</div>", reader["Remark"].ToString());
                                    }
                                }
                            }
                            catch (Exception) { }
                        });

                        connection.Close();
                    }
                }

                var dataQuery_Shift_1 =
                                   (
                                       from d in items
                                       where d.Shift == 1
                                       orderby d.Company
                                       select d
                                   );

                var items_Shift_1 = dataQuery_Shift_1.ToList();

                var dataQuery_Shift_2 =
                                   (
                                       from d in items
                                       where d.Shift == 2
                                       orderby d.Company
                                       select d
                                   );

                var items_Shift_2 = dataQuery_Shift_2.ToList();

                var result = new { Success = true, Permission = permission_Item, User = UserX.Information(user, is_Show_User_Menu, is_Show_User_Relation), Message = string.Empty, MessageDetail = string.Empty, Version = Configuration.VERSION, Items_Shift_1 = items_Shift_1, Items_Shift_2 = items_Shift_2 };
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
                    var data = new Tunnel
                    {
                        CompanyCode = OurUtility.ValueOf(p_collection, "company"),
                        Name = OurUtility.ValueOf(p_collection, "name"),
                        IsActive = OurUtility.ValueOf(p_collection, "isactive").Equals("1"),

                        CreatedOn = DateTime.Now,
                        CreatedBy = user.UserId
                    };

                    db.Tunnels.Add(data);
                    db.SaveChanges();

                    var result = new { Success = true, Permission = permission_Item, Message = BaseConstants.MESSAGE_CREATE_SUCCESS, MessageDetail = string.Empty, Version = Configuration.VERSION};
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
                int id = OurUtility.ToInt32(OurUtility.ValueOf(Request, ".id"));
                DateTime now = DateTime.Now;
                string dateTime_str = string.Empty;
                DateTime time = now;

                string changed_Tunnel = string.Empty;
                string changed_to_Draft = string.Empty;
                string separator = string.Empty;

                using (MERCY_Ctx db = new MERCY_Ctx())
                {
                    string recordId = string.Empty;
                    for (int i=0; ;i++)
                    {
                        recordId = OurUtility.ValueOf(Request, "RecordId"+i.ToString());
                        if (string.IsNullOrEmpty(recordId) ) break;
                        //if (string.IsNullOrEmpty(recordId) || (recordId != "-1")) break;

                        dateTime_str = OurUtility.ValueOf(Request, "Time" + i.ToString());
                        time = OurUtility.ToDateTime(dateTime_str);

                        if (recordId == "-1")
                        {
                            // NEW data

                            try
                            {
                                var data = new Tunnel_Actual
                                {
                                    HaulingRequest_Reference = id,
                                    TunnelId = OurUtility.ToInt32(OurUtility.ValueOf(Request, "Tunnel" + i.ToString())),
                                    Time = time,
                                    Remark = OurUtility.ValueOf(Request, "Remark" + i.ToString()),
                                    Status = "Draft",

                                    CreatedOn = DateTime.Now,
                                    CreatedBy = user.UserId
                                };

                                db.Tunnel_Actual.Add(data);
                                db.SaveChanges();

                                changed_Tunnel = data.TunnelId.ToString() + "," + data.TunnelId.ToString();
                                Add_to_History(db, data, true, changed_Tunnel);

                                changed_to_Draft += separator + data.RecordId.ToString();
                                separator = ",";
                            }
                            catch {}

                        }
                        else
                        {
                            // UPDATE data

                            try
                            {    
                                var dataQuery =
                                    (
                                        from d in db.Tunnel_Actual
                                        where d.RecordId.ToString() == recordId
                                        select d
                                    );

                                var data = dataQuery.SingleOrDefault();

                                if (i == 0)
                                {
                                    // [First Record] --> don't change "Time"
                                    time = data.Time;
                                }

                                if (data.TunnelId != OurUtility.ToInt32(OurUtility.ValueOf(Request, "Tunnel" + i.ToString())) ||
                                        data.Time != time ||
                                        data.Remark != OurUtility.ValueOf(Request, "Remark" + i.ToString())
                                    )
                                {

                                    // reset dulu nilai ini
                                    changed_Tunnel = string.Empty;

                                    // if Tunnel is changed then, reset Status to be "Draft"
                                    if (data.TunnelId != OurUtility.ToInt32(OurUtility.ValueOf(Request, "Tunnel" + i.ToString())))
                                    {
                                        data.Status = "Draft";

                                        // record perubahan Tunnel
                                        changed_Tunnel = data.TunnelId.ToString() + "," + OurUtility.ValueOf(Request, "Tunnel" + i.ToString());

                                        changed_to_Draft += separator + data.RecordId.ToString();
                                        separator = ",";
                                    }

                                    data.TunnelId = OurUtility.ToInt32(OurUtility.ValueOf(Request, "Tunnel" + i.ToString()));
                                    data.Time = time;
                                    data.Remark = OurUtility.ValueOf(Request, "Remark" + i.ToString());

                                    data.LastModifiedOn = DateTime.Now;
                                    data.LastModifiedBy = user.UserId;

                                    Add_to_History(db, data, changed_Tunnel);

                                    db.SaveChanges();
                                }
                            }
                            catch {}
                        }
                    }

                    //DELETE
                    string[] listDeleted = OurUtility.ValueOf(Request, "Deleted").Split(',');

                    foreach (var rid in listDeleted)
                    {
                        try
                        {
                            var dataQuery =
                                (
                                    from d in db.Tunnel_Actual
                                    where d.RecordId.ToString() == rid
                                    select d
                                );

                            var dt = dataQuery.SingleOrDefault();
                            db.Tunnel_Actual.Remove(dt);

                            db.SaveChanges();
                        }
                        catch {}
                    }

                    var result = new { Success = true, Permission = permission_Item, Message = BaseConstants.MESSAGE_UPDATE_SUCCESS, MessageDetail = string.Empty, Version = Configuration.VERSION, Changed_to_Draft = changed_to_Draft };
                    return Json(result, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                var result = new { Success = false, Permission = permission_Item, ex.Message, MessageDetail = ex.InnerException.Message, Version = Configuration.VERSION, Items = string.Empty, Total = 0 };
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
                int id = OurUtility.ToInt32(OurUtility.ValueOf(Request, ".id"));

                using (MERCY_Ctx db = new MERCY_Ctx())
                {
                    var dataQuery =
                        (
                            from d in db.Tunnels
                            where d.TunnelId == id
                            select d
                        );

                    var data = dataQuery.SingleOrDefault();
                    db.Tunnels.Remove(data);

                    db.SaveChanges();

                    var result = new { Success = true, Permission = permission_Item, Message = BaseConstants.MESSAGE_DELETE_SUCCESS, MessageDetail = string.Empty, Version = Configuration.VERSION};
                    return Json(result, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                var result = new { Success = false, Permission = permission_Item, ex.Message, MessageDetail = string.Empty, Version = Configuration.VERSION, Items = string.Empty, Total = 0 };
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
                                from d in db.Tunnels
                                where d.IsActive
                                orderby d.Name
                                select new
                                {
                                    id = d.CompanyCode
                                    , text = d.Name
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
                                from d in db.HaulingRequest_Detail_PortionBlending
                                where d.RecordIdx == id
                                select new Model_View_TunnelManagement
                                {
                                    Company = d.Company
                                    , Product = d.Product
                                    , BlendingDate = d.BlendingDate
                                    , Shift = d.Shift
                                    , Hopper = d.Hopper
                                    , Tunnel = d.Tunnel
                                }
                            );

                    var item = dataQuery.SingleOrDefault();
                    if (item == null)
                    {
                        var result_NotFound = new { Success = false, Permission = permission_Item, User = UserX.Information(user, is_Show_User_Menu, is_Show_User_Relation), Message = "Id: " + id.ToString() + " is not found", MessageDetail = string.Empty, Version = Configuration.VERSION };
                        return Json(result_NotFound, JsonRequestBehavior.AllowGet);
                    }

                    item.Editable = false;

                    item.BlendingDate_Str = OurUtility.DateFormat(item.BlendingDate, "dd-MMM-yyyy");
                    item.BlendingDate_Str2 = OurUtility.DateFormat(item.BlendingDate, "yyyy-MM-dd");
                    item.BlendingDate_Str3 = OurUtility.DateFormat(item.BlendingDate, "yyyy-MM-dd");

                    DateTime now = DateTime.Now;

                    if (item.Shift == 1)
                    {
                        string shift_start = item.BlendingDate_Str2 + " 07:00";
                        string shift_end = item.BlendingDate_Str3 + " 19:00";

                        if (now >= OurUtility.ToDateTime(shift_start) && now < OurUtility.ToDateTime(shift_end))
                        {
                            item.Editable = true;
                        }
                    }
                    else
                    {
                        string shift_start = item.BlendingDate_Str2 + " 19:00";

                        // Next Day
                        item.BlendingDate_Str3 = OurUtility.DateFormat(item.BlendingDate.AddDays(1), "yyyy-MM-dd");

                        string shift_end = item.BlendingDate_Str3 + " 07:00";

                        if (now >= OurUtility.ToDateTime(shift_start) && now < OurUtility.ToDateTime(shift_end))
                        {
                            item.Editable = true;
                        }
                    }
                    
                    var dataQuery_Tunnel =
                            (
                                from d in db.Tunnels
                                where d.CompanyCode == item.Company
                                orderby d.Name
                                select new
                                {
                                    id = d.TunnelId
                                    , text = d.Name
                                }
                            );

                    var item_Tunnel = dataQuery_Tunnel.ToList();

                    List<Model_View_Tunnel_Actual> item_Tunnel_Actual = new List<Model_View_Tunnel_Actual>();

                    using (var connection = new SqlConnection(db.Database.Connection.ConnectionString))
                    {
                        connection.Open();

                        SqlCommand command = connection.CreateCommand();

                        command.CommandText = string.Format(@"
                                                                select d.RecordId
                                                                        , d.TunnelId
                                                                        , Convert(varchar(20), [Time], 120) Time_str
                                                                        , d.Remark
                                                                        , t.Name
                                                                        , d.Status
                                                                from Tunnel_Actual d, Tunnel t
                                                                where d.TunnelId = t.TunnelId
                                                                    and d.HaulingRequest_Reference = {0}
                                                                order by d.RecordId
                                                                ", id);

                        string time = string.Empty;

                        Model_View_Tunnel_Actual actual_Tunnel = null;
                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                actual_Tunnel = new Model_View_Tunnel_Actual
                                {
                                    RecordId = OurUtility.ToInt64(OurUtility.ValueOf(reader, "RecordId")),
                                    TunnelId = OurUtility.ToInt32(OurUtility.ValueOf(reader, "TunnelId")),
                                    Time = OurUtility.ToDateTime(OurUtility.ValueOf(reader, "Time_str"))
                                };

                                actual_Tunnel.Time_Str = OurUtility.DateFormat(actual_Tunnel.Time, @"yyyy-MM-dd HH:mm:ss");
                                actual_Tunnel.Time_Str2 = OurUtility.DateFormat(actual_Tunnel.Time, "dd-MMM-yyyy");
                                actual_Tunnel.Time_Hour = OurUtility.DateFormat(actual_Tunnel.Time, "HH");
                                actual_Tunnel.Time_Minute = OurUtility.DateFormat(actual_Tunnel.Time, "mm");

                                actual_Tunnel.Remark = OurUtility.ValueOf(reader, "Remark");
                                actual_Tunnel.Tunnel_Name = OurUtility.ValueOf(reader, "Name");
                                actual_Tunnel.Status = OurUtility.ValueOf(reader, "Status");

                                item_Tunnel_Actual.Add(actual_Tunnel);
                            }
                        }

                        connection.Close();
                    }

                    /*
                            var dataQuery_Tunnel_Actual =
                            (
                                from d in db.Tunnel_Actual
                                    join t in db.Tunnels on d.TunnelId equals t.TunnelId
                                where d.HaulingRequest_Reference == id
                                orderby d.RecordId
                                select new 
                                {
                                    d.RecordId
                                    , d.TunnelId
                                    , d.Time
                                    , d.Remark
                                    , t.Name
                                }
                            );

                    var item_Tunnel_Actual = dataQuery_Tunnel_Actual.ToList();*/

                    var data_Discussions = DiscussionController.Get("TunnelManagement", id);

                    var result = new { Success = true, Permission = permission_Item, User = UserX.Information(user, is_Show_User_Menu, is_Show_User_Relation), Message = string.Empty, MessageDetail = string.Empty, Version = Configuration.VERSION, Item = item, Tunnel = item_Tunnel, Tunnel_Actual = item_Tunnel_Actual, Discussions = data_Discussions };
                    return Json(result, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                var result = new { Success = false, Permission = permission_Item, ex.Message, MessageDetail = string.Empty, Version = Configuration.VERSION, Items = string.Empty, Total = 0 };
                return Json(result, JsonRequestBehavior.AllowGet);
            }
        }

        public JsonResult Approve()
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
                int id = OurUtility.ToInt32(OurUtility.ValueOf(Request, ".id"));

                string changed_to_Approved = string.Empty;
                string separator = string.Empty;

                using (MERCY_Ctx db = new MERCY_Ctx())
                {
                    var dataQuery =
                        (
                            from d in db.Tunnel_Actual
                            where d.HaulingRequest_Reference == id
                                && d.Status == "Draft"
                            select d
                        );

                    var items = dataQuery.ToList();
                    foreach (var d in items)
                    {
                        d.Status = "Approved";

                        d.LastModifiedOn = DateTime.Now;
                        d.LastModifiedBy = user.UserId;

                        Add_to_History(db, d, string.Empty);

                        changed_to_Approved += separator + d.RecordId.ToString();
                        separator = ",";
                    }

                    db.SaveChanges();

                    var result = new { Success = true, Permission = permission_Item, Message = "Approve Success", MessageDetail = string.Empty, Version = Configuration.VERSION, Changed_to_Approved = changed_to_Approved };
                    return Json(result, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                var result = new { Success = false, Permission = permission_Item, ex.Message, MessageDetail = string.Empty, Version = Configuration.VERSION, Items = string.Empty, Total = 0 };
                return Json(result, JsonRequestBehavior.AllowGet);
            }
        }

        public static void Add_to_History(MERCY_Ctx p_db, Tunnel_Actual p_data, string p_changed_Tunnel)
        {
            Add_to_History(p_db, p_data, false, p_changed_Tunnel);
        }

        public static void Add_to_History(MERCY_Ctx p_db, Tunnel_Actual p_data, bool p_is_SaveChanges, string p_changed_Tunnel)
        {
            try
            {
                var history = new Tunnel_Actual_History
                {
                    CreatedOn_ = DateTime.Now,
                    Tunnel_Actual_Id = p_data.RecordId,
                    HaulingRequest_Reference = p_data.HaulingRequest_Reference,
                    TunnelId = p_data.TunnelId,
                    CreatedBy = p_data.CreatedBy,
                    CreatedOn = p_data.CreatedOn,
                    LastModifiedBy = p_data.LastModifiedBy,
                    LastModifiedOn = p_data.LastModifiedOn,
                    Time = p_data.Time,
                    Remark = p_data.Remark,
                    Status = p_data.Status,
                    Changed_Tunnel = p_changed_Tunnel
                };

                p_db.Tunnel_Actual_History.Add(history);

                if (p_is_SaveChanges) p_db.SaveChanges();
            }
            catch {}
        }
    }
}