using System;
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
    public class SamplingRequestController : Controller
    {
        public ActionResult Index2()
        {
            return View();
        }

        public JsonResult Index()
        {
            OurUtility.Set_Response_ServerMessage(Response);

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

            string company = OurUtility.ValueOf(Request, "c");
            string samplingType = OurUtility.ValueOf(Request, "ty");
            string dateFrom = OurUtility.ValueOf(Request, "dateFrom");
            string dateTo = OurUtility.ValueOf(Request, "dateTo");

            bool is_company_ALL = (string.IsNullOrEmpty(company) || company == "all");
            bool is_samplingType_ALL = (string.IsNullOrEmpty(samplingType) || samplingType == "all");
            
            bool isAll_Text = true;
            string txt = Request["txt"];
            isAll_Text = string.IsNullOrEmpty(txt);

            try
            {
                DateTime dateFrom_O = DateTime.Now;
                DateTime dateTo_O = DateTime.Now.AddDays(1);

                try
                {
                    dateFrom_O = DateTime.Parse(dateFrom);
                }
                catch { }
                try
                {
                    dateTo_O = DateTime.Parse(dateTo).AddDays(1);
                }
                catch {}

                using (MERCY_Ctx db = new MERCY_Ctx())
                {
                    var dataQuery =
                            (
                                from dt in db.SamplingRequests
                                    join usr in db.Users on dt.CreatedBy equals usr.UserId
                                //orderby dt.Company, dt.SamplingType, dt.SamplingRequestId descending
                                orderby dt.SamplingRequestId descending
                                where (is_company_ALL || dt.Company == company) 
                                        && (is_samplingType_ALL || dt.SamplingType == samplingType)
                                        && dt.RequestDate >= dateFrom_O
                                        && dt.RequestDate < dateTo_O
                                        && (isAll_Text || dt.Company.Contains(txt)
                                                        || dt.SamplingType.Contains(txt)
                                                        || usr.FullName.Contains(txt)
                                                        || usr.Department.Contains(txt)
                                                        )
                                orderby dt.SamplingRequestId descending
                                select new Model_View_SamplingRequest
                                {
                                    SamplingRequestId = dt.SamplingRequestId
                                    , Company = dt.Company
                                    , SamplingType = dt.SamplingType
                                    , RequestDate = dt.RequestDate
                                    , Requestor = usr.FullName
                                    , Department = usr.Department
                                    , Email = usr.Email
                                    , Location = dt.HAC_Text
                                    , PICArea = dt.PICArea
                                    , CreatedOn = dt.CreatedOn
                                    , CreatedBy_Str = usr.FullName
                                }
                            );

                    //var data = dataQuery.Skip(skip).Take(pageSize).ToList();
                    var items = dataQuery.ToList();

                    try
                    {
                        items.ForEach(c =>
                        {
                            c.RequestDate_Str = OurUtility.DateFormat(c.RequestDate, "dd-MMM-yyyy");
                            //c.RequestDate_Str = OurUtility.DateFormat(c.RequestDate, "dd-MMM-yyyy HH:mm");
                            c.RequestDate_Str2 = OurUtility.DateFormat(c.RequestDate, "MM/dd/yyyy");
                            c.CreatedOn_Str = OurUtility.DateFormat(c.CreatedOn, "dd-MMM-yyyy HH:mm");
                        });
                    }
                    catch {}

                    var draw = OurUtility.ValueOf(Request, "draw");

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
                    var data = new SamplingRequest
                    {
                        Company = OurUtility.ValueOf(p_collection, "company"),
                        SamplingType = OurUtility.ValueOf(p_collection, "samplingType"),
                        RequestDate = DateTime.Parse(OurUtility.ValueOf(p_collection, "requestDate")),
                        HAC_Text = OurUtility.ValueOf(p_collection, "location"),
                        Tonnage = OurUtility.ValueOf(p_collection, "tonnage"),
                        PICArea = OurUtility.ValueOf(p_collection, "picarea"),
                        Remark = OurUtility.ValueOf(p_collection, "remark"),
                        CV = OurUtility.ValueOf(p_collection, "cv").Equals("1"),
                        TS = OurUtility.ValueOf(p_collection, "ts").Equals("1"),
                        TM = OurUtility.ValueOf(p_collection, "tm").Equals("1"),
                        PROX = OurUtility.ValueOf(p_collection, "prox").Equals("1"),
                        RD = OurUtility.ValueOf(p_collection, "rd").Equals("1"),
                        SIZE = OurUtility.ValueOf(p_collection, "size").Equals("1"),
                        FLOW = OurUtility.ValueOf(p_collection, "flow").Equals("1"),

                        CreatedOn = DateTime.Now,
                        CreatedBy = user.UserId
                    };

                    db.SamplingRequests.Add(data);
                    db.SaveChanges();

                    long samplingId = data.SamplingRequestId;

                    var dataH = new SamplingRequest_History
                    {
                        CreatedOn = DateTime.Now,
                        CreatedBy = user.UserId,
                        SamplingRequestId = samplingId,
                        Description = "Submit"
                    };
                    db.SamplingRequest_History.Add(dataH);
                    db.SaveChanges();

                    string msgx = string.Empty;
                    try
                    {
                        using (var connection = new SqlConnection(db.Database.Connection.ConnectionString))
                        {
                            connection.Open();

                            switch (data.SamplingType)
                            {
                                case "PIT Sampling":
                                    SeamController.Add(connection, samplingId, OurUtility.ValueOf(p_collection, "pit_seam"), ref msgx);
                                    break;
                                case "DT Sampling":
                                    SeamController.Add(connection, samplingId, OurUtility.ValueOf(p_collection, "pit_seam"), ref msgx);
                                    break;
                                case "ROM":
                                    RomController.Add(connection, samplingId, OurUtility.ValueOf(p_collection, "roms"), ref msgx);
                                    break;
                            }

                            connection.Close();
                        }
                    }
                    catch {}

                    int recordNumber = OurUtility.ToInt32(Request["recordNumber"]);

                    string labIds = string.Empty;
                    string separator = string.Empty;

                    for (int i = 1; i <= recordNumber; i++)
                    {
                        try
                        {
                            string x = OurUtility.ValueOf(p_collection, "lab" + i.ToString());
                            if (string.IsNullOrEmpty(x)) continue;

                            var lab = new SamplingRequest_Lab
                            {
                                SamplingRequest = samplingId,
                                LabId = x,

                                CreatedOn = DateTime.Now,
                                CreatedBy = user.UserId
                            };
                            lab.CreatedOn_Date_Only = lab.CreatedOn.ToString("yyyy-MM-dd");
                            lab.CreatedOn_Year_Only = lab.CreatedOn.Year;
                            lab.Company = data.Company;

                            db.SamplingRequest_Lab.Add(lab);
                            db.SaveChanges();

                            // for Data History: only Add if necessary
                            if ( ! string.IsNullOrEmpty(lab.LabId))
                            {
                                labIds += separator + lab.LabId;
                                separator = ",";
                            }
                        }
                        catch {}
                    }

                    // for Data History: only Add if necessary
                    if ( ! string.IsNullOrEmpty(labIds))
                    {
                        var dataH2 = new SamplingRequest_History();
                        dataH.CreatedOn = DateTime.Now;
                        dataH.CreatedBy = user.UserId;
                        dataH.SamplingRequestId = samplingId;
                        dataH.Description = "Update Lab ID: " + labIds;
                        db.SamplingRequest_History.Add(dataH);
                        db.SaveChanges();
                    }

                    var result = new { Success = true, Permission = permission_Item, Message = BaseConstants.MESSAGE_CREATE_SUCCESS, MessageDetail = string.Empty, Version = Configuration.VERSION, Data = samplingId };
                    return Json(result, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                var result = new { Success = false, Permission = permission_Item, ex.Message, MessageDetail = string.Empty, Version = Configuration.VERSION};
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
                                from dt in db.SamplingRequests
                                join usr in db.Users on dt.CreatedBy equals usr.UserId
                                where dt.SamplingRequestId == id
                                select new Model_View_SamplingRequest
                                {
                                    SamplingRequestId = dt.SamplingRequestId
                                    , Company = dt.Company
                                    , SamplingType = dt.SamplingType
                                    , RequestDate = dt.RequestDate
                                    , Requestor = usr.FullName
                                    , Department = usr.Department
                                    , Email = usr.Email
                                    , Location = dt.HAC_Text
                                    , PICArea = dt.PICArea
                                    , Remark = dt.Remark
                                    , Tonnage = dt.Tonnage
                                    , CreatedOn = dt.CreatedOn
                                    , CreatedBy_Str = usr.FullName
                                    , CV = dt.CV
                                    , TS = dt.TS
                                    , TM = dt.TM
                                    , PROX = dt.PROX
                                    , RD = dt.RD
                                    , SIZE = dt.SIZE
                                    , FLOW = dt.FLOW
                                }
                            );

                    var item = dataQuery.SingleOrDefault();
                    if (item == null)
                    {
                        var result_NotFound = new { Success = false, Permission = permission_Item, User = UserX.Information(user, is_Show_User_Menu, is_Show_User_Relation), Message = "Id: " + id.ToString() + " is not found", MessageDetail = string.Empty, Version = Configuration.VERSION};
                        return Json(result_NotFound, JsonRequestBehavior.AllowGet);
                    }
                    
                    item.RequestDate_Str = OurUtility.DateFormat(item.RequestDate, "dd-MMM-yyyy HH:mm");
                    item.RequestDate_Str2 = OurUtility.DateFormat(item.RequestDate, "MM/dd/yyyy");
                    item.CreatedOn_Str = OurUtility.DateFormat(item.CreatedOn, "dd-MMM-yyyy HH:mm");

                    var dataQuery_Labs =
                            (
                                from dt in db.SamplingRequest_Lab
                                where dt.SamplingRequest == id
                                select new Model_View_SamplingRequest_Lab
                                {
                                    RecordId = dt.RecordId
                                    , LabId = dt.LabId
                                    , CreatedOn = dt.CreatedOn
                                    , LastModifiedOn = dt.LastModifiedOn
                                }
                            );

                    var items_Labs = dataQuery_Labs.ToList();

                    try
                    {
                        items_Labs.ForEach(c =>
                        {
                            c.CreatedOn_Str = OurUtility.DateFormat(c.CreatedOn, "dd-MMM-yyyy - HH:mm");
                            c.CreatedOn_Str2 = OurUtility.DateFormat(c.CreatedOn, "dd-MMM-yyyy");

                            if (c.LastModifiedOn != null)
                            {
                                c.CreatedOn_Str = OurUtility.DateFormat(c.LastModifiedOn, "dd-MMM-yyyy - HH:mm");
                            }
                        });
                    }
                    catch {}

                    var dataQuery_Seam =
                                (
                                    from d in db.SamplingRequest_SEAM
                                    where d.SamplingRequest == id
                                    orderby d.COMPANY_PIT_SEAM
                                    select new 
                                    {
                                        d.SEAM
                                        , d.COMPANY_PIT_SEAM
                                    }
                                );

                    var items_Seams = dataQuery_Seam.ToList();

                    var dataQuery_ROM =
                                (
                                    from d in db.SamplingRequest_ROM
                                    where d.SamplingRequest == id
                                    orderby d.Block, d.ROM_Name
                                    select new
                                    {
                                        d.ROM_ID
                                        , d.ROM_Name
                                        , d.Block
                                        , Names = (d.Block + " " + d.ROM_Name)
                                    }
                                );

                    var items_ROMs = dataQuery_ROM.ToList();

                    var dataQuery_History =
                                (
                                    from d in db.SamplingRequest_History
                                    join u in db.Users on d.CreatedBy equals u.UserId
                                    where d.SamplingRequestId == id
                                    orderby d.RecordId descending
                                    select new Model_View_SamplingRequest_History
                                    {
                                        CreatedOn = d.CreatedOn
                                        , CreatedBy = d.CreatedBy
                                        , FullName = u.FullName
                                        , Description = d.Description
                                    }
                                );

                    var items_History = dataQuery_History.ToList();
                    try
                    {
                        items_History.ForEach(c =>
                        {
                            c.CreatedOn_Str = OurUtility.DateFormat(c.CreatedOn, @"dd/MM/yyyy HH:mm:ss");
                        });
                    }
                    catch {}

                    var result = new { Success = true, Permission = permission_Item, User = UserX.Information(user, is_Show_User_Menu, is_Show_User_Relation), Message = string.Empty, MessageDetail = string.Empty, Version = Configuration.VERSION, Item = item, Labs = items_Labs, Seams = items_Seams, ROMs = items_ROMs, Histories = items_History };
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
            long id = OurUtility.ToInt64(Request[".id"]);

            try
            {
                using (MERCY_Ctx db = new MERCY_Ctx())
                {
                    var dataQuery =
                        (
                            from d in db.SamplingRequests
                            where d.SamplingRequestId == id
                            select d
                        );

                    var data = dataQuery.SingleOrDefault();

                    data.Company = OurUtility.ValueOf(p_collection, "company");
                    data.SamplingType = OurUtility.ValueOf(p_collection, "samplingType");
                    data.RequestDate = DateTime.Parse(OurUtility.ValueOf(p_collection, "requestDate"));
                    data.HAC_Text = OurUtility.ValueOf(p_collection, "location");
                    data.Tonnage = OurUtility.ValueOf(p_collection, "tonnage");
                    data.PICArea = OurUtility.ValueOf(p_collection, "picarea");
                    data.Remark = OurUtility.ValueOf(p_collection, "remark");
                    data.CV = OurUtility.ValueOf(p_collection, "cv").Equals("1");
                    data.TS = OurUtility.ValueOf(p_collection, "ts").Equals("1");
                    data.TM = OurUtility.ValueOf(p_collection, "tm").Equals("1");
                    data.PROX = OurUtility.ValueOf(p_collection, "prox").Equals("1");
                    data.RD = OurUtility.ValueOf(p_collection, "rd").Equals("1");
                    data.SIZE = OurUtility.ValueOf(p_collection, "size").Equals("1");
                    data.FLOW = OurUtility.ValueOf(p_collection, "flow").Equals("1");

                    data.LastModifiedOn = DateTime.Now;
                    data.LastModifiedBy = user.UserId;

                    db.SaveChanges();

                    long samplingId = data.SamplingRequestId;

                    string msgx = string.Empty;
                    try
                    {
                        using (var connection = new SqlConnection(db.Database.Connection.ConnectionString))
                        {
                            connection.Open();

                            SeamController.Delete(connection, id, ref msgx);
                            RomController.Delete(connection, id, ref msgx);

                            switch (data.SamplingType)
                            {
                                case "PIT Sampling":
                                    SeamController.Add(connection, id, OurUtility.ValueOf(p_collection, "pit_seam"), ref msgx);
                                    break;
                                case "DT Sampling":
                                    SeamController.Add(connection, id, OurUtility.ValueOf(p_collection, "pit_seam"), ref msgx);
                                    break;
                                case "ROM":
                                    RomController.Add(connection, id, OurUtility.ValueOf(p_collection, "roms"), ref msgx);
                                    break;
                            }

                            connection.Close();
                        }
                    }
                    catch {}

                    int recordNumber = OurUtility.ToInt32(Request["recordNumber"]);
                    string labId = string.Empty;

                    string labIds = string.Empty;
                    string separator = string.Empty;

                    long recordId = 0;

                    for (int i = 1; i <= recordNumber; i++)
                    {
                        try
                        {
                            labId = OurUtility.ValueOf(p_collection, "lab" + i.ToString());
                            if (string.IsNullOrEmpty(labId)) continue;
                            if (labId == "undefined")
                            {
                                labId = string.Empty;
                            }

                            recordId = OurUtility.ToInt64(OurUtility.ValueOf(p_collection, "labIdx_" + i.ToString()));

                            // Check data from Table Detail
                            var dataQueryx =
                            (
                                from d in db.SamplingRequest_Lab
                                where d.RecordId == recordId
                                select d
                            );

                            var lab = dataQueryx.SingleOrDefault();
                            if (lab == null)
                            {
                                // -- Mode ADD
                                lab = new SamplingRequest_Lab
                                {
                                    SamplingRequest = samplingId,
                                    LabId = labId,

                                    CreatedOn = DateTime.Now,
                                    CreatedBy = user.UserId
                                };
                                lab.CreatedOn_Date_Only = lab.CreatedOn.ToString("yyyy-MM-dd");
                                lab.CreatedOn_Year_Only = lab.CreatedOn.Year;
                                lab.Company = data.Company;

                                db.SamplingRequest_Lab.Add(lab);
                                db.SaveChanges();

                                // for Data History: only Add if necessary
                                if ( ! string.IsNullOrEmpty(labId))
                                {
                                    labIds += separator + labId;
                                    separator = ",";
                                }
                            }
                            else
                            {
                                // -- Mode Edit

                                // jika salah satu Nilai berubah
                                // baru dilakukan Save - Changed
                                if ((lab.SamplingRequest != samplingId)
                                    || (lab.LabId != labId)
                                    )
                                {
                                    // for Data History: only Add if necessary
                                    if ( ! string.IsNullOrEmpty(labId))
                                    {
                                        if (lab.LabId != labId)
                                        {
                                            labIds += separator + labId;
                                            separator = ",";
                                        }
                                    }

                                    lab.SamplingRequest = samplingId;
                                    lab.LabId = labId;

                                    lab.LastModifiedOn = DateTime.Now;
                                    lab.LastModifiedBy = user.UserId;

                                    db.SaveChanges();
                                }
                            }
                        }
                        catch {}
                    }

                    // for Data History: only Add if necessary
                    if ( ! string.IsNullOrEmpty(labIds))
                    {
                        var dataH = new SamplingRequest_History
                        {
                            CreatedOn = DateTime.Now,
                            CreatedBy = user.UserId,
                            SamplingRequestId = id,
                            Description = "Update Lab ID: " + labIds
                        };
                        db.SamplingRequest_History.Add(dataH);
                        db.SaveChanges();
                    }

                    var result = new { Success = true, Permission = permission_Item, Message = BaseConstants.MESSAGE_UPDATE_SUCCESS, MessageDetail = string.Empty, Version = Configuration.VERSION};
                    return Json(result, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                var result = new { Success = false, Permission = permission_Item, ex.Message, MessageDetail = string.Empty, Version = Configuration.VERSION};
                return Json(result, JsonRequestBehavior.AllowGet);
            }
        }

        public JsonResult CheckReport()
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

            bool success = false;
            string message = string.Empty;
            int count = 0;

            string id = Request[".id"];

            try
            {
                using (MERCY_Ctx db = new MERCY_Ctx())
                {
                    using (var connection = new SqlConnection(db.Database.Connection.ConnectionString))
                    {
                        connection.Open();

                        SqlCommand command = connection.CreateCommand();

                        string sql = string.Format(@"exec Report_Form_Sampling_Check {0}", id);

                        command.CommandText = sql;

                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                count = OurUtility.ToInt32(reader["Count2"].ToString());

                                success = true;
                                message = string.Empty;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                message = ex.Message;
            }

            var result = new { Success = success, Permission = permission_Item, Count = count, Message = message, Time = DateTime.Now.ToString("dd-MMM-yyyy HH:mm:ss.ffff"), Version = Configuration.VERSION };
            return Json(result, JsonRequestBehavior.AllowGet);
        }
    }
}