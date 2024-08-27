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
    public class BargeLineUpController : Controller
    {
        public JsonResult Index()
        {
            return Json("BargeLineUpController", JsonRequestBehavior.AllowGet);
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

        public JsonResult ChooseFile()
        {
            // Check Permission based on Current Url
            UserX user = new UserX(Request);
            Permission_Item permission_Item = null;

            Permission.Check_API(Request, user, ref permission_Item);
            if (!permission_Item.Is_Add)
            {
                var msgX = new { Success = false, Permission = permission_Item, Message = Permission.ERROR_PERMISSION_ADD, MessageDetail = string.Empty, Version = Configuration.VERSION };
                return Json(msgX, JsonRequestBehavior.AllowGet);
            }

            string fileName = string.Empty;
            string fileName2 = string.Empty;
            string msg = string.Empty;
            long id_File_Physical = 0;

            if (!OurUtility.Upload(Request, UploadFolder, ref fileName, ref fileName2, ref msg))
            {
                var msgX = new { Success = false, Permission = permission_Item, Message = msg, MessageDetail = string.Empty, Version = Configuration.VERSION, Id = -1 };
                return Json(msgX, JsonRequestBehavior.AllowGet);

            }

            if (!OurUtility.Upload_Record(user, fileName, fileName2, OurUtility.UPLOAD_BargeLineUp, ref id_File_Physical, ref msg))
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
            if (!permission_Item.Is_Add)
            {
                var msgX = new { Success = false, Permission = permission_Item, Message = Permission.ERROR_PERMISSION_ADD, MessageDetail = string.Empty, Version = Configuration.VERSION };
                return Json(msgX, JsonRequestBehavior.AllowGet);
            }

            long id_File_Physical = OurUtility.ToInt64(Request[".id"]);
            int site = OurUtility.ToInt32(Request["site"]);
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

                    var refMapping = db.BargeLineUpMappings.Where(x => x.Sheet == "BLU").ToList();
                    // Open the spreadsheet document for read-only access.
                    using (SpreadsheetDocument document = SpreadsheetDocument.Open(UploadFolder + fileName, false))
                    {
                        // Retrieve a reference to the workbook part.
                        WorkbookPart wbPart = document.WorkbookPart;

                        WorksheetPart wsPart = null;

                        int baris = 1;
                        bool found_baris = false;
                        bool is_Hidden = false;
                        string loading_start = string.Empty;

                        string status = string.Empty;
                        List<Unique_BLU> keys_Unique_BLU = new List<Unique_BLU>();

                        TEMPORARY_Barge_Line_Up detail = null;

                        foreach (Sheet sheet in wbPart.Workbook.Sheets)
                        {
                            is_Hidden = (sheet.State != null
                                            && sheet.State.HasValue
                                            && (sheet.State.Value == SheetStateValues.Hidden
                                                    || sheet.State.Value == SheetStateValues.VeryHidden));

                            // Skip worksheet yang "Hidden"
                            if (is_Hidden) continue;

                            // somehow, we can change "Validation of SheetName"
                            is_Sheet_Valid = false;
                            sheetName = sheet.Name.ToString().Trim();

                            if (sheetName.ToUpper().StartsWith("BLU"))
                            {
                                // this is BLU Section
                                is_Sheet_Valid = true;
                            }

                            if (sheetName.ToUpper().StartsWith("VLU"))
                            {
                                // this is VLU: Vessel Line Up
                                is_Sheet_Valid = true;

                                string vluResult = Process_VLU(wbPart, sheet, site, id_File_Physical, db, user);
                                if (vluResult != string.Empty)
                                {
                                    var msgX = new { Success = false, Permission = permission_Item, Message = vluResult, MessageDetail = string.Empty, Version = Configuration.VERSION };
                                    return Json(msgX, JsonRequestBehavior.AllowGet);
                                }

                                continue;
                            }

                            if (!is_Sheet_Valid) continue;

                            try
                            {
                                wsPart = (WorksheetPart)(wbPart.GetPartById(sheet.Id));

                                baris = 1;
                                found_baris = false;

                                // cari "POL (PORT OF LOADING)"
                                while (baris <= 1000)
                                {
                                    if (ExcelProcessing.GetCellValue(wbPart, wsPart, "A" + baris.ToString()).ToUpper() == "POL (PORT OF LOADING)")
                                    {
                                        found_baris = true;
                                        break;
                                    }

                                    baris++;
                                }

                                if (!found_baris) continue;

                                string value_column_A = string.Empty;
                                string value_column_C = string.Empty;
                                string value_column_D = string.Empty;

                                string port_of_loading = "";
                                string term = string.Empty;
                                string vessel_name = string.Empty;
                                string trip_id = string.Empty;
                                string tug_boat = string.Empty;
                                string barge = string.Empty;
                                string product = string.Empty;
                                string capacity = string.Empty;
                                string file_destination = string.Empty;
                                string remark = string.Empty;
                                string loading_start_mapping = string.Empty;

                                try
                                {
                                    port_of_loading = refMapping.Where(x => x.DataKey == "PORT_OF_LOADING").FirstOrDefault().TemplateColumn;
                                    term = refMapping.Where(x => x.DataKey == "TERM").FirstOrDefault().TemplateColumn;
                                    vessel_name = refMapping.Where(x => x.DataKey == "VESSEL_NAME").FirstOrDefault().TemplateColumn;
                                    trip_id = refMapping.Where(x => x.DataKey == "TRIP_ID").FirstOrDefault().TemplateColumn;
                                    tug_boat = refMapping.Where(x => x.DataKey == "TUG_BOAT").FirstOrDefault().TemplateColumn;
                                    barge = refMapping.Where(x => x.DataKey == "BARGE").FirstOrDefault().TemplateColumn;
                                    product = refMapping.Where(x => x.DataKey == "PRODUCT").FirstOrDefault().TemplateColumn;
                                    capacity = refMapping.Where(x => x.DataKey == "CAPACITY").FirstOrDefault().TemplateColumn;
                                    file_destination = refMapping.Where(x => x.DataKey == "FILE_DESTINATION").FirstOrDefault().TemplateColumn;
                                    remark = refMapping.Where(x => x.DataKey == "REMARK").FirstOrDefault().TemplateColumn;
                                    loading_start_mapping = refMapping.Where(x => x.DataKey == "LOADING_START").FirstOrDefault().TemplateColumn;
                                }
                                catch
                                {
                                    var msgX = new { Success = false, Permission = permission_Item, Message = "Mapping profile not found", MessageDetail = string.Empty, Version = Configuration.VERSION };
                                    return Json(msgX, JsonRequestBehavior.AllowGet);
                                }


                                while (baris <= 1000)
                                {
                                    baris++;

                                    value_column_A = ExcelProcessing.GetCellValue(wbPart, wsPart, "A" + baris.ToString()).ToUpper();
                                    value_column_C = ExcelProcessing.GetCellValue(wbPart, wsPart, port_of_loading + baris.ToString()).ToUpper();
                                    value_column_D = ExcelProcessing.GetCellValue(wbPart, wsPart, term + baris.ToString()).ToUpper();

                                    if (value_column_C.ToUpper() != "BUNYUT")
                                    {
                                        // skip: bukan BUNYUT
                                        continue;
                                    }

                                    if (value_column_A.Contains("PORT"))
                                    {
                                        // skip: tulisan [BUNYUT PORT, EMB PORT, KMIA PORT]
                                        continue;
                                    }
                                    if (value_column_A == "NIL")
                                    {
                                        // skip: tulisan [ NIL ]
                                        continue;
                                    }
                                    if (value_column_A == "DRY DOCK / BREAKDOWN")
                                    {
                                        // skip: tulisan [ DRY DOCK / BREAKDOWN ]
                                        continue;
                                    }
                                    if (value_column_A == "PLAN NEXT TRIP")
                                    {
                                        // skip: tulisan [ PLAN NEXT TRIP ]
                                        continue;
                                    }

                                    if (string.IsNullOrEmpty(loading_start))
                                    {
                                        string temp = ExcelProcessing.GetCellValue(wbPart, wsPart, loading_start_mapping + baris.ToString());
                                        loading_start = ExcelProcessing.GetDate(temp, "yyyy-MM-dd", "");
                                    }

                                    detail = new TEMPORARY_Barge_Line_Up
                                    {
                                        File_Physical = id_File_Physical,
                                        Site = site,
                                        Sheet = sheetName,
                                        EstimateStartLoading = loading_start,
                                        Port_of_Loading = ExcelProcessing.GetCellValue(wbPart, wsPart, port_of_loading + baris.ToString()),
                                        VesselName = ExcelProcessing.GetCellValue(wbPart, wsPart, vessel_name + baris.ToString()),
                                        TripID = ExcelProcessing.GetCellValue(wbPart, wsPart, trip_id + baris.ToString()),
                                        TugBoat = ExcelProcessing.GetCellValue(wbPart, wsPart, tug_boat + baris.ToString()),
                                        Barge = ExcelProcessing.GetCellValue(wbPart, wsPart, barge + baris.ToString()),
                                        Product = ExcelProcessing.GetCellValue(wbPart, wsPart, product + baris.ToString()),
                                        Capacity = ExcelProcessing.GetCellValue(wbPart, wsPart, capacity + baris.ToString()),
                                        FinalDestinantion = ExcelProcessing.GetCellValue(wbPart, wsPart, file_destination + baris.ToString()),
                                        Remark = ExcelProcessing.GetCellValue(wbPart, wsPart, remark + baris.ToString())
                                    };

                                    if (
                                        // string.IsNullOrEmpty(detail.EstimateStartLoading)
                                        string.IsNullOrEmpty(detail.Port_of_Loading)
                                        && string.IsNullOrEmpty(detail.VesselName)
                                        && string.IsNullOrEmpty(detail.TripID)
                                        && string.IsNullOrEmpty(detail.TugBoat)
                                        && string.IsNullOrEmpty(detail.Barge)
                                        && string.IsNullOrEmpty(detail.Product)
                                        && string.IsNullOrEmpty(detail.Capacity)
                                        && string.IsNullOrEmpty(detail.FinalDestinantion)
                                        && string.IsNullOrEmpty(value_column_D)
                                        )
                                    {
                                        break;
                                    }

                                    detail.CreatedOn = DateTime.Now;
                                    detail.CreatedBy = user.UserId;
                                    detail.CreatedOn_Date_Only = detail.CreatedOn.ToString("yyyy-MM-dd");

                                    Check_Validation_BLU(detail, keys_Unique_BLU);

                                    // init Status, start with "InValid"
                                    status = "Invalid";

                                    if (detail.EstimateStartLoading_isvalid
                                            && detail.Port_of_Loading_isvalid
                                            && detail.VesselName_isvalid
                                            && detail.TripID_isvalid
                                            && detail.TugBoat_isvalid
                                            && detail.Barge_isvalid
                                            && detail.Product_isvalid
                                            && detail.Capacity_isvalid
                                            && detail.FinalDestinantion_isvalid
                                            && detail.Remark_isvalid
                                    )
                                    {
                                        status = "New";
                                    }

                                    detail.Status = status;

                                    db.TEMPORARY_Barge_Line_Up.Add(detail);
                                    db.SaveChanges();

                                    // for Next "Unique"
                                    try
                                    {
                                        Unique_BLU a = new Unique_BLU
                                        {
                                            CreatedOn_Date_Only = detail.CreatedOn_Date_Only,
                                            TripID = detail.TripID
                                        };
                                        keys_Unique_BLU.Add(a);
                                    }
                                    catch { }
                                }

                                // Handling Status: Update
                                try
                                {
                                    bool isNeed_Update = false;

                                    // ambil semua data yang baru di Insert diatas
                                    // yang Status: New
                                    List<TEMPORARY_Barge_Line_Up> recordSet = (
                                                                                from dt in db.TEMPORARY_Barge_Line_Up
                                                                                orderby dt.RecordId
                                                                                where dt.File_Physical == id_File_Physical //data yang baru di Insert diatas
                                                                                        && dt.Status == "New"
                                                                                select dt
                                                                             ).ToList();

                                    // proses RecordSet
                                    foreach (TEMPORARY_Barge_Line_Up row in recordSet)
                                    {
                                        // cari data "sebelumnya" di Table "Actual Upload"
                                        // punya Unique Key yang sama
                                        if (db.UPLOAD_Barge_Line_Up.Where(d => d.TripID == row.TripID && d.CreatedOn_Date_Only == row.CreatedOn_Date_Only // punya Unique yang sama   
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
                                catch (Exception ex)
                                {
                                    var msgX = new { Success = false, Permission = permission_Item, ex.Message, MessageDetail = string.Empty, Version = Configuration.VERSION };
                                    return Json(msgX, JsonRequestBehavior.AllowGet);
                                }
                            }
                            catch (Exception ex)
                            {
                                var msgX = new { Success = false, Permission = permission_Item, ex.Message, MessageDetail = string.Empty, Version = Configuration.VERSION };
                                return Json(msgX, JsonRequestBehavior.AllowGet);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                var msgX = new { Success = false, Permission = permission_Item, ex.Message, MessageDetail = string.Empty, Version = Configuration.VERSION };
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
            if (!permission_Item.Is_Add)
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
                    var dataQuery_Port_of_Loading =
                            (
                                from d in db.TEMPORARY_Barge_Line_Up
                                where d.File_Physical == id
                                orderby d.RecordId
                                select new Model_TEMPORARY_Barge_Line_Up
                                {
                                    Status = d.Status
                                    ,
                                    Sheet = d.Sheet
                                    ,
                                    EstimateStartLoading = d.EstimateStartLoading
                                    ,
                                    Port_of_Loading = d.Port_of_Loading
                                    ,
                                    VesselName = d.VesselName
                                    ,
                                    TripID = d.TripID
                                    ,
                                    TugBoat = d.TugBoat
                                    ,
                                    Barge = d.Barge
                                    ,
                                    Product = d.Product
                                    ,
                                    Capacity = d.Capacity
                                    ,
                                    FinalDestinantion = d.FinalDestinantion
                                    ,
                                    Remark = d.Remark

                                    ,
                                    EstimateStartLoading_isvalid = d.EstimateStartLoading_isvalid
                                    ,
                                    Port_of_Loading_isvalid = d.Port_of_Loading_isvalid
                                    ,
                                    VesselName_isvalid = d.VesselName_isvalid
                                    ,
                                    TripID_isvalid = d.TripID_isvalid
                                    ,
                                    TugBoat_isvalid = d.TugBoat_isvalid
                                    ,
                                    Barge_isvalid = d.Barge_isvalid
                                    ,
                                    Product_isvalid = d.Product_isvalid
                                    ,
                                    Capacity_isvalid = d.Capacity_isvalid
                                    ,
                                    FinalDestinantion_isvalid = d.FinalDestinantion_isvalid
                                    ,
                                    Remark_isvalid = d.Remark_isvalid
                                }
                            );

                    var items_Port_of_Loading = dataQuery_Port_of_Loading.ToList();

                    items_Port_of_Loading.ForEach(c =>
                    {
                        c.Capacity_Str = Value_Of(c.Capacity_isvalid, c.Capacity);
                        c.EstimateStartLoading_Str = OurUtility.ToDateTime(c.EstimateStartLoading).ToString("dd-MMM-yyyy");
                    });

                    var dataQuery_Vessel_Line_Up =
                            (
                                from d in db.TEMPORARY_VLU
                                where d.File_Physical == id
                                orderby d.RecordId
                                select new Model_TEMPORARY_VLU
                                {
                                    Status_Upload = d.Status_Upload
                                    ,
                                    Month = d.Month
                                    ,
                                    Shipper = d.Shipper
                                    ,
                                    NO = d.NO
                                    ,
                                    VESSEL = d.VESSEL
                                    ,
                                    LAYCAN_From = d.LAYCAN_From
                                    ,
                                    LAYCAN_To = d.LAYCAN_To
                                    ,
                                    DESTINATION = d.DESTINATION
                                    ,
                                    BUYER = d.BUYER
                                    ,
                                    ETA = d.ETA
                                    ,
                                    ATA = d.ATA
                                    ,
                                    Commenced_Loading = d.Commenced_Loading
                                    ,
                                    Completed_Loading = d.Completed_Loading
                                    ,
                                    PORT = d.PORT
                                    ,
                                    DEM = d.DEM
                                    ,
                                    STATUS = d.STATUS

                                    ,
                                    Month_isvalid = d.Month_isvalid
                                    ,
                                    Shipper_isvalid = d.Shipper_isvalid
                                    ,
                                    NO_isvalid = d.NO_isvalid
                                    ,
                                    VESSEL_isvalid = d.VESSEL_isvalid
                                    ,
                                    LAYCAN_isvalid = d.LAYCAN_isvalid
                                    ,
                                    LAYCAN_2_isvalid = d.LAYCAN_2_isvalid
                                    ,
                                    DESTINATION_isvalid = d.DESTINATION_isvalid
                                    ,
                                    BUYER_isvalid = d.BUYER_isvalid
                                    ,
                                    ETA_isvalid = d.ETA_isvalid
                                    ,
                                    ATA_isvalid = d.ATA_isvalid
                                    ,
                                    Commenced_Loading_isvalid = d.Commenced_Loading_isvalid
                                    ,
                                    Completed_Loading_isvalid = d.Completed_Loading_isvalid
                                    ,
                                    PORT_isvalid = d.PORT_isvalid
                                    ,
                                    DEM_isvalid = d.DEM_isvalid
                                    ,
                                    STATUS_isvalid = d.STATUS_isvalid
                                }
                            );

                    var items_Vessel_Line_Up = dataQuery_Vessel_Line_Up.ToList();

                    items_Vessel_Line_Up.ForEach(c =>
                    {
                        c.LAYCAN_From_Str = OurUtility.ToDateTime(c.LAYCAN_From).ToString("dd-MMM");
                        c.LAYCAN_To_Str = OurUtility.ToDateTime(c.LAYCAN_To).ToString("dd-MMM");
                        c.ETA_Str = c.ETA == string.Empty ? string.Empty : OurUtility.ToDateTime(c.ETA).ToString("dd-MMM-yyyy HH:mm");
                        c.ATA_Str = c.ATA == string.Empty ? string.Empty : OurUtility.ToDateTime(c.ATA).ToString("dd-MMM-yyyy HH:mm");
                        c.Commenced_Loading_Str = c.Commenced_Loading == string.Empty ? string.Empty : OurUtility.ToDateTime(c.Commenced_Loading).ToString("dd-MMM-yyyy HH:mm");
                        c.Completed_Loading_Str = c.Completed_Loading == string.Empty ? string.Empty : OurUtility.ToDateTime(c.Completed_Loading).ToString("dd-MMM-yyyy HH:mm");
                        c.DEM_Str = c.DEM.ToString();
                    });

                    var result = new { Success = true, Permission = permission_Item, Message = string.Empty, MessageDetail = string.Empty, Version = Configuration.VERSION, Port_of_Loading = items_Port_of_Loading, Total = items_Port_of_Loading.Count, Vessel_Line_Up = items_Vessel_Line_Up };
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
            if (!permission_Item.Is_Add)
            {
                var msgX = new { Success = false, Permission = permission_Item, Message = Permission.ERROR_PERMISSION_READ, MessageDetail = string.Empty, Version = Configuration.VERSION };
                return Json(msgX, JsonRequestBehavior.AllowGet);
            }

            long id = OurUtility.ToInt64(OurUtility.ValueOf(Request, ".id"));

            result_line_up result = new result_line_up();

            try
            {
                using (MERCY_Ctx db = new MERCY_Ctx())
                {
                    using (var connection = new SqlConnection(db.Database.Connection.ConnectionString))
                    {
                        connection.Open();

                        SqlCommand command = connection.CreateCommand();

                        command.CommandText = string.Format(@"exec UPLOAD_ACTUAL_Barge_Line_Up {0}", id);

                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                result.port_of_loading = new { Success = OurUtility.ValueOf(reader, "cStatus").ToUpper() == "OK", Count = OurUtility.ValueOf(reader, "cCount"), Count2 = OurUtility.ValueOf(reader, "cCount2"), Permission = permission_Item, Message = OurUtility.ValueOf(reader, "cMessage"), MessageDetail = string.Empty, Version = Configuration.VERSION };
                            }
                        }

                        SqlCommand command2 = connection.CreateCommand();

                        command2.CommandText = string.Format(@"exec UPLOAD_ACTUAL_Barge_LineUp_VLU {0}", id);

                        using (SqlDataReader reader2 = command2.ExecuteReader())
                        {
                            if (reader2.Read())
                            {
                                result.vessel_line_up = new { Success = OurUtility.ValueOf(reader2, "cStatus").ToUpper() == "OK", Count = OurUtility.ValueOf(reader2, "cCount"), Count2 = OurUtility.ValueOf(reader2, "cCount2"), Permission = permission_Item, Message = OurUtility.ValueOf(reader2, "cMessage"), MessageDetail = string.Empty, Version = Configuration.VERSION };
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

        private static void Check_Validation_BLU(TEMPORARY_Barge_Line_Up p_detail, List<Unique_BLU> p_keys_Unique)
        {
            // Key: Valid, jika belum pernah ada
            p_detail.TripID_isvalid = Calculate_Validation_of_Text(p_detail.TripID);

            if (p_detail.TripID_isvalid)
            {
                if (!Calculate_Validation_of_Key_BLU(p_detail.CreatedOn_Date_Only, p_detail.TripID, p_keys_Unique))
                {
                    p_detail.TripID_isvalid = false;
                }
            }

            // Other column
            p_detail.EstimateStartLoading_isvalid = Calculate_Validation_of_Text(p_detail.EstimateStartLoading);
            p_detail.Port_of_Loading_isvalid = Calculate_Validation_of_Text(p_detail.Port_of_Loading);
            p_detail.VesselName_isvalid = Calculate_Validation_of_Text(p_detail.VesselName);
            p_detail.TugBoat_isvalid = Calculate_Validation_of_Text(p_detail.TugBoat);
            p_detail.Barge_isvalid = Calculate_Validation_of_Text(p_detail.Barge);
            p_detail.Product_isvalid = Calculate_Validation_of_Text(p_detail.Product);
            p_detail.Capacity_isvalid = Calculate_Validation_of_Integer(p_detail.Capacity);
            p_detail.FinalDestinantion_isvalid = Calculate_Validation_of_Text(p_detail.FinalDestinantion);

            // Nilai Remark boleh Kosong
            p_detail.Remark_isvalid = true;
        }

        private static bool Calculate_Validation_of_Key_BLU(string p_createdOn_Date_Only, string p_TripID, List<Unique_BLU> p_keys_Unique)
        {
            bool result = false;

            // -- Special Case
            // 1. Check "kosong"
            if (string.IsNullOrEmpty(p_createdOn_Date_Only) || string.IsNullOrEmpty(p_TripID))
            {
                // nilai Kosong dianggap Tidak Valid
                return false;
            }

            // Asumsi: Valid
            result = true;

            // Key: Valid, jika belum pernah ada
            foreach (Unique_BLU item in p_keys_Unique)
            {
                if (item.CreatedOn_Date_Only == p_createdOn_Date_Only
                    && item.TripID == p_TripID)
                {
                    // tidak Valid
                    // karena kombinasi diatas, sudah pernah ada
                    result = false;
                }
            }

            return result;
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

        public static bool Calculate_Validation_of_Integer(string p_value)
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
            if (OurUtility.ToInt32(p_value, -9999) > -9999)
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
            if (!p_isValid)
            {
                // maka Tampilkan Nilai apa adanya
                return p_value;
            }

            // disini pasti Valid
            // maka tampilkan versi "Numeric" yang sudah dilakukan Round()
            return string.Format("{0:N0}", OurUtility.Round(p_value, 2));
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

        private string Process_VLU(WorkbookPart p_workbookPart, Sheet p_worksheet, int p_site, long p_id_File_Physical, MERCY_Ctx p_db, UserX p_user)
        {
            try
            {
                using (MERCY_Ctx db = new MERCY_Ctx())
                {
                    var refMapping = db.BargeLineUpMappings.Where(x => x.Sheet == "VLU").ToList();

                    string month_mapping = string.Empty;
                    string shipper = string.Empty;
                    string vessel = string.Empty;
                    string no = string.Empty;
                    string laycan_from = string.Empty;
                    string laycan_to = string.Empty;
                    string destination = string.Empty;
                    string buyer = string.Empty;
                    string eta = string.Empty;
                    string ata = string.Empty;
                    string commenced_loading = string.Empty;
                    string completed_loading = string.Empty;
                    string port = string.Empty;
                    string dem = string.Empty;
                    string status_mapping = string.Empty;

                    try
                    {
                        month_mapping = refMapping.Where(x => x.DataKey == "MONTH").FirstOrDefault().TemplateColumn;
                        shipper = refMapping.Where(x => x.DataKey == "SHIPPER").FirstOrDefault().TemplateColumn;
                        vessel = refMapping.Where(x => x.DataKey == "VESSEL").FirstOrDefault().TemplateColumn;
                        no = refMapping.Where(x => x.DataKey == "NO").FirstOrDefault().TemplateColumn;
                        laycan_from = refMapping.Where(x => x.DataKey == "LAYCAN_FROM").FirstOrDefault().TemplateColumn;
                        laycan_to = refMapping.Where(x => x.DataKey == "LAYCAN_TO").FirstOrDefault().TemplateColumn;
                        destination = refMapping.Where(x => x.DataKey == "DESTINATION").FirstOrDefault().TemplateColumn;
                        buyer = refMapping.Where(x => x.DataKey == "BUYER").FirstOrDefault().TemplateColumn;
                        eta = refMapping.Where(x => x.DataKey == "ETA").FirstOrDefault().TemplateColumn;
                        ata = refMapping.Where(x => x.DataKey == "ATA").FirstOrDefault().TemplateColumn;
                        commenced_loading = refMapping.Where(x => x.DataKey == "COMMENCED_LOADING").FirstOrDefault().TemplateColumn;
                        completed_loading = refMapping.Where(x => x.DataKey == "COMPLETED_LOADING").FirstOrDefault().TemplateColumn;
                        port = refMapping.Where(x => x.DataKey == "PORT").FirstOrDefault().TemplateColumn;
                        dem = refMapping.Where(x => x.DataKey == "DEM").FirstOrDefault().TemplateColumn;
                        status_mapping = refMapping.Where(x => x.DataKey == "STATUS").FirstOrDefault().TemplateColumn;
                    }
                    catch
                    {
                        return "Mapping profile not found";
                    }

                    WorksheetPart wsPart = (WorksheetPart)(p_workbookPart.GetPartById(p_worksheet.Id));
                    int baris = 6;
                    string month = string.Empty;

                    TEMPORARY_VLU vlu = null;
                    string currentMonth = DateTime.Now.ToString("MMM");
                    DateTime laycan_From;
                    DateTime laycan_To;

                    List<Unique_VLU> keys_Unique = new List<Unique_VLU>();

                    while (true)
                    {
                        month = ExcelProcessing.GetCellValue(p_workbookPart, wsPart, month_mapping + baris.ToString());

                        if (string.IsNullOrEmpty(month))
                        {
                            break;
                        }

                        vlu = new TEMPORARY_VLU
                        {
                            Sheet = p_worksheet.Name.ToString().Trim(),
                            File_Physical = p_id_File_Physical,
                            Site = p_site,
                            Status_Upload = "Ignored",

                            Month = ExcelProcessing.GetCellValue(p_workbookPart, wsPart, month_mapping + baris.ToString()),
                            Shipper = ExcelProcessing.GetCellValue(p_workbookPart, wsPart, shipper + baris.ToString()),
                            NO = ExcelProcessing.GetCellValue(p_workbookPart, wsPart, no + baris.ToString()),
                            VESSEL = ExcelProcessing.GetCellValue(p_workbookPart, wsPart, vessel + baris.ToString()),
                            LAYCAN_From = ExcelProcessing.GetDate(ExcelProcessing.GetCellValue(p_workbookPart, wsPart, laycan_from + baris.ToString()), "yyyy-MM-dd"),
                            LAYCAN_To = ExcelProcessing.GetDate(ExcelProcessing.GetCellValue(p_workbookPart, wsPart, laycan_to + baris.ToString()), "yyyy-MM-dd")
                        };
                        laycan_From = OurUtility.ToDateTime(vlu.LAYCAN_From);
                        laycan_To = OurUtility.ToDateTime(vlu.LAYCAN_To);

                        if (laycan_From.ToString("MMM") == currentMonth || laycan_To.ToString("MMM") == currentMonth)
                        {
                            vlu.Status_Upload = "New";
                        }

                        vlu.DESTINATION = ExcelProcessing.GetCellValue(p_workbookPart, wsPart, destination + baris.ToString());
                        vlu.BUYER = ExcelProcessing.GetCellValue(p_workbookPart, wsPart, buyer + baris.ToString());
                        vlu.ETA = ExcelProcessing.GetDate(ExcelProcessing.GetCellValue(p_workbookPart, wsPart, eta + baris.ToString()), "yyyy-MM-dd HH:mm");
                        vlu.ATA = ExcelProcessing.GetDate(ExcelProcessing.GetCellValue(p_workbookPart, wsPart, ata + baris.ToString()), "yyyy-MM-dd HH:mm");
                        vlu.Commenced_Loading = ExcelProcessing.GetDate(ExcelProcessing.GetCellValue(p_workbookPart, wsPart, commenced_loading + baris.ToString()), "yyyy-MM-dd HH:mm");
                        vlu.Completed_Loading = ExcelProcessing.GetDate(ExcelProcessing.GetCellValue(p_workbookPart, wsPart, completed_loading + baris.ToString()), "yyyy-MM-dd HH:mm");
                        vlu.PORT = ExcelProcessing.GetCellValue(p_workbookPart, wsPart, port + baris.ToString());
                        vlu.DEM = ExcelProcessing.GetCellValue(p_workbookPart, wsPart, dem + baris.ToString());
                        vlu.STATUS = ExcelProcessing.GetCellValue(p_workbookPart, wsPart, status_mapping + baris.ToString());

                        vlu.CreatedOn = DateTime.Now;
                        vlu.CreatedBy = p_user.UserId;
                        vlu.CreatedOn_Date_Only = vlu.CreatedOn.ToString("yyyy-MM-dd");

                        vlu.ETA = vlu.ETA == "1899-12-30 00:00" ? string.Empty : vlu.ETA;
                        vlu.ATA = vlu.ATA == "1899-12-30 00:00" ? string.Empty : vlu.ATA;
                        vlu.Commenced_Loading = vlu.Commenced_Loading == "1899-12-30 00:00" ? string.Empty : vlu.Commenced_Loading;
                        vlu.Completed_Loading = vlu.Completed_Loading == "1899-12-30 00:00" ? string.Empty : vlu.Completed_Loading;

                        if (vlu.Status_Upload == "New")
                        {
                            Check_Validation_VLU(vlu, keys_Unique);

                            // init Status, start with "InValid"
                            string status = "Invalid";

                            if (vlu.Month_isvalid
                                && vlu.Shipper_isvalid
                                && vlu.NO_isvalid
                                && vlu.VESSEL_isvalid
                                && vlu.LAYCAN_isvalid
                                && vlu.LAYCAN_2_isvalid
                                && vlu.DESTINATION_isvalid
                                && vlu.BUYER_isvalid
                                && vlu.ETA_isvalid
                                && vlu.ATA_isvalid
                                && vlu.Commenced_Loading_isvalid
                                && vlu.Completed_Loading_isvalid
                                && vlu.PORT_isvalid
                                && vlu.DEM_isvalid
                                && vlu.STATUS_isvalid
                            )
                            {
                                status = "New";
                            }

                            vlu.Status_Upload = status;
                        }

                        p_db.TEMPORARY_VLU.Add(vlu);
                        p_db.SaveChanges();

                        baris++;

                        // for Next "Unique"
                        try
                        {
                            Unique_VLU a = new Unique_VLU
                            {
                                CreatedOn_Date_Only = vlu.CreatedOn_Date_Only,
                                Vessel = vlu.VESSEL,
                                Laycan_From = vlu.LAYCAN_From,
                                Laycan_To = vlu.LAYCAN_To,
                                Shipper = vlu.Shipper
                            };
                            keys_Unique.Add(a);
                        }
                        catch { }
                    }
                }

                // Handling Status: Update
                try
                {
                    bool isNeed_Update = false;

                    // ambil semua data yang baru di Insert diatas
                    // yang Status: New
                    List<TEMPORARY_VLU> recordSet = (
                                                                from dt in p_db.TEMPORARY_VLU
                                                                orderby dt.RecordId
                                                                where dt.File_Physical == p_id_File_Physical //data yang baru di Insert diatas
                                                                        && dt.Status_Upload == "New"
                                                                select dt
                                                             ).ToList();

                    using (MERCY_Ctx db = new MERCY_Ctx())
                    {
                        using (var connection = new SqlConnection(db.Database.Connection.ConnectionString))
                        {
                            connection.Open();

                            SqlCommand command = connection.CreateCommand();



                            // proses RecordSet:
                            foreach (TEMPORARY_VLU row in recordSet)
                            {
                                // cari data "sebelumnya" di Table "Actual Upload"
                                // punya Unique Key yang sama

                                command.CommandText = string.Format(@"
                                                                    select RecordId from UPLOAD_VLU
                                                                    where CreatedOn_Date_Only = '{0}'
	                                                                    and VESSEL = '{1}'
	                                                                    and Convert(varchar(10), LAYCAN_From, 120) = '{2}'
	                                                                    and Convert(varchar(10), LAYCAN_To, 120) = '{3}'
                                                                    ", row.CreatedOn_Date_Only
                                                                    , row.VESSEL
                                                                    , row.LAYCAN_From
                                                                    , row.LAYCAN_To);
                                using (SqlDataReader reader = command.ExecuteReader())
                                {
                                    if (reader.Read())
                                    {
                                        row.Status_Upload = "Update";

                                        isNeed_Update = true;
                                    }
                                }

                                /*
                                if (p_db.UPLOAD_VLU.Where(d => d.CreatedOn_Date_Only == row.CreatedOn_Date_Only
                                                                && d.VESSEL == row.VESSEL
                                                                && d.LAYCAN_From.Date.ToString("yyyy-MM-dd") == row.LAYCAN_From
                                                                && d.LAYCAN_To.Date.ToString("yyyy-MM-dd") == row.LAYCAN_To
                                                                  // punya Unique yang sama   
                                                                  ).Any())
                                {}
                                */
                            }
                            connection.Close();
                        }
                    }

                    // update if Necessary
                    if (isNeed_Update)
                    {
                        p_db.SaveChanges();
                    }
                }
                catch (Exception ex)
                {
                    return ex.Message;
                }
            }
            catch(Exception ex)
            {
                return ex.Message;
            }
            return "";
        }

        private static void Check_Validation_VLU(TEMPORARY_VLU p_detail, List<Unique_VLU> p_keys_Unique)
        {
            // Key: Valid, jika belum pernah ada
            p_detail.VESSEL_isvalid = Calculate_Validation_of_Text(p_detail.VESSEL);
            p_detail.LAYCAN_isvalid = Calculate_Validation_of_Text(p_detail.LAYCAN_From);
            p_detail.LAYCAN_2_isvalid = Calculate_Validation_of_Text(p_detail.LAYCAN_To);
            p_detail.Shipper_isvalid = Calculate_Validation_of_Text(p_detail.Shipper);

            if (p_detail.VESSEL_isvalid && p_detail.LAYCAN_isvalid && p_detail.LAYCAN_2_isvalid && p_detail.Shipper_isvalid)
            {
                if (!Calculate_Validation_of_Key_VLU(p_detail.CreatedOn_Date_Only, p_detail.VESSEL, p_detail.LAYCAN_From, p_detail.LAYCAN_To, p_detail.Shipper, p_keys_Unique))
                {
                    p_detail.VESSEL_isvalid = false;
                    p_detail.LAYCAN_isvalid = false;
                    p_detail.LAYCAN_2_isvalid = false;
                    p_detail.Shipper_isvalid = false;
                }
            }

            // Other column
            p_detail.Month_isvalid = true;
            p_detail.NO_isvalid = true;
            p_detail.DESTINATION_isvalid = true;
            p_detail.BUYER_isvalid = true;
            p_detail.ETA_isvalid = Calculate_Validation_of_Text(p_detail.ETA);
            p_detail.ATA_isvalid = true;
            p_detail.Commenced_Loading_isvalid = true;
            p_detail.Completed_Loading_isvalid = true;
            p_detail.PORT_isvalid = true;
            p_detail.DEM_isvalid = true;
            p_detail.STATUS_isvalid = true;
        }

        private static bool Calculate_Validation_of_Key_VLU(string p_createdOn_Date_Only, string p_Vessel, string p_Laycan_From, string p_Laycan_To, string p_Shipper, List<Unique_VLU> p_keys_Unique)
        {
            // Asumsi: Valid
            bool result = true;

            // Key: Valid, jika belum pernah ada
            foreach (Unique_VLU item in p_keys_Unique)
            {
                if (item.CreatedOn_Date_Only == p_createdOn_Date_Only
                    && item.Vessel == p_Vessel
                    && item.Laycan_From == p_Laycan_From
                    && item.Laycan_To == p_Laycan_To
                    && item.Shipper == p_Shipper)
                {
                    // tidak Valid
                    // karena kombinasi diatas, sudah pernah ada
                    result = false;
                }
            }

            return result;
        }

        class Unique_BLU
        {
            public string CreatedOn_Date_Only { get; set; }
            public string TripID { get; set; }
        }

        class Unique_VLU
        {
            public string CreatedOn_Date_Only { get; set; }
            public string Vessel { get; set; }
            public string Laycan_From { get; set; }
            public string Laycan_To { get; set; }
            public string Shipper { get; set; }
        }

        public JsonResult GetByVessel()
        {
            return null;
        }

        class result_line_up
        {
            public object port_of_loading { get; set; }
            public object vessel_line_up { get; set; }
        }
    }
}