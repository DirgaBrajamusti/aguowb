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
    public class ConsumableController : Controller
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

            // -- Actual code
            try
            {
                bool is_Show_User_Menu = OurUtility.ValueOf(Request, "u_menu").Equals("1");
                bool is_Show_User_Relation = OurUtility.ValueOf(Request, "u_relation").Equals("1");

                bool isAll_Text = true;
                string txt = OurUtility.ValueOf(Request, "txt");
                isAll_Text = string.IsNullOrEmpty(txt);

                string instrument_str = Request["Instrument"];
                int instrument = OurUtility.ToInt32(instrument_str);
                bool is_All_Instrument = (string.IsNullOrEmpty(instrument_str) || instrument_str == "all");

                int? siteId = null;
                if (!string.IsNullOrEmpty(OurUtility.ValueOf(Request, "site")) && OurUtility.ValueOf(Request, "site") != "all")
                    siteId = int.Parse(OurUtility.ValueOf(Request, "site"));
                string companyCode = !string.IsNullOrEmpty(OurUtility.ValueOf(Request, "company")) && OurUtility.ValueOf(Request, "company") != "all" ?
                    OurUtility.ValueOf(Request, "company").ToString() : null;

                bool allSite = siteId == null;
                bool allCompany = companyCode == null;

                using (MERCY_Ctx db = new MERCY_Ctx())
                {
                    var allowedSites = db.UserSites.Where(u => u.UserId == user.UserId).Select(s => s.SiteId).ToList();
                    var allowedCompanies = db.UserCompanies.Where(u => u.UserId == user.UserId).Select(c => c.CompanyCode).ToList();

                    var dataQuery =
                            (
                                from d in db.Consumables
                                    join u in db.Users on d.CreatedBy equals u.UserId
                                    join unit in db.UnitMeasurements on d.UnitType equals unit.RecordId
                                    join i in db.Instruments on d.Instrument equals i.RecordId
                                    join s in db.Sites on d.SiteId equals s.SiteId
                                    join c in db.Companies on d.CompanyCode equals c.CompanyCode
                                where (isAll_Text || d.PartName.Contains(txt) || d.PartNumber.Contains(txt))
                                    && (is_All_Instrument || d.Instrument == instrument)
                                    && allowedSites.Any(u => u == d.SiteId)
                                    && allowedCompanies.Any(u => u == d.CompanyCode)
                                    && (allSite || d.SiteId == siteId)
                                    && (allCompany || d.CompanyCode == companyCode)
                                orderby d.RecordId
                                select new
                                {
                                    d.RecordId
                                    , d.PartName
                                    , d.PartNumber
                                    , d.CurrentQuantity
                                    , d.MinimumQuantity
                                    , d.UnitType
                                    , unit.UnitName
                                    , i.InstrumentName
                                    , d.SiteId
                                    , s.SiteName
                                    , d.CompanyCode
                                    , CompanyName = c.Name
                                }
                            );

                    var items = dataQuery.ToList();

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
            
            if (string.IsNullOrEmpty(OurUtility.ValueOf(p_collection, "SiteId")))
            {
                var msg = new { Success = false, Permission = permission_Item, Message = "Site id cannot be null!", MessageDetail = string.Empty, Version = Configuration.VERSION };
                return Json(msg, JsonRequestBehavior.AllowGet);
            }
            int siteId = int.Parse(OurUtility.ValueOf(p_collection, "SiteId"));

            if (string.IsNullOrEmpty(OurUtility.ValueOf(p_collection, "CompanyCode")))
            {
                var msg = new { Success = false, Permission = permission_Item, Message = "Company code cannot be null!", MessageDetail = string.Empty, Version = Configuration.VERSION };
                return Json(msg, JsonRequestBehavior.AllowGet);
            }
            var companyCode = OurUtility.ValueOf(p_collection, "CompanyCode");

            // -- Actual code
            try
            {
                string msg2 = string.Empty;
                string noteFile = string.Empty;
                string noteFile2 = string.Empty;
                string pictureFile = string.Empty;
                string pictureFile2 = string.Empty;

                OurUtility.Upload(Request, "file", UploadFolder, ref noteFile, ref noteFile2, ref msg2);
                OurUtility.Upload(Request, "file2", UploadFolder, ref pictureFile, ref pictureFile2, ref msg2);

                using (MERCY_Ctx db = new MERCY_Ctx())
                {
                    if (!db.Sites.Any(u => u.SiteId == siteId)) 
                    {
                        var msg = new { Success = false, Permission = permission_Item, Message = "The requested Site is not registered in our system! Please consider selecting a different site.", MessageDetail = string.Empty, Version = Configuration.VERSION };
                        return Json(msg, JsonRequestBehavior.AllowGet);
                    }
                    if (!db.UserSites.Any(u => u.SiteId == siteId && u.UserId == user.UserId))
                    {
                        var msg = new { Success = false, Permission = permission_Item, Message = "The requested Site is not allowed! Please consider selecting a different site.", MessageDetail = string.Empty, Version = Configuration.VERSION };
                        return Json(msg, JsonRequestBehavior.AllowGet);
                    }

                    if (!db.Companies.Any(u => u.SiteId == siteId && u.CompanyCode == companyCode))
                    {
                        var msg = new { Success = false, Permission = permission_Item, Message = "The requested Company is not registered in our system! Please consider selecting a different company.", MessageDetail = string.Empty, Version = Configuration.VERSION };
                        return Json(msg, JsonRequestBehavior.AllowGet);
                    }
                    if (!db.UserCompanies.Any(u => u.CompanyCode == companyCode && u.UserId == user.UserId))
                    {
                        var msg = new { Success = false, Permission = permission_Item, Message = "The requested Company is not allowed! Please consider selecting a different company.", MessageDetail = string.Empty, Version = Configuration.VERSION };
                        return Json(msg, JsonRequestBehavior.AllowGet);
                    }

                    var data = new Consumable
                    {
                        SiteId = siteId,
                        CompanyCode = companyCode,
                        Instrument = OurUtility.ToInt32(OurUtility.ValueOf(p_collection, "Instrument")),
                        CurrentQuantity = OurUtility.ToInt32(OurUtility.ValueOf(p_collection, "CurrentQuantity")),
                        MinimumQuantity = OurUtility.ToInt32(OurUtility.ValueOf(p_collection, "MinimumQuantity")),
                        UnitType = OurUtility.ToInt32(OurUtility.ValueOf(p_collection, "UnitType")),
                        ReoderDays = OurUtility.ToInt32(OurUtility.ValueOf(p_collection, "ReoderDays")),
                        PartName = OurUtility.ValueOf(p_collection, "PartName"),
                        PartNumber = OurUtility.ValueOf(p_collection, "PartNumber"),
                        MSDS_Code = OurUtility.ValueOf(p_collection, "MSDS_Code"),
                        InputQuantity = OurUtility.ToInt32(OurUtility.ValueOf(p_collection, "InputQuantity")),
                        OutputQuantity = OurUtility.ToInt32(OurUtility.ValueOf(p_collection, "OutputQuantity")),
                        Price = OurUtility.ToDouble(OurUtility.ValueOf(p_collection, "Price")),
                        Remark = OurUtility.ValueOf(p_collection, "Remark"),
                        NotesFile = noteFile,
                        NotesFile2 = noteFile2,
                        PictureFile = pictureFile,
                        PictureFile2 = pictureFile2,

                        CreatedOn = DateTime.Now,
                        CreatedBy = user.UserId
                    };

                    db.Consumables.Add(data);
                    db.SaveChanges();

                    string description = string.Format("Current Qty: {0}, Min. Qty: {1}", data.CurrentQuantity, data.MinimumQuantity);
                    InsertHistory(db, data, description);

                    long id = data.RecordId;

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
                    var allowedSites = db.UserSites.Where(u => u.UserId == user.UserId).Select(s => s.SiteId).ToList();
                    var allowedCompanies = db.UserCompanies.Where(u => u.UserId == user.UserId).Select(c => c.CompanyCode).ToList();

                    var dataQuery =
                            (
                                from dt in db.Consumables
                                    join usr in db.Users on dt.CreatedBy equals usr.UserId
                                    join s in db.Sites on dt.SiteId equals s.SiteId
                                    join c in db.Companies on dt.CompanyCode equals c.CompanyCode
                                where dt.RecordId == id
                                    && allowedSites.Any(u => u == dt.SiteId)
                                    && allowedCompanies.Any(u => u == dt.CompanyCode)
                                select new
                                {
                                    dt.Instrument
                                    , dt.CurrentQuantity
                                    , dt.MinimumQuantity
                                    , dt.UnitType
                                    , dt.ReoderDays
                                    , dt.PartName
                                    , dt.PartNumber
                                    , dt.MSDS_Code
                                    , dt.InputQuantity
                                    , dt.OutputQuantity
                                    , dt.Price
                                    , dt.Remark
                                    , dt.NotesFile
                                    , dt.PictureFile
                                    , dt.SiteId
                                    , s.SiteName
                                    , dt.CompanyCode
                                    , CompanyName = c.Name
                                }
                            );

                    var data = dataQuery.SingleOrDefault();
                    if (data == null)
                    {
                        var result_NotFound = new { Success = false, Permission = permission_Item, User = UserX.Information(user, is_Show_User_Menu, is_Show_User_Relation), Message = "Id: " + id.ToString() + " is not found", MessageDetail = string.Empty, Version = Configuration.VERSION};
                        return Json(result_NotFound, JsonRequestBehavior.AllowGet);
                    }
                    
                    var dataQuery_History =
                                (
                                    from d in db.Consumable_History
                                        join u in db.Users on d.CreatedBy equals u.UserId
                                    where d.Consumable_Data == id
                                    orderby d.RecordId descending
                                    select new Model_View_Consumable_History
                                    {
                                        CreatedOn = d.CreatedOn
                                        , LastModifiedOn = d.LastModifiedOn
                                        , CreatedBy = d.CreatedBy
                                        , FullName = u.FullName
                                        , Remark = d.Remark
                                        , Description = d.Description
                                        , CurrentQuantity = d.CurrentQuantity
                                        , InputQuantity = d.InputQuantity
                                        , OutputQuantity = d.OutputQuantity
                                        , MinimumQuantity = d.MinimumQuantity
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

                    return Json(new { Success = true, Permission = permission_Item, User = UserX.Information(user, is_Show_User_Menu, is_Show_User_Relation), Item = data, Histories = data_History }, JsonRequestBehavior.AllowGet);
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

            if (string.IsNullOrEmpty(OurUtility.ValueOf(p_collection, "SiteId")))
            {
                var msg = new { Success = false, Permission = permission_Item, Message = "Site id cannot be null!", MessageDetail = string.Empty, Version = Configuration.VERSION };
                return Json(msg, JsonRequestBehavior.AllowGet);
            }
            int siteId = int.Parse(OurUtility.ValueOf(p_collection, "SiteId"));

            if (string.IsNullOrEmpty(OurUtility.ValueOf(p_collection, "CompanyCode")))
            {
                var msg = new { Success = false, Permission = permission_Item, Message = "Company code cannot be null!", MessageDetail = string.Empty, Version = Configuration.VERSION };
                return Json(msg, JsonRequestBehavior.AllowGet);
            }
            var companyCode = OurUtility.ValueOf(p_collection, "CompanyCode");

            // -- Actual code
            long id = OurUtility.ToInt64(Request[".id"]);

            try
            {
                string msg2 = string.Empty;
                string noteFile = string.Empty;
                string noteFile2 = string.Empty;
                string pictureFile = string.Empty;
                string pictureFile2 = string.Empty;

                OurUtility.Upload(Request, "file", UploadFolder, ref noteFile, ref noteFile2, ref msg2);
                OurUtility.Upload(Request, "file2", UploadFolder, ref pictureFile, ref pictureFile2, ref msg2);

                using (MERCY_Ctx db = new MERCY_Ctx())
                {
                    if (!db.Sites.Any(u => u.SiteId == siteId))
                    {
                        var msg = new { Success = false, Permission = permission_Item, Message = "The requested Site is not registered in our system! Please consider selecting a different site.", MessageDetail = string.Empty, Version = Configuration.VERSION };
                        return Json(msg, JsonRequestBehavior.AllowGet);
                    }
                    if (!db.UserSites.Any(u => u.SiteId == siteId && u.UserId == user.UserId))
                    {
                        var msg = new { Success = false, Permission = permission_Item, Message = "The requested Site is not allowed! Please consider selecting a different site.", MessageDetail = string.Empty, Version = Configuration.VERSION };
                        return Json(msg, JsonRequestBehavior.AllowGet);
                    }

                    if (!db.Companies.Any(u => u.SiteId == siteId && u.CompanyCode == companyCode))
                    {
                        var msg = new { Success = false, Permission = permission_Item, Message = "The requested Company is not registered in our system! Please consider selecting a different company.", MessageDetail = string.Empty, Version = Configuration.VERSION };
                        return Json(msg, JsonRequestBehavior.AllowGet);
                    }
                    if (!db.UserCompanies.Any(u => u.CompanyCode == companyCode && u.UserId == user.UserId))
                    {
                        var msg = new { Success = false, Permission = permission_Item, Message = "The requested Company is not allowed! Please consider selecting a different company.", MessageDetail = string.Empty, Version = Configuration.VERSION };
                        return Json(msg, JsonRequestBehavior.AllowGet);
                    }

                    var dataQuery =
                        (
                            from d in db.Consumables
                            where d.RecordId == id
                            select d
                        );

                    var data = dataQuery.SingleOrDefault();

                    bool is_Quality_Minimum_Changed = (data.MinimumQuantity != OurUtility.ToInt32(OurUtility.ValueOf(p_collection, "MinimumQuantity")));

                    //data.StockStatus = 0;
                    data.SiteId = siteId;
                    data.CompanyCode = companyCode;
                    data.Instrument = OurUtility.ToInt32(OurUtility.ValueOf(p_collection, "Instrument"));
                    data.CurrentQuantity = OurUtility.ToInt32(OurUtility.ValueOf(p_collection, "CurrentQuantity"));
                    data.MinimumQuantity = OurUtility.ToInt32(OurUtility.ValueOf(p_collection, "MinimumQuantity"));
                    data.UnitType = OurUtility.ToInt32(OurUtility.ValueOf(p_collection, "UnitType"));
                    data.ReoderDays = OurUtility.ToInt32(OurUtility.ValueOf(p_collection, "ReoderDays"));
                    data.PartName = OurUtility.ValueOf(p_collection, "PartName");
                    data.PartNumber = OurUtility.ValueOf(p_collection, "PartNumber");
                    data.MSDS_Code = OurUtility.ValueOf(p_collection, "MSDS_Code");
                    data.InputQuantity = OurUtility.ToInt32(OurUtility.ValueOf(p_collection, "InputQuantity"));
                    data.OutputQuantity = OurUtility.ToInt32(OurUtility.ValueOf(p_collection, "OutputQuantity"));
                    data.Price = OurUtility.ToDouble(OurUtility.ValueOf(p_collection, "Price"));
                    data.Remark = OurUtility.ValueOf(p_collection, "Remark");
                    // change file: if necessary
                    if ( ! string.IsNullOrEmpty(noteFile))
                    {
                        data.NotesFile = noteFile;
                        data.NotesFile2 = noteFile2;
                    }
                    // change file: if necessary
                    if ( ! string.IsNullOrEmpty(pictureFile))
                    {
                        data.PictureFile = pictureFile;
                        data.PictureFile2 = pictureFile2;
                    }

                    data.LastModifiedOn = DateTime.Now;
                    data.LastModifiedBy = user.UserId;

                    db.SaveChanges();

                    string description = string.Empty;
                    string description_Input = string.Empty;
                    string description_Output = string.Empty;
                    string description_Input_Output = string.Empty;


                    if (is_Quality_Minimum_Changed)
                    {
                        description = string.Format("Min. Qty: {0}", data.MinimumQuantity);
                    }
                    if (data.InputQuantity > 0)
                    {
                        description_Input = string.Format("Input  Qty: {0}", data.InputQuantity);
                    }
                    if (data.OutputQuantity > 0)
                    {
                        description_Output = string.Format("Output   Qty: {0}", data.OutputQuantity);
                    }

                    if ( !string.IsNullOrEmpty(description_Input) 
                        && 
                         !string.IsNullOrEmpty(description_Output))
                    {
                        description_Input_Output = description_Input + ", " + description_Output;
                    }
                    else
                    {
                        if ( ! string.IsNullOrEmpty(description_Input))
                        {
                            description_Input_Output = description_Input;
                        }
                        else
                        {
                            description_Input_Output = description_Output;
                        }
                    }

                    if ( !string.IsNullOrEmpty(description) 
                        && 
                         !string.IsNullOrEmpty(description_Input_Output))
                    {
                        description += @"<br/>" + description_Input_Output;
                    }
                    else
                    {
                        if ( ! string.IsNullOrEmpty(description))
                        {
                            
                        }
                        else
                        {
                            description = description_Input_Output;
                        }
                    }

                    InsertHistory(db, data, description);

                    var result = new { Success = true, Permission = permission_Item, Message = BaseConstants.MESSAGE_UPDATE_SUCCESS, MessageDetail = string.Empty, Version = Configuration.VERSION};
                    return Json(result, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                var result = new { Success = false, Permission = permission_Item, ex.Message, MessageDetail = string.Empty, Version = Configuration.VERSION, Items = string.Empty, Total = 0 };
                return Json(result, JsonRequestBehavior.AllowGet);
            }
        }

        private bool InsertHistory(MERCY_Ctx p_db, Consumable p_data, string p_description)
        {
            bool result = false;

            try
            {
                using (MERCY_Ctx db = new MERCY_Ctx())
                {
                    var history = new Consumable_History
                    {
                        Consumable_Data = p_data.RecordId,
                        Instrument = p_data.Instrument,
                        CurrentQuantity = p_data.CurrentQuantity,
                        MinimumQuantity = p_data.MinimumQuantity,
                        UnitType = p_data.UnitType,
                        ReoderDays = p_data.ReoderDays,
                        PartName = p_data.PartName,
                        PartNumber = p_data.PartNumber,
                        MSDS_Code = p_data.MSDS_Code,
                        InputQuantity = p_data.InputQuantity,
                        OutputQuantity = p_data.OutputQuantity,
                        Price = p_data.Price,
                        Remark = p_data.Remark,
                        NotesFile = p_data.NotesFile,
                        NotesFile2 = p_data.NotesFile2,
                        PictureFile = p_data.PictureFile,
                        PictureFile2 = p_data.PictureFile2,

                        CreatedOn = p_data.CreatedOn,
                        CreatedBy = p_data.CreatedBy,
                        LastModifiedOn = p_data.LastModifiedOn,
                        LastModifiedBy = p_data.LastModifiedBy,

                        Description = p_description
                    };

                    db.Consumable_History.Add(history);
                    db.SaveChanges();

                    result = true;
                }
            }
            catch {}

            return result ;
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
                p_history = (p_history.Trim().ToLower()=="yes"?"1":"0");
                
                using (MERCY_Ctx db = new MERCY_Ctx())
                {
                    if (p_email == "yes")
                    {
                        var dataQuery =
                        (
                            from usr in db.Users
                            where usr.IsConsumable
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

                        string sql = string.Format(@"exec PROCESS_Reminder_Lab_Consumable '{0}', {1}", p_now, p_history);
                        command.CommandText = sql;
                        int count_period_1 = 0;
                        int count_period_2 = 0;

                        string style_td_border = "border-right:1px solid #dee2e6 !important;border-bottom:1px solid #dee2e6 !important;height:40px;padding: 0px !important;";

                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            while(reader.Read())
                            {
                                content_temp = string.Format(@"
                                                            <tr>
                                                                <td style='{7}'>## No_Line ##</td>
                                                                <td style='{7}'>{0}</td>
                                                                <td style='{7}'>{1}</td>
                                                                <td style='{7}'>{2}</td>
                                                                <td style='{7}'>{3}</td>
                                                                <td style='{7}'>{4}</td>
                                                                <td style='{7}text-align:right;'>{5}</td>
                                                                <td style='{7}text-align:right;'>{6}</td>
                                                            </tr>
                                                            ", reader["InstrumentName"].ToString()
                                                            , reader["PartName"].ToString()
                                                            , reader["PartNumber"].ToString()
                                                            , reader["MSDS_Code"].ToString()
                                                            , reader["UnitName"].ToString()
                                                            , String.Format("{0:n0}", OurUtility.ToInt32(reader["CurrentQuantity"].ToString()))
                                                            , String.Format("{0:n0}", OurUtility.ToInt32(reader["MinimumQuantity"].ToString()))
                                                            , style_td_border
                                                            );

                                if (reader["Period"].ToString() == config.Reminder_Consumable_1)
                                {
                                    count_period_1++;
                                    content_Periode_1 += content_temp.Replace("## No_Line ##", count_period_1.ToString());
                                }
                                else if (reader["Period"].ToString() == config.Reminder_Consumable_2)
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
                                                                        <td style='text-align:center;{1}'>Instrument</td>
                                                                        <td style='text-align:center;{1}'>Part Name</td>
                                                                        <td style='text-align:center;{1}'>Part Number</td>
                                                                        <td style='text-align:center;{1}'>MSDS Code</td>
                                                                        <td style='text-align:center;{1}'>UoM</td>
                                                                        <td style='text-align:center;{1}'>Current Qty</td>
                                                                        <td style='text-align:center;{1}'>Min. Qty</td>
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
                                                                        <td style='text-align:center;{1}'>Instrument</td>
                                                                        <td style='text-align:center;{1}'>Part Name</td>
                                                                        <td style='text-align:center;{1}'>Part Number</td>
                                                                        <td style='text-align:center;{1}'>MSDS Code</td>
                                                                        <td style='text-align:center;{1}'>UoM</td>
                                                                        <td style='text-align:center;{1}'>Current Qty</td>
                                                                        <td style='text-align:center;{1}'>Min. Qty</td>
                                                                    </tr>
                                                                    {0}
                                                                </table>
                                                                ", content_Periode_2, style_td_border);
                        }

                        connection.Close();
                    }
                }

                var result = "Status : Ok - scheduler Consumable";

                if (p_email == "yes")
                {
                    NotificationController notification = new NotificationController();
                    notification.InitializeController(this.Request.RequestContext);

                    string email_body = string.Empty;
                    string email_subject = config.Reminder_Consumable_Subject;

                    if ( ! string.IsNullOrEmpty(content_Periode_1))
                    {
                        email_body = System.IO.File.ReadAllText(notification.TemplateFolder + @"\email_template_consumable_1.html", Encoding.UTF8);
                        email_body = email_body.Replace("{TABLE}", content_Periode_1);

                        notification.Send(string.Empty, email_subject, email_address_MasterData, config.Reminder_Consumable_CC, email_body);
                    }

                    if ( ! string.IsNullOrEmpty(content_Periode_2))
                    {
                        email_body = System.IO.File.ReadAllText(notification.TemplateFolder + @"\email_template_consumable_2.html", Encoding.UTF8);
                        email_body = email_body.Replace("{TABLE}", content_Periode_2);

                        notification.Send(string.Empty, email_subject, email_address_MasterData, config.Reminder_Consumable_CC, email_body);
                    }
                }
                else
                {
                    result = string.Format(@"Status : Ok - scheduler Consumable
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
                                            , config.Reminder_Consumable_1, (OurUtility.ToInt32(config.Reminder_Consumable_1) > 1 ? "s" : ""), (string.IsNullOrEmpty(content_Periode_1) ? "&nbsp;&nbsp;&nbsp;Empty" : content_Periode_1)
                                            , config.Reminder_Consumable_2, (OurUtility.ToInt32(config.Reminder_Consumable_2) > 1 ? "s" : ""), (string.IsNullOrEmpty(content_Periode_2) ? "&nbsp;&nbsp;&nbsp;Empty" : content_Periode_2)
                                            );
                }

                return result;
            }
            catch (Exception ex)
            {
                var result = "Status : Error, Message : " + ex.Message;
                return result;
            }
        }
    }
}