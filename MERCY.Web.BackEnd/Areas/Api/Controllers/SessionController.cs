using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Web.Mvc;
using MERCY.Data.EntityFramework;
using MERCY.Web.BackEnd.Helpers;
using MERCY.Web.BackEnd.Models;
using MERCY.Web.BackEnd.Security;

namespace MERCY.Web.BackEnd.Areas.Api.Controllers
{
    public class SessionController : Controller
    {
        // GET: Api/Session/ClaimSession
        public JsonResult ClaimSession([Bind(Prefix = "menus[]")] List<string> menus)
        {
            // Check Permission based on Current Url
            UserX user = new UserX(Request);
            Permission_Item permission_Item = null;

            if (user.UserId == 0)
            {
                var msg = new { Success = false, Permission = permission_Item, Message = "You need to be logged in to claim session", MessageDetail = string.Empty, Version = Configuration.VERSION };
                return Json(msg, JsonRequestBehavior.AllowGet);
            }

            bool is_Show_User_Menu = OurUtility.ValueOf(Request, "u_menu").Equals("1");
            bool is_Show_User_Relation = OurUtility.ValueOf(Request, "u_relation").Equals("1");

            using (MERCY_Ctx db = new MERCY_Ctx())
            {
                var transaction = db.Database.BeginTransaction();
                try
                {
                    Configuration config = new Configuration();

                    menus.ForEach(menu =>
                    {
                        string otherUserName = string.Empty;
                        var session = db.Sessions.Where(s => s.Menu == menu).FirstOrDefault();
                        // menu session not exist
                        if (session == null)
                        {
                            session = new Session
                            {
                                Menu = menu,
                                Token = SessionHelper.GenerateJwtToken(user.UserId, config.Session_Security_Key, DateTime.Now.AddMinutes(OurUtility.ToInt32(config.Session_Expired_Time_in_Minutes))),
                                CreatedBy = user.UserId,
                                CreatedOn = DateTime.Now,
                            };
                            db.Sessions.Add(session);
                        }
                        else
                        {
                            if (session.Token != null)
                            {
                                try
                                {
                                    var principal = SessionHelper.VerifyToken(session.Token, config.Session_Security_Key);
                                    var userId = OurUtility.ToInt32(principal.FindFirst(ClaimTypes.NameIdentifier).Value);
                                    if (userId != user.UserId)
                                    {
                                        var otherUser = db.Users.Find(userId);
                                        otherUserName = otherUser.FullName;

                                        throw new Exception($"{otherUserName} is accessing this page.");
                                    }
                                }
                                catch (Exception e)
                                {
                                    if (e.Message == "Invalid Token" || e.Message == $"{otherUserName} is accessing this page.")
                                    {
                                        throw e;
                                    }
                                }
                            }
                        }

                        session.Token = SessionHelper.GenerateJwtToken(user.UserId, config.Session_Security_Key, DateTime.Now.AddMinutes(OurUtility.ToInt32(config.Session_Expired_Time_in_Minutes)));
                        session.LastModifiedBy = user.UserId;
                        session.LastModifiedOn = DateTime.Now;

                        db.SaveChanges();
                    });

                    var item = new
                    {
                        SessionExpiredTime = DateTime.Now.AddMinutes(OurUtility.ToInt32(config.Session_Expired_Time_in_Minutes)).ToString("yyyy-MM-dd HH:mm:sszzz"),
                        ExtendSessionTime = DateTime.Now.AddMinutes(OurUtility.ToInt32(config.Session_Expired_Time_in_Minutes) - 1).ToString("yyyy-MM-dd HH:mm:sszzz")
                    };

                    transaction.Commit();
                    var result = new { Success = true, Permission = permission_Item, User = UserX.Information(user, is_Show_User_Menu, is_Show_User_Relation), Message = string.Empty, MessageDetail = string.Empty, Version = Configuration.VERSION, Items = item, Total = 0 };
                    return Json(result, JsonRequestBehavior.AllowGet);
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    transaction.Dispose();

                    var result = new { Success = false, Permission = permission_Item, ex.Message, MessageDetail = string.Empty, Version = Configuration.VERSION, Items = string.Empty, Total = 0 };
                    return Json(result, JsonRequestBehavior.AllowGet);
                }
            }
        }

        // GET: Api/Session/CloseSession
        public JsonResult CloseSession([Bind(Prefix = "menus[]")] List<string> menus)
        {
            // Check Permission based on Current Url
            UserX user = new UserX(Request);
            Permission_Item permission_Item = null;

            if (user.UserId == 0)
            {
                var msg = new { Success = false, Permission = permission_Item, Message = "You need to be logged in to close session", MessageDetail = string.Empty, Version = Configuration.VERSION };
                return Json(msg, JsonRequestBehavior.AllowGet);
            }

            bool is_Show_User_Menu = OurUtility.ValueOf(Request, "u_menu").Equals("1");
            bool is_Show_User_Relation = OurUtility.ValueOf(Request, "u_relation").Equals("1");

            Configuration config = new Configuration();
            using (MERCY_Ctx db = new MERCY_Ctx())
            {
                var transaction = db.Database.BeginTransaction();
                try
                {
                    menus.ForEach(menu =>
                    {
                        string otherUserName = string.Empty;
                        var session = db.Sessions.Where(s => s.Menu == menu).FirstOrDefault();
                        // menu session not exist
                        if (session == null)
                        {
                            throw new Exception("Session not found.");
                        }

                        if (session.Token == null)
                        {
                            throw new Exception("Session was not claimed.");
                        }

                        var principal = SessionHelper.VerifyToken(session.Token, config.Session_Security_Key);
                        var userId = OurUtility.ToInt32(principal.FindFirst(ClaimTypes.NameIdentifier).Value);
                        if (userId != user.UserId)
                        {
                            var otherUser = db.Users.Find(userId);
                            otherUserName = otherUser.FullName;

                            throw new Exception($"{otherUserName} is accessing this page.");
                        }

                        session.Token = null;
                        session.LastModifiedBy = user.UserId;
                        session.LastModifiedOn = DateTime.Now;

                        db.SaveChanges();
                    });

                    transaction.Commit();
                    var result = new { Success = true, Permission = permission_Item, User = UserX.Information(user, is_Show_User_Menu, is_Show_User_Relation), Message = string.Empty, MessageDetail = string.Empty, Version = Configuration.VERSION, Items = string.Empty, Total = 0 };
                    return Json(result, JsonRequestBehavior.AllowGet);
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    transaction.Dispose();
                    var result = new { Success = false, Permission = permission_Item, ex.Message, MessageDetail = string.Empty, Version = Configuration.VERSION, Items = string.Empty, Total = 0 };
                    return Json(result, JsonRequestBehavior.AllowGet);
                }
            }
        }
    }
}