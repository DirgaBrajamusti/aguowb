using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

using MERCY.Data.EntityFramework;
using MERCY.Web.BackEnd.Models;
using MERCY.Web.BackEnd.Helpers;
using MERCY.Web.BackEnd.Security;
using Permission = MERCY.Web.BackEnd.Security.Permission;

using System.Data;

namespace MERCY.Web.BackEnd.Areas.Api.Controllers
{
    public class BargeLineUpMappingController : Controller
    {
        public JsonResult Index()
        {
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

            var sheet = OurUtility.ValueOf(Request, "sheet");
            try
            {
                using (MERCY_Ctx db = new MERCY_Ctx())
                {
                    var dataQuery =
                            (
                                from b in db.BargeLineUpMappings
                                where b.Sheet == sheet
                                select new
                                {
                                    b.Id,
                                    b.DataRequired,
                                    b.TemplateColumn,
                                    b.DataKey,
                                    b.Sheet
                                }
                            );

                    var items = dataQuery.ToList();

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

        public JsonResult Create(List<BargeLineUpMapping> bargeMappingList)
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

            try
            {
                using (MERCY_Ctx db = new MERCY_Ctx())
                {
                    foreach(var item in bargeMappingList)
                    {
                        var bargeMapping = new BargeLineUpMapping();
                        bargeMapping.DataRequired = item.DataRequired;
                        bargeMapping.TemplateColumn = item.TemplateColumn;
                        bargeMapping.Sheet = item.Sheet;
                        bargeMapping.DataKey = item.DataRequired.ToUpper().Replace(" ", "_");
                        bargeMapping.CreatedBy = user.UserId;
                        bargeMapping.CreatedAt = DateTime.Now;

                        db.BargeLineUpMappings.Add(bargeMapping);
                    }
                    db.SaveChanges();
                }
            }
            catch {}

            var msg = new { Success = true, Permission = permission_Item, Message = string.Empty, MessageDetail = string.Empty, Version = Configuration.VERSION };
            return Json(msg, JsonRequestBehavior.AllowGet);
        }

        public JsonResult Edit(List<BargeLineUpMapping> bargeMappingList)
        {
            // Check Permission based on Current Url
            UserX user = new UserX(Request);
            Permission_Item permission_Item = null;

            Permission.Check_API(Request, user, ref permission_Item);
            if (!permission_Item.Is_Edit)
            {
                var msgX = new { Success = false, Permission = permission_Item, Message = Permission.ERROR_PERMISSION_READ, MessageDetail = string.Empty, Version = Configuration.VERSION };
                return Json(msgX, JsonRequestBehavior.AllowGet);
            }

            try
            {
                using (MERCY_Ctx db = new MERCY_Ctx())
                {
                    foreach (var item in bargeMappingList)
                    {
                        var bargeLineUpMapping = db.BargeLineUpMappings.Find(item.Id);
                        bargeLineUpMapping.TemplateColumn = item.TemplateColumn;
                    }
                    db.SaveChanges();
                }
            }
            catch { }

            var msg = new { Success = true, Permission = permission_Item, Message = string.Empty, MessageDetail = string.Empty, Version = Configuration.VERSION };
            return Json(msg, JsonRequestBehavior.AllowGet);
        }
    }
}