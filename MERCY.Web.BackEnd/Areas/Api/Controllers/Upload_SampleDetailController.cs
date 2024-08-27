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

namespace MERCY.Web.BackEnd.Areas.Api.Controllers
{
    public class Upload_SampleDetailController : Controller
    {
        public JsonResult Index()
        {
            return Json("Upload_SampleDetailController", JsonRequestBehavior.AllowGet);
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

            if ( ! OurUtility.Upload_Record(user, fileName, fileName2, OurUtility.UPLOAD_SampleDetail, ref id_File_Physical, ref msg))
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

                        TEMPORARY_SampleDetail detail = null;

                        foreach (Sheet sheet in wbPart.Workbook.Sheets)
                        {
                            is_Hidden = (sheet.State != null
                                            && sheet.State.HasValue
                                            && (sheet.State.Value == SheetStateValues.Hidden
                                                    || sheet.State.Value == SheetStateValues.VeryHidden));
                            
                            // Skip worksheet yang "Hidden"
                            if (is_Hidden) continue;
                            
                            try
                            {
                                wsPart = (WorksheetPart)(wbPart.GetPartById(sheet.Id));
                                
                                // mulai dari Baris
                                baris = 9;

                                while (true)
                                {
                                    if (string.IsNullOrEmpty(ExcelProcessing.GetCellValue(wbPart, wsPart, "A" + baris.ToString()))) break;

                                    detail = new TEMPORARY_SampleDetail
                                    {
                                        File_Physical = id_File_Physical,
                                        Status = "New",
                                        Sheet = sheet.Name.ToString(),

                                        SAMPLE = ExcelProcessing.GetCellValue(wbPart, wsPart, "B" + baris.ToString()),
                                        GEOLOGIST = ExcelProcessing.GetCellValue(wbPart, wsPart, "C" + baris.ToString()),
                                        SEAM = ExcelProcessing.GetCellValue(wbPart, wsPart, "D" + baris.ToString()),
                                        SAMPLE_TYPE = ExcelProcessing.GetCellValue(wbPart, wsPart, "E" + baris.ToString()),
                                        DEPTH_FROM = ExcelProcessing.GetCellValue(wbPart, wsPart, "F" + baris.ToString()),
                                        DEPTH_TO = ExcelProcessing.GetCellValue(wbPart, wsPart, "G" + baris.ToString()),
                                        Total_Moisture = ExcelProcessing.GetCellValue(wbPart, wsPart, "H" + baris.ToString()),
                                        Proximate_analysis = ExcelProcessing.GetCellValue(wbPart, wsPart, "I" + baris.ToString()),
                                        Sulfur_content = ExcelProcessing.GetCellValue(wbPart, wsPart, "J" + baris.ToString()),
                                        Calorific_value = ExcelProcessing.GetCellValue(wbPart, wsPart, "K" + baris.ToString()),
                                        Relative_density = ExcelProcessing.GetCellValue(wbPart, wsPart, "L" + baris.ToString()),
                                        CSN = ExcelProcessing.GetCellValue(wbPart, wsPart, "M" + baris.ToString()),
                                        Ash_analysis = ExcelProcessing.GetCellValue(wbPart, wsPart, "N" + baris.ToString()),
                                        HGI = ExcelProcessing.GetCellValue(wbPart, wsPart, "O" + baris.ToString()),
                                        Ultimate_analysis = ExcelProcessing.GetCellValue(wbPart, wsPart, "P" + baris.ToString()),
                                        Chlorine = ExcelProcessing.GetCellValue(wbPart, wsPart, "Q" + baris.ToString()),
                                        Phosphorous = ExcelProcessing.GetCellValue(wbPart, wsPart, "R" + baris.ToString()),
                                        Fluorine = ExcelProcessing.GetCellValue(wbPart, wsPart, "S" + baris.ToString()),
                                        Lead_ = ExcelProcessing.GetCellValue(wbPart, wsPart, "T" + baris.ToString()),
                                        Zinc = ExcelProcessing.GetCellValue(wbPart, wsPart, "U" + baris.ToString()),
                                        Form_of_Sulphur = ExcelProcessing.GetCellValue(wbPart, wsPart, "V" + baris.ToString()),
                                        Ash_fusions_temperature = ExcelProcessing.GetCellValue(wbPart, wsPart, "W" + baris.ToString()),
                                        TraceElement = ExcelProcessing.GetCellValue(wbPart, wsPart, "X" + baris.ToString()),

                                        CreatedOn = DateTime.Now,
                                        CreatedBy = user.UserId
                                    };

                                    Check_Validation(detail, keys_Unique);

                                    // reset Status, start with "InValid"
                                    status = "Invalid";

                                    if (detail.SAMPLE_isvalid
                                    )
                                    {
                                        status = "New";
                                    }

                                    detail.Status = status;

                                    db.TEMPORARY_SampleDetail.Add(detail);
                                    db.SaveChanges();

                                    baris++;

                                    // for Next "Unique"
                                    try
                                    {
                                        keys_Unique.Add(detail.SAMPLE);
                                    }
                                    catch {}
                                }

                                // Handling Status: Update
                                try
                                {
                                    bool isNeed_Update = false;

                                    // ambil semua data yang baru di Insert diatas
                                    // check sudah pernah ada di Database
                                    List<TEMPORARY_SampleDetail> recordSet = (
                                                                                from dt in db.TEMPORARY_SampleDetail
                                                                                orderby dt.RecordId
                                                                                where dt.File_Physical == id_File_Physical //data yang baru di Insert diatas
                                                                                select dt
                                                                             ).ToList();

                                    // proses RecordSet
                                    foreach (TEMPORARY_SampleDetail row in recordSet)
                                    {
                                        // cari data "sebelumnya" di Table "Actual Upload"
                                        // punya Unique Key yang sama
                                        if (db.AnalysisRequest_Detail.Where(d => d.SampleID == row.SAMPLE // punya Unique yang sama   
                                                                          ).Any())
                                        {
                                            row.Status = "Invalid 2";

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

        public JsonResult DisplayContent2()
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
                                from d in db.TEMPORARY_SampleDetail
                                orderby d.SAMPLE
                                where d.File_Physical == id
                                select new Model_View_TEMPORARY_SampleDetail
                                {
                                    RecordId = d.RecordId
                                    , File_Physical = d.File_Physical
                                    , Status = d.Status
                                    , Sheet = d.Sheet
                                    , SAMPLE = d.SAMPLE
                                    , GEOLOGIST = d.GEOLOGIST
                                    , SEAM = d.SEAM
                                    , SAMPLE_TYPE = d.SAMPLE_TYPE
                                    , DEPTH_FROM = d.DEPTH_FROM
                                    , DEPTH_TO = d.DEPTH_TO
                                    , Total_Moisture = d.Total_Moisture
                                    , Proximate_analysis = d.Proximate_analysis
                                    , Sulfur_content = d.Sulfur_content
                                    , Calorific_value = d.Calorific_value
                                    , Relative_density = d.Relative_density
                                    , CSN = d.CSN
                                    , Ash_analysis = d.Ash_analysis
                                    , HGI = d.HGI
                                    , Ultimate_analysis = d.Ultimate_analysis
                                    , Chlorine = d.Chlorine
                                    , Phosphorous = d.Phosphorous
                                    , Fluorine = d.Fluorine
                                    , Lead_ = d.Lead_
                                    , Zinc = d.Zinc
                                    , Form_of_Sulphur = d.Form_of_Sulphur
                                    , Ash_fusions_temperature = d.Ash_fusions_temperature
                                    , TraceElement = d.TraceElement
                                    , CreatedBy = d.CreatedBy 
                                    , CreatedOn = d.CreatedOn
                                }
                            );

                    var items = dataQuery.ToList();

                    items.ForEach(c =>
                    {
                        c.Thick = Math.Abs(OurUtility.Round(OurUtility.ToDecimal(c.DEPTH_FROM) - OurUtility.ToDecimal(c.DEPTH_TO), 2));
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
                                from dt in db.TEMPORARY_SampleDetail
                                orderby dt.RecordId // dt.SAMPLE
                                where dt.File_Physical == id
                                select new Model_View_AnalysisRequest_Detail
                                {
                                    RecordId = -1
                                    , SampleID = dt.SAMPLE
                                    , DEPTH_FROM =  dt.DEPTH_FROM
                                    , DEPTH_TO = dt.DEPTH_TO
                                    , SampleType = dt.SAMPLE_TYPE
                                    , SEAM = dt.SEAM
                                    , LabId = ""
                                    , CreatedOn = dt.CreatedOn
                                    , Status = dt.Status
                                    , Verification = true
                                    , Verification_Comment = string.Empty
                                    , ReTest = false

                                    , SAMPLE_isvalid = dt.SAMPLE_isvalid

                                    // parameters
                                    , Total_Moisture = dt.Total_Moisture
                                    , Proximate_analysis = dt.Proximate_analysis
                                    , Sulfur_content = dt.Sulfur_content
                                    , Calorific_value = dt.Calorific_value
                                    , Relative_density = dt.Relative_density
                                    , CSN = dt.CSN
                                    , Ash_analysis = dt.Ash_analysis
                                    , HGI = dt.HGI
                                    , Ultimate_analysis = dt.Ultimate_analysis
                                    , Chlorine = dt.Chlorine
                                    , Phosphorous = dt.Phosphorous
                                    , Fluorine = dt.Fluorine
                                    , Lead_ = dt.Lead_
                                    , Zinc = dt.Zinc
                                    , Form_of_Sulphur = dt.Form_of_Sulphur
                                    , Ash_fusions_temperature = dt.Ash_fusions_temperature
                                    , TraceElement = dt.TraceElement
                                }
                            );

                    var items = dataQuery.ToList();

                    string duplicate_Id = string.Empty;
                    string separator = string.Empty;

                    Model_View_Parameter_Analysis_Request parameter = new Model_View_Parameter_Analysis_Request
                    {
                        TM = false,
                        Prox = false,
                        TS = false,
                        CV = false,
                        RD = false,
                        CSN = false,
                        AA = false,
                        HGI = false,
                        Ultimate = false,
                        Chlorine = false,
                        Phosporus = false,
                        Fluorine = false,
                        Lead = false,
                        Zinc = false,
                        AFT = false,
                        TraceElement = false
                    };

                    items.ForEach(c =>
                    {
                        c.CreatedOn_Str = string.Empty;
                        c.CreatedOn_Str2 = string.Empty;

                        c.From2 = OurUtility.ToDecimal(c.DEPTH_FROM, 2);
                        c.To2 = OurUtility.ToDecimal(c.DEPTH_TO, 2);
                        c.Thick = Math.Abs(OurUtility.Round(OurUtility.ToDecimal(c.DEPTH_FROM) - OurUtility.ToDecimal(c.DEPTH_TO), 2)).ToString();

                        switch (c.Status)
                        {
                            case "Invalid":
                                duplicate_Id += separator + "Sample ID #" + c.SampleID + " is duplicate in File";
                                separator = "<br/>";
                                break;
                            case "Invalid 2":
                                duplicate_Id += separator + "Sample ID #" + c.SampleID + " is duplicate in Database";
                                separator = "<br/>";
                                break;
                        }

                        if (c.Total_Moisture.Equals("v")) parameter.TM = true;
                        if (c.Proximate_analysis.Equals("v")) parameter.Prox = true;
                        if (c.Sulfur_content.Equals("v")) parameter.TS = true;                  // ????
                        if (c.Calorific_value.Equals("v")) parameter.CV = true;
                        if (c.Relative_density.Equals("v")) parameter.RD = true;
                        if (c.CSN.Equals("v")) parameter.CSN = true;
                        if (c.Ash_analysis.Equals("v")) parameter.AA = true;
                        if (c.HGI.Equals("v")) parameter.HGI = true;
                        if (c.Ultimate_analysis.Equals("v")) parameter.Ultimate = true;
                        if (c.Chlorine.Equals("v")) parameter.Chlorine = true;
                        if (c.Phosphorous.Equals("v")) parameter.Phosporus = true;
                        if (c.Fluorine.Equals("v")) parameter.Fluorine = true;
                        if (c.Lead_.Equals("v")) parameter.Lead = true;
                        if (c.Zinc.Equals("v")) parameter.Zinc = true;
                        //if (c.Form_of_Sulphur.Equals("v")) parameter.aaaaa = true;                       // ????
                        if (c.Ash_fusions_temperature.Equals("v")) parameter.AFT = true;
                        if ( ! string.IsNullOrEmpty(c.TraceElement)) parameter.TraceElement = true;        // ????
                    });

                    int count = items.Count;
                    if (count <= 0)
                    {
                        duplicate_Id = "File format is invalid";
                    }

                    var result = new { Success = (string.IsNullOrEmpty(duplicate_Id) && count > 0), Permission = permission_Item, Message = duplicate_Id, MessageDetail = string.Empty, Version = Configuration.VERSION, Items = items, Total = count, Parameters = parameter };
                    return Json(result, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                var result = new { Success = false, Permission = permission_Item, ex.Message, MessageDetail = string.Empty, Version = Configuration.VERSION, Items = string.Empty, Total = 0 };
                return Json(result, JsonRequestBehavior.AllowGet);
            }
        }
        
        public static void Check_Validation(TEMPORARY_SampleDetail p_detail, List<string> p_keys_Unique)
        {
            // Key: Valid, jika belum pernah ada
            p_detail.SAMPLE_isvalid = Calculate_Validation_of_Key(p_detail.SAMPLE, p_keys_Unique);

            // Other column
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
    }
}