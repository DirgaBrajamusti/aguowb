using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

using MERCY.Data.EntityFramework;
using MERCY.Web.BackEnd.Models;
using MERCY.Web.BackEnd.Helpers;
using MERCY.Web.BackEnd.Security;
using Permission = MERCY.Web.BackEnd.Security.Permission;

namespace MERCY.Web.BackEnd.Areas.Api.Controllers
{
    public class CompanyController : Controller
    {
        public void InitializeController(System.Web.Routing.RequestContext context)
        {
            base.Initialize(context);
        }

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

            bool isAll_Text = true;
            string txt = OurUtility.ValueOf(Request, "txt");
            isAll_Text = string.IsNullOrEmpty(txt);

            bool isAll_Site = true;
            string site_str = Request["site"];
            int site = OurUtility.ToInt32(site_str);
            isAll_Site = (string.IsNullOrEmpty(site_str) || site == 0);

            // -- Actual code
            try
            {
                using (MERCY_Ctx db = new MERCY_Ctx())
                {
                    var dataQuery =
                            (
                                from d in db.Companies
                                    join u in db.Users on d.CreatedBy equals u.UserId
                                    join s in db.Sites on d.SiteId equals s.SiteId
                                where (isAll_Text || d.CompanyCode.Contains(txt))
                                        && (isAll_Site || d.SiteId == site)
                                orderby d.IsActive descending, s.SiteName, d.CompanyCode
                                select new Model_View_Company
                                {
                                    CompanyCode = d.CompanyCode
                                    , IsActive = d.IsActive
                                    , Name = d.Name
                                    , CreatedOn = d.CreatedOn
                                    , FullName = u.FullName
                                    , SiteName = s.SiteName
                                    , Is_DataROM_from_BI = d.Is_DataROM_from_BI
                                }
                            );

                    var items = dataQuery.ToList();
                    try
                    {
                        items.ForEach(c =>
                        {
                            c.CreatedOn_Str = OurUtility.DateFormat(c.CreatedOn, @"dd-MMM-yyyy HH:mm:ss");
                            c.CreatedOn_Str2 = OurUtility.DateFormat(c.CreatedOn, "dd-MMM-yyyy");
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
                using (MERCY_Ctx db = new MERCY_Ctx())
                {
                    var data = new Company
                    {
                        CompanyCode = OurUtility.ValueOf(p_collection, "code"),
                        Name = OurUtility.ValueOf(p_collection, "name"),
                        IsActive = OurUtility.ValueOf(p_collection, "isactive").Equals("1"),
                        SiteId = OurUtility.ToInt32(OurUtility.ValueOf(p_collection, "site")),
                        Is_DataROM_from_BI = OurUtility.ValueOf(p_collection, "bi").Equals("1"),

                        CreatedOn = DateTime.Now,
                        CreatedBy = user.UserId
                    };

                    db.Companies.Add(data);
                    db.SaveChanges();

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
            if ( ! permission_Item.Is_Edit)
            {
                var msg = new { Success = false, Permission = permission_Item, Message = Permission.ERROR_PERMISSION_EDIT, MessageDetail = string.Empty, Version = Configuration.VERSION };
                return Json(msg, JsonRequestBehavior.AllowGet);
            }

            // -- Actual code
            try
            {
                string code = OurUtility.ValueOf(Request, ".id");

                using (MERCY_Ctx db = new MERCY_Ctx())
                {
                    var dataQuery =
                        (
                            from d in db.Companies
                            where d.CompanyCode == code
                            select d
                        );

                    var data = dataQuery.SingleOrDefault();

                    data.Name = OurUtility.ValueOf(p_collection, "name");
                    data.IsActive = OurUtility.ValueOf(p_collection, "isactive").Equals("1");
                    data.Is_DataROM_from_BI = OurUtility.ValueOf(p_collection, "bi").Equals("1");

                    data.LastModifiedOn = DateTime.Now;
                    data.LastModifiedBy = user.UserId;

                    db.SaveChanges();

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
                string code = OurUtility.ValueOf(Request, ".id");

                using (MERCY_Ctx db = new MERCY_Ctx())
                {
                    var dataQuery =
                        (
                            from d in db.Companies
                            where d.CompanyCode == code
                            select d
                        );

                    var data = dataQuery.SingleOrDefault();
                    db.Companies.Remove(data);

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
                                from d in db.Companies
                                where d.IsActive
                                orderby d.Name
                                select new
                                {
                                    id = d.CompanyCode
                                    , text = d.Name
                                    , d.SiteId
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

        internal bool Is_from_BigData(string p_company)
        {
            bool result = false;

            try
            {
                using (MERCY_Ctx db = new MERCY_Ctx())
                {
                    var dataQuery =
                            (
                                from d in db.Companies
                                where d.CompanyCode == p_company
                                select new
                                {
                                    d.Is_DataROM_from_BI
                                }
                            );

                    var data = dataQuery.SingleOrDefault();
                    if (data != null)
                    {
                        result = data.Is_DataROM_from_BI;
                    }
                }
            }
            catch {}

            return result;
        }

        public JsonResult GetROMsQuality()
        {
            // Check Permission based on Current Url
            UserX user = new UserX(Request);
            Permission_Item permission_Item = null;

            Permission.Check_API(Request, user, ref permission_Item);
            /*if ( ! permission_Item.Is_View)
            {
                var msg = new { Success = false, Permission = permission_Item, Message = Permission.ERROR_PERMISSION_READ, MessageDetail = string.Empty, Version = Configuration.VERSION };
                return Json(msg, JsonRequestBehavior.AllowGet);
            }*/

            // -- Actual code
            string company = Request["company"];

            try
            {
                using (MERCY_Ctx db = new MERCY_Ctx())
                {
                    var dataQuery =
                            (
                                from d in db.ROMs
                                where d.CompanyCode == company
                                select new Model_View_Local_ROM
                                {
                                    Block = "" ,
                                    ROM_Name = d.ROMName,
                                    ROM_Id = d.ROMId,
                                    Ton = d.Ton,
                                    CV = d.CV,
                                    TS = d.TS,
                                    ASH = d.ASH,
                                    IM = d.IM,
                                    TM = d.TM,
                                    Process_Date = null, //d.Process_Date,
                                    company_code = d.CompanyCode,
                                    Names = string.Empty
                                }
                            ); ;

                    var items = dataQuery.ToList();

                    items.ForEach(c =>
                    {
                        c.Process_Date_Str = OurUtility.DateFormat(c.Process_Date, "dd-MMM-yyyy HH:mm");
                        c.Ton_Str = c.Ton.ToString();
                        c.CV_Str = string.Format("{0:N0}", OurUtility.Round(c.CV, 0));
                        c.TS_Str = string.Format("{0:N2}", OurUtility.Round(c.TS, 2));
                        c.ASH_Str = string.Format("{0:N2}", OurUtility.Round(c.ASH, 2));
                        c.IM_Str = string.Format("{0:N2}", OurUtility.Round(c.IM, 2));
                        c.TM_Str = string.Format("{0:N2}", OurUtility.Round(c.TM, 2));

                        try
                        {
                            c.Names = (c.ROM_Name);
                        }
                        catch {}
                    });

                    List<Model_View_Local_ROM> sorted_Items = items.OrderBy(x => x.Names).ToList();

                    var result = new { Success = true, Permission = permission_Item, Message = string.Empty, MessageDetail = string.Empty, Version = Configuration.VERSION, Items = sorted_Items, Total = sorted_Items.Count };
                    return Json(result, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                var result = new { Success = false, Permission = permission_Item, ex.Message, MessageDetail = string.Empty, Version = Configuration.VERSION, Items = string.Empty, Total = 0 };
                return Json(result, JsonRequestBehavior.AllowGet);
            }
        }

        public JsonResult SynchronizeProduct()
        {
            var controllerB = new ExternalDataController();
            controllerB.InitializeController(this.Request.RequestContext);

            var products = controllerB.SynchronizeProduct();

            return products;
        }

        public JsonResult SynchronizeROM()
        {
            var controllerB = new ExternalDataController();
            controllerB.InitializeController(this.Request.RequestContext);

            var roms = controllerB.SynchronizeROM();

            return roms;
        }
    }
}