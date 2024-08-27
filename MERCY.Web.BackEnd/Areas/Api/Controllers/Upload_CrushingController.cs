﻿using System;
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
    public class Upload_CrushingController : Controller
    {
        public JsonResult Index()
        {
            return Json("Upload_CrushingController", JsonRequestBehavior.AllowGet);
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

            if ( ! OurUtility.Upload_Record(user, fileName, fileName2, OurUtility.UPLOAD_CRUSHING_PLANT, ref id_File_Physical, ref msg))
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
            decimal CONST_100 = 100.0M;

            string message = string.Empty;

            try
            {
                bool isSheets_Valid = true;
                string sheet_Validation = string.Empty;

                List<Model_Product> products_Data = Product_Utility.Get(company.ToUpper());
                string products = string.Empty;
                string separator = string.Empty;

                List<string> products_List = new List<string>();
                foreach (Model_Product product in products_Data)
                {
                    // lakukan "Trim"
                    products_List.Add(product.Product_name.Trim());

                    products += separator + product.Product_name.Trim();
                    separator = ", ";
                }

                // Check jika Product: kosong
                if (string.IsNullOrEmpty(products))
                {
                    sheet_Validation = "Product for Company:" + company + " is Empty!";
                    var msgX = new { Status = false, Message = sheet_Validation, Id = id_File_Physical };
                    return Json(msgX, JsonRequestBehavior.AllowGet);
                }

                using (MERCY_Ctx db = new MERCY_Ctx())
                {
                    var dataQuery =
                            (
                                from d in db.TEMPORARY_File
                                where d.RecordId == id_File_Physical
                                select d
                            );

                    var dataFile = dataQuery.SingleOrDefault();
                    string sheetName = string.Empty;
                    bool is_Sheet_Valid = false;

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
                        long first_RecordId = 0;
                        string no = string.Empty;
                        
                        string tm_plan = string.Empty;
                        string ash_plan = string.Empty;
                        string ts_plan = string.Empty;
                        string cv_plan = string.Empty;

                        TEMPORARY_CRUSHING_PLANT_Header header = null;
                        TEMPORARY_CRUSHING_PLANT detail = null;

                        List<string> sheet_List = new List<string>();
                        foreach (Sheet sheet in wbPart.Workbook.Sheets)
                        {
                            is_Hidden = (sheet.State != null
                                            && sheet.State.HasValue
                                            && (sheet.State.Value == SheetStateValues.Hidden
                                                    || sheet.State.Value == SheetStateValues.VeryHidden));

                            // Skip worksheet yang "Hidden"
                            if (is_Hidden) continue;

                            sheetName = sheet.Name.ToString().Trim();
                            sheet_List.Add(sheetName);
                        }

                        foreach (string product in products_List)
                        {
                            if ( ! sheet_List.Contains(product))
                            {
                                isSheets_Valid = false;
                                sheet_Validation = string.Format(@"Sheet [{0}] not found!###Sheets required: [{1}]", product, products);
                                break;
                            }
                        }
                        if ( ! isSheets_Valid)
                        {
                            var msgX = new { Status = false, Message = sheet_Validation, Id = id_File_Physical };
                            return Json(msgX, JsonRequestBehavior.AllowGet);
                        }

                        foreach (Sheet sheet in wbPart.Workbook.Sheets)
                        {
                            is_Hidden = (sheet.State != null
                                            && sheet.State.HasValue
                                            && (sheet.State.Value == SheetStateValues.Hidden
                                                    || sheet.State.Value == SheetStateValues.VeryHidden));
                            
                            // Skip worksheet yang "Hidden"
                            if (is_Hidden) continue;

                            is_Sheet_Valid = false;
                            sheetName = sheet.Name.ToString().Trim();

                            if (products_List.Contains(sheetName))
                            {
                                is_Sheet_Valid = true;
                            }

                            if ( ! is_Sheet_Valid) continue;

                            try
                            {
                                wsPart = (WorksheetPart)(wbPart.GetPartById(sheet.Id));
                                
                                // jika Job_No kosong, maka Worksheet "kosong"
                                if (string.IsNullOrEmpty(ExcelProcessing.GetCellValue(wbPart, wsPart, "C9"))) continue;

                                header = new TEMPORARY_CRUSHING_PLANT_Header
                                {
                                    Sheet = sheetName,
                                    File_Physical = id_File_Physical,
                                    Company = company,
                                    Date_Detail = ExcelProcessing.GetCellValue(wbPart, wsPart, "U7"),
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

                                db.TEMPORARY_CRUSHING_PLANT_Header.Add(header);
                                db.SaveChanges();
                                
                                // mulai dari Baris
                                baris = 18;
                                first_RecordId = 0;

                                tm_plan = string.Empty;
                                ash_plan = string.Empty;
                                ts_plan = string.Empty;
                                cv_plan = string.Empty;
                                
                                string dateProduction = string.Empty;

                                while (true)
                                {
                                    dateProduction = ExcelProcessing.GetCellValue(wbPart, wsPart, "A" + baris.ToString());

                                    if (string.IsNullOrEmpty(dateProduction)) break;
                                    if (dateProduction == "Cummulative Result") break;

                                    detail = new TEMPORARY_CRUSHING_PLANT
                                    {
                                        Header = header.RecordId,

                                        Date_Production = ExcelProcessing.GetDate(dateProduction, "yyyy-MM-dd"),
                                        Shift_Work = ExcelProcessing.GetCellValue(wbPart, wsPart, "B" + baris.ToString()),
                                        Tonnage = ExcelProcessing.GetCellValue(wbPart, wsPart, "C" + baris.ToString()),
                                        Sample_ID = ExcelProcessing.GetCellValue(wbPart, wsPart, "D" + baris.ToString()),

                                        TM = ExcelProcessing.GetCellValue(wbPart, wsPart, "E" + baris.ToString()),
                                        M = ExcelProcessing.GetCellValue(wbPart, wsPart, "F" + baris.ToString()),
                                        ASH_adb = ExcelProcessing.GetCellValue(wbPart, wsPart, "G" + baris.ToString())
                                    };
                                    detail.ASH_arb = OurUtility.ToDecimal(OurUtility.ToDecimal(detail.ASH_adb, 2) * (CONST_100 - OurUtility.ToDecimal(detail.TM, 2)) / (CONST_100 - OurUtility.ToDecimal(detail.M, 2)), 2);
                                    detail.VM_adb = ExcelProcessing.GetCellValue(wbPart, wsPart, "I" + baris.ToString());
                                    detail.VM_arb = OurUtility.ToDecimal(OurUtility.ToDecimal(detail.VM_adb, 2) * (CONST_100 - OurUtility.ToDecimal(detail.TM, 2)) / (CONST_100 - OurUtility.ToDecimal(detail.M, 2)), 2);
                                    detail.FC_adb = OurUtility.ToDecimal(CONST_100 - OurUtility.ToDecimal(detail.M, 2) - OurUtility.ToDecimal(detail.ASH_adb, 2) - OurUtility.ToDecimal(detail.VM_adb, 2), 2);
                                    detail.FC_arb = OurUtility.ToDecimal(OurUtility.ToDecimal(detail.FC_adb, 2) * (CONST_100 - OurUtility.ToDecimal(detail.TM, 2)) / (CONST_100 - OurUtility.ToDecimal(detail.M, 2)), 2);
                                    detail.TS_adb = ExcelProcessing.GetCellValue(wbPart, wsPart, "M" + baris.ToString());
                                    detail.TS_arb = OurUtility.ToDecimal(OurUtility.ToDecimal(detail.TS_adb, 2) * (CONST_100 - OurUtility.ToDecimal(detail.TM, 2)) / (CONST_100 - OurUtility.ToDecimal(detail.M, 2)), 2);
                                    detail.CV_adb = ExcelProcessing.GetCellValue(wbPart, wsPart, "O" + baris.ToString());
                                    detail.CV_db = OurUtility.ToDecimal(OurUtility.ToDecimal(detail.CV_adb, 0) * (CONST_100) / (CONST_100 - OurUtility.ToDecimal(detail.M, 2)), 0);
                                    detail.CV_arb = OurUtility.ToDecimal(OurUtility.ToDecimal(detail.CV_adb, 0) * (CONST_100 - OurUtility.ToDecimal(detail.TM, 2)) / (CONST_100 - OurUtility.ToDecimal(detail.M, 2)), 0);
                                    detail.CV_daf = OurUtility.ToDecimal(OurUtility.ToDecimal(detail.CV_adb, 0) * (CONST_100) / (CONST_100 - OurUtility.ToDecimal(detail.M, 2) - OurUtility.ToDecimal(detail.ASH_adb, 2)), 0);
                                    detail.CV_ad_15 = OurUtility.ToDecimal(OurUtility.ToDecimal(detail.CV_adb, 0) * (CONST_100 - 15) / (CONST_100 - OurUtility.ToDecimal(detail.M, 2)), 0);
                                    detail.CV_ad_16 = OurUtility.ToDecimal(OurUtility.ToDecimal(detail.CV_adb, 0) * (CONST_100 - 16) / (CONST_100 - OurUtility.ToDecimal(detail.M, 2)), 0);
                                    detail.CV_ad_17 = OurUtility.ToDecimal(OurUtility.ToDecimal(detail.CV_adb, 0) * (CONST_100 - 17) / (CONST_100 - OurUtility.ToDecimal(detail.M, 2)), 0);

                                    detail.Remark = ExcelProcessing.GetCellValue(wbPart, wsPart, "V" + baris.ToString());

                                    if (string.IsNullOrEmpty(tm_plan))
                                    {
                                        tm_plan = ExcelProcessing.GetCellValue(wbPart, wsPart, "W" + baris.ToString());
                                        ash_plan = ExcelProcessing.GetCellValue(wbPart, wsPart, "Y" + baris.ToString());
                                        ts_plan = ExcelProcessing.GetCellValue(wbPart, wsPart, "AA" + baris.ToString());
                                        cv_plan = ExcelProcessing.GetCellValue(wbPart, wsPart, "AC" + baris.ToString());
                                    }

                                    detail.TM_Plan = tm_plan;
                                    detail.ASH_Plan = ash_plan;
                                    detail.TS_Plan = ts_plan;
                                    detail.CV_Plan = cv_plan;

                                    detail.CreatedOn = DateTime.Now;
                                    detail.CreatedBy = user.UserId;
                                    detail.CreatedOn_Date_Only = detail.CreatedOn.ToString("yyyy-MM-dd");
                                    detail.CreatedOn_Year_Only = detail.CreatedOn.Year;
                                    detail.Company = company;
                                    detail.Sheet = sheetName;

                                    Check_Validation(detail, keys_Unique);

                                    // init Status, start with "InValid"
                                    status = "Invalid";

                                    if (detail.Date_Production_isvalid
                                        && detail.Shift_Work_isvalid
                                        && detail.Tonnage_isvalid
                                        && detail.Sample_ID_isvalid
                                        && detail.TM_isvalid
                                        && detail.M_isvalid
                                        && detail.ASH_adb_isvalid
                                        && detail.ASH_arb_isvalid
                                        && detail.VM_adb_isvalid
                                        && detail.VM_arb_isvalid
                                        && detail.FC_adb_isvalid
                                        && detail.FC_arb_isvalid
                                        && detail.TS_adb_isvalid
                                        && detail.TS_arb_isvalid
                                        && detail.CV_adb_isvalid
                                        && detail.CV_db_isvalid
                                        && detail.CV_arb_isvalid
                                        && detail.CV_daf_isvalid
                                        && detail.CV_ad_15_isvalid
                                        && detail.CV_ad_16_isvalid
                                        && detail.CV_ad_17_isvalid
                                        && detail.Remark_isvalid

                                        && detail.CV_Plan_isvalid
                                        && detail.TM_Plan_isvalid
                                        && detail.ASH_Plan_isvalid
                                        && detail.TS_Plan_isvalid
                                    )
                                    {
                                        status = "New";
                                    }

                                    detail.Status = status;

                                    db.TEMPORARY_CRUSHING_PLANT.Add(detail);
                                    db.SaveChanges();

                                    // simpan Id pertama
                                    if (first_RecordId <= 0) first_RecordId = detail.RecordId;

                                    baris++;

                                    // for Next "Unique"
                                    try
                                    {
                                        keys_Unique.Add(detail.Sample_ID);
                                    }
                                    catch {}
                                }

                                try
                                {
                                    bool isNeed_Update = false;

                                    // ambil semua data yang baru di Insert diatas
                                    // yang Status: New
                                    List<TEMPORARY_CRUSHING_PLANT> recordSet = (
                                                                                from dt in db.TEMPORARY_CRUSHING_PLANT
                                                                                orderby dt.RecordId
                                                                                where dt.Header == header.RecordId // data yang baru di Insert diatas
                                                                                        && dt.Status == "New"
                                                                                select dt
                                                                                ).ToList();

                                    // proses RecordSet
                                    foreach (TEMPORARY_CRUSHING_PLANT row in recordSet)
                                    {
                                        // cari data "sebelumnya" di Table "Actual Upload"
                                        // punya Unique Key yang sama
                                        if (db.UPLOAD_CRUSHING_PLANT.Where(d =>
                                                                                d.RecordId < first_RecordId // data "sebelumnya"
                                                                                && d.Sample_ID == row.Sample_ID
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
                                from dt in db.TEMPORARY_CRUSHING_PLANT
                                join head in db.TEMPORARY_CRUSHING_PLANT_Header on dt.Header equals head.RecordId
                                orderby dt.RecordId
                                where head.File_Physical == id
                                //&& head.Sheet == sheet
                                select new Model_View_TEMPORARY_CRUSHING_PLANT
                                {
                                    Status = dt.Status
                                    , data_RecordId = dt.RecordId
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
                                    , Sheet = head.Sheet

                                    , TM_Plan = dt.TM_Plan
                                    , ASH_Plan = dt.ASH_Plan
                                    , TS_Plan = dt.TS_Plan
                                    , CV_Plan = dt.CV_Plan

                                    , Date_Production_isvalid = dt.Date_Production_isvalid
                                    , Shift_Work_isvalid = dt.Shift_Work_isvalid
                                    , Tonnage_isvalid = dt.Tonnage_isvalid
                                    , Sample_ID_isvalid = dt.Sample_ID_isvalid
                                    , TM_isvalid = dt.TM_isvalid
                                    , M_isvalid = dt.M_isvalid
                                    , ASH_adb_isvalid = dt.ASH_adb_isvalid
                                    , ASH_arb_isvalid = dt.ASH_arb_isvalid
                                    , VM_adb_isvalid = dt.VM_adb_isvalid
                                    , VM_arb_isvalid = dt.VM_arb_isvalid
                                    , FC_adb_isvalid = dt.FC_adb_isvalid
                                    , FC_arb_isvalid = dt.FC_arb_isvalid
                                    , TS_adb_isvalid = dt.TS_adb_isvalid
                                    , TS_arb_isvalid = dt.TS_arb_isvalid
                                    , CV_adb_isvalid = dt.CV_adb_isvalid
                                    , CV_db_isvalid = dt.CV_db_isvalid
                                    , CV_arb_isvalid = dt.CV_arb_isvalid
                                    , CV_daf_isvalid = dt.CV_daf_isvalid
                                    , CV_ad_15_isvalid = dt.CV_ad_15_isvalid
                                    , CV_ad_16_isvalid = dt.CV_ad_16_isvalid
                                    , CV_ad_17_isvalid = dt.CV_ad_17_isvalid
                                    , Remark_isvalid = dt.Remark_isvalid

                                    , CV_Plan_isvalid = dt.CV_Plan_isvalid
                                    , TM_Plan_isvalid = dt.TM_Plan_isvalid
                                    , ASH_Plan_isvalid = dt.ASH_Plan_isvalid
                                    , TS_Plan_isvalid = dt.TS_Plan_isvalid
                                }
                            );

                    var items = dataQuery.ToList();

                    items.ForEach(c =>
                    {
                        c.Tonnage_Str = Value_Of_3(c.Tonnage_isvalid, c.Tonnage);
                        c.TM_Str = Value_Of(c.TM_isvalid, c.TM);
                        c.M_Str = Value_Of(c.M_isvalid, c.M);
                        c.ASH_adb_Str = Value_Of(c.ASH_adb_isvalid, c.ASH_adb);
                        c.ASH_arb_Str = String.Format("{0:n2}", Decimal.Round(c.ASH_arb, 2));
                        c.VM_adb_Str = Value_Of(c.VM_adb_isvalid, c.VM_adb);
                        c.VM_arb_Str = String.Format("{0:n2}", Decimal.Round(c.VM_arb, 2));
                        c.FC_adb_Str = String.Format("{0:n2}", Decimal.Round(c.FC_adb, 2));
                        c.FC_arb_Str = String.Format("{0:n2}", Decimal.Round(c.FC_arb, 2));
                        c.TS_adb_Str = Value_Of(c.TS_adb_isvalid, c.TS_adb);
                        c.TS_arb_Str = String.Format("{0:n2}", Decimal.Round(c.TS_arb, 2));
                        c.CV_adb_Str = Value_Of(c.CV_adb_isvalid, c.CV_adb);
                        c.CV_db_Str = String.Format("{0:n0}", Decimal.Round(c.CV_db, 0));
                        c.CV_arb_Str = String.Format("{0:n0}", Decimal.Round(c.CV_arb, 0));
                        c.CV_daf_Str = String.Format("{0:n0}", Decimal.Round(c.CV_daf, 0));
                        c.CV_ad_15_Str = String.Format("{0:n0}", Decimal.Round(c.CV_ad_15, 0));
                        c.CV_ad_16_Str = String.Format("{0:n0}", Decimal.Round(c.CV_ad_16, 0));
                        c.CV_ad_17_Str = String.Format("{0:n0}", Decimal.Round(c.CV_ad_17, 0));

                        c.TM_Plan_Str = Value_Of(c.TM_Plan_isvalid, c.TM_Plan);
                        c.ASH_Plan_Str = Value_Of(c.TM_isvalid, c.ASH_Plan);
                        c.TS_Plan_Str = Value_Of(c.TM_isvalid, c.TM);
                        c.CV_Plan_Str = Value_Of(c.TM_isvalid, c.TM);
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

                        command.CommandText = string.Format(@"exec UPLOAD_ACTUAL_CRUSHING_PLANT {0}", id);

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

        public static void Check_Validation(TEMPORARY_CRUSHING_PLANT p_detail, List<string> p_keys_Unique)
        {
            // Key: Valid, jika belum pernah ada
            p_detail.Sample_ID_isvalid = Calculate_Validation_of_Key(p_detail.Sample_ID, p_keys_Unique);

            // Other column
            p_detail.Date_Production_isvalid = Calculate_Validation_of_Text(p_detail.Date_Production);
            p_detail.Shift_Work_isvalid = Calculate_Validation_of_Text(p_detail.Shift_Work);
            p_detail.Tonnage_isvalid = Calculate_Validation_of_Numeric(p_detail.Tonnage);
            p_detail.TM_isvalid = Calculate_Validation_of_Numeric(p_detail.TM);
            p_detail.M_isvalid = Calculate_Validation_of_Numeric(p_detail.M);
            p_detail.ASH_adb_isvalid = Calculate_Validation_of_Numeric(p_detail.ASH_adb);
            p_detail.ASH_arb_isvalid = true;
            p_detail.VM_adb_isvalid = Calculate_Validation_of_Numeric(p_detail.VM_adb);
            p_detail.VM_arb_isvalid = true;
            p_detail.FC_adb_isvalid = true;
            p_detail.FC_arb_isvalid = true;
            p_detail.TS_adb_isvalid = Calculate_Validation_of_Numeric(p_detail.TS_adb);
            p_detail.TS_arb_isvalid = true;
            p_detail.CV_adb_isvalid = Calculate_Validation_of_Numeric(p_detail.CV_adb);
            p_detail.CV_db_isvalid = true;
            p_detail.CV_arb_isvalid = true;
            p_detail.CV_daf_isvalid = true;
            p_detail.CV_ad_15_isvalid = true;
            p_detail.CV_ad_16_isvalid = true;
            p_detail.CV_ad_17_isvalid = true;
            p_detail.Remark_isvalid = true;

            p_detail.CV_Plan_isvalid = Calculate_Validation_of_Numeric(p_detail.CV_Plan);
            p_detail.TM_Plan_isvalid = Calculate_Validation_of_Numeric(p_detail.TM_Plan);
            p_detail.ASH_Plan_isvalid = Calculate_Validation_of_Numeric(p_detail.ASH_Plan);
            p_detail.TS_Plan_isvalid = Calculate_Validation_of_Numeric(p_detail.TS_Plan);
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
            if ( ! p_isValid)
            {
                // maka Tampilkan Nilai apa adanya
                return p_value;
            }

            // disini pasti Valid
            // maka tampilkan versi "Numeric" yang sudah dilakukan Round()
            return string.Format("{0:N0}", OurUtility.Round(p_value, 0));
        }

        public static string Value_Of_3(bool p_isValid, string p_value)
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
            return string.Format("{0:N3}", OurUtility.Round(p_value, 3));
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
                                                                left join TEMPORARY_CRUSHING_PLANT_Header h on t.RecordId = h.File_Physical
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
                                from d in p_db.UPLOAD_CRUSHING_PLANT
                                where d.LECO_Stamp == p_leco_Stamp
                                select d
                            );

                var data = dataQuery.ToList();
                bool is_Changed = false;

                decimal CONST_100 = 100.0M;

                foreach (var detail in data)
                {
                    /* See: ParsingContent()
                    detail.ASH_arb = OurUtility.ToDecimal(OurUtility.ToDecimal(detail.ASH_adb, 2) * (CONST_100 - OurUtility.ToDecimal(detail.TM, 2)) / (CONST_100 - OurUtility.ToDecimal(detail.M, 2)), 2);
                    detail.VM_arb = OurUtility.ToDecimal(OurUtility.ToDecimal(detail.VM_adb, 2) * (CONST_100 - OurUtility.ToDecimal(detail.TM, 2)) / (CONST_100 - OurUtility.ToDecimal(detail.M, 2)), 2);
                    detail.FC_adb = OurUtility.ToDecimal(CONST_100 - OurUtility.ToDecimal(detail.M, 2) - OurUtility.ToDecimal(detail.ASH_adb, 2) - OurUtility.ToDecimal(detail.VM_adb, 2), 2);
                    detail.FC_arb = OurUtility.ToDecimal(OurUtility.ToDecimal(detail.FC_adb, 2) * (CONST_100 - OurUtility.ToDecimal(detail.TM, 2)) / (CONST_100 - OurUtility.ToDecimal(detail.M, 2)), 2);
                    detail.TS_arb = OurUtility.ToDecimal(OurUtility.ToDecimal(detail.TS_adb, 2) * (CONST_100 - OurUtility.ToDecimal(detail.TM, 2)) / (CONST_100 - OurUtility.ToDecimal(detail.M, 2)), 2);
                    detail.CV_db = OurUtility.ToDecimal(OurUtility.ToDecimal(detail.CV_adb, 0) * (CONST_100) / (CONST_100 - OurUtility.ToDecimal(detail.M, 2)), 0);
                    detail.CV_arb = OurUtility.ToDecimal(OurUtility.ToDecimal(detail.CV_adb, 0) * (CONST_100 - OurUtility.ToDecimal(detail.TM, 2)) / (CONST_100 - OurUtility.ToDecimal(detail.M, 2)), 0);
                    detail.CV_daf = OurUtility.ToDecimal(OurUtility.ToDecimal(detail.CV_adb, 0) * (CONST_100) / (CONST_100 - OurUtility.ToDecimal(detail.M, 2) - OurUtility.ToDecimal(detail.ASH_adb, 2)), 0);
                    detail.CV_ad_15 = OurUtility.ToDecimal(OurUtility.ToDecimal(detail.CV_adb, 0) * (CONST_100 - 15) / (CONST_100 - OurUtility.ToDecimal(detail.M, 2)), 0);
                    detail.CV_ad_16 = OurUtility.ToDecimal(OurUtility.ToDecimal(detail.CV_adb, 0) * (CONST_100 - 16) / (CONST_100 - OurUtility.ToDecimal(detail.M, 2)), 0);
                    detail.CV_ad_17 = OurUtility.ToDecimal(OurUtility.ToDecimal(detail.CV_adb, 0) * (CONST_100 - 17) / (CONST_100 - OurUtility.ToDecimal(detail.M, 2)), 0);
                    */

                    detail.ASH_arb = detail.ASH_adb * (CONST_100 - detail.TM) / (CONST_100 - detail.M);
                    detail.VM_arb = detail.VM_adb * (CONST_100 - detail.TM) / (CONST_100 - detail.M);
                    detail.FC_adb = CONST_100 - detail.M - detail.ASH_adb - detail.VM_adb;
                    detail.FC_arb = detail.FC_adb * (CONST_100 - detail.TM) / (CONST_100 - detail.M);
                    detail.TS_arb = detail.TS_adb * (CONST_100 - detail.TM) / (CONST_100 - detail.M);
                    detail.CV_db = detail.CV_adb * (CONST_100) / (CONST_100 - detail.M);
                    detail.CV_arb = detail.CV_adb * (CONST_100 - detail.TM) / (CONST_100 - detail.M);
                    detail.CV_daf = detail.CV_adb * (CONST_100) / (CONST_100 - detail.M - detail.ASH_adb);
                    detail.CV_ad_15 = detail.CV_adb * (CONST_100 - 15) / (CONST_100 - detail.M);
                    detail.CV_ad_16 = detail.CV_adb * (CONST_100 - 16) / (CONST_100 - detail.M);
                    detail.CV_ad_17 = detail.CV_adb * (CONST_100 - 17) / (CONST_100 - detail.M);

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