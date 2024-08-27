using MERCY.Data.EntityFramework;
using MERCY.Web.BackEnd.Helpers;
using MERCY.Web.BackEnd.Models;
using MERCY.Web.BackEnd.Security;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Web.Mvc;
using Permission = MERCY.Web.BackEnd.Security.Permission;

namespace MERCY.Web.BackEnd.Areas.Api.Controllers
{
    public class CoalInventoryController : Controller
    {
        public void InitializeController(System.Web.Routing.RequestContext context)
        {
            base.Initialize(context);
        }

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

            // -- Actual code
            try
            {
                bool is_Show_User_Menu = OurUtility.ValueOf(Request, "u_menu").Equals("1");
                bool is_Show_User_Relation = OurUtility.ValueOf(Request, "u_relation").Equals("1");
                string company_code = OurUtility.ValueOf(Request, "company").Equals("all") ? string.Empty : OurUtility.ValueOf(Request, "company");
                string r_location = OurUtility.ValueOf(Request, "location").Equals("all") ? string.Empty : OurUtility.ValueOf(Request, "location");
                var period = OurUtility.ToDateTime(OurUtility.ValueOf(Request, "period"));
                var allDate = OurUtility.ValueOf(Request, "period") == "";
                int location = r_location == "" ? 0 : Convert.ToInt32(r_location);
                var config = new Configuration();
                var maxInputPeriod = Convert.ToInt32(config.COAL_INVENTORY_INPUT_PERIOD);
                var search = OurUtility.ValueOf(Request, "txt");

                using (MERCY_Ctx db = new MERCY_Ctx())
                {
                    var dataQuery =
                            (
                                from ci in db.CoalInventories
                                join rl in db.ROMLocations on ci.ROMLocationId equals rl.Id
                                where ci.CompanyCode.Contains(company_code)
                                        && (location == 0 || ci.ROMLocationId == location)
                                        && ((allDate && ci.Period.Month == DateTime.Now.Month) || ci.Period.Month == period.Month)
                                        && ((ci.CompanyCode.ToUpper().Contains(search)) || (rl.Name.ToUpper().Contains(search))
                                            || (ci.Tonnage.ToString().Contains(search)))
                                orderby rl.Name ascending
                                select new Model_View_Coal_Inventory
                                {
                                    CompanyCode = ci.CompanyCode,
                                    Code = rl.Code,
                                    Id = ci.Id,
                                    ROMLocationId = ci.ROMLocationId,
                                    ROMLocationName = rl.Name,
                                    Period = ci.Period,
                                    SurveyDate = ci.SurveyDate,
                                    StartTime = ci.StartTime,
                                    EndTime = ci.EndTime,
                                    StartTimeString = "",
                                    EndTimeString = "",
                                    Tonnage = ci.Tonnage,
                                    Status = ci.Status
                                }
                            );

                    var items = dataQuery.ToList();

                    items.ForEach(c =>
                    {
                        TimeSpan timespan = c.StartTime;
                        c.StartTimeString = String.Format("{0:00}:{1:00}", timespan.Hours, timespan.Minutes);

                        timespan = c.EndTime;
                        c.EndTimeString = String.Format("{0:00}:{1:00}", timespan.Hours, timespan.Minutes);

                        var details = db.CoalInventories.Find(c.Id).CoalInventoryDetails.ToList();

                        foreach (var detail in details)
                        {
                            var coalInventoryDetail = new Model_View_Coal_Inventory_Detail();
                            coalInventoryDetail.Id = detail.Id;
                            coalInventoryDetail.CoalInventoryId = detail.CoalInventoryId;
                            coalInventoryDetail.ROMLocationDetailName = detail.ROMLocationDetail.Name;
                            coalInventoryDetail.ROMLocationDetailId = detail.ROMLocationDetailId;
                            coalInventoryDetail.Volume = detail.Volume;
                            coalInventoryDetail.FactorScale = detail.FactorScale;
                            coalInventoryDetail.Remark = detail.Remark;
                            coalInventoryDetail.Tonnage = detail.Tonnage;
                            coalInventoryDetail.CreatedBy = detail.CreatedBy;
                            coalInventoryDetail.CreatedOn = detail.CreatedOn;
                            c.CoalInventoryDetails.Add(coalInventoryDetail);
                        }
                    });

                    var result = new { Success = true, Permission = permission_Item, User = UserX.Information(user, is_Show_User_Menu, is_Show_User_Relation), Message = string.Empty, MessageDetail = string.Empty, Version = Configuration.VERSION, Items = items, MaxInputPeriod = maxInputPeriod, Total = items.Count, Status = items?.FirstOrDefault()?.Status };
                    return Json(result, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                var result = new { Success = false, Permission = permission_Item, ex.Message, MessageDetail = string.Empty, Version = Configuration.VERSION, Items = string.Empty, Total = 0 };
                return Json(result, JsonRequestBehavior.AllowGet);
            }
        }

        public JsonResult GetById(int id)
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

            // -- Actual code
            try
            {
                bool is_Show_User_Menu = OurUtility.ValueOf(Request, "u_menu").Equals("1");
                bool is_Show_User_Relation = OurUtility.ValueOf(Request, "u_relation").Equals("1");

                using (MERCY_Ctx db = new MERCY_Ctx())
                {
                    var refCoalInventory = db.CoalInventories;

                    var dataQuery =
                            (
                                from ci in refCoalInventory
                                join rl in db.ROMLocations on ci.ROMLocationId equals rl.Id
                                where ci.Id == id
                                select new Model_View_Coal_Inventory
                                {
                                    CompanyCode = ci.CompanyCode,
                                    Id = ci.Id,
                                    ROMLocationId = rl.Id,
                                    ROMLocationName = rl.Name,
                                    Period = ci.Period,
                                    SurveyDate = ci.SurveyDate,
                                    StartTime = ci.StartTime,
                                    EndTime = ci.EndTime,
                                    StartTimeString = "",
                                    EndTimeString = "",
                                    Status = ci.Status
                                }
                            );

                    var item = dataQuery.SingleOrDefault();

                    TimeSpan timespan = item.StartTime;
                    item.StartTimeString = String.Format("{0:00}:{1:00}", timespan.Hours, timespan.Minutes);

                    timespan = item.EndTime;
                    item.EndTimeString = String.Format("{0:00}:{1:00}", timespan.Hours, timespan.Minutes);

                    var details = refCoalInventory.Find(id).CoalInventoryDetails.ToList();

                    foreach(var detail in details)
                    {
                        var coalInventoryDetail = new Model_View_Coal_Inventory_Detail();
                        coalInventoryDetail.Id = detail.Id;
                        coalInventoryDetail.CoalInventoryId = detail.CoalInventoryId;
                        coalInventoryDetail.ROMLocationDetailName = detail.ROMLocationDetail.Name;
                        coalInventoryDetail.ROMLocationDetailId = detail.ROMLocationDetailId;
                        coalInventoryDetail.Volume = detail.Volume;
                        coalInventoryDetail.FactorScale = detail.FactorScale;
                        coalInventoryDetail.Remark = detail.Remark;
                        coalInventoryDetail.Tonnage = detail.Tonnage;
                        coalInventoryDetail.CreatedBy = detail.CreatedBy;
                        coalInventoryDetail.CreatedOn = detail.CreatedOn;
                        item.CoalInventoryDetails.Add(coalInventoryDetail);
                    }

                    var dataAttachments =
                            (
                                from d in db.CoalInventoryAttachments
                                where d.CoalInventoryId == id
                                orderby d.FileName
                                select new
                                {
                                    d.Id,
                                    d.CoalInventoryId,
                                    d.OriginalName,
                                    d.FileName
                                }
                            );

                    var itemAttachments = dataAttachments.ToList();

                    var result = new { Success = true, Permission = permission_Item, User = UserX.Information(user, is_Show_User_Menu, is_Show_User_Relation), Message = string.Empty, MessageDetail = string.Empty, Version = Configuration.VERSION, Item = item, Attachments = itemAttachments };
                    return Json(result, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                var result = new { Success = false, Permission = permission_Item, ex.Message, MessageDetail = string.Empty, Version = Configuration.VERSION, Items = string.Empty, Total = 0 };
                return Json(result, JsonRequestBehavior.AllowGet);
            }
        }

        public JsonResult Create(CoalInventory coalInventory)
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
                var transaction = db.Database.BeginTransaction();
                try
                {
                    string validationMessage = InputValidation(db, coalInventory);

                    if (validationMessage != "Success")
                    {
                        var validationResult = new { Success = false, Permission = permission_Item, Message = validationMessage, MessageDetail = string.Empty, Version = Configuration.VERSION, Items = string.Empty, Total = 0 };
                        return Json(validationResult, JsonRequestBehavior.AllowGet);
                    }

                    var newCoalInventory = new CoalInventory
                    {
                        CompanyCode = coalInventory.CompanyCode,
                        ROMLocationId = coalInventory.ROMLocationId,
                        Period = coalInventory.Period,
                        SurveyDate = coalInventory.SurveyDate,
                        StartTime = coalInventory.StartTime,
                        EndTime = coalInventory.EndTime,
                        CreatedBy = user.UserId,
                        CreatedOn = DateTime.Now,
                        Tonnage = coalInventory.Tonnage
                    };

                    db.CoalInventories.Add(newCoalInventory);
                    db.SaveChanges();

                    CreateCoalInventoryDetail(newCoalInventory, db, coalInventory, user);

                    transaction.Commit();
                    var result = new { Success = true, Permission = permission_Item, Message = BaseConstants.MESSAGE_CREATE_SUCCESS, MessageDetail = string.Empty, Version = Configuration.VERSION, Id = newCoalInventory.Id };
                    return Json(result, JsonRequestBehavior.AllowGet);
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    transaction.Dispose();
                    var result = new { Success = false, Permission = permission_Item, ex.Message, MessageDetail = string.Empty, Version = Configuration.VERSION, Items = string.Empty, Total = 0 };
                    return Json(result, JsonRequestBehavior.AllowGet);
                }
            }
        }

