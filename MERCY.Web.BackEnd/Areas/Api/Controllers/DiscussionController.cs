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
    public class DiscussionController : Controller
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

            bool isAll_Text = true;
            string txt = OurUtility.ValueOf(Request, "txt");
            isAll_Text = string.IsNullOrEmpty(txt);

            // -- Actual code
            try
            {
                using (MERCY_Ctx db = new MERCY_Ctx())
                {
                    var dataQuery =
                            (
                                from d in db.Feedbacks
                                    join u in db.Users on d.CreatedBy equals u.UserId
                                where (isAll_Text || u.FullName.Contains(txt))
                                orderby d.CreatedOn descending, u.FullName
                                select new Model_View_Feedback
                                {
                                    FeedbackId = d.FeedbackId
                                    , Accuracy = d.Accuracy
                                    , Objectivity = d.Objectivity
                                    , EasyToUnderstand = d.EasyToUnderstand
                                    , Detailed = d.Detailed
                                    , Punctuality = d.Punctuality
                                    , Remark = d.Remark
                                    , CreatedOn = d.CreatedOn
                                    , FullName = u.FullName
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

            // -- Not necessary checking Permission
            Permission.Check_API(Request, user, ref permission_Item);
            if ( ! permission_Item.Is_View)
            {
                var msg = new { Success = false, Permission = permission_Item, Message = Permission.ERROR_PERMISSION_READ, MessageDetail = string.Empty, Version = Configuration.VERSION };
                return Json(msg, JsonRequestBehavior.AllowGet);
            }

            // -- Actual code
            try
            {
                string page = OurUtility.ValueOf(p_collection, "page");

                using (MERCY_Ctx db = new MERCY_Ctx())
                {
                    var data = new Discussion
                    {
                        Page = page,
                        ReferenceId = OurUtility.ToInt64(OurUtility.ValueOf(p_collection, "r")),
                        Typeof = OurUtility.ValueOf(p_collection, "typeof"),
                        Remark = OurUtility.ValueOf(p_collection, "remark"),
                        Attached = string.Empty,
                        Attached2 = string.Empty
                    };

                    if (data.Typeof.ToLower() == "file")
                    {
                        string msg2 = string.Empty;
                        string attached = string.Empty;
                        string attached2 = string.Empty;

                        OurUtility.Upload(Request, "file", UploadFolder, ref attached, ref attached2, ref msg2);

                        data.Attached = attached + msg2;
                        data.Attached2 = attached2;
                    }

                    data.CreatedOn = DateTime.Now;
                    data.CreatedBy = user.UserId;

                    db.Discussions.Add(data);
                    db.SaveChanges();

                    long id = data.RecordId;
                    string a = string.Empty;
                    var data_Discussions = Get(db, page, data.ReferenceId, ref a);

                    var result = new { Success = true, Permission = permission_Item, A = a, Discussions = data_Discussions, Message = BaseConstants.MESSAGE_CREATE_SUCCESS, MessageDetail = string.Empty, Version = Configuration.VERSION, Data = id };
                    return Json(result, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                var result = new { Success = false, Permission = permission_Item, ex.Message, MessageDetail = string.Empty, Version = Configuration.VERSION, Items = string.Empty, Total = 0 };
                return Json(result, JsonRequestBehavior.AllowGet);
            }
        }

        internal static List<Model_View_Discussion> Get(string p_page, long p_referenceId)
        {
            List<Model_View_Discussion> result = null;

            try
            {
                using (MERCY_Ctx db = new MERCY_Ctx())
                {
                    string msg = string.Empty;
                    return Get(db, p_page, p_referenceId, ref msg);
                }
            }
            catch {}

            return result;
        }

        private static List<Model_View_Discussion> Get(MERCY_Ctx p_db, string p_page, long p_referenceId, ref string p_message)
        {
            List<Model_View_Discussion> result = null;

            p_message = string.Empty;

            try
            {
                var dataQuery =
                        (
                            from d in p_db.Discussions
                            join u in p_db.Users on d.CreatedBy equals u.UserId
                            where d.Page == p_page && d.ReferenceId == p_referenceId
                            orderby d.RecordId descending // d.CreatedOn, u.FullName
                            select new Model_View_Discussion
                            {
                                RecordId = d.RecordId
                                , Typeof = d.Typeof
                                , Remark = d.Remark
                                , Attached = d.Attached
                                , Attached2 = d.Attached2
                                , CreatedOn = d.CreatedOn
                                , FullName = u.FullName
                            }
                        );

                result = dataQuery.ToList();
                try
                {
                    result.ForEach(c =>
                    {
                        c.CreatedOn_Str = OurUtility.DateFormat(c.CreatedOn, @"dd-MMM-yyyy HH:mm:ss");
                        c.CreatedOn_Str2 = OurUtility.DateFormat(c.CreatedOn, "dd-MMM-yyyy");
                    });
                }
                catch {}
            }
            catch (Exception ex)
            {
                p_message = ex.Message;
            }

            return result;
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
    }
}