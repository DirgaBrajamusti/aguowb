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
    public class MaintenanceActivityController : Controller
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

            string companyCode = OurUtility.ValueOf(Request, "company").ToString();

            int? siteId = null;
            if (!string.IsNullOrEmpty(OurUtility.ValueOf(Request, "site")) && OurUtility.ValueOf(Request, "site") != "all")
                siteId = int.Parse(OurUtility.ValueOf(Request, "site"));

            bool allCompany = OurUtility.ValueOf(Request, "company") == "all";
            bool allSite = siteId == null;

            bool isAll_Text = true;
            string txt = OurUtility.ValueOf(Request, "txt");
            isAll_Text = string.IsNullOrEmpty(txt);

            // -- Actual code
            try
            {
                using (MERCY_Ctx db = new MERCY_Ctx())
                {
                    var allowedCompanies = db.UserCompanies.Where(u => u.UserId == user.UserId).Select(c => c.CompanyCode).ToList();  
                    var allowedSites = db.UserSites.Where(u => u.UserId == user.UserId).Select(u => u.SiteId).ToList();

                    var dataQuery =
                            (
                                from d in db.MaintenanceActivities
                                    join u in db.Users on d.CreatedBy equals u.UserId
                                    join c in db.Companies on d.CompanyCode equals c.CompanyCode into cc
                                    from c in cc.DefaultIfEmpty()
                                    join s in db.Sites on d.SiteId equals s.SiteId into mas
                                    from s in mas.DefaultIfEmpty()
                                where (isAll_Text || d.Category.Contains(txt)) 
                                        && ((allCompany && allowedCompanies.Any(u => u == d.CompanyCode)) || d.CompanyCode == companyCode)
                                        && ((allSite && allowedSites.Any(u => u == d.SiteId)) || d.SiteId == siteId)
                                orderby d.Status descending, d.Category
                                select new Model_View_MaintenanceActivity
                                {
                                    RecordId = d.RecordId
                                    , Status = d.Status
                                    , Category = d.Category
                                    , CreatedOn = d.CreatedOn
                                    , FullName = u.FullName
                                    , CompanyCode = d.CompanyCode
                                    , SiteId = d.SiteId
                                    , CompanyName = c.Name
                                    , SiteName = s.SiteName
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

            int siteId = 0;
            if (!string.IsNullOrEmpty(OurUtility.ValueOf(Request, "site")))
                siteId = int.Parse(OurUtility.ValueOf(Request, "site"));
            var companyCode = OurUtility.ValueOf(p_collection, "company");

            // -- Actual code
            try
            {
                using (MERCY_Ctx db = new MERCY_Ctx())
                {
                    if (string.IsNullOrEmpty(OurUtility.ValueOf(Request, "site")) || !db.Sites.Any(u => u.SiteId == siteId)) 
                    {
                        var msg = new { Success = false, Permission = permission_Item, Message = "The requested site is not registered in our system! Please consider selecting a different site.", MessageDetail = string.Empty, Version = Configuration.VERSION };
                        return Json(msg, JsonRequestBehavior.AllowGet);
                    }

                    if (string.IsNullOrEmpty(companyCode) || !db.Companies.Any(u => u.CompanyCode == companyCode && u.SiteId == siteId))
                    {
                        var msg = new { Success = false, Permission = permission_Item, Message = "The requested Company is not registered in our system! Please consider selecting a different company.", MessageDetail = string.Empty, Version = Configuration.VERSION };
                        return Json(msg, JsonRequestBehavior.AllowGet);
                    }
                    
                    var categoryname = OurUtility.ValueOf(p_collection, "Category").ToString();
                    if (db.MaintenanceActivities.Any(u => u.Category == categoryname && u.CompanyCode == companyCode && u.SiteId == siteId))
                    {
                        var msg = new { Success = false, Permission = permission_Item, Message = "Maintenance Activity name has been existed for " + companyCode + " Company!", MessageDetail = string.Empty, Version = Configuration.VERSION };
                        return Json(msg, JsonRequestBehavior.AllowGet);
                    }

                    var data = new MaintenanceActivity
                    {
                        Category = categoryname,
                        Status = OurUtility.ValueOf(p_collection, "Status").Equals("1"),
                        CompanyCode = companyCode,
                        SiteId = siteId,
                        CreatedOn = DateTime.Now,
                        CreatedBy = user.UserId
                    };

                    db.MaintenanceActivities.Add(data);
                    db.SaveChanges();

                    long id = data.RecordId;

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

            int siteId = 0;
            if (!string.IsNullOrEmpty(OurUtility.ValueOf(Request, "site")))
                siteId = int.Parse(OurUtility.ValueOf(Request, "site"));
            var companyCode = OurUtility.ValueOf(p_collection, "company");

            // -- Actual code
            try
            {
                long id = OurUtility.ToInt64(Request[".id"]);

                using (MERCY_Ctx db = new MERCY_Ctx())
                {
                    if (string.IsNullOrEmpty(OurUtility.ValueOf(Request, "site")) || !db.Sites.Any(u => u.SiteId == siteId))
                    {
                        var msg = new { Success = false, Permission = permission_Item, Message = "The requested site is not registered in our system! Please consider selecting a different site.", MessageDetail = string.Empty, Version = Configuration.VERSION };
                        return Json(msg, JsonRequestBehavior.AllowGet);
                    }

                    if (string.IsNullOrEmpty(companyCode) || !db.Companies.Any(u => u.CompanyCode == companyCode && u.SiteId == siteId))
                    {
                        var msg = new { Success = false, Permission = permission_Item, Message = "The requested Company is not registered in our system! Please consider selecting a different company.", MessageDetail = string.Empty, Version = Configuration.VERSION };
                        return Json(msg, JsonRequestBehavior.AllowGet);
                    }

                    var dataQuery =
                        (
                            from d in db.MaintenanceActivities
                            where d.RecordId == id
                            select d
                        );

                    var data = dataQuery.SingleOrDefault();
                    var categoryname = OurUtility.ValueOf(p_collection, "Category").ToString();

                    if (db.MaintenanceActivities.Any(u => u.RecordId != id && u.Category == categoryname && u.CompanyCode == companyCode && u.SiteId == siteId))
                    {
                        var msg = new { Success = false, Permission = permission_Item, Message = "Maintenance Activity name has been existed for " + companyCode + " Company!", MessageDetail = string.Empty, Version = Configuration.VERSION };
                        return Json(msg, JsonRequestBehavior.AllowGet);
                    }

                    data.Category = categoryname;
                    data.Status = OurUtility.ValueOf(p_collection, "Status").Equals("1");
                    data.CompanyCode = companyCode;
                    data.SiteId = siteId;

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

        public JsonResult Delete(FormCollection p_collection)
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
                long id = OurUtility.ToInt64(Request[".id"]);

                using (MERCY_Ctx db = new MERCY_Ctx())
                {
                    var dataQuery =
                        (
                            from d in db.MaintenanceActivities
                            where d.RecordId == id
                            select d
                        );

                    var data = dataQuery.SingleOrDefault();
                    db.MaintenanceActivities.Remove(data);

                    db.SaveChanges();

                    var result = new { Success = true, Permission = permission_Item, Message = BaseConstants.MESSAGE_DELETE_SUCCESS, MessageDetail = string.Empty, Version = Configuration.VERSION, Data = id };
                    return Json(result, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                var result = new { Success = false, Permission = permission_Item, ex.Message, MessageDetail = string.Empty, Version = Configuration.VERSION, Items = string.Empty, Total = 0 };
                return Json(result, JsonRequestBehavior.AllowGet);
            }
        }

        public JsonResult Bulk(FormCollection p_collection)
        {
            // Check Permission based on Current Url
            UserX user = new UserX(Request);
            Permission_Item permission_Item = null;

            Permission.Check_API(Request, user, ref permission_Item);
            if (permission_Item.Is_Add && permission_Item.Is_Edit && permission_Item.Is_Delete) {}
            else
            {
                var msg = new { Success = false, Permission = permission_Item, Message = Permission.ERROR_PERMISSION_ADD, MessageDetail = string.Empty, Version = Configuration.VERSION };
                return Json(msg, JsonRequestBehavior.AllowGet);
            }

            // -- Actual code
            try
            {
                int dataProcessed = 0;
                string id_str = string.Empty;
                int id = 0;

                using (MERCY_Ctx db = new MERCY_Ctx())
                {
                    while (true)
                    {
                        if (dataProcessed >= 100) break;

                        id_str = OurUtility.ValueOf(p_collection, "RecordId" + dataProcessed);
                        id = OurUtility.ToInt32(id_str);
                        if (id == 0) break;

                        try
                        {
                            if (id < 0)
                            {
                                // -- Add Record
                                var data = new MaintenanceActivity
                                {
                                    Category = OurUtility.ValueOf(p_collection, "Category" + dataProcessed),
                                    Status = OurUtility.ValueOf(p_collection, "Status" + dataProcessed).Equals("1"),

                                    CreatedOn = DateTime.Now,
                                    CreatedBy = user.UserId
                                };

                                db.MaintenanceActivities.Add(data);
                                db.SaveChanges();
                            }
                            else
                            {
                                // -- Edit Record
                                var dataQuery =
                                       (
                                           from d in db.MaintenanceActivities
                                           where d.RecordId == id
                                           select d
                                       );

                                var data = dataQuery.SingleOrDefault();
                                if (data == null) continue;

                                data.Category = OurUtility.ValueOf(p_collection, "Category" + dataProcessed);
                                data.Status = OurUtility.ValueOf(p_collection, "Status" + dataProcessed).Equals("1");

                                data.LastModifiedOn = DateTime.Now;
                                data.LastModifiedBy = user.UserId;

                                db.SaveChanges();
                            }
                        }
                        catch {}

                        dataProcessed++;
                    }

                    // data Deleted
                    string deleted_ids_x = OurUtility.ValueOf(p_collection, "Deleted");
                    string[] deleted_ids = deleted_ids_x.Split(',');
                    foreach (string idx in deleted_ids)
                    {
                        if (string.IsNullOrEmpty(idx.Trim())) continue;

                        id = OurUtility.ToInt32(idx.Trim());
                        try
                        {
                            MaintenanceActivity data = db.MaintenanceActivities.Find(id);
                            db.MaintenanceActivities.Remove(data);
                            db.SaveChanges();
                        }
                        catch {}
                    }

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
                string companyCode = OurUtility.ValueOf(Request, "company").ToString();

                int? siteId = null;
                if (!string.IsNullOrEmpty(OurUtility.ValueOf(Request, "site")) && OurUtility.ValueOf(Request, "site") != "all")
                    siteId = int.Parse(OurUtility.ValueOf(Request, "site"));

                bool allCompany = OurUtility.ValueOf(Request, "company") == "all";
                bool allSite = siteId == null;

                bool onlyActive = string.IsNullOrEmpty(OurUtility.ValueOf(Request, "onlyActive")) || OurUtility.ValueOf(Request, "onlyActive").Equals("1");

                using (MERCY_Ctx db = new MERCY_Ctx())
                {
                    var allowedCompanies = db.UserCompanies.Where(u => u.UserId == user.UserId).Select(c => c.CompanyCode).ToList();
                    var allowedSites = db.UserSites.Where(u => u.UserId == user.UserId).Select(u => u.SiteId).ToList();

                    var dataQuery =
                            (
                                from d in db.MaintenanceActivities
                                where (onlyActive && d.Status == true || onlyActive == false) && 
                                      ((allCompany && allowedCompanies.Any(u => u == d.CompanyCode)) || d.CompanyCode == companyCode)
                                      && ((allSite && allowedSites.Any(u => u == d.SiteId)) || d.SiteId == siteId) 
                                orderby d.Category
                                select new
                                {
                                    id = d.RecordId
                                    , text = d.Category
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
    }
}