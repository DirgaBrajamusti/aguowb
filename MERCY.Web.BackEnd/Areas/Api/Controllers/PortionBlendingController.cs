using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

using System.Data;
using System.Data.SqlClient;

using Newtonsoft.Json;

using MERCY.Data.EntityFramework;
using MERCY.Web.BackEnd.Models;
using MERCY.Web.BackEnd.Helpers;
using MERCY.Web.BackEnd.Security;
using Permission = MERCY.Web.BackEnd.Security.Permission;

namespace MERCY.Web.BackEnd.Areas.Api.Controllers
{
    public class PortionBlendingController : Controller
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

            // -- Actual code
            string company = Request["company"];
            bool is_company_ALL = (string.IsNullOrEmpty(company) || company == "all");

            string dateFrom = Request["dateFrom"];
            string dateTo = Request["dateTo"];

            string shift_str = Request["shift"];
            bool is_shift_ALL = (string.IsNullOrEmpty(shift_str) || shift_str == "all");
            int shift = OurUtility.ToInt32(shift_str);

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

                using (MERCY_Ctx db = new MERCY_Ctx())
                {
                    var dataQuery =
                            (
                                from d in db.PortionBlendings
                                where (is_company_ALL || d.Company == company)
                                    && d.BlendingDate >= dateFrom_O
                                    && d.BlendingDate < dateTo_O
                                    && (is_shift_ALL || d.Shift == shift)
                                orderby d.BlendingDate, d.Shift
                                select new Model_View_PortionBlending
                                {
                                    RecordId = d.RecordId
                                    , Company = d.Company
                                    , BlendingDate = d.BlendingDate
                                    , Shift = d.Shift
                                    , Product = d.Product
                                    , ROM = ""
                                    , Destination = ""
                                    , Portion = ""
                                    , Hopper = d.Hopper
                                    , Tunnel = d.Tunnel
                                    , Remark = d.Remark
                                    , TotalBucket = d.TotalBucket
                                    , ROM_ID = d.ROM_ID
                                    , ROM_Name = d.ROM_Name
                                }
                            );

                    var items = dataQuery.ToList();

                    try
                    {
                        items.ForEach(c =>
                        {
                            try
                            {
                                c.BlendingDate_Str = OurUtility.DateFormat(c.BlendingDate, "dd-MMM-yyyy");
                                //c.RequestDate_Str = OurUtility.DateFormat(c.RequestDate, "dd-MMM-yyyy HH:mm");
                                //c.RequestDate_Str2 = OurUtility.DateFormat(c.RequestDate, "MM/dd/yyyy");
                                //c.CreatedOn_Str = OurUtility.DateFormat(c.CreatedOn, "dd-MMM-yyyy HH:mm");

                                var dataQuery_Detail =
                                (
                                    from d in db.PortionBlending_Details
                                    where d.PortionBlending == c.RecordId
                                    orderby d.block, d.ROM_Name
                                    select new
                                    {
                                        Names = (d.block + " " + d.ROM_Name)
                                        , d.Portion
                                    }
                                );

                                var items_Detail = dataQuery_Detail.ToList();

                                int i = 0;
                                items_Detail.ForEach(e =>
                                {
                                    i++;
                                    c.ROM += (i>1?"<br/>":"") + e.Names;
                                    c.Portion += (i > 1 ? "<br/>" : "") + e.Portion + @"%";
                                });

                                c.Destination += c.Hopper;

                                c.Remark_Str = OurUtility.Handle_Enter(c.Remark);
                            }
                            catch {}
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
                    string company = OurUtility.ValueOf(p_collection, "Company");
                    string product = OurUtility.ValueOf(p_collection, "Product");
                    DateTime blendingDate = DateTime.Parse(OurUtility.ValueOf(p_collection, "BlendingDate"));
                    int shift = OurUtility.ToInt32(OurUtility.ValueOf(p_collection, "Shift"));
                    string romName = OurUtility.ValueOf(p_collection, "RomName");
                    int romId = OurUtility.ToInt32(OurUtility.ValueOf(p_collection, "RomId"));
                    int totalBucket = OurUtility.ToInt32(OurUtility.ValueOf(p_collection, "TotalBucket"));
                    bool isAdditionalBucket = OurUtility.ValueOf(p_collection, "IsAdditionalBucket").Equals("1");

                    var dataQueryx =
                            (
                                from d in db.PortionBlendings
                                where d.Company == company
                                    && d.BlendingDate == blendingDate
                                    && d.Shift == shift
                                select d
                            );

                    var lab = dataQueryx.FirstOrDefault();
                    if (lab == null)
                    {
                        // aman
                    }
                    else
                    {
                        // sudah ada data
                        string msg2 = string.Format(@"Data already exists in Portion Blending.
    Company: {0}
    Blending Date: {1}
    Shift: {2}", company, blendingDate.ToString("dd-MMM-yyyy"), shift);
                        var result2 = new { Success = false, Permission = permission_Item, Message = msg2, MessageDetail = string.Empty, Version = Configuration.VERSION, Data = 0 };
                        return Json(result2, JsonRequestBehavior.AllowGet);
                    }

                    var data = new PortionBlending
                    {
                        Company = company,
                        Product = product,
                        BlendingDate = blendingDate,
                        Shift = shift,
                        NoHauling = OurUtility.ValueOf(p_collection, "NoHauling").Equals("1"),
                        Remark = OurUtility.ValueOf(p_collection, "Remark"),
                        TotalBucket = null,
                        ROM_ID = null,
                        ROM_Name = null,
                    };

                    if (isAdditionalBucket)
                    {
                        data.TotalBucket = totalBucket;
                        data.ROM_ID = romId;
                        data.ROM_Name = romName;
                    }

                    if (data.NoHauling)
                    {
                        // karena NoHauling == true, maka tidak usah disimpan nilai-nilai:
                        data.CV = 0;
                        data.TS = 0;
                        data.ASH = 0;
                        data.IM = 0;
                        data.TM = 0;
                        data.Ton = 0;

                        data.Hopper = string.Empty;
                        data.Tunnel = string.Empty;
                    }
                    else
                    {
                        // karena NoHauling == false, maka simpan nilai-nilai:
                        data.CV = OurUtility.ToDouble(OurUtility.ValueOf(p_collection, "CV"));
                        data.TS = OurUtility.ToDouble(OurUtility.ValueOf(p_collection, "TS"));
                        data.ASH = OurUtility.ToDouble(OurUtility.ValueOf(p_collection, "ASH"));
                        data.IM = OurUtility.ToDouble(OurUtility.ValueOf(p_collection, "IM"));
                        data.TM = OurUtility.ToDouble(OurUtility.ValueOf(p_collection, "TM"));
                        data.Ton = OurUtility.ToDecimal(OurUtility.ValueOf(p_collection, "Ton"));
                        data.Hopper = OurUtility.ValueOf(p_collection, "Hopper");
                        data.Tunnel = OurUtility.ValueOf(p_collection, "Tunnel");

                        // saat menyimpan ke Database, kita memastikan bahwa kita melakukan Rounding
                        data.CV = OurUtility.Round(data.CV, 0);
                        data.TS = OurUtility.Round(data.TS, 2);
                        data.ASH = OurUtility.Round(data.ASH, 2);
                        data.IM = OurUtility.Round(data.IM, 2);
                        data.TM = OurUtility.Round(data.TM, 2);
                        data.Ton = OurUtility.Round(data.Ton, 2);
                    }

                    data.CreatedOn = DateTime.Now;
                    data.CreatedBy = user.UserId;

                    db.PortionBlendings.Add(data);
                    db.SaveChanges();

                    long id = data.RecordId;

                    if (data.NoHauling)
                    {
                        // karena NoHauling == true, maka tidak usah disimpan nilai-nilai "ROM Quality"
                    }
                    else
                    {
                        // karena NoHauling == false, maka simpan nilai-nilai "ROM Quality"
                        string msg = string.Empty;
                        string msgx = string.Empty;
                        using (var connection = new SqlConnection(db.Database.Connection.ConnectionString))
                        {
                            connection.Open();

                            Add_Detail(connection, id, OurUtility.ValueOf(p_collection, "ROM_Quality"), ref msgx);
                            msg += msgx;
                            connection.Close();
                        }
                    }
                    
                    var result = new { Success = true, Permission = permission_Item, Message = BaseConstants.MESSAGE_CREATE_SUCCESS, MessageDetail = string.Empty, Version = Configuration.VERSION, Data = id };
                    return Json(result, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                var result = new { Success = false, Permission = permission_Item, ex.Message, MessageDetail = string.Empty, Version = Configuration.VERSION, Data = 0 };
                return Json(result, JsonRequestBehavior.AllowGet);
            }
        }
        
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
                                from d in db.PortionBlendings
                                where d.RecordId == id
                                select new Model_View_PortionBlending
                                {
                                    RecordId = d.RecordId
                                    , Company = d.Company
                                    , Product = d.Product
                                    , BlendingDate = d.BlendingDate
                                    , Shift = d.Shift
                                    , NoHauling = d.NoHauling
                                    , Remark = d.Remark
                                    , CV = d.CV
                                    , TS = d.TS
                                    , ASH = d.ASH
                                    , IM = d.IM
                                    , TM = d.TM
                                    , Ton = d.Ton
                                    , Hopper = d.Hopper
                                    , Tunnel = d.Tunnel
                                    , ROM_ID = d.ROM_ID
                                    , ROM_Name = d.ROM_Name
                                    , TotalBucket = d.TotalBucket
                                }
                            );

                    var data = dataQuery.SingleOrDefault();
                    if (data == null)
                    {
                        var result_NotFound = new { Success = false, Permission = permission_Item, User = UserX.Information(user, is_Show_User_Menu, is_Show_User_Relation), Message = "Id: " + id.ToString() + " is not found", MessageDetail = string.Empty, Version = Configuration.VERSION};
                        return Json(result_NotFound, JsonRequestBehavior.AllowGet);
                    }
                    
                    data.BlendingDate_Str = OurUtility.DateFormat(data.BlendingDate, "MM/dd/yyyy");

                    var dataQuery_ROM_Quality =
                            (
                                from d in db.PortionBlending_Details
                                where d.PortionBlending == id
                                orderby d.block, d.ROM_Name
                                select new Model_View_PortionBlending_Details
                                {
                                    RecordId = d.RecordId
                                    , ROMQuality_RecordId = d.ROMQuality_RecordId
                                    , Portion = d.Portion

                                    , ROM_Id = d.ROM_ID
                                    , Names = (d.block + " " + d.ROM_Name)
                                    , CV = d.CV
                                    , Block = d.block
                                    , ROM_Name = d.ROM_Name
                                    , TM = d.TM
                                    , TS = d.TS
                                    , Ton = d.Ton
                                    , IM = d.IM
                                    , ASH = d.ASH
                                }
                            );

                    var data_ROM_Quality = dataQuery_ROM_Quality.ToList();

                    double cv_total = 0.0;
                    double ts_total = 0.0;
                    double ash_total = 0.0;
                    double im_total = 0.0;
                    double tm_total = 0.0;

                    data_ROM_Quality.ForEach(c2 =>
                    {
                        c2.CV_Str = string.Format("{0:N0}", OurUtility.Round(c2.CV, 0));

                        //Kalkulasi Quality, dengan strategi seperti berikut:
                        //   - pastikan bahwa semua data selalu di Round terlebih dahulu
                        //   - baru kemudian dilakukan Kalkulasi Penjumlahan
                        cv_total += ((OurUtility.Round(c2.CV, 2) * c2.Portion) / 100.0);
                        ts_total += ((OurUtility.Round(c2.TS, 2) * c2.Portion) / 100.0);
                        ash_total += ((OurUtility.Round(c2.ASH, 2) * c2.Portion) / 100.0);
                        im_total += ((OurUtility.Round(c2.IM, 2) * c2.Portion) / 100.0);
                        tm_total += ((OurUtility.Round(c2.TM, 2) * c2.Portion) / 100.0);
                    });

                    // kembali dilakukan Rounding, agar konsisten
                    cv_total = OurUtility.Round(cv_total, 0);
                    ts_total = OurUtility.Round(ts_total, 2);
                    ash_total = OurUtility.Round(ash_total, 2);
                    im_total = OurUtility.Round(im_total, 2);
                    tm_total = OurUtility.Round(tm_total, 2);

                    string cv_total_str = string.Format("{0:N0}", cv_total);
                    string ts_total_str = string.Format("{0:N2}", ts_total);
                    string ash_total_str = string.Format("{0:N2}", ash_total);
                    string im_total_str = string.Format("{0:N2}", im_total);
                    string tm_total_str = string.Format("{0:N2}", tm_total);

                    return Json(new { Success = true, Permission = permission_Item, User = UserX.Information(user, is_Show_User_Menu, is_Show_User_Relation), Item = data, ROM_Quality = data_ROM_Quality,  Quality_CV = cv_total_str, Quality_TS = ts_total_str, Quality_ASH = ash_total_str, Quality_IM = im_total_str, Quality_TM = tm_total_str }, JsonRequestBehavior.AllowGet);
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
            long id = OurUtility.ToInt64(Request[".id"]);

            try
            {
                using (MERCY_Ctx db = new MERCY_Ctx())
                {
                    var dataQuery =
                        (
                            from d in db.PortionBlendings
                            where d.RecordId == id
                            select d
                        );

                    var data = dataQuery.SingleOrDefault();

                    string company = OurUtility.ValueOf(p_collection, "Company");
                    string product = OurUtility.ValueOf(p_collection, "Product");
                    DateTime blendingDate = DateTime.Parse(OurUtility.ValueOf(p_collection, "BlendingDate"));
                    int shift = OurUtility.ToInt32(OurUtility.ValueOf(p_collection, "Shift"));

                    string romName = OurUtility.ValueOf(p_collection, "RomName");
                    int romId = OurUtility.ToInt32(OurUtility.ValueOf(p_collection, "RomId"));
                    int totalBucket = OurUtility.ToInt32(OurUtility.ValueOf(p_collection, "TotalBucket"));
                    bool isAdditionalBucket = OurUtility.ValueOf(p_collection, "IsAdditionalBucket").Equals("1");

                    // nilai-nilai berikut, tidak bisa diubah
                    //data.Company = company;
                    //data.BlendingDate = blendingDate;
                    //data.Shift = shift;

                    // lakukan perubahan nilai-nilai:
                    data.Product = product;
                    data.NoHauling = OurUtility.ValueOf(p_collection, "NoHauling").Equals("1");
                    data.Remark = OurUtility.ValueOf(p_collection, "Remark");
                    data.ROM_ID = null;
                    data.ROM_Name = null;
                    data.TotalBucket = null;

                    if (isAdditionalBucket)
                    {
                        data.ROM_ID = romId;
                        data.ROM_Name = romName;
                        data.TotalBucket = totalBucket;
                    }

                    if (data.NoHauling)
                    {
                        // karena NoHauling == true, maka tidak usah disimpan nilai-nilai:
                        data.CV = 0;
                        data.TS = 0;
                        data.ASH = 0;
                        data.IM = 0;
                        data.TM = 0;
                        data.Ton = 0;

                        data.Hopper = string.Empty;
                        data.Tunnel = string.Empty;
                    }
                    else
                    {
                        // karena NoHauling == false, maka simpan nilai-nilai:
                        data.CV = OurUtility.ToDouble(OurUtility.ValueOf(p_collection, "CV"));
                        data.TS = OurUtility.ToDouble(OurUtility.ValueOf(p_collection, "TS"));
                        data.ASH = OurUtility.ToDouble(OurUtility.ValueOf(p_collection, "ASH"));
                        data.IM = OurUtility.ToDouble(OurUtility.ValueOf(p_collection, "IM"));
                        data.TM = OurUtility.ToDouble(OurUtility.ValueOf(p_collection, "TM"));
                        data.Ton = OurUtility.ToDecimal(OurUtility.ValueOf(p_collection, "Ton"));
                        data.Hopper = OurUtility.ValueOf(p_collection, "Hopper");
                        data.Tunnel = OurUtility.ValueOf(p_collection, "Tunnel");

                        // saat menyimpan ke Database, kita memastikan bahwa kita melakukan Rounding
                        data.CV = OurUtility.Round(data.CV, 0);
                        data.TS = OurUtility.Round(data.TS, 2);
                        data.ASH = OurUtility.Round(data.ASH, 2);
                        data.IM = OurUtility.Round(data.IM, 2);
                        data.TM = OurUtility.Round(data.TM, 2);
                        data.Ton = OurUtility.Round(data.Ton, 2);
                    }

                    data.LastModifiedOn = DateTime.Now;
                    data.LastModifiedBy = user.UserId;

                    db.SaveChanges();

                    string msg = string.Empty;
                    string msgx = string.Empty;
                    using (var connection = new SqlConnection(db.Database.Connection.ConnectionString))
                    {
                        connection.Open();

                        Delete_Detail(connection, id, ref msgx);

                        if (data.NoHauling)
                        {
                            // karena NoHauling == true, maka tidak usah disimpan nilai-nilai "ROM Quality"
                        }
                        else
                        {
                            // karena NoHauling == false, maka simpan nilai-nilai "ROM Quality"
                            Add_Detail(connection, id, OurUtility.ValueOf(p_collection, "ROM_Quality"), ref msgx);
                        }

                        msg += msgx;
                        connection.Close();
                    }

                    var result = new { Success = true, Permission = permission_Item, Message = BaseConstants.MESSAGE_UPDATE_SUCCESS, MessageDetail = string.Empty, Version = Configuration.VERSION, Data = id };
                    return Json(result, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                var result = new { Success = false, Permission = permission_Item, ex.Message, MessageDetail = string.Empty, Version = Configuration.VERSION, Data = 0 };
                return Json(result, JsonRequestBehavior.AllowGet);
            }
        }

        public JsonResult GetROMsQuality()
        {
            var controller_Company = new CompanyController();
            controller_Company.InitializeController(this.Request.RequestContext);

            if(controller_Company.Is_from_BigData(OurUtility.ValueOf(Request, "company")))
            {
                // this is from BigData
                var controllerB = new ExternalDataController();
                controllerB.InitializeController(this.Request.RequestContext);

                return controllerB.GetROMsQuality();
            }

            // Load data from Local Database
            return controller_Company.GetROMsQuality();
        }

        private static bool Add_Detail(SqlConnection p_db, long p_portionBlending, string p_ROM_Quality, ref string p_message)
        {
            bool result = false;

            p_message = string.Empty;

            try
            {
                p_ROM_Quality = System.Uri.UnescapeDataString(p_ROM_Quality);
                p_ROM_Quality = System.Uri.UnescapeDataString(p_ROM_Quality);
                p_ROM_Quality = System.Web.HttpUtility.HtmlDecode(p_ROM_Quality);

                string[] rom_Qualities = p_ROM_Quality.Split(new string[] { "###" }, StringSplitOptions.None);

                string recordId = string.Empty;
                string block = string.Empty;
                string rom = string.Empty;
                string romId = string.Empty;
                string ton = string.Empty;

                string cv = string.Empty;
                string ts = string.Empty;
                string ash = string.Empty;
                string im = string.Empty;
                string tm = string.Empty;
                string portion = string.Empty;

                SqlCommand command = p_db.CreateCommand();
                string sql = string.Empty;

                foreach (string rom_Quality in rom_Qualities)
                {
                    if (string.IsNullOrEmpty(rom_Quality)) continue;

                    try
                    {
                        string[] dataParts = rom_Quality.Split('#');

                        recordId = dataParts[0].Trim();
                        block = dataParts[1].Trim();
                        rom = dataParts[2].Trim();
                        romId = dataParts[3].Trim();
                        ton = dataParts[4].Trim();

                        cv = dataParts[5].Trim();
                        ts = dataParts[6].Trim();
                        ash = dataParts[7].Trim();
                        im = dataParts[8].Trim();
                        tm = dataParts[9].Trim();
                        portion = dataParts[10].Trim();

                        // saat menyimpan ke Database, kita memastikan bahwa kita melakukan Rounding
                        cv = OurUtility.Round(cv, 0);
                        ts = OurUtility.Round(ts, 2);
                        ash = OurUtility.Round(ash, 2);
                        im = OurUtility.Round(im, 2);
                        tm = OurUtility.Round(tm, 2);
                        ton = OurUtility.Round(ton, 2);

                        sql = string.Format(@"insert into PortionBlending_Details(PortionBlending, ROMQuality_RecordId, block, ROM_Name, ROM_ID, CV, TS, ASH, IM, TM, Ton, Portion) 
                                                values({0}, {1}, '{2}', '{3}', {4}, {5}, {6}, {7}, {8}, {9}, {10}, {11})"
                                                , p_portionBlending.ToString(), recordId, block, rom, romId
                                                , cv, ts, ash, im, tm, ton, portion);

                        command.CommandText = sql;
                        command.ExecuteNonQuery();
                    }
                    catch {}
                }

                result = true;
            }
            catch {}

            return result;
        }

        private static bool Delete_Detail(SqlConnection p_db, long p_portionBlending, ref string p_message)
        {
            bool result = false;

            p_message = string.Empty;

            try
            {
                SqlCommand command = p_db.CreateCommand();
                string sql = string.Format(@"delete from PortionBlending_Details where PortionBlending = {0}", p_portionBlending);
                command.CommandText = sql;
                command.ExecuteNonQuery();

                result = true;
            }
            catch {}

            return result;
        }

        public JsonResult Optimize()
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

            List<object> result = new List<object>();

            bool success = false;
            string messageDetail = string.Empty;

            try
            {
                int portion_Sum = 0;
                int portion = DateTime.Now.Second;
                portion = (portion>30?portion-30:portion);

                string data_selected = OurUtility.ValueOf(Request, "data_selected");
                dynamic data = JsonConvert.DeserializeObject(data_selected);
                int count = (int)data.Count;
                int i = 0;
                foreach (var line in data.Lines)
                {
                    i++;

                    if (count == i)
                    {
                        var itemx = new {Key = line.Key.ToString()
                                            , ROM = line.ROM.ToString()
                                            , Portion = (100 - portion_Sum)
                                        };
                        result.Add(itemx);

                        break;
                    }

                    var item = new {Key = line.Key.ToString()
                                            , ROM = line.ROM.ToString()
                                            , Portion = portion
                                    };
                    result.Add(item);

                    portion_Sum += portion;
                    portion += 5;
                }

                success = true;
            }
            catch (Exception ex)
            {
                messageDetail = ex.Message;
                result = new List<object>();
            }

            var msg2 = new { Success = success, Result = result, MessageDetail = messageDetail, Version = Configuration.VERSION };
            return Json(msg2, JsonRequestBehavior.AllowGet);
        }
    }
}