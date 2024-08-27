using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

using System.IO;

using MERCY.Data.EntityFramework;
using MERCY.Web.BackEnd.Helpers;
using MERCY.Web.BackEnd.Models;
using MERCY.Web.BackEnd.Security;
using Permission = MERCY.Web.BackEnd.Security.Permission;

using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;

using System.Data;
using System.Data.SqlClient;

namespace MERCY.Web.BackEnd.Areas.Api.Controllers
{
    public class FileController : Controller
    {
        private bool is_view = true;
        private bool is_add = true;
        private bool is_delete = true;
        private bool is_edit = true;
        private bool is_active = true;

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

            bool isAll_fileType = true;
            string fileType = OurUtility.ValueOf(Request, "fileType");
            isAll_fileType = string.IsNullOrEmpty(fileType) || (fileType == "all");

            string dateFrom = OurUtility.ValueOf(Request, "dateFrom");
            string dateTo = OurUtility.ValueOf(Request, "dateTo");

            // -- Actual code
            try
            {
                DateTime dateFrom_O = DateTime.Now;
                DateTime dateTo_O = DateTime.Now.AddDays(1);

                try
                {
                    dateFrom_O = DateTime.Parse(dateFrom);
                }
                catch {}
                try
                {
                    dateTo_O = DateTime.Parse(dateTo).AddDays(1);
                }
                catch {}

                List<Model_View_File> items = new List<Model_View_File>();
                
                using (MERCY_Ctx db = new MERCY_Ctx())
                {
                    using (var connection = new SqlConnection(db.Database.Connection.ConnectionString))
                    {
                        connection.Open();

                        SqlCommand command = connection.CreateCommand();

                        command.CommandText = string.Format(@"
                                                            select distinct top 100
                                                                t.RecordId
                                                                , t.CreatedBy
                                                                , Convert(varchar(19), t.CreatedOn, 120) as CreatedOn
                                                                , t.FileName
                                                                , t.Link
                                                                , t.FileType
                                                                , u.FullName
                                                                , sampling.Company as sampling_Company
                                                                , pit.Company as pit_Company
                                                                , exploration.Company as exploration_Company
                                                                , barge.Company as barge_Company
                                                                , crushing.Company as crushing_Company
                                                                , hac.Company as hac_Company
                                                            FROM TEMPORARY_File t
                                                                left join [User] u on t.CreatedBy = u.UserId
                                                                left join TEMPORARY_Sampling_ROM_Header sampling on t.RecordId = sampling.File_Physical
                                                                left join TEMPORARY_Geology_Pit_Monitoring_Header pit on t.RecordId = pit.File_Physical
                                                                left join TEMPORARY_Geology_Explorasi_Header exploration on t.RecordId = exploration.File_Physical
                                                                left join TEMPORARY_BARGE_LOADING_Header barge on t.RecordId = barge.File_Physical
                                                                left join TEMPORARY_CRUSHING_PLANT_Header crushing on t.RecordId = crushing.File_Physical
                                                                left join TEMPORARY_HAC_Header hac on t.RecordId = hac.File_Physical
                                                            where Convert(varchar(10), t.CreatedOn, 120) >= '{0}'
                                                                and Convert(varchar(10), t.CreatedOn, 120) < '{1}'
                                                            order by t.RecordId desc
                                                            ", OurUtility.DateFormat(dateFrom_O, "yyyy-MM-dd")
                                                            , OurUtility.DateFormat(dateTo_O, "yyyy-MM-dd"));

                        Model_View_File file = null;
                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                file = new Model_View_File
                                {
                                    RecordId = OurUtility.ValueOf(reader, "RecordId"),
                                    CreatedBy = OurUtility.ValueOf(reader, "CreatedBy"),
                                    CreatedOn = OurUtility.ValueOf(reader, "CreatedOn"),
                                    FileName = OurUtility.ValueOf(reader, "FileName"),
                                    Link = OurUtility.ValueOf(reader, "Link"),
                                    FileType = OurUtility.ValueOf(reader, "FileType"),
                                    CreatedBy_Str = OurUtility.ValueOf(reader, "FullName"),

                                    Company = string.Empty
                                };
                                switch (file.FileType)
                                {
                                    case OurUtility.UPLOAD_Sampling_ROM:
                                        file.Company = OurUtility.ValueOf(reader, "sampling_Company");
                                        break;
                                    case OurUtility.UPLOAD_Geology_Pit_Monitoring:
                                        file.Company = OurUtility.ValueOf(reader, "pit_Company");
                                        break;
                                    case OurUtility.UPLOAD_Geology_Explorasi:
                                        file.Company = OurUtility.ValueOf(reader, "exploration_Company");
                                        break;
                                    case OurUtility.UPLOAD_BARGE_LOADING:
                                        file.Company = OurUtility.ValueOf(reader, "barge_Company");
                                        break;
                                    case OurUtility.UPLOAD_CRUSHING_PLANT:
                                        file.Company = OurUtility.ValueOf(reader, "crushing_Company");
                                        break;
                                    case OurUtility.UPLOAD_HAC:
                                        file.Company = OurUtility.ValueOf(reader, "hac_Company");
                                        break;
                                }

                                file.CreatedOn_Str = OurUtility.DateFormat(file.CreatedOn, "dd-MMM-yyyy HH:mm:ss");

                                items.Add(file);
                            }
                        }

                        connection.Close();
                    }
                }

