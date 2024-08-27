using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

using System.Data;

using MERCY.Data.EntityFramework;
using MERCY.Data.EntityFramework_BigData;
using MERCY.Web.BackEnd.Models;
using MERCY.Web.BackEnd.Helpers;
using MERCY.Web.BackEnd.Security;
using Permission = MERCY.Web.BackEnd.Security.Permission;

namespace MERCY.Web.BackEnd.Areas.Api.Controllers
{
    public class RomTransferController : Controller
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

            string dateFrom = OurUtility.ValueOf(Request, "dateFrom");
            string dateTo = OurUtility.ValueOf(Request, "dateTo");

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
                                from d in db.ROMTransfers
                                where (is_company_ALL || d.Company == company)
                                    && d.TransferDate >= dateFrom_O
                                    && d.TransferDate < dateTo_O
                                    && (is_shift_ALL || d.Shift == shift)
                                orderby d.TransferDate, d.Shift
                                select new Model_View_ROMTransfer
                                {
                                    RecordId = d.RecordId
                                    , Company = d.Company
                                    , TransferDate = d.TransferDate
                                    , Shift = d.Shift
                                    , Source_Block = d.Source_Block
                                    , Source_ROM_ID = d.Source_ROM_ID
                                    , Source_ROM_Name = d.Source_ROM_Name
                                    , Source = (d.Source_Block + " " + d.Source_ROM_Name)
                                    , Destination_Block = d.Destination_Block
                                    , Destination_ROM_ID = d.Destination_ROM_ID
                                    , Destination_ROM_Name = d.Destination_ROM_Name
                                    , Destination = (d.Destination_Block + " " + d.Destination_ROM_Name)
                                    , Remark = d.Remark
                                }
                            );

                    var items = dataQuery.ToList();
                    try
                    {
                        items.ForEach(c =>
                        {
                            c.TransferDate_Str = OurUtility.DateFormat(c.TransferDate, "dd-MMM-yyyy");
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
            if ( ! permission_Item.Is_Add)
            {
                var msg = new { Success = false, Permission = permission_Item, Message = Permission.ERROR_PERMISSION_ADD, MessageDetail = string.Empty, Version = Configuration.VERSION };
                return Json(msg, JsonRequestBehavior.AllowGet);
            }

            // -- Actual code
            try
            {
                string company = OurUtility.ValueOf(p_collection, "Company");
                DateTime transferDate = DateTime.Parse(OurUtility.ValueOf(p_collection, "TransferDate"));
                int shift = OurUtility.ToInt32(OurUtility.ValueOf(p_collection, "Shift"));

                int source_ROM_ID = -1;
                string source_Block = string.Empty;
                string source_ROM_Name = string.Empty;
                int destination_ROM_ID = -1;
                string destination_Block = string.Empty;
                string destination_ROM_Name = string.Empty;
                
                if (company == "BEK")
                {
                    source_Block = OurUtility.ValueOf(p_collection, "Source_ROM_Location");
                    source_ROM_Name = OurUtility.ValueOf(p_collection, "Source_ROM_Number2");
                    destination_Block = OurUtility.ValueOf(p_collection, "Destination_ROM_Location");
                    destination_ROM_Name = OurUtility.ValueOf(p_collection, "Destination_ROM_Number2");
                }
                else
                {
                    source_ROM_ID = OurUtility.ToInt32(OurUtility.ValueOf(p_collection, "Source_ROM_Number"));
                    destination_ROM_ID = OurUtility.ToInt32(OurUtility.ValueOf(p_collection, "Destination_ROM_Number"));

                    Dictionary<int, string> blocks = new Dictionary<int, string>();
                    Dictionary<int, string> romNames = new Dictionary<int, string>();

                    try
                    {
                        using (MERCY_BigData_Ctx db = new MERCY_BigData_Ctx())
                        {
                            var dataQuery =
                                    (
                                        from d in db.Mercy_ROM_Info
                                        select new
                                        {
                                            d.ROM_id
                                            , d.company_code
                                            , d.Block
                                            , d.ROM_Name
                                            , Names = (d.Block + " " + d.ROM_Name)
                                        }
                                    );

                            var data = dataQuery.ToList();

                            data.ForEach(c =>
                            {
                                blocks.Add(c.ROM_id, c.Block);
                                romNames.Add(c.ROM_id, c.ROM_Name);
                            });
                        }
                    }
                    catch {}

                    try
                    {
                        source_Block = blocks[source_ROM_ID];
                    }
                    catch {}
                    try
                    {
                        source_ROM_Name = romNames[source_ROM_ID];
                    }
                    catch {}
                    try
                    {
                        destination_Block = blocks[destination_ROM_ID];
                    }
                    catch {}
                    try
                    {
                        destination_ROM_Name = romNames[destination_ROM_ID];
                    }
                    catch {}
                }

                using (MERCY_Ctx db = new MERCY_Ctx())
                {
                    var dataQueryx =
                                    (
                                        from d in db.ROMTransfers
                                        where d.Company == company
                                            && d.TransferDate == transferDate
                                            && d.Shift == shift
                                            && d.Source_ROM_ID == source_ROM_ID
                                            && d.Destination_ROM_ID == destination_ROM_ID
                                        select d
                                    );

                    var romTransfer = dataQueryx.SingleOrDefault();
                    if (romTransfer == null)
                    {
                        // aman
                    }
                    else
                    {
                        // sudah ada data
                        string msg2 = string.Format(@"Data already exists in ROM Transfer.
    Company: {0}
    Transfer Date: {1}
    Shift: {2}
    Source: {3}
    Destination: {4}
"
                                                    , company, transferDate.ToString("dd-MMM-yyyy"), shift, (source_Block + " " + source_ROM_Name), (destination_Block + " " + destination_ROM_Name));

                        var result2 = new { Success = false, Permission = permission_Item, Message = msg2, MessageDetail = string.Empty, Version = Configuration.VERSION, Data = 0 };
                        return Json(result2, JsonRequestBehavior.AllowGet);
                    }

                    var data = new ROMTransfer
                    {
                        Company = company,
                        TransferDate = transferDate,
                        Shift = shift,

                        Source_ROM_ID = source_ROM_ID,
                        Source_Block = source_Block,
                        Source_ROM_Name = source_ROM_Name,

                        Destination_ROM_ID = destination_ROM_ID,
                        Destination_Block = destination_Block,
                        Destination_ROM_Name = destination_ROM_Name,

                        Remark = OurUtility.ValueOf(p_collection, "Remark"),

                        CreatedOn = DateTime.Now,
                        CreatedBy = user.UserId
                    };

                    db.ROMTransfers.Add(data);
                    db.SaveChanges();

                    long id = data.RecordId;

                    var result = new { Success = true, Permission = permission_Item, Message = BaseConstants.MESSAGE_CREATE_SUCCESS, MessageDetail = string.Empty, Version = Configuration.VERSION, Data = id };
                    return Json(result, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                var result = new { Success = false, Permission = permission_Item, ex.Message, MessageDetail = string.Empty, Version = Configuration.VERSION};
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
                                from d in db.ROMTransfers
                                where d.RecordId == id
                                select new Model_View_ROMTransfer
                                {
                                    RecordId = d.RecordId
                                    , Company = d.Company
                                    , TransferDate = d.TransferDate
                                    , Shift = d.Shift
                                    , Source_Block = d.Source_Block
                                    , Source_ROM_ID = d.Source_ROM_ID
                                    , Source_ROM_Name = d.Source_ROM_Name
                                    , Source = (d.Source_Block + " " + d.Source_ROM_Name)
                                    , Destination_Block = d.Destination_Block
                                    , Destination_ROM_ID = d.Destination_ROM_ID
                                    , Destination_ROM_Name = d.Destination_ROM_Name
                                    , Destination = (d.Destination_Block + " " + d.Destination_ROM_Name)
                                    , Remark = d.Remark
                                }
                            );

                    var item = dataQuery.SingleOrDefault();
                    if (item == null)
                    {
                        var result_NotFound = new { Success = false, Permission = permission_Item, User = UserX.Information(user, is_Show_User_Menu, is_Show_User_Relation), Message = "Id: " + id.ToString() + " is not found", MessageDetail = string.Empty, Version = Configuration.VERSION};
                        return Json(result_NotFound, JsonRequestBehavior.AllowGet);
                    }

                    item.TransferDate_Str = OurUtility.DateFormat(item.TransferDate, "MM/dd/yyyy");
                    
                    var result = new { Success = true, Permission = permission_Item, User = UserX.Information(user, is_Show_User_Menu, is_Show_User_Relation), Message = string.Empty, MessageDetail = string.Empty, Version = Configuration.VERSION, Item = item};
                    return Json(result, JsonRequestBehavior.AllowGet);
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
                string company = OurUtility.ValueOf(p_collection, "Company");
                DateTime transferDate = DateTime.Parse(OurUtility.ValueOf(p_collection, "TransferDate"));
                int shift = OurUtility.ToInt32(OurUtility.ValueOf(p_collection, "Shift"));

                int source_ROM_ID = -1;
                string source_Block = string.Empty;
                string source_ROM_Name = string.Empty;
                int destination_ROM_ID = -1;
                string destination_Block = string.Empty;
                string destination_ROM_Name = string.Empty;

                if (company == "BEK")
                {
                    source_Block = OurUtility.ValueOf(p_collection, "Source_ROM_Location");
                    source_ROM_Name = OurUtility.ValueOf(p_collection, "Source_ROM_Number2");
                    destination_Block = OurUtility.ValueOf(p_collection, "Destination_ROM_Location");
                    destination_ROM_Name = OurUtility.ValueOf(p_collection, "Destination_ROM_Number2");
                }
                else
                {
                    source_ROM_ID = OurUtility.ToInt32(OurUtility.ValueOf(p_collection, "Source_ROM_Number"));
                    destination_ROM_ID = OurUtility.ToInt32(OurUtility.ValueOf(p_collection, "Destination_ROM_Number"));

                    Dictionary<int, string> blocks = new Dictionary<int, string>();
                    Dictionary<int, string> romNames = new Dictionary<int, string>();

                    try
                    {
                        using (MERCY_BigData_Ctx db = new MERCY_BigData_Ctx())
                        {
                            var dataQuery =
                                    (
                                        from d in db.Mercy_ROM_Info
                                        select new
                                        {
                                            d.ROM_id
                                            , d.company_code
                                            , d.Block
                                            , d.ROM_Name
                                            , Names = (d.Block + " " + d.ROM_Name)
                                        }
                                    );

                            var data = dataQuery.ToList();

                            data.ForEach(c =>
                            {
                                blocks.Add(c.ROM_id, c.Block);
                                romNames.Add(c.ROM_id, c.ROM_Name);
                            });
                        }
                    }
                    catch {}

                    try
                    {
                        source_Block = blocks[source_ROM_ID];
                    }
                    catch {}
                    try
                    {
                        source_ROM_Name = romNames[source_ROM_ID];
                    }
                    catch {}
                    try
                    {
                        destination_Block = blocks[destination_ROM_ID];
                    }
                    catch {}
                    try
                    {
                        destination_ROM_Name = romNames[destination_ROM_ID];
                    }
                    catch {}
                }

                using (MERCY_Ctx db = new MERCY_Ctx())
                {
                    var dataQueryx =
                                    (
                                        from d in db.ROMTransfers
                                        where d.RecordId != id // bukan dirinya sendiri
                                            && d.Company == company
                                            && d.TransferDate == transferDate
                                            && d.Shift == shift
                                            && d.Source_ROM_ID == source_ROM_ID
                                            && d.Destination_ROM_ID == destination_ROM_ID
                                        select d
                                    );

                    var romTransfer = dataQueryx.SingleOrDefault();
                    if (romTransfer == null)
                    {
                        // aman
                    }
                    else
                    {
                        // sudah ada data
                        string msg2 = string.Format(@"Data already exists in ROM Transfer.
    Company: {0}
    Transfer Date: {1}
    Shift: {2}
    Source: {3}
    Destination: {4}
"
                                                    , company, transferDate.ToString("dd-MMM-yyyy"), shift, (source_Block + " " + source_ROM_Name), (destination_Block + " " + destination_ROM_Name));

                        var result2 = new { Success = false, Permission = permission_Item, Message = msg2, MessageDetail = string.Empty, Version = Configuration.VERSION, Data = 0 };
                        return Json(result2, JsonRequestBehavior.AllowGet);
                    }

                    var dataQuery =
                        (
                            from d in db.ROMTransfers
                            where d.RecordId == id
                            select d
                        );

                    var data = dataQuery.SingleOrDefault();

                    data.Company = company;
                    data.TransferDate = transferDate;
                    data.Shift = shift;

                    data.Source_ROM_ID = source_ROM_ID;
                    data.Source_Block = source_Block;
                    data.Source_ROM_Name = source_ROM_Name;

                    data.Destination_ROM_ID = destination_ROM_ID;
                    data.Destination_Block = destination_Block;
                    data.Destination_ROM_Name = destination_ROM_Name;

                    data.Remark = OurUtility.ValueOf(p_collection, "Remark");


                    data.LastModifiedOn = DateTime.Now;
                    data.LastModifiedBy = user.UserId;

                    db.SaveChanges();

                    var result = new { Success = true, Permission = permission_Item, Message = BaseConstants.MESSAGE_UPDATE_SUCCESS, MessageDetail = string.Empty, Version = Configuration.VERSION, Data = id };
                    return Json(result, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                var result = new { Success = false, Permission = permission_Item, ex.Message, MessageDetail = string.Empty, Version = Configuration.VERSION, Items = string.Empty, Total = 0 };
                return Json(result, JsonRequestBehavior.AllowGet);
            }
        }
    }
}