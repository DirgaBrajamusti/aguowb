using System;
using System.Linq;
using System.Web.Mvc;

using MERCY.Data.EntityFramework;
using MERCY.Web.BackEnd.Models;
using MERCY.Web.BackEnd.Helpers;
using MERCY.Web.BackEnd.Security;
using Permission = MERCY.Web.BackEnd.Security.Permission;

namespace MERCY.Web.BackEnd.Areas.Api.Controllers
{
    public class TunnelController : Controller
    {
        public JsonResult Index()
        {
            // Check Permission based on Current Url
            UserX user = new UserX(Request);
            Permission_Item permission_Item = null;

            // -- Not necessary checking Permission
            // -- just Logging User: is enough
            if (user.UserId <= 0)
            {
                var msg = new { Success = false, Permission = permission_Item, Message = Permission.ERROR_PERMISSION_READ + " [not Login]", MessageDetail = string.Empty, Version = Configuration.VERSION };
                return Json(msg, JsonRequestBehavior.AllowGet);
            }

            bool is_Show_User_Menu = OurUtility.ValueOf(Request, "u_menu").Equals("1");
            bool is_Show_User_Relation = OurUtility.ValueOf(Request, "u_relation").Equals("1");

            bool isAll_Text = true;
            string txt = OurUtility.ValueOf(Request, "txt");
            isAll_Text = string.IsNullOrEmpty(txt);

            bool isAll_Company = true;
            string company = OurUtility.ValueOf(Request, "company");
            isAll_Company = (string.IsNullOrEmpty(company) || company == "all");

            bool isAllStatus = true;
            string status = OurUtility.ValueOf(Request, "is_active").ToLower();
            isAllStatus = string.IsNullOrEmpty(status);

            // -- Actual code
            try
            {
                using (MERCY_Ctx db = new MERCY_Ctx())
                {
                    var dataQuery =
                            (
                                from d in db.Tunnels
                                join u in db.Users on d.CreatedBy equals u.UserId
                                join c in db.Companies on d.CompanyCode equals c.CompanyCode
                                join p in db.Products on d.ProductId equals p.ProductId
                                where (isAll_Text || d.CompanyCode.Contains(txt))
                                        && (isAll_Company || d.CompanyCode == company)
                                        && (isAllStatus || d.IsActive == (status == "true"))
                                orderby d.IsActive descending, d.CompanyCode
                                select new Model_View_Tunnel
                                {
                                    TunnelId = d.TunnelId
                                    , CompanyCode = d.CompanyCode
                                    , IsActive = d.IsActive
                                    , Name = d.Name
                                    , CreatedOn = d.CreatedOn
                                    , FullName = u.FullName
                                    , CompanyName = c.Name
                                    , ProductId = d.ProductId
                                    , Product_Str = p.ProductName
                                    , EffectiveDate = d.EffectiveDate
                                    , Approval = d.Approval
                                    , Remark = d.Remark
                                }
                            );

                    var items = dataQuery.ToList();

                    items.ForEach(c =>
                    {
                        c.CreatedOn_Str = OurUtility.DateFormat(c.CreatedOn, @"dd-MMM-yyyy HH:mm:ss");
                        c.CreatedOn_Str2 = OurUtility.DateFormat(c.CreatedOn, "dd-MMM-yyyy");
                        c.EffectiveDate_Str = OurUtility.DateFormat(c.EffectiveDate, "dd-MMM-yyyy");
                    });

                    var dataQuery_Product =
                            (
                                from d in db.Products
                                orderby d.CompanyCode, d.ProductName
                                select new 
                                {
                                    d.CompanyCode
                                    , d.ProductId
                                    , d.ProductName
                                }
                            );
                    var items_Products = dataQuery_Product.ToList();

                    var result = new { Success = true, Permission = permission_Item, User = UserX.Information(user, is_Show_User_Menu, is_Show_User_Relation), Message = string.Empty, MessageDetail = string.Empty, Version = Configuration.VERSION, Items = items, Total = items.Count, Products = items_Products };
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
            if (!permission_Item.Is_View)
            {
                var msg = new { Success = false, Permission = permission_Item, Message = Permission.ERROR_PERMISSION_READ, MessageDetail = string.Empty, Version = Configuration.VERSION };
                return Json(msg, JsonRequestBehavior.AllowGet);
            }

            bool is_Show_User_Menu = OurUtility.ValueOf(Request, "u_menu").Equals("1");
            bool is_Show_User_Relation = OurUtility.ValueOf(Request, "u_relation").Equals("1");

            bool isAllCompany = true;
            string company = OurUtility.ValueOf(Request, "company");
            isAllCompany = (string.IsNullOrEmpty(company) || company == "all");

            // -- Actual code
            try
            {
                using (MERCY_Ctx db = new MERCY_Ctx())
                {
                    var refProduct = db.Products.ToList();
                    var dataQuery =
                            (
                                from d in db.Tunnels
                                join u in db.Users on d.CreatedBy equals u.UserId
                                join c in db.Companies on d.CompanyCode equals c.CompanyCode
                                join p in db.Products on d.ProductId equals p.ProductId
                                where d.IsActive == true && (isAllCompany || d.CompanyCode == company)
                                && p.IsActive == true
                                orderby d.TunnelId
                                select new Model_View_Tunnel
                                {
                                    TunnelId = d.TunnelId,
                                    CompanyCode = d.CompanyCode,
                                    IsActive = d.IsActive,
                                    Name = d.Name,
                                    CreatedOn = d.CreatedOn,
                                    FullName = u.FullName,
                                    CompanyName = c.Name,
                                    ProductId = d.ProductId,
                                    Product_Str = p.ProductName,
                                    EffectiveDate = d.EffectiveDate,
                                    Approval = d.Approval,
                                    Remark = d.Remark,
                                    UserId = user.UserId,
                                    Title = user.Title,
                                    Department = user.Department
                                }
                            );

                    var items = dataQuery.ToList();
                    var currentDate = DateTime.Now.Date;

                    var tunnelHistory = db.TunnelHistories.Where(x => x.EffectiveDate <= currentDate && x.IsActive == true).ToList();
                    items.ForEach(c =>
                    {
                        c.CreatedOn_Str = OurUtility.DateFormat(c.CreatedOn, @"dd-MMM-yyyy HH:mm:ss");
                        c.CreatedOn_Str2 = OurUtility.DateFormat(c.CreatedOn, "dd-MMM-yyyy");
                        c.EffectiveDate_Str = OurUtility.DateFormat(c.EffectiveDate, "dd-MMM-yyyy");

                        if (c.EffectiveDate > DateTime.Now)
                        {
                            var latestTunnelHistory = tunnelHistory.Where(x => x.TunnelId == c.TunnelId).OrderByDescending(x => x.EffectiveDate).FirstOrDefault();

                            if (latestTunnelHistory != null)
                            {
                                c.ProductId = latestTunnelHistory.ProductId;
                                c.Product_Str = refProduct.Where(x => x.ProductId == c.ProductId).FirstOrDefault().ProductName;
                                c.EffectiveDate = latestTunnelHistory.EffectiveDate;
                                c.EffectiveDate_Str = OurUtility.DateFormat(c.EffectiveDate, "dd-MMM-yyyy");
                            }
                        }
                    });

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
                using (MERCY_Ctx db = new MERCY_Ctx())
                {
                    var data = new Tunnel
                    {
                        CompanyCode = OurUtility.ValueOf(p_collection, "company"),
                        Name = OurUtility.ValueOf(p_collection, "name"),
                        ProductId = OurUtility.ToInt32(OurUtility.ValueOf(p_collection, "product")),
                        IsActive = OurUtility.ValueOf(p_collection, "isactive").Equals("1"),

                        CreatedOn = DateTime.Now,
                        CreatedBy = user.UserId,

                        Remark = OurUtility.ValueOf(p_collection, "remark"),
                        EffectiveDate = OurUtility.ToDateTime(OurUtility.ValueOf(p_collection, "effectiveDate")),
                        Approval = "Submitted"
                    };

                    db.Tunnels.Add(data);
                    db.SaveChanges();

                    AddHistory(db, data);

                    var result = new { Success = true, Permission = permission_Item, Message = BaseConstants.MESSAGE_CREATE_SUCCESS, MessageDetail = string.Empty, Version = Configuration.VERSION};
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
            if (!permission_Item.Is_Edit)
            {
                var msg = new { Success = false, Permission = permission_Item, Message = Permission.ERROR_PERMISSION_EDIT, MessageDetail = string.Empty, Version = Configuration.VERSION };
                return Json(msg, JsonRequestBehavior.AllowGet);
            }

            // -- Actual code
            try
            {
                int id = OurUtility.ToInt32(OurUtility.ValueOf(Request, ".id"));

                using (MERCY_Ctx db = new MERCY_Ctx())
                {
                    var dataQuery =
                        (
                            from d in db.Tunnels
                            where d.TunnelId == id
                            select d
                        );

                    var data = dataQuery.SingleOrDefault();

                    data.Name = OurUtility.ValueOf(p_collection, "name");
                    data.ProductId = OurUtility.ToInt32(OurUtility.ValueOf(p_collection, "product"));
                    data.Remark = OurUtility.ValueOf(p_collection, "remark");
                    data.EffectiveDate = OurUtility.ToDateTime(OurUtility.ValueOf(p_collection, "effectiveDate"));
                    data.IsActive = OurUtility.ValueOf(p_collection, "isactive").Equals("1");
                    data.LastModifiedOn = DateTime.Now;
                    data.LastModifiedBy = user.UserId;

                    if (data.ProductId != OurUtility.ToInt32(OurUtility.ValueOf(p_collection, "product")))
                    {
                        data.Approval = "Submitted";
                        data.IsActive = false;
                    }

                    db.SaveChanges();

                    AddHistory(db, data);

                    var result = new { Success = true, Permission = permission_Item, Message = BaseConstants.MESSAGE_UPDATE_SUCCESS, MessageDetail = string.Empty, Version = Configuration.VERSION};
                    return Json(result, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                var result = new { Success = false, Permission = permission_Item, ex.Message, MessageDetail = string.Empty, Version = Configuration.VERSION, Items = string.Empty, Total = 0 };
                return Json(result, JsonRequestBehavior.AllowGet);
            }
        }

        public JsonResult Delete()
        {
            // Check Permission based on Current Url
            UserX user = new UserX(Request);
            Permission_Item permission_Item = null;

            Permission.Check_API(Request, user, ref permission_Item);
            if ( ! permission_Item.Is_Delete)
            {
                var msg = new { Success = false, Permission = permission_Item, Message = Permission.ERROR_PERMISSION_DELETE, MessageDetail = string.Empty, Version = Configuration.VERSION };
                return Json(msg, JsonRequestBehavior.AllowGet);
            }

            // -- Actual code
            try
            {
                int id = OurUtility.ToInt32(OurUtility.ValueOf(Request, ".id"));

                using (MERCY_Ctx db = new MERCY_Ctx())
                {
                    var dataQuery =
                        (
                            from d in db.Tunnels
                            where d.TunnelId == id
                            select d
                        );

                    var tunnelHistory =
                        (
                            from d in db.TunnelHistories
                            where d.TunnelId == id
                            select d
                        ).ToList();

                    var data = dataQuery.SingleOrDefault();

                    db.TunnelHistories.RemoveRange(tunnelHistory);
                    db.Tunnels.Remove(data);

                    db.SaveChanges();

                    var result = new { Success = true, Permission = permission_Item, Message = BaseConstants.MESSAGE_DELETE_SUCCESS, MessageDetail = string.Empty, Version = Configuration.VERSION};
                    return Json(result, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                var result = new { Success = false, Permission = permission_Item, ex.Message, MessageDetail = string.Empty, Version = Configuration.VERSION, Items = string.Empty, Total = 0 };
                return Json(result, JsonRequestBehavior.AllowGet);
            }
        }

        public JsonResult Get_ddl()
        {
            // Check Permission based on Current Url
            UserX user = new UserX(Request);
            Permission_Item permission_Item = null;

            // -- Not necessary checking Permission
            //Permission.Check_API(Request, user, ref permission_Item);
            // -- just Logging User: is enough
            if (user.UserId <= 0)
            {
                var msg = new { Success = false, Permission = permission_Item, Message = Permission.ERROR_PERMISSION_READ + " [not Login]", MessageDetail = string.Empty, Version = Configuration.VERSION };
                return Json(msg, JsonRequestBehavior.AllowGet);
            }

            // -- Actual code
            try
            {
                using (MERCY_Ctx db = new MERCY_Ctx())
                {
                    var dataQuery =
                            (
                                from d in db.Tunnels
                                where d.IsActive
                                orderby d.Name
                                select new
                                {
                                    id = d.CompanyCode
                                    , text = d.Name
                                }
                            );

                    var items = dataQuery.ToList();
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

        public JsonResult History()
        {
            // Check Permission based on Current Url
            UserX user = new UserX(Request);
            Permission_Item permission_Item = null;

            // -- Not necessary checking Permission
            //Permission.Check_API(Request, user, ref permission_Item);
            // -- just Logging User: is enough
            if (user.UserId <= 0)
            {
                var msg = new { Success = false, Permission = permission_Item, Message = Permission.ERROR_PERMISSION_READ + " [not Login]", MessageDetail = string.Empty, Version = Configuration.VERSION };
                return Json(msg, JsonRequestBehavior.AllowGet);
            }

            // -- Actual code
            try
            {
                int id = OurUtility.ToInt32(OurUtility.ValueOf(Request, ".id"));

                using (MERCY_Ctx db = new MERCY_Ctx())
                {
                    var dataQuery =
                            (
                                from d in db.TunnelHistories
                                join p in db.Products on d.ProductId equals p.ProductId
                                where d.TunnelId == id
                                orderby d.RecordId
                                select new Model_View_Tunnel_History
                                {
                                    RecordId = d.RecordId
                                    , Company = d.CompanyCode
                                    , Name = d.Name
                                    , Product = p.ProductName
                                    , CreatedOn = d.CreatedOn
                                    , LastModifiedOn = d.LastModifiedOn
                                    , Remark = d.Remark
                                    , EffectiveDate_Date = d.EffectiveDate
                                }
                            );

                    var items = dataQuery.ToList();
                    items.ForEach(c =>
                    {
                        c.EffectiveDate = (c.EffectiveDate_Date == null ? null : OurUtility.DateFormat(c.EffectiveDate_Date, @"dd-MMM-yyyy HH:mm:ss"));
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

        private static void AddHistory(MERCY_Ctx p_db, Tunnel p_tunnel)
        {
            try
            {
                var data = new TunnelHistory
                {
                    TunnelId = p_tunnel.TunnelId,
                    CompanyCode = p_tunnel.CompanyCode,
                    Name = p_tunnel.Name,
                    CreatedBy = p_tunnel.CreatedBy,
                    CreatedOn = p_tunnel.CreatedOn,
                    LastModifiedBy = p_tunnel.LastModifiedBy,
                    LastModifiedOn = p_tunnel.LastModifiedOn,
                    IsActive = p_tunnel.IsActive,
                    ProductId = p_tunnel.ProductId,
                    Remark = p_tunnel.Remark,
                    EffectiveDate = p_tunnel.EffectiveDate
                };

                p_db.TunnelHistories.Add(data);
                p_db.SaveChanges();
            }
            catch {}
        }

        public JsonResult Approve(FormCollection p_collection)
        {
            // Check Permission based on Current Url
            UserX user = new UserX(Request);
            Permission_Item permission_Item = null;

            Permission.Check_API(Request, user, ref permission_Item);
            if (!permission_Item.Is_Delete)
            {
                var msg = new { Success = false, Permission = permission_Item, Message = Permission.ERROR_PERMISSION_DELETE, MessageDetail = string.Empty, Version = Configuration.VERSION };
                return Json(msg, JsonRequestBehavior.AllowGet);
            }

            // -- Actual code
            try
            {
                int id = OurUtility.ToInt32(OurUtility.ValueOf(p_collection, ".id"));

                using (MERCY_Ctx db = new MERCY_Ctx())
                {
                    var dataQuery =
                        (
                            from d in db.Tunnels
                            where d.TunnelId == id
                            select d
                        );

                    var data = dataQuery.SingleOrDefault();

                    var lastActiveData = data.TunnelHistories.Where(th => th.IsActive).OrderByDescending(th => th.CreatedOn).FirstOrDefault();
                    if (lastActiveData == null || (data.ProductId != lastActiveData.ProductId))
                    {
                        data.Approval = "Acknowledged";
                        data.IsActive = true;
                    }

                    db.SaveChanges();

                    AddHistory(db, data);

                    var result = new { Success = true, Permission = permission_Item, Message = BaseConstants.MESSAGE_UPDATE_SUCCESS, MessageDetail = string.Empty, Version = Configuration.VERSION };
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