                var result = new { Success = true, Permission = permission_Item, User = UserX.Information(user, is_Show_User_Menu, is_Show_User_Relation), Message = string.Empty, MessageDetail = string.Empty, Version = Configuration.VERSION, Items = items, Total = items.Count };
                return Json(result, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                var result = new { Success = false, Permission = permission_Item, ex.Message, MessageDetail = string.Empty, Version = Configuration.VERSION, Items = string.Empty, Total = 0 };
                return Json(result, JsonRequestBehavior.AllowGet);
            }
        }
        
        private string UploadFolder
        {
            get {
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
            JsonResult result = Json(new { Status = "unknown", Message = "", Id = -1 }, JsonRequestBehavior.AllowGet);

            string fileName = string.Empty;
            string fileName2 = string.Empty;
            string msg = string.Empty;
            long id_File_Physical = 0;

            // Check Permission based on Current Url
            UserX user = new UserX(Request);

            /*Permission.Check_API(Request, user, ref permission_Item);
            if (!permission_Item.Is_View)
            {
                var msg = new { Success = false, Permission = permission_Item, Message = Permission.ERROR_PERMISSION_READ, MessageDetail = string.Empty, Version = Configuration.VERSION };
                return Json(msg, JsonRequestBehavior.AllowGet);
            }*/

            string fileType = Request[".ty"];
            string company = Request["company"];

            if ( ! OurUtility.Upload(Request, UploadFolder, ref fileName, ref fileName2, ref msg))
            {
                var x = new { Status = "unknown", Message = msg, Id = -1 };
                return Json(x, JsonRequestBehavior.AllowGet);
            }

            if ( ! OurUtility.Upload_Record(user, fileName, fileName2, fileType, ref id_File_Physical, ref msg))
            {
                var x = new { Status = "unknown", Message = msg, Id = -1 };
                return Json(x, JsonRequestBehavior.AllowGet);
            }

            switch (fileType)
            {
                case OurUtility.UPLOAD_Sampling_ROM:
                    result = Upload_Sampling_ROM(user, fileName2, id_File_Physical, company);
                    break;
                case OurUtility.UPLOAD_Geology_Pit_Monitoring:
                    result = Upload_Geology_Pit_Monitoring(user, fileName2, id_File_Physical, company);
                    break;
                case OurUtility.UPLOAD_Geology_Explorasi:
                    result = Upload_Geology_Explorasi(user, fileName2, id_File_Physical, company);
                    break;
                case OurUtility.UPLOAD_BARGE_LOADING:
                    result = Upload_BARGE_LOADING(user, fileName2, id_File_Physical, company);
                    break;
                case OurUtility.UPLOAD_CRUSHING_PLANT:
                    result = Upload_CRUSHING_PLANT(user, fileName2, id_File_Physical, company);
                    break;
            }

            return result;
        }
            
        private JsonResult Upload_Geology_Pit_Monitoring(UserX p_executedBy, string p_fileName, long p_id_File_Physical, string p_company)
        {
            string overall_message = string.Empty;
            string overall_status = "unknown";

            long id_Header = 0;

            try
            {
                string uploadSheetName = "Sheet1";

                // Open the spreadsheet document for read-only access.
                using (SpreadsheetDocument document = SpreadsheetDocument.Open(UploadFolder + p_fileName, false))
                {
                    // Retrieve a reference to the workbook part.
                    WorkbookPart wbPart = document.WorkbookPart;

                    // Find the sheet with the supplied name, and then use that 
                    // Sheet object to retrieve a reference to the first worksheet.
                    Sheet theSheet = wbPart.Workbook.Descendants<Sheet>().Where(s => s.Name == uploadSheetName).FirstOrDefault();

                    // Throw an exception if there is no sheet.
                    if (theSheet == null)
                    {
                        throw new ArgumentException("sheetName");
                    }

                    // Retrieve a reference to the worksheet part.
                    WorksheetPart wsPart = (WorksheetPart)(wbPart.GetPartById(theSheet.Id));
                    
                    try
                    {
                        using (MERCY_Ctx db = new MERCY_Ctx())
                        {
                            var data_header = new TEMPORARY_Geology_Pit_Monitoring_Header
                            {
                                File_Physical = p_id_File_Physical,
                                Company = p_company,
                                Date_Detail = ExcelProcessing.GetCellValue(wbPart, wsPart, "N6"),
                                Job_No = ExcelProcessing.GetCellValue(wbPart, wsPart, "C8").Replace(": ", ""),
                                Report_To = ExcelProcessing.GetCellValue(wbPart, wsPart, "C9").Replace(": ", ""),
                                Date_Received = ExcelProcessing.GetCellValue(wbPart, wsPart, "C10").Replace(": ", ""),
                                Nomor = ExcelProcessing.GetCellValue(wbPart, wsPart, "C11").Replace(": ", ""),

                                CreatedOn = DateTime.Now,
                                CreatedBy = p_executedBy.UserId
                            };

                            db.TEMPORARY_Geology_Pit_Monitoring_Header.Add(data_header);
                            db.SaveChanges();

                            id_Header = data_header.RecordId;

                            // mulai dari Baris
                            int baris = 16;

                            List<string> keysUnique = new List<string>();
                            string status = "New";
                            long first_RecordId = 0;

                            bool complete = false;
                            decimal CONST_100 = 100.0M;

                            while ( ! complete)
                            {
                                if (string.IsNullOrEmpty(ExcelProcessing.GetCellValue(wbPart, wsPart, "B" + baris.ToString()))) break;

                                // init Status, start with "New"
                                status = "New";

                                var data = new TEMPORARY_Geology_Pit_Monitoring
                                {
                                    Header = id_Header,
                                    Sample_ID = ExcelProcessing.GetCellValue(wbPart, wsPart, "B" + baris.ToString()),
                                    SampleType = ExcelProcessing.GetCellValue(wbPart, wsPart, "C" + baris.ToString()),
                                    Lab_ID = ExcelProcessing.GetCellValue(wbPart, wsPart, "D" + baris.ToString()) + ExcelProcessing.GetCellValue(wbPart, wsPart, "E" + baris.ToString()),
                                    Mass_Spl = ExcelProcessing.GetCellValue(wbPart, wsPart, "F" + baris.ToString()),
                                    TM = ExcelProcessing.GetCellValue(wbPart, wsPart, "G" + baris.ToString()),
                                    M = ExcelProcessing.GetCellValue(wbPart, wsPart, "H" + baris.ToString()),
                                    VM = ExcelProcessing.GetCellValue(wbPart, wsPart, "I" + baris.ToString()),
                                    Ash = ExcelProcessing.GetCellValue(wbPart, wsPart, "J" + baris.ToString())
                                };
                                data.FC = OurUtility.ToDecimal((CONST_100 - OurUtility.ToDecimal(data.Ash, 2) - OurUtility.ToDecimal(data.VM, 2) - OurUtility.ToDecimal(data.M, 2)), 2);
                                data.TS = ExcelProcessing.GetCellValue(wbPart, wsPart, "L" + baris.ToString());
                                data.Cal_ad = ExcelProcessing.GetCellValue(wbPart, wsPart, "M" + baris.ToString());
                                data.Cal_db = OurUtility.ToDecimal(OurUtility.ToDecimal(data.Cal_ad, 0) * CONST_100 / (CONST_100 - OurUtility.ToDecimal(data.M, 2)), 0);
                                data.Cal_ar = OurUtility.ToDecimal(OurUtility.ToDecimal(data.Cal_ad, 0) * (CONST_100 - OurUtility.ToDecimal(data.TM, 2)) / (CONST_100 - OurUtility.ToDecimal(data.M, 2)), 0);
                                data.Cal_daf = OurUtility.ToDecimal(OurUtility.ToDecimal(data.Cal_ad, 0) * CONST_100 / (CONST_100 - OurUtility.ToDecimal(data.M, 2) - OurUtility.ToDecimal(data.Ash, 2)), 0);
                                data.RD = ExcelProcessing.GetCellValue(wbPart, wsPart, "Q" + baris.ToString());

                                data.CreatedOn = DateTime.Now;
                                data.CreatedBy = p_executedBy.UserId;

                                if (string.IsNullOrEmpty(data.Lab_ID))
                                {
                                    status = "Invalid";
                                }
                                // check with Current Collections
                                else if (keysUnique.Contains(data.Lab_ID))
                                {
                                    status = "Invalid";
                                }

                                data.Status = status;

                                db.TEMPORARY_Geology_Pit_Monitoring.Add(data);
                                db.SaveChanges();

                                // simpan Id pertama
                                if (first_RecordId <= 0) first_RecordId = data.RecordId;

                                // maju 1 baris
                                baris++;
                                // batas Jumlah Baris Data, yang boleh diproses
                                if (baris >= 200) break;

                                // for Next "Unique"
                                try
                                {
                                    keysUnique.Add(data.Lab_ID);
                                }
                                catch {}
                            }

                            // Handling Status: Update
                            try
                            {
                                bool isNeed_Update = false;

                                // ambil semua data yang baru di Insert diatas
                                // yang Status bukan "Invalid"
                                List<TEMPORARY_Geology_Pit_Monitoring> recordSet = (
                                                                                    from dt in db.TEMPORARY_Geology_Pit_Monitoring
                                                                                    orderby dt.RecordId
                                                                                    where dt.Header == id_Header //data yang baru di Insert diatas
                                                                                            && dt.Status != "Invalid"
                                                                                    select dt
                                                                                    ).ToList();

                                // proses RecordSet
                                foreach (TEMPORARY_Geology_Pit_Monitoring row in recordSet)
                                {
                                    // cari data "sebelumnya" di Table "Actual Upload"
                                    // punya Unique Key yang sama
                                    if (db.UPLOAD_Geology_Pit_Monitoring.Where(d => d.Lab_ID == row.Lab_ID // punya Unique yang sama   
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

                            overall_status = "Ok";
                            overall_message = "Success Upload on: " + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");
                        }
                    }
                    catch (Exception ex)
                    {
                        overall_message = "Error inner: " + ex.Message;
                    }
                }
            }
            catch (Exception ex)
            {
                overall_message = "Error " + ex.Message;
            }

            var result = new { Status = overall_status, Message = overall_message, Id = p_id_File_Physical };
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        private JsonResult Upload_Geology_Explorasi(UserX p_executedBy, string p_fileName, long p_id_File_Physical, string p_company)
        {
            string overall_message = string.Empty;
            string overall_status = "unknown";

            long id_Header = 0;

            try
            {
                string uploadSheetName = "Sheet1";

                // Open the spreadsheet document for read-only access.
                using (SpreadsheetDocument document = SpreadsheetDocument.Open(UploadFolder + p_fileName, false))
                {
                    // Retrieve a reference to the workbook part.
                    WorkbookPart wbPart = document.WorkbookPart;

                    // Find the sheet with the supplied name, and then use that 
                    // Sheet object to retrieve a reference to the first worksheet.
                    Sheet theSheet = wbPart.Workbook.Descendants<Sheet>().Where(s => s.Name == uploadSheetName).FirstOrDefault();

                    // Throw an exception if there is no sheet.
                    if (theSheet == null)
                    {
                        throw new ArgumentException("sheetName");
                    }

                    // Retrieve a reference to the worksheet part.
                    WorksheetPart wsPart = (WorksheetPart)(wbPart.GetPartById(theSheet.Id));

                    try
                    {
                        using (MERCY_Ctx db = new MERCY_Ctx())
                        {
                            var data_header = new TEMPORARY_Geology_Explorasi_Header
                            {
                                File_Physical = p_id_File_Physical,
                                Company = p_company,
                                Date_Detail = ExcelProcessing.GetCellValue(wbPart, wsPart, "N6"),
                                Job_No = ExcelProcessing.GetCellValue(wbPart, wsPart, "C8").Replace(": ", ""),
                                Report_To = ExcelProcessing.GetCellValue(wbPart, wsPart, "C9").Replace(": ", ""),
                                Date_Received = ExcelProcessing.GetCellValue(wbPart, wsPart, "C10").Replace(": ", ""),
                                Nomor = ExcelProcessing.GetCellValue(wbPart, wsPart, "C11").Replace(": ", ""),

                                CreatedOn = DateTime.Now,
                                CreatedBy = p_executedBy.UserId
                            };

                            db.TEMPORARY_Geology_Explorasi_Header.Add(data_header);
                            db.SaveChanges();

                            id_Header = data_header.RecordId;

                            // mulai dari Baris
                            int baris = 16;

                            List<string> keysUnique = new List<string>();
                            string status = "New";
                            long first_RecordId = 0;

                            bool complete = false;
                            decimal CONST_100 = 100.0M;

                            while ( ! complete)
                            {
                                if (string.IsNullOrEmpty(ExcelProcessing.GetCellValue(wbPart, wsPart, "B" + baris.ToString()))) break;

                                // init Status, start with "New"
                                status = "New";

                                var data = new TEMPORARY_Geology_Explorasi
                                {
                                    Header = id_Header,
                                    Sample_ID = ExcelProcessing.GetCellValue(wbPart, wsPart, "B" + baris.ToString()),
                                    SampleType = ExcelProcessing.GetCellValue(wbPart, wsPart, "C" + baris.ToString()),
                                    Lab_ID = ExcelProcessing.GetCellValue(wbPart, wsPart, "D" + baris.ToString()) + ExcelProcessing.GetCellValue(wbPart, wsPart, "E" + baris.ToString()),
                                    Mass_Spl = ExcelProcessing.GetCellValue(wbPart, wsPart, "F" + baris.ToString()),
                                    TM = ExcelProcessing.GetCellValue(wbPart, wsPart, "G" + baris.ToString()),
                                    M = ExcelProcessing.GetCellValue(wbPart, wsPart, "H" + baris.ToString()),
                                    VM = ExcelProcessing.GetCellValue(wbPart, wsPart, "I" + baris.ToString()),
                                    Ash = ExcelProcessing.GetCellValue(wbPart, wsPart, "J" + baris.ToString())
                                };
                                data.FC = OurUtility.ToDecimal((CONST_100 - OurUtility.ToDecimal(data.Ash, 2) - OurUtility.ToDecimal(data.VM, 2) - OurUtility.ToDecimal(data.M, 2)), 2);
                                data.TS = ExcelProcessing.GetCellValue(wbPart, wsPart, "L" + baris.ToString());
                                data.Cal_ad = ExcelProcessing.GetCellValue(wbPart, wsPart, "M" + baris.ToString());
                                data.Cal_db = OurUtility.ToDecimal(OurUtility.ToDecimal(data.Cal_ad, 0) * CONST_100 / (CONST_100 - OurUtility.ToDecimal(data.M, 2)), 0);
                                data.Cal_ar = OurUtility.ToDecimal(OurUtility.ToDecimal(data.Cal_ad, 0) * (CONST_100 - OurUtility.ToDecimal(data.TM, 2)) / (CONST_100 - OurUtility.ToDecimal(data.M, 2)), 0);
                                data.Cal_daf = OurUtility.ToDecimal(OurUtility.ToDecimal(data.Cal_ad, 0) * CONST_100 / (CONST_100 - OurUtility.ToDecimal(data.M, 2) - OurUtility.ToDecimal(data.Ash, 2)), 0);
                                data.RD = ExcelProcessing.GetCellValue(wbPart, wsPart, "Q" + baris.ToString());

                                data.CreatedOn = DateTime.Now;
                                data.CreatedBy = p_executedBy.UserId;

                                if (string.IsNullOrEmpty(data.Lab_ID))
                                {
                                    status = "Invalid";
                                }
                                // check with Current Collections
                                else if (keysUnique.Contains(data.Lab_ID))
                                {
                                    status = "Invalid";
                                }

                                data.Status = status;

                                db.TEMPORARY_Geology_Explorasi.Add(data);
                                db.SaveChanges();

                                // simpan Id pertama
                                if (first_RecordId <= 0) first_RecordId = data.RecordId;

                                // maju 1 baris
                                baris++;
                                // batas Jumlah Baris Data, yang boleh diproses
                                if (baris >= 200) break;

                                // for Next "Unique"
                                try
                                {
                                    keysUnique.Add(data.Lab_ID);
                                }
                                catch {}
                            }

                            // Handling Status: Update
                            try
                            {
                                bool isNeed_Update = false;

                                // ambil semua data yang baru di Insert diatas
                                // yang Status bukan "Invalid"
                                List<TEMPORARY_Geology_Explorasi> recordSet = (
                                                                                from dt in db.TEMPORARY_Geology_Explorasi
                                                                                orderby dt.RecordId
                                                                                where dt.Header == id_Header //data yang baru di Insert diatas
                                                                                        && dt.Status != "Invalid"
                                                                                select dt
                                                                                ).ToList();

                                // proses RecordSet
                                foreach (TEMPORARY_Geology_Explorasi row in recordSet)
                                {
                                    // cari data "sebelumnya" di Table "Actual Upload"
                                    // punya Unique Key yang sama
                                    if (db.UPLOAD_Geology_Explorasi.Where(d => d.Lab_ID == row.Lab_ID // punya Unique yang sama   
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

                            overall_status = "Ok";
                            overall_message = "Success Upload on: " + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");
                        }
                    }
                    catch (Exception ex)
                    {
                        overall_message = "Error inner: " + ex.Message;
                    }
                }
            }
            catch (Exception ex)
            {
                overall_message = "Error " + ex.Message;
            }

            var result = new { Status = overall_status, Message = overall_message, Id = p_id_File_Physical };
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        private JsonResult Upload_Sampling_ROM(UserX p_executedBy, string p_fileName, long p_id_File_Physical, string p_company)
        {
            string overall_message = string.Empty;
            string overall_status = "unknown";

            long id_Header = 0;

            try
            {
                string uploadSheetName = "ROM SPC ";

                // Open the spreadsheet document for read-only access.
                using (SpreadsheetDocument document = SpreadsheetDocument.Open(UploadFolder + p_fileName, false))
                {
                    // Retrieve a reference to the workbook part.
                    WorkbookPart wbPart = document.WorkbookPart;

                    // Find the sheet with the supplied name, and then use that 
                    // Sheet object to retrieve a reference to the first worksheet.
                    Sheet theSheet = wbPart.Workbook.Descendants<Sheet>().Where(s => s.Name == uploadSheetName).FirstOrDefault();

                    // Throw an exception if there is no sheet.
                    if (theSheet == null)
                    {
                        throw new ArgumentException("sheetName");
                    }

                    // Retrieve a reference to the worksheet part.
                    WorksheetPart wsPart = (WorksheetPart)(wbPart.GetPartById(theSheet.Id));

                    try
                    {
                        using (MERCY_Ctx db = new MERCY_Ctx())
                        {
                            var data_header = new TEMPORARY_Sampling_ROM_Header
                            {
                                File_Physical = p_id_File_Physical,
                                Company = p_company,
                                Date_Detail = ExcelProcessing.GetCellValue(wbPart, wsPart, "L7"),
                                Job_No = ExcelProcessing.GetCellValue(wbPart, wsPart, "C9").Replace(": ", ""),
                                Report_To = ExcelProcessing.GetCellValue(wbPart, wsPart, "C10").Replace(": ", ""),
                                Method1 = ExcelProcessing.GetCellValue(wbPart, wsPart, "C11").Replace(": ", ""),
                                Method2 = ExcelProcessing.GetCellValue(wbPart, wsPart, "C12").Replace(": ", ""),
                                Method3 = ExcelProcessing.GetCellValue(wbPart, wsPart, "C13").Replace(": ", ""),
                                Method4 = ExcelProcessing.GetCellValue(wbPart, wsPart, "C14").Replace(": ", ""),

                                CreatedOn = DateTime.Now,
                                CreatedBy = p_executedBy.UserId
                            };

                            db.TEMPORARY_Sampling_ROM_Header.Add(data_header);
                            db.SaveChanges();

                            id_Header = data_header.RecordId;

                            // mulai dari Baris
                            int baris = 18;

                            List<string> keysUnique = new List<string>();
                            string status = "New";
                            long first_RecordId = 0;
                            
                            bool complete = false;
                            while (!complete)
                            {
                                if (string.IsNullOrEmpty(ExcelProcessing.GetCellValue(wbPart, wsPart, "B" + baris.ToString()))) break;

                                // init Status, start with "New"
                                status = "New";

                                var data = new TEMPORARY_Sampling_ROM
                                {
                                    Header = id_Header,
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
                                    CreatedBy = p_executedBy.UserId
                                };

                                if (string.IsNullOrEmpty(data.Lab_ID))
                                {
                                    status = "Invalid";
                                }
                                // check with Current Collections
                                else if (keysUnique.Contains(data.Lab_ID))
                                {
                                    status = "Invalid";
                                }

                                data.Status = status;

                                db.TEMPORARY_Sampling_ROM.Add(data);
                                db.SaveChanges();

                                // simpan Id pertama
                                if (first_RecordId <= 0) first_RecordId = data.RecordId;

                                // maju 1 baris
                                baris++;
                                // batas Jumlah Baris Data, yang boleh diproses
                                if (baris >= 200) break;

                                // for Next "Unique"
                                try
                                {
                                    keysUnique.Add(data.Lab_ID);
                                }
                                catch {}
                            }

                            // Handling Status: Update
                            try
                            {
                                bool isNeed_Update = false;

                                // ambil semua data yang baru di Insert diatas
                                // yang Status bukan "Invalid"
                                List<TEMPORARY_Sampling_ROM> recordSet = (
                                                                            from dt in db.TEMPORARY_Sampling_ROM
                                                                            orderby dt.RecordId
                                                                            where dt.Header == id_Header //data yang baru di Insert diatas
                                                                                    && dt.Status != "Invalid"
                                                                            select dt
                                                                         ).ToList();

                                // proses RecordSet
                                foreach (TEMPORARY_Sampling_ROM row in recordSet)
                                {
                                    // cari data "sebelumnya" di Table "Actual Upload"
                                    // punya Unique Key yang sama
                                    if (db.UPLOAD_Sampling_ROM.Where(d => d.Lab_ID == row.Lab_ID // punya Unique yang sama   
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

                            overall_status = "Ok";
                            overall_message = "Success Upload on: " + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");
                        }
                    }
                    catch (Exception ex)
                    {
                        overall_message = "Error inner: " + ex.Message;
                    }
                }
            }
            catch (Exception ex)
            {
                overall_message = "Error " + ex.Message;
            }

            var result = new { Status = overall_status, Message = overall_message, Id = p_id_File_Physical };
            return Json(result, JsonRequestBehavior.AllowGet);
        }
        
        private JsonResult Upload_CRUSHING_PLANT(UserX p_executedBy, string p_fileName, long p_id_File_Physical, string p_company)
        {
            string overall_message = string.Empty;
            string overall_status = "unknown";

            long id_Header = 0;

            try
            {
                string uploadSheetName = string.Empty;

                Configuration config = new Configuration();
                string products = config.Excel_Product_TCM;

                switch (p_company)
                {
                    case "TCM":
                        products = config.Excel_Product_TCM;
                        break;
                    case "BEK":
                        products = config.Excel_Product_BEK;
                        break;
                }

                Sheet theSheet = null;

                // Open the spreadsheet document for read-only access.
                using (SpreadsheetDocument document = SpreadsheetDocument.Open(UploadFolder + p_fileName, false))
                {
                    // Retrieve a reference to the workbook part.
                    WorkbookPart wbPart = document.WorkbookPart;

                    bool isSheets_Valid = true;
                    string[] sheets_products = products.Split(',');
                    foreach (string sheet_product in sheets_products)
                    {
                        uploadSheetName = sheet_product;

                        try
                        {
                            theSheet = wbPart.Workbook.Descendants<Sheet>().Where(s => s.Name == uploadSheetName).FirstOrDefault();

                            if (theSheet == null)
                            {
                                isSheets_Valid = false;
                                overall_message = string.Format(@"Sheet [{0}] not found!###Sheets required: [{1}]", uploadSheetName, products);
                                break;
                            }
                        }
                        catch
                        {
                            isSheets_Valid = false;
                            overall_message = string.Format(@"Sheet [{0}] not found!###Sheets required: [{1}]", uploadSheetName, products);
                            break;
                        }
                    }

                    if ( ! isSheets_Valid)
                    {
                        var result3 = new { Status = overall_status, Message = overall_message, Id = p_id_File_Physical };
                        return Json(result3, JsonRequestBehavior.AllowGet);
                    }

                    string tm_plan = string.Empty;
                    string ash_plan = string.Empty;
                    string ts_plan = string.Empty;
                    string cv_plan = string.Empty;

                    foreach (string sheet_product in sheets_products)
                    {
                        uploadSheetName = sheet_product;

                        tm_plan = string.Empty;
                        ash_plan = string.Empty;
                        ts_plan = string.Empty;
                        cv_plan = string.Empty;

                        // Find the sheet with the supplied name, and then use that 
                        // Sheet object to retrieve a reference to the first worksheet.
                        theSheet = wbPart.Workbook.Descendants<Sheet>().Where(s => s.Name == uploadSheetName).FirstOrDefault();

                        // Throw an exception if there is no sheet.
                        if (theSheet == null)
                        {
                            throw new ArgumentException("sheetName");
                        }

                        // Retrieve a reference to the worksheet part.
                        WorksheetPart wsPart = (WorksheetPart)(wbPart.GetPartById(theSheet.Id));

                        try
                        {
                            using (MERCY_Ctx db = new MERCY_Ctx())
                            {
                                var data_header = new TEMPORARY_CRUSHING_PLANT_Header
                                {
                                    Sheet = uploadSheetName.Trim(),
                                    File_Physical = p_id_File_Physical,
                                    Company = p_company,
                                    Date_Detail = ExcelProcessing.GetCellValue(wbPart, wsPart, "U7"),
                                    Job_No = ExcelProcessing.GetCellValue(wbPart, wsPart, "C9").Replace(": ", ""),
                                    Report_To = ExcelProcessing.GetCellValue(wbPart, wsPart, "C10").Replace(": ", ""),
                                    Method1 = ExcelProcessing.GetCellValue(wbPart, wsPart, "C11").Replace(": ", ""),
                                    Method2 = ExcelProcessing.GetCellValue(wbPart, wsPart, "C12").Replace(": ", ""),
                                    Method3 = ExcelProcessing.GetCellValue(wbPart, wsPart, "C13").Replace(": ", ""),
                                    Method4 = ExcelProcessing.GetCellValue(wbPart, wsPart, "C14").Replace(": ", ""),

                                    CreatedOn = DateTime.Now,
                                    CreatedBy = p_executedBy.UserId
                                };

                                db.TEMPORARY_CRUSHING_PLANT_Header.Add(data_header);
                                db.SaveChanges();

                                id_Header = data_header.RecordId;

                                // mulai dari Baris
                                int baris = 18;

                                List<string> keysUnique = new List<string>();
                                string status = "New";
                                long first_RecordId = 0;

                                string dateProduction = string.Empty;
                                bool complete = false;
                                decimal CONST_100 = 100.0M;

                                while ( ! complete)
                                {
                                    dateProduction = ExcelProcessing.GetCellValue(wbPart, wsPart, "A" + baris.ToString());

                                    if (string.IsNullOrEmpty(dateProduction)) break;
                                    if (dateProduction == "Cummulative Result") break;

                                    // init Status, start with "New"
                                    status = "New";

                                    var data = new TEMPORARY_CRUSHING_PLANT
                                    {
                                        Header = id_Header,
                                        Date_Production = ExcelProcessing.GetDate(dateProduction, "yyyy-MM-dd"),
                                        Shift_Work = ExcelProcessing.GetCellValue(wbPart, wsPart, "B" + baris.ToString()),
                                        Tonnage = ExcelProcessing.GetCellValue(wbPart, wsPart, "C" + baris.ToString()),
                                        Sample_ID = ExcelProcessing.GetCellValue(wbPart, wsPart, "D" + baris.ToString()),

                                        TM = ExcelProcessing.GetCellValue(wbPart, wsPart, "E" + baris.ToString()),
                                        M = ExcelProcessing.GetCellValue(wbPart, wsPart, "F" + baris.ToString()),
                                        ASH_adb = ExcelProcessing.GetCellValue(wbPart, wsPart, "G" + baris.ToString())
                                    };
                                    data.ASH_arb = OurUtility.ToDecimal(OurUtility.ToDecimal(data.ASH_adb, 2) * (CONST_100 - OurUtility.ToDecimal(data.TM, 2)) / (CONST_100 - OurUtility.ToDecimal(data.M, 2)), 2);
                                    data.VM_adb = ExcelProcessing.GetCellValue(wbPart, wsPart, "I" + baris.ToString());
                                    data.VM_arb = OurUtility.ToDecimal(OurUtility.ToDecimal(data.VM_adb, 2) * (CONST_100 - OurUtility.ToDecimal(data.TM, 2)) / (CONST_100 - OurUtility.ToDecimal(data.M, 2)), 2);
                                    data.FC_adb = OurUtility.ToDecimal(CONST_100 - OurUtility.ToDecimal(data.M, 2) - OurUtility.ToDecimal(data.ASH_adb, 2) - OurUtility.ToDecimal(data.VM_adb, 2), 2);
                                    data.FC_arb = OurUtility.ToDecimal(OurUtility.ToDecimal(data.FC_adb, 2) * (CONST_100 - OurUtility.ToDecimal(data.TM, 2)) / (CONST_100 - OurUtility.ToDecimal(data.M, 2)), 2);
                                    data.TS_adb = ExcelProcessing.GetCellValue(wbPart, wsPart, "M" + baris.ToString());
                                    data.TS_arb = OurUtility.ToDecimal(OurUtility.ToDecimal(data.TS_adb, 2) * (CONST_100 - OurUtility.ToDecimal(data.TM, 2)) / (CONST_100 - OurUtility.ToDecimal(data.M, 2)), 2);
                                    data.CV_adb = ExcelProcessing.GetCellValue(wbPart, wsPart, "O" + baris.ToString());
                                    data.CV_db = OurUtility.ToDecimal(OurUtility.ToDecimal(data.CV_adb, 0) * (CONST_100) / (CONST_100 - OurUtility.ToDecimal(data.M, 2)), 0);
                                    data.CV_arb = OurUtility.ToDecimal(OurUtility.ToDecimal(data.CV_adb, 0) * (CONST_100 - OurUtility.ToDecimal(data.TM, 2)) / (CONST_100 - OurUtility.ToDecimal(data.M, 2)), 0);
                                    data.CV_daf = OurUtility.ToDecimal(OurUtility.ToDecimal(data.CV_adb, 0) * (CONST_100) / (CONST_100 - OurUtility.ToDecimal(data.M, 2) - OurUtility.ToDecimal(data.ASH_adb, 2)), 0);
                                    data.CV_ad_15 = OurUtility.ToDecimal(OurUtility.ToDecimal(data.CV_adb, 0) * (CONST_100 - 15) / (CONST_100 - OurUtility.ToDecimal(data.M, 2)), 0);
                                    data.CV_ad_16 = OurUtility.ToDecimal(OurUtility.ToDecimal(data.CV_adb, 0) * (CONST_100 - 16) / (CONST_100 - OurUtility.ToDecimal(data.M, 2)), 0);
                                    data.CV_ad_17 = OurUtility.ToDecimal(OurUtility.ToDecimal(data.CV_adb, 0) * (CONST_100 - 17) / (CONST_100 - OurUtility.ToDecimal(data.M, 2)), 0);

                                    data.Remark = ExcelProcessing.GetCellValue(wbPart, wsPart, "V" + baris.ToString());

                                    if (string.IsNullOrEmpty(tm_plan))
                                    {
                                        tm_plan = ExcelProcessing.GetCellValue(wbPart, wsPart, "W" + baris.ToString());
                                        ash_plan = ExcelProcessing.GetCellValue(wbPart, wsPart, "Y" + baris.ToString());
                                        ts_plan = ExcelProcessing.GetCellValue(wbPart, wsPart, "AA" + baris.ToString());
                                        cv_plan = ExcelProcessing.GetCellValue(wbPart, wsPart, "AC" + baris.ToString());
                                    }

                                    data.TM_Plan = tm_plan;
                                    data.ASH_Plan = ash_plan;
                                    data.TS_Plan = ts_plan;
                                    data.CV_Plan = cv_plan;

                                    data.CreatedOn = DateTime.Now;
                                    data.CreatedBy = p_executedBy.UserId;

                                    if (string.IsNullOrEmpty(data.Sample_ID))
                                    {
                                        status = "Invalid";
                                    }
                                    // check with Current Collections
                                    else if (keysUnique.Contains(data.Sample_ID))
                                    {
                                        status = "Invalid";
                                    }

                                    data.Status = status;

                                    db.TEMPORARY_CRUSHING_PLANT.Add(data);
                                    db.SaveChanges();

                                    // simpan Id pertama
                                    if (first_RecordId <= 0) first_RecordId = data.RecordId;

                                    // maju 1 baris
                                    baris++;
                                    // batas Jumlah Baris Data, yang boleh diproses
                                    if (baris >= 200) break;

                                    // for Next "Unique"
                                    try
                                    {
                                        keysUnique.Add(data.Sample_ID);
                                    }
                                    catch {}
                                }

                                // Handling Status: Update
                                try
                                {
                                    bool isNeed_Update = false;

                                    // ambil semua data yang baru di Insert diatas
                                    // yang Status bukan "Invalid"
                                    List<TEMPORARY_CRUSHING_PLANT> recordSet = (
                                                                                from dt in db.TEMPORARY_CRUSHING_PLANT
                                                                                orderby dt.RecordId
                                                                                where dt.Header == id_Header // data yang baru di Insert diatas
                                                                                        && dt.Status != "Invalid"
                                                                                select dt
                                                                                ).ToList();

                                    // proses RecordSet
                                    foreach (TEMPORARY_CRUSHING_PLANT row in recordSet)
                                    {
                                        // cari data "sebelumnya" di Table "Actual Upload"
                                        // punya Unique Key yang sama
                                        if (db.UPLOAD_CRUSHING_PLANT.Where(d =>
                                                                                d.RecordId < first_RecordId // data "sebelumnya"
                                                                                && d.Sample_ID == row.Sample_ID // punya Unique yang sama   
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

                                overall_status = "Ok";
                                overall_message = "Success Upload on: " + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");
                            }
                        }
                        catch (Exception ex)
                        {
                            overall_message = "Error inner: " + ex.Message;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                overall_message = "Error " + ex.Message;
            }

            var result = new { Status=overall_status, Message=overall_message, Id=p_id_File_Physical };
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        private JsonResult Upload_BARGE_LOADING(UserX p_executedBy, string p_fileName, long p_id_File_Physical, string p_company)
        {
            string overall_message = string.Empty;
            string overall_status = "unknown";

            long id_Header = 0;

            try
            {
                string uploadSheetName = string.Empty;

                Configuration config = new Configuration();
                string products = config.Excel_Product_TCM;

                switch (p_company)
                {
                    case "TCM":
                        products = config.Excel_Product_TCM;
                        break;
                    case "BEK":
                        products = config.Excel_Product_BEK;
                        break;
                }

                Sheet theSheet = null;

                // Open the spreadsheet document for read-only access.
                using (SpreadsheetDocument document = SpreadsheetDocument.Open(UploadFolder + p_fileName, false))
                {
                    // Retrieve a reference to the workbook part.
                    WorkbookPart wbPart = document.WorkbookPart;

                    bool isSheets_Valid = true;
                    string[] sheets_products = products.Split(',');
                    foreach (string sheet_product in sheets_products)
                    {
                        uploadSheetName = sheet_product;

                        try
                        {
                            theSheet = wbPart.Workbook.Descendants<Sheet>().Where(s => s.Name == uploadSheetName).FirstOrDefault();

                            if (theSheet == null)
                            {
                                isSheets_Valid = false;
                                overall_message = string.Format(@"Sheet [{0}] not found!###Sheets required: [{1}]", uploadSheetName, products);
                                break;
                            }
                        }
                        catch
                        {
                            isSheets_Valid = false;
                            overall_message = string.Format(@"Sheet [{0}] not found!###Sheets required: [{1}]", uploadSheetName, products);
                            break;
                        }
                    }

                    if ( ! isSheets_Valid)
                    {
                        var result3 = new { Status = overall_status, Message = overall_message, Id = p_id_File_Physical };
                        return Json(result3, JsonRequestBehavior.AllowGet);
                    }

                    string tm_plan = string.Empty;
                    string ash_plan = string.Empty;
                    string ts_plan = string.Empty;
                    string cv_plan = string.Empty;

                    foreach (string sheet_product in sheets_products)
                    {
                        uploadSheetName = sheet_product;

                        tm_plan = string.Empty;
                        ash_plan = string.Empty;
                        ts_plan = string.Empty;
                        cv_plan = string.Empty;

                        // Find the sheet with the supplied name, and then use that 
                        // Sheet object to retrieve a reference to the first worksheet.
                        theSheet = wbPart.Workbook.Descendants<Sheet>().Where(s => s.Name == uploadSheetName).FirstOrDefault();

                        // Throw an exception if there is no sheet.
                        if (theSheet == null)
                        {
                            throw new ArgumentException("sheetName");
                        }

                        // Retrieve a reference to the worksheet part.
                        WorksheetPart wsPart = (WorksheetPart)(wbPart.GetPartById(theSheet.Id));

                        try
                        {
                            using (MERCY_Ctx db = new MERCY_Ctx())
                            {
                                var data_header = new TEMPORARY_BARGE_LOADING_Header
                                {
                                    Sheet = uploadSheetName.Trim(),
                                    File_Physical = p_id_File_Physical,
                                    Company = p_company,
                                    Date_Detail = ExcelProcessing.GetCellValue(wbPart, wsPart, "I7"),
                                    Job_No = ExcelProcessing.GetCellValue(wbPart, wsPart, "C9").Replace(": ", ""),
                                    Report_To = ExcelProcessing.GetCellValue(wbPart, wsPart, "C10").Replace(": ", ""),
                                    Method1 = ExcelProcessing.GetCellValue(wbPart, wsPart, "C11").Replace(": ", ""),
                                    Method2 = ExcelProcessing.GetCellValue(wbPart, wsPart, "C12").Replace(": ", ""),
                                    Method3 = ExcelProcessing.GetCellValue(wbPart, wsPart, "C13").Replace(": ", ""),
                                    Method4 = ExcelProcessing.GetCellValue(wbPart, wsPart, "C14").Replace(": ", ""),

                                    CreatedOn = DateTime.Now,
                                    CreatedBy = p_executedBy.UserId
                                };

                                db.TEMPORARY_BARGE_LOADING_Header.Add(data_header);
                                db.SaveChanges();

                                id_Header = data_header.RecordId;

                                // mulai dari Baris
                                int baris = 18;

                                List<string> keysUnique = new List<string>();
                                string status = "New";
                                long first_RecordId = 0;

                                string no = string.Empty;
                                bool complete = false;
                                decimal CONST_100 = 100.0M;

                                while ( ! complete)
                                {
                                    no = ExcelProcessing.GetCellValue(wbPart, wsPart, "A" + baris.ToString());

                                    if (string.IsNullOrEmpty(no)) break;
                                    if (no == "Cummulative Result") break;

                                    // init Status, start with "New"
                                    status = "New";

                                    var data = new TEMPORARY_BARGE_LOADING
                                    {
                                        Header = id_Header,
                                        JOB_Number = ExcelProcessing.GetCellValue(wbPart, wsPart, "B" + baris.ToString()),
                                        ID_Number = ExcelProcessing.GetCellValue(wbPart, wsPart, "C" + baris.ToString()),
                                        Service_Trip_Number = ExcelProcessing.GetCellValue(wbPart, wsPart, "D" + baris.ToString()),
                                        Date_Sampling = ExcelProcessing.GetDate(ExcelProcessing.GetCellValue(wbPart, wsPart, "E" + baris.ToString()), "yyyy-MM-dd"),
                                        Date_Report = ExcelProcessing.GetDate(ExcelProcessing.GetCellValue(wbPart, wsPart, "F" + baris.ToString()), "yyyy-MM-dd"),
                                        Tonnage = ExcelProcessing.GetCellValue(wbPart, wsPart, "G" + baris.ToString()),
                                        Name = ExcelProcessing.GetCellValue(wbPart, wsPart, "H" + baris.ToString()),
                                        Destination = ExcelProcessing.GetCellValue(wbPart, wsPart, "I" + baris.ToString()),
                                        Temperature = ExcelProcessing.GetCellValue(wbPart, wsPart, "J" + baris.ToString()),

                                        TM = ExcelProcessing.GetCellValue(wbPart, wsPart, "K" + baris.ToString()),
                                        M = ExcelProcessing.GetCellValue(wbPart, wsPart, "L" + baris.ToString()),
                                        ASH_adb = ExcelProcessing.GetCellValue(wbPart, wsPart, "M" + baris.ToString())
                                    };
                                    data.ASH_arb = OurUtility.ToDecimal(OurUtility.ToDecimal(data.ASH_adb, 2) * (CONST_100 - OurUtility.ToDecimal(data.TM, 2)) / (CONST_100 - OurUtility.ToDecimal(data.M, 2)), 2);
                                    data.VM_adb = ExcelProcessing.GetCellValue(wbPart, wsPart, "O" + baris.ToString());
                                    data.VM_arb = OurUtility.ToDecimal(OurUtility.ToDecimal(data.VM_adb, 2) * (CONST_100 - OurUtility.ToDecimal(data.TM, 2)) / (CONST_100 - OurUtility.ToDecimal(data.M, 2)), 2);
                                    data.FC_adb = OurUtility.ToDecimal(CONST_100 - OurUtility.ToDecimal(data.M, 2) - OurUtility.ToDecimal(data.ASH_adb, 2) - OurUtility.ToDecimal(data.VM_adb, 2), 2);
                                    data.FC_arb = OurUtility.ToDecimal(OurUtility.ToDecimal(data.FC_adb, 2) * (CONST_100 - OurUtility.ToDecimal(data.TM, 2)) / (CONST_100 - OurUtility.ToDecimal(data.M, 2)), 2);
                                    data.TS_adb = ExcelProcessing.GetCellValue(wbPart, wsPart, "S" + baris.ToString());
                                    data.TS_arb = OurUtility.ToDecimal(OurUtility.ToDecimal(data.TS_adb, 2) * (CONST_100 - OurUtility.ToDecimal(data.TM, 2)) / (CONST_100 - OurUtility.ToDecimal(data.M, 2)), 2);
                                    data.CV_adb = ExcelProcessing.GetCellValue(wbPart, wsPart, "U" + baris.ToString());
                                    data.CV_db = OurUtility.ToDecimal(OurUtility.ToDecimal(data.CV_adb, 0) * (CONST_100) / (CONST_100 - OurUtility.ToDecimal(data.M, 2)), 0);
                                    data.CV_arb = OurUtility.ToDecimal(OurUtility.ToDecimal(data.CV_adb, 0) * (CONST_100 - OurUtility.ToDecimal(data.TM, 2)) / (CONST_100 - OurUtility.ToDecimal(data.M, 2)), 0);
                                    data.CV_daf = OurUtility.ToDecimal(OurUtility.ToDecimal(data.CV_adb, 0) * (CONST_100) / (CONST_100 - OurUtility.ToDecimal(data.M, 2) - OurUtility.ToDecimal(data.ASH_adb, 2)), 0);
                                    data.CV_ad_15 = OurUtility.ToDecimal(OurUtility.ToDecimal(data.CV_adb, 0) * (CONST_100 - 15) / (CONST_100 - OurUtility.ToDecimal(data.M, 2)), 0);
                                    data.CV_ad_16 = OurUtility.ToDecimal(OurUtility.ToDecimal(data.CV_adb, 0) * (CONST_100 - 16) / (CONST_100 - OurUtility.ToDecimal(data.M, 2)), 0);
                                    data.CV_ad_17 = OurUtility.ToDecimal(OurUtility.ToDecimal(data.CV_adb, 0) * (CONST_100 - 17) / (CONST_100 - OurUtility.ToDecimal(data.M, 2)), 0);

                                    data.Remark = ExcelProcessing.GetCellValue(wbPart, wsPart, "AB" + baris.ToString());

                                    if (string.IsNullOrEmpty(tm_plan))
                                    {
                                        tm_plan = ExcelProcessing.GetCellValue(wbPart, wsPart, "AC" + baris.ToString());
                                        ash_plan = ExcelProcessing.GetCellValue(wbPart, wsPart, "AE" + baris.ToString());
                                        ts_plan = ExcelProcessing.GetCellValue(wbPart, wsPart, "AG" + baris.ToString());
                                        cv_plan = ExcelProcessing.GetCellValue(wbPart, wsPart, "AI" + baris.ToString());
                                    }

                                    data.TM_Plan = tm_plan;
                                    data.ASH_Plan = ash_plan;
                                    data.TS_Plan = ts_plan;
                                    data.CV_Plan = cv_plan;

                                    data.CreatedOn = DateTime.Now;
                                    data.CreatedBy = p_executedBy.UserId;

                                    if (string.IsNullOrEmpty(data.ID_Number))
                                    {
                                        status = "Invalid";
                                    }
                                    // check with Current Collections
                                    else if (keysUnique.Contains(data.ID_Number))
                                    {
                                        status = "Invalid";
                                    }

                                    data.Status = status;

                                    db.TEMPORARY_BARGE_LOADING.Add(data);
                                    db.SaveChanges();

                                    // simpan Id pertama
                                    if (first_RecordId <= 0) first_RecordId = data.RecordId;

                                    // maju 1 baris
                                    baris++;
                                    // batas Jumlah Baris Data, yang boleh diproses
                                    if (baris >= 200) break;

                                    // for Next "Unique"
                                    try
                                    {
                                        keysUnique.Add(data.ID_Number);
                                    }
                                    catch {}
                                }

                                // Handling Status: Update
                                try
                                {
                                    bool isNeed_Update = false;

                                    // ambil semua data yang baru di Insert diatas
                                    // yang Status bukan "Invalid"
                                    List<TEMPORARY_BARGE_LOADING> recordSet = (
                                                                                from dt in db.TEMPORARY_BARGE_LOADING
                                                                                orderby dt.RecordId
                                                                                where dt.Header == id_Header // data yang baru di Insert diatas
                                                                                        && dt.Status != "Invalid"
                                                                                select dt
                                                                                ).ToList();

                                    // proses RecordSet
                                    foreach (TEMPORARY_BARGE_LOADING row in recordSet)
                                    {
                                        // cari data "sebelumnya" di Table "Actual Upload"
                                        // punya Unique Key yang sama
                                        if (db.UPLOAD_BARGE_LOADING.Where(d =>
                                                                                d.RecordId < first_RecordId // data "sebelumnya"
                                                                                && d.ID_Number == row.ID_Number // punya Unique yang sama   
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

                                overall_status = "Ok";
                                overall_message = "Success Upload on: " + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");
                            }
                        }
                        catch (Exception ex)
                        {
                            overall_message = "Error inner: " + ex.Message;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                overall_message = "Error " + ex.Message;
            }

            var result = new { Status = overall_status, Message = overall_message, Id = p_id_File_Physical };
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        public ActionResult Download()
        {
            var dir = Server.MapPath("/temp");
            var path = string.Empty;

            try
            {
                string fileName = Request[".id"];

                path = Path.Combine(dir, fileName);
                if (System.IO.File.Exists(path))
                {
                    return base.File(path, "application/ms-excel", fileName);
                }
                else
                {
                    // ???
                    return null;
                }
            }
            catch
            {
                // ???
                return null;
            }
        }

        public ActionResult DownloadAllType()
        {
            var dir = Server.MapPath("/temp");
            var path = string.Empty;

            try
            {
                string fileName = Request[".id"];

                FileInfo fi = new FileInfo(fileName);
                var ext = fi.Extension;

                path = Path.Combine(dir, fileName);
                if (System.IO.File.Exists(path))
                {
                    return File(path, ext, fileName);
                }
                else
                {
                    return null;
                }
            }
            catch
            {
                return null;
            }
        }

        public JsonResult BARGE_LOADING()
        {
            // -- Permission validation
            // get Current Url
            string url_API = Request.Url.AbsolutePath.ToLower();

            // Check Permission based on Current Url
            //Permission.Check(url_API, ref is_view, ref is_add, ref is_delete, ref is_edit, ref is_active);

            // -- Calculation for isview
            is_view = (is_view || is_add || is_delete || is_edit || is_active);

            if (!is_view)
            {
                var msg = new { Data = string.Empty, Total = 0, AggregateResults = string.Empty, Errors = "" };
                return Json(msg, JsonRequestBehavior.AllowGet);
            }

            long id = OurUtility.ToInt64(Request[".id"]);

            // -- Actual code
            JsonResult result = Json(string.Empty, JsonRequestBehavior.AllowGet);

            using (MERCY_Ctx db = new MERCY_Ctx())
            {
                var dataQuery =
                        (
                            from dt in db.TEMPORARY_BARGE_LOADING
                            join head in db.TEMPORARY_BARGE_LOADING_Header on dt.Header equals head.RecordId
                            orderby dt.RecordId
                            where head.File_Physical == id
                                //&& head.Sheet == sheet
                            select new Model_View_TEMPORARY_TEMPORARY_BARGE_LOADING
                            {
                                data_RecordId = dt.RecordId
                                , JOB_Number = dt.JOB_Number
                                , ID_Number = dt.ID_Number
                                , Service_Trip_Number =  dt.Service_Trip_Number
                                , Date_Sampling = dt.Date_Sampling
                                , Date_Report = dt.Date_Report
                                , Tonnage = dt.Tonnage
                                , Name = dt.Name
                                , Destination = dt.Destination
                                , Temperature = dt.Temperature
                                , TM = dt.TM
                                , M = dt.M
                                , ASH_adb =  dt.ASH_adb
                                , ASH_arb = dt.ASH_arb
                                , VM_adb = dt.VM_adb
                                , VM_arb = dt.VM_arb
                                , FC_adb = dt.FC_adb
                                , FC_arb = dt.FC_arb
                                , TS_adb = dt.TS_adb
                                , TS_arb = dt.TS_arb
                                , CV_adb = dt.CV_adb
                                , CV_db = dt.CV_db
                                , CV_arb = dt.CV_arb
                                , CV_daf = dt.CV_daf
                                , CV_ad_15 = dt.CV_ad_15
                                , CV_ad_16 = dt.CV_ad_16
                                , CV_ad_17 = dt.CV_ad_17
                                , Remark = dt.Remark
                                , Status = dt.Status
                                , Sheet = head.Sheet

                                , TM_Plan = dt.TM_Plan
                                , ASH_Plan = dt.ASH_Plan
                                , TS_Plan = dt.TS_Plan
                                , CV_Plan = dt.CV_Plan
                            }
                        );

                var data = dataQuery.ToList();

                data.ForEach(c =>
                {
                    c.Tonnage_Str = OurUtility.ToDecimal(c.Tonnage).ToString("0.000");
                    c.Temperature_Str = OurUtility.ToDecimal(c.Temperature).ToString("0.00");
                    c.TM_Str = OurUtility.ToDecimal(c.TM).ToString("0.00");
                    c.M_Str = OurUtility.ToDecimal(c.M).ToString("0.00");
                    c.ASH_adb_Str = OurUtility.ToDecimal(c.ASH_adb).ToString("0.00");
                    c.ASH_arb_Str = c.ASH_arb.ToString("0.00");
                    c.VM_adb_Str = OurUtility.ToDecimal(c.VM_adb).ToString("0.00");
                    c.VM_arb_Str = c.VM_arb.ToString("0.00");
                    c.FC_adb_Str = c.FC_adb.ToString("0.00");
                    c.FC_arb_Str = c.FC_arb.ToString("0.00");
                    c.TS_adb_Str = OurUtility.ToDecimal(c.TS_adb).ToString("0.00");
                    c.TS_arb_Str = c.TS_arb.ToString("0.00");
                    c.CV_adb_Str = String.Format("{0:n0}", OurUtility.ToDecimal(c.CV_adb, 0));
                    c.CV_db_Str = String.Format("{0:n0}", Decimal.Round(c.CV_db, 0));
                    c.CV_arb_Str = String.Format("{0:n0}", Decimal.Round(c.CV_arb, 0));
                    c.CV_daf_Str = String.Format("{0:n0}", Decimal.Round(c.CV_daf, 0));
                    c.CV_ad_15_Str = String.Format("{0:n0}", Decimal.Round(c.CV_ad_15, 0));
                    c.CV_ad_16_Str = String.Format("{0:n0}", Decimal.Round(c.CV_ad_16, 0));
                    c.CV_ad_17_Str = String.Format("{0:n0}", Decimal.Round(c.CV_ad_17, 0));

                    c.TM_Plan_Str = OurUtility.ToDecimal(c.TM_Plan).ToString("0.00");
                    c.ASH_Plan_Str = OurUtility.ToDecimal(c.ASH_Plan).ToString("0.00");
                    c.TS_Plan_Str = OurUtility.ToDecimal(c.TS_Plan).ToString("0.00");
                    c.CV_Plan_Str = c.CV_Plan;
                });

                var draw = OurUtility.ValueOf(Request, "draw");
                var recordsTotal = dataQuery.Count();

                return Json(new { draw, recordsFiltered = recordsTotal, recordsTotal, data }, JsonRequestBehavior.AllowGet);
            }
        }

        public JsonResult BARGE_LOADING_Header()
        {
            // -- Permission validation
            // get Current Url
            string url_API = Request.Url.AbsolutePath.ToLower();

            // Check Permission based on Current Url
            //Permission.Check(url_API, ref is_view, ref is_add, ref is_delete, ref is_edit, ref is_active);

            // -- Calculation for isview
            is_view = (is_view || is_add || is_delete || is_edit || is_active);

            if (!is_view)
            {
                var msg = new { Data = string.Empty, Total = 0, AggregateResults = string.Empty, Errors = "" };
                return Json(msg, JsonRequestBehavior.AllowGet);
            }

            long id = OurUtility.ToInt64(Request[".id"]);

            // -- Actual code
            JsonResult result = Json(string.Empty, JsonRequestBehavior.AllowGet);

            using (MERCY_Ctx db = new MERCY_Ctx())
            {
                var dataQuery =
                        (
                            from dt in db.TEMPORARY_BARGE_LOADING_Header
                            join f in db.TEMPORARY_File on dt.File_Physical equals f.RecordId
                            where dt.File_Physical == id
                            select new
                            {
                                dt.Date_Detail
                                , dt.Job_No
                                , dt.Report_To
                                , dt.Method1
                                , dt.Method2
                                , dt.Method3
                                , dt.Method4
                                , f.FileName
                                , f.Link
                            }
                        );

                result = Json(dataQuery.ToList(), JsonRequestBehavior.AllowGet);
            }

            return result;
        }

        public JsonResult CRUSHING_PLANT()
        {
            // -- Permission validation
            // get Current Url
            string url_API = Request.Url.AbsolutePath.ToLower();

            // Check Permission based on Current Url
            //Permission.Check(url_API, ref is_view, ref is_add, ref is_delete, ref is_edit, ref is_active);

            // -- Calculation for isview
            is_view = (is_view || is_add || is_delete || is_edit || is_active);

            if (!is_view)
            {
                var msg = new { Data = string.Empty, Total = 0, AggregateResults = string.Empty, Errors = "" };
                return Json(msg, JsonRequestBehavior.AllowGet);
            }

            long id = OurUtility.ToInt64(Request[".id"]);

            // -- Actual code
            JsonResult result = Json(string.Empty, JsonRequestBehavior.AllowGet);

            using (MERCY_Ctx db = new MERCY_Ctx())
            {
                var dataQuery =
                        (
                            from dt in db.TEMPORARY_CRUSHING_PLANT
                            join head in db.TEMPORARY_CRUSHING_PLANT_Header on dt.Header equals head.RecordId
                            orderby dt.RecordId
                            where head.File_Physical == id
                                //&& head.Sheet == sheet
                            select new Model_View_TEMPORARY_CRUSHING_PLANT
                            {
                                data_RecordId = dt.RecordId
                                , Date_Production = dt.Date_Production
                                , Shift_Work = dt.Shift_Work
                                , Tonnage = dt.Tonnage
                                , Sample_ID = dt.Sample_ID
                                , TM = dt.TM
                                , M = dt.M
                                , ASH_adb =  dt.ASH_adb
                                , ASH_arb = dt.ASH_arb
                                , VM_adb = dt.VM_adb
                                , VM_arb = dt.VM_arb
                                , FC_adb = dt.FC_adb
                                , FC_arb = dt.FC_arb
                                , TS_adb = dt.TS_adb
                                , TS_arb = dt.TS_arb
                                , CV_adb = dt.CV_adb
                                , CV_db = dt.CV_db
                                , CV_arb = dt.CV_arb
                                , CV_daf = dt.CV_daf
                                , CV_ad_15 = dt.CV_ad_15
                                , CV_ad_16 = dt.CV_ad_16
                                , CV_ad_17 = dt.CV_ad_17
                                , Remark = dt.Remark
                                , Status = dt.Status
                                , Sheet = head.Sheet

                                , TM_Plan = dt.TM_Plan
                                , ASH_Plan = dt.ASH_Plan
                                , TS_Plan = dt.TS_Plan
                                , CV_Plan = dt.CV_Plan
                            }
                        );

                var data = dataQuery.ToList();

                data.ForEach(c =>
                {
                    c.Tonnage_Str = OurUtility.ToDecimal(c.Tonnage).ToString("0.000");
                    c.TM_Str = OurUtility.ToDecimal(c.TM).ToString("0.00");
                    c.M_Str = OurUtility.ToDecimal(c.M).ToString("0.00");
                    c.ASH_adb_Str = OurUtility.ToDecimal(c.ASH_adb).ToString("0.00");
                    c.ASH_arb_Str = c.ASH_arb.ToString("0.00");
                    c.VM_adb_Str = OurUtility.ToDecimal(c.VM_adb).ToString("0.00");
                    c.VM_arb_Str = c.VM_arb.ToString("0.00");
                    c.FC_adb_Str = c.FC_adb.ToString("0.00");
                    c.FC_arb_Str = c.FC_arb.ToString("0.00");
                    c.TS_adb_Str = OurUtility.ToDecimal(c.TS_adb).ToString("0.00");
                    c.TS_arb_Str = c.TS_arb.ToString("0.00");
                    c.CV_adb_Str = String.Format("{0:n0}", OurUtility.ToDecimal(c.CV_adb, 0));
                    c.CV_db_Str = String.Format("{0:n0}", Decimal.Round(c.CV_db, 0));
                    c.CV_arb_Str = String.Format("{0:n0}", Decimal.Round(c.CV_arb, 0));
                    c.CV_daf_Str = String.Format("{0:n0}", Decimal.Round(c.CV_daf, 0));
                    c.CV_ad_15_Str = String.Format("{0:n0}", Decimal.Round(c.CV_ad_15, 0));
                    c.CV_ad_16_Str = String.Format("{0:n0}", Decimal.Round(c.CV_ad_16, 0));
                    c.CV_ad_17_Str = String.Format("{0:n0}", Decimal.Round(c.CV_ad_17, 0));

                    c.TM_Plan_Str = OurUtility.ToDecimal(c.TM_Plan).ToString("0.00");
                    c.ASH_Plan_Str = OurUtility.ToDecimal(c.ASH_Plan).ToString("0.00");
                    c.TS_Plan_Str = OurUtility.ToDecimal(c.TS_Plan).ToString("0.00");
                    c.CV_Plan_Str = c.CV_Plan;
                });

                var draw = OurUtility.ValueOf(Request, "draw");
                var recordsTotal = dataQuery.Count();

                return Json(new { draw, recordsFiltered = recordsTotal, recordsTotal, data }, JsonRequestBehavior.AllowGet);
            }
        }

        public JsonResult CRUSHING_PLANT_Header()
        {
            // -- Permission validation
            // get Current Url
            string url_API = Request.Url.AbsolutePath.ToLower();

            // Check Permission based on Current Url
            //Permission.Check(url_API, ref is_view, ref is_add, ref is_delete, ref is_edit, ref is_active);

            // -- Calculation for isview
            is_view = (is_view || is_add || is_delete || is_edit || is_active);

            if (!is_view)
            {
                var msg = new { Data = string.Empty, Total = 0, AggregateResults = string.Empty, Errors = "" };
                return Json(msg, JsonRequestBehavior.AllowGet);
            }

            long id = OurUtility.ToInt64(Request[".id"]);

            // -- Actual code
            JsonResult result = Json(string.Empty, JsonRequestBehavior.AllowGet);

            using (MERCY_Ctx db = new MERCY_Ctx())
            {
                var dataQuery =
                        (
                            from dt in db.TEMPORARY_CRUSHING_PLANT_Header
                            join f in db.TEMPORARY_File on dt.File_Physical equals f.RecordId
                            where dt.File_Physical == id
                            select new
                            {
                                dt.Date_Detail
                                , dt.Job_No
                                , dt.Report_To
                                , dt.Method1
                                , dt.Method2
                                , dt.Method3
                                , dt.Method4
                                , f.FileName
                                , f.Link
                            }
                        );

                result = Json(dataQuery.ToList(), JsonRequestBehavior.AllowGet);
            }

            return result;
        }

        public JsonResult Sampling_ROM()
        {
            // -- Permission validation
            // get Current Url
            string url_API = Request.Url.AbsolutePath.ToLower();

            // Check Permission based on Current Url
            //Permission.Check(url_API, ref is_view, ref is_add, ref is_delete, ref is_edit, ref is_active);

            // -- Calculation for isview
            is_view = (is_view || is_add || is_delete || is_edit || is_active);

            if (!is_view)
            {
                var msg = new { Data = string.Empty, Total = 0, AggregateResults = string.Empty, Errors = "" };
                return Json(msg, JsonRequestBehavior.AllowGet);
            }

            long id = OurUtility.ToInt64(Request[".id"]);

            // -- Actual code
            JsonResult result = Json(string.Empty, JsonRequestBehavior.AllowGet);

            using (MERCY_Ctx db = new MERCY_Ctx())
            {
                var dataQuery =
                        (
                            from dt in db.TEMPORARY_Sampling_ROM
                            join head in db.TEMPORARY_Sampling_ROM_Header on dt.Header equals head.RecordId
                            orderby dt.RecordId
                            where head.File_Physical == id
                            select new Model_View_TEMPORARY_Sampling_ROM
                            {
                                data_RecordId = dt.RecordId
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
                                , Status = dt.Status
                            }
                        );

                var data = dataQuery.ToList();

                data.ForEach(c =>
                {
                    c.TM_Str = OurUtility.ToDecimal(c.TM).ToString("0.00");
                    c.M_Str = OurUtility.ToDecimal(c.M).ToString("0.00");
                    c.ASH_Str = OurUtility.ToDecimal(c.ASH).ToString("0.00");
                    c.TS_Str = OurUtility.ToDecimal(c.TS).ToString("0.00");
                    c.CV_Str = String.Format("{0:n0}", OurUtility.ToInt32(c.CV));
                });

                var draw = OurUtility.ValueOf(Request, "draw");
                var recordsTotal = dataQuery.Count();

                return Json(new { draw, recordsFiltered = recordsTotal, recordsTotal, data }, JsonRequestBehavior.AllowGet);
            }
        }

        public JsonResult Sampling_ROM_Header()
        {
            // -- Permission validation
            // get Current Url
            string url_API = Request.Url.AbsolutePath.ToLower();

            // Check Permission based on Current Url
            //Permission.Check(url_API, ref is_view, ref is_add, ref is_delete, ref is_edit, ref is_active);

            // -- Calculation for isview
            is_view = (is_view || is_add || is_delete || is_edit || is_active);

            if (!is_view)
            {
                var msg = new { Data = string.Empty, Total = 0, AggregateResults = string.Empty, Errors = "" };
                return Json(msg, JsonRequestBehavior.AllowGet);
            }

            long id = OurUtility.ToInt64(Request[".id"]);

            // -- Actual code
            JsonResult result = Json(string.Empty, JsonRequestBehavior.AllowGet);
            
            using (MERCY_Ctx db = new MERCY_Ctx())
            {
                var dataQuery =
                        (
                            from dt in db.TEMPORARY_Sampling_ROM_Header
                            join f in db.TEMPORARY_File on dt.File_Physical equals f.RecordId
                            where dt.File_Physical == id
                            select new
                            {
                                dt.Date_Detail
                                , dt.Job_No
                                , dt.Report_To
                                , dt.Method1
                                , dt.Method2
                                , dt.Method3
                                , dt.Method4
                                , f.FileName
                                , f.Link
                            }
                        );

                result = Json(dataQuery.ToList(), JsonRequestBehavior.AllowGet);
            }

            return result;
        }
        
        public JsonResult Geology_Pit_Monitoring()
        {
            // -- Permission validation
            // get Current Url
            string url_API = Request.Url.AbsolutePath.ToLower();

            // Check Permission based on Current Url
            //Permission.Check(url_API, ref is_view, ref is_add, ref is_delete, ref is_edit, ref is_active);

            // -- Calculation for isview
            is_view = (is_view || is_add || is_delete || is_edit || is_active);

            if (!is_view)
            {
                var msg = new { Data = string.Empty, Total = 0, AggregateResults = string.Empty, Errors = "" };
                return Json(msg, JsonRequestBehavior.AllowGet);
            }

            long id = OurUtility.ToInt64(Request[".id"]);

            // -- Actual code
            JsonResult result = Json(string.Empty, JsonRequestBehavior.AllowGet);

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
                                data_RecordId = dt.RecordId
                                , Sample_ID = dt.Sample_ID
                                , SampleType = dt.SampleType
                                , Lab_ID = dt.Lab_ID
                                , Mass_Spl = dt.Mass_Spl
                                , TM = dt.TM
                                , M = dt.M
                                , VM = dt.VM
                                , Ash = dt.Ash
                                , TS = dt.TS
                                , Cal_ad = dt.Cal_ad
                                , Cal_db = dt.Cal_db
                                , Cal_ar = dt.Cal_ar
                                , Cal_daf = dt.Cal_daf
                                , RD = dt.RD
                                , Status = dt.Status
                            }
                        );

                var data = dataQuery.ToList();

                data.ForEach(c =>
                {
                    c.Mass_Spl_Str = OurUtility.ToDecimal(c.Mass_Spl).ToString("0.00");
                    c.TM_Str = OurUtility.ToDecimal(c.TM).ToString("0.00");
                    c.M_Str = OurUtility.ToDecimal(c.M).ToString("0.00");
                    c.VM_Str = OurUtility.ToDecimal(c.VM).ToString("0.00");
                    c.Ash_Str = OurUtility.ToDecimal(c.Ash).ToString("0.00");
                    c.FC_Str = c.FC.ToString("0.00");
                    c.TS_Str = OurUtility.ToDecimal(c.TS).ToString("0.00");
                    c.Cal_ad_Str = String.Format("{0:n0}", Decimal.Round(OurUtility.ToDecimal(c.Cal_ad), 0));
                    c.Cal_db_Str = String.Format("{0:n0}", Decimal.Round(c.Cal_db, 0));
                    c.Cal_ar_Str = String.Format("{0:n0}", Decimal.Round(c.Cal_ar, 0));
                    c.Cal_daf_Str = String.Format("{0:n0}", Decimal.Round(c.Cal_daf, 0));
                    c.RD_Str = OurUtility.ToDecimal(c.RD).ToString("0.00");
                });

                var draw = OurUtility.ValueOf(Request, "draw");
                var recordsTotal = dataQuery.Count();

                return Json(new { draw, recordsFiltered = recordsTotal, recordsTotal, data }, JsonRequestBehavior.AllowGet);
            }
        }

        public JsonResult Geology_Pit_Monitoring_Header()
        {
            // -- Permission validation
            // get Current Url
            string url_API = Request.Url.AbsolutePath.ToLower();

            // Check Permission based on Current Url
            //Permission.Check(url_API, ref is_view, ref is_add, ref is_delete, ref is_edit, ref is_active);

            // -- Calculation for isview
            is_view = (is_view || is_add || is_delete || is_edit || is_active);

            if (!is_view)
            {
                var msg = new { Data = string.Empty, Total = 0, AggregateResults = string.Empty, Errors = "" };
                return Json(msg, JsonRequestBehavior.AllowGet);
            }

            long id = OurUtility.ToInt64(Request[".id"]);

            // -- Actual code
            JsonResult result = Json(string.Empty, JsonRequestBehavior.AllowGet);

            using (MERCY_Ctx db = new MERCY_Ctx())
            {
                var dataQuery =
                        (
                            from dt in db.TEMPORARY_Geology_Pit_Monitoring_Header
                            join f in db.TEMPORARY_File on dt.File_Physical equals f.RecordId
                            where dt.File_Physical == id
                            select new
                            {
                                dt.Date_Detail
                                , dt.Job_No
                                , dt.Report_To
                                , dt.Date_Received
                                , dt.Nomor
                                , f.FileName
                                , f.Link
                            }
                        );

                result = Json(dataQuery.ToList(), JsonRequestBehavior.AllowGet);
            }

            return result;
        }

        public JsonResult Geology_Explorasi()
        {
            // -- Permission validation
            // get Current Url
            string url_API = Request.Url.AbsolutePath.ToLower();

            // Check Permission based on Current Url
            //Permission.Check(url_API, ref is_view, ref is_add, ref is_delete, ref is_edit, ref is_active);

            // -- Calculation for isview
            is_view = (is_view || is_add || is_delete || is_edit || is_active);

            if (!is_view)
            {
                var msg = new { Data = string.Empty, Total = 0, AggregateResults = string.Empty, Errors = "" };
                return Json(msg, JsonRequestBehavior.AllowGet);
            }

            long id = OurUtility.ToInt64(Request[".id"]);

            // -- Actual code
            JsonResult result = Json(string.Empty, JsonRequestBehavior.AllowGet);

            using (MERCY_Ctx db = new MERCY_Ctx())
            {
                var dataQuery =
                        (
                            from dt in db.TEMPORARY_Geology_Explorasi
                            join head in db.TEMPORARY_Geology_Explorasi_Header on dt.Header equals head.RecordId
                            orderby dt.RecordId
                            where head.File_Physical == id
                            select new Model_View_TEMPORARY_TEMPORARY_Geology_Explorasi
                            {
                                data_RecordId = dt.RecordId
                                , Sample_ID = dt.Sample_ID
                                , SampleType = dt.SampleType
                                , Lab_ID = dt.Lab_ID
                                , Mass_Spl = dt.Mass_Spl
                                , TM = dt.TM
                                , M = dt.M
                                , VM = dt.VM
                                , Ash = dt.Ash
                                , TS = dt.TS
                                , Cal_ad = dt.Cal_ad
                                , Cal_db = dt.Cal_db
                                , Cal_ar = dt.Cal_ar
                                , Cal_daf = dt.Cal_daf
                                , RD = dt.RD
                                , Status = dt.Status
                            }
                        );

                var data = dataQuery.ToList();

                data.ForEach(c =>
                {
                    c.Mass_Spl_Str = OurUtility.ToDecimal(c.Mass_Spl).ToString("0.00");
                    c.TM_Str = OurUtility.ToDecimal(c.TM).ToString("0.00");
                    c.M_Str = OurUtility.ToDecimal(c.M).ToString("0.00");
                    c.VM_Str = OurUtility.ToDecimal(c.VM).ToString("0.00");
                    c.Ash_Str = OurUtility.ToDecimal(c.Ash).ToString("0.00");
                    c.FC_Str = c.FC.ToString("0.00");
                    c.TS_Str = OurUtility.ToDecimal(c.TS).ToString("0.00");
                    c.Cal_ad_Str = String.Format("{0:n0}", Decimal.Round(OurUtility.ToDecimal(c.Cal_ad), 0));
                    c.Cal_db_Str = String.Format("{0:n0}", Decimal.Round(c.Cal_db, 0));
                    c.Cal_ar_Str = String.Format("{0:n0}", Decimal.Round(c.Cal_ar, 0));
                    c.Cal_daf_Str = String.Format("{0:n0}", Decimal.Round(c.Cal_daf, 0));
                    c.RD_Str = OurUtility.ToDecimal(c.RD).ToString("0.00");
                });

                var draw = OurUtility.ValueOf(Request, "draw");
                var recordsTotal = dataQuery.Count();

                return Json(new { draw, recordsFiltered = recordsTotal, recordsTotal, data }, JsonRequestBehavior.AllowGet);
            }
        }

        public JsonResult Geology_Explorasi_Header()
        {
            // -- Permission validation
            // get Current Url
            string url_API = Request.Url.AbsolutePath.ToLower();

            // Check Permission based on Current Url
            //Permission.Check(url_API, ref is_view, ref is_add, ref is_delete, ref is_edit, ref is_active);

            // -- Calculation for isview
            is_view = (is_view || is_add || is_delete || is_edit || is_active);

            if (!is_view)
            {
                var msg = new { Data = string.Empty, Total = 0, AggregateResults = string.Empty, Errors = "" };
                return Json(msg, JsonRequestBehavior.AllowGet);
            }

            long id = OurUtility.ToInt64(Request[".id"]);

            // -- Actual code
            JsonResult result = Json(string.Empty, JsonRequestBehavior.AllowGet);

            using (MERCY_Ctx db = new MERCY_Ctx())
            {
                var dataQuery =
                        (
                            from dt in db.TEMPORARY_Geology_Explorasi_Header
                            join f in db.TEMPORARY_File on dt.File_Physical equals f.RecordId
                            where dt.File_Physical == id
                            select new
                            {
                                dt.Date_Detail
                                , dt.Job_No
                                , dt.Report_To
                                , dt.Date_Received
                                , dt.Nomor
                                , f.FileName
                                , f.Link
                            }
                        );

                result = Json(dataQuery.ToList(), JsonRequestBehavior.AllowGet);
            }

            return result;
        }

        public JsonResult ProcessFile()
        {
            JsonResult result = Json(new { Status = "unknown", Message = "", Count = 0 }, JsonRequestBehavior.AllowGet);

            try
            {
                using (MERCY_Ctx db = new MERCY_Ctx())
                {
                    using (var connection = new SqlConnection(db.Database.Connection.ConnectionString))
                    {
                        connection.Open();

                        SqlCommand command = connection.CreateCommand();

                        string id = Request[".id"];
                        string fileType = Request[".ty"];
                        string spName = "UPLOAD_ACTUAL_Sampling_ROM";

                        switch (fileType)
                        {
                            case OurUtility.UPLOAD_Sampling_ROM:
                                spName = "UPLOAD_ACTUAL_Sampling_ROM";
                                break;
                            case OurUtility.UPLOAD_Geology_Pit_Monitoring:
                                spName = "UPLOAD_ACTUAL_GEOLOGY_PIT";
                                break;
                            case OurUtility.UPLOAD_Geology_Explorasi:
                                spName = "UPLOAD_ACTUAL_GEOLOGY_EXPLORASI";
                                break;
                            case OurUtility.UPLOAD_BARGE_LOADING:
                                spName = "UPLOAD_ACTUAL_BARGE_LOADING";
                                break;
                            case OurUtility.UPLOAD_CRUSHING_PLANT:
                                spName = "UPLOAD_ACTUAL_CRUSHING_PLANT";
                                break;
                        }

                        command.CommandText = string.Format(@"exec {0} {1}", spName, id);

                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                result = Json(new { Status = reader["cStatus"].ToString(), Message = reader["cMessage"].ToString(), Count = reader["cCount"].ToString(), Count2 = reader["cCount2"].ToString() }, JsonRequestBehavior.AllowGet);
                            }
                        }
                    }
                }
            }
            catch {}

            return result;
        }
    }

    class Model_View_TEMPORARY_Sampling_ROM : TEMPORARY_Sampling_ROM
    {
        public long data_RecordId { get; set; }
        public string TM_Str { get; set; }
        public string M_Str { get; set; }
        public string ASH_Str { get; set; }
        public string TS_Str { get; set; }
        public string CV_Str { get; set; }
    }

    class Model_View_TEMPORARY_Geology_Pit_Monitoring : TEMPORARY_Geology_Pit_Monitoring
    {
        public long data_RecordId { get; set; }
        public string Mass_Spl_Str { get; set; }
        public string TM_Str { get; set; }
        public string M_Str { get; set; }
        public string VM_Str { get; set; }
        public string Ash_Str { get; set; }
        public string FC_Str { get; set; }
        public string TS_Str { get; set; }
        public string Cal_ad_Str { get; set; }
        public string Cal_db_Str { get; set; }
        public string Cal_ar_Str { get; set; }
        public string Cal_daf_Str { get; set; }
        public string RD_Str { get; set; }
    }

    class Model_View_TEMPORARY_TEMPORARY_Geology_Explorasi : TEMPORARY_Geology_Explorasi
    {
        public long data_RecordId { get; set; }
        public string Mass_Spl_Str { get; set; }
        public string TM_Str { get; set; }
        public string M_Str { get; set; }
        public string VM_Str { get; set; }
        public string Ash_Str { get; set; }
        public string FC_Str { get; set; }
        public string TS_Str { get; set; }
        public string Cal_ad_Str { get; set; }
        public string Cal_db_Str { get; set; }
        public string Cal_ar_Str { get; set; }
        public string Cal_daf_Str { get; set; }
        public string RD_Str { get; set; }
    }

    class Model_View_TEMPORARY_TEMPORARY_BARGE_LOADING : TEMPORARY_BARGE_LOADING
    {
        public long data_RecordId { get; set; }
        public string Tonnage_Str { get; set; }
        public string Temperature_Str { get; set; }
        public string TM_Str { get; set; }
        public string M_Str { get; set; }
        public string ASH_adb_Str { get; set; }
        public string ASH_arb_Str { get; set; }
        public string VM_adb_Str { get; set; }
        public string VM_arb_Str { get; set; }
        public string FC_adb_Str { get; set; }
        public string FC_arb_Str { get; set; }
        public string TS_adb_Str { get; set; }
        public string TS_arb_Str { get; set; }
        public string CV_adb_Str { get; set; }
        public string CV_db_Str { get; set; }
        public string CV_arb_Str { get; set; }
        public string CV_daf_Str { get; set; }
        public string CV_ad_15_Str { get; set; }
        public string CV_ad_16_Str { get; set; }
        public string CV_ad_17_Str { get; set; }

        public string TM_Plan_Str { get; set; }
        public string ASH_Plan_Str { get; set; }
        public string TS_Plan_Str { get; set; }
        public string CV_Plan_Str { get; set; }
        public string TM_Average_Str { get; set; }
        public string ASH_Average_Str { get; set; }
        public string TS_Average_Str { get; set; }
        public string CV_Average_Str { get; set; }
    }

    class Model_View_TEMPORARY_CRUSHING_PLANT : TEMPORARY_CRUSHING_PLANT
    {
        public long data_RecordId { get; set; }
        public string Tonnage_Str { get; set; }
        public string TM_Str { get; set; }
        public string M_Str { get; set; }
        public string ASH_adb_Str { get; set; }
        public string ASH_arb_Str { get; set; }
        public string VM_adb_Str { get; set; }
        public string VM_arb_Str { get; set; }
        public string FC_adb_Str { get; set; }
        public string FC_arb_Str { get; set; }
        public string TS_adb_Str { get; set; }
        public string TS_arb_Str { get; set; }
        public string CV_adb_Str { get; set; }
        public string CV_db_Str { get; set; }
        public string CV_arb_Str { get; set; }
        public string CV_daf_Str { get; set; }
        public string CV_ad_15_Str { get; set; }
        public string CV_ad_16_Str { get; set; }
        public string CV_ad_17_Str { get; set; }
        public string TM_Plan_Str { get; set; }
        public string ASH_Plan_Str { get; set; }
        public string TS_Plan_Str { get; set; }
        public string CV_Plan_Str { get; set; }
        public string TM_Average_Str { get; set; }
        public string ASH_Average_Str { get; set; }
        public string TS_Average_Str { get; set; }
        public string CV_Average_Str { get; set; }
    }

    class Model_View_TEMPORARY_SampleDetail : TEMPORARY_SampleDetail
    {
        public decimal Thick { get; set; }
    }
}
