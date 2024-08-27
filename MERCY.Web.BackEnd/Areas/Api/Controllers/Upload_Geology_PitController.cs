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

namespace MERCY.Web.BackEnd.Areas.Api.Controllers
{
    public class Upload_Geology_PitController : Controller
    {
        public JsonResult Index()
        {
            return Json("Upload_Geology_PitController", JsonRequestBehavior.AllowGet);
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

            if ( ! OurUtility.Upload_Record(user, fileName, fileName2, OurUtility.UPLOAD_Geology_Pit_Monitoring, ref id_File_Physical, ref msg))
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
            string company = OurUtility.ValueOf(Request, "company");
            string sheetName = string.Empty;
            bool is_Sheet_Valid = false;

            decimal CONST_100 = 100.0M;

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

                        int baris = 2;
                        bool is_Hidden = false;

                        string status = string.Empty;
                        List<string> keys_Unique = new List<string>();

                        TEMPORARY_Geology_Pit_Monitoring_Header header = null;
                        TEMPORARY_Geology_Pit_Monitoring detail = null;

                        foreach (Sheet sheet in wbPart.Workbook.Sheets)
                        {
                            is_Hidden = (sheet.State != null
                                            && sheet.State.HasValue
                                            && (sheet.State.Value == SheetStateValues.Hidden
                                                    || sheet.State.Value == SheetStateValues.VeryHidden));
                            
                            // Skip worksheet yang "Hidden"
                            if (is_Hidden) continue;

                            // somehow, we can change "Validation of SheetName"
                            is_Sheet_Valid = true;
                            sheetName = sheet.Name.ToString().Trim();

                            if ( ! is_Sheet_Valid) continue;

                            try
                            {
                                wsPart = (WorksheetPart)(wbPart.GetPartById(sheet.Id));
                                
                                // jika Job_No kosong, maka Worksheet "kosong"
                                if (string.IsNullOrEmpty(ExcelProcessing.GetCellValue(wbPart, wsPart, "C8"))) continue;

                                header = new TEMPORARY_Geology_Pit_Monitoring_Header
                                {
                                    File_Physical = id_File_Physical,
                                    Sheet = sheetName,
                                    Company = company,
                                    Date_Detail = ExcelProcessing.GetCellValue(wbPart, wsPart, "N6"),
                                    Job_No = ExcelProcessing.GetCellValue(wbPart, wsPart, "C8").Replace(": ", ""),
                                    Report_To = ExcelProcessing.GetCellValue(wbPart, wsPart, "C9").Replace(": ", ""),
                                    Date_Received = ExcelProcessing.GetCellValue(wbPart, wsPart, "C10").Replace(": ", ""),
                                    Nomor = ExcelProcessing.GetCellValue(wbPart, wsPart, "C11").Replace(": ", ""),

                                    CreatedOn = DateTime.Now,
                                    CreatedBy = user.UserId
                                };
                                header.CreatedOn_Date_Only = header.CreatedOn.ToString("yyyy-MM-dd");
                                header.CreatedOn_Year_Only = header.CreatedOn.Year;

                                db.TEMPORARY_Geology_Pit_Monitoring_Header.Add(header);
                                db.SaveChanges();
                                
                                // mulai dari Baris
                                baris = 16;

                                while (true)
                                {
                                    if (string.IsNullOrEmpty(ExcelProcessing.GetCellValue(wbPart, wsPart, "B" + baris.ToString()))) break;

                                    detail = new TEMPORARY_Geology_Pit_Monitoring
                                    {
                                        Header = header.RecordId,

                                        Sample_ID = ExcelProcessing.GetCellValue(wbPart, wsPart, "B" + baris.ToString()),
                                        SampleType = ExcelProcessing.GetCellValue(wbPart, wsPart, "C" + baris.ToString()),
                                        Lab_ID = ExcelProcessing.GetCellValue(wbPart, wsPart, "D" + baris.ToString()) + ExcelProcessing.GetCellValue(wbPart, wsPart, "E" + baris.ToString()),
                                        Mass_Spl = ExcelProcessing.GetCellValue(wbPart, wsPart, "F" + baris.ToString()),
                                        TM = ExcelProcessing.GetCellValue(wbPart, wsPart, "G" + baris.ToString()),
                                        M = ExcelProcessing.GetCellValue(wbPart, wsPart, "H" + baris.ToString()),
                                        VM = ExcelProcessing.GetCellValue(wbPart, wsPart, "I" + baris.ToString()),
                                        Ash = ExcelProcessing.GetCellValue(wbPart, wsPart, "J" + baris.ToString())
                                    };
                                    detail.FC = OurUtility.ToDecimal((CONST_100 - OurUtility.ToDecimal(detail.Ash, 2) - OurUtility.ToDecimal(detail.VM, 2) - OurUtility.ToDecimal(detail.M, 2)), 2);
                                    detail.TS = ExcelProcessing.GetCellValue(wbPart, wsPart, "L" + baris.ToString());
                                    detail.Cal_ad = ExcelProcessing.GetCellValue(wbPart, wsPart, "M" + baris.ToString());
                                    detail.Cal_db = OurUtility.ToDecimal(OurUtility.ToDecimal(detail.Cal_ad, 0) * CONST_100 / (CONST_100 - OurUtility.ToDecimal(detail.M, 2)), 0);
                                    detail.Cal_ar = OurUtility.ToDecimal(OurUtility.ToDecimal(detail.Cal_ad, 0) * (CONST_100 - OurUtility.ToDecimal(detail.TM, 2)) / (CONST_100 - OurUtility.ToDecimal(detail.M, 2)), 0);
                                    detail.Cal_daf = OurUtility.ToDecimal(OurUtility.ToDecimal(detail.Cal_ad, 0) * CONST_100 / (CONST_100 - OurUtility.ToDecimal(detail.M, 2) - OurUtility.ToDecimal(detail.Ash, 2)), 0);
                                    detail.RD = ExcelProcessing.GetCellValue(wbPart, wsPart, "Q" + baris.ToString());

                                    detail.CreatedOn = DateTime.Now;
                                    detail.CreatedBy = user.UserId;
                                    detail.CreatedOn_Date_Only = detail.CreatedOn.ToString("yyyy-MM-dd");
                                    detail.CreatedOn_Year_Only = detail.CreatedOn.Year;
                                    detail.Company = company;
                                    detail.Sheet = sheetName;

                                    Check_Validation(detail, keys_Unique);

                                    // reset Status, start with "InValid"
                                    status = "Invalid";

                                    if (detail.Sample_ID_isvalid
                                            && detail.SampleType_isvalid
                                            && detail.Lab_ID_isvalid
                                            && detail.Mass_Spl_isvalid
                                            && detail.TM_isvalid
                                            && detail.M_isvalid
                                            && detail.VM_isvalid
                                            && detail.Ash_isvalid
                                            && detail.FC_isvalid
                                            && detail.TS_isvalid
                                            && detail.Cal_ad_isvalid
                                            && detail.Cal_db_isvalid
                                            && detail.Cal_ar_isvalid
                                            && detail.Cal_daf_isvalid
                                            && detail.RD_isvalid
                                    )
                                    {
                                        status = "New";
                                    }

                                    detail.Status = status;

                                    db.TEMPORARY_Geology_Pit_Monitoring.Add(detail);
                                    db.SaveChanges();

                                    baris++;

                                    // for Next "Unique"
                                    try
                                    {
                                        keys_Unique.Add(detail.Lab_ID);
                                    }
                                    catch {}
                                }

                                // Handling Status: Update
                                try
                                {
                                    bool isNeed_Update = false;

                                    // ambil semua data yang baru di Insert diatas
                                    // yang Status: New
                                    List<TEMPORARY_Geology_Pit_Monitoring> recordSet = (
                                                                                        from dt in db.TEMPORARY_Geology_Pit_Monitoring
                                                                                        orderby dt.RecordId
                                                                                        where dt.Header == header.RecordId //data yang baru di Insert diatas
                                                                                                && dt.Status == "New"
                                                                                        select dt
                                                                                        ).ToList();

                                    // proses RecordSet
                                    foreach (TEMPORARY_Geology_Pit_Monitoring row in recordSet)
                                    {
                                        // cari data "sebelumnya" di Table "Actual Upload"
                                        // punya Unique Key yang sama
                                        if (db.UPLOAD_Geology_Pit_Monitoring.Where(d =>
                                                                                        d.Lab_ID == row.Lab_ID
                                                                                        && d.Company == row.Company
                                                                                        && d.CreatedOn_Year_Only == row.CreatedOn_Year_Only
                                                                                  ).Any())  // punya Unique yang sama   
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
                                from dt in db.TEMPORARY_Geology_Pit_Monitoring
                                join head in db.TEMPORARY_Geology_Pit_Monitoring_Header on dt.Header equals head.RecordId
                                orderby dt.RecordId
                                where head.File_Physical == id
                                select new Model_View_TEMPORARY_Geology_Pit_Monitoring
                                {
                                    Status = dt.Status
                                    , data_RecordId = dt.RecordId
                                    , Sample_ID = dt.Sample_ID
                                    , SampleType = dt.SampleType
                                    , Lab_ID = dt.Lab_ID
                                    , Mass_Spl = dt.Mass_Spl
                                    , TM = dt.TM
                                    , M = dt.M
                                    , VM = dt.VM
                                    , Ash = dt.Ash
                                    , FC = dt.FC
                                    , TS = dt.TS
                                    , Cal_ad = dt.Cal_ad
                                    , Cal_db = dt.Cal_db
                                    , Cal_ar = dt.Cal_ar
                                    , Cal_daf = dt.Cal_daf
                                    , RD = dt.RD

                                    , Sample_ID_isvalid = dt.Sample_ID_isvalid
                                    , SampleType_isvalid = dt.SampleType_isvalid
                                    , Lab_ID_isvalid = dt.Lab_ID_isvalid
                                    , Mass_Spl_isvalid = dt.Mass_Spl_isvalid
                                    , TM_isvalid = dt.TM_isvalid
                                    , M_isvalid = dt.M_isvalid
                                    , VM_isvalid = dt.VM_isvalid
                                    , Ash_isvalid = dt.Ash_isvalid
                                    , FC_isvalid = dt.FC_isvalid
                                    , TS_isvalid = dt.TS_isvalid
                                    , Cal_ad_isvalid = dt.Cal_ad_isvalid
                                    , Cal_db_isvalid = dt.Cal_db_isvalid
                                    , Cal_ar_isvalid = dt.Cal_ar_isvalid
                                    , Cal_daf_isvalid = dt.Cal_daf_isvalid
                                    , RD_isvalid = dt.RD_isvalid
                                }
                            );

                    var items = dataQuery.ToList();

                    items.ForEach(c =>
                    {
                        c.Mass_Spl_Str = Value_Of(c.Mass_Spl_isvalid, c.Mass_Spl);
                        c.TM_Str = Value_Of(c.TM_isvalid, c.TM);
                        c.M_Str = Value_Of(c.M_isvalid, c.M);
                        c.VM_Str = Value_Of(c.VM_isvalid, c.VM);
                        c.Ash_Str = Value_Of(c.Ash_isvalid, c.Ash);
                        c.FC_Str = c.FC.ToString("0.00");
                        c.TS_Str = Value_Of(c.TS_isvalid, c.TS);
                        c.Cal_ad_Str = String.Format("{0:n0}", Decimal.Round(OurUtility.ToDecimal(c.Cal_ad), 0));
                        c.Cal_db_Str = String.Format("{0:n0}", Decimal.Round(c.Cal_db, 0));
                        c.Cal_ar_Str = String.Format("{0:n0}", Decimal.Round(c.Cal_ar, 0));
                        c.Cal_daf_Str = String.Format("{0:n0}", Decimal.Round(c.Cal_daf, 0));
                        c.RD_Str = Value_Of(c.RD_isvalid, c.RD);
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
                    string leco_Stamp = string.Empty;

                    using (var connection = new SqlConnection(db.Database.Connection.ConnectionString))
                    {
                        connection.Open();

                        SqlCommand command = connection.CreateCommand();

                        command.CommandText = string.Format(@"exec UPLOAD_ACTUAL_GEOLOGY_PIT {0}", id);

                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                result = new { Success = OurUtility.ValueOf(reader, "cStatus").ToUpper() == "OK", Count = OurUtility.ValueOf(reader, "cCount"), Count2 = OurUtility.ValueOf(reader, "cCount2"), Permission = permission_Item, Message = OurUtility.ValueOf(reader, "cMessage"), MessageDetail = string.Empty, Version = Configuration.VERSION };

                                leco_Stamp = OurUtility.ValueOf(reader, "leco_Stamp");
                            }
                        }

                        connection.Close();
                    }

                    if ( ! string.IsNullOrEmpty(leco_Stamp))
                    {
                        Apply_Formula_After_PROCESS_LECO(db, leco_Stamp);
                    }
                }

                return Json(result, JsonRequestBehavior.AllowGet);
            }
            catch { }

            var msg = new { Success = true, Permission = permission_Item, Message = string.Empty, MessageDetail = string.Empty, Version = Configuration.VERSION };
            return Json(msg, JsonRequestBehavior.AllowGet);
        }

        public static void Check_Validation(TEMPORARY_Geology_Pit_Monitoring p_detail, List<string> p_keys_Unique)
        {
            // Key: Valid, jika belum pernah ada
            p_detail.Lab_ID_isvalid = Calculate_Validation_of_Key(p_detail.Lab_ID, p_keys_Unique);

            // Other column
            p_detail.Sample_ID_isvalid = Calculate_Validation_of_Text(p_detail.Sample_ID);
            p_detail.SampleType_isvalid = Calculate_Validation_of_Text(p_detail.SampleType);
            p_detail.Lab_ID_isvalid = Calculate_Validation_of_Text(p_detail.Lab_ID);
            p_detail.Mass_Spl_isvalid = Calculate_Validation_of_Numeric(p_detail.Mass_Spl);
            p_detail.TM_isvalid = Calculate_Validation_of_Numeric(p_detail.TM);
            p_detail.M_isvalid = Calculate_Validation_of_Numeric(p_detail.M);
            p_detail.VM_isvalid = Calculate_Validation_of_Numeric(p_detail.VM);
            p_detail.Ash_isvalid = Calculate_Validation_of_Numeric(p_detail.Ash);
            p_detail.FC_isvalid = true;
            p_detail.TS_isvalid = Calculate_Validation_of_Numeric(p_detail.TS);
            p_detail.Cal_ad_isvalid = Calculate_Validation_of_Numeric(p_detail.Cal_ad);
            p_detail.Cal_db_isvalid = true;
            p_detail.Cal_ar_isvalid = true;
            p_detail.Cal_daf_isvalid = true;
            p_detail.RD_isvalid = Calculate_Validation_of_Numeric(p_detail.RD);
        }

        public static bool Calculate_Validation_of_Key(string p_value, List<string> p_keys_Unique)
        {

            // -- Special Case
            // 1. Check "kosong"
            if (string.IsNullOrEmpty(p_value))
            {
                // nilai Kosong dianggap Tidak Valid
                return false;
            }

            // Key: Valid, jika belum pernah ada
            if ( ! p_keys_Unique.Contains(p_value))
            {
                return true;
            }

            return false;
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
            string result = string.Empty;
            
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
            string result = string.Empty;

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
                                                                ,h.Company
                                                            from TEMPORARY_File t
                                                                left join TEMPORARY_Geology_Pit_Monitoring_Header h on t.RecordId = h.File_Physical
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
                                file.Company = OurUtility.ValueOf(reader, "Company");
                            }
                        }

