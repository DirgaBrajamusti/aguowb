using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

using System.Data;
using System.Data.SqlClient;

using MERCY.Data.EntityFramework;
using MERCY.Web.BackEnd.Models;
using MERCY.Web.BackEnd.Helpers;
using MERCY.Web.BackEnd.Security;
using Permission = MERCY.Web.BackEnd.Security.Permission;
using MERCY.Web.BackEnd.Dto.HaulingRequest;

namespace MERCY.Web.BackEnd.Areas.Api.Controllers
{
    public class HaulingController : Controller
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
            string dateFrom = Request["dateFrom"];
            string dateTo = Request["dateTo"];

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
                                from d in db.HaulingRequests
                                where d.CreatedOn >= dateFrom_O
                                    && d.CreatedOn < dateTo_O
                                orderby d.CreatedOn
                                select new Model_View_HaulingRequest
                                {
                                    RecordId = d.RecordId
                                    , CreatedOn = d.CreatedOn
                                }
                            );

                    var items = dataQuery.ToList();

                    try
                    {
                        items.ForEach(c =>
                        {

                            var dataQuery_PortionBlending =
                                    (
                                        from d in db.HaulingRequest_Detail
                                        join p in db.PortionBlendings on d.DataId equals p.RecordId
                                        join p_detail in db.PortionBlending_Details on p.RecordId equals p_detail.PortionBlending
                                        where d.HaulingRequest == c.RecordId
                                            && d.DataType == "PortionBlending"
                                        orderby p.Company
                                        select new 
                                        {
                                            p.Company
                                            , p.Shift
                                            , p.Remark
                                            , p.NoHauling
                                            , Source = (p_detail.block + " " + p_detail.ROM_Name)
                                            , p_detail.Portion
                                            , p_detail.CV
                                            , p_detail.TS
                                            , p_detail.ASH
                                        }
                                    );

                            var items_PortionBlending = dataQuery_PortionBlending.ToList();

                            var dataQuery_ROMTransfer =
                                    (
                                        from d in db.HaulingRequest_Detail
                                        join r in db.ROMTransfers on d.DataId equals r.RecordId
                                        where d.HaulingRequest == c.RecordId
                                            && d.DataType == "ROM_Transfer"
                                        orderby r.Company
                                        select new
                                        {
                                            r.Company
                                            , r.Shift
                                            , r.Remark
                                            , Source = (r.Source_Block + " " + r.Source_ROM_Name)
                                            , Destination = (r.Destination_Block + " " + r.Destination_ROM_Name)
                                        }
                                    );

                            var items_ROMTransfer = dataQuery_ROMTransfer.ToList();

                            c.CreatedOn_Str = OurUtility.DateFormat(c.CreatedOn, "dd-MMM-yyyy");
                            c.Company = "";
                            c.Source = "";
                            c.Destination = "";
                            c.Quality = "";

                            List<string> companies = new List<string>();

                            double detail_CV = 0.0;
                            double detail_TS = 0.0;
                            double detail_ASH = 0.0;

                            // Portion Blending
                            items_PortionBlending.ForEach(c2 =>
                            {
                                try
                                {
                                    if ( ! companies.Contains(c2.Company)) companies.Add(c2.Company);
                                }
                                catch {}

                                c.Source += c2.Source + " (" + c2.Portion + "%)<br/>";
                                
                                //Kalkulasi Quality, dengan strategi seperti berikut:
                                //   - pastikan bahwa semua data selalu di Round terlebih dahulu
                                //   - baru kemudian dilakukan Kalkulasi Penjumlahan
                                detail_CV += ((OurUtility.Round(c2.CV, 2) * c2.Portion) / 100.0);
                                detail_TS += ((OurUtility.Round(c2.TS, 2) * c2.Portion) / 100.0);
                                detail_ASH += ((OurUtility.Round(c2.ASH, 2) * c2.Portion) / 100.0);
                            });
                            
                            // kembali dilakukan Rounding, agar konsisten
                            detail_CV = OurUtility.Round(detail_CV, 0);
                            detail_TS = OurUtility.Round(detail_TS, 2);
                            detail_ASH = OurUtility.Round(detail_ASH, 2);
                            
                            c.Quality = string.Format(@"CV={0:N0}<br/>TS={1:N2}<br/>ASH={2:N2}", detail_CV, detail_TS, detail_ASH);

                            // ROM_Transfer
                            items_ROMTransfer.ForEach(c3 =>
                            {
                                try
                                {
                                    if ( ! companies.Contains(c3.Company)) companies.Add(c3.Company);
                                }
                                catch {}

                                c.Source += c3.Source + "<br/>";
                                c.Destination += c3.Destination + "<br/>";
                            });

                            int i = 0;
                            foreach (string cmp in companies)
                            {
                                i++;

                                c.Company += (i>1?", ":"") + cmp;
                            }
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

        public JsonResult Create()
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

            // -- Actual code
            try
            {
                DateTime dateFrom = DateTime.Parse(Request["dateFrom"]);
                DateTime dateTo = DateTime.Parse(Request["dateTo"]);

                using (MERCY_Ctx db = new MERCY_Ctx())
                {
                    var data = new HaulingRequest
                    {
                        DateFrom = dateFrom,
                        DateTo = dateTo,

                        CreatedOn = DateTime.Now,
                        CreatedBy = user.UserId
                    };

                    db.HaulingRequests.Add(data);
                    db.SaveChanges();

                    long id = data.RecordId;

                    DateTime dateTo_O = dateTo.AddDays(1);

                    bool skip_Shift_1 = false;

                    DateTime now = DateTime.Now;

                    // simulasi Shift:
                    //  o before Shift 1
                    //  o At Shift 1
                    //  o At Shift 2
                    //now = DateTime.Parse(now.ToString("yyyy-MM-dd 06:50"));
                    //now = DateTime.Parse(now.ToString("yyyy-MM-dd 07:50"));
                    //now = DateTime.Parse(now.ToString("yyyy-MM-dd 19:50"));

                    string dateFrom_str = now.ToString("yyyy-MM-dd");
                    DateTime now_DateOnly = DateTime.Parse(dateFrom_str);

                    if (now.Hour < 7)
                    {
                        // SHIFT 2: sisa hari sebelumnya
                    }
                    else if (now.Hour < 19)
                    {
                        // saat ini: adalah Shift 1/A

                        // -- skip Shift 1
                        skip_Shift_1 = true;
                    }
                    else
                    {
                        // saat ini: adalah Shift 2/B

                        // -- ambil hari berikutnya
                        dateFrom_str = now.AddDays(1).ToString("yyyy-MM-dd");
                    }

                    DateTime dateFrom_O = DateTime.Parse(dateFrom_str);
                    if (dateFrom > dateFrom_O)
                    {
                        dateFrom_O = dateFrom;
                    }

                    // Portion Blending
                    try
                    {
                        var dataQuery_PortionBlending =
                                (
                                    from d in db.PortionBlendings
                                    where d.BlendingDate >= dateFrom_O
                                        && d.BlendingDate < dateTo_O
                                        && (
                                            ! (skip_Shift_1 && d.BlendingDate == now_DateOnly && d.Shift == 1)
                                        )
                                    select new
                                    {
                                        d.RecordId
                                    }
                                );

                        var items_PortionBlending = dataQuery_PortionBlending.ToList();

                        items_PortionBlending.ForEach(c2 =>
                        {
                            var data_Detail = new HaulingRequest_Detail
                            {
                                HaulingRequest = id,
                                DataType = "PortionBlending",
                                DataId = c2.RecordId
                            };

                            db.HaulingRequest_Detail.Add(data_Detail);
                            db.SaveChanges();
                        });
                    }
                    catch (Exception)
                    { }

                    // ROM_Transfer
                    try
                    {
                        var dataQuery_ROMTransfer =
                                (
                                    from d in db.ROMTransfers
                                    where d.TransferDate >= dateFrom_O
                                        && d.TransferDate < dateTo_O
                                        && (
                                             ! (skip_Shift_1 && d.TransferDate == now_DateOnly && d.Shift == 1)
                                        )
                                    select new
                                    {
                                        d.RecordId
                                    }
                                );

                        var items_ROMTransfer = dataQuery_ROMTransfer.ToList();

                        items_ROMTransfer.ForEach(c3 =>
                        {
                            var data_Detail = new HaulingRequest_Detail
                            {
                                HaulingRequest = id,
                                DataType = "ROM_Transfer",
                                DataId = c3.RecordId
                            };

                            db.HaulingRequest_Detail.Add(data_Detail);
                            db.SaveChanges();
                        });
                    }
                    catch (Exception)
                    { }

                    using (var connection = new SqlConnection(db.Database.Connection.ConnectionString))
                    {
                        connection.Open();

                        SqlCommand command = connection.CreateCommand();

                        string sql = string.Format(@"exec PROCESS_HAULING_REQUEST_DETAIL {0}", id);
                        command.CommandText = sql;

                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            if (reader.Read())
                            {}
                        }

                        string createdOn = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

                        // Actual_Tunnel + History of Actual_Tunnel
                        try
                        {
                            sql = string.Format(@"exec PROCESS_HAULING_REQUEST_DETAIL_Tunnel_Actual {0}, {1}", id, user.UserId);
                            command.CommandText = sql;

                            using (SqlDataReader reader = command.ExecuteReader())
                            {
                                if (reader.Read())
                                {}
                            }
                        }
                        catch {}
                    }

                    var result = new { Success = true, Permission = permission_Item, Message = BaseConstants.MESSAGE_CREATE_SUCCESS, MessageDetail = string.Empty, Version = Configuration.VERSION, Data = id };
                    return Json(result, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                var result = new { Success = false, Permission = permission_Item, ex.Message, MessageDetail = string.Empty, Version = Configuration.VERSION, Items = string.Empty, Total = 0 };
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

            // -- Actual code
            long id = OurUtility.ToInt64(Request[".id"]);
            if (id <= 0)
            {
                // -- special purpose
                // this Id is only for Checking CurrentUser:Info

                var result = new { Success = true, Permission = permission_Item, User = UserX.Information(user, is_Show_User_Menu, is_Show_User_Relation), Version = Configuration.VERSION };
                return Json(result, JsonRequestBehavior.AllowGet);
            }

            bool date_in_Table = ( ! string.IsNullOrEmpty(Request["date"]));

            string message = string.Empty;
            string content = string.Empty;
            string email_Requestor = string.Empty;
            string email_subject_Shift = string.Empty;

            DateTime dateFrom = DateTime.Now;
            DateTime dateTo = DateTime.Now;

            if (Get(id, date_in_Table, false, ref content, ref email_Requestor, ref dateFrom, ref dateTo, ref email_subject_Shift, ref message))
            {
                var result = new { Success = true, Permission = permission_Item, User = UserX.Information(user, is_Show_User_Menu, is_Show_User_Relation), Message = string.Empty, MessageDetail = string.Empty, Version = Configuration.VERSION, Content = content };
                return Json(result, JsonRequestBehavior.AllowGet);
            }
            else
            {
                var result = new { Success = false, Permission = permission_Item, User = UserX.Information(user, is_Show_User_Menu, is_Show_User_Relation), Message = message, MessageDetail = string.Empty, Version = Configuration.VERSION, Content = string.Empty };
                return Json(result, JsonRequestBehavior.AllowGet);
            }
        }

        internal bool Get(long p_id, bool date_in_Table, bool p_is_inEmail, ref string content, ref string p_email_Requestor, ref DateTime p_dateFrom, ref DateTime p_dateTo, ref string p_subject_Shift, ref string p_message, int p_site = 0)
        {
            bool result = false;

            try
            {
                using (MERCY_Ctx db = new MERCY_Ctx())
                {
                    result = Get(db, p_id, date_in_Table, p_is_inEmail, ref content, ref p_email_Requestor, ref p_dateFrom, ref p_dateTo, ref p_subject_Shift, ref p_message, p_site);
                }
            }
            catch (Exception ex)
            {
                p_message = ex.Message;
            }

            return result;
        }

        private bool Get(MERCY_Ctx db, long p_id, bool date_in_Table, bool p_is_inEmail, ref string content, ref string p_email_Requestor, ref DateTime p_dateFrom, ref DateTime p_dateTo, ref string p_subject_Shift, ref string p_message, int p_site = 0)
        {
            bool result = false;

            content = string.Empty;
            p_message = string.Empty;
            p_email_Requestor = string.Empty;
            p_subject_Shift = string.Empty;

            try
            {
                var dataQuery =
                        (
                            from d in db.HaulingRequests
                            join usr in db.Users on d.CreatedBy equals usr.UserId
                            where d.RecordId == p_id
                            select new 
                            {
                                d.RecordId
                                , d.CreatedOn
                                , d.DateFrom
                                , d.DateTo
                                , usr.Email
                            }
                        );

                var itemx = dataQuery.SingleOrDefault();
                if (itemx == null)
                {
                    p_message = "Id: " + p_id.ToString() + " is not found";
                    return result;
                }

                p_email_Requestor = itemx.Email;
                //p_dateFrom = itemx.DateFrom;
                //p_dateTo = itemx.DateTo;

                var dataQuery_PortionBlending =
                        (
                            from p in db.HaulingRequest_Detail_PortionBlending
                            join c in db.Companies on p.Company equals c.CompanyCode
                            join pb in db.PortionBlendings on p.RecordId_Snapshot equals pb.RecordId into pbs
                            from pb in pbs.DefaultIfEmpty()
                            where p.HaulingRequest == p_id && (c.SiteId == p_site || p_site == 0)
                            select new
                            {
                                p.RecordId_Snapshot
                                , p.BlendingDate
                                , p.Company
                                , p.Shift
                                , p.Remark
                                , p.NoHauling
                                , p.CV
                                , p.TS
                                , p.ASH
                                , p.Product
                                , p.Hopper
                                , p.Tunnel
                                , pb.ROM_Name
                                , pb.TotalBucket
                            }
                        );

                var items_PortionBlending = dataQuery_PortionBlending.ToList();

                var dataQuery_ROMTransfer =
                        (
                            from r in db.HaulingRequest_Detail_ROMTransfer
                            join c in db.Companies on r.Company equals c.CompanyCode
                            where r.HaulingRequest == p_id && (c.SiteId == p_site || p_site == 0)
                            select new
                            {
                                r.RecordId_Snapshot
                                , r.TransferDate
                                , r.Company
                                , r.Shift
                                , r.Remark
                                , Source = (r.Source_Block + " " + r.Source_ROM_Name)
                                , Destination = (r.Destination_Block + " " + r.Destination_ROM_Name)
                            }
                        );

                var items_ROMTransfer = dataQuery_ROMTransfer.ToList();

                List<DateTime> haulingDates = new List<DateTime>();
                List<string> companies = new List<string>();

                // Portion Blending
                items_PortionBlending.ForEach(c2 =>
                {
                    try
                    {
                        if ( ! haulingDates.Contains(c2.BlendingDate)) haulingDates.Add(c2.BlendingDate);
                        if ( ! companies.Contains(c2.Company)) companies.Add(c2.Company);
                    }
                    catch {}
                });

                // ROM_Transfer
                items_ROMTransfer.ForEach(c3 =>
                {
                    try
                    {
                        if ( ! haulingDates.Contains(c3.TransferDate)) haulingDates.Add(c3.TransferDate);
                        if ( ! companies.Contains(c3.Company)) companies.Add(c3.Company);
                    }
                    catch {}
                });

                companies.Sort();
                companies.Reverse();

                string companies_title = string.Empty;
                foreach (string company in companies)
                {
                    companies_title += (string.IsNullOrEmpty(companies_title)?"":@"/") + company;
                }

                string content_per_Date = string.Empty;
                string content_per_Shift = string.Empty;
                string dateStr = string.Empty;
                string line_ROM_Transfer = string.Empty;
                string line_PortionBlending = string.Empty;

                string cell_date = string.Empty;
                string cell_shift = string.Empty;
                string cell_company = string.Empty;

                int rowspan_Date = 0;
                int rowspan_Shift = 0;
                int rowspan_Company = 0;
                int line_color = 0;

                string style_background = string.Empty;
                string style_td_border = "border-right:1px solid #dee2e6 !important;border-bottom:1px solid #dee2e6 !important;height:40px;padding: 0px !important;";

                haulingDates.Sort();

                int i_date = 0;
                foreach (DateTime haulingDate in haulingDates)
                {
                    i_date++;

                    if (i_date <= 1)
                    {
                        p_dateFrom = haulingDate;
                    }
                    p_dateTo = haulingDate;

                    content_per_Date = string.Empty;
                    rowspan_Date = 0;

                    if (date_in_Table)
                    {
                        cell_date = string.Format(@"<td rowspan='__ROWSPAN_DATE__' style='text-align:center;vertical-align:middle;{1}'>{0}</td>", haulingDate.ToString("dd-MMM-yyyy"), style_td_border);
                    }
                    else
                    {
                        content_per_Date += string.Format(@"<div class='data_date'><span class='data_date2'>Date:</span> <span class='data_date3'>{0}</span></div>", haulingDate.ToString("dd-MMM-yyyy"));
                    }

                    for (int shift = 1; shift <= 2; shift++)
                    {
                        content_per_Shift = string.Empty;
                        rowspan_Shift = 0;

                        if (date_in_Table)
                        {
                            cell_shift = string.Format(@"<td rowspan='__ROWSPAN_SHIFT__' style='text-align:center;vertical-align:middle;font-size:20px !important;font-weight:bold !important;{1}'>{0}</td>", (shift==1?"A":"B"), style_td_border);
                            line_color = 0;
                        }

                        foreach (string company in companies)
                        {
                            // reset
                            rowspan_Company = 0;

                            // about line
                            line_color++;

                            line_ROM_Transfer = string.Empty;
                            line_PortionBlending = string.Empty;

                            if ((line_color % 2) == 0)
                            {
                                style_background = @"background-color:#ffffff !important;";
                            }
                            else
                            {
                                style_background = @"background-color:#EBF1DE !important;";
                            }

                            var dataQuery_R =
                                (
                                    from d in items_ROMTransfer
                                    where d.TransferDate == haulingDate
                                        && d.Shift == shift
                                        && d.Company == company
                                    orderby d.Source, d.Destination
                                    select d
                                );

                            cell_company = "__COMPANY_ROM_TRANSFER__";

                            var item_Rs = dataQuery_R.ToList();
                            item_Rs.ForEach(item_R =>
                            {
                                rowspan_Date++;
                                rowspan_Shift++;
                                rowspan_Company++;
                               
                                line_ROM_Transfer += string.Format(@"
                                                                    <tr>{4}{5}
                                                                        {0}
                                                                        <td style='padding:0px 0px 0px 50px !important;vertical-align:middle;{6}{7};padding-left:30px !important;'>{1}</td>
                                                                        <td colspan='3' style='text-align:center;vertical-align:middle;{6}{7}'>{2}</td>
                                                                        <td style='{6}{7}'></td>
                                                                        <td style='text-align:center;vertical-align:middle;{6}{7}'>{3}</td>
                                                                    </tr>
                                                                    ", cell_company, item_R.Source, item_R.Destination, OurUtility.Handle_Enter(item_R.Remark)
                                                                    , cell_date, cell_shift, style_background, style_td_border);

                                cell_date = string.Empty;
                                cell_shift = string.Empty;
                                cell_company = string.Empty;
                            });

                            var dataQuery_P =
                                (
                                    from d in items_PortionBlending
                                    where d.BlendingDate == haulingDate
                                        && d.Shift == shift
                                        && d.Company == company
                                    select d
                                );

                            cell_company = "__COMPANY_PORTION_BLENDING__";

                            var item_Ps = dataQuery_P.ToList();
                            item_Ps.ForEach(item_P =>
                            {
                                rowspan_Date++;
                                rowspan_Shift++;
                                rowspan_Company++;

                                if (item_P.NoHauling)
                                {
                                    line_PortionBlending += string.Format(@"
                                                                        <tr>{2}{3}
                                                                            {0}
                                                                            <td colspan='6' style='text-align:center;vertical-align:middle;font-size:14px !important;font-weight:bold !important;{4}{5}'>{1}</td>
                                                                        </tr>
                                                                        ", cell_company, OurUtility.Handle_Enter(item_P.Remark)
                                                                        , cell_date, cell_shift, style_background, style_td_border);

                                    cell_date = string.Empty;
                                    cell_shift = string.Empty;
                                    cell_company = string.Empty;
                                }
                                else
                                {
                                    string source = string.Empty;

                                    double detail_CV = 0.0;
                                    double detail_TS = 0.0;
                                    double detail_ASH = 0.0;

                                    var dataQuery_Source_Portion =
                                    (
                                        from p_detail in db.HaulingRequest_Detail_PortionBlending_Details
                                        where p_detail.PortionBlending == item_P.RecordId_Snapshot
                                            && p_detail.HaulingRequest == p_id
                                        orderby p_detail.block, p_detail.ROM_Name
                                        select p_detail
                                    );

                                    string portion_Percentage = string.Empty;

                                    var items_Source_Portion = dataQuery_Source_Portion.ToList();
                                    items_Source_Portion.ForEach(c4 =>
                                    {
                                        if (p_is_inEmail)
                                        {
                                            if (c4.Portion == 0)
                                            {
                                                portion_Percentage = string.Empty;
                                            }
                                            else
                                            {
                                                portion_Percentage = string.Format("({0} {1})", c4.Portion, @"%");
                                            }
                                        }
                                        else
                                        {
                                            portion_Percentage = string.Format("({0} {1})", c4.Portion, @"%");
                                        }
                                        
                                        source += string.Format(@"<tr><td style='vertical-align:middle;padding:0px 0px 0px 50px !important;{3}{4};padding-left:30px !important;'>{0} {1} {2}</td></tr>", c4.block, c4.ROM_Name, portion_Percentage, style_background, style_td_border);

                                        //Kalkulasi Quality, dengan strategi seperti berikut:
                                        //   - pastikan bahwa semua data selalu di Round terlebih dahulu
                                        //   - baru kemudian dilakukan Kalkulasi Penjumlahan
                                        detail_CV += ((OurUtility.Round(c4.CV, 2) * c4.Portion) / 100.0);
                                        detail_TS += ((OurUtility.Round(c4.TS, 2) * c4.Portion) / 100.0);
                                        detail_ASH += ((OurUtility.Round(c4.ASH, 2) * c4.Portion) / 100.0);
                                    });
                                    
                                    // kembali dilakukan Rounding, agar konsisten
                                    detail_CV = OurUtility.Round(detail_CV, 0);
                                    detail_TS = OurUtility.Round(detail_TS, 2);
                                    detail_ASH = OurUtility.Round(detail_ASH, 2);
                            
                                    string quality = string.Format(@"CV={0:N0}<br/>TS={1:N2}<br/>ASH={2:N2}", detail_CV, detail_TS, detail_ASH);

                                    var additionalBucketContent = string.Empty;
                                    if (item_P.TotalBucket != null)
                                    {
                                        additionalBucketContent = string.Format(@"<p style='margin-top:8px;'>Add {0} bucket from {1} into each unit SDT</p>", item_P.TotalBucket, item_P.ROM_Name);
                                    }

                                    line_PortionBlending = string.Format(@"
                                                                        <tr>{7}{8}
                                                                            {0}
                                                                            <td style='padding:0px !important;{9}'><table style='width:100%;height:100%;{10}'>{1}</table></td>
                                                                            <td style='text-align:center;vertical-align:middle;{9}{10}'>{2}</td>
                                                                            <td style='text-align:center;vertical-align:middle;{9}{10}'>{3}</td>
                                                                            <td style='text-align:center;vertical-align:middle;{9}{10}'>{4}</td>
                                                                            <td style='text-align:center;vertical-align:middle;{9}{10}'>{5}</td>
                                                                            <td style='text-align:center;vertical-align:middle;{9}{10}'>{6} {11}</td>
                                                                        </tr>
                                                                        ", cell_company, source, item_P.Hopper, item_P.Tunnel, item_P.Product, quality, OurUtility.Handle_Enter(item_P.Remark)
                                                                        , cell_date, cell_shift, style_background, style_td_border, additionalBucketContent);

                                    cell_date = string.Empty;
                                    cell_shift = string.Empty;
                                    cell_company = string.Empty;
                                }
                            });

                            if (( ! string.IsNullOrEmpty(line_ROM_Transfer))
                                && ( ! string.IsNullOrEmpty(line_PortionBlending)))
                            {
                                line_ROM_Transfer = line_ROM_Transfer.Replace("__COMPANY_ROM_TRANSFER__", "<td rowspan='" + rowspan_Company.ToString() + "' style='text-align:center;vertical-align:middle;font-size:14px !important;font-weight:bold !important;" + style_background + style_td_border + "'>" + company + "</td>");
                                line_PortionBlending = line_PortionBlending.Replace("__COMPANY_PORTION_BLENDING__", "");
                            }
                            else
                            {
                                if ( ! string.IsNullOrEmpty(line_ROM_Transfer))
                                {
                                    line_ROM_Transfer = line_ROM_Transfer.Replace("__COMPANY_ROM_TRANSFER__", "<td rowspan='" + rowspan_Company.ToString() + "' style='text-align:center;vertical-align:middle;font-size:14px !important;font-weight:bold !important;" + style_background + style_td_border + "'>" + company + "</td>");
                                    line_PortionBlending = line_PortionBlending.Replace("__COMPANY_PORTION_BLENDING__", "");
                                }
                                else
                                {
                                    line_ROM_Transfer = line_ROM_Transfer.Replace("__COMPANY_ROM_TRANSFER__", "");
                                    line_PortionBlending = line_PortionBlending.Replace("__COMPANY_PORTION_BLENDING__", "<td rowspan='" + rowspan_Company.ToString() + "' style='text-align:center;vertical-align:middle;font-size:14px !important;font-weight:bold !important;" + style_background + style_td_border + "'>" + company + "</td>");
                                }
                            }

                            content_per_Shift += line_ROM_Transfer + line_PortionBlending;
                        }

                        if (i_date <= 1)
                        {
                            if (shift == 1)
                            {
                                if (string.IsNullOrEmpty(content_per_Shift)) p_subject_Shift = " (Shift B) ";
                            }
                        }

                        if (string.IsNullOrEmpty(content_per_Shift)) continue;

                        content_per_Shift = content_per_Shift.Replace("__ROWSPAN_SHIFT__", rowspan_Shift.ToString());

                        if (date_in_Table)
                        {
                            content_per_Date += content_per_Shift;
                        }
                        else
                        {
                            content_per_Date += string.Format(@"
                                                        <div class='data_shift'><span>Shift:</span> <span>{0}</span></div>
                                                        <table style='width:100%;margin: 10px 0px !important;border-left:1px solid #dee2e6 !important;border-top:1px solid #dee2e6 !important;'>
                                                            <tr>
                                                                <td rowspan='2' style='text-align:center;vertical-align:middle;width:100px;background-color:#DAEEF3;font-size:14px !important;font-weight:bold !important;{2}'>Company</td>
                                                                <td rowspan='2' style='text-align:center;vertical-align:middle;min-width:300px;background-color:#DAEEF3;font-size:14px !important;font-weight:bold !important;{2}'>Source</td>
                                                                <td colspan='3' style='text-align:center;vertical-align:middle;background-color:#DAEEF3;font-size:14px !important;font-weight:bold !important;{2}'>Destination</td>
                                                                <td rowspan='2' style='text-align:center;vertical-align:middle;width:130px;background-color:#DAEEF3;font-size:14px !important;font-weight:bold !important;{2}'>Quality</td>
                                                                <td rowspan='2' style='text-align:center;vertical-align:middle;width:210px;background-color:#DAEEF3;font-size:14px !important;font-weight:bold !important;{2}'>Remark</td>
                                                            </tr>
                                                            <tr>
                                                                <td             style='text-align:center;vertical-align:middle;width:170px;background-color:#DAEEF3;font-size:14px !important;font-weight:bold !important;{2}'>Hopper</td>
                                                                <td             style='text-align:center;vertical-align:middle;width:70px;background-color:#DAEEF3;font-size:14px !important;font-weight:bold !important;{2}'>Tunnel</td>
                                                                <td             style='text-align:center;vertical-align:middle;width:80px;background-color:#DAEEF3;font-size:14px !important;font-weight:bold !important;{2}'>Product</td>
                                                            </tr>
                                                            {1}
                                                        </table>
                                                        ", shift, content_per_Shift, style_td_border);
                        }
                    }

                    content += content_per_Date.Replace("__ROWSPAN_DATE__", rowspan_Date.ToString());
                }

                if (date_in_Table)
                {
                    content = string.Format(@"
                                                        <table style='width:100%;border-left:1px solid #dee2e6 !important;border-top:1px solid #dee2e6 !important;margin-bottom:50px;'>
                                                            <tr>
                                                                <td colspan='9' style='text-align:center;vertical-align:middle;background-color:#00B050 !important;font-size:24px !important;color: #ffffff !important;font-weight:bold !important;font-style: italic !important;{2}'>REQUEST HAULING FC PRODUCTION {1}</td>
                                                            </tr>
                                                            <tr>
                                                                <td rowspan='2' style='text-align:center;vertical-align:middle;width:110px;background-color:#DAEEF3;font-size:14px !important;font-weight:bold !important;{2}'>Date</td>
                                                                <td rowspan='2' style='text-align:center;vertical-align:middle;width:100px;background-color:#DAEEF3;font-size:14px !important;font-weight:bold !important;{2}'>Shift</td>
                                                                <td rowspan='2' style='text-align:center;vertical-align:middle;width:100px;background-color:#DAEEF3;font-size:14px !important;font-weight:bold !important;{2}'>Company</td>
                                                                <td rowspan='2' style='text-align:center;vertical-align:middle;min-width:300px;background-color:#DAEEF3;font-size:14px !important;font-weight:bold !important;{2}'>Source</td>
                                                                <td colspan='3' style='text-align:center;vertical-align:middle;width:100px;background-color:#DAEEF3;font-size:14px !important;font-weight:bold !important;{2}'>Destination</td>
                                                                <td rowspan='2' style='text-align:center;vertical-align:middle;width:130px;background-color:#DAEEF3;font-size:14px !important;font-weight:bold !important;{2}'>Quality</td>
                                                                <td rowspan='2' style='text-align:center;vertical-align:middle;width:210px;background-color:#DAEEF3;font-size:14px !important;font-weight:bold !important;{2}'>Remark</td>
                                                            </tr>
                                                            <tr>
                                                                <td             style='text-align:center;vertical-align:middle;width:170px;background-color:#DAEEF3;font-size:14px !important;font-weight:bold !important;{2}'>Hopper</td>
                                                                <td             style='text-align:center;vertical-align:middle;width:70px;background-color:#DAEEF3;font-size:14px !important;font-weight:bold !important;{2}'>Tunnel</td>
                                                                <td             style='text-align:center;vertical-align:middle;width:80px;background-color:#DAEEF3;font-size:14px !important;font-weight:bold !important;{2}'>Product</td>
                                                            </tr>
                                                            {0}
                                                        </table>
                                                        ", content, companies_title, style_td_border);
                }

                result = true;
            }
            catch (Exception ex)
            {
                content = string.Empty;
                p_message = ex.Message;
            }

            return result;
        }

        public JsonResult Preview()
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

            DateTime dateFrom = DateTime.Parse(Request["dateFrom"]);
            DateTime dateTo = DateTime.Parse(Request["dateTo"]);

            int siteId = OurUtility.ToInt32(Request["siteId"]);
            bool date_in_Table = ( ! string.IsNullOrEmpty(Request["date"]));

            string message = string.Empty;
            string content = string.Empty;

            if (Get(dateFrom, dateTo, date_in_Table, ref content, ref message, siteId))
            {
                var result = new { Success = true, Permission = permission_Item, User = UserX.Information(user, is_Show_User_Menu, is_Show_User_Relation), Message = string.Empty, MessageDetail = string.Empty, Version = Configuration.VERSION, Content = content };
                return Json(result, JsonRequestBehavior.AllowGet);
            }
            else
            {
                var result = new { Success = false, Permission = permission_Item, User = UserX.Information(user, is_Show_User_Menu, is_Show_User_Relation), Message = message, MessageDetail = string.Empty, Version = Configuration.VERSION, Content = string.Empty };
                return Json(result, JsonRequestBehavior.AllowGet);
            }
        }

        internal bool Get(DateTime dateFrom, DateTime dateTo, bool date_in_Table, ref string content, ref string p_message, int site=0)
        {
            bool result = false;

            try
            {
                using (MERCY_Ctx db = new MERCY_Ctx())
                {
                    result = Get(db, dateFrom, dateTo, date_in_Table, ref content, ref p_message, site);
                }
            }
            catch (Exception ex)
            {
                p_message = ex.Message;
            }

            return result;
        }

        private bool Get(MERCY_Ctx db, DateTime dateFrom, DateTime dateTo, bool date_in_Table, ref string content, ref string p_message, int site = 0)
        {
            bool result = false;

            content = string.Empty;
            p_message = string.Empty;

            try
            {
                DateTime dateTo_O = dateTo.AddDays(1);

                bool skip_Shift_1 = false;

                DateTime now = DateTime.Now;

                // simulasi Shift:
                //  o before Shift 1
                //  o At Shift 1
                //  o At Shift 2
                //now = DateTime.Parse(now.ToString("yyyy-MM-dd 06:50"));
                //now = DateTime.Parse(now.ToString("yyyy-MM-dd 07:50"));
                //now = DateTime.Parse(now.ToString("yyyy-MM-dd 19:50"));
                
                string dateFrom_str = now.ToString("yyyy-MM-dd");
                DateTime now_DateOnly = DateTime.Parse(dateFrom_str);

                if (now.Hour < 7)
                {
                    // SHIFT 2: sisa hari sebelumnya
                }
                else if (now.Hour < 19)
                {
                    // saat ini: adalah Shift 1/A

                    // -- skip Shift 1
                    skip_Shift_1 = true;
                }
                else
                {
                    // saat ini: adalah Shift 2/B

                    // -- ambil hari berikutnya
                    dateFrom_str = now.AddDays(1).ToString("yyyy-MM-dd");
                }
                
                DateTime dateFrom_O = DateTime.Parse(dateFrom_str);
                if (dateFrom > dateFrom_O)
                {
                    dateFrom_O = dateFrom;
                }

                var dataQuery_PortionBlending =
                        (
                            from p in db.PortionBlendings
                            join c in db.Companies on p.Company equals c.CompanyCode
                            where p.BlendingDate >= dateFrom_O
                                && p.BlendingDate < dateTo_O
                                && (
                                    ! (skip_Shift_1 && p.BlendingDate == now_DateOnly && p.Shift == 1)
                                ) && (c.SiteId == site || site == 0)
                            select p
                        );

                var items_PortionBlending = dataQuery_PortionBlending.ToList();

                var dataQuery_ROMTransfer =
                        (
                            from r in db.ROMTransfers
                            join c in db.Companies on r.Company equals c.CompanyCode
                            where r.TransferDate >= dateFrom_O
                                && r.TransferDate < dateTo_O
                                && (
                                     ! (skip_Shift_1 && r.TransferDate == now_DateOnly && r.Shift == 1)
                                ) && (c.SiteId == site || site == 0)
                            select new
                            {
                                r.RecordId
                                , r.TransferDate
                                , r.Company
                                , r.Shift
                                , r.Remark
                                , Source = (r.Source_Block + " " + r.Source_ROM_Name)
                                , Destination = (r.Destination_Block + " " + r.Destination_ROM_Name)
                            }
                        );

                var items_ROMTransfer = dataQuery_ROMTransfer.ToList();

                List<DateTime> haulingDates = new List<DateTime>();
                List<string> companies = new List<string>();

                // Portion Blending
                items_PortionBlending.ForEach(c2 =>
                {
                    try
                    {
                        if ( ! haulingDates.Contains(c2.BlendingDate)) haulingDates.Add(c2.BlendingDate);
                        if ( ! companies.Contains(c2.Company)) companies.Add(c2.Company);
                    }
                    catch {}
                });

                // ROM_Transfer
                items_ROMTransfer.ForEach(c3 =>
                {
                    try
                    {
                        if ( ! haulingDates.Contains(c3.TransferDate)) haulingDates.Add(c3.TransferDate);
                        if ( ! companies.Contains(c3.Company)) companies.Add(c3.Company);
                    }
                    catch {}
                });

                companies.Sort();
                companies.Reverse();

                string companies_title = string.Empty;
                foreach (string company in companies)
                {
                    companies_title += (string.IsNullOrEmpty(companies_title)?"":@"/") + company;
                }

                string content_per_Date = string.Empty;
                string content_per_Shift = string.Empty;
                string dateStr = string.Empty;
                string line_ROM_Transfer = string.Empty;
                string line_PortionBlending = string.Empty;

                string cell_date = string.Empty;
                string cell_shift = string.Empty;
                string cell_company = string.Empty;

                int rowspan_Date = 0;
                int rowspan_Shift = 0;
                int rowspan_Company = 0;
                int line_color = 0;

                string style_background = string.Empty;
                string style_td_border = "border-right:1px solid #dee2e6 !important;border-bottom:1px solid #dee2e6 !important;height:40px;padding: 0px !important;";

                haulingDates.Sort();

                foreach (DateTime haulingDate in haulingDates)
                {
                    content_per_Date = string.Empty;
                    rowspan_Date = 0;

                    if (date_in_Table)
                    {
                        cell_date = string.Format(@"<td rowspan='__ROWSPAN_DATE__' style='text-align:center;vertical-align:middle;{1}'>{0}</td>", haulingDate.ToString("dd-MMM-yyyy"), style_td_border);
                    }
                    else
                    {
                        content_per_Date += string.Format(@"<div class='data_date'><span class='data_date2'>Date:</span> <span class='data_date3'>{0}</span></div>", haulingDate.ToString("dd-MMM-yyyy"));
                    }

                    for (int shift = 1; shift <= 2; shift++)
                    {
                        content_per_Shift = string.Empty;
                        rowspan_Shift = 0;

                        if (date_in_Table)
                        {
                            cell_shift = string.Format(@"<td rowspan='__ROWSPAN_SHIFT__' style='text-align:center;vertical-align:middle;font-size:20px !important;font-weight:bold !important;{1}'>{0}</td>", (shift==1?"A":"B"), style_td_border);
                            line_color = 0;
                        }

                        foreach (string company in companies)
                        {
                            // reset
                            rowspan_Company = 0;

                            // about line
                            line_color++;

                            line_ROM_Transfer = string.Empty;
                            line_PortionBlending = string.Empty;

                            if ((line_color % 2) == 0)
                            {
                                style_background = @"background-color:#ffffff !important;";
                            }
                            else
                            {
                                style_background = @"background-color:#EBF1DE !important;";
                            }

                            var dataQuery_R =
                                (
                                    from d in items_ROMTransfer
                                    where d.TransferDate == haulingDate
                                        && d.Shift == shift
                                        && d.Company == company
                                    orderby d.Source, d.Destination
                                    select d
                                );

                            cell_company = "__COMPANY_ROM_TRANSFER__";

                            var item_Rs = dataQuery_R.ToList();
                            item_Rs.ForEach(item_R =>
                            {
                                rowspan_Date++;
                                rowspan_Shift++;
                                rowspan_Company++;

                                line_ROM_Transfer += string.Format(@"
                                                                    <tr>{4}{5}
                                                                        {0}
                                                                        <td style='padding:0px 0px 0px 50px !important;vertical-align:middle;{6}{7};padding-left:30px !important;'>{1}</td>
                                                                        <td colspan='3' style='text-align:center;vertical-align:middle;{6}{7}'>{2}</td>
                                                                        <td style='{6}{7}'></td>
                                                                        <td style='text-align:center;vertical-align:middle;{6}{7}'>{3}</td>
                                                                    </tr>
                                                                    ", cell_company, item_R.Source, item_R.Destination, OurUtility.Handle_Enter(item_R.Remark)
                                                                    , cell_date, cell_shift, style_background, style_td_border);

                                cell_date = string.Empty;
                                cell_shift = string.Empty;
                                cell_company = string.Empty;
                            });

                            var dataQuery_P =
                                (
                                    from d in items_PortionBlending
                                    where d.BlendingDate == haulingDate
                                        && d.Shift == shift
                                        && d.Company == company
                                    select d
                                );

                            cell_company = "__COMPANY_PORTION_BLENDING__";

                            var item_Ps = dataQuery_P.ToList();
                            item_Ps.ForEach(item_P =>
                            {
                                rowspan_Date++;
                                rowspan_Shift++;
                                rowspan_Company++;

                                if (item_P.NoHauling)
                                {
                                    line_PortionBlending += string.Format(@"
                                                                        <tr>{2}{3}
                                                                            {0}
                                                                            <td colspan='6' style='text-align:center;vertical-align:middle;font-size:14px !important;font-weight:bold !important;{4}{5}'>{1}</td>
                                                                        </tr>
                                                                        ", cell_company, OurUtility.Handle_Enter(item_P.Remark)
                                                                        , cell_date, cell_shift, style_background, style_td_border);

                                    cell_date = string.Empty;
                                    cell_shift = string.Empty;
                                    cell_company = string.Empty;
                                }
                                else
                                {
                                    string source = string.Empty;

                                    double detail_CV = 0.0;
                                    double detail_TS = 0.0;
                                    double detail_ASH = 0.0;

                                    var dataQuery_Source_Portion =
                                    (
                                        from p_detail in db.PortionBlending_Details
                                        where p_detail.PortionBlending == item_P.RecordId
                                        orderby p_detail.block, p_detail.ROM_Name
                                        select p_detail
                                    );

                                    var items_Source_Portion = dataQuery_Source_Portion.ToList();
                                    items_Source_Portion.ForEach(c4 =>
                                    {
                                        source += string.Format(@"<tr><td style='vertical-align:middle;padding:0px 0px 0px 50px !important;{4}{5};padding-left:30px !important;'>{0} {1} ({2} {3})</td></tr>", c4.block, c4.ROM_Name, c4.Portion, @"%", style_background, style_td_border);

                                        //Kalkulasi Quality, dengan strategi seperti berikut:
                                        //   - pastikan bahwa semua data selalu di Round terlebih dahulu
                                        //   - baru kemudian dilakukan Kalkulasi Penjumlahan
                                        detail_CV += ((OurUtility.Round(c4.CV, 2) * c4.Portion) / 100.0);
                                        detail_TS += ((OurUtility.Round(c4.TS, 2) * c4.Portion) / 100.0);
                                        detail_ASH += ((OurUtility.Round(c4.ASH, 2) * c4.Portion) / 100.0);
                                    });

                                    // kembali dilakukan Rounding, agar konsisten
                                    detail_CV = OurUtility.Round(detail_CV, 0);
                                    detail_TS = OurUtility.Round(detail_TS, 2);
                                    detail_ASH = OurUtility.Round(detail_ASH, 2);
                                    
                                    string quality = string.Format(@"CV={0:N0}<br/>TS={1:N2}<br/>ASH={2:N2}", detail_CV, detail_TS, detail_ASH);
                                    var additionalBucketContent = string.Empty;
                                    if (item_P.TotalBucket != null)
                                    {
                                        additionalBucketContent = string.Format(@"<p style='margin-top:8px;'>Add {0} bucket from {1} into each unit SDT</p>", item_P.TotalBucket, item_P.ROM_Name);
                                    }

                                    line_PortionBlending = string.Format(@"
                                                                        <tr>{7}{8}
                                                                            {0}
                                                                            <td style='padding:0px !important;{9}'><table style='width:100%;height:100%;{10}'>{1}</table></td>
                                                                            <td style='text-align:center;vertical-align:middle;{9}{10}'>{2}</td>
                                                                            <td style='text-align:center;vertical-align:middle;{9}{10}'>{3}</td>
                                                                            <td style='text-align:center;vertical-align:middle;{9}{10}'>{4}</td>
                                                                            <td style='text-align:center;vertical-align:middle;{9}{10}'>{5}</td>
                                                                            <td style='text-align:center;vertical-align:middle;{9}{10}'>{6} {11}</td>
                                                                        </tr>
                                                                        ", cell_company, source, item_P.Hopper, item_P.Tunnel, item_P.Product, quality, OurUtility.Handle_Enter(item_P.Remark)
                                                                        , cell_date, cell_shift, style_background, style_td_border, additionalBucketContent);

                                    cell_date = string.Empty;
                                    cell_shift = string.Empty;
                                    cell_company = string.Empty;
                                }
                            });

                            if (( ! string.IsNullOrEmpty(line_ROM_Transfer))
                                && ( ! string.IsNullOrEmpty(line_PortionBlending)))
                            {
                                line_ROM_Transfer = line_ROM_Transfer.Replace("__COMPANY_ROM_TRANSFER__", "<td rowspan='" + rowspan_Company.ToString() + "' style='text-align:center;vertical-align:middle;font-size:14px !important;font-weight:bold !important;" + style_background + style_td_border + "'>" + company + "</td>");
                                line_PortionBlending = line_PortionBlending.Replace("__COMPANY_PORTION_BLENDING__", "");
                            }
                            else
                            {
                                if ( ! string.IsNullOrEmpty(line_ROM_Transfer))
                                {
                                    line_ROM_Transfer = line_ROM_Transfer.Replace("__COMPANY_ROM_TRANSFER__", "<td rowspan='" + rowspan_Company.ToString() + "' style='text-align:center;vertical-align:middle;font-size:14px !important;font-weight:bold !important;" + style_background + style_td_border + "'>" + company + "</td>");
                                    line_PortionBlending = line_PortionBlending.Replace("__COMPANY_PORTION_BLENDING__", "");
                                }
                                else
                                {
                                    line_ROM_Transfer = line_ROM_Transfer.Replace("__COMPANY_ROM_TRANSFER__", "");
                                    line_PortionBlending = line_PortionBlending.Replace("__COMPANY_PORTION_BLENDING__", "<td rowspan='" + rowspan_Company.ToString() + "' style='text-align:center;vertical-align:middle;font-size:14px !important;font-weight:bold !important;" + style_background + style_td_border + "'>" + company + "</td>");
                                }
                            }

                            content_per_Shift += line_ROM_Transfer + line_PortionBlending;
                        }

                        if (string.IsNullOrEmpty(content_per_Shift)) continue;

                        content_per_Shift = content_per_Shift.Replace("__ROWSPAN_SHIFT__", rowspan_Shift.ToString());

                        if (date_in_Table)
                        {
                            content_per_Date += content_per_Shift;
                        }
                        else
                        {
                            content_per_Date += string.Format(@"
                                                        <div class='data_shift'><span>Shift:</span> <span>{0}</span></div>
                                                        <table style='width:100%;margin: 10px 0px !important;border-left:1px solid #dee2e6 !important;border-top:1px solid #dee2e6 !important;'>
                                                            <tr>
                                                                <td rowspan='2' style='text-align:center;vertical-align:middle;width:100px;background-color:#DAEEF3;font-size:14px !important;font-weight:bold !important;{2}'>Company</td>
                                                                <td rowspan='2' style='text-align:center;vertical-align:middle;min-width:300px;background-color:#DAEEF3;font-size:14px !important;font-weight:bold !important;{2}'>Source</td>
                                                                <td colspan='3' style='text-align:center;vertical-align:middle;background-color:#DAEEF3;font-size:14px !important;font-weight:bold !important;{2}'>Destination</td>
                                                                <td rowspan='2' style='text-align:center;vertical-align:middle;width:130px;background-color:#DAEEF3;font-size:14px !important;font-weight:bold !important;{2}'>Quality</td>
                                                                <td rowspan='2' style='text-align:center;vertical-align:middle;width:210px;background-color:#DAEEF3;font-size:14px !important;font-weight:bold !important;{2}'>Remark</td>
                                                            </tr>
                                                            <tr>
                                                                <td             style='text-align:center;vertical-align:middle;width:170px;background-color:#DAEEF3;font-size:14px !important;font-weight:bold !important;{2}'>Hopper</td>
                                                                <td             style='text-align:center;vertical-align:middle;width:70px;background-color:#DAEEF3;font-size:14px !important;font-weight:bold !important;{2}'>Tunnel</td>
                                                                <td             style='text-align:center;vertical-align:middle;width:80px;background-color:#DAEEF3;font-size:14px !important;font-weight:bold !important;{2}'>Product</td>
                                                            </tr>
                                                            {1}
                                                        </table>
                                                        ", shift, content_per_Shift, style_td_border);
                        }
                    }

                    content += content_per_Date.Replace("__ROWSPAN_DATE__", rowspan_Date.ToString());
                }

                if (date_in_Table)
                {
                    content = string.Format(@"
                                                        <table style='width:100%;border-left:1px solid #dee2e6 !important;border-top:1px solid #dee2e6 !important;margin-bottom:50px;'>
                                                            <tr>
                                                                <td colspan='9' style='text-align:center;vertical-align:middle;background-color:#00B050 !important;font-size:24px !important;color: #ffffff !important;font-weight:bold !important;font-style: italic !important;{2}'>REQUEST HAULING FC PRODUCTION {1}</td>
                                                            </tr>
                                                            <tr>
                                                                <td rowspan='2' style='text-align:center;vertical-align:middle;width:110px;background-color:#DAEEF3;font-size:14px !important;font-weight:bold !important;{2}'>Date</td>
                                                                <td rowspan='2' style='text-align:center;vertical-align:middle;width:100px;background-color:#DAEEF3;font-size:14px !important;font-weight:bold !important;{2}'>Shift</td>
                                                                <td rowspan='2' style='text-align:center;vertical-align:middle;width:100px;background-color:#DAEEF3;font-size:14px !important;font-weight:bold !important;{2}'>Company</td>
                                                                <td rowspan='2' style='text-align:center;vertical-align:middle;min-width:300px;background-color:#DAEEF3;font-size:14px !important;font-weight:bold !important;{2}'>Source</td>
                                                                <td colspan='3' style='text-align:center;vertical-align:middle;width:100px;background-color:#DAEEF3;font-size:14px !important;font-weight:bold !important;{2}'>Destination</td>
                                                                <td rowspan='2' style='text-align:center;vertical-align:middle;width:130px;background-color:#DAEEF3;font-size:14px !important;font-weight:bold !important;{2}'>Quality</td>
                                                                <td rowspan='2' style='text-align:center;vertical-align:middle;width:210px;background-color:#DAEEF3;font-size:14px !important;font-weight:bold !important;{2}'>Remark</td>
                                                            </tr>
                                                            <tr>
                                                                <td             style='text-align:center;vertical-align:middle;width:170px;background-color:#DAEEF3;font-size:14px !important;font-weight:bold !important;{2}'>Hopper</td>
                                                                <td             style='text-align:center;vertical-align:middle;width:70px;background-color:#DAEEF3;font-size:14px !important;font-weight:bold !important;{2}'>Tunnel</td>
                                                                <td             style='text-align:center;vertical-align:middle;width:80px;background-color:#DAEEF3;font-size:14px !important;font-weight:bold !important;{2}'>Product</td>
                                                            </tr>
                                                            {0}
                                                        </table>
                                                        ", content, companies_title, style_td_border);
                }

                result = true;
            }
            catch (Exception ex)
            {
                content = string.Empty;
                p_message = ex.Message;
            }

            return result;
        }

        public JsonResult Edit(FormCollection p_collection)
        {
            return null;
        }
        
        public JsonResult GetAll()
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

            string content = string.Empty;
            string p_message = string.Empty;
            string p_email_Requestor = string.Empty;

            try
            {
                DateTime dateFrom = DateTime.Parse(Request["dateFrom"]);
                DateTime dateTo = DateTime.Parse(Request["dateTo"]);
                DateTime dateTo_O = dateTo.AddDays(1);

                int siteId = OurUtility.ToInt32(Request["siteId"]);

                string style_background = string.Empty;
                string style_td_border = "border-right:1px solid #dee2e6 !important;border-bottom:1px solid #dee2e6 !important;height:40px;padding: 0px !important;";

                using (MERCY_Ctx db = new MERCY_Ctx())
                {
                    var dataQuery_Hauling =
                            (
                                from d in db.HaulingRequests
                                join usr in db.Users on d.CreatedBy equals usr.UserId
                                where d.CreatedOn >= dateFrom
                                    && d.CreatedOn < dateTo_O
                                orderby d.CreatedOn descending 
                                select new 
                                {
                                    d.RecordId
                                    , d.CreatedOn
                                    , usr.FullName
                                }
                            );
                    
                    var items_Hauling = dataQuery_Hauling.ToList();

                    string content_per_Hauling = string.Empty;
                    int rowspan_1_Hauling = 0;
                    string cell_Hauling = string.Empty;

                    items_Hauling.ForEach(hauling =>
                    {
                        content_per_Hauling = string.Empty;
                        rowspan_1_Hauling = 0;

                        cell_Hauling = string.Format(@"<td rowspan='__ROWSPAN_HAULING__' style='text-align:center;vertical-align:middle;{1}'><a href='/Haulingv/Form?.id={2}'>{0}</a> ({3})</td>", hauling.CreatedOn.ToString("dd-MMM-yyyy HH:mm:ss"), style_td_border, hauling.RecordId, hauling.FullName);

                        var dataQuery_PortionBlending =
                                (
                                    from p in db.HaulingRequest_Detail_PortionBlending
                                    join c in db.Companies on p.Company equals c.CompanyCode
                                    join pb in db.PortionBlendings on p.RecordId_Snapshot equals pb.RecordId into pbs
                                    from pb in pbs.DefaultIfEmpty()
                                    where p.HaulingRequest == hauling.RecordId && c.SiteId == siteId
                                    select new
                                    {
                                        p.RecordId_Snapshot
                                        , p.BlendingDate
                                        , p.Company
                                        , p.Shift
                                        , p.Remark
                                        , p.NoHauling
                                        , p.CV
                                        , p.TS
                                        , p.ASH
                                        , p.Product
                                        , p.Hopper
                                        , p.Tunnel
                                        , pb.ROM_Name
                                        , pb.TotalBucket
                                    }
                                );

                        var items_PortionBlending = dataQuery_PortionBlending.ToList();

                        var dataQuery_ROMTransfer =
                                (
                                    from r in db.HaulingRequest_Detail_ROMTransfer
                                    join c in db.Companies on r.Company equals c.CompanyCode
                                    where r.HaulingRequest == hauling.RecordId && c.SiteId == siteId
                                    select new
                                    {
                                        r.RecordId_Snapshot
                                        , r.TransferDate
                                        , r.Company
                                        , r.Shift
                                        , r.Remark
                                        , Source = (r.Source_Block + " " + r.Source_ROM_Name)
                                        , Destination = (r.Destination_Block + " " + r.Destination_ROM_Name)
                                    }
                                );

                        var items_ROMTransfer = dataQuery_ROMTransfer.ToList();

                        List<DateTime> haulingDates = new List<DateTime>();
                        List<string> companies = new List<string>();

                        // Portion Blending
                        items_PortionBlending.ForEach(c2 =>
                        {
                            try
                            {
                                if ( ! haulingDates.Contains(c2.BlendingDate)) haulingDates.Add(c2.BlendingDate);
                                if ( ! companies.Contains(c2.Company)) companies.Add(c2.Company);
                            }
                            catch {}
                        });

                        // ROM_Transfer
                        items_ROMTransfer.ForEach(c3 =>
                        {
                            try
                            {
                                if ( ! haulingDates.Contains(c3.TransferDate)) haulingDates.Add(c3.TransferDate);
                                if ( ! companies.Contains(c3.Company)) companies.Add(c3.Company);
                            }
                            catch {}
                        });

                        companies.Sort();
                        companies.Reverse();

                        string companies_title = string.Empty;
                        foreach (string company in companies)
                        {
                            companies_title += (string.IsNullOrEmpty(companies_title)?"":@"/") + company;
                        }

                        string content_per_Date = string.Empty;
                        string content_per_Shift = string.Empty;
                        string dateStr = string.Empty;
                        string line_ROM_Transfer = string.Empty;
                        string line_PortionBlending = string.Empty;

                        string cell_date = string.Empty;
                        string cell_shift = string.Empty;
                        string cell_company = string.Empty;

                        int rowspan_Date = 0;
                        int rowspan_Shift = 0;
                        int rowspan_Company = 0;
                        int line_color = 0;

                        haulingDates.Sort();

                        foreach (DateTime haulingDate in haulingDates)
                        {
                            content_per_Date = string.Empty;
                            rowspan_Date = 0;

                            cell_date = string.Format(@"<td rowspan='__ROWSPAN_DATE__' style='text-align:center;vertical-align:middle;{1}'>{0}</td>", haulingDate.ToString("dd-MMM-yyyy"), style_td_border);

                            for (int shift = 1; shift <= 2; shift++)
                            {
                                content_per_Shift = string.Empty;
                                rowspan_Shift = 0;

                                cell_shift = string.Format(@"<td rowspan='__ROWSPAN_SHIFT__' style='text-align:center;vertical-align:middle;font-size:20px !important;font-weight:bold !important;{1}'>{0}</td>", (shift==1?"A":"B"), style_td_border);
                                line_color = 0;

                                foreach (string company in companies)
                                {
                                    // reset
                                    rowspan_Company = 0;

                                    // about line
                                    line_color++;

                                    line_ROM_Transfer = string.Empty;
                                    line_PortionBlending = string.Empty;

                                    if ((line_color % 2) == 0)
                                    {
                                        style_background = @"background-color:#ffffff !important;";
                                    }
                                    else
                                    {
                                        style_background = @"background-color:#EBF1DE !important;";
                                    }

                                    var dataQuery_R =
                                        (
                                            from d in items_ROMTransfer
                                            where d.TransferDate == haulingDate
                                                && d.Shift == shift
                                                && d.Company == company
                                            orderby d.Source, d.Destination
                                            select d
                                        );

                                    cell_company = "__COMPANY_ROM_TRANSFER__";

                                    var item_Rs = dataQuery_R.ToList();
                                    item_Rs.ForEach(item_R =>
                                    {
                                        rowspan_1_Hauling++;
                                        rowspan_Date++;
                                        rowspan_Shift++;
                                        rowspan_Company++;

                                        line_ROM_Transfer += string.Format(@"
                                                                            <tr>{8}{4}{5}
                                                                                {0}
                                                                                <td style='padding:0px 0px 0px 50px !important;vertical-align:middle;{6}{7};padding-left:30px !important;'>{1}</td>
                                                                                <td colspan='3' style='text-align:center;vertical-align:middle;{6}{7}'>{2}</td>
                                                                                <td style='{6}{7}'></td>
                                                                                <td style='text-align:center;vertical-align:middle;{6}{7}'>{3}</td>
                                                                            </tr>
                                                                            ", cell_company, item_R.Source, item_R.Destination, OurUtility.Handle_Enter(item_R.Remark)
                                                                            , cell_date, cell_shift, style_background, style_td_border
                                                                            , cell_Hauling);

                                        cell_Hauling = string.Empty;
                                        cell_date = string.Empty;
                                        cell_shift = string.Empty;
                                        cell_company = string.Empty;
                                    });

                                    var dataQuery_P =
                                        (
                                            from d in items_PortionBlending
                                            where d.BlendingDate == haulingDate
                                                && d.Shift == shift
                                                && d.Company == company
                                            select d
                                        );

                                    cell_company = "__COMPANY_PORTION_BLENDING__";

                                    var item_Ps = dataQuery_P.ToList();
                                    item_Ps.ForEach(item_P =>
                                    {
                                        rowspan_1_Hauling++;
                                        rowspan_Date++;
                                        rowspan_Shift++;
                                        rowspan_Company++;

                                        if (item_P.NoHauling)
                                        {
                                            line_PortionBlending += string.Format(@"
                                                                                <tr>{6}{2}{3}
                                                                                    {0}
                                                                                    <td colspan='6' style='text-align:center;vertical-align:middle;font-size:14px !important;font-weight:bold !important;{4}{5}'>{1}</td>
                                                                                </tr>
                                                                                ", cell_company, OurUtility.Handle_Enter(item_P.Remark)
                                                                                , cell_date, cell_shift, style_background, style_td_border
                                                                                , cell_Hauling);

                                            cell_Hauling = string.Empty;
                                            cell_date = string.Empty;
                                            cell_shift = string.Empty;
                                            cell_company = string.Empty;
                                        }
                                        else
                                        {
                                            string source = string.Empty;

                                            double detail_CV = 0.0;
                                            double detail_TS = 0.0;
                                            double detail_ASH = 0.0;

                                            var dataQuery_Source_Portion =
                                            (
                                                from p_detail in db.HaulingRequest_Detail_PortionBlending_Details
                                                where p_detail.PortionBlending == item_P.RecordId_Snapshot
                                                    && p_detail.HaulingRequest == hauling.RecordId
                                                orderby p_detail.block, p_detail.ROM_Name
                                                select p_detail
                                            );

                                            var items_Source_Portion = dataQuery_Source_Portion.ToList();
                                            items_Source_Portion.ForEach(c4 =>
                                            {
                                                source += string.Format(@"<tr><td style='vertical-align:middle;padding:0px 0px 0px 50px !important;{4}{5};padding-left:30px !important;'>{0} {1} ({2} {3})</td></tr>", c4.block, c4.ROM_Name, c4.Portion, @"%", style_background, style_td_border);

                                                //Kalkulasi Quality, dengan strategi seperti berikut:
                                                //   - pastikan bahwa semua data selalu di Round terlebih dahulu
                                                //   - baru kemudian dilakukan Kalkulasi Penjumlahan
                                                detail_CV += ((OurUtility.Round(c4.CV, 2) * c4.Portion) / 100.0);
                                                detail_TS += ((OurUtility.Round(c4.TS, 2) * c4.Portion) / 100.0);
                                                detail_ASH += ((OurUtility.Round(c4.ASH, 2) * c4.Portion) / 100.0);
                                            });
                                            
                                            // kembali dilakukan Rounding, agar konsisten
                                            detail_CV = OurUtility.Round(detail_CV, 0);
                                            detail_TS = OurUtility.Round(detail_TS, 2);
                                            detail_ASH = OurUtility.Round(detail_ASH, 2);
                                    
                                            string quality = string.Format(@"CV={0:N0}<br/>TS={1:N2}<br/>ASH={2:N2}", detail_CV, detail_TS, detail_ASH);
                                            var additionalBucketContent = string.Empty;
                                            if (item_P.TotalBucket != null)
                                            {
                                                additionalBucketContent = string.Format(@"<p style='margin-top:8px;'>Add {0} bucket from {1} into each unit SDT</p>", item_P.TotalBucket, item_P.ROM_Name);
                                            }

                                            line_PortionBlending = string.Format(@"
                                                                                <tr>{11}{7}{8}
                                                                                    {0}
                                                                                    <td style='padding:0px !important;{9}'><table style='width:100%;height:100%;{10}'>{1}</table></td>
                                                                                    <td style='text-align:center;vertical-align:middle;{9}{10}'>{2}</td>
                                                                                    <td style='text-align:center;vertical-align:middle;{9}{10}'>{3}</td>
                                                                                    <td style='text-align:center;vertical-align:middle;{9}{10}'>{4}</td>
                                                                                    <td style='text-align:center;vertical-align:middle;{9}{10}'>{5}</td>
                                                                                    <td style='text-align:center;vertical-align:middle;{9}{10}'>{6} {12}</td>
                                                                                </tr>
                                                                                ", cell_company, source, item_P.Hopper, item_P.Tunnel, item_P.Product, quality, OurUtility.Handle_Enter(item_P.Remark)
                                                                                , cell_date, cell_shift, style_background, style_td_border
                                                                                , cell_Hauling, additionalBucketContent);

                                            cell_Hauling = string.Empty;
                                            cell_date = string.Empty;
                                            cell_shift = string.Empty;
                                            cell_company = string.Empty;
                                        }
                                    });

                                    if (( ! string.IsNullOrEmpty(line_ROM_Transfer))
                                        && ( ! string.IsNullOrEmpty(line_PortionBlending)))
                                    {
                                        line_ROM_Transfer = line_ROM_Transfer.Replace("__COMPANY_ROM_TRANSFER__", "<td rowspan='" + rowspan_Company.ToString() + "' style='text-align:center;vertical-align:middle;font-size:14px !important;font-weight:bold !important;" + style_background + style_td_border + "'>" + company + "</td>");
                                        line_PortionBlending = line_PortionBlending.Replace("__COMPANY_PORTION_BLENDING__", "");
                                    }
                                    else
                                    {
                                        if ( ! string.IsNullOrEmpty(line_ROM_Transfer))
                                        {
                                            line_ROM_Transfer = line_ROM_Transfer.Replace("__COMPANY_ROM_TRANSFER__", "<td rowspan='" + rowspan_Company.ToString() + "' style='text-align:center;vertical-align:middle;font-size:14px !important;font-weight:bold !important;" + style_background + style_td_border + "'>" + company + "</td>");
                                            line_PortionBlending = line_PortionBlending.Replace("__COMPANY_PORTION_BLENDING__", "");
                                        }
                                        else
                                        {
                                            line_ROM_Transfer = line_ROM_Transfer.Replace("__COMPANY_ROM_TRANSFER__", "");
                                            line_PortionBlending = line_PortionBlending.Replace("__COMPANY_PORTION_BLENDING__", "<td rowspan='" + rowspan_Company.ToString() + "' style='text-align:center;vertical-align:middle;font-size:14px !important;font-weight:bold !important;" + style_background + style_td_border + "'>" + company + "</td>");
                                        }
                                    }

                                    content_per_Shift += line_ROM_Transfer + line_PortionBlending;
                                }

                                if (string.IsNullOrEmpty(content_per_Shift)) continue;

                                content_per_Shift = content_per_Shift.Replace("__ROWSPAN_SHIFT__", rowspan_Shift.ToString());

                                content_per_Date += content_per_Shift;
                            }

                            content_per_Hauling += content_per_Date.Replace("__ROWSPAN_DATE__", rowspan_Date.ToString());
                        }

                        content += content_per_Hauling.Replace("__ROWSPAN_HAULING__", rowspan_1_Hauling.ToString());
                    });

                    content = string.Format(@"
                                            <table style='width:100%;border-left:1px solid #dee2e6 !important;border-top:1px solid #dee2e6 !important;margin-bottom:50px;'>
                                                <tr>
                                                    <td colspan='10' style='text-align:center;vertical-align:middle;background-color:#00B050 !important;font-size:24px !important;color: #ffffff !important;font-weight:bold !important;font-style: italic !important;{2}'>REQUEST HAULING FC PRODUCTION {1}</td>
                                                </tr>
                                                <tr>
                                                    <td rowspan='2' style='text-align:center;vertical-align:middle;width:110px;background-color:#DAEEF3;font-size:14px !important;font-weight:bold !important;{2}'>Submitted On</td>
                                                    <td rowspan='2' style='text-align:center;vertical-align:middle;width:110px;background-color:#DAEEF3;font-size:14px !important;font-weight:bold !important;{2}'>Date</td>
                                                    <td rowspan='2' style='text-align:center;vertical-align:middle;width:100px;background-color:#DAEEF3;font-size:14px !important;font-weight:bold !important;{2}'>Shift</td>
                                                    <td rowspan='2' style='text-align:center;vertical-align:middle;width:100px;background-color:#DAEEF3;font-size:14px !important;font-weight:bold !important;{2}'>Company</td>
                                                    <td rowspan='2' style='text-align:center;vertical-align:middle;min-width:300px;background-color:#DAEEF3;font-size:14px !important;font-weight:bold !important;{2}'>Source</td>
                                                    <td colspan='3' style='text-align:center;vertical-align:middle;width:100px;background-color:#DAEEF3;font-size:14px !important;font-weight:bold !important;{2}'>Destination</td>
                                                    <td rowspan='2' style='text-align:center;vertical-align:middle;width:130px;background-color:#DAEEF3;font-size:14px !important;font-weight:bold !important;{2}'>Quality</td>
                                                    <td rowspan='2' style='text-align:center;vertical-align:middle;width:210px;background-color:#DAEEF3;font-size:14px !important;font-weight:bold !important;{2}'>Remark</td>
                                                </tr>
                                                <tr>
                                                    <td             style='text-align:center;vertical-align:middle;width:170px;background-color:#DAEEF3;font-size:14px !important;font-weight:bold !important;{2}'>Hopper</td>
                                                    <td             style='text-align:center;vertical-align:middle;width:70px;background-color:#DAEEF3;font-size:14px !important;font-weight:bold !important;{2}'>Tunnel</td>
                                                    <td             style='text-align:center;vertical-align:middle;width:80px;background-color:#DAEEF3;font-size:14px !important;font-weight:bold !important;{2}'>Product</td>
                                                </tr>
                                                {0}
                                            </table>
                                            ", content, "", style_td_border);
                }
            }
            catch (Exception ex)
            {
                content = ex.Message;
                p_message = ex.Message;
            }

            var result = new { Success = true, Permission = permission_Item, User = UserX.Information(user, is_Show_User_Menu, is_Show_User_Relation), Content = content, Message = string.Empty, MessageDetail = string.Empty, Version = Configuration.VERSION };
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        public JsonResult SummaryHaulingRequest(string[] company)
        {
            // Check Permission based on Current Url
            UserX user = new UserX(Request);
            Permission_Item permission_Item = null;
            bool is_Show_User_Menu = OurUtility.ValueOf(Request, "u_menu").Equals("1");
            bool is_Show_User_Relation = OurUtility.ValueOf(Request, "u_relation").Equals("1");
            Permission.Check_API(Request, user, ref permission_Item);
            if (!permission_Item.Is_View)
            {
                var msg = new { Success = false, Permission = permission_Item, Message = Permission.ERROR_PERMISSION_READ, MessageDetail = string.Empty, Version = Configuration.VERSION };
                return Json(msg, JsonRequestBehavior.AllowGet);
            }
            DateTime dateFrom = DateTime.Parse(Request["dateFrom"]);
            DateTime dateTo = DateTime.Parse(Request["dateTo"]);
            string rom = Request["rom"];
            bool allRom = string.IsNullOrEmpty(rom);
            bool allCompany = company == null || (company.Count() > 0 && company[0] == string.Empty) ? true : false;

            try
            {
                // get from hauling_request_detail_portionblending base on blending_date from filter
                // order by hauling request(id) as desc 
                // even if each data as same as blending date, choise only one data with highest hauling request (id)
                // join with hauling request detail portion blending details with same hauling request
                // get for each rom name and tonase
                using (MERCY_Ctx db = new MERCY_Ctx())
                {
                    var query = (
                            from h in db.HaulingRequest_Detail_PortionBlending
                            join hd in db.HaulingRequest_Detail_PortionBlending_Details on h.HaulingRequest equals hd.HaulingRequest
                            where
                            (
                                h.BlendingDate >= dateFrom
                                &&
                                h.BlendingDate <= dateTo
                            ) &&
                            (
                                allCompany || company.Any(u => u == h.Company)
                            )
                            select new
                            {
                                HaulingRequest = h.HaulingRequest,
                                h.BlendingDate,
                                hd.ROM_Name,
                                h.Shift,
                                hd.Ton,
                                h.Company,
                            }
                         );
                    var haulingRequest = query.ToList().OrderByDescending(u => u.HaulingRequest).AsEnumerable();
                    // get uniq date
                    var dates = haulingRequest.Select(u => new { u.BlendingDate, u.Shift }).Distinct().ToList();                    // eliminate data with date and hihest hauling request (id)
                    var romPerDate = dates.Select(u => new
                    {
                        Roms = haulingRequest.Where(h => h.HaulingRequest == haulingRequest.Where(hr => hr.BlendingDate == u.BlendingDate && hr.Shift == u.Shift).FirstOrDefault().HaulingRequest).ToList()
                    }).Distinct().ToList();
                    // this's base data before mapper by shift and rom name
                    var roms = new List<RomDto> { };
                    romPerDate.ForEach(romdate =>
                    {
                        var rommap = romdate.Roms.Select(r => new RomDto
                        {
                            RomName = r.ROM_Name,
                            Shift = r.Shift,
                            Ton = r.Ton,
                        });
                        roms.AddRange(rommap);
                    });

                    // get uniq romname and shift to filter base data
                    var romFilter = roms.Select(u => new
                    {
                        u.RomName,
                        u.Shift
                    }).Distinct().ToList();

                    // mapping base data by filter
                    var romResult = romFilter.Select(u => new
                    {
                        RomName = u.RomName,
                        Shift = u.Shift,
                        Ton = roms.Where(r => r.RomName == u.RomName && r.Shift == u.Shift).Select(x => x.Ton).Sum(),
                    });

                    // maaping base data by rom
                    var filterByRom = romResult.Where(d => (allRom || d.RomName.ToLower().Contains(rom.ToLower()))).OrderBy(u => u.RomName).ToList();
                    var result = new { Success = true, Permission = permission_Item, User = UserX.Information(user, is_Show_User_Menu, is_Show_User_Relation), Message = string.Empty, MessageDetail = string.Empty, Version = Configuration.VERSION, Items = filterByRom, Total = filterByRom.Count };
                    return Json(result, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                var result = new { Success = true, Permission = permission_Item, User = UserX.Information(user), Content = ex, Message = string.Empty, MessageDetail = string.Empty, Version = Configuration.VERSION };
                return Json(result, JsonRequestBehavior.AllowGet);
            }
        }
        public JsonResult TS_Synchronize()
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

            var date = string.IsNullOrEmpty(OurUtility.ValueOf(Request, "Date")) ? DateTime.Now : DateTime.Parse(OurUtility.ValueOf(Request, "Date"));
            var isAllCompany = string.IsNullOrEmpty(OurUtility.ValueOf(Request, "Company")) ? true : false;
            var company = OurUtility.ValueOf(Request, "Company");
            try
            {
                using (MERCY_Ctx db = new MERCY_Ctx())
                {
                    var result =
                            (
                                from hr in db.HaulingRequests
                                join h in db.HaulingRequest_Detail_PortionBlending on hr.RecordId equals h.HaulingRequest into hrh
                                from h in hrh.DefaultIfEmpty()
                                join p in db.PortionBlendings on h.RecordId_Snapshot equals p.RecordId into pb
                                from p in pb.DefaultIfEmpty()
                                join pd in db.PortionBlending_Details on p.RecordId equals pd.PortionBlending into pbd
                                from pd in pbd.DefaultIfEmpty()
                                where h.BlendingDate.Year == date.Year &&
                                      h.BlendingDate.Month == date.Month &&
                                      h.BlendingDate.Day == date.Day &&
                                      h.DeletedOn == null &&
                                      p.DeletedOn == null &&
                                      (
                                         isAllCompany || p.Company == company
                                      )
                                orderby hr.CreatedOn descending
                                select new
                                {
                                    haulingDate = hr.CreatedOn,
                                    p.Company,
                                    Block = pd.block,
                                    RomName = pd.ROM_Name,
                                    Date = p.BlendingDate,
                                    p.Shift,
                                    AdditionalBucketROM = p.ROM_Name,
                                    AdditionalTotalBucket = p.TotalBucket
                                }
                            ).ToList();
                    var shifts = result.Select(data => data.Shift).Distinct().ToList();
                    var latestDatePerShift = shifts.Select(shift => new
                    {
                        shift = shift,
                        haulingDate = result.Where(data => data.Shift == shift)
                                            .Select(data => data.haulingDate)
                                            .Distinct()
                                            .Take(1)
                                            .SingleOrDefault()
                    }).ToList();
                    if (result.Count > 0)
                    {
                        var latestHaulingRequest = result.Where(data => latestDatePerShift.Any(hr => hr.haulingDate == data.haulingDate && hr.shift == data.Shift))
                       .Select(data => new
                       {
                           data.Company,
                           data.Block,
                           data.RomName,
                           Date = data.Date.ToString("yyyy-MM-dd"),
                           data.Shift,
                           data.AdditionalBucketROM,
                           data.AdditionalTotalBucket,
                           HaulingDate = data.haulingDate.ToString("yyyy-MM-dd HH:mm:ss"),
                       }).ToList();
                        var response = new { Success = true, Message = string.Empty, MessageDetail = string.Empty, Version = Configuration.VERSION, Items = latestHaulingRequest, Total = latestHaulingRequest.Count };
                        return Json(response, JsonRequestBehavior.AllowGet);
                    }
                    return Json(new { Success = true, Message = "Data Not Found", MessageDetail = string.Empty, Version = Configuration.VERSION }, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                var msg = new { Success = false, Message = ex.Message, MessageDetail = ex, Version = Configuration.VERSION };
                return Json(msg, JsonRequestBehavior.AllowGet);
            }
        }
    }
}