        public JsonResult Edit(CoalInventory coalInventory)
        {
            // Check Permission based on Current Url
            UserX user = new UserX(Request);
            Permission_Item permission_Item = null;

            Permission.Check_API(Request, user, ref permission_Item);
            if (!permission_Item.Is_Edit)
            {
                var msg = new { Success = false, Permission = permission_Item, Message = Permission.ERROR_PERMISSION_ADD, MessageDetail = string.Empty, Version = Configuration.VERSION };
                return Json(msg, JsonRequestBehavior.AllowGet);
            }

            // -- Actual code
            using (MERCY_Ctx db = new MERCY_Ctx())
            {
                try
                {
                    string validationMessage = InputValidation(db, coalInventory);

                    if(validationMessage != "Success")
                    {
                        var validationResult = new { Success = false, Permission = permission_Item, Message = validationMessage, MessageDetail = string.Empty, Version = Configuration.VERSION, Items = string.Empty, Total = 0 };
                        return Json(validationResult, JsonRequestBehavior.AllowGet);
                    }

                    var newCoalInventory = db.CoalInventories.Find(coalInventory.Id);

                    newCoalInventory.CompanyCode = coalInventory.CompanyCode;
                    newCoalInventory.ROMLocationId = coalInventory.ROMLocationId;
                    newCoalInventory.Period = coalInventory.Period;
                    newCoalInventory.SurveyDate = coalInventory.SurveyDate;
                    newCoalInventory.StartTime = coalInventory.StartTime;
                    newCoalInventory.EndTime = coalInventory.EndTime;
                    newCoalInventory.Tonnage = coalInventory.Tonnage;
                    newCoalInventory.LastModifiedBy = user.UserId;
                    newCoalInventory.LastModifiedOn = DateTime.Now;

                    var coalInventoryDetailOld = db.CoalInventoryDetails.Where(x => x.CoalInventoryId == coalInventory.Id).ToList();
                    foreach (var item in coalInventoryDetailOld)
                    {
                        db.CoalInventoryDetails.Remove(item);
                    }

                    CreateCoalInventoryDetail(newCoalInventory, db, coalInventory, user);

                    db.SaveChanges();

                    var result = new { Success = true, Permission = permission_Item, Message = BaseConstants.MESSAGE_CREATE_SUCCESS, MessageDetail = string.Empty, Version = Configuration.VERSION };
                    return Json(result, JsonRequestBehavior.AllowGet);
                }
                catch (Exception ex)
                {
                    var result = new { Success = false, Permission = permission_Item, ex.Message, MessageDetail = string.Empty, Version = Configuration.VERSION, Items = string.Empty, Total = 0 };
                    return Json(result, JsonRequestBehavior.AllowGet);
                }
            }
        }