                        connection.Close();
                    }
                }
            }
            catch {}

            var result = new { Success = true, Message = string.Empty, MessageDetail = string.Empty, Version = Configuration.VERSION, Data = file };
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        private void Apply_Formula_After_PROCESS_LECO(MERCY_Ctx p_db, string p_leco_Stamp)
        {
            // double check
            if (string.IsNullOrEmpty(p_leco_Stamp)) return;

            try
            {
                var dataQuery =
                            (
                                from d in p_db.UPLOAD_Geology_Pit_Monitoring
                                where d.LECO_Stamp == p_leco_Stamp
                                select d
                            );

                var data = dataQuery.ToList();
                bool is_Changed = false;

                decimal CONST_100 = 100.0M;

                foreach (var detail in data)
                {
                    /* See: ParsingContent()
                    detail.FC = OurUtility.ToDecimal((CONST_100 - OurUtility.ToDecimal(detail.Ash, 2) - OurUtility.ToDecimal(detail.VM, 2) - OurUtility.ToDecimal(detail.M, 2)), 2);
                    detail.Cal_db = OurUtility.ToDecimal(OurUtility.ToDecimal(detail.Cal_ad, 0) * CONST_100 / (CONST_100 - OurUtility.ToDecimal(detail.M, 2)), 0);
                    detail.Cal_ar = OurUtility.ToDecimal(OurUtility.ToDecimal(detail.Cal_ad, 0) * (CONST_100 - OurUtility.ToDecimal(detail.TM, 2)) / (CONST_100 - OurUtility.ToDecimal(detail.M, 2)), 0);
                    detail.Cal_daf = OurUtility.ToDecimal(OurUtility.ToDecimal(detail.Cal_ad, 0) * CONST_100 / (CONST_100 - OurUtility.ToDecimal(detail.M, 2) - OurUtility.ToDecimal(detail.Ash, 2)), 0);
                    */

                    detail.FC = (CONST_100 - detail.Ash - detail.VM - detail.M);
                    detail.Cal_db = detail.Cal_ad * CONST_100 / (CONST_100 - detail.M);
                    detail.Cal_ar = detail.Cal_ad * (CONST_100 - detail.TM) / (CONST_100 - detail.M);
                    detail.Cal_daf = detail.Cal_ad * CONST_100 / (CONST_100 - detail.M - detail.Ash);
                    
                    // flag
                    is_Changed = true;
                }

                // saved if necessary
                if (is_Changed)
                {
                    p_db.SaveChanges();
                }
            }
            catch {}
        }
    }
}