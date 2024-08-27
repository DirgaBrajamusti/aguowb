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
    public class Upload_Sampling_ROMController : Controller
    {
        public JsonResult Index()
        {
            return Json("Upload_Sampling_ROMController", JsonRequestBehavior.AllowGet);
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

            if ( ! OurUtility.Upload_Record(user, fileName, fileName2, OurUtility.UPLOAD_Sampling_ROM, ref id_File_Physical, ref msg))
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

                        TEMPORARY_Sampling_ROM_Header header = null;
                        TEMPORARY_Sampling_ROM detail = null;

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
                                if (string.IsNullOrEmpty(ExcelProcessing.GetCellValue(wbPart, wsPart, "C9"))) continue;

                                header = new TEMPORARY_Sampling_ROM_Header
                                {
                                    File_Physical = id_File_Physical,
                                    Sheet = sheetName,
                                    Company = company,
                                    Date_Detail = ExcelProcessing.GetCellValue(wbPart, wsPart, "L7"),
                                    Job_No = ExcelProcessing.GetCellValue(wbPart, wsPart, "C9").Replace(": ", ""),
                                    Report_To = ExcelProcessing.GetCellValue(wbPart, wsPart, "C10").Replace(": ", ""),
                                    Method1 = ExcelProcessing.GetCellValue(wbPart, wsPart, "C11").Replace(": ", ""),
                                    Method2 = ExcelProcessing.GetCellValue(wbPart, wsPart, "C12").Replace(": ", ""),
                                    Method3 = ExcelProcessing.GetCellValue(wbPart, wsPart, "C13").Replace(": ", ""),
                                    Method4 = ExcelProcessing.GetCellValue(wbPart, wsPart, "C14").Replace(": ", ""),

                                    CreatedOn = DateTime.Now,
                                    CreatedBy = user.UserId
                                };
                                header.CreatedOn_Date_Only = header.CreatedOn.ToString("yyyy-MM-dd");
                                header.CreatedOn_Year_Only = header.CreatedOn.Year;

                                db.TEMPORARY_Sampling_ROM_Header.Add(header);
                                db.SaveChanges();
                                
                                // mulai dari Baris
                                baris = 18;

                                while (true)
                                {
                                    if (string.IsNullOrEmpty(ExcelProcessing.GetCellValue(wbPart, wsPart, "B" + baris.ToString()))) break;

                                    detail = new TEMPORARY_Sampling_ROM
                                    {
                                        Header = header.RecordId,

                                        Date_Request = ExcelProcessing.GetDate(ExcelProcessing.GetCellValue(wbPart, wsPart, "A" + baris.ToString()), "yyyy-MM-dd", ""),
                                        Date_Sampling = ExcelProcessing.GetDate(ExcelProcessing.GetCellValue(wbPart, wsPart, "B" + baris.ToString()), "yyyy-MM-dd", ""),
                                        Day_work = ExcelProcessing.GetCellValue(wbPart, wsPart, "C" + baris.ToString()),
                                        LOT = ExcelProcessing.GetCellValue(wbPart, wsPart, "D" + baris.ToString()),
                                        Lab_ID = ExcelProcessing.GetCellValue(wbPart, wsPart, "E" + baris.ToString()) + ExcelProcessing.GetCellValue(wbPart, wsPart, "F" + baris.ToString()),
                                        TM = ExcelProcessing.GetCellValue(wbPart, wsPart, "G" + baris.ToString()),
                                        M = ExcelProcessing.GetCellValue(wbPart, wsPart, "H" + baris.ToString()),
                                        ASH = ExcelProcessing.GetCellValue(wbPart, wsPart, "I" + baris.ToString()),
                                        TS = ExcelProcessing.GetCellValue(wbPart, wsPart, "J" + baris.ToString()),
                                        CV = ExcelProcessing.GetCellValue(wbPart, wsPart, "K" + baris.ToString()),
                                        Remark = ExcelProcessing.GetCellValue(wbPart, wsPart, "L" + baris.ToString()),
                                        Seam = ExcelProcessing.GetCellValue(wbPart, wsPart, "N" + baris.ToString()),

                                        CreatedOn = DateTime.Now,
                                        CreatedBy = user.UserId
                                    };
                                    detail.CreatedOn_Date_Only = detail.CreatedOn.ToString("yyyy-MM-dd");
                                    detail.CreatedOn_Year_Only = detail.CreatedOn.Year;
                                    detail.Company = company;
                                    detail.Sheet = sheetName;

                                    Check_Validation(detail, keys_Unique);

                                    // reset Status, start with "InValid"
                                    status = "Invalid";

                                    if (detail.Date_Request_isvalid
                                            && detail.Date_Sampling_isvalid
                                            && detail.Day_work_isvalid
                                            && detail.LOT_isvalid
                                            && detail.Lab_ID_isvalid
                                            && detail.TM_isvalid
                                            && detail.M_isvalid
                                            && detail.ASH_isvalid
                                            && detail.TS_isvalid
                                            && detail.CV_isvalid
                                            && detail.Remark_isvalid
                                            && detail.Seam_isvalid
                                    )
                                    {
                                        status = "New";
                                    }

                                    detail.Status = status;

                                    db.TEMPORARY_Sampling_ROM.Add(detail);
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
                                    List<TEMPORARY_Sampling_ROM> recordSet = (
                                                                                from dt in db.TEMPORARY_Sampling_ROM
                                                                                orderby dt.RecordId
                                                                                where dt.Header == header.RecordId //data yang baru di Insert diatas
                                                                                        && dt.Status == "New"
                                                                                select dt
                                                                             ).ToList();

                                    // proses RecordSet
                                    foreach (TEMPORARY_Sampling_ROM row in recordSet)
                                    {
                                        // cari data "sebelumnya" di Table "Actual Upload"
                                        // punya Unique Key yang sama
                                        if (db.UPLOAD_Sampling_ROM.Where(d => 
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
                                from dt in db.TEMPORARY_Sampling_ROM
                                join h in db.TEMPORARY_Sampling_ROM_Header on dt.Header equals h.RecordId
                                orderby dt.RecordId
                                where h.File_Physical == id
                                select new Model_View_TEMPORARY_Sampling_ROM
                                {
                                    Status = dt.Status
                                    , data_RecordId = dt.RecordId
                                    , Date_Request = dt.Date_Request
                                    , Date_Sampling = dt.Date_Sampling
                                    , Day_work = dt.Day_work
                                    , LOT = dt.LOT
                                    , Lab_ID = dt.Lab_ID
                                    , TM = dt.TM
                                    , M = dt.M
                                    , ASH = dt.ASH
                                    , TS = dt.TS
                                    , CV = dt.CV
                                    , Remark = dt.Remark
                                    , Seam = dt.Seam

                                    , Date_Request_isvalid = dt.Date_Request_isvalid
                                    , Date_Sampling_isvalid = dt.Date_Sampling_isvalid
                                    , Day_work_isvalid = dt.Day_work_isvalid
                                    , LOT_isvalid = dt.LOT_isvalid
                                    , Lab_ID_isvalid = dt.Lab_ID_isvalid
                                    , TM_isvalid = dt.TM_isvalid
                                    , M_isvalid = dt.M_isvalid
                                    , ASH_isvalid = dt.ASH_isvalid
                                    , TS_isvalid = dt.TS_isvalid
                                    , CV_isvalid = dt.CV_isvalid
                                    , Remark_isvalid = dt.Remark_isvalid
                                    , Seam_isvalid = dt.Seam_isvalid
                                }
                            );

                    var items = dataQuery.ToList();

                    items.ForEach(c =>
                    {
                        c.TM_Str = Value_Of(c.TM_isvalid, c.TM);
                        c.M_Str = Value_Of(c.M_isvalid, c.M);
                        c.ASH_Str = Value_Of(c.ASH_isvalid, c.ASH);
                        c.TS_Str = Value_Of(c.TS_isvalid, c.TS);
                        c.CV_Str = Value_Of_0(c.CV_isvalid, c.CV);
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
                        connection.Open();

                        SqlCommand command = connection.CreateCommand();

                        command.CommandText = string.Format(@"exec UPLOAD_ACTUAL_Sampling_ROM {0}", id);

                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                result = new { Success = OurUtility.ValueOf(reader, "cStatus").ToUpper() == "OK", Count = OurUtility.ValueOf(reader, "cCount"), Count2 = OurUtility.ValueOf(reader, "cCount2"), Permission = permission_Item, Message = OurUtility.ValueOf(reader, "cMessage"), MessageDetail = string.Empty, Version = Configuration.VERSION };
                            }
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

        public static void Check_Validation(TEMPORARY_Sampling_ROM p_detail, List<string> p_keys_Unique)
        {
            // Key: Valid, jika belum pernah ada
            p_detail.Lab_ID_isvalid = Calculate_Validation_of_Key(p_detail.Lab_ID, p_keys_Unique);

            // Other column
            p_detail.Date_Request_isvalid = true; // Calculate_Validation_of_Text(p_detail.Date_Request);
            p_detail.Date_Sampling_isvalid = Calculate_Validation_of_Text(p_detail.Date_Sampling);
            p_detail.Day_work_isvalid = Calculate_Validation_of_Text(p_detail.Day_work);
            p_detail.LOT_isvalid = Calculate_Validation_of_Text(p_detail.LOT);
            p_detail.TM_isvalid = Calculate_Validation_of_Numeric(p_detail.TM);
            p_detail.M_isvalid = Calculate_Validation_of_Numeric(p_detail.M);
            p_detail.ASH_isvalid = Calculate_Validation_of_Numeric(p_detail.ASH);
            p_detail.TS_isvalid = Calculate_Validation_of_Numeric(p_detail.TS);
            p_detail.CV_isvalid = Calculate_Validation_of_Numeric(p_detail.CV);
            p_detail.Remark_isvalid = true;
            p_detail.Seam_isvalid = true;
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
                                                                left join TEMPORARY_Sampling_ROM_Header h on t.RecordId = h.File_Physical
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

            var result = new { Success = true, Message = string.Empty, MessageDetail = string.Empty, Version = Configuration.VERSION, Data = file};
            return Json(result, JsonRequestBehavior.AllowGet);
        }
    }
}