        public JsonResult GetRomLocation(string companyCode)
        {
            // Check Permission based on Current Url
            UserX user = new UserX(Request);
            Permission_Item permission_Item = null;

            //Not necessary checking permission, just logging user is enough
            if (user.UserId <= 0)
            {
                var msg = new { Success = false, Permission = permission_Item, Message = Permission.ERROR_PERMISSION_READ + " [not Login]", MessageDetail = string.Empty, Version = Configuration.VERSION };
                return Json(msg, JsonRequestBehavior.AllowGet);
            }

            using (MERCY_Ctx db = new MERCY_Ctx())
            {
                var dataQuery =
                        (
                            from rl in db.ROMLocations
                            where rl.CompanyCode == companyCode
                            orderby rl.Name
                            select new
                            {
                                rl.Id
                                ,
                                rl.Name
                            }
                        );

                var items = dataQuery.ToList();

                var result = new { Success = true, Permission = permission_Item, Message = string.Empty, MessageDetail = string.Empty, Version = Configuration.VERSION, Items = items, Total = items.Count };
                return Json(result, JsonRequestBehavior.AllowGet);
            }
        }

        public JsonResult GetRomLocationDetail(int romLocationId)
        {
            // Check Permission based on Current Url
            UserX user = new UserX(Request);
            Permission_Item permission_Item = null;

            //Not necessary checking permission, just logging user is enough
            if (user.UserId <= 0)
            {
                var msg = new { Success = false, Permission = permission_Item, Message = Permission.ERROR_PERMISSION_READ + " [not Login]", MessageDetail = string.Empty, Version = Configuration.VERSION };
                return Json(msg, JsonRequestBehavior.AllowGet);
            }

            using (MERCY_Ctx db = new MERCY_Ctx())
            {
                var dataQuery =
                        (
                            from rld in db.ROMLocationDetails
                            where rld.ROMLocationId == romLocationId
                            orderby rld.Name
                            select new
                            {
                                rld.Id
                                ,
                                rld.Name
                            }
                        );

                var items = dataQuery.ToList();

                var result = new { Success = true, Permission = permission_Item, Message = string.Empty, MessageDetail = string.Empty, Version = Configuration.VERSION, Items = items, Total = items.Count };
                return Json(result, JsonRequestBehavior.AllowGet);
            }
        }

