using System;
using System.Linq;
using System.Web.Mvc;

using System.Data;
using System.Data.SqlClient;
using System.Text;

using MERCY.Data.EntityFramework;
using MERCY.Web.BackEnd.Models;
using MERCY.Web.BackEnd.Helpers;
using MERCY.Web.BackEnd.Security;
using Permission = MERCY.Web.BackEnd.Security.Permission;

namespace MERCY.Web.BackEnd.Areas.Api.Controllers
{
    public class LabMaintenanceController : Controller
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

            bool isAll_Text = true;
            string txt = OurUtility.ValueOf(Request, "txt");
            isAll_Text = string.IsNullOrEmpty(txt);

            // -- Actual code
            try
            {
                string instrument_str = Request["Instrument"];
                int instrument = OurUtility.ToInt32(instrument_str);
                bool is_All_Instrument = (string.IsNullOrEmpty(instrument_str) || instrument_str == "all");

                string category_str = Request["MaintenanceActivity"];
                int category = OurUtility.ToInt32(category_str);
                bool is_All_Category = (string.IsNullOrEmpty(category_str) || category_str == "all");

                string companyCode = OurUtility.ValueOf(Request, "company").ToString();
                int? siteId = null;
                if (!string.IsNullOrEmpty(OurUtility.ValueOf(Request, "site")) && OurUtility.ValueOf(Request, "site") != "all")
                    siteId = int.Parse(OurUtility.ValueOf(Request, "site"));

                bool allCompany = string.IsNullOrEmpty(OurUtility.ValueOf(Request, "company")) || OurUtility.ValueOf(Request, "company") == "all";
                bool allSite = siteId == null || OurUtility.ValueOf(Request, "site") == "all";

                using (MERCY_Ctx db = new MERCY_Ctx())
                {
                    var allowedCompanies = db.UserCompanies.Where(u => u.UserId == user.UserId).Select(c => c.CompanyCode).ToList();
                    var allowedSites = db.UserSites.Where(u => u.UserId == user.UserId).Select(u => u.SiteId).ToList();

                    var dataQuery =
                            (
                                from d in db.LabMaintenances
                                    join u in db.Users on d.CreatedBy equals u.UserId
                                    join a in db.Users on d.AssignedTo equals a.UserId
                                    join i in db.Instruments on d.Instrument equals i.RecordId
                                    join ma in db.MaintenanceActivities on d.Category equals ma.RecordId
                                    join c in db.Companies on d.CompanyCode equals c.CompanyCode into cc
                                    from c in cc.DefaultIfEmpty()
                                    join s in db.Sites on d.SiteId equals s.SiteId into lms
                                    from s in lms.DefaultIfEmpty()
                                where (isAll_Text || d.Description.Contains(txt) || d.InstrumentCode.Contains(txt))
                                    && (is_All_Instrument || d.Instrument == instrument)
                                    && (is_All_Category || d.Category == category)
                                    && ((allCompany && allowedCompanies.Any(u => u == d.CompanyCode)) || d.CompanyCode == companyCode)
                                    && ((allSite && allowedSites.Any(u => u == d.SiteId)) || d.SiteId == siteId)
                                orderby ma.Category, i.InstrumentName
                                select new Model_View_LabMaintenance
                                {
                                    RecordId = d.RecordId
                                    , IsActive = d.IsActive
                                    , Instrument = d.Instrument
                                    , InstrumentName = i.InstrumentName
                                    , Description = d.Description
                                    , InstrumentCode = d.InstrumentCode
                                    , AssignedTo =  d.AssignedTo
                                    , AssignedTo_FullName = a.FullName
                                    , NextSchedule = d.NextSchedule
                                    , CategoryName = ma.Category
                                    , CompanyName = c.Name
                                    , SiteName = s.SiteName
                                    , SiteId = d.SiteId
                                    , CompanyCode = d.CompanyCode
                                }
                            );

                    var items = dataQuery.ToList();
                    try
                    {
                        DateTime now = DateTime.Now;

                        items.ForEach(c =>
                        {
                            c.CreatedOn_Str = OurUtility.DateFormat(c.CreatedOn, @"dd-MMM-yyyy HH:mm:ss");
                            c.CreatedOn_Str2 = OurUtility.DateFormat(c.CreatedOn, "dd-MMM-yyyy");
                            c.NextSchedule_Str = OurUtility.DateFormat(c.NextSchedule, @"dd-MMM-yyyy HH:mm");
                            c.NextSchedule_Str2 = OurUtility.DateFormat(c.NextSchedule, "dd-MMM-yyyy");

                            c.Status = "Active";
                            if (c.NextSchedule < now)
                            {
                                c.Status = "Overdue";
                            }
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
                int siteId = 0;
                if (!string.IsNullOrEmpty(OurUtility.ValueOf(Request, "site")))
                    siteId = int.Parse(OurUtility.ValueOf(Request, "site"));
                var companyCode = OurUtility.ValueOf(p_collection, "company");

                string msg2 = string.Empty;
                string noteFile = string.Empty;
                string noteFile2 = string.Empty;
                string runWhat = string.Empty;
                string runWhat2 = string.Empty;

                OurUtility.Upload(Request, "file", UploadFolder, ref noteFile, ref noteFile2, ref msg2);
                OurUtility.Upload(Request, "file2", UploadFolder, ref runWhat, ref runWhat2, ref msg2);

                using (MERCY_Ctx db = new MERCY_Ctx())
                {
                    if (string.IsNullOrEmpty(OurUtility.ValueOf(Request, "site")) || !db.Sites.Any(u => u.SiteId == siteId))
                    {
                        var msg = new { Success = false, Permission = permission_Item, Message = "The requested site is not registered in our system! Please consider selecting a different site.", MessageDetail = string.Empty, Version = Configuration.VERSION };
                        return Json(msg, JsonRequestBehavior.AllowGet);
                    }

                    if (string.IsNullOrEmpty(companyCode) || !db.Companies.Any(u => u.CompanyCode == companyCode && u.SiteId == siteId))
                    {
                        var msg = new { Success = false, Permission = permission_Item, Message = "The requested Company is not registered in our system! Please consider selecting a different company.", MessageDetail = string.Empty, Version = Configuration.VERSION };
                        return Json(msg, JsonRequestBehavior.AllowGet);
                    }

                    var data = new LabMaintenance
                    {
                        Category = OurUtility.ToInt32(OurUtility.ValueOf(p_collection, "Category")),
                        Instrument = OurUtility.ToInt32(OurUtility.ValueOf(p_collection, "Instrument")),
                        InstrumentCode = OurUtility.ValueOf(p_collection, "InstrumentCode"),
                        Description = OurUtility.ValueOf(p_collection, "Description"),
                        AssignedTo = OurUtility.ToInt32(OurUtility.ValueOf(p_collection, "AssignedTo")),
                        Remark = OurUtility.ValueOf(p_collection, "Remark"),
                        NotesFile = noteFile,
                        NotesFile2 = noteFile2,
                        RunWhat = runWhat,
                        RunWhat2 = runWhat2,
                        NextSchedule = DateTime.Parse(OurUtility.ValueOf(p_collection, "NextSchedule")),
                        RepeatEvery = OurUtility.ValueOf(p_collection, "RepeatEvery"),
                        RepeatCount = OurUtility.ToInt32(OurUtility.ValueOf(p_collection, "RepeatCount")),
                        IsActive = OurUtility.ValueOf(p_collection, "IsActive").ToUpper().Equals("TRUE"),
                        CompanyCode = companyCode,
                        SiteId = siteId,

                        CreatedOn = DateTime.Now,
                        CreatedBy = user.UserId
                    };

                    db.LabMaintenances.Add(data);
                    db.SaveChanges();

                    InsertHistory(db, data);

                    long id = data.RecordId;

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
                catch {}

                return result;
            }
        }

        private bool InsertHistory(MERCY_Ctx p_db, LabMaintenance p_data)
        {
            bool result = false;

            try
            {
                using (MERCY_Ctx db = new MERCY_Ctx())
                {
                    var history = new LabMaintenance_History
                    {
                        LabMaintenance_Data = p_data.RecordId,
                        Category = p_data.Category,
                        Instrument = p_data.Instrument,
                        InstrumentCode = p_data.InstrumentCode,
                        Description = p_data.Description,
                        AssignedTo = p_data.AssignedTo,
                        Remark = p_data.Remark,
                        NotesFile = p_data.NotesFile,
                        NotesFile2 = p_data.NotesFile2,
                        RunWhat = p_data.RunWhat,
                        RunWhat2 = p_data.RunWhat2,
                        NextSchedule = p_data.NextSchedule,
                        RepeatEvery = p_data.RepeatEvery,
                        RepeatCount = p_data.RepeatCount,
                        IsActive = p_data.IsActive,

                        CreatedOn = p_data.CreatedOn,
                        CreatedBy = p_data.CreatedBy,
                        LastModifiedOn = p_data.LastModifiedOn,
                        LastModifiedBy = p_data.LastModifiedBy
                    };

                    db.LabMaintenance_History.Add(history);
                    db.SaveChanges();

                    result = true;
                }
            }
            catch {}

            return result;
        }

        private bool Complete(MERCY_Ctx p_db, LabMaintenance p_data, int p_executedBy)
        {
            bool result = false;

            try
            {
                using (MERCY_Ctx db = new MERCY_Ctx())
                {
                    var complete = new LabMaintenance_Complete
                    {
                        LabMaintenance_Data = p_data.RecordId,
                        Category = p_data.Category,
                        Instrument = p_data.Instrument,
                        InstrumentCode = p_data.InstrumentCode,
                        Description = p_data.Description,
                        AssignedTo = p_data.AssignedTo,
                        Remark = p_data.Remark,
                        NotesFile = p_data.NotesFile,
                        NotesFile2 = p_data.NotesFile2,
                        RunWhat = p_data.RunWhat,
                        RunWhat2 = p_data.RunWhat2,
                        NextSchedule = p_data.NextSchedule,
                        RepeatEvery = p_data.RepeatEvery,
                        RepeatCount = p_data.RepeatCount,
                        IsActive = p_data.IsActive,

                        CreatedOn = p_data.CreatedOn,
                        CreatedBy = p_data.CreatedBy,
                        LastModifiedOn = p_data.LastModifiedOn,
                        LastModifiedBy = p_data.LastModifiedBy,
                        CompletedBy = p_executedBy,
                        CompletedOn = DateTime.Now
                    };

                    db.LabMaintenance_Complete.Add(complete);
                    db.SaveChanges();

                    result = true;
                }
            }
            catch {}

            return result;
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
                                from dt in db.LabMaintenances
                                join usr in db.Users on dt.CreatedBy equals usr.UserId
                                join c in db.Companies on dt.CompanyCode equals c.CompanyCode into cc
                                from c in cc.DefaultIfEmpty()
                                join s in db.Sites on dt.SiteId equals s.SiteId into lms
                                from s in lms.DefaultIfEmpty()
                                where dt.RecordId == id
                                select new Model_View_LabMaintenance
                                {
                                    Category = dt.Category
                                    , Instrument = dt.Instrument
                                    , InstrumentCode = dt.InstrumentCode
                                    , Description = dt.Description
                                    , AssignedTo = dt.AssignedTo
                                    , Remark = dt.Remark
                                    , NotesFile = dt.NotesFile
                                    , NotesFile2 = dt.NotesFile2
                                    , RunWhat = dt.RunWhat
                                    , RunWhat2 = dt.RunWhat2
                                    , NextSchedule = dt.NextSchedule
                                    , RepeatEvery = dt.RepeatEvery
                                    , RepeatCount = dt.RepeatCount
                                    , CompanyCode = dt.CompanyCode
                                    , CompanyName = c.Name
                                    , SiteId = dt.SiteId
                                    , SiteName = s.SiteName
                                }
                            );

                    var data = dataQuery.SingleOrDefault();
                    if (data == null)
                    {
                        var result_NotFound = new { Success = false, Permission = permission_Item, User = UserX.Information(user, is_Show_User_Menu, is_Show_User_Relation), Message = "Id: " + id.ToString() + " is not found", MessageDetail = string.Empty, Version = Configuration.VERSION};
                        return Json(result_NotFound, JsonRequestBehavior.AllowGet);
                    }

                    data.NextSchedule_Str = OurUtility.DateFormat(data.NextSchedule, @"yyyy-MM-dd HH:mm:ss");
                    data.NextSchedule_Str2 = OurUtility.DateFormat(data.NextSchedule, "dd-MMM-yyyy");
                    data.NextSchedule_Hour = OurUtility.DateFormat(data.NextSchedule, "HH");
                    data.NextSchedule_Minute = OurUtility.DateFormat(data.NextSchedule, "mm");
                    data.NextSchedule_AmPm = OurUtility.DateFormat(data.NextSchedule, "tt");

                    var dataQuery_History =
                                (
                                    from d in db.LabMaintenance_History
                                        join u in db.Users on d.CreatedBy equals u.UserId
                                        join c in db.MaintenanceActivities on d.Category equals c.RecordId
                                        join i in db.Instruments on d.Instrument equals i.RecordId
                                    where d.LabMaintenance_Data == id
                                    orderby d.RecordId descending
                                    select new Model_View_LabMaintenance_History
                                    {
                                        CreatedOn = d.CreatedOn
                                        , CreatedBy = d.CreatedBy
                                        , FullName = u.FullName
                                        , CategoryName = c.Category
                                        , InstrumentName = i.InstrumentName
                                        , Description = d.Description
                                        , LastModifiedOn = d.LastModifiedOn
                                    }
                                );

                    var data_History = dataQuery_History.ToList();
                    try
                    {
                        data_History.ForEach(c =>
                        {
                            if (c.LastModifiedOn == null)
                            {
                                c.Date_Str = OurUtility.DateFormat(c.CreatedOn, @"dd-MMM-yyyy HH:mm:ss");
                            }
                            else
                            {
                                c.Date_Str = OurUtility.DateFormat(c.LastModifiedOn, @"dd-MMM-yyyy HH:mm:ss");
                            }
                        });
                    }
                    catch {}

                    var dataQuery_Complete =
                                (
                                    from d in db.LabMaintenance_Complete
                                        join u in db.Users on d.CompletedBy equals u.UserId
                                    where d.LabMaintenance_Data == id
                                    orderby d.RecordId descending
                                    select new Model_View_LabMaintenance_History
                                    {
                                        CreatedOn = d.CreatedOn
                                        , CreatedBy = d.CreatedBy
                                        , FullName = u.FullName
                                        , Description = d.Description
                                        , LastModifiedOn = d.CompletedOn
                                        , NotesFile = d.NotesFile
                                        , NotesFile2 = d.NotesFile2
                                        , RunWhat = d.RunWhat
                                        , RunWhat2 = d.RunWhat2
                                    }
                                );

                    var data_Complete = dataQuery_Complete.ToList();
                    try
                    {
                        data_Complete.ForEach(c =>
                        {
                            c.Date_Str = OurUtility.DateFormat(c.LastModifiedOn, @"dd-MMM-yyyy HH:mm:ss");
                        });
                    }
                    catch {}

                    var result = new { Success = true, Permission = permission_Item, User = UserX.Information(user, is_Show_User_Menu, is_Show_User_Relation), Message = string.Empty, MessageDetail = string.Empty, Version = Configuration.VERSION, Item = data, Histories = data_History, Completes = data_Complete };
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
            string message = string.Empty;
            if (Edit(p_collection, user, false, ref message))
            {
                var msg = new { Success = true, Permission = permission_Item, Data = string.Empty, Message = BaseConstants.MESSAGE_UPDATE_SUCCESS, MessageException = string.Empty, StackTrace = string.Empty };
                return Json(msg, JsonRequestBehavior.AllowGet);
            }
            else
            {
                var msg = new { Success = false, Permission = permission_Item, Data = string.Empty, Message = message, MessageException = message, StackTrace = string.Empty};
                return Json(msg, JsonRequestBehavior.AllowGet);
            }
        }

        private bool Edit(FormCollection p_collection, UserX p_user, bool p_is_Complete_also, ref string p_message)
        {
            bool result = false;

            p_message = string.Empty;

            try
            {
                int siteId = 0;
                if (!string.IsNullOrEmpty(OurUtility.ValueOf(Request, "site")))
                    siteId = int.Parse(OurUtility.ValueOf(Request, "site"));
                var companyCode = OurUtility.ValueOf(p_collection, "company");

                long id = OurUtility.ToInt64(Request[".id"]);
                string msg2 = string.Empty;
                string noteFile = string.Empty;
                string noteFile2 = string.Empty;
                string runWhat = string.Empty;
                string runWhat2 = string.Empty;

                OurUtility.Upload(Request, "file", UploadFolder, ref noteFile, ref noteFile2, ref msg2);
                OurUtility.Upload(Request, "file2", UploadFolder, ref runWhat, ref runWhat2, ref msg2);

                using (MERCY_Ctx db = new MERCY_Ctx())
                {
                    if (string.IsNullOrEmpty(OurUtility.ValueOf(Request, "site")) || !db.Sites.Any(u => u.SiteId == siteId))
                    {
                        p_message = "The requested site is not registered in our system! Please consider selecting a different site.";
                        return result;
                    }

                    if (string.IsNullOrEmpty(companyCode) || !db.Companies.Any(u => u.CompanyCode == companyCode && u.SiteId == siteId))
                    {
                        p_message  = "The requested Company is not registered in our system! Please consider selecting a different company.";
                        return result;
                    }

                    var dataQuery =
                        (
                            from d in db.LabMaintenances
                            where d.RecordId == id
                            select d
                        );

                    var data = dataQuery.SingleOrDefault();

                    //data.Category = OurUtility.ToInt32(OurUtility.ValueOf(p_collection, "Category"));
                    //data.Instrument = OurUtility.ToInt32(OurUtility.ValueOf(p_collection, "Instrument"));
                    data.InstrumentCode = OurUtility.ValueOf(p_collection, "InstrumentCode");
                    data.Description = OurUtility.ValueOf(p_collection, "Description");
                    data.AssignedTo = OurUtility.ToInt32(OurUtility.ValueOf(p_collection, "AssignedTo"));
                    data.Remark = OurUtility.ValueOf(p_collection, "Remark");
                    
                    // change file: if necessary
                    if ( ! string.IsNullOrEmpty(noteFile))
                    {
                        data.NotesFile = noteFile;
                        data.NotesFile2 = noteFile2;
                    }
                    // change file: if necessary
                    if ( ! string.IsNullOrEmpty(runWhat))
                    {
                        data.RunWhat = runWhat;
                        data.RunWhat2 = runWhat2;
                    }

                    data.NextSchedule = DateTime.Parse(OurUtility.ValueOf(p_collection, "NextSchedule"));
                    data.RepeatEvery = OurUtility.ValueOf(p_collection, "RepeatEvery");
                    data.RepeatCount = OurUtility.ToInt32(OurUtility.ValueOf(p_collection, "RepeatCount"));
                    data.IsActive = OurUtility.ValueOf(p_collection, "IsActive").ToUpper().Equals("TRUE");
                    data.CompanyCode = companyCode;
                    data.SiteId = siteId;

                    data.LastModifiedOn = DateTime.Now;
                    data.LastModifiedBy = p_user.UserId;

                    db.SaveChanges();

                    InsertHistory(db, data);

                    if (p_is_Complete_also)
                    {
                        // simpan dulu sebagai Complete
                        Complete(db, data, p_user.UserId);

                        // lakukan perubahan data
                        switch (data.RepeatEvery)
                        {
                            case "Day":
                                data.NextSchedule = data.NextSchedule.AddDays(1);
                                break;
                            case "Month":
                                data.NextSchedule = data.NextSchedule.AddMonths(1);
                                break;
                            case "Year":
                                data.NextSchedule = data.NextSchedule.AddYears(1);
                                break;
                        }

                        // reset
                        data.NotesFile = string.Empty;
                        data.NotesFile2 = string.Empty;
                        data.RunWhat = string.Empty;
                        data.RunWhat2 = string.Empty;

                        data.LastModifiedOn = DateTime.Now;
                        data.LastModifiedBy = p_user.UserId;

                        db.SaveChanges();

                        InsertHistory(db, data);
                    }
                }

                result = true;
            }
            catch (Exception ex)
            {
                p_message = ex.Message;
            }

            return result;
        }

        public JsonResult Complete(FormCollection p_collection)
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
            string message = string.Empty;
            if (Edit(p_collection, user, true, ref message))
            {
                var msg = new { Success = true, Permission = permission_Item, Data = string.Empty, Message = BaseConstants.MESSAGE_UPDATE_SUCCESS, MessageException = string.Empty, StackTrace = string.Empty };
                return Json(msg, JsonRequestBehavior.AllowGet);
            }
            else
            {
                var msg = new { Success = false, Permission = permission_Item, Data = string.Empty, Message = message, MessageException = message, StackTrace = string.Empty };
                return Json(msg, JsonRequestBehavior.AllowGet);
            }
        }

        public string Scheduler()
        {
            // Check Permission based on Current Url
            UserX user = new UserX(Request);
            Permission_Item permission_Item = null;

            Permission.Check_API(Request, user, ref permission_Item);
            if ( ! permission_Item.Is_View)
            {
                var msg = new { Success = false, Permission = permission_Item, Message = Permission.ERROR_PERMISSION_READ, MessageDetail = string.Empty, Version = Configuration.VERSION };
                //return Json(msg, JsonRequestBehavior.AllowGet);
                return Permission.ERROR_PERMISSION_READ;
            }

            // -- Actual code
            try
            {
                Configuration config = new Configuration();

                string email_address_MasterData = string.Empty;
                string email_Fullname = string.Empty;

                string content_temp = string.Empty;
                string content_Periode_1 = string.Empty;
                string content_Periode_2 = string.Empty;

                string p_email = OurUtility.ValueOf(Request, "email");
                string p_now = OurUtility.ValueOf(Request, "now");
                string p_history = OurUtility.ValueOf(Request, "history");

                p_email = p_email.Trim().ToLower();
                p_now = p_now.Trim();
                p_history = (p_history.Trim().ToLower() == "yes" ? "1" : "0");

                using (MERCY_Ctx db = new MERCY_Ctx())
                {
                    if (p_email == "yes")
                    {
                        var dataQuery =
                        (
                            from usr in db.Users
                            where usr.IsLabMaintenance
                            select new
                            {
                                usr.FullName
                                , usr.Email
                            }
                        );

                        var data = dataQuery.ToList();
                        int email_count = 0;
                        data.ForEach(c =>
                        {
                            if ( ! string.IsNullOrEmpty(c.Email))
                            {
                                email_count++;

                                email_address_MasterData += (email_count > 1 ? "," : "") + c.Email;
                                email_Fullname += (email_count > 1 ? "," : "") + c.FullName;
                            }
                        });
                    }

                    using (var connection = new SqlConnection(db.Database.Connection.ConnectionString))
                    {
                        connection.Open();

                        SqlCommand command = connection.CreateCommand();

                        string sql = string.Format(@"exec PROCESS_Reminder_Lab_Maintenance '{0}', {1}", p_now, p_history);
                        command.CommandText = sql;
                        int count_period_1 = 0;
                        int count_period_2 = 0;

                        string style_td_border = "border-right:1px solid #dee2e6 !important;border-bottom:1px solid #dee2e6 !important;height:40px;padding: 0px !important;";

                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                content_temp = string.Format(@"
                                                            <tr>
                                                                <td style='{5}'>## No_Line ##</td>
                                                                <td style='{5}'>{0}</td>
                                                                <td style='{5}'>{1}</td>
                                                                <td style='{5}'>{2}</td>
                                                                <td style='{5}'>{3}</td>
                                                                <td style='{5}'>{4}</td>
                                                            </tr>
                                                            ", reader["Category"].ToString()
                                                            , reader["InstrumentName"].ToString()
                                                            , reader["InstrumentCode"].ToString()
                                                            , reader["Description"].ToString()
                                                            , OurUtility.DateFormat(DateTime.Parse(reader["NextSchedule"].ToString()), "dd-MMM-yyyy HH:mm")
                                                            , style_td_border
                                                            );

                                if (reader["Period"].ToString() == config.Reminder_Maintenance_1)
                                {
                                    count_period_1++;
                                    content_Periode_1 += content_temp.Replace("## No_Line ##", count_period_1.ToString());
                                }
                                else if (reader["Period"].ToString() == config.Reminder_Maintenance_2)
                                {
                                    count_period_2++;
                                    content_Periode_2 += content_temp.Replace("## No_Line ##", count_period_2.ToString());
                                }
                            }
                        }

                        if ( ! string.IsNullOrEmpty(content_Periode_1))
                        {
                            content_Periode_1 = string.Format(@"
                                                                <table style='min-width:800px !important;border-left:1px solid #dee2e6 !important;border-top:1px solid #dee2e6 !important;'>
                                                                    <tr>
                                                                        <td style='text-align:center;{1}'>No</td>
                                                                        <td style='text-align:center;{1}'>Category</td>
                                                                        <td style='text-align:center;{1}'>Instrument</td>
                                                                        <td style='text-align:center;{1}'>Instrument Code</td>
                                                                        <td style='text-align:center;{1}'>Description</td>
                                                                        <td style='text-align:center;{1}'>Next Schedule</td>
                                                                    </tr>
                                                                    {0}
                                                                </table>
                                                                ", content_Periode_1, style_td_border);
                        }
                        if ( ! string.IsNullOrEmpty(content_Periode_2))
                        {
                            content_Periode_2 = string.Format(@"
                                                                <table style='min-width:800px !important;border-left:1px solid #dee2e6 !important;border-top:1px solid #dee2e6 !important;'>
                                                                    <tr>
                                                                        <td style='text-align:center;{1}'>No</td>
                                                                        <td style='text-align:center;{1}'>Category</td>
                                                                        <td style='text-align:center;{1}'>Instrument</td>
                                                                        <td style='text-align:center;{1}'>Instrument Code</td>
                                                                        <td style='text-align:center;{1}'>Description</td>
                                                                        <td style='text-align:center;{1}'>Next Schedule</td>
                                                                    </tr>
                                                                    {0}
                                                                </table>
                                                                ", content_Periode_2, style_td_border);
                        }

                        connection.Close();
                    }
                }

                var result = "Status : Ok - scheduler LabMaintenance";

                if (p_email == "yes")
                {
                    NotificationController notification = new NotificationController();
                    notification.InitializeController(this.Request.RequestContext);

                    string email_body = string.Empty;
                    string email_subject = config.Reminder_Maintenance_Subject;

                    if ( ! string.IsNullOrEmpty(content_Periode_1))
                    {
                        email_body = System.IO.File.ReadAllText(notification.TemplateFolder + @"\email_template_maintenance_1.html", Encoding.UTF8);
                        email_body = email_body.Replace("{TABLE}", content_Periode_1);

                        notification.Send(string.Empty, email_subject, email_address_MasterData, config.Reminder_Maintenance_CC, email_body);
                    }

                    if ( ! string.IsNullOrEmpty(content_Periode_2))
                    {
                        email_body = System.IO.File.ReadAllText(notification.TemplateFolder + @"\email_template_maintenance_2.html", Encoding.UTF8);
                        email_body = email_body.Replace("{TABLE}", content_Periode_2);

                        notification.Send(string.Empty, email_subject, email_address_MasterData, config.Reminder_Maintenance_CC, email_body);
                    }
                }
                else
                {
                    result = string.Format(@"Status : Ok - scheduler LabMaintenance
                                            <div style='margin-left:50px;'>
                                                <div style='margin-top:5px;'>Notification for <strong>{0} day{1}</strong></div>
                                                <div style='margin-top:5 !important;'>
                                                    {2}
                                                </div>
                                                <div style='margin-top:10px;'>Notification for <strong>{3} day{4}</strong></div>
                                                <div style='margin-top:5 !important;'>
                                                    {5}
                                                </div>
                                            </div>
                                            "
                                            , config.Reminder_Maintenance_1, (OurUtility.ToInt32(config.Reminder_Maintenance_1) > 1 ? "s" : ""), (string.IsNullOrEmpty(content_Periode_1) ? "&nbsp;&nbsp;&nbsp;Empty" : content_Periode_1)
                                            , config.Reminder_Maintenance_2, (OurUtility.ToInt32(config.Reminder_Maintenance_2) > 1 ? "s" : ""), (string.IsNullOrEmpty(content_Periode_2) ? "&nbsp;&nbsp;&nbsp;Empty" : content_Periode_2)
                                            );
                }

                return result;
            }
            catch (Exception ex)
            {
                return "Error : " + ex.Message;
            }
        }
    }
}