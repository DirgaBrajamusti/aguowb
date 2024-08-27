using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

using MERCY.Data.EntityFramework;
using MERCY.Web.BackEnd.Models;
using MERCY.Web.BackEnd.Helpers;
using MERCY.Web.BackEnd.Security;
using Permission = MERCY.Web.BackEnd.Security.Permission;

using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;

using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.Text.RegularExpressions;

namespace MERCY.Web.BackEnd.Areas.Api.Controllers
{
    public class BargeQualityPlanController : Controller
    {
        public JsonResult Index()
        {
            return Json("BargeQualityPlanController", JsonRequestBehavior.AllowGet);
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

        public JsonResult ChooseFile()
        {
            // Check Permission based on Current Url
            UserX user = new UserX(Request);
            Permission_Item permission_Item = null;

            Permission.Check_API(Request, user, ref permission_Item);
            if ( ! permission_Item.Is_Add)
            {
                var msgX = new { Success = false, Permission = permission_Item, Message = Permission.ERROR_PERMISSION_ADD, MessageDetail = string.Empty, Version = Configuration.VERSION };
                return Json(msgX, JsonRequestBehavior.AllowGet);
            }

            string fileName = string.Empty;
            string fileName2 = string.Empty;
            string msg = string.Empty;
            long id_File_Physical = 0;

            if ( ! OurUtility.Upload(Request, UploadFolder, ref fileName, ref fileName2, ref msg))
            {
                var msgX = new { Success = false, Permission = permission_Item, Message = msg, MessageDetail = string.Empty, Version = Configuration.VERSION, Id = -1 };
                return Json(msgX, JsonRequestBehavior.AllowGet);

            }

            if ( ! OurUtility.Upload_Record(user, fileName, fileName2, OurUtility.UPLOAD_BargeQualityPlan, ref id_File_Physical, ref msg))
            {
                var msgX = new { Success = false, Permission = permission_Item, Message = msg, MessageDetail = string.Empty, Version = Configuration.VERSION, Id = -1 };
                return Json(msgX, JsonRequestBehavior.AllowGet);
            }

            var result = new { Success = true, Permission = permission_Item, Message = BaseConstants.MESSAGE_CREATE_SUCCESS, MessageDetail = string.Empty, Version = Configuration.VERSION, Data = id_File_Physical };
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        public JsonResult ParsingContent()
        {
            // Check Permission based on Current Url
            UserX user = new UserX(Request);
            Permission_Item permission_Item = null;

            Permission.Check_API(Request, user, ref permission_Item);
            if ( ! permission_Item.Is_Add)
            {
                var msgX = new { Success = false, Permission = permission_Item, Message = Permission.ERROR_PERMISSION_ADD, MessageDetail = string.Empty, Version = Configuration.VERSION };
                return Json(msgX, JsonRequestBehavior.AllowGet);
            }

            long id_File_Physical = OurUtility.ToInt64(Request[".id"]);
            int site = OurUtility.ToInt32(Request["site"]);

            string message = string.Empty;

            try
            {
                using (MERCY_Ctx db = new MERCY_Ctx())
                {
                    var dataQuery =
                            (
                                from d in db.TEMPORARY_File
                                where d.RecordId == id_File_Physical
                                select d
                            );

                    var dataFile = dataQuery.SingleOrDefault();

                    string fileName = dataFile.Link;

                    // Open the spreadsheet document for read-only access.
                    using (SpreadsheetDocument document = SpreadsheetDocument.Open(UploadFolder + fileName, false))
                    {
                        // Retrieve a reference to the workbook part.
                        WorkbookPart wbPart = document.WorkbookPart;

                        WorksheetPart wsPart = null;

                        int baris = 1;
                        string v_TripNo = "";
                        string v_Vessel = "";
                        string v_Buyer = "";
                        string v_LaycanFrom = "";
                        string v_LaycanTo = "";
                        bool is_Hidden = false;

                        string status = string.Empty;
                        List<Unique_Barge_Quality_Plan> keys_Unique = new List<Unique_Barge_Quality_Plan>();
                        int no = 0;
                        decimal temp = 0M;

                        TEMPORARY_Barge_Quality_Plan_Header header = null;
                        TEMPORARY_Barge_Quality_Plan detail = null;

                        foreach (Sheet sheet in wbPart.Workbook.Sheets)
                        {
                            baris = 2;

                            is_Hidden = (sheet.State != null
                                            && sheet.State.HasValue
                                            && (sheet.State.Value == SheetStateValues.Hidden
                                                    || sheet.State.Value == SheetStateValues.VeryHidden));
                            
                            // Skip worksheet yang "Hidden"
                            if (is_Hidden) continue;

                            try
                            {
                                wsPart = (WorksheetPart)(wbPart.GetPartById(sheet.Id));

                                string keyword = ExcelProcessing.GetCellValue(wbPart, wsPart, "B" + baris.ToString());
                                string format = "dd/MM/yyyy";
                                CultureInfo provider = CultureInfo.InvariantCulture;
                                string[] splittedKeyword = Regex.Split(keyword, "(.*)\\)(.*) to (.*) with laycan (.*)-(.*)").Where(w => !w.Equals(string.Empty)).ToArray();

                                v_TripNo = splittedKeyword[0].Trim();
                                v_Vessel = splittedKeyword[1].Trim();
                                v_Buyer = splittedKeyword[2].Trim();
                                v_LaycanFrom = DateTime.ParseExact($"{splittedKeyword[3].Trim()}/{DateTime.Now.Year}", format, provider).ToString("yyyy-MM-dd");
                                v_LaycanTo = DateTime.ParseExact($"{splittedKeyword[4].Trim()}/{DateTime.Now.Year}", format, provider).ToString("yyyy-MM-dd");

                                header = new TEMPORARY_Barge_Quality_Plan_Header
                                {
                                    File_Physical = id_File_Physical,
                                    Site = site,
                                    Sheet = sheet.Name.ToString(),
                                    TripNo = v_TripNo,
                                    Vessel = v_Vessel,
                                    Buyer = v_Buyer,
                                    Laycan_From = v_LaycanFrom,
                                    Laycan_To = v_LaycanTo,

                                    CreatedOn = DateTime.Now,
                                    CreatedBy = user.UserId
                                };
                                header.CreatedOn_Date_Only = header.CreatedOn.ToString("yyyy-MM-dd");

                                db.TEMPORARY_Barge_Quality_Plan_Header.Add(header);
                                db.SaveChanges();

                                baris = 4;

                                while (true)
                                {
                                    no = OurUtility.ToInt32(ExcelProcessing.GetCellValue(wbPart, wsPart, "B" + baris.ToString()));
                                    if (no <= 0) break;

                                    detail = new TEMPORARY_Barge_Quality_Plan
                                    {
                                        Header = header.RecordId,

                                        Barge_Ton_plan = ExcelProcessing.GetCellValue(wbPart, wsPart, "C" + baris.ToString()),
                                        Barge_Ton_actual = ExcelProcessing.GetCellValue(wbPart, wsPart, "D" + baris.ToString()),
                                        TM_plan = ExcelProcessing.GetCellValue(wbPart, wsPart, "E" + baris.ToString()),
                                        TM_actual = ExcelProcessing.GetCellValue(wbPart, wsPart, "F" + baris.ToString()),
                                        M_plan = ExcelProcessing.GetCellValue(wbPart, wsPart, "G" + baris.ToString()),
                                        M_actual = ExcelProcessing.GetCellValue(wbPart, wsPart, "H" + baris.ToString()),
                                        ASH_plan = ExcelProcessing.GetCellValue(wbPart, wsPart, "I" + baris.ToString()),
                                        ASH_actual = ExcelProcessing.GetCellValue(wbPart, wsPart, "J" + baris.ToString()),
                                        TS_plan = ExcelProcessing.GetCellValue(wbPart, wsPart, "K" + baris.ToString()),
                                        TS_actual = ExcelProcessing.GetCellValue(wbPart, wsPart, "L" + baris.ToString()),
                                        CV_ADB_plan = ExcelProcessing.GetCellValue(wbPart, wsPart, "M" + baris.ToString()),
                                        CV_ADB_actual = ExcelProcessing.GetCellValue(wbPart, wsPart, "N" + baris.ToString())
                                    };

                                    //detail.CV_AR_plan = ExcelProcessing.GetCellValue(wbPart, wsPart, "O" + baris.ToString());
                                    temp = (OurUtility.ToDecimal(detail.CV_ADB_plan) * (100 - OurUtility.ToDecimal(detail.TM_plan)) / (100 - OurUtility.ToDecimal(detail.M_plan)));
                                    detail.CV_AR_plan = string.Format("{0:N2}", OurUtility.Round(temp, 2));

                                    //detail.CV_AR_actual = ExcelProcessing.GetCellValue(wbPart, wsPart, "P" + baris.ToString());
                                    temp = (OurUtility.ToDecimal(detail.CV_ADB_actual) * (100 - OurUtility.ToDecimal(detail.TM_actual)) / (100 - OurUtility.ToDecimal(detail.M_actual)));
                                    detail.CV_AR_actual = string.Format("{0:N2}", OurUtility.Round(temp, 2));

                                    detail.Product = ExcelProcessing.GetCellValue(wbPart, wsPart, "Q" + baris.ToString());
                                    detail.TugName = ExcelProcessing.GetCellValue(wbPart, wsPart, "R" + baris.ToString());
                                    detail.BargeName = ExcelProcessing.GetCellValue(wbPart, wsPart, "S" + baris.ToString());

                                    detail.CreatedOn = DateTime.Now;
                                    detail.CreatedBy = user.UserId;
                                    detail.CreatedOn_Date_Only = detail.CreatedOn.ToString("yyyy-MM-dd");

                                    Check_Validation(detail, keys_Unique);

                                    // init Status, start with "InValid"
                                    status = "Invalid";

                                    if (detail.Barge_Ton_plan_isvalid
                                            && detail.Barge_Ton_actual_isvalid
                                            && detail.TM_plan_isvalid
                                            && detail.TM_actual_isvalid
                                            && detail.M_plan_isvalid
                                            && detail.M_actual_isvalid
                                            && detail.ASH_plan_isvalid
                                            && detail.ASH_actual_isvalid
                                            && detail.TS_plan_isvalid
                                            && detail.TS_actual_isvalid
                                            && detail.CV_ADB_plan_isvalid
                                            && detail.CV_ADB_actual_isvalid
                                            && detail.CV_AR_plan_isvalid
                                            && detail.CV_AR_actual_isvalid
                                            && detail.Product_isvalid
                                            && detail.TugName_isvalid
                                            && detail.BargeName_isvalid
                                    )
                                    {
                                        status = "New";
                                    }

                                    detail.Status = status;

                                    db.TEMPORARY_Barge_Quality_Plan.Add(detail);
                                    db.SaveChanges();

                                    baris++;

                                    // for Next "Unique"
                                    try
                                    {
                                        Unique_Barge_Quality_Plan a = new Unique_Barge_Quality_Plan
                                        {
                                            CreatedOn_Date_Only = detail.CreatedOn_Date_Only,
                                            Barge = detail.BargeName,
                                            Tug = detail.TugName
                                        };
                                        keys_Unique.Add(a);
                                    }
                                    catch {}
                                }

                                // Handling Status: Update
                                try
                                {
                                    bool isNeed_Update = false;

                                    // ambil semua data yang baru di Insert diatas
                                    // yang Status: New
                                    List<TEMPORARY_Barge_Quality_Plan> recordSet = (
                                                                                from dt in db.TEMPORARY_Barge_Quality_Plan
                                                                                orderby dt.RecordId
                                                                                where dt.Header == header.RecordId //data yang baru di Insert diatas
                                                                                        && dt.Status == "New"
                                                                                select dt
                                                                             ).ToList();

                                    // proses RecordSet
                                    foreach (TEMPORARY_Barge_Quality_Plan row in recordSet)
                                    {
                                        // cari data "sebelumnya" di Table "Actual Upload"
                                        // punya Unique Key yang sama
                                        if (db.UPLOAD_Barge_Quality_Plan.Where(d => d.CreatedOn_Date_Only == row.CreatedOn_Date_Only 
                                                                                    && d.TugName == row.TugName
                                                                                    && d.BargeName == row.BargeName // punya Unique yang sama   
                                                                          ).Any())
                                        {
                                            row.Status = "Update";

                                            isNeed_Update = true;
                                        }
                                    }

                                    // update if Necessary
                                    if (isNeed_Update)
                                    {
                                        db.SaveChanges();
                                    }
                                }
                                catch {}
                            }
                            catch {}
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                var msgX = new { Success = false, Permission = permission_Item, ex.Message, MessageDetail = string.Empty, Version = Configuration.VERSION};
                return Json(msgX, JsonRequestBehavior.AllowGet);
            }

            var result = new { Success = true, Permission = permission_Item, Message = BaseConstants.MESSAGE_CREATE_SUCCESS + ", detail " + message, MessageDetail = string.Empty, Version = Configuration.VERSION, Data = id_File_Physical };
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        public JsonResult DisplayContent()
        {
            // Check Permission based on Current Url
            UserX user = new UserX(Request);
            Permission_Item permission_Item = null;

            Permission.Check_API(Request, user, ref permission_Item);
            if ( ! permission_Item.Is_Add)
            {
                var msg = new { Success = false, Permission = permission_Item, Message = Permission.ERROR_PERMISSION_READ, MessageDetail = string.Empty, Version = Configuration.VERSION };
                return Json(msg, JsonRequestBehavior.AllowGet);
            }

            long id = OurUtility.ToInt64(OurUtility.ValueOf(Request, ".id"));

            // -- Actual code
            try
            {
                using (MERCY_Ctx db = new MERCY_Ctx())
                {
                    var dataQuery =
                            (
                                from h in db.TEMPORARY_Barge_Quality_Plan_Header
                                join d in db.TEMPORARY_Barge_Quality_Plan on h.RecordId equals d.Header
                                where h.File_Physical == id
                                orderby h.RecordId, d.RecordId
                                select new Model_TEMPORARY_Barge_Quality_Plan
                                {
                                    Status = d.Status
                                    , Sheet = h.Sheet
                                    , TripNo = h.TripNo
                                    , Vessel = h.Vessel
                                    , Buyer = h.Buyer
                                    , LaycanFrom = h.Laycan_From
                                    , LaycanTo = h.Laycan_To

                                    , Barge_Ton_plan = d.Barge_Ton_plan
                                    , Barge_Ton_actual = d.Barge_Ton_actual
                                    , TM_plan = d.TM_plan
                                    , TM_actual = d.TM_actual
                                    , M_plan = d.M_plan
                                    , M_actual = d.M_actual
                                    , ASH_plan = d.ASH_plan
                                    , ASH_actual = d.ASH_actual
                                    , TS_plan = d.TS_plan
                                    , TS_actual = d.TS_actual
                                    , CV_ADB_plan = d.CV_ADB_plan
                                    , CV_ADB_actual = d.CV_ADB_actual
                                    , CV_AR_plan = d.CV_AR_plan
                                    , CV_AR_actual = d.CV_AR_actual
                                    , Product = d.Product
                                    , TugName = d.TugName
                                    , BargeName = d.BargeName

                                    , Barge_Ton_plan_isvalid = d.Barge_Ton_plan_isvalid
                                    , Barge_Ton_actual_isvalid = d.Barge_Ton_actual_isvalid
                                    , TM_plan_isvalid = d.TM_plan_isvalid
                                    , TM_actual_isvalid = d.TM_actual_isvalid
                                    , M_plan_isvalid = d.M_plan_isvalid
                                    , M_actual_isvalid = d.M_actual_isvalid
                                    , ASH_plan_isvalid = d.ASH_plan_isvalid
                                    , ASH_actual_isvalid = d.ASH_actual_isvalid
                                    , TS_plan_isvalid = d.TS_plan_isvalid
                                    , TS_actual_isvalid = d.TS_actual_isvalid
                                    , CV_ADB_plan_isvalid = d.CV_ADB_plan_isvalid
                                    , CV_ADB_actual_isvalid = d.CV_ADB_actual_isvalid
                                    , CV_AR_plan_isvalid = d.CV_AR_plan_isvalid
                                    , CV_AR_actual_isvalid = d.CV_AR_actual_isvalid
                                    , Product_isvalid = d.Product_isvalid
                                    , TugName_isvalid = d.TugName_isvalid
                                    , BargeName_isvalid = d.BargeName_isvalid
                                }
                            );

                    var items = dataQuery.ToList();

                    items.ForEach(c =>
                    {
                        c.Barge_Ton_plan_Str = Value_Of(c.Barge_Ton_plan_isvalid, c.Barge_Ton_plan);
                        c.Barge_Ton_actual_Str = Value_Of(c.Barge_Ton_actual_isvalid, c.Barge_Ton_actual);
                        c.TM_plan_Str = Value_Of(c.TM_plan_isvalid, c.TM_plan);
                        c.TM_actual_Str = Value_Of(c.TM_actual_isvalid, c.TM_actual);
                        c.M_plan_Str = Value_Of(c.M_plan_isvalid, c.M_plan);
                        c.M_actual_Str = Value_Of(c.M_actual_isvalid, c.M_actual);
                        c.ASH_plan_Str = Value_Of(c.ASH_plan_isvalid, c.ASH_plan);
                        c.ASH_actual_Str = Value_Of(c.ASH_actual_isvalid, c.ASH_actual);
                        c.TS_plan_Str = Value_Of(c.TS_plan_isvalid, c.TS_plan);
                        c.TS_actual_Str = Value_Of(c.TS_actual_isvalid, c.TS_actual);
                        c.CV_ADB_plan_Str = Value_Of_0(c.CV_ADB_plan_isvalid, c.CV_ADB_plan);
                        c.CV_ADB_actual_Str = Value_Of_0(c.CV_ADB_actual_isvalid, c.CV_ADB_actual);
                        c.CV_AR_plan_Str = Value_Of_0(c.CV_AR_plan_isvalid, c.CV_AR_plan);
                        c.CV_AR_actual_Str = Value_Of_0(c.CV_AR_actual_isvalid, c.CV_AR_actual);
                    });

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

        public JsonResult Save()
        {
            // Check Permission based on Current Url
            UserX user = new UserX(Request);
            Permission_Item permission_Item = null;

            Permission.Check_API(Request, user, ref permission_Item);
            if ( ! permission_Item.Is_Add)
            {
                var msgX = new { Success = false, Permission = permission_Item, Message = Permission.ERROR_PERMISSION_READ, MessageDetail = string.Empty, Version = Configuration.VERSION };
                return Json(msgX, JsonRequestBehavior.AllowGet);
            }

            long id = OurUtility.ToInt64(OurUtility.ValueOf(Request, ".id"));

            object result = null;

            try
            {
                using (MERCY_Ctx db = new MERCY_Ctx())
                {
                    using (var connection = new SqlConnection(db.Database.Connection.ConnectionString))
                    {
                        var uploadSuccess = false;
                        connection.Open();

                        SqlCommand command = connection.CreateCommand();

                        command.CommandText = string.Format(@"exec UPLOAD_ACTUAL_Barge_Quality_Plan {0}", id);

                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                uploadSuccess = OurUtility.ValueOf(reader, "cStatus").ToUpper() == "OK";
                                result = new { Success = OurUtility.ValueOf(reader, "cStatus").ToUpper() == "OK", Count = OurUtility.ValueOf(reader, "cCount"), Count2 = OurUtility.ValueOf(reader, "cCount2"), Permission = permission_Item, Message = OurUtility.ValueOf(reader, "cMessage"), MessageDetail = string.Empty, Version = Configuration.VERSION };
                            }
                        }

                        if (uploadSuccess)
                        {
                            SqlCommand command2 = connection.CreateCommand();
                            command2.CommandText = string.Format(@"exec UPLOAD_ACTUAL_Barge_Quality_Plan_Header {0}", id);
                            command2.ExecuteNonQuery();
                        }

                        connection.Close();
                    }
                }

                return Json(result, JsonRequestBehavior.AllowGet);
            }
            catch { }

            var msg = new { Success = true, Permission = permission_Item, Message = string.Empty, MessageDetail = string.Empty, Version = Configuration.VERSION };
            return Json(msg, JsonRequestBehavior.AllowGet);
        }

        private static void Check_Validation(TEMPORARY_Barge_Quality_Plan p_detail, List<Unique_Barge_Quality_Plan> p_keys_Unique)
        {
            // Key: Valid, jika belum pernah ada
            p_detail.TugName_isvalid = Calculate_Validation_of_Text(p_detail.TugName);
            p_detail.BargeName_isvalid = Calculate_Validation_of_Text(p_detail.BargeName);

            if (p_detail.TugName_isvalid && p_detail.BargeName_isvalid)
            {
                if ( ! Calculate_Validation_of_Key(p_detail.CreatedOn_Date_Only, p_detail.TugName, p_detail.BargeName, p_keys_Unique))
                {
                    p_detail.TugName_isvalid = false;
                    p_detail.BargeName_isvalid = false;
                }
            }

            // Other column
            p_detail.Barge_Ton_plan_isvalid = Calculate_Validation_of_Numeric(p_detail.Barge_Ton_plan);
            p_detail.Barge_Ton_actual_isvalid = Calculate_Validation_of_Numeric(p_detail.Barge_Ton_actual);
            p_detail.TM_plan_isvalid = Calculate_Validation_of_Numeric(p_detail.TM_plan);
            p_detail.TM_actual_isvalid = Calculate_Validation_of_Numeric(p_detail.TM_actual);
            p_detail.M_plan_isvalid = Calculate_Validation_of_Numeric(p_detail.M_plan);
            p_detail.M_actual_isvalid = Calculate_Validation_of_Numeric(p_detail.M_actual);
            p_detail.ASH_plan_isvalid = Calculate_Validation_of_Numeric(p_detail.ASH_plan);
            p_detail.ASH_actual_isvalid = Calculate_Validation_of_Numeric(p_detail.ASH_actual);
            p_detail.TS_plan_isvalid = Calculate_Validation_of_Numeric(p_detail.TS_plan);
            p_detail.TS_actual_isvalid = Calculate_Validation_of_Numeric(p_detail.TS_actual);
            p_detail.CV_ADB_plan_isvalid = Calculate_Validation_of_Numeric(p_detail.CV_ADB_plan);
            p_detail.CV_ADB_actual_isvalid = Calculate_Validation_of_Numeric(p_detail.CV_ADB_actual);
            p_detail.CV_AR_plan_isvalid = Calculate_Validation_of_Numeric(p_detail.CV_AR_plan);
            p_detail.CV_AR_actual_isvalid = Calculate_Validation_of_Numeric(p_detail.CV_AR_actual);
            p_detail.Product_isvalid = Calculate_Validation_of_Text(p_detail.Product);
        }

        private static bool Calculate_Validation_of_Key(string p_createdOn_Date_Only, string p_Tug, string p_Barge, List<Unique_Barge_Quality_Plan> p_keys_Unique)
        {
            bool result;

            // -- Special Case
            // 1. Check "kosong"
            if (string.IsNullOrEmpty(p_Tug) || string.IsNullOrEmpty(p_Barge))
            {
                // nilai Kosong dianggap Tidak Valid
                return false;
            }

            // Asumsi: Valid
            result = true;

            // Key: Valid, jika belum pernah ada
            foreach (Unique_Barge_Quality_Plan item in p_keys_Unique)
            {
                if (item.Tug != "TBN" && item.Barge != "TBN")
                {
                    if (item.CreatedOn_Date_Only == p_createdOn_Date_Only
                    && item.Tug == p_Tug
                    && item.Barge == p_Barge)
                    {
                        // tidak Valid
                        // karena kombinasi diatas, sudah pernah ada
                        result = false;
                    }
                }
            }

            return result;
        }

        class Unique_Barge_Quality_Plan
        {
            public string CreatedOn_Date_Only { get; set; }
            public string Tug { get; set; }
            public string Barge { get; set; }
        }

        public static bool Calculate_Validation_of_Text(string p_value)
        {

            // -- Special Case
            // 1. Check "kosong"
            if (string.IsNullOrEmpty(p_value))
            {
                // nilai Kosong dianggap Tidak Valid
                return false;
            }

            return true;
        }

        public static bool Calculate_Validation_of_Numeric(string p_value)
        {
            bool result = false;
            
            // -- Special Case
            // 1. Check "kosong"
            if (string.IsNullOrEmpty(p_value))
            {
                // nilai Kosong dianggap Tidak Valid
                return false;
            }
            
            // 2. Nilai "N/A"
            if (p_value.ToLower() == @"n/a")
            {
                // nilai "N/A" dianggap Valid
                return true;
            }
            
            // Nilai Numeric atau bukan ?
            if (OurUtility.ToDecimal(p_value, -9999.0M) > -9999.0M)
            {
                return true;
            }
            
            return result;
        }
        
        public static string Value_Of(bool p_isValid, string p_value)
        {            
            // -- Special Case
            // 1. Check "kosong"
            if (string.IsNullOrEmpty(p_value))
            {
                return p_value;
            }
            
            // 2. Nilai "N/A"
            if (p_value.ToLower() == @"n/a")
            {
                return p_value;
            }
            
            // Tidak Valid
            if ( ! p_isValid)
            {
                // maka Tampilkan Nilai apa adanya
                return p_value;
            }
            
            // disini pasti Valid
            // maka tampilkan versi "Numeric" yang sudah dilakukan Round()
            return string.Format("{0:N2}", OurUtility.Round(p_value, 2));
        }

        public static string Value_Of_0(bool p_isValid, string p_value)
        {
            // -- Special Case
            // 1. Check "kosong"
            if (string.IsNullOrEmpty(p_value))
            {
                return p_value;
            }

            // 2. Nilai "N/A"
            if (p_value.ToLower() == @"n/a")
            {
                return p_value;
            }

            // Tidak Valid
            if (!p_isValid)
            {
                // maka Tampilkan Nilai apa adanya
                return p_value;
            }

            // disini pasti Valid
            // maka tampilkan versi "Numeric" yang sudah dilakukan Round()
            return string.Format("{0:N0}", OurUtility.Round(p_value, 0));
        }

        public JsonResult File()
        {
            // Check Permission based on Current Url
            UserX user = new UserX(Request);

            // -- Not necessary checking Permission
            //Permission.Check_API(Request, user, ref permission_Item);
            // -- just Logging User: is enough
            if (user.UserId <= 0)
            {
                var msg = new { Success = false, Message = Permission.ERROR_PERMISSION_READ + " [not Login]", MessageDetail = string.Empty, Version = Configuration.VERSION };
                return Json(msg, JsonRequestBehavior.AllowGet);
            }

            string file_Id = OurUtility.ValueOf(Request, ".id");
            Model_View_File file = new Model_View_File();

            // -- Actual code
            try
            {
                using (MERCY_Ctx db = new MERCY_Ctx())
                {
                    using (var connection = new SqlConnection(db.Database.Connection.ConnectionString))
                    {
                        connection.Open();

                        SqlCommand command = connection.CreateCommand();

                        command.CommandText = string.Format(@"
                                                            select
                                                                t.RecordId
                                                                ,t.CreatedBy
                                                                ,Convert(varchar(28), t.CreatedOn, 120) as CreatedOn
                                                                ,t.FileName
                                                                ,t.Link
                                                                ,t.FileType
                                                            from TEMPORARY_File t
                                                            where t.RecordId = {0}
                                                            ", file_Id);


                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                file.RecordId = OurUtility.ValueOf(reader, "RecordId");
                                file.CreatedBy = OurUtility.ValueOf(reader, "CreatedBy");
                                file.CreatedOn = OurUtility.ValueOf(reader, "CreatedOn");
                                file.FileName = OurUtility.ValueOf(reader, "FileName");
                                file.Link = OurUtility.ValueOf(reader, "Link");
                                file.FileType = OurUtility.ValueOf(reader, "FileType");
                                file.Company = string.Empty;
                            }
                        }

                        connection.Close();
                    }
                }
            }
            catch { }

            var result = new { Success = true, Message = string.Empty, MessageDetail = string.Empty, Version = Configuration.VERSION, Data = file };
            return Json(result, JsonRequestBehavior.AllowGet);
        }
    }
}