        void CreateCoalInventoryDetail(CoalInventory savedCoalInventory, MERCY_Ctx db, CoalInventory coalInventory, UserX user)
        {
            foreach (var item in coalInventory.CoalInventoryDetails)
            {
                var toBeSavedCoalInventoryDetail = new CoalInventoryDetail
                {
                    CoalInventoryId = savedCoalInventory.Id,
                    ROMLocationDetailId = item.ROMLocationDetailId,
                    Volume = item.Volume,
                    FactorScale = item.FactorScale,
                    Tonnage = item.Tonnage,
                    Remark = item.Remark,
                    CreatedBy = user.UserId,
                    CreatedOn = DateTime.Now
                };
                db.CoalInventoryDetails.Add(toBeSavedCoalInventoryDetail);
            }
            db.SaveChanges();
        }

        public JsonResult Attachment(FormCollection p_collection)
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

                    Process_Attachements(db, user, p_collection);

                    var result = new { Success = true, Permission = permission_Item, Message = BaseConstants.MESSAGE_CREATE_SUCCESS, MessageDetail = string.Empty, Version = Configuration.VERSION };
                    return Json(result, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                var result = new { Success = false, Permission = permission_Item, ex.Message, MessageDetail = string.Empty, Version = Configuration.VERSION, Items = string.Empty, Total = 0 };
                return Json(result, JsonRequestBehavior.AllowGet);
            }
        }

