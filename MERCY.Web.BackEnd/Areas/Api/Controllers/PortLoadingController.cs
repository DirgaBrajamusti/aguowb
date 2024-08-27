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
using System.Data.SqlClient;

namespace MERCY.Web.BackEnd.Areas.Api.Controllers
{
    public class PortLoadingController : Controller
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

            string company = OurUtility.ValueOf(Request, "company");
            string period = OurUtility.ValueOf(Request, "period");

            // -- Actual code
            try
            {
                List<Model_View_Port_Loading> items = new List<Model_View_Port_Loading>();
                List<Model_Id_Text> companies = new List<Model_Id_Text>();

                using (MERCY_Ctx db = new MERCY_Ctx())
                {
                    var dataQuery =
                            (
                                from d in db.Companies
                                where d.IsActive
                                orderby d.Name
                                select new Model_Id_Text
                                {
                                    id = d.CompanyCode
                                    , text = d.Name
                                    , parent_id = d.SiteId.ToString()
                                }
                            );

                    companies = dataQuery.ToList();

                    // if Parameter:company is Empty
                    try
                    {
                        if (string.IsNullOrEmpty(company))
                        {
                            company = companies[0].id;
                        }
                    }
                    catch {}

                    // if Parameter:period is Empty
                    if (string.IsNullOrEmpty(period))
                    {
                        period = OurUtility.DateFormat(DateTime.Now, "yyyy-MM");
                    }

                    using (var connection = new SqlConnection(db.Database.Connection.ConnectionString))
                    {
                        connection.Open();

                        SqlCommand command = connection.CreateCommand();

                        command.CommandText = string.Format(@"
                                                            select * from
                                                            (
                                                                select p.ProductId, p.ProductName, p.CompanyCode
	                                                                , l.Periode,  l.Record_Id, 'budget' as Type_
	                                                                , l.Tonnage, l.CV, l.TS, l.ASH, l.IM, l.TM
                                                                from Product p
	                                                                left join Port_Loading l on p.ProductName = l.ProductName and l.Type = 'budget' and l.Periode = '{1}'
                                                                where p.CompanyCode = '{0}'
                                                                
                                                                union

                                                                select p.ProductId, p.ProductName, p.CompanyCode
	                                                                , l.Periode,  l.Record_Id, 'outlook' as Type_
	                                                                , l.Tonnage, l.CV, l.TS, l.ASH, l.IM, l.TM
                                                                from Product p
	                                                                left join Port_Loading l on p.ProductName = l.ProductName and l.Type = 'outlook' and l.Periode = '{1}'
                                                                where p.CompanyCode = '{0}'
                                                            ) x order by Type_, ProductName"
                                                            , company
                                                            , period);

                        Model_View_Port_Loading item = null;
                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                item = new Model_View_Port_Loading
                                {
                                    ProductId = OurUtility.ValueOf(reader, "ProductId"),
                                    ProductName = OurUtility.ValueOf(reader, "ProductName"),
                                    CompanyCode = OurUtility.ValueOf(reader, "CompanyCode"),
                                    Periode = OurUtility.ValueOf(reader, "Periode"),
                                    RecordId = OurUtility.ValueOf(reader, "Record_Id"),
                                    Type = OurUtility.ValueOf(reader, "Type_"),
                                    Tonnage = OurUtility.ValueOf(reader, "Tonnage"),
                                    CV = OurUtility.ValueOf(reader, "CV"),
                                    TS = OurUtility.ValueOf(reader, "TS"),
                                    ASH = OurUtility.ValueOf(reader, "ASH"),
                                    IM = OurUtility.ValueOf(reader, "IM"),
                                    TM = OurUtility.ValueOf(reader, "TM")
                                };

                                if (string.IsNullOrEmpty(item.RecordId)) item.RecordId = "-1";
                                if (string.IsNullOrEmpty(item.Tonnage)) item.Tonnage = "0";
                                if (string.IsNullOrEmpty(item.CV)) item.CV = "0";
                                if (string.IsNullOrEmpty(item.TS)) item.TS = "0.00";
                                if (string.IsNullOrEmpty(item.ASH)) item.ASH = "0.00";
                                if (string.IsNullOrEmpty(item.IM)) item.IM = "0.00";
                                if (string.IsNullOrEmpty(item.TM)) item.TM = "0.00";

                                items.Add(item);
                            }
                        }

                        connection.Close();
                    }
                }

                var result = new { Success = true, Permission = permission_Item, User = UserX.Information(user, is_Show_User_Menu, is_Show_User_Relation), Message = string.Empty + " - " + company, MessageDetail = string.Empty, Version = Configuration.VERSION, Items = items, Total = items.Count, Companies = companies };
                return Json(result, JsonRequestBehavior.AllowGet);
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
                    var data = new Site
                    {
                        SiteName = OurUtility.ValueOf(p_collection, "name"),
                        IsActive = OurUtility.ValueOf(p_collection, "isactive").Equals("1"),

                        CreatedOn = DateTime.Now,
                        CreatedBy = user.UserId
                    };

                    db.Sites.Add(data);
                    db.SaveChanges();

                    long id = data.SiteId;

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

            // -- Actual code
            try
            {
                long id = OurUtility.ToInt64(Request[".id"]);

                using (MERCY_Ctx db = new MERCY_Ctx())
                {
                    Port_Loading data = null;

                    if (id == -1)
                    {
                        data = new Port_Loading
                        {
                            CreatedOn = DateTime.Now,
                            CreatedBy = user.UserId,

                            Type = OurUtility.ValueOf(p_collection, "Type"),
                            CompanyCode = OurUtility.ValueOf(p_collection, "company"),
                            Periode = OurUtility.ValueOf(p_collection, "period"),
                            ProductId = OurUtility.ToInt32(OurUtility.ValueOf(p_collection, "ProductId")),
                            ProductName = OurUtility.ValueOf(p_collection, "Product")
                        };
                    }
                    else
                    {
                        var dataQuery =
                            (
                                from d in db.Port_Loading
                                where d.Record_Id == id
                                select d
                            );

                        data = dataQuery.SingleOrDefault();

                        data.LastModifiedOn = DateTime.Now;
                        data.LastModifiedBy = user.UserId;
                    }

                    data.Tonnage =  OurUtility.ToDouble(OurUtility.ValueOf(p_collection, "Tonnage"));
                    data.CV = OurUtility.ToDouble(OurUtility.ValueOf(p_collection, "CV"));
                    data.TS = OurUtility.ToDecimal(OurUtility.ValueOf(p_collection, "TS"));
                    data.ASH = OurUtility.ToDecimal(OurUtility.ValueOf(p_collection, "ASH"));
                    data.IM = OurUtility.ToDecimal(OurUtility.ValueOf(p_collection, "IM"));
                    data.TM = OurUtility.ToDecimal(OurUtility.ValueOf(p_collection, "TM"));

                    if (id == -1)
                    {
                        db.Port_Loading.Add(data);
                    }

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
                long id = OurUtility.ToInt64(Request[".id"]);

                using (MERCY_Ctx db = new MERCY_Ctx())
                {
                    var dataQuery =
                        (
                            from d in db.Sites
                            where d.SiteId == id
                            select d
                        );

                    var data = dataQuery.SingleOrDefault();
                    db.Sites.Remove(data);

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
                                from d in db.Sites
                                where d.IsActive
                                orderby d.SiteName
                                select new
                                {
                                    id = d.SiteId
                                    , text = d.SiteName
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
                                var data = new Site
                                {
                                    SiteName = OurUtility.ValueOf(p_collection, "Name" + dataProcessed),
                                    IsActive = OurUtility.ValueOf(p_collection, "Status" + dataProcessed).Equals("1"),

                                    CreatedOn = DateTime.Now,
                                    CreatedBy = user.UserId
                                };

                                db.Sites.Add(data);
                                db.SaveChanges();
                            }
                            else
                            {
                                // -- Edit Record
                                var dataQuery =
                                       (
                                           from d in db.Sites
                                           where d.SiteId == id
                                           select d
                                       );

                                var data = dataQuery.SingleOrDefault();
                                if (data == null) continue;

                                data.SiteName = OurUtility.ValueOf(p_collection, "Name" + dataProcessed);
                                data.IsActive = OurUtility.ValueOf(p_collection, "Status" + dataProcessed).Equals("1");

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
                            Site data = db.Sites.Find(id);
                            db.Sites.Remove(data);
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
    }
}