        public string InputValidation(MERCY_Ctx db, CoalInventory coalInventory)
        {
            try
            {
                var refCoalInventory = db.CoalInventories.ToList();
                var romLocationDetails = db.ROMLocationDetails.Where(x => x.ROMLocationId == coalInventory.ROMLocationId).ToList();

                foreach (var detail in coalInventory.CoalInventoryDetails)
                {
                    if (!romLocationDetails.Any(x => x.Id == detail.ROMLocationDetailId))
                    {
                        return "ROM location detail does not match selected ROM location.";
                    }
                }

                var romLocationDetailCount = coalInventory.CoalInventoryDetails.Select(x => x.ROMLocationDetailId).Distinct().Count();

                if(coalInventory.CoalInventoryDetails.Count() != romLocationDetailCount)
                {
                    return "Cannot insert duplicate ROM location detail in one report. Please choose another ROM location detail.";
                }

                if (coalInventory.CoalInventoryDetails.Count() == 0)
                {
                    return "Coal inventory detail cannot be empty, please add at least one record in the detail section.";
                }
                if (refCoalInventory.Any(x => (x.ROMLocationId == coalInventory.ROMLocationId) && (x.Period.Month == DateTime.Now.AddMonths(-1).Month) && (x.CompanyCode == coalInventory.CompanyCode) && (x.Id != coalInventory.Id)))
                {
                    return "ROM Location is already reported on the selected period. Please choose another ROM location.";
                }
                else if(coalInventory.Period.Month != DateTime.Now.AddMonths(-1).Month)
                {
                    return "Period can only be last month. Please choose another period.";
                }
                else
                {
                    return "Success";
                }
            }
            catch(Exception ex)
            {
                return ex.Message;
            }
        }

        private void Process_Attachements(MERCY_Ctx db, UserX executedBy, FormCollection p_collection)
        {
            int file_Counter = OurUtility.ToInt32(OurUtility.ValueOf(p_collection, "file_Counter"));
            int coalInventoryId = OurUtility.ToInt32(OurUtility.ValueOf(p_collection, "coal_inventory_id"));

            string fileName = string.Empty;
            string fileName2 = string.Empty;
            string msg = string.Empty;

            for (int i = 1; i <= file_Counter; i++)
            {
                OurUtility.Upload(Request, "fileInput_Upload_" + i.ToString(), UploadFolder, ref fileName, ref fileName2, ref msg);

                try
                {
                    // -- Add Record
                    var data = new CoalInventoryAttachment
                    {
                        CoalInventoryId = coalInventoryId,
                        OriginalName = fileName,
                        FileName = fileName2,

                        CreatedOn = DateTime.Now,
                        CreatedBy = executedBy.UserId
                    };

                    db.CoalInventoryAttachments.Add(data);
                    db.SaveChanges();
                }
                catch(Exception ex)
                {
                    throw new Exception(ex.Message);
                }
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
                        CoalInventoryAttachment data = db.CoalInventoryAttachments.Find(recordId);

                        FileInfo fi = new FileInfo(UploadFolder + "/" + data.FileName);
                        fi.Delete();

                        db.CoalInventoryAttachments.Remove(data);
                        db.SaveChanges();

                    }
                    catch(Exception ex)
                    {
                        throw new Exception(ex.Message);
                    }
                }
            }
            catch { }
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

        public JsonResult Finalize()
        {
            // Check Permission based on Current Url
            UserX user = new UserX(Request);
            Permission_Item permission_Item = null;

            Permission.Check_API(Request, user, ref permission_Item);
            if (!permission_Item.Is_Edit)
            {
                var msg = new { Success = false, Permission = permission_Item, Message = Permission.ERROR_PERMISSION_ADD, MessageDetail = string.Empty, Version = Configuration.VERSION };
                return Json(msg, JsonRequestBehavior.AllowGet);
            }

            // -- Actual code
            using (MERCY_Ctx db = new MERCY_Ctx())
            {
                try
                {
                    var period = Convert.ToDateTime(OurUtility.ValueOf(Request, "period"));
                    var companyCode = OurUtility.ValueOf(Request, "company");

                    var finalCoalInventory = db.CoalInventories.Where(x => x.Period.Month == period.Month
                                             && x.Period.Year == period.Year && x.CompanyCode == companyCode).ToList();
                    finalCoalInventory.ForEach((fci) => fci.Status = "Finalized");

                    db.SaveChanges();

                    var result = new { Success = true, Permission = permission_Item, Message = BaseConstants.MESSAGE_CREATE_SUCCESS, MessageDetail = string.Empty, Version = Configuration.VERSION };
                    return Json(result, JsonRequestBehavior.AllowGet);
                }
                catch (Exception ex)
                {
                    var result = new { Success = false, Permission = permission_Item, ex.Message, MessageDetail = string.Empty, Version = Configuration.VERSION, Items = string.Empty, Total = 0 };
                    return Json(result, JsonRequestBehavior.AllowGet);
                }
            }
        }

        public JsonResult DailySyncBeginningStock()
        {
            // Check Permission based on Current Url
            UserX user = new UserX(Request);

            // -- Not necessary checking Permission
            // -- just Logging User: is enough
            if (user.UserId <= 0)
            {
                var msg = new { Success = false, Message = Permission.ERROR_PERMISSION_READ + " [not Login]", MessageDetail = string.Empty, Version = Configuration.VERSION };
                return Json(msg, JsonRequestBehavior.AllowGet);
            }

            // fetch data daily at 07.00
            // fetch data for each ROM at end of month (current month - 1)

            string connectionString = System.Configuration.ConfigurationManager.ConnectionStrings["MERCY_Beginning_Stock_Ctx"].ConnectionString;
            var roms = new List<RomDto> { };
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                var sql = string.Format(@"
                                            WITH CTE AS (
                                              SELECT
                                                Date,
                                                rom_name,
                                                companycode,
                                                alias1,
                                                Ton,
                                                DateCreated,
                                                ROW_NUMBER() OVER (PARTITION BY Date, rom_name, companycode, alias1 ORDER BY DateCreated DESC) AS rn
                                              FROM dwh.DIM_SURVEY
                                              WHERE 
                                            DATEPART(YEAR, Date) = {0}
                                            AND DATEPART(MONTH, Date) = {1}
                                            AND Category = 'Opening_Balance'
                                            )

                                            SELECT Date as period, rom_name as romName, companycode as companyCode, alias1 as source, Ton as ton, DateCreated as createdOn
                                            FROM CTE
                                            WHERE rn = 1;
                                            ", DateTime.Now.Year, DateTime.Now.AddMonths(-1).Month);
                var command = connection.CreateCommand();
                command.CommandText = sql;
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        roms.Add(new RomDto
                        {
                            romName = reader["romName"].ToString(),
                            period = new DateTime(DateTime.Parse(reader["period"].ToString()).Year, DateTime.Parse(reader["period"].ToString()).Month, 1),
                            createdOn = DateTime.Parse(reader["createdOn"].ToString()),
                            companyCode = reader["companyCode"].ToString(),
                            source = reader["source"].ToString(),
                            ton = decimal.Parse(reader["ton"].ToString()),
                        });
                    }
                }
                connection.Close();
            }

            foreach(var rom in roms)
            {
                CreateCoalInventorySync(rom);
            }

            return Json(new { data = roms }, JsonRequestBehavior.AllowGet);
        }

        private class RomDto {
           public string romName { get; set; }
           public DateTime period { get; set; }
           public DateTime createdOn { get; set; }
           public string companyCode { get; set; }
           public string source { get; set; }
           public decimal ton { get; set; }
        }

        private void CreateCoalInventorySync(RomDto data)
        {
            using (MERCY_Ctx db = new MERCY_Ctx())
            {
                var transaction = db.Database.BeginTransaction();
                try
                {
                    // get rom location id from source
                    var romLocationId = db.ROMLocations.Where(u => data.source.Contains(u.Code) && u.CompanyCode == data.companyCode).Select(u => u.Id).FirstOrDefault();
                    if (romLocationId == 0)
                        return;

                    var romLocationDetailId = db.ROMLocationDetails.Where(u => u.Name == data.romName).Select(u => u.Id).FirstOrDefault();
                    if (romLocationDetailId == 0)
                        return;

                    // fetching existing data 
                    var coalInventory = db.CoalInventories.Where(u => u.ROMLocationId == romLocationId && u.Period == data.period).FirstOrDefault();
                    if (coalInventory == null)
                    {
                        // initial new coal inventory data
                        coalInventory = new CoalInventory
                        {
                            CompanyCode = data.companyCode,
                            ROMLocationId = romLocationId,
                            Period = new DateTime(data.period.Year, data.period.Month, 1),
                            SurveyDate = new DateTime(data.period.Year, data.period.Month, 25), // as default 
                            StartTime = new TimeSpan(8, 0, 0),  // as default
                            EndTime = new TimeSpan(12, 0, 0), // as default
                            CreatedOn = data.createdOn,
                            Tonnage = data.ton,
                        };

                        // inital new coal inventory detail (rom)
                        coalInventory.CoalInventoryDetails.Add(new CoalInventoryDetail
                        {
                            CoalInventoryId = coalInventory.Id,
                            ROMLocationDetailId = romLocationDetailId,
                            Volume = 1,
                            FactorScale = 1,
                            Tonnage = data.ton,
                            CreatedOn = data.createdOn,
                            Remark = "Automatics from mine market", // as default
                        });
                        db.CoalInventories.Add(coalInventory);
                        db.SaveChanges();
                    }
                    else
                    {
                        // If a coal inventory exists, created by the user and is newer than existing data
                        // then update the total tonnage and either replace the tonnage or create new data
                        var existingCoalInventoryDetail = coalInventory.CoalInventoryDetails.Where(u => u.ROMLocationDetailId == romLocationDetailId)
                                                                                            .FirstOrDefault();
                        if (existingCoalInventoryDetail != null ) 
                        {
                            if(existingCoalInventoryDetail.CreatedOn < data.createdOn)
                            {
                                // adjust total tonnage
                                // decrease total tonnage by exisiting rom
                                // that will be replaced with newer rom tonnage
                                coalInventory.Tonnage = coalInventory.Tonnage - existingCoalInventoryDetail.Tonnage + data.ton;
                                existingCoalInventoryDetail.Tonnage = data.ton;
                                existingCoalInventoryDetail.Remark = "Automatics from mine market";
                                existingCoalInventoryDetail.CreatedOn = data.createdOn;
                            }
                            // ignore if existing data is newer
                        }
                        else
                        {
                            // increase tonnage by new rom ton
                            coalInventory.Tonnage = coalInventory.Tonnage + data.ton;
                            // create new one
                            coalInventory.CoalInventoryDetails.Add(new CoalInventoryDetail
                            {
                                CoalInventoryId = coalInventory.Id,
                                ROMLocationDetailId = romLocationDetailId,
                                Volume = 1,
                                FactorScale = 1,
                                Tonnage = data.ton,
                                CreatedOn = data.createdOn,
                                Remark = "Automatics from mine market", // as default
                            });
                        }
                        db.SaveChanges();
                    }

                    transaction.Commit();
                }
                catch (Exception)
                {
                    transaction.Rollback();
                    transaction.Dispose();
                }
            }
        }
    }
}