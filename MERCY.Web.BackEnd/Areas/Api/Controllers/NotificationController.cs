using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

using System.IO;
using System.Text;
using System.Net.Mail;

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
    public class NotificationController : Controller
    {
        public void InitializeController(System.Web.Routing.RequestContext context)
        {
            base.Initialize(context);
        }

        public string Index()
        {
            return (new TestController()).Index();
        }

        public string TemplateFolder
        {
            get
            {
                string result = @"c:\temp";

                try
                {
                    result = Server.MapPath("~") + @"\templates";
                    result = result.Replace(@"\\", @"\");
                }
                catch { }

                return result;
            }
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
                catch { }

                return result;
            }
        }

        public string Send()
        {
            // Check Permission based on Current Url
            UserX user = new UserX(Request);

            // -- Not necessary checking Permission
            //Permission.Check_API(Request, user, ref permission_Item);
            // -- just Logging User: is enough
            /*TODO:if (user.UserId <= 0)
            {
                var msg = new { Success = false, Permission = permission_Item, Message = Permission.ERROR_PERMISSION_READ + " [not Login]", MessageDetail = string.Empty, Version = Configuration.VERSION };
                return Permission.ERROR_PERMISSION_READ + " [not Login]";
            }*/

            if (Request["type"] == "Hauling")
            {
                return Send_Hauling();
            }

            if (Request["type"] == "Feedback")
            {
                return Send_Feedback();
            }

            string notificationType = "Sampling";
            if (Request["type"] == "Analysis")
            {
                notificationType = "Analysis";
            }

            string url_link = Request["requestFrom"];
            if (string.IsNullOrEmpty(url_link))
            {
                url_link = OurUtility.Url(Request);
            }

            string url_image = url_link + @"/Api/Notification/Picture?f=";
            string url_detail = url_link + @"/SamplingRequestv";

            Configuration config = new Configuration();
            long id = OurUtility.ToInt64(Request["request"]);
            string data_company = string.Empty;
            string data_user_Fullname = string.Empty;
            string data_user_Email_Requestor = string.Empty;
            string data_type = string.Empty;
            string email_to = string.Empty;
            string email_subject = config.Email_Sampling_Subject;
            if (notificationType == "Analysis")
            {
                email_subject = config.Email_Analysis_Subject;
                url_detail = url_link + @"/AnalysisRequestv";
            }

            email_subject = email_subject.Replace("{ID}", id.ToString());

            try
            {
                if (notificationType == "Sampling")
                {
                    using (MERCY_Ctx db = new MERCY_Ctx())
                    {
                        var dataQuery =
                                (
                                    from dt in db.SamplingRequests
                                    join usr in db.Users on dt.CreatedBy equals usr.UserId
                                    where dt.SamplingRequestId == id
                                    select new
                                    {
                                        dt.Company
                                        ,
                                        dt.SamplingType
                                        ,
                                        usr.FullName
                                        ,
                                        usr.Email
                                        ,
                                        dt.HAC_Text
                                        ,
                                        dt.SamplingRequestId
                                    }
                                );

                        var data = dataQuery.SingleOrDefault();

                        data_company = data.Company;
                        data_user_Fullname = data.FullName;
                        data_user_Email_Requestor = data.Email;
                        data_type = data.SamplingType;

                        email_subject = email_subject.Replace("{COMPANY}", data_company)
                                                    .Replace("{SAMPLING_TYPE}", data.SamplingType)
                                                    .Replace("{LOCATION}", Get_Location(db, data_company, data.SamplingType, data.HAC_Text, data.SamplingRequestId))
                                                    ;
                    }
                }
                else
                {
                    using (MERCY_Ctx db = new MERCY_Ctx())
                    {
                        var dataQuery =
                                (
                                    from dt in db.AnalysisRequests
                                    join usr in db.Users on dt.CreatedBy equals usr.UserId
                                    where dt.AnalysisRequestId == id
                                    && usr.IsActive == true
                                    select new
                                    {
                                        dt.Company
                                        ,
                                        dt.AnalysisType
                                        ,
                                        usr.FullName
                                        ,
                                        usr.Email
                                        ,
                                        dt.LetterNo
                                    }
                                );

                        var data = dataQuery.SingleOrDefault();

                        data_company = data.Company;
                        data_user_Fullname = data.FullName;
                        data_user_Email_Requestor = data.Email;
                        data_type = data.AnalysisType;

                        email_subject = email_subject.Replace("{COMPANY}", data_company)
                                                    .Replace("{ANALYSIS_TYPE}", data_type)
                                                    .Replace("{LETTER_NO}", data.LetterNo)
                                                    ;
                    }
                }
            }
            catch { }

            string email_body = "Dear user,";
            email_body = System.IO.File.ReadAllText(TemplateFolder + @"\email_template.html", Encoding.UTF8);

            int email_count = 0;
            string email_status_sending = string.Empty;

            using (MERCY_Ctx db = new MERCY_Ctx())
            {
                var dataQuery =
                        (
                            from usr in db.Users
                            join uc in db.UserCompanies on usr.UserId equals uc.UserId
                            where usr.IsUserLab 
                            && (uc.CompanyCode == data_company || data_company == string.Empty)
                            select new
                            {
                                usr.FullName
                                ,
                                usr.Email
                            }
                        );

                var data = dataQuery.ToList();


                try
                {
                    string email_address_UserLab = string.Empty;
                    string email_Fullname = string.Empty;

                    data.ForEach(c =>
                    {
                        if (!string.IsNullOrEmpty(c.Email))
                        {
                            email_count++;

                            email_address_UserLab += (email_count > 1 ? "," : "") + c.Email;
                            email_Fullname += (email_count > 1 ? "," : "") + c.FullName;
                        }
                    });

                    if (notificationType == "Sampling")
                    {
                        email_body = email_body.Replace("{MERCY_TYPE}", "Sampling Request");
                    }
                    else
                    {
                        email_body = email_body.Replace("{MERCY_TYPE}", "Analysis Request");
                    }

                    email_body = email_body.Replace("{Username}", email_Fullname);
                    email_body = email_body.Replace("{MERCY_USER}", data_user_Fullname);
                    email_body = email_body.Replace("{MERCY_COMPANY}", data_company);
                    email_body = email_body.Replace("{MERCY_ID}", id.ToString());
                    email_body = email_body.Replace("{MERCY_DATA_TYPE}", data_type);
                    email_body = email_body.Replace("{MERCY_IMAGE_URL}", url_image);
                    email_body = email_body.Replace("{MERCY_DETAIL}", url_detail);

                    email_status_sending += ", " + Send(string.Empty, email_subject, email_address_UserLab, data_user_Email_Requestor, email_body);
                }
                catch { }
            }

            var result_Object = new { Success = true, Message = "Sending " + email_count.ToString() + " email" + (email_count > 0 ? "s" : ""), Time = DateTime.Now.ToString("dd-MMM-yyyy HH:mm:ss.ffff"), Message_Email = email_status_sending };

            return JsonConvert.SerializeObject(result_Object);
        }

        internal JsonResult Send(string p_emailFrom, string p_subject, string p_to, string p_cc, string p_body, Attachment p_attachment)
        {
            bool success = false;
            string msg = string.Empty;

            try
            {
                Configuration config = new Configuration();

                string email_server = config.Email_Server;
                int port = OurUtility.ToInt32(config.Email_Port);
                string email_user = (string.IsNullOrEmpty(p_emailFrom) ? config.Email_User : p_emailFrom);
                string email_password = config.Email_Password;

                MailMessage message = new MailMessage();
                SmtpClient smtpClient = new SmtpClient();

                MailAddress fromAddress = new MailAddress(email_user);
                message.From = fromAddress;

                // make sure, There is NO Space
                p_to = p_to.Replace(" ", "");
                // handling ; and ,
                p_to = p_to.Replace(";", ",");
                foreach (var address in p_to.Split(new[] { "," }, StringSplitOptions.RemoveEmptyEntries))
                {
                    try
                    {
                        message.To.Add(address);
                    }
                    catch { }
                }

                if (!string.IsNullOrEmpty(p_cc))
                {
                    // make sure, There is NO Space
                    p_cc = p_cc.Replace(" ", "");
                    // handling ; and ,
                    p_cc = p_cc.Replace(";", ",");
                    foreach (var address in p_cc.Split(new[] { "," }, StringSplitOptions.RemoveEmptyEntries))
                    {
                        try
                        {
                            message.CC.Add(address);
                        }
                        catch { }
                    }
                }

                message.Subject = p_subject;
                message.IsBodyHtml = true;
                message.Body = p_body;

                // We use gmail as our smtp client
                smtpClient.Host = email_server;
                if (port > 0) smtpClient.Port = port;
                if (port != 25)
                {
                    smtpClient.EnableSsl = true;
                }

                if (!string.IsNullOrEmpty(email_password))
                {
                    smtpClient.UseDefaultCredentials = true;
                    smtpClient.Credentials = new System.Net.NetworkCredential(email_user, email_password);
                }

                if (p_attachment != null)
                {
                    message.Attachments.Add(p_attachment);
                }

                smtpClient.Send(message);

                success = true;
                msg = "Success sending e-mail.";
            }
            catch (Exception ex)
            {
                msg = ex.Message;
                success = false;
            }

            var result = new { Success = success, Message = msg, Time = DateTime.Now.ToString("dd-MMM-yyyy HH:mm:ss.ffff") };
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        internal JsonResult Send(string p_emailFrom, string p_subject, string p_to, string p_cc, string p_body)
        {
            return Send(p_emailFrom, p_subject, p_to, p_cc, p_body, null);
        }

        public ActionResult Picture()
        {
            var dir = Server.MapPath("/templates");
            var path = string.Empty;

            string file = Request["f"];
            try
            {
                path = Path.Combine(dir, file);
                return base.File(path, "image/jpeg");
            }
            catch { }

            return null;
        }

        public string Get()
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
                return Permission.ERROR_PERMISSION_READ;
            }

            bool success = false;
            string message = string.Empty;
            string count = "0";

            string requestType = "Sampling";
            string request_Table = "SamplingRequest";
            string request_Column = "SamplingRequestId";

            if (Request["type"] == "Analysis")
            {
                requestType = "Analysis";
                request_Table = "AnalysisRequest";
                request_Column = "AnalysisRequestId";
            }

            try
            {
                using (MERCY_Ctx db = new MERCY_Ctx())
                {
                    using (var connection = new SqlConnection(db.Database.Connection.ConnectionString))
                    {
                        connection.Open();

                        SqlCommand command = connection.CreateCommand();

                        string sql = string.Format(@"
                                                    select Count(*) as Count2 from {0}
                                                    where {1} not in
                                                    (
                                                    select RequestId from ReadRequest where RequestType = '{2}'and CreatedBy = {3}
                                                    )
                                                    ", request_Table, request_Column, requestType, user.UserId);

                        command.CommandText = sql;

                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                count = reader["Count2"].ToString();

                                success = true;
                                message = string.Empty;
                            }
                        }

                        connection.Close();
                    }
                }
            }
            catch (Exception ex)
            {
                message = ex.Message;
            }

            var result_Object = new { Success = success, Count = count, Message = message, Time = DateTime.Now.ToString("dd-MMM-yyyy HH:mm:ss.ffff") };

            return JsonConvert.SerializeObject(result_Object);
        }


        public JsonResult GetAll()
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

            bool success = false;
            string message = string.Empty;
            string count = "0";
            string count_sampling = "0";
            string count_analysis = "0";

            string requestType = "Sampling";
            string request_Table = "SamplingRequest";
            string request_Column = "SamplingRequestId";

            try
            {
                using (MERCY_Ctx db = new MERCY_Ctx())
                {
                    using (var connection = new SqlConnection(db.Database.Connection.ConnectionString))
                    {
                        connection.Open();

                        SqlCommand command = connection.CreateCommand();

                        string sql = string.Format(@"
                                                    select Count(*) as Count2 from {0}
                                                    where {1} not in
                                                    (
                                                    select RequestId from ReadRequest where RequestType = '{2}'and CreatedBy = {3}
                                                    )
                                                    ", request_Table, request_Column, requestType, user.UserId);

                        command.CommandText = sql;

                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                count_sampling = reader["Count2"].ToString();
                            }
                        }

                        requestType = "Analysis";
                        request_Table = "AnalysisRequest";
                        request_Column = "AnalysisRequestId";

                        sql = string.Format(@"
                                                    select Count(*) as Count2 from {0}
                                                    where {1} not in
                                                    (
                                                    select RequestId from ReadRequest where RequestType = '{2}'and CreatedBy = {3}
                                                    )
                                                    ", request_Table, request_Column, requestType, user.UserId);

                        command.CommandText = sql;

                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                count_analysis = reader["Count2"].ToString();
                            }
                        }

                        success = true;
                    }
                }
            }
            catch { }

            count = "[ " + count_sampling + " / " + count_analysis + " ]";

            var result = new { Success = success, Count = count, Message = message, Time = DateTime.Now.ToString("dd-MMM-yyyy HH:mm:ss.ffff") };
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        public string ReadRequest()
        {
            // Check Permission based on Current Url
            UserX user = new UserX(Request);

            // -- Not necessary checking Permission
            //Permission.Check_API(Request, user, ref permission_Item);
            // -- just Logging User: is enough
            if (user.UserId <= 0)
            {
                //var msg = new { Success = false, Permission = permission_Item, Message = Permission.ERROR_PERMISSION_READ + " [not Login]", MessageDetail = string.Empty, Version = Configuration.VERSION };
                return Permission.ERROR_PERMISSION_READ + " [not Login]";
            }

            bool success = false;
            string msg = string.Empty;

            string requestType = "Sampling";
            if (Request["type"] == "Analysis")
            {
                requestType = "Analysis";
            }

            long id = OurUtility.ToInt64(Request["request"]);

            try
            {
                using (MERCY_Ctx db = new MERCY_Ctx())
                {
                    var data = new ReadRequest
                    {
                        RequestType = requestType,
                        RequestId = id,

                        CreatedOn = DateTime.Now,
                        CreatedBy = user.UserId
                    };

                    db.ReadRequests.Add(data);
                    db.SaveChanges();

                    success = true;
                }
            }
            catch { }

            var result_Object = new { Success = success, Message = msg, Time = DateTime.Now.ToString("dd-MMM-yyyy HH:mm:ss.ffff") };

            return JsonConvert.SerializeObject(result_Object);
        }

        private string EmailDomainHandling(string email)
        {
            Configuration config = new Configuration();

            if (email == string.Empty)
            {
                return email;
            }
            else
            {
                MailAddress fromAddress = new MailAddress(email);
                var domains = config.Email_User_Domain.Split(new[] { "," }, StringSplitOptions.RemoveEmptyEntries);
                if (!domains.Any(u => u == fromAddress.Host))
                {
                    email = string.Empty;
                }
            }

            return email;
        }
        private string Send_Hauling()
        {
            Configuration config = new Configuration();
            long id = OurUtility.ToInt64(Request["request"]);

            string email_to = string.Empty;
            string email_cc = string.Empty;
            string email_subject = config.Email_Hauling_Subject;
            string contentEmail = "{EmailBody}";

            int site = string.IsNullOrEmpty(Request["siteId"]) ? 0 : OurUtility.ToInt32(Request["siteId"]);
            string company = string.Empty;
            string email_To_Menu = config.Notification_Hauling_Request_Email_To_Menu;
            string email_To_Access = config.Notification_Hauling_Request_Email_To_Access;
            string email_Cc_Access = config.Notification_Hauling_Request_Email_Cc_Access;

            // -- this is Production stage
            // To based on "Calculation"
            List<Model_View_UserList_access_Menu> userList_To_Menu =
                Notification3Controller.UserList_access_Menu(email_To_Menu, email_To_Access, company, site);

            List<Model_View_UserList_access_Menu> userList_Cc_Menu =
                Notification3Controller.UserList_access_Menu(email_To_Menu, email_Cc_Access, company, site);

            // To based on "Calculation" above
            var real_user_email_to = Notification3Controller.EmailAddress(userList_To_Menu, true);

            // Cc based on "Calculation" above
            var real_user_email_cc = Notification3Controller.EmailAddress(userList_Cc_Menu, true);

            // check development stages
            if (config.Is_Production)
            {
                // To based on "Calculation" above
                email_to = real_user_email_to;

                // Cc based on "Calculation" above
                email_cc = real_user_email_cc;
            }
            else
            {
                // -- Not Production
                // send email base on config
                email_to = config.Email_Hauling_User;
                email_cc = config.Email_Hauling_User_CC;

                // give information in Body of Email
                var optionalHeader = "---------------------------------------------------------------------"
                         + "<br/>This message was sent from development environment."
                         + "<br/>Email To: " + real_user_email_to
                         + "<br/><br/>Email Cc: "+ "{EmailRequestor}," + real_user_email_cc
                         + "<br/>---------------------------------------------------------------------"
                         + "<br/><br/>" + "{EmailBody}";
                contentEmail = contentEmail.Replace("{EmailBody}", optionalHeader);
            }

            email_subject = email_subject.Replace("{ID}", id.ToString());

            string url_link = Request["requestFrom"];
            if (string.IsNullOrEmpty(url_link))
            {
                url_link = OurUtility.Url(Request);
            }

            string email_body = "Dear user,";
            email_body = System.IO.File.ReadAllText(TemplateFolder + @"\email_template_hauling.html", Encoding.UTF8);
            email_body = email_body.Replace("{ID}", id.ToString());
            email_body = email_body.Replace("{URL}", url_link);

            int email_count = 0;
            string content = string.Empty;
            string email_Requestor = string.Empty;
            string email_subject_Shift = string.Empty;
            DateTime dateFrom = DateTime.Now;
            DateTime dateTo = DateTime.Now;
            string msg = string.Empty;

            HaulingController hauling = new HaulingController();
            hauling.Get(id, true, true, ref content, ref email_Requestor, ref dateFrom, ref dateTo, ref email_subject_Shift, ref msg, site);
            string subject_date = string.Empty;
            if (dateFrom == dateTo)
            {
                subject_date = dateFrom.ToString("dd MMM yyyy") + email_subject_Shift;
            }
            else
            {
                subject_date = dateFrom.ToString("dd MMM yyyy") + email_subject_Shift + " - " + dateTo.ToString("dd MMM yyyy");
            }
            email_subject = email_subject.Replace("{DATE}", subject_date);

            email_body = email_body.Replace("{EMAIL_BODY}", content + msg);

            if (!string.IsNullOrEmpty(email_cc))
            {
                email_cc = "," + email_cc;
            }

            contentEmail = contentEmail.Replace("{EmailBody}", email_body);
            if(!config.Is_Production) contentEmail = contentEmail.Replace("{EmailRequestor}", email_Requestor);

            JsonResult send_return = Send(EmailDomainHandling(email_Requestor), email_subject, email_to, email_Requestor + email_cc, contentEmail);
            dynamic send_Data = send_return.Data;

            var result_Object = new { send_Data.Success, Message = "Sending " + email_count.ToString() + " email" + (email_count > 0 ? "s" : ""), Time = DateTime.Now.ToString("dd-MMM-yyyy HH:mm:ss.ffff"), Message_Email = send_Data.Message };

            return JsonConvert.SerializeObject(result_Object);
        }

        private string Get_Location(MERCY_Ctx p_db, string p_company, string p_samplingType, string p_HAC, long p_samplingRequestId)
        {
            string result = string.Empty;

            switch (p_samplingType)
            {
                case "HAC":
                    result = p_HAC;
                    break;
                case "ROM":
                    result = Get_Location_ROM(p_db, p_samplingRequestId);
                    break;
                case "PIT Sampling":
                    result = Get_Location_PIT(p_db, p_company, p_samplingRequestId);
                    break;
                case "DT Sampling":
                    result = Get_Location_PIT(p_db, p_company, p_samplingRequestId);
                    break;
            }

            return result;
        }

        private string Get_Location_ROM(MERCY_Ctx p_db, long p_samplingRequestId)
        {
            string result = string.Empty;

            try
            {
                var dataQuery =
                                (
                                    from d in p_db.SamplingRequest_ROM
                                    where d.SamplingRequest == p_samplingRequestId
                                    orderby d.Block, d.ROM_Name
                                    select new
                                    {
                                        d.ROM_ID
                                        ,
                                        d.Block
                                        ,
                                        d.ROM_Name
                                        ,
                                        Names = d.Block + " " + d.ROM_Name
                                    }
                                );

                var data = dataQuery.ToList();

                int i = 0;
                data.ForEach(c =>
                {
                    i++;

                    result += (i > 1 ? " | " : "") + c.Names;
                });
            }
            catch { }

            return result;
        }

        private string Get_Location_PIT(MERCY_Ctx p_db, string p_company, long p_samplingRequestId)
        {
            string result = string.Empty;

            try
            {
                var dataQuery =
                                (
                                    from d in p_db.SamplingRequest_SEAM
                                    where d.SamplingRequest == p_samplingRequestId
                                    orderby d.COMPANY_PIT_SEAM
                                    select new
                                    {
                                        d.SEAM
                                        ,
                                        d.COMPANY_PIT_SEAM
                                    }
                                );

                var data = dataQuery.ToList();

                int i = 0;
                string pit_previous = string.Empty;
                string pit_current = string.Empty;

                data.ForEach(c =>
                {
                    i++;

                    pit_current = c.COMPANY_PIT_SEAM.Replace(p_company + "__", "")
                                            .Replace("__" + c.SEAM, "");

                    if (i <= 1)
                    {
                        result += pit_current + " " + c.SEAM;
                    }
                    else
                    {
                        if (pit_previous == pit_current)
                        {
                            result += ", " + c.SEAM;
                        }
                        else
                        {
                            result += " | " + pit_current + " " + c.SEAM;
                        }
                    }

                    pit_previous = pit_current;
                });
            }
            catch { }

            return result;
        }

        private string Send_Feedback()
        {
            Configuration config = new Configuration();
            long id = OurUtility.ToInt64(Request[".id"]);
            string data_company = string.Empty;
            string data_user_Fullname = string.Empty;
            string data_user_Email = string.Empty;
            string data_type = string.Empty;
            string email_to = config.Email_Feedback_User;
            string email_cc = config.Email_Feedback_User_CC;
            string email_subject = config.Email_Feedback_Subject;

            string url_link = Request["requestFrom"];
            if (string.IsNullOrEmpty(url_link))
            {
                url_link = OurUtility.Url(Request);
            }
            string url_image = url_link + @"/Api/Notification/Picture?f=";

            email_subject = email_subject.Replace("{ID}", id.ToString());

            string email_body = "Dear user,";
            email_body = System.IO.File.ReadAllText(TemplateFolder + @"\email_template_feedback.html", Encoding.UTF8);
            email_body = email_body.Replace("{ID}", id.ToString());
            email_body = email_body.Replace("{URL}", url_link);
            email_body = email_body.Replace("{MERCY_IMAGE_URL}", url_image);

            int email_count = 0;
            string content = string.Empty;
            string email_Requestor = string.Empty;
            string email_subject_Shift = string.Empty;
            DateTime dateFrom = DateTime.Now;
            DateTime dateTo = DateTime.Now;
            string msg = string.Empty;

            Model_View_Feedback feedback = (new FeedbackController()).Get(id);
            if (feedback == null)
            {
                var msg2 = new { Success = false, Message = "Feedback [" + id.ToString() + "] not found!" };
                return JsonConvert.SerializeObject(msg2);
            }

            string subject_date = OurUtility.DateFormat(feedback.CreatedOn, @"dd-MMM-yyyy HH:mm:ss");
            email_subject = email_subject.Replace("{Date}", subject_date);

            //nanti akan dibuat function tersendiri jika hasil test sudah jalan
            for (int i = 1; i <= 5; i++)
            {
                if (i <= feedback.Accuracy)
                {
                    email_body = email_body.Replace("{MERCY_Accuracy_" + i.ToString() + "}", "star_on.png");
                }
                else
                {
                    email_body = email_body.Replace("{MERCY_Accuracy_" + i.ToString() + "}", "star_off.png");
                }

                if (i <= feedback.Objectivity)
                {
                    email_body = email_body.Replace("{MERCY_Objectivity_" + i.ToString() + "}", "star_on.png");
                }
                else
                {
                    email_body = email_body.Replace("{MERCY_Objectivity_" + i.ToString() + "}", "star_off.png");
                }

                if (i <= feedback.EasyToUnderstand)
                {
                    email_body = email_body.Replace("{MERCY_Easy_" + i.ToString() + "}", "star_on.png");
                }
                else
                {
                    email_body = email_body.Replace("{MERCY_Easy_" + i.ToString() + "}", "star_off.png");
                }

                if (i <= feedback.Detailed)
                {
                    email_body = email_body.Replace("{MERCY_Detailed_" + i.ToString() + "}", "star_on.png");
                }
                else
                {
                    email_body = email_body.Replace("{MERCY_Detailed_" + i.ToString() + "}", "star_off.png");
                }

                if (i <= feedback.Punctuality)
                {
                    email_body = email_body.Replace("{MERCY_Puctuality_" + i.ToString() + "}", "star_on.png");
                }
                else
                {
                    email_body = email_body.Replace("{MERCY_Puctuality_" + i.ToString() + "}", "star_off.png");
                }
            }

            email_body = email_body.Replace("{UserName}", feedback.FullName);
            email_body = email_body.Replace("{Date}", subject_date);
            email_body = email_body.Replace("{Remark}", feedback.Remark);

            if (!string.IsNullOrEmpty(email_cc))
            {
                email_cc = "," + email_cc;
            }

            JsonResult send_return = Send(email_Requestor, email_subject, email_to, email_Requestor + email_cc, email_body);
            dynamic send_Data = send_return.Data;

            var result_Object = new { send_Data.Success, Message = "Sending " + email_count.ToString() + " email" + (email_count > 0 ? "s" : ""), Time = DateTime.Now.ToString("dd-MMM-yyyy HH:mm:ss.ffff"), Message_Email = send_Data.Message };

            return JsonConvert.SerializeObject(result_Object);
        }

        public JsonResult Upload_ROM_Sampling_General_Email()
        {
            string id = OurUtility.ValueOf(Request, ".id");

            Configuration config = new Configuration();

            string email_subject = config.Notification_Upload_ROM_Sampling_General_Email_Subject;
            string email_to = config.Email_To;
            string email_cc = string.Empty;

            string uploaded_file_type = OurUtility.UPLOAD_Sampling_ROM;
            string uploaded_date = string.Empty;
            string company = string.Empty;
            string fileName = string.Empty;
            string link = string.Empty;
            string email_date_Format = config.Notification_Upload_ROM_Sampling_Date_Format;

            string email_To_Menu = config.Notification_Upload_ROM_Sampling_General_Email_To_Menu;
            string email_To_Access = config.Notification_Upload_ROM_Sampling_General_Email_To_Access;

            // To based on "Calculation"
            List<Model_View_UserList_access_Menu> userList_To_Menu = Notification3Controller.UserList_access_Menu(email_To_Menu, email_To_Access, id, uploaded_file_type, ref uploaded_date, ref company, ref fileName, ref link);

            string email_Cc_Name = config.Notification_Upload_ROM_Sampling_General_Email_Cc_Menu;
            string email_Cc_Access = config.Notification_Upload_ROM_Sampling_General_Email_Cc_Access;

            // Cc based on "Calculation"
            List<Model_View_UserList_access_Menu> userList_Cc_Menu = Notification3Controller.UserList_access_Menu(email_Cc_Name, email_Cc_Access, id, uploaded_file_type, ref uploaded_date, ref company, ref fileName, ref link);

            uploaded_date = OurUtility.DateFormat(uploaded_date, email_date_Format);

            email_subject = email_subject.Replace("{ID}", id.ToString());
            email_subject = email_subject.Replace("{Company}", company);
            email_subject = email_subject.Replace("{UploadedDate}", uploaded_date);

            string email_body = System.IO.File.ReadAllText(TemplateFolder + @"\email_template_Upload_ROM_Sampling_General_Email.html", Encoding.UTF8);

            if (config.Is_Production)
            {
                // -- this is Production

                // To based on "Calculation" above
                email_to = Notification3Controller.EmailAddress(userList_To_Menu);

                // Cc based on "Calculation" above
                email_cc = Notification3Controller.EmailAddress(userList_Cc_Menu);
            }
            else
            {
                // -- Not Production

                // To based on "Calculation" above
                email_to = Notification3Controller.EmailAddress_and_Name(userList_To_Menu);

                // Cc based on "Calculation" above
                email_cc = Notification3Controller.EmailAddress_and_Name(userList_Cc_Menu);

                // give information in Body of Email
                email_body = "---------------------------------------------------------------------"
                         + "<br/>Email To: " + email_to
                         + "<br/><br/>Email Cc: " + email_cc
                         + "<br/>---------------------------------------------------------------------"
                         + "<br/><br/>" + email_body;

                // "Testing - email Address"
                email_to = config.Email_To;

                // reset Cc
                email_cc = string.Empty;
            }

            email_body = email_body.Replace("{Company}", company);
            email_body = email_body.Replace("{UploadedDate}", uploaded_date);
            email_body = email_body.Replace("{MERCY_URL}", config.FrontEnd_Url);

            Attachment attachment = null;
            try
            {
                attachment = new Attachment(UploadFolder + link)
                {
                    Name = fileName
                };
            }
            catch { }

            return Send(string.Empty, email_subject, email_to, email_cc, email_body, attachment);
        }

        public JsonResult Upload_Geology_Pit_Monitoring_General_Email()
        {
            string id = OurUtility.ValueOf(Request, ".id");

            Configuration config = new Configuration();

            string email_subject = config.Notification_Upload_Geology_Pit_Monitoring_General_Email_Subject;
            string email_to = config.Email_To;
            string email_cc = string.Empty;

            string uploaded_file_type = OurUtility.UPLOAD_Geology_Pit_Monitoring;
            string uploaded_date = string.Empty;
            string company = string.Empty;
            string fileName = string.Empty;
            string link = string.Empty;
            string email_date_Format = config.Notification_Upload_Geology_Pit_Monitoring_Date_Format;

            string email_To_Menu = config.Notification_Upload_Geology_Pit_Monitoring_General_Email_To_Menu;
            string email_To_Access = config.Notification_Upload_Geology_Pit_Monitoring_General_Email_To_Access;

            // To based on "Calculation"
            List<Model_View_UserList_access_Menu> userList_To_Menu = Notification3Controller.UserList_access_Menu(email_To_Menu, email_To_Access, id, uploaded_file_type, ref uploaded_date, ref company, ref fileName, ref link);

            string email_Cc_Name = config.Notification_Upload_Geology_Pit_Monitoring_General_Email_Cc_Menu;
            string email_Cc_Access = config.Notification_Upload_Geology_Pit_Monitoring_General_Email_Cc_Access;

            // Cc based on "Calculation"
            List<Model_View_UserList_access_Menu> userList_Cc_Menu = Notification3Controller.UserList_access_Menu(email_Cc_Name, email_Cc_Access, id, uploaded_file_type, ref uploaded_date, ref company, ref fileName, ref link);

            uploaded_date = OurUtility.DateFormat(uploaded_date, email_date_Format);

            email_subject = email_subject.Replace("{ID}", id.ToString());
            email_subject = email_subject.Replace("{Company}", company);
            email_subject = email_subject.Replace("{UploadedDate}", uploaded_date);

            string email_body = System.IO.File.ReadAllText(TemplateFolder + @"\email_template_Upload_Geology_Pit_Monitoring_General_Email.html", Encoding.UTF8);

            if (config.Is_Production)
            {
                // -- this is Production

                // To based on "Calculation" above
                email_to = Notification3Controller.EmailAddress(userList_To_Menu);

                // Cc based on "Calculation" above
                email_cc = Notification3Controller.EmailAddress(userList_Cc_Menu);
            }
            else
            {
                // -- Not Production

                // To based on "Calculation" above
                email_to = Notification3Controller.EmailAddress_and_Name(userList_To_Menu);

                // Cc based on "Calculation" above
                email_cc = Notification3Controller.EmailAddress_and_Name(userList_Cc_Menu);

                // give information in Body of Email
                email_body = "---------------------------------------------------------------------"
                         + "<br/>Email To: " + email_to
                         + "<br/><br/>Email Cc: " + email_cc
                         + "<br/>---------------------------------------------------------------------"
                         + "<br/><br/>" + email_body;

                // "Testing - email Address"
                email_to = config.Email_To;

                // reset Cc
                email_cc = string.Empty;
            }

            email_body = email_body.Replace("{Company}", company);
            email_body = email_body.Replace("{UploadedDate}", uploaded_date);
            email_body = email_body.Replace("{MERCY_URL}", config.FrontEnd_Url);

            Attachment attachment = null;
            try
            {
                attachment = new Attachment(UploadFolder + link)
                {
                    Name = fileName
                };
            }
            catch { }

            return Send(string.Empty, email_subject, email_to, email_cc, email_body, attachment);
        }

        public JsonResult Upload_Geology_Exploration_General_Email()
        {
            string id = OurUtility.ValueOf(Request, ".id");

            Configuration config = new Configuration();

            string email_subject = config.Notification_Upload_Geology_Exploration_General_Email_Subject;
            string email_to = config.Email_To;
            string email_cc = string.Empty;

            string uploaded_file_type = OurUtility.UPLOAD_Geology_Explorasi;
            string uploaded_date = string.Empty;
            string company = string.Empty;
            string fileName = string.Empty;
            string link = string.Empty;
            string email_date_Format = config.Notification_Upload_Geology_Exploration_Date_Format;

            string email_To_Menu = config.Notification_Upload_Geology_Exploration_General_Email_To_Menu;
            string email_To_Access = config.Notification_Upload_Geology_Exploration_General_Email_To_Access;

            // To based on "Calculation"
            List<Model_View_UserList_access_Menu> userList_To_Menu = Notification3Controller.UserList_access_Menu(email_To_Menu, email_To_Access, id, uploaded_file_type, ref uploaded_date, ref company, ref fileName, ref link);

            string email_Cc_Name = config.Notification_Upload_Geology_Exploration_General_Email_Cc_Menu;
            string email_Cc_Access = config.Notification_Upload_Geology_Exploration_General_Email_Cc_Access;

            // Cc based on "Calculation"
            List<Model_View_UserList_access_Menu> userList_Cc_Menu = Notification3Controller.UserList_access_Menu(email_Cc_Name, email_Cc_Access, id, uploaded_file_type, ref uploaded_date, ref company, ref fileName, ref link);

            uploaded_date = OurUtility.DateFormat(uploaded_date, email_date_Format);

            email_subject = email_subject.Replace("{ID}", id.ToString());
            email_subject = email_subject.Replace("{Company}", company);
            email_subject = email_subject.Replace("{UploadedDate}", uploaded_date);

            string email_body = System.IO.File.ReadAllText(TemplateFolder + @"\email_template_Upload_Geology_Exploration_General_Email.html", Encoding.UTF8);

            if (config.Is_Production)
            {
                // -- this is Production

                // To based on "Calculation" above
                email_to = Notification3Controller.EmailAddress(userList_To_Menu);

                // Cc based on "Calculation" above
                email_cc = Notification3Controller.EmailAddress(userList_Cc_Menu);
            }
            else
            {
                // -- Not Production

                // To based on "Calculation" above
                email_to = Notification3Controller.EmailAddress_and_Name(userList_To_Menu);

                // Cc based on "Calculation" above
                email_cc = Notification3Controller.EmailAddress_and_Name(userList_Cc_Menu);

                // give information in Body of Email
                email_body = "---------------------------------------------------------------------"
                         + "<br/>Email To: " + email_to
                         + "<br/><br/>Email Cc: " + email_cc
                         + "<br/>---------------------------------------------------------------------"
                         + "<br/><br/>" + email_body;

                // "Testing - email Address"
                email_to = config.Email_To;

                // reset Cc
                email_cc = string.Empty;
            }

            email_body = email_body.Replace("{Company}", company);
            email_body = email_body.Replace("{UploadedDate}", uploaded_date);
            email_body = email_body.Replace("{MERCY_URL}", config.FrontEnd_Url);

            Attachment attachment = null;
            try
            {
                attachment = new Attachment(UploadFolder + link)
                {
                    Name = fileName
                };
            }
            catch { }

            return Send(string.Empty, email_subject, email_to, email_cc, email_body, attachment);
        }

        public JsonResult Upload_FC_Crushing()
        {
            string id = OurUtility.ValueOf(Request, ".id");

            Configuration config = new Configuration();

            string email_subject = config.Notification_Upload_FC_Crushing_Subject;
            string email_to = config.Email_To;
            string email_cc = string.Empty;

            string uploaded_file_type = OurUtility.UPLOAD_CRUSHING_PLANT;
            string uploaded_date = string.Empty;
            string company = string.Empty;
            string fileName = string.Empty;
            string link = string.Empty;
            string email_date_Format = config.Notification_Upload_Geology_Exploration_Date_Format;

            string email_To_Menu = config.Notification_Upload_FC_Crushing_To_Menu;
            string email_To_Access = config.Notification_Upload_FC_Crushing_To_Access;

            // To based on "Calculation"
            List<Model_View_UserList_access_Menu> userList_To_Menu = Notification3Controller.UserList_access_Menu(email_To_Menu, email_To_Access, id, uploaded_file_type, ref uploaded_date, ref company, ref fileName, ref link);

            string email_Cc_Name = config.Notification_Upload_FC_Crushing_Cc_Menu;
            string email_Cc_Access = config.Notification_Upload_FC_Crushing_Cc_Access;

            // Cc based on "Calculation"
            List<Model_View_UserList_access_Menu> userList_Cc_Menu = Notification3Controller.UserList_access_Menu(email_Cc_Name, email_Cc_Access, id, uploaded_file_type, ref uploaded_date, ref company, ref fileName, ref link);

            uploaded_date = OurUtility.DateFormat(uploaded_date, email_date_Format);

            email_subject = email_subject.Replace("{ID}", id.ToString());
            email_subject = email_subject.Replace("{Company}", company);
            email_subject = email_subject.Replace("{UploadedDate}", uploaded_date);

            string email_body = System.IO.File.ReadAllText(TemplateFolder + @"\email_template_Upload_FC_Crushing.html", Encoding.UTF8);

            if (config.Is_Production)
            {
                // -- this is Production

                // To based on "Calculation" above
                email_to = Notification3Controller.EmailAddress(userList_To_Menu);

                // Cc based on "Calculation" above
                email_cc = Notification3Controller.EmailAddress(userList_Cc_Menu);
            }
            else
            {
                // -- Not Production

                // To based on "Calculation" above
                email_to = Notification3Controller.EmailAddress_and_Name(userList_To_Menu);

                // Cc based on "Calculation" above
                email_cc = Notification3Controller.EmailAddress_and_Name(userList_Cc_Menu);

                // give information in Body of Email
                email_body = "---------------------------------------------------------------------"
                         + "<br/>Email To: " + email_to
                         + "<br/><br/>Email Cc: " + email_cc
                         + "<br/>---------------------------------------------------------------------"
                         + "<br/><br/>" + email_body;

                // "Testing - email Address"
                email_to = config.Email_To;

                // reset Cc
                email_cc = string.Empty;
            }

            email_body = email_body.Replace("{Company}", company);
            email_body = email_body.Replace("{UploadedDate}", uploaded_date);
            email_body = email_body.Replace("{MERCY_URL}", config.FrontEnd_Url);

            Attachment attachment = null;
            try
            {
                attachment = new Attachment(UploadFolder + link)
                {
                    Name = fileName
                };
            }
            catch { }

            return Send(string.Empty, email_subject, email_to, email_cc, email_body, attachment);
        }

        public JsonResult Upload_FC_Barging()
        {
            string id = OurUtility.ValueOf(Request, ".id");

            Configuration config = new Configuration();

            string email_subject = config.Notification_Upload_FC_Barging_Subject;
            string email_to = config.Email_To;
            string email_cc = string.Empty;

            string uploaded_file_type = OurUtility.UPLOAD_BARGE_LOADING;
            string uploaded_date = string.Empty;
            string company = string.Empty;
            string fileName = string.Empty;
            string link = string.Empty;
            string email_date_Format = config.Notification_Upload_Geology_Exploration_Date_Format;

            string email_To_Menu = config.Notification_Upload_FC_Barging_To_Menu;
            string email_To_Access = config.Notification_Upload_FC_Barging_To_Access;

            // To based on "Calculation"
            List<Model_View_UserList_access_Menu> userList_To_Menu = Notification3Controller.UserList_access_Menu(email_To_Menu, email_To_Access, id, uploaded_file_type, ref uploaded_date, ref company, ref fileName, ref link);

            string email_Cc_Name = config.Notification_Upload_FC_Barging_Cc_Menu;
            string email_Cc_Access = config.Notification_Upload_FC_Barging_Cc_Access;

            // Cc based on "Calculation"
            List<Model_View_UserList_access_Menu> userList_Cc_Menu = Notification3Controller.UserList_access_Menu(email_Cc_Name, email_Cc_Access, id, uploaded_file_type, ref uploaded_date, ref company, ref fileName, ref link);

            uploaded_date = OurUtility.DateFormat(uploaded_date, email_date_Format);

            email_subject = email_subject.Replace("{ID}", id.ToString());
            email_subject = email_subject.Replace("{Company}", company);
            email_subject = email_subject.Replace("{UploadedDate}", uploaded_date);

            string email_body = System.IO.File.ReadAllText(TemplateFolder + @"\email_template_Upload_FC_Barging.html", Encoding.UTF8);

            if (config.Is_Production)
            {
                // -- this is Production

                // To based on "Calculation" above
                email_to = Notification3Controller.EmailAddress(userList_To_Menu);

                // Cc based on "Calculation" above
                email_cc = Notification3Controller.EmailAddress(userList_Cc_Menu);
            }
            else
            {
                // -- Not Production

                // To based on "Calculation" above
                email_to = Notification3Controller.EmailAddress_and_Name(userList_To_Menu);

                // Cc based on "Calculation" above
                email_cc = Notification3Controller.EmailAddress_and_Name(userList_Cc_Menu);

                // give information in Body of Email
                email_body = "---------------------------------------------------------------------"
                         + "<br/>Email To: " + email_to
                         + "<br/><br/>Email Cc: " + email_cc
                         + "<br/>---------------------------------------------------------------------"
                         + "<br/><br/>" + email_body;

                // "Testing - email Address"
                email_to = config.Email_To;

                // reset Cc
                email_cc = string.Empty;
            }

            email_body = email_body.Replace("{Company}", company);
            email_body = email_body.Replace("{UploadedDate}", uploaded_date);
            email_body = email_body.Replace("{MERCY_URL}", config.FrontEnd_Url);

            Attachment attachment = null;
            try
            {
                attachment = new Attachment(UploadFolder + link)
                {
                    Name = fileName
                };
            }
            catch { }

            return Send(string.Empty, email_subject, email_to, email_cc, email_body, attachment);
        }

        public JsonResult Upload_ROM_Sampling_Specific_Email()
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

            string id = OurUtility.ValueOf(Request, ".id");
            string company = OurUtility.ValueOf(Request, "company");
            int year = DateTime.Now.Year;

            Configuration config = new Configuration();

            string separator = string.Empty;
            string requestID_All = string.Empty;
            string requestID_Links = string.Empty;
            string url_Detail = config.FrontEnd_Url + @"/SamplingRequestv/Form?.id=";

            using (MERCY_Ctx db = new MERCY_Ctx())
            {
                using (var connection = new SqlConnection(db.Database.Connection.ConnectionString))
                {
                    connection.Open();

                    SqlCommand command = connection.CreateCommand();
                    SqlCommand command_per_Request = connection.CreateCommand();
                    SqlCommand command_History = connection.CreateCommand();

                    command.CommandText = string.Format(@"
                                                            select SamplingRequest, LabId
                                                            from SamplingRequest_Lab
                                                            where Company = '{1}' 
                                                                and CreatedOn_Year_Only = {2}
                                                                and LabId in 
                                                            (
                                                                select d.Lab_ID
                                                                from UPLOAD_Sampling_ROM d
                                                                    , TEMPORARY_Sampling_ROM t
                                                                    , TEMPORARY_Sampling_ROM_Header h
                                                                where d.TEMPORARY = t.RecordId
                                                                    and t.Header = h.RecordId
                                                                    and h.File_Physical = {0}
                                                                    and d.Company = '{1}' 
                                                                    and d.CreatedOn_Year_Only = {2}
                                                            )
                                                            order by SamplingRequest, LabId
                                                            ", id, company, year);

                    List<Model_View_Values> samplingRequests = new List<Model_View_Values>();
                    Model_View_Values temp = null;
                    string history_Remark = string.Empty;
                    string lab_id_str = string.Empty;
                    string separator2 = string.Empty;

                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            temp = new Model_View_Values
                            {
                                Name = OurUtility.ValueOf(reader, "SamplingRequest"),
                                Description = OurUtility.ValueOf(reader, "LabId")
                            };

                            samplingRequests.Add(temp);
                        }
                    }

                    if (samplingRequests.Count > 0)
                    {
                        var requets_Ids = (from d in samplingRequests
                                           orderby d.Name
                                           select d.Name).Distinct();

                        foreach (string request_Id in requets_Ids)
                        {
                            requestID_All += separator + request_Id;
                            separator = ",";

                            requestID_Links += string.Format("<li><a href='{0}'>{0}</a></li>", url_Detail + request_Id);

                            // ambil LabIds
                            var lab_Ids = (from d in samplingRequests
                                           where d.Name == request_Id
                                           orderby d.Description
                                           select d.Description).ToList();

                            lab_id_str = string.Empty;
                            separator2 = string.Empty;
                            foreach (string lab_id in lab_Ids)
                            {
                                lab_id_str += separator2 + lab_id;
                                separator2 = ",";
                            }

                            // Check lagi, apa Complete vs Partial
                            command_per_Request.CommandText = string.Format(@"
                                                                            select LabId
                                                                            from SamplingRequest_Lab
                                                                            where SamplingRequest = {0}
                                                                                and LabId not in
                                                                                (
                                                                                    select Lab_ID
                                                                                    from UPLOAD_Sampling_ROM
                                                                                    where Company = '{1}' 
                                                                                        and CreatedOn_Year_Only = {2}
                                                                                )
                                                                            ", request_Id, company, year);

                            history_Remark = string.Empty;
                            using (SqlDataReader reader_per_Request = command_per_Request.ExecuteReader())
                            {
                                if (reader_per_Request.Read())
                                {
                                    // masih ada LabID untuk Request tersebut == yang belum di Upload
                                    // NOT Complete
                                    history_Remark = "(Partial Lab Sampling) Upload Lab Result for Lab ID: " + lab_id_str;
                                }
                                else
                                {
                                    // semua data LabID untuk Request tersebut == sudah terUpload semuanya
                                    // All Complete
                                    history_Remark = "(Completed Lab Sampling) Upload Lab Result for Lab ID: " + lab_id_str;
                                }
                            }

                            // for Data History: add dengan keterangan
                            command_History.CommandText = string.Format(
                                                                            @"insert into SamplingRequest_History(
                                                                                CreatedOn, CreatedBy
                                                                                , SamplingRequestId
                                                                                , Description
                                                                            )
                                                                            values(GetDate(), {0}, {1}, '{2}')", user.UserId, request_Id, history_Remark);
                            command_History.ExecuteNonQuery();
                        }

                        requestID_Links = string.Format("<ul>{0}</ul>", requestID_Links);
                    }

                    connection.Close();
                }
            }

            if (string.IsNullOrEmpty(requestID_All))
            {
                var msg = new { Success = false, Message = "No data SamplingRequest is affected", MessageDetail = string.Empty, Version = Configuration.VERSION };
                return Json(msg, JsonRequestBehavior.AllowGet);
            }

            string email_subject = config.Notification_Upload_ROM_Sampling_Specific_Email_Subject;
            string email_to = config.Email_To;
            string email_cc = string.Empty;

            string uploaded_file_type = OurUtility.UPLOAD_Sampling_ROM;
            string uploaded_date = string.Empty;
            string fileName = string.Empty;
            string link = string.Empty;
            string email_date_Format = config.Notification_Upload_ROM_Sampling_Date_Format;

            string email_To_Menu = config.Notification_Upload_ROM_Sampling_Specific_Email_To_Menu;
            string email_To_Access = config.Notification_Upload_ROM_Sampling_Specific_Email_To_Access;

            // To based on "Calculation"
            List<Model_View_UserList_access_Menu> userList_To_Menu = Notification3Controller.UserList_access_Menu(email_To_Menu, email_To_Access, id, uploaded_file_type, ref uploaded_date, ref company, ref fileName, ref link);

            string email_Cc_Name = config.Notification_Upload_ROM_Sampling_Specific_Email_Cc_Menu;
            string email_Cc_Access = config.Notification_Upload_ROM_Sampling_Specific_Email_Cc_Access;

            // Cc based on "Calculation"
            List<Model_View_UserList_access_Menu> userList_Cc_Menu = Notification3Controller.UserList_access_Menu(email_Cc_Name, email_Cc_Access, id, uploaded_file_type, ref uploaded_date, ref company, ref fileName, ref link);

            uploaded_date = OurUtility.DateFormat(uploaded_date, email_date_Format);

            email_subject = email_subject.Replace("{Company}", company);
            email_subject = email_subject.Replace("{UploadedDate}", uploaded_date);
            email_subject = email_subject.Replace("{ID}", requestID_All);

            string email_body = System.IO.File.ReadAllText(TemplateFolder + @"\email_template_Upload_ROM_Sampling_Specific_Email.html", Encoding.UTF8);

            if (config.Is_Production)
            {
                // -- this is Production

                // To based on "Calculation" above
                email_to = Notification3Controller.EmailAddress(userList_To_Menu);

                // Cc based on "Calculation" above
                email_cc = Notification3Controller.EmailAddress(userList_Cc_Menu);
            }
            else
            {
                // -- Not Production

                // To based on "Calculation" above
                email_to = Notification3Controller.EmailAddress_and_Name(userList_To_Menu);

                // Cc based on "Calculation" above
                email_cc = Notification3Controller.EmailAddress_and_Name(userList_Cc_Menu);

                // give information in Body of Email
                email_body = "---------------------------------------------------------------------"
                         + "<br/>Email To: " + email_to
                         + "<br/><br/>Email Cc: " + email_cc
                         + "<br/>---------------------------------------------------------------------"
                         + "<br/><br/>" + email_body;

                // "Testing - email Address"
                email_to = config.Email_To;

                // reset Cc
                email_cc = string.Empty;
            }

            email_body = email_body.Replace("{Company}", company);
            email_body = email_body.Replace("{UploadedDate}", uploaded_date);
            email_body = email_body.Replace("{request_ID}", requestID_All);
            email_body = email_body.Replace("{URLs}", requestID_Links);

            Attachment attachment = null;
            try
            {
                attachment = new Attachment(UploadFolder + link)
                {
                    Name = fileName
                };
            }
            catch { }

            return Send(string.Empty, email_subject, email_to, email_cc, email_body, attachment);
        }

        public JsonResult Upload_Geology_Pit_Monitoring_Specific_Email()
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

            string id = OurUtility.ValueOf(Request, ".id");
            string company = OurUtility.ValueOf(Request, "company");
            int year = DateTime.Now.Year;

            Configuration config = new Configuration();

            string separator = string.Empty;
            string requestID_All = string.Empty;
            string requestID_Links = string.Empty;
            string url_Detail = config.FrontEnd_Url + @"/AnalysisRequestv/Form?.id=";

            using (MERCY_Ctx db = new MERCY_Ctx())
            {
                using (var connection = new SqlConnection(db.Database.Connection.ConnectionString))
                {
                    connection.Open();

                    SqlCommand command = connection.CreateCommand();
                    SqlCommand command_per_Request = connection.CreateCommand();
                    SqlCommand command_History = connection.CreateCommand();

                    command.CommandText = string.Format(@"
                                                            select AnalysisRequest, LabId
                                                            from AnalysisRequest_Detail
                                                            where Company = '{1}' 
                                                                and CreatedOn_Year_Only = {2}
                                                                and LabId in 
                                                            (
                                                                select d.Lab_ID
                                                                from UPLOAD_Geology_Pit_Monitoring d
                                                                    , TEMPORARY_Geology_Pit_Monitoring t
                                                                    , TEMPORARY_Geology_Pit_Monitoring_Header h
                                                                where d.TEMPORARY = t.RecordId
                                                                    and t.Header = h.RecordId
                                                                    and h.File_Physical = {0}
                                                                    and d.Company = '{1}' 
                                                                    and d.CreatedOn_Year_Only = {2}
                                                            )
                                                            order by AnalysisRequest, LabId
                                                            ", id, company, year);

                    List<Model_View_Values> analysisRequests = new List<Model_View_Values>();
                    Model_View_Values temp = null;
                    string history_Remark = string.Empty;
                    string lab_id_str = string.Empty;
                    string separator2 = string.Empty;

                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            temp = new Model_View_Values
                            {
                                Name = OurUtility.ValueOf(reader, "AnalysisRequest"),
                                Description = OurUtility.ValueOf(reader, "LabId")
                            };

                            analysisRequests.Add(temp);
                        }
                    }

                    if (analysisRequests.Count > 0)
                    {
                        var requets_Ids = (from d in analysisRequests
                                           orderby d.Name
                                           select d.Name).Distinct();

                        foreach (string request_Id in requets_Ids)
                        {
                            requestID_All += separator + request_Id;
                            separator = ",";

                            requestID_Links += string.Format("<li><a href='{0}'>{0}</a></li>", url_Detail + request_Id);

                            // ambil LabIds
                            var lab_Ids = (from d in analysisRequests
                                           where d.Name == request_Id
                                           orderby d.Description
                                           select d.Description).ToList();

                            lab_id_str = string.Empty;
                            separator2 = string.Empty;
                            foreach (string lab_id in lab_Ids)
                            {
                                lab_id_str += separator2 + lab_id;
                                separator2 = ",";
                            }

                            // Check lagi, apa Complete vs Partial
                            command_per_Request.CommandText = string.Format(@"
                                                                            select LabId
                                                                            from AnalysisRequest_Detail
                                                                            where AnalysisRequest = {0}
                                                                                and LabId not in
                                                                                (
                                                                                    select Lab_ID
                                                                                    from UPLOAD_Geology_Pit_Monitoring
                                                                                    where Company = '{1}' 
                                                                                        and CreatedOn_Year_Only = {2}
                                                                                )
                                                                            ", request_Id, company, year);

                            history_Remark = string.Empty;
                            using (SqlDataReader reader_per_Request = command_per_Request.ExecuteReader())
                            {
                                if (reader_per_Request.Read())
                                {
                                    // masih ada LabID untuk Request tersebut == yang belum di Upload
                                    // NOT Complete
                                    history_Remark = "(Partial Lab Analysis) Upload Lab Result for Lab ID: " + lab_id_str;
                                }
                                else
                                {
                                    // semua data LabID untuk Request tersebut == sudah terUpload semuanya
                                    // All Complete
                                    history_Remark = "(Completed Lab Analysis) Upload Lab Result for Lab ID: " + lab_id_str;
                                }
                            }

                            // for Data History: add dengan keterangan
                            command_History.CommandText = string.Format(
                                                                            @"insert into AnalysisRequest_History(
                                                                                CreatedOn, CreatedBy
                                                                                , AnalysisRequestId
                                                                                , Description
                                                                            )
                                                                            values(GetDate(), {0}, {1}, '{2}')", user.UserId, request_Id, history_Remark);
                            command_History.ExecuteNonQuery();
                        }

                        requestID_Links = string.Format("<ul>{0}</ul>", requestID_Links);
                    }

                    connection.Close();
                }
            }

            if (string.IsNullOrEmpty(requestID_All))
            {
                var msg = new { Success = false, Message = "No data AnalysisRequest is affected", MessageDetail = string.Empty, Version = Configuration.VERSION };
                return Json(msg, JsonRequestBehavior.AllowGet);
            }

            string email_subject = config.Notification_Upload_Geology_Pit_Monitoring_Specific_Email_Subject;
            string email_to = config.Email_To;
            string email_cc = string.Empty;

            string uploaded_file_type = OurUtility.UPLOAD_Geology_Pit_Monitoring;
            string uploaded_date = string.Empty;
            string fileName = string.Empty;
            string link = string.Empty;
            string email_date_Format = config.Notification_Upload_Geology_Pit_Monitoring_Date_Format;

            string email_To_Menu = config.Notification_Upload_Geology_Pit_Monitoring_Specific_Email_To_Menu;
            string email_To_Access = config.Notification_Upload_Geology_Pit_Monitoring_Specific_Email_To_Access;

            // To based on "Calculation"
            List<Model_View_UserList_access_Menu> userList_To_Menu = Notification3Controller.UserList_access_Menu(email_To_Menu, email_To_Access, id, uploaded_file_type, ref uploaded_date, ref company, ref fileName, ref link);

            string email_Cc_Name = config.Notification_Upload_Geology_Pit_Monitoring_Specific_Email_Cc_Menu;
            string email_Cc_Access = config.Notification_Upload_Geology_Pit_Monitoring_Specific_Email_Cc_Access;

            // Cc based on "Calculation"
            List<Model_View_UserList_access_Menu> userList_Cc_Menu = Notification3Controller.UserList_access_Menu(email_Cc_Name, email_Cc_Access, id, uploaded_file_type, ref uploaded_date, ref company, ref fileName, ref link);

            uploaded_date = OurUtility.DateFormat(uploaded_date, email_date_Format);

            email_subject = email_subject.Replace("{Company}", company);
            email_subject = email_subject.Replace("{UploadedDate}", uploaded_date);
            email_subject = email_subject.Replace("{ID}", requestID_All);

            string email_body = System.IO.File.ReadAllText(TemplateFolder + @"\email_template_Upload_Geology_Pit_Monitoring_Specific_Email.html", Encoding.UTF8);

            if (config.Is_Production)
            {
                // -- this is Production

                // To based on "Calculation" above
                email_to = Notification3Controller.EmailAddress(userList_To_Menu);

                // Cc based on "Calculation" above
                email_cc = Notification3Controller.EmailAddress(userList_Cc_Menu);
            }
            else
            {
                // -- Not Production

                // To based on "Calculation" above
                email_to = Notification3Controller.EmailAddress_and_Name(userList_To_Menu);

                // Cc based on "Calculation" above
                email_cc = Notification3Controller.EmailAddress_and_Name(userList_Cc_Menu);

                // give information in Body of Email
                email_body = "---------------------------------------------------------------------"
                         + "<br/>Email To: " + email_to
                         + "<br/><br/>Email Cc: " + email_cc
                         + "<br/>---------------------------------------------------------------------"
                         + "<br/><br/>" + email_body;

                // "Testing - email Address"
                email_to = config.Email_To;

                // reset Cc
                email_cc = string.Empty;
            }

            email_body = email_body.Replace("{Company}", company);
            email_body = email_body.Replace("{UploadedDate}", uploaded_date);
            email_body = email_body.Replace("{request_ID}", requestID_All);
            email_body = email_body.Replace("{URLs}", requestID_Links);

            Attachment attachment = null;
            try
            {
                attachment = new Attachment(UploadFolder + link)
                {
                    Name = fileName
                };
            }
            catch { }

            return Send(string.Empty, email_subject, email_to, email_cc, email_body, attachment);
        }

        public JsonResult Upload_Geology_Exploration_Specific_Email()
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

            string id = OurUtility.ValueOf(Request, ".id");
            string company = OurUtility.ValueOf(Request, "company");
            int year = DateTime.Now.Year;

            Configuration config = new Configuration();

            string separator = string.Empty;
            string requestID_All = string.Empty;
            string requestID_Links = string.Empty;
            string url_Detail = config.FrontEnd_Url + @"/AnalysisRequestv/Form?.id=";

            using (MERCY_Ctx db = new MERCY_Ctx())
            {
                using (var connection = new SqlConnection(db.Database.Connection.ConnectionString))
                {
                    connection.Open();

                    SqlCommand command = connection.CreateCommand();
                    SqlCommand command_per_Request = connection.CreateCommand();
                    SqlCommand command_History = connection.CreateCommand();

                    command.CommandText = string.Format(@"
                                                            select AnalysisRequest, LabId
                                                            from AnalysisRequest_Detail
                                                            where Company = '{1}' 
                                                                and CreatedOn_Year_Only = {2}
                                                                and LabId in 
                                                            (
                                                                select d.Lab_ID
                                                                from UPLOAD_Geology_Explorasi d
                                                                    , TEMPORARY_Geology_Explorasi t
                                                                    , TEMPORARY_Geology_Explorasi_Header h
                                                                where d.TEMPORARY = t.RecordId
                                                                    and t.Header = h.RecordId
                                                                    and h.File_Physical = {0}
                                                                    and d.Company = '{1}' 
                                                                    and d.CreatedOn_Year_Only = {2}
                                                            )
                                                            order by AnalysisRequest, LabId
                                                            ", id, company, year);

                    List<Model_View_Values> analysisRequests = new List<Model_View_Values>();
                    Model_View_Values temp = null;
                    string history_Remark = string.Empty;
                    string lab_id_str = string.Empty;
                    string separator2 = string.Empty;

                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            temp = new Model_View_Values
                            {
                                Name = OurUtility.ValueOf(reader, "AnalysisRequest"),
                                Description = OurUtility.ValueOf(reader, "LabId")
                            };

                            analysisRequests.Add(temp);
                        }
                    }

                    if (analysisRequests.Count > 0)
                    {
                        var requets_Ids = (from d in analysisRequests
                                           orderby d.Name
                                           select d.Name).Distinct();

                        foreach (string request_Id in requets_Ids)
                        {
                            requestID_All += separator + request_Id;
                            separator = ",";

                            requestID_Links += string.Format("<li><a href='{0}'>{0}</a></li>", url_Detail + request_Id);

                            // ambil LabIds
                            var lab_Ids = (from d in analysisRequests
                                           where d.Name == request_Id
                                           orderby d.Description
                                           select d.Description).ToList();

                            lab_id_str = string.Empty;
                            separator2 = string.Empty;
                            foreach (string lab_id in lab_Ids)
                            {
                                lab_id_str += separator2 + lab_id;
                                separator2 = ",";
                            }

                            // Check lagi, apa Complete vs Partial
                            command_per_Request.CommandText = string.Format(@"
                                                                            select LabId
                                                                            from AnalysisRequest_Detail
                                                                            where AnalysisRequest = {0}
                                                                                and LabId not in
                                                                                (
                                                                                    select Lab_ID
                                                                                    from UPLOAD_Geology_Explorasi
                                                                                    where Company = '{1}' 
                                                                                        and CreatedOn_Year_Only = {2}
                                                                                )
                                                                            ", request_Id, company, year);

                            history_Remark = string.Empty;
                            using (SqlDataReader reader_per_Request = command_per_Request.ExecuteReader())
                            {
                                if (reader_per_Request.Read())
                                {
                                    // masih ada LabID untuk Request tersebut == yang belum di Upload
                                    // NOT Complete
                                    history_Remark = "(Partial Lab Analysis) Upload Lab Result for Lab ID: " + lab_id_str;
                                }
                                else
                                {
                                    // semua data LabID untuk Request tersebut == sudah terUpload semuanya
                                    // All Complete
                                    history_Remark = "(Completed Lab Analysis) Upload Lab Result for Lab ID: " + lab_id_str;
                                }
                            }

                            // for Data History: add dengan keterangan
                            command_History.CommandText = string.Format(
                                                                            @"insert into AnalysisRequest_History(
                                                                                CreatedOn, CreatedBy
                                                                                , AnalysisRequestId
                                                                                , Description
                                                                            )
                                                                            values(GetDate(), {0}, {1}, '{2}')", user.UserId, request_Id, history_Remark);
                            command_History.ExecuteNonQuery();
                        }

                        requestID_Links = string.Format("<ul>{0}</ul>", requestID_Links);
                    }

                    connection.Close();
                }
            }

            if (string.IsNullOrEmpty(requestID_All))
            {
                var msg = new { Success = false, Message = "No data AnalysisRequest is affected", MessageDetail = string.Empty, Version = Configuration.VERSION };
                return Json(msg, JsonRequestBehavior.AllowGet);
            }

            string email_subject = config.Notification_Upload_Geology_Exploration_Specific_Email_Subject;
            string email_to = config.Email_To;
            string email_cc = string.Empty;

            string uploaded_file_type = OurUtility.UPLOAD_Geology_Explorasi;
            string uploaded_date = string.Empty;
            string fileName = string.Empty;
            string link = string.Empty;
            string email_date_Format = config.Notification_Upload_Geology_Exploration_Date_Format;

            string email_To_Menu = config.Notification_Upload_Geology_Exploration_Specific_Email_To_Menu;
            string email_To_Access = config.Notification_Upload_Geology_Exploration_Specific_Email_To_Access;

            // To based on "Calculation"
            List<Model_View_UserList_access_Menu> userList_To_Menu = Notification3Controller.UserList_access_Menu(email_To_Menu, email_To_Access, id, uploaded_file_type, ref uploaded_date, ref company, ref fileName, ref link);

            string email_Cc_Name = config.Notification_Upload_Geology_Exploration_Specific_Email_Cc_Menu;
            string email_Cc_Access = config.Notification_Upload_Geology_Exploration_Specific_Email_Cc_Access;

            // Cc based on "Calculation"
            List<Model_View_UserList_access_Menu> userList_Cc_Menu = Notification3Controller.UserList_access_Menu(email_Cc_Name, email_Cc_Access, id, uploaded_file_type, ref uploaded_date, ref company, ref fileName, ref link);

            uploaded_date = OurUtility.DateFormat(uploaded_date, email_date_Format);

            email_subject = email_subject.Replace("{Company}", company);
            email_subject = email_subject.Replace("{UploadedDate}", uploaded_date);
            email_subject = email_subject.Replace("{ID}", requestID_All);

            string email_body = System.IO.File.ReadAllText(TemplateFolder + @"\email_template_Upload_Geology_Exploration_Specific_Email.html", Encoding.UTF8);

            if (config.Is_Production)
            {
                // -- this is Production

                // To based on "Calculation" above
                email_to = Notification3Controller.EmailAddress(userList_To_Menu);

                // Cc based on "Calculation" above
                email_cc = Notification3Controller.EmailAddress(userList_Cc_Menu);
            }
            else
            {
                // -- Not Production

                // To based on "Calculation" above
                email_to = Notification3Controller.EmailAddress_and_Name(userList_To_Menu);

                // Cc based on "Calculation" above
                email_cc = Notification3Controller.EmailAddress_and_Name(userList_Cc_Menu);

                // give information in Body of Email
                email_body = "---------------------------------------------------------------------"
                         + "<br/>Email To: " + email_to
                         + "<br/><br/>Email Cc: " + email_cc
                         + "<br/>---------------------------------------------------------------------"
                         + "<br/><br/>" + email_body;

                // "Testing - email Address"
                email_to = config.Email_To;

                // reset Cc
                email_cc = string.Empty;
            }

            email_body = email_body.Replace("{Company}", company);
            email_body = email_body.Replace("{UploadedDate}", uploaded_date);
            email_body = email_body.Replace("{request_ID}", requestID_All);
            email_body = email_body.Replace("{URLs}", requestID_Links);

            Attachment attachment = null;
            try
            {
                attachment = new Attachment(UploadFolder + link)
                {
                    Name = fileName
                };
            }
            catch { }

            return Send(string.Empty, email_subject, email_to, email_cc, email_body, attachment);
        }

        public JsonResult SamplingRequest()
        {
            long id = OurUtility.ToInt64(OurUtility.ValueOf(Request, ".id"));
            string company = OurUtility.ValueOf(Request, "company");

            Configuration config = new Configuration();

            string email_subject = config.Notification_Sampling_Request_Subject;
            string email_to = config.Email_To;
            string email_cc = string.Empty;

            string email_date_Format = config.Notification_Sampling_Request_Date_Format;

            string email_To_Menu = config.Notification_Sampling_Request_To_Menu;
            string email_To_Access = config.Notification_Sampling_Request_To_Access;

            // To based on "Calculation"
            List<Model_View_UserList_access_Menu> userList_To_Menu = Notification3Controller.UserList_access_Menu(email_To_Menu, ref company);

            string email_Cc_Name = config.Notification_Sampling_Request_Cc_Menu;
            string email_Cc_Access = config.Notification_Sampling_Request_Cc_Access;

            // Cc based on "Calculation"
            List<Model_View_UserList_access_Menu> userList_Cc_Menu = Notification3Controller.UserList_access_Menu(email_Cc_Name, ref company);

            string data_company = string.Empty;
            string data_user_Fullname = string.Empty;
            string data_user_Email_Requestor = string.Empty;
            string data_type = string.Empty;
            string data_Location = string.Empty;

            try
            {
                using (MERCY_Ctx db = new MERCY_Ctx())
                {
                    var dataQuery =
                            (
                                from dt in db.SamplingRequests
                                join usr in db.Users on dt.CreatedBy equals usr.UserId
                                where dt.SamplingRequestId == id
                                select new
                                {
                                    dt.Company
                                    ,
                                    dt.SamplingType
                                    ,
                                    usr.FullName
                                    ,
                                    usr.Email
                                    ,
                                    dt.HAC_Text
                                    ,
                                    dt.SamplingRequestId
                                }
                            );

                    var data = dataQuery.SingleOrDefault();

                    data_company = data.Company;
                    data_user_Fullname = data.FullName;
                    data_user_Email_Requestor = data.Email;
                    data_type = data.SamplingType;

                    // Update email based on company data
                    company = data.Company;
                    userList_To_Menu = Notification3Controller.UserList_access_Menu(email_To_Menu, ref company);
                    userList_Cc_Menu = Notification3Controller.UserList_access_Menu(email_Cc_Name, ref company);

                    data_Location = Get_Location(db, data_company, data.SamplingType, data.HAC_Text, data.SamplingRequestId);
                }
            }
            catch { }

            email_subject = email_subject.Replace("{ID}", id.ToString());
            email_subject = email_subject.Replace("{Company}", data_company);
            email_subject = email_subject.Replace("{SAMPLING_TYPE}", data_type);
            email_subject = email_subject.Replace("{LOCATION}", data_Location);

            string email_body = System.IO.File.ReadAllText(TemplateFolder + @"\email_template_SamplingRequest.html", Encoding.UTF8);

            if (config.Is_Production)
            {
                // -- this is Production

                // To based on "Calculation" above
                email_to = Notification3Controller.EmailAddress(userList_To_Menu);

                // Cc based on "Calculation" above
                email_cc = Notification3Controller.EmailAddress(userList_Cc_Menu);
            }
            else
            {
                // -- Not Production

                // To based on "Calculation" above
                email_to = Notification3Controller.EmailAddress_and_Name(userList_To_Menu);

                // Cc based on "Calculation" above
                email_cc = Notification3Controller.EmailAddress_and_Name(userList_Cc_Menu);

                // give information in Body of Email
                email_body = "---------------------------------------------------------------------"
                         + "<br/>Email To: " + email_to
                         + "<br/><br/>Email Cc: " + email_cc
                         + "<br/>---------------------------------------------------------------------"
                         + "<br/><br/>" + email_body;

                // "Testing - email Address"
                email_to = config.Email_To;

                // reset Cc
                email_cc = string.Empty;
            }

            string url_image = config.FrontEnd_Url + @"/Api/Notification/Picture?f=";
            string url_detail = config.FrontEnd_Url + @"/SamplingRequestv";

            //email_body = email_body.Replace("{Username}", email_Fullname);
            email_body = email_body.Replace("{MERCY_USER}", data_user_Fullname);
            email_body = email_body.Replace("{MERCY_COMPANY}", data_company);
            email_body = email_body.Replace("{MERCY_ID}", id.ToString());
            email_body = email_body.Replace("{MERCY_DATA_TYPE}", data_type);
            email_body = email_body.Replace("{MERCY_IMAGE_URL}", url_image);
            email_body = email_body.Replace("{MERCY_DETAIL}", url_detail);
            email_body = email_body.Replace("{MERCY_TYPE}", "Sampling Request");

            Attachment attachment = null;

            return Send(string.Empty, email_subject, email_to, email_cc, email_body, attachment);
        }


        public JsonResult AnalysisRequest()
        {
            long id = OurUtility.ToInt64(OurUtility.ValueOf(Request, ".id"));
            string company = OurUtility.ValueOf(Request, "company");

            Configuration config = new Configuration();

            string email_subject = config.Notification_Analysis_Request_Subject;
            string email_to = config.Email_To;
            string email_cc = string.Empty;

            string email_date_Format = config.Notification_AnalysisRequest_Date_Format;

            string email_To_Menu = config.Notification_Analysis_Request_To_Menu;
            string email_To_Access = config.Notification_Analysis_Request_To_Access;

            // To based on "Calculation"
            List<Model_View_UserList_access_Menu> userList_To_Menu = Notification3Controller.UserList_access_Menu(email_To_Menu, ref company);

            string email_Cc_Name = config.Notification_Analysis_Request_Cc_Menu;
            string email_Cc_Access = config.Notification_Analysis_Request_Cc_Access;

            // Cc based on "Calculation"
            List<Model_View_UserList_access_Menu> userList_Cc_Menu = Notification3Controller.UserList_access_Menu(email_Cc_Name, ref company);

            string data_company = string.Empty;
            string data_user_Fullname = string.Empty;
            string data_user_Email_Requestor = string.Empty;
            string data_type = string.Empty;
            string data_LetterNo = string.Empty;

            try
            {
                using (MERCY_Ctx db = new MERCY_Ctx())
                {
                    var dataQuery =
                                (
                                    from dt in db.AnalysisRequests
                                    join usr in db.Users on dt.CreatedBy equals usr.UserId
                                    where dt.AnalysisRequestId == id
                                    select new
                                    {
                                        dt.Company
                                        ,
                                        dt.AnalysisType
                                        ,
                                        usr.FullName
                                        ,
                                        usr.Email
                                        ,
                                        dt.LetterNo
                                    }
                                );

                    var data = dataQuery.SingleOrDefault();

                    data_company = data.Company;
                    data_user_Fullname = data.FullName;
                    data_user_Email_Requestor = data.Email;
                    data_type = data.AnalysisType;
                    data_LetterNo = data.LetterNo;

                    // Update email based on company data
                    company = data.Company;
                    userList_To_Menu = Notification3Controller.UserList_access_Menu(email_To_Menu, ref company);
                    userList_Cc_Menu = Notification3Controller.UserList_access_Menu(email_Cc_Name, ref company);
                }
            }
            catch { }

            email_subject = email_subject.Replace("{ID}", id.ToString());
            email_subject = email_subject.Replace("{Company}", data_company);
            email_subject = email_subject.Replace("{ANALYSIS_TYPE}", data_type);
            email_subject = email_subject.Replace("{LETTER_NO}", data_LetterNo);

            string email_body = System.IO.File.ReadAllText(TemplateFolder + @"\email_template_AnalysisRequest.html", Encoding.UTF8);

            if (config.Is_Production)
            {
                // -- this is Production

                // To based on "Calculation" above
                email_to = Notification3Controller.EmailAddress(userList_To_Menu);

                // Cc based on "Calculation" above
                email_cc = Notification3Controller.EmailAddress(userList_Cc_Menu);
            }
            else
            {
                // -- Not Production

                // To based on "Calculation" above
                email_to = Notification3Controller.EmailAddress_and_Name(userList_To_Menu);

                // Cc based on "Calculation" above
                email_cc = Notification3Controller.EmailAddress_and_Name(userList_Cc_Menu);

                // give information in Body of Email
                email_body = "---------------------------------------------------------------------"
                         + "<br/>Email To: " + email_to
                         + "<br/><br/>Email Cc: " + email_cc
                         + "<br/>---------------------------------------------------------------------"
                         + "<br/><br/>" + email_body;

                // "Testing - email Address"
                email_to = config.Email_To;

                // reset Cc
                email_cc = string.Empty;
            }

            string url_image = config.FrontEnd_Url + @"/Api/Notification/Picture?f=";
            string url_detail = config.FrontEnd_Url + @"/AnalysisRequestv";

            //email_body = email_body.Replace("{Username}", email_Fullname);
            email_body = email_body.Replace("{MERCY_USER}", data_user_Fullname);
            email_body = email_body.Replace("{MERCY_COMPANY}", data_company);
            email_body = email_body.Replace("{MERCY_ID}", id.ToString());
            email_body = email_body.Replace("{MERCY_DATA_TYPE}", data_type);
            email_body = email_body.Replace("{MERCY_IMAGE_URL}", url_image);
            email_body = email_body.Replace("{MERCY_DETAIL}", url_detail);
            email_body = email_body.Replace("{MERCY_TYPE}", "Analysis Request");

            Attachment attachment = null;

            return Send(string.Empty, email_subject, email_to, email_cc, email_body, attachment);
        }

        public JsonResult Discussion()
        {
            string page = OurUtility.ValueOf(Request, ".p");

            JsonResult result = null;

            switch (page)
            {
                case "1":
                case "AnalysisRequest":
                    result = Discussion_AnalysisRequest();
                    break;
                case "2":
                case "TunnelManagement":
                    result = Discussion_TunnelManagement();
                    break;
                default:
                    result = Discussion_AnalysisRequest();
                    break;
            }

            return result;
        }

        private JsonResult Discussion_AnalysisRequest()
        {
            long id = OurUtility.ToInt64(OurUtility.ValueOf(Request, ".id"));
            string company = OurUtility.ValueOf(Request, "company");

            Configuration config = new Configuration();

            string email_subject = config.Notification_Discussion_Subject;
            string email_to = config.Email_To;
            string email_cc = string.Empty;

            string email_date_Format = config.Notification_Discussion_Date_Format;

            string email_To_Menu = config.Notification_Discussion_To_Menu;
            string email_To_Access = config.Notification_Discussion_To_Access;

            // To based on "Calculation"
            List<Model_View_UserList_access_Menu> userList_To_Menu = Notification3Controller.UserList_access_Menu(email_To_Menu, ref company);

            string email_Cc_Name = config.Notification_Discussion_Cc_Menu;
            string email_Cc_Access = config.Notification_Discussion_Cc_Access;

            // Cc based on "Calculation"
            List<Model_View_UserList_access_Menu> userList_Cc_Menu = Notification3Controller.UserList_access_Menu(email_Cc_Name, ref company);

            string data_company = string.Empty;
            string data_user_Fullname = string.Empty;
            string data_user_Email_Requestor = string.Empty;
            string data_type = string.Empty;
            string data_LetterNo = string.Empty;

            try
            {
                using (MERCY_Ctx db = new MERCY_Ctx())
                {
                    var dataQuery =
                                (
                                    from dt in db.AnalysisRequests
                                    join usr in db.Users on dt.CreatedBy equals usr.UserId
                                    where dt.AnalysisRequestId == id
                                    select new
                                    {
                                        dt.Company
                                        ,
                                        dt.AnalysisType
                                        ,
                                        usr.FullName
                                        ,
                                        usr.Email
                                        ,
                                        dt.LetterNo
                                    }
                                );

                    var data = dataQuery.SingleOrDefault();

                    data_company = data.Company;
                    data_user_Fullname = data.FullName;
                    data_user_Email_Requestor = data.Email;
                    data_type = data.AnalysisType;
                    data_LetterNo = data.LetterNo;

                    // Update email based on company data
                    company = data.Company;
                    userList_To_Menu = Notification3Controller.UserList_access_Menu(email_To_Menu, ref company);
                    userList_Cc_Menu = Notification3Controller.UserList_access_Menu(email_Cc_Name, ref company);
                }
            }
            catch { }

            email_subject = email_subject.Replace("{ID}", id.ToString());
            email_subject = email_subject.Replace("{Company}", data_company);
            email_subject = email_subject.Replace("{ANALYSIS_TYPE}", data_type);
            email_subject = email_subject.Replace("{LETTER_NO}", data_LetterNo);

            string email_body = System.IO.File.ReadAllText(TemplateFolder + @"\email_template_discussion_AnalysisRequest.html", Encoding.UTF8);

            if (config.Is_Production)
            {
                // -- this is Production

                // To based on "Calculation" above
                email_to = Notification3Controller.EmailAddress(userList_To_Menu);

                // Cc based on "Calculation" above
                email_cc = Notification3Controller.EmailAddress(userList_Cc_Menu);
            }
            else
            {
                // -- Not Production

                // To based on "Calculation" above
                email_to = Notification3Controller.EmailAddress_and_Name(userList_To_Menu);

                // Cc based on "Calculation" above
                email_cc = Notification3Controller.EmailAddress_and_Name(userList_Cc_Menu);

                // give information in Body of Email
                email_body = "---------------------------------------------------------------------"
                         + "<br/>Email To: " + email_to
                         + "<br/><br/>Email Cc: " + email_cc
                         + "<br/>---------------------------------------------------------------------"
                         + "<br/><br/>" + email_body;

                // "Testing - email Address"
                email_to = config.Email_To;

                // reset Cc
                email_cc = string.Empty;
            }

            string url_image = config.FrontEnd_Url + @"/Api/Notification/Picture?f=";
            string url_detail = config.FrontEnd_Url + @"/AnalysisRequestv";

            //email_body = email_body.Replace("{Username}", email_Fullname);
            email_body = email_body.Replace("{MERCY_USER}", data_user_Fullname);
            email_body = email_body.Replace("{MERCY_COMPANY}", data_company);
            email_body = email_body.Replace("{MERCY_ID}", id.ToString());
            email_body = email_body.Replace("{MERCY_DATA_TYPE}", data_type);
            email_body = email_body.Replace("{MERCY_IMAGE_URL}", url_image);
            email_body = email_body.Replace("{MERCY_DETAIL}", url_detail);
            email_body = email_body.Replace("{MERCY_TYPE}", "Analysis Request");

            Attachment attachment = null;

            return Send(string.Empty, email_subject, email_to, email_cc, email_body, attachment);
        }

        public JsonResult Discussion_TunnelManagement()
        {
            long id = OurUtility.ToInt64(OurUtility.ValueOf(Request, ".id"));
            string company = OurUtility.ValueOf(Request, "company");

            Configuration config = new Configuration();

            string email_subject = config.Notification_Discussion_TunnelManagement_Subject;
            string email_to = config.Email_To;
            string email_cc = string.Empty;

            string email_date_Format = config.Notification_Discussion_TunnelManagement_Date_Format;

            string email_To_Menu = config.Notification_Discussion_TunnelManagement_To_Menu;
            string email_To_Access = config.Notification_Discussion_TunnelManagement_To_Access;

            // To based on "Calculation"
            List<Model_View_UserList_access_Menu> userList_To_Menu = Notification3Controller.UserList_access_Menu(email_To_Menu, ref company);

            string email_Cc_Name = config.Notification_Discussion_TunnelManagement_Cc_Menu;
            string email_Cc_Access = config.Notification_Discussion_TunnelManagement_Cc_Access;

            // Cc based on "Calculation"
            List<Model_View_UserList_access_Menu> userList_Cc_Menu = Notification3Controller.UserList_access_Menu(email_Cc_Name, ref company);

            string data_company = string.Empty;
            string data_blending_Date = string.Empty;
            string data_shift = string.Empty;
            string data_tunnel = string.Empty;

            try
            {
                using (MERCY_Ctx db = new MERCY_Ctx())
                {
                    var dataQuery =
                            (
                                from d in db.HaulingRequest_Detail_PortionBlending
                                where d.RecordIdx == id
                                select new Model_View_TunnelManagement
                                {
                                    Company = d.Company
                                    ,
                                    Product = d.Product
                                    ,
                                    BlendingDate = d.BlendingDate
                                    ,
                                    Shift = d.Shift
                                    ,
                                    Hopper = d.Hopper
                                    ,
                                    Tunnel = d.Tunnel
                                }
                            );

                    var data = dataQuery.SingleOrDefault();

                    data_company = data.Company;
                    data_blending_Date = OurUtility.DateFormat(data.BlendingDate, "dd-MMM-yyyy");
                    data_shift = (data.Shift == 1 ? "A" : "B");
                    data_tunnel = data.Tunnel;

                    // Update email based on company data
                    company = data.Company;
                    userList_To_Menu = Notification3Controller.UserList_access_Menu(email_To_Menu, ref company);
                    userList_Cc_Menu = Notification3Controller.UserList_access_Menu(email_Cc_Name, ref company);
                }
            }
            catch { }

            email_subject = email_subject.Replace("{ID}", id.ToString());
            email_subject = email_subject.Replace("{Company}", data_company);
            email_subject = email_subject.Replace("{HaulingRequest}", data_blending_Date);
            email_subject = email_subject.Replace("{Shift}", data_shift);

            string email_body = System.IO.File.ReadAllText(TemplateFolder + @"\email_template_discussion_TunnelManagement.html", Encoding.UTF8);

            if (config.Is_Production)
            {
                // -- this is Production

                // To based on "Calculation" above
                email_to = Notification3Controller.EmailAddress(userList_To_Menu);

                // Cc based on "Calculation" above
                email_cc = Notification3Controller.EmailAddress(userList_Cc_Menu);
            }
            else
            {
                // -- Not Production

                // To based on "Calculation" above
                email_to = Notification3Controller.EmailAddress_and_Name(userList_To_Menu);

                // Cc based on "Calculation" above
                email_cc = Notification3Controller.EmailAddress_and_Name(userList_Cc_Menu);

                // give information in Body of Email
                email_body = "---------------------------------------------------------------------"
                         + "<br/>Email To: " + email_to
                         + "<br/><br/>Email Cc: " + email_cc
                         + "<br/>---------------------------------------------------------------------"
                         + "<br/><br/>" + email_body;

                // "Testing - email Address"
                email_to = config.Email_To;

                // reset Cc
                email_cc = string.Empty;
            }

            string url_detail = config.FrontEnd_Url + @"/TunnelManagementv";

            email_body = email_body.Replace("{ID}", id.ToString());
            email_body = email_body.Replace("{Company}", data_company);
            email_body = email_body.Replace("{HaulingRequest}", data_blending_Date);
            email_body = email_body.Replace("{Shift}", data_shift);
            email_body = email_body.Replace("{Url}", url_detail);

            Attachment attachment = null;

            return Send(string.Empty, email_subject, email_to, email_cc, email_body, attachment);
        }

        public JsonResult AnalysisRequest_Verification()
        {
            long id = OurUtility.ToInt64(OurUtility.ValueOf(Request, ".id"));
            string company = OurUtility.ValueOf(Request, "company");

            Configuration config = new Configuration();

            string email_subject = config.Notification_Analysis_Request_Verification_Subject;
            string email_to = config.Email_To;
            string email_cc = string.Empty;

            string email_date_Format = config.Notification_AnalysisRequest_Verification_Date_Format;

            string email_To_Menu = config.Notification_Analysis_Request_Verification_To_Menu;
            string email_To_Access = config.Notification_Analysis_Request_Verification_To_Access;

            // To based on "Calculation"
            List<Model_View_UserList_access_Menu> userList_To_Menu = Notification3Controller.UserList_access_Menu(email_To_Menu, ref company);

            string email_Cc_Name = config.Notification_Analysis_Request_Verification_Cc_Menu;
            string email_Cc_Access = config.Notification_Analysis_Request_Verification_Cc_Access;

            // Cc based on "Calculation"
            List<Model_View_UserList_access_Menu> userList_Cc_Menu = Notification3Controller.UserList_access_Menu(email_Cc_Name, ref company);

            string data_company = string.Empty;
            string data_user_Fullname = string.Empty;
            string data_user_Email_Requestor = string.Empty;
            string data_type = string.Empty;
            string data_LetterNo = string.Empty;

            try
            {
                using (MERCY_Ctx db = new MERCY_Ctx())
                {
                    var dataQuery =
                                (
                                    from dt in db.AnalysisRequests
                                    join usr in db.Users on dt.CreatedBy equals usr.UserId
                                    where dt.AnalysisRequestId == id
                                    select new
                                    {
                                        dt.Company
                                        ,
                                        dt.AnalysisType
                                        ,
                                        usr.FullName
                                        ,
                                        usr.Email
                                        ,
                                        dt.LetterNo
                                    }
                                );

                    var data = dataQuery.SingleOrDefault();

                    data_company = data.Company;
                    data_user_Fullname = data.FullName;
                    data_user_Email_Requestor = data.Email;
                    data_type = data.AnalysisType;
                    data_LetterNo = data.LetterNo;

                    // Update email based on company data
                    company = data.Company;
                    userList_To_Menu = Notification3Controller.UserList_access_Menu(email_To_Menu, ref company);
                    userList_Cc_Menu = Notification3Controller.UserList_access_Menu(email_Cc_Name, ref company);
                }
            }
            catch { }

            email_subject = email_subject.Replace("{ID}", id.ToString());
            email_subject = email_subject.Replace("{Company}", data_company);
            email_subject = email_subject.Replace("{ANALYSIS_TYPE}", data_type);
            email_subject = email_subject.Replace("{LETTER_NO}", data_LetterNo);

            string email_body = System.IO.File.ReadAllText(TemplateFolder + @"\email_template_AnalysisRequest_Verification.html", Encoding.UTF8);

            if (config.Is_Production)
            {
                // -- this is Production

                // To based on "Calculation" above
                email_to = Notification3Controller.EmailAddress(userList_To_Menu);

                // Cc based on "Calculation" above
                email_cc = Notification3Controller.EmailAddress(userList_Cc_Menu);
            }
            else
            {
                // -- Not Production

                // To based on "Calculation" above
                email_to = Notification3Controller.EmailAddress_and_Name(userList_To_Menu);

                // Cc based on "Calculation" above
                email_cc = Notification3Controller.EmailAddress_and_Name(userList_Cc_Menu);

                // give information in Body of Email
                email_body = "---------------------------------------------------------------------"
                         + "<br/>Email To: " + email_to
                         + "<br/><br/>Email Cc: " + email_cc
                         + "<br/>---------------------------------------------------------------------"
                         + "<br/><br/>" + email_body;

                // "Testing - email Address"
                email_to = config.Email_To;

                // reset Cc
                email_cc = string.Empty;
            }

            string url_image = config.FrontEnd_Url + @"/Api/Notification/Picture?f=";
            string url_detail = config.FrontEnd_Url + @"/AnalysisRequestv";

            //email_body = email_body.Replace("{Username}", email_Fullname);
            email_body = email_body.Replace("{MERCY_USER}", data_user_Fullname);
            email_body = email_body.Replace("{MERCY_COMPANY}", data_company);
            email_body = email_body.Replace("{MERCY_ID}", id.ToString());
            email_body = email_body.Replace("{MERCY_DATA_TYPE}", data_type);
            email_body = email_body.Replace("{MERCY_IMAGE_URL}", url_image);
            email_body = email_body.Replace("{MERCY_DETAIL}", url_detail);
            email_body = email_body.Replace("{MERCY_TYPE}", "Analysis Request");

            Attachment attachment = null;

            return Send(string.Empty, email_subject, email_to, email_cc, email_body, attachment);
        }

        public JsonResult AnalysisRequest_ReTest()
        {
            long id = OurUtility.ToInt64(OurUtility.ValueOf(Request, ".id"));
            string company = OurUtility.ValueOf(Request, "company");

            Configuration config = new Configuration();

            string email_subject = config.Notification_Analysis_Request_Subject.Replace("Analysis Request", "Analysis Request Retest");
            string email_to = config.Email_To;
            string email_cc = string.Empty;

            string email_date_Format = config.Notification_AnalysisRequest_Date_Format;

            string email_To_Menu = config.Notification_Analysis_Request_To_Menu;
            string email_To_Access = config.Notification_Analysis_Request_To_Access;

            // To based on "Calculation"
            List<Model_View_UserList_access_Menu> userList_To_Menu = Notification3Controller.UserList_access_Menu(email_To_Menu, ref company);

            string email_Cc_Name = config.Notification_Analysis_Request_Cc_Menu;
            string email_Cc_Access = config.Notification_Analysis_Request_Cc_Access;

            // Cc based on "Calculation"
            List<Model_View_UserList_access_Menu> userList_Cc_Menu = Notification3Controller.UserList_access_Menu(email_Cc_Name, ref company);

            string data_company = string.Empty;
            string data_user_Fullname = string.Empty;
            string data_user_Email_Requestor = string.Empty;
            string data_type = string.Empty;
            string data_LetterNo = string.Empty;

            try
            {
                using (MERCY_Ctx db = new MERCY_Ctx())
                {
                    var dataQuery =
                                (
                                    from dt in db.AnalysisRequests
                                    join usr in db.Users on dt.CreatedBy equals usr.UserId
                                    where dt.AnalysisRequestId == id
                                    select new
                                    {
                                        dt.Company
                                        ,
                                        dt.AnalysisType
                                        ,
                                        usr.FullName
                                        ,
                                        usr.Email
                                        ,
                                        dt.LetterNo
                                    }
                                );

                    var data = dataQuery.SingleOrDefault();

                    data_company = data.Company;
                    data_user_Fullname = data.FullName;
                    data_user_Email_Requestor = data.Email;
                    data_type = data.AnalysisType;
                    data_LetterNo = data.LetterNo;

                    // Update email based on company data
                    company = data.Company;
                    userList_To_Menu = Notification3Controller.UserList_access_Menu(email_To_Menu, ref company);
                    userList_Cc_Menu = Notification3Controller.UserList_access_Menu(email_Cc_Name, ref company);
                }
            }
            catch { }

            email_subject = email_subject.Replace("{ID}", id.ToString());
            email_subject = email_subject.Replace("{Company}", data_company);
            email_subject = email_subject.Replace("{ANALYSIS_TYPE}", data_type);
            email_subject = email_subject.Replace("{LETTER_NO}", data_LetterNo);

            string email_body = System.IO.File.ReadAllText(TemplateFolder + @"\email_template_AnalysisRequest_ReTest.html", Encoding.UTF8);

            if (config.Is_Production)
            {
                // -- this is Production

                // To based on "Calculation" above
                email_to = Notification3Controller.EmailAddress(userList_To_Menu);

                // Cc based on "Calculation" above
                email_cc = Notification3Controller.EmailAddress(userList_Cc_Menu);
            }
            else
            {
                // -- Not Production

                // To based on "Calculation" above
                email_to = Notification3Controller.EmailAddress_and_Name(userList_To_Menu);

                // Cc based on "Calculation" above
                email_cc = Notification3Controller.EmailAddress_and_Name(userList_Cc_Menu);

                // give information in Body of Email
                email_body = "---------------------------------------------------------------------"
                         + "<br/>Email To: " + email_to
                         + "<br/><br/>Email Cc: " + email_cc
                         + "<br/>---------------------------------------------------------------------"
                         + "<br/><br/>" + email_body;

                // "Testing - email Address"
                email_to = config.Email_To;

                // reset Cc
                email_cc = string.Empty;
            }

            string url_image = config.FrontEnd_Url + @"/Api/Notification/Picture?f=";
            string url_detail = config.FrontEnd_Url + @"/AnalysisRequestv";

            //email_body = email_body.Replace("{Username}", email_Fullname);
            email_body = email_body.Replace("{MERCY_USER}", data_user_Fullname);
            email_body = email_body.Replace("{MERCY_COMPANY}", data_company);
            email_body = email_body.Replace("{MERCY_ID}", id.ToString());
            email_body = email_body.Replace("{MERCY_DATA_TYPE}", data_type);
            email_body = email_body.Replace("{MERCY_IMAGE_URL}", url_image);
            email_body = email_body.Replace("{MERCY_DETAIL}", url_detail);
            email_body = email_body.Replace("{MERCY_TYPE}", "Analysis Request");

            Attachment attachment = null;

            return Send(string.Empty, email_subject, email_to, email_cc, email_body, attachment);
        }

        public JsonResult Upload_HAC_General_Email()
        {
            string id = OurUtility.ValueOf(Request, ".id");

            Configuration config = new Configuration();

            string email_subject = config.Notification_Upload_HAC_General_Email_Subject;
            string email_to = config.Email_To;
            string email_cc = string.Empty;

            string uploaded_file_type = OurUtility.UPLOAD_HAC;
            string uploaded_date = string.Empty;
            string company = string.Empty;
            string fileName = string.Empty;
            string link = string.Empty;
            string email_date_Format = config.Notification_Upload_HAC_Date_Format;

            string email_To_Menu = config.Notification_Upload_HAC_General_Email_To_Menu;
            string email_To_Access = config.Notification_Upload_HAC_General_Email_To_Access;

            // To based on "Calculation"
            List<Model_View_UserList_access_Menu> userList_To_Menu = Notification3Controller.UserList_access_Menu(email_To_Menu, email_To_Access, id, uploaded_file_type, ref uploaded_date, ref company, ref fileName, ref link);

            string email_Cc_Name = config.Notification_Upload_HAC_General_Email_Cc_Menu;
            string email_Cc_Access = config.Notification_Upload_HAC_General_Email_Cc_Access;

            // Cc based on "Calculation"
            List<Model_View_UserList_access_Menu> userList_Cc_Menu = Notification3Controller.UserList_access_Menu(email_Cc_Name, email_Cc_Access, id, uploaded_file_type, ref uploaded_date, ref company, ref fileName, ref link);

            uploaded_date = OurUtility.DateFormat(uploaded_date, email_date_Format);

            email_subject = email_subject.Replace("{ID}", id.ToString());
            email_subject = email_subject.Replace("{Company}", company);
            email_subject = email_subject.Replace("{UploadedDate}", uploaded_date);

            string email_body = System.IO.File.ReadAllText(TemplateFolder + @"\email_template_Upload_HAC_General_Email.html", Encoding.UTF8);

            if (config.Is_Production)
            {
                // -- this is Production

                // To based on "Calculation" above
                email_to = Notification3Controller.EmailAddress(userList_To_Menu);

                // Cc based on "Calculation" above
                email_cc = Notification3Controller.EmailAddress(userList_Cc_Menu);
            }
            else
            {
                // -- Not Production

                // To based on "Calculation" above
                email_to = Notification3Controller.EmailAddress_and_Name(userList_To_Menu);

                // Cc based on "Calculation" above
                email_cc = Notification3Controller.EmailAddress_and_Name(userList_Cc_Menu);

                // give information in Body of Email
                email_body = "---------------------------------------------------------------------"
                         + "<br/>Email To: " + email_to
                         + "<br/><br/>Email Cc: " + email_cc
                         + "<br/>---------------------------------------------------------------------"
                         + "<br/><br/>" + email_body;

                // "Testing - email Address"
                email_to = config.Email_To;

                // reset Cc
                email_cc = string.Empty;
            }

            email_body = email_body.Replace("{Company}", company);
            email_body = email_body.Replace("{UploadedDate}", uploaded_date);
            email_body = email_body.Replace("{MERCY_URL}", config.FrontEnd_Url);

            Attachment attachment = null;
            try
            {
                attachment = new Attachment(UploadFolder + link)
                {
                    Name = fileName
                };
            }
            catch { }

            return Send(string.Empty, email_subject, email_to, email_cc, email_body, attachment);
        }

        public JsonResult Upload_HAC_Specific_Email()
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

            string id = OurUtility.ValueOf(Request, ".id");
            string company = OurUtility.ValueOf(Request, "company");
            int year = DateTime.Now.Year;

            Configuration config = new Configuration();

            string separator = string.Empty;
            string requestID_All = string.Empty;
            string requestID_Links = string.Empty;
            string url_Detail = config.FrontEnd_Url + @"/SamplingRequestv/Form?.id=";

            using (MERCY_Ctx db = new MERCY_Ctx())
            {
                using (var connection = new SqlConnection(db.Database.Connection.ConnectionString))
                {
                    connection.Open();

                    SqlCommand command = connection.CreateCommand();
                    SqlCommand command_per_Request = connection.CreateCommand();
                    SqlCommand command_History = connection.CreateCommand();

                    command.CommandText = string.Format(@"
                                                            select SamplingRequest, LabId
                                                            from SamplingRequest_Lab
                                                            where Company = '{1}' 
                                                                and CreatedOn_Year_Only = {2}
                                                                and LabId in 
                                                            (
                                                                select d.Lab_ID
                                                                from UPLOAD_HAC d
                                                                    , TEMPORARY_HAC t
                                                                    , TEMPORARY_HAC_Header h
                                                                where d.TEMPORARY = t.RecordId
                                                                    and t.Header = h.RecordId
                                                                    and h.File_Physical = {0}
                                                                    and d.Company = '{1}' 
                                                                    and d.CreatedOn_Year_Only = {2}
                                                            )
                                                            order by SamplingRequest, LabId
                                                            ", id, company, year);

                    List<Model_View_Values> samplingRequests = new List<Model_View_Values>();
                    Model_View_Values temp = null;
                    string history_Remark = string.Empty;
                    string lab_id_str = string.Empty;
                    string separator2 = string.Empty;

                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            temp = new Model_View_Values
                            {
                                Name = OurUtility.ValueOf(reader, "SamplingRequest"),
                                Description = OurUtility.ValueOf(reader, "LabId")
                            };

                            samplingRequests.Add(temp);
                        }
                    }

                    if (samplingRequests.Count > 0)
                    {
                        var requets_Ids = (from d in samplingRequests
                                           orderby d.Name
                                           select d.Name).Distinct();

                        foreach (string request_Id in requets_Ids)
                        {
                            requestID_All += separator + request_Id;
                            separator = ",";

                            requestID_Links += string.Format("<li><a href='{0}'>{0}</a></li>", url_Detail + request_Id);

                            // ambil LabIds
                            var lab_Ids = (from d in samplingRequests
                                           where d.Name == request_Id
                                           orderby d.Description
                                           select d.Description).ToList();

                            lab_id_str = string.Empty;
                            separator2 = string.Empty;
                            foreach (string lab_id in lab_Ids)
                            {
                                lab_id_str += separator2 + lab_id;
                                separator2 = ",";
                            }

                            // Check lagi, apa Complete vs Partial
                            command_per_Request.CommandText = string.Format(@"
                                                                            select LabId
                                                                            from SamplingRequest_Lab
                                                                            where SamplingRequest = {0}
                                                                                and LabId not in
                                                                                (
                                                                                    select Lab_ID
                                                                                    from UPLOAD_HAC
                                                                                    where Company = '{1}' 
                                                                                        and CreatedOn_Year_Only = {2}
                                                                                )
                                                                            ", request_Id, company, year);

                            history_Remark = string.Empty;
                            using (SqlDataReader reader_per_Request = command_per_Request.ExecuteReader())
                            {
                                if (reader_per_Request.Read())
                                {
                                    // masih ada LabID untuk Request tersebut == yang belum di Upload
                                    // NOT Complete
                                    history_Remark = "(Partial Lab Sampling) Upload Lab Result for Lab ID: " + lab_id_str;
                                }
                                else
                                {
                                    // semua data LabID untuk Request tersebut == sudah terUpload semuanya
                                    // All Complete
                                    history_Remark = "(Completed Lab Sampling) Upload Lab Result for Lab ID: " + lab_id_str;
                                }
                            }

                            // for Data History: add dengan keterangan
                            command_History.CommandText = string.Format(
                                                                            @"insert into SamplingRequest_History(
                                                                                CreatedOn, CreatedBy
                                                                                , SamplingRequestId
                                                                                , Description
                                                                            )
                                                                            values(GetDate(), {0}, {1}, '{2}')", user.UserId, request_Id, history_Remark);
                            command_History.ExecuteNonQuery();
                        }

                        requestID_Links = string.Format("<ul>{0}</ul>", requestID_Links);
                    }

                    connection.Close();
                }
            }

            if (string.IsNullOrEmpty(requestID_All))
            {
                var msg = new { Success = false, Message = "No data SamplingRequest is affected", MessageDetail = string.Empty, Version = Configuration.VERSION };
                return Json(msg, JsonRequestBehavior.AllowGet);
            }

            string email_subject = config.Notification_Upload_HAC_Specific_Email_Subject;
            string email_to = config.Email_To;
            string email_cc = string.Empty;

            string uploaded_file_type = OurUtility.UPLOAD_HAC;
            string uploaded_date = string.Empty;
            string fileName = string.Empty;
            string link = string.Empty;
            string email_date_Format = config.Notification_Upload_HAC_Date_Format;

            string email_To_Menu = config.Notification_Upload_HAC_Specific_Email_To_Menu;
            string email_To_Access = config.Notification_Upload_HAC_Specific_Email_To_Access;

            // To based on "Calculation"
            List<Model_View_UserList_access_Menu> userList_To_Menu = Notification3Controller.UserList_access_Menu(email_To_Menu, email_To_Access, id, uploaded_file_type, ref uploaded_date, ref company, ref fileName, ref link);

            string email_Cc_Name = config.Notification_Upload_HAC_Specific_Email_Cc_Menu;
            string email_Cc_Access = config.Notification_Upload_HAC_Specific_Email_Cc_Access;

            // Cc based on "Calculation"
            List<Model_View_UserList_access_Menu> userList_Cc_Menu = Notification3Controller.UserList_access_Menu(email_Cc_Name, email_Cc_Access, id, uploaded_file_type, ref uploaded_date, ref company, ref fileName, ref link);

            uploaded_date = OurUtility.DateFormat(uploaded_date, email_date_Format);

            email_subject = email_subject.Replace("{Company}", company);
            email_subject = email_subject.Replace("{UploadedDate}", uploaded_date);
            email_subject = email_subject.Replace("{ID}", requestID_All);

            string email_body = System.IO.File.ReadAllText(TemplateFolder + @"\email_template_Upload_HAC_Specific_Email.html", Encoding.UTF8);

            if (config.Is_Production)
            {
                // -- this is Production

                // To based on "Calculation" above
                email_to = Notification3Controller.EmailAddress(userList_To_Menu);

                // Cc based on "Calculation" above
                email_cc = Notification3Controller.EmailAddress(userList_Cc_Menu);
            }
            else
            {
                // -- Not Production

                // To based on "Calculation" above
                email_to = Notification3Controller.EmailAddress_and_Name(userList_To_Menu);

                // Cc based on "Calculation" above
                email_cc = Notification3Controller.EmailAddress_and_Name(userList_Cc_Menu);

                // give information in Body of Email
                email_body = "---------------------------------------------------------------------"
                         + "<br/>Email To: " + email_to
                         + "<br/><br/>Email Cc: " + email_cc
                         + "<br/>---------------------------------------------------------------------"
                         + "<br/><br/>" + email_body;

                // "Testing - email Address"
                email_to = config.Email_To;

                // reset Cc
                email_cc = string.Empty;
            }

            email_body = email_body.Replace("{Company}", company);
            email_body = email_body.Replace("{UploadedDate}", uploaded_date);
            email_body = email_body.Replace("{request_ID}", requestID_All);
            email_body = email_body.Replace("{URLs}", requestID_Links);

            Attachment attachment = null;
            try
            {
                attachment = new Attachment(UploadFolder + link)
                {
                    Name = fileName
                };
            }
            catch { }

            return Send(string.Empty, email_subject, email_to, email_cc, email_body, attachment);
        }

        public JsonResult ActualLoading_Finalize()
        {
            UserX user = new UserX(Request);
            long id = OurUtility.ToInt64(OurUtility.ValueOf(Request, ".id"));
            string company = OurUtility.ValueOf(Request, "company");

            Configuration config = new Configuration();

            string email_subject = config.Notification_Actual_Loading_Finalize_Subject;
            string email_to = config.Email_To;
            string email_cc = string.Empty;

            string email_date_Format = config.Notification_Actual_Loading_Finalize_Date_Format;

            string email_To_Menu = config.Notification_Actual_Loading_Finalize_To_Menu;
            string email_To_Access = config.Notification_Actual_Loading_Finalize_To_Access;

            // To based on "Calculation"
            List<Model_View_UserList_access_Menu> userList_To_Menu = Notification3Controller.UserList_access_Menu(email_To_Menu, ref company);

            string email_Cc_Name = config.Notification_Actual_Loading_Finalize_Cc_Menu;
            string email_Cc_Access = config.Notification_Actual_Loading_Finalize_Cc_Access;

            // Cc based on "Calculation"
            List<Model_View_UserList_access_Menu> userList_Cc_Menu = Notification3Controller.UserList_access_Menu(email_Cc_Name, ref company);

            List<Model_View_Loading_Actual> loadingActuals = new List<Model_View_Loading_Actual>();

            DateTime dateFrom_O = DateTime.Now;
            string email_part_1 = string.Empty;

            string email_body = System.IO.File.ReadAllText(TemplateFolder + @"\email_template_ActualLoading_Finalize.html", Encoding.UTF8);

            try
            {
                string site = OurUtility.ValueOf(Request, "site");
                int site_i = OurUtility.ToInt32(site);
                string shipmentType = OurUtility.ValueOf(Request, "shipmentType");
                string dateFrom = OurUtility.ValueOf(Request, "dateFrom");
                string dateTo = OurUtility.ValueOf(Request, "dateTo");

                bool is_site_ALL = (string.IsNullOrEmpty(site) || site == "all");
                bool is_shipmentType_ALL = (string.IsNullOrEmpty(shipmentType) || shipmentType == "all");

                bool isAll_Text = true;
                string txt = Request["txt"];
                isAll_Text = string.IsNullOrEmpty(txt);

                DateTime dateTo_O = DateTime.Now.AddDays(1);

                try
                {
                    dateFrom_O = DateTime.Parse(dateFrom);
                }
                catch { }
                try
                {
                    dateTo_O = DateTime.Parse(dateTo).AddDays(1);
                }
                catch { }

                using (MERCY_Ctx db = new MERCY_Ctx())
                {
                    var dataQuery =
                            (
                                from d in db.Loading_Actual
                                join s in db.Sites on d.SiteId equals s.SiteId
                                where (d.Status == "Draft")
                                        && (is_site_ALL || d.SiteId == site_i)
                                        && (is_shipmentType_ALL || d.Shipment_Type == shipmentType)
                                        && d.Arrival_Time >= dateFrom_O
                                        && d.Arrival_Time < dateTo_O
                                        && (isAll_Text || d.No_Ref_Report.Contains(txt)
                                                    || d.No_Services_Trip.Contains(txt)
                                                    || d.TugName.Contains(txt)
                                                    || d.Barge_Name.Contains(txt))
                                orderby d.RecordId
                                select d
                            );

                    var items = dataQuery.ToList();

                    Model_View_Loading_Actual x = null;
                    int i = 0;
                    foreach (var d in items)
                    {
                        i++;

                        email_part_1 += string.Format(@"
                                                        <tr>
                                                            <td style='border-right: 1px solid black;border-bottom: 1px solid black;text-align:center;width:40px;'>{0}</td>
                                                            <td style='border-right: 1px solid black;border-bottom: 1px solid black;'>{1}</td>
                                                            <td style='border-right: 1px solid black;border-bottom: 1px solid black;text-align:center;'>{2}</td>
                                                            <td style='border-right: 1px solid black;border-bottom: 1px solid black;text-align:center;'>{3}</td>
                                                            <td style='border-right: 1px solid black;border-bottom: 1px solid black;text-align:center;'>{4}</td>
                                                            <td style='border-right: 1px solid black;border-bottom: 1px solid black;text-align:center;'></td>
                                                            <td style='border-right: 1px solid black;border-bottom: 1px solid black;font-weight:bold;text-align:center;width:40px;'></td>
                                                        </tr>
                                                        ", i, d.TugName
                                                         , OurUtility.DateFormat(d.Commenced_Loading, "dd-MMM-yyyy HH:mm")
                                                         , OurUtility.DateFormat(d.Completed_Loading, "dd-MMM-yyyy HH:mm")
                                                         , OurUtility.DateFormat(d.Departure, "dd-MMM-yyyy HH:mm")
                                                        );

                        x = new Model_View_Loading_Actual
                        {
                            RecordId = d.RecordId
                            ,
                            SiteId = d.SiteId
                            ,
                            CreatedOn = d.CreatedOn
                            ,
                            Status = d.Status
                            ,
                            SiteName = "s.SiteName"
                            ,
                            No_Ref_Report = d.No_Ref_Report
                            ,
                            No_Services_Trip = d.No_Services_Trip
                            ,
                            TugName = d.TugName
                            ,
                            Barge_Name = d.Barge_Name
                            ,
                            Route = d.Route
                            ,
                            Load_Type = d.Load_Type
                            ,
                            Arrival_Time = d.Arrival_Time
                            ,
                            Departure = d.Departure
                            ,
                            Coal_Quality_Spec = d.Coal_Quality_Spec
                            ,
                            Cargo_Loaded = ""
                        };

                        d.Status = "Finalized";

                        d.LastModifiedOn = DateTime.Now;
                        d.LastModifiedBy = user.UserId;
                    }

                    db.SaveChanges();

                    using (var connection = new SqlConnection(db.Database.Connection.ConnectionString))
                    {
                        connection.Open();

                        SqlCommand command = connection.CreateCommand();

                        string sql = string.Format(@"exec Report_Manual_Barging_and_Loading_Performance");

                        command.CommandText = sql;

                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                email_body = email_body.Replace("{Plan_TCM_Year}", OurUtility.ValueOf(reader, "Plan_TCM_Year"));
                                email_body = email_body.Replace("{Plan_TCM_Month_Budget}", OurUtility.ValueOf(reader, "Plan_TCM_Month_Budget"));
                                email_body = email_body.Replace("{Plan_TCM_Month_Outlook}", OurUtility.ValueOf(reader, "Plan_TCM_Month_Outlook"));
                                email_body = email_body.Replace("{Plan_BEK_Year}", OurUtility.ValueOf(reader, "Plan_BEK_Year"));
                                email_body = email_body.Replace("{Plan_BEK_Month_Budget}", OurUtility.ValueOf(reader, "Plan_BEK_Month_Budget"));
                                email_body = email_body.Replace("{Plan_BEK_Month_Outlook}", OurUtility.ValueOf(reader, "Plan_BEK_Month_Outlook"));
                                email_body = email_body.Replace("{Actual_TCM_Year}", OurUtility.ValueOf(reader, "Actual_TCM_Year"));
                                email_body = email_body.Replace("{Actual_TCM_Month}", OurUtility.ValueOf(reader, "Actual_TCM_Month"));
                                email_body = email_body.Replace("{Actual_BEK_Year}", OurUtility.ValueOf(reader, "Actual_BEK_Year"));
                                email_body = email_body.Replace("{Actual_BEK_Month}", OurUtility.ValueOf(reader, "Actual_BEK_Month"));
                                email_body = email_body.Replace("{Blending_Total}", OurUtility.ValueOf(reader, "Blending_Total"));
                                email_body = email_body.Replace("{Blending_MTD_TCM}", OurUtility.ValueOf(reader, "Blending_MTD_TCM"));
                                email_body = email_body.Replace("{Blending_MTD_BEK}", OurUtility.ValueOf(reader, "Blending_MTD_BEK"));
                                email_body = email_body.Replace("{Blending_YTD}", OurUtility.ValueOf(reader, "Blending_YTD"));
                                email_body = email_body.Replace("{Plan_MTD}", OurUtility.ValueOf(reader, "Plan_MTD"));
                                email_body = email_body.Replace("{Plan_YTD}", OurUtility.ValueOf(reader, "Plan_YTD"));
                                email_body = email_body.Replace("{Actual_MTD}", OurUtility.ValueOf(reader, "Actual_MTD"));
                                email_body = email_body.Replace("{Actual_YTD}", OurUtility.ValueOf(reader, "Actual_YTD"));
                            }
                        }

                        connection.Close();
                    }
                }
            }
            catch { }

            if (config.Is_Production)
            {
                // -- this is Production

                // To based on "Calculation" above
                email_to = Notification3Controller.EmailAddress(userList_To_Menu);

                // Cc based on "Calculation" above
                email_cc = Notification3Controller.EmailAddress(userList_Cc_Menu);
            }
            else
            {
                // -- Not Production

                // To based on "Calculation" above
                email_to = Notification3Controller.EmailAddress_and_Name(userList_To_Menu);

                // Cc based on "Calculation" above
                email_cc = Notification3Controller.EmailAddress_and_Name(userList_Cc_Menu);

                // give information in Body of Email
                email_body = "---------------------------------------------------------------------"
                         + "<br/>Email To: " + email_to
                         + "<br/><br/>Email Cc: " + email_cc
                         + "<br/>---------------------------------------------------------------------"
                         + "<br/><br/>" + email_body;

                // "Testing - email Address"
                email_to = config.Email_To;

                // reset Cc
                email_cc = string.Empty;
            }

            string url_image = config.FrontEnd_Url + @"/Api/Notification/Picture?f=";

            string data_company = string.Empty;
            string data_user_Fullname = string.Empty;
            string data_user_Email_Requestor = string.Empty;
            string data_type = string.Empty;
            string data_LetterNo = string.Empty;

            email_subject = email_subject.Replace("{ID}", id.ToString());
            email_subject = email_subject.Replace("{CreatedDate}", OurUtility.DateFormat(dateFrom_O, "dd-MMM-yyyy"));
            email_subject = email_subject.Replace("{ANALYSIS_TYPE}", data_type);
            email_subject = email_subject.Replace("{LETTER_NO}", data_LetterNo);

            //email_body = email_body.Replace("{Username}", email_Fullname);
            email_body = email_body.Replace("{MERCY_USER}", data_user_Fullname);
            email_body = email_body.Replace("{MERCY_COMPANY}", data_company);
            email_body = email_body.Replace("{MERCY_ID}", id.ToString());
            email_body = email_body.Replace("{MERCY_DATA_TYPE}", data_type);
            email_body = email_body.Replace("{MERCY_IMAGE_URL}", url_image);
            email_body = email_body.Replace("{MERCY_TYPE}", "Analysis Request");

            email_body = email_body.Replace("display:nonex;", "display:none;");
            email_body = email_body.Replace("{Year}", OurUtility.DateFormat(dateFrom_O, "yyyy"));
            email_body = email_body.Replace("{Month}", OurUtility.DateFormat(dateFrom_O, "MMMM"));
            email_body = email_body.Replace("{Site}", OurUtility.ValueOf(Request, "site_text"));
            email_body = email_body.Replace("{PART_1}", email_part_1);

            Attachment attachment = null;

            return Send(string.Empty, email_subject, email_to, email_cc, email_body, attachment);

            //var result = new { Success = true, Message = "", Time = DateTime.Now.ToString("dd-MMM-yyyy HH:mm:ss.ffff") };

            //return Json(result, JsonRequestBehavior.AllowGet);
        }

        public JsonResult ActualTunnel_Approve()
        {
            long id = OurUtility.ToInt64(OurUtility.ValueOf(Request, ".id"));
            string company = OurUtility.ValueOf(Request, "company");

            Configuration config = new Configuration();

            string email_subject = config.Notification_Actual_Tunnel_Approve_Subject;
            string email_to = config.Email_To;
            string email_cc = string.Empty;

            string email_date_Format = config.Notification_Actual_Tunnel_Approve_Date_Format;

            string email_To_Menu = config.Notification_Actual_Tunnel_Approve_To_Menu;
            string email_To_Access = config.Notification_Actual_Tunnel_Approve_To_Access;

            // To based on "Calculation"
            List<Model_View_UserList_access_Menu> userList_To_Menu = Notification3Controller.UserList_access_Menu(email_To_Menu, ref company);

            string email_Cc_Name = config.Notification_Actual_Tunnel_Approve_Cc_Menu;
            string email_Cc_Access = config.Notification_Actual_Tunnel_Approve_Cc_Access;

            // Cc based on "Calculation"
            List<Model_View_UserList_access_Menu> userList_Cc_Menu = Notification3Controller.UserList_access_Menu(email_Cc_Name, ref company);

            string data_company = string.Empty;
            string data_blending_Date = string.Empty;
            string data_shift = string.Empty;
            string data_tunnel = string.Empty;

            string changed_Status = string.Empty;
            Dictionary<string, string> tunnels = new Dictionary<string, string>();

            try
            {
                using (MERCY_Ctx db = new MERCY_Ctx())
                {
                    var dataQuery =
                            (
                                from d in db.HaulingRequest_Detail_PortionBlending
                                where d.RecordIdx == id
                                select new Model_View_TunnelManagement
                                {
                                    Company = d.Company
                                    ,
                                    Product = d.Product
                                    ,
                                    BlendingDate = d.BlendingDate
                                    ,
                                    Shift = d.Shift
                                    ,
                                    Hopper = d.Hopper
                                    ,
                                    Tunnel = d.Tunnel
                                }
                            );

                    var data = dataQuery.SingleOrDefault();

                    data_company = data.Company;
                    data_blending_Date = OurUtility.DateFormat(data.BlendingDate, "dd-MMM-yyyy");
                    data_shift = (data.Shift == 1 ? "A" : "B");
                    data_tunnel = data.Tunnel;

                    // Update email based on company data
                    company = data.Company;
                    userList_To_Menu = Notification3Controller.UserList_access_Menu(email_To_Menu, ref company);
                    userList_Cc_Menu = Notification3Controller.UserList_access_Menu(email_Cc_Name, ref company);

                    var dataQuery_Tunnels =
                            (
                                from d in db.Tunnels
                                select new
                                {
                                    d.TunnelId
                                    ,
                                    d.Name
                                }
                            );

                    var data_Tunnels = dataQuery_Tunnels.ToList();
                    foreach (var itemz in data_Tunnels)
                    {
                        try
                        {
                            tunnels.Add(itemz.TunnelId.ToString(), itemz.Name);
                        }
                        catch { }
                    }

                    using (var connection = new SqlConnection(db.Database.Connection.ConnectionString))
                    {
                        connection.Open();

                        SqlCommand command = connection.CreateCommand();

                        string ids = OurUtility.ValueOf(Request, "Changed_to_Approved");

                        foreach (var idx in ids.Split(new[] { "," }, StringSplitOptions.RemoveEmptyEntries))
                        {
                            try
                            {
                                command.CommandText = string.Format(@"
                                                                        select top 1 Convert(varchar(16), Time, 120) as Time_, Changed_Tunnel
                                                                        from Tunnel_Actual_History
                                                                        where Tunnel_Actual_Id = {0} and Changed_Tunnel <> ''
                                                                        order by RecordId_ desc
                                                                    ", idx);

                                using (SqlDataReader reader = command.ExecuteReader())
                                {
                                    while (reader.Read())
                                    {
                                        try
                                        {
                                            var changed_Tunnels = OurUtility.ValueOf(reader, "Changed_Tunnel").Split(',');

                                            changed_Status += string.Format(@"
                                                                        <tr>
                                                                            <td>{0}</td>
                                                                            <td>{1}</td>
                                                                            <td>{2}</td>
                                                                        </tr>
                                                                        ", OurUtility.DateFormat(OurUtility.ValueOf(reader, "Time_"), "dd-MMM-yyyy HH:mm")
                                                                        , OurUtility.ValueOf(tunnels, changed_Tunnels[0])
                                                                        , OurUtility.ValueOf(tunnels, changed_Tunnels[1])
                                                                        );
                                        }
                                        catch { }
                                    }
                                }
                            }
                            catch { }
                        }

                        connection.Close();
                    }

                    if (!string.IsNullOrEmpty(changed_Status))
                    {
                        changed_Status = string.Format(@"<table border='1'>
                                                            <tr>
                                                                <td style='text-align:center;width:150px'><b>Time</b></td>
                                                                <td style='text-align:center;width:100px'><b>prev Tunnel</b></td>
                                                                <td style='text-align:center;width:100px'><b>New Tunnel</b></td>
                                                            </tr>
                                                            {0}
                                                         </table>", changed_Status);
                    }
                }
            }
            catch { }

            email_subject = email_subject.Replace("{ID}", id.ToString());
            email_subject = email_subject.Replace("{Company}", data_company);
            email_subject = email_subject.Replace("{HaulingRequest}", data_blending_Date);
            email_subject = email_subject.Replace("{Shift}", data_shift);

            string email_body = System.IO.File.ReadAllText(TemplateFolder + @"\email_template_Tunnel_Management_Approve.html", Encoding.UTF8);

            if (config.Is_Production)
            {
                // -- this is Production

                // To based on "Calculation" above
                email_to = Notification3Controller.EmailAddress(userList_To_Menu);

                // Cc based on "Calculation" above
                email_cc = Notification3Controller.EmailAddress(userList_Cc_Menu);
            }
            else
            {
                // -- Not Production

                // To based on "Calculation" above
                email_to = Notification3Controller.EmailAddress_and_Name(userList_To_Menu);

                // Cc based on "Calculation" above
                email_cc = Notification3Controller.EmailAddress_and_Name(userList_Cc_Menu);

                // give information in Body of Email
                email_body = "---------------------------------------------------------------------"
                         + "<br/>Email To: " + email_to
                         + "<br/><br/>Email Cc: " + email_cc
                         + "<br/>---------------------------------------------------------------------"
                         + "<br/><br/>" + email_body;

                // "Testing - email Address"
                email_to = config.Email_To;

                // reset Cc
                email_cc = string.Empty;
            }

            string url_detail = config.FrontEnd_Url + @"/TunnelManagementv";

            email_body = email_body.Replace("{ID}", id.ToString());
            email_body = email_body.Replace("{Company}", data_company);
            email_body = email_body.Replace("{HaulingRequest}", data_blending_Date);
            email_body = email_body.Replace("{Shift}", data_shift);
            email_body = email_body.Replace("{Url}", url_detail);
            email_body = email_body.Replace("{DATA}", changed_Status);

            Attachment attachment = null;

            return Send(string.Empty, email_subject, email_to, email_cc, email_body, attachment);
        }

        public JsonResult ActualTunnel_Draft()
        {
            long id = OurUtility.ToInt64(OurUtility.ValueOf(Request, ".id"));
            string company = OurUtility.ValueOf(Request, "company");

            Configuration config = new Configuration();

            string email_subject = config.Notification_Actual_Tunnel_Draft_Subject;
            string email_to = config.Email_To;
            string email_cc = string.Empty;

            string email_date_Format = config.Notification_Actual_Tunnel_Draft_Date_Format;

            string email_To_Menu = config.Notification_Actual_Tunnel_Draft_To_Menu;
            string email_To_Access = config.Notification_Actual_Tunnel_Draft_To_Access;

            // To based on "Calculation"
            List<Model_View_UserList_access_Menu> userList_To_Menu = Notification3Controller.UserList_access_Menu(email_To_Menu, ref company);

            string email_Cc_Name = config.Notification_Actual_Tunnel_Draft_Cc_Menu;
            string email_Cc_Access = config.Notification_Actual_Tunnel_Draft_Cc_Access;

            // Cc based on "Calculation"
            List<Model_View_UserList_access_Menu> userList_Cc_Menu = Notification3Controller.UserList_access_Menu(email_Cc_Name, ref company);

            string data_company = string.Empty;
            string data_blending_Date = string.Empty;
            string data_shift = string.Empty;
            string data_tunnel = string.Empty;

            string changed_Status = string.Empty;
            Dictionary<string, string> tunnels = new Dictionary<string, string>();

            try
            {
                using (MERCY_Ctx db = new MERCY_Ctx())
                {
                    var dataQuery =
                            (
                                from d in db.HaulingRequest_Detail_PortionBlending
                                where d.RecordIdx == id
                                select new Model_View_TunnelManagement
                                {
                                    Company = d.Company
                                    ,
                                    Product = d.Product
                                    ,
                                    BlendingDate = d.BlendingDate
                                    ,
                                    Shift = d.Shift
                                    ,
                                    Hopper = d.Hopper
                                    ,
                                    Tunnel = d.Tunnel
                                }
                            );

                    var data = dataQuery.SingleOrDefault();

                    data_company = data.Company;
                    data_blending_Date = OurUtility.DateFormat(data.BlendingDate, "dd-MMM-yyyy");
                    data_shift = (data.Shift == 1 ? "A" : "B");
                    data_tunnel = data.Tunnel;

                    // Update email based on company data
                    company = data.Company;
                    userList_To_Menu = Notification3Controller.UserList_access_Menu(email_To_Menu, ref company);
                    userList_Cc_Menu = Notification3Controller.UserList_access_Menu(email_Cc_Name, ref company);

                    var dataQuery_Tunnels =
                            (
                                from d in db.Tunnels
                                select new
                                {
                                    d.TunnelId
                                    ,
                                    d.Name
                                }
                            );

                    var data_Tunnels = dataQuery_Tunnels.ToList();
                    foreach (var itemz in data_Tunnels)
                    {
                        try
                        {
                            tunnels.Add(itemz.TunnelId.ToString(), itemz.Name);
                        }
                        catch { }
                    }

                    using (var connection = new SqlConnection(db.Database.Connection.ConnectionString))
                    {
                        connection.Open();

                        SqlCommand command = connection.CreateCommand();

                        string ids = OurUtility.ValueOf(Request, "Changed_to_Draft");

                        foreach (var idx in ids.Split(new[] { "," }, StringSplitOptions.RemoveEmptyEntries))
                        {
                            try
                            {
                                command.CommandText = string.Format(@"
                                                                        select top 1 Convert(varchar(16), Time, 120) as Time_, Changed_Tunnel
                                                                        from Tunnel_Actual_History
                                                                        where Tunnel_Actual_Id = {0} and Changed_Tunnel <> ''
                                                                        order by RecordId_ desc
                                                                    ", idx);

                                using (SqlDataReader reader = command.ExecuteReader())
                                {
                                    while (reader.Read())
                                    {
                                        try
                                        {
                                            var changed_Tunnels = OurUtility.ValueOf(reader, "Changed_Tunnel").Split(',');

                                            changed_Status += string.Format(@"
                                                                        <tr>
                                                                            <td>{0}</td>
                                                                            <td>{1}</td>
                                                                            <td>{2}</td>
                                                                        </tr>
                                                                        ", OurUtility.DateFormat(OurUtility.ValueOf(reader, "Time_"), "dd-MMM-yyyy HH:mm")
                                                                        , OurUtility.ValueOf(tunnels, changed_Tunnels[0])
                                                                        , OurUtility.ValueOf(tunnels, changed_Tunnels[1])
                                                                        );
                                        }
                                        catch { }
                                    }
                                }
                            }
                            catch { }
                        }

                        connection.Close();
                    }

                    if (!string.IsNullOrEmpty(changed_Status))
                    {
                        changed_Status = string.Format(@"<table border='1'>
                                                            <tr>
                                                                <td style='text-align:center;width:150px'><b>Time</b></td>
                                                                <td style='text-align:center;width:100px'><b>prev Tunnel</b></td>
                                                                <td style='text-align:center;width:100px'><b>New Tunnel</b></td>
                                                            </tr>
                                                            {0}
                                                         </table>", changed_Status);
                    }
                }
            }
            catch { }

            email_subject = email_subject.Replace("{ID}", id.ToString());
            email_subject = email_subject.Replace("{Company}", data_company);
            email_subject = email_subject.Replace("{HaulingRequest}", data_blending_Date);
            email_subject = email_subject.Replace("{Shift}", data_shift);

            string email_body = System.IO.File.ReadAllText(TemplateFolder + @"\email_template_Tunnel_Management_Draft.html", Encoding.UTF8);

            if (config.Is_Production)
            {
                // -- this is Production

                // To based on "Calculation" above
                email_to = Notification3Controller.EmailAddress(userList_To_Menu);

                // Cc based on "Calculation" above
                email_cc = Notification3Controller.EmailAddress(userList_Cc_Menu);
            }
            else
            {
                // -- Not Production

                // To based on "Calculation" above
                email_to = Notification3Controller.EmailAddress_and_Name(userList_To_Menu);

                // Cc based on "Calculation" above
                email_cc = Notification3Controller.EmailAddress_and_Name(userList_Cc_Menu);

                // give information in Body of Email
                email_body = "---------------------------------------------------------------------"
                         + "<br/>Email To: " + email_to
                         + "<br/><br/>Email Cc: " + email_cc
                         + "<br/>---------------------------------------------------------------------"
                         + "<br/><br/>" + email_body;

                // "Testing - email Address"
                email_to = config.Email_To;

                // reset Cc
                email_cc = string.Empty;
            }

            string url_detail = config.FrontEnd_Url + @"/TunnelManagementv";

            email_body = email_body.Replace("{ID}", id.ToString());
            email_body = email_body.Replace("{Company}", data_company);
            email_body = email_body.Replace("{HaulingRequest}", data_blending_Date);
            email_body = email_body.Replace("{Shift}", data_shift);
            email_body = email_body.Replace("{Url}", url_detail);
            email_body = email_body.Replace("{DATA}", changed_Status);

            Attachment attachment = null;

            return Send(string.Empty, email_subject, email_to, email_cc, email_body, attachment);
        }

        public JsonResult Tunnel_Approve()
        {
            long id = OurUtility.ToInt64(OurUtility.ValueOf(Request, ".id"));

            Configuration config = new Configuration();

            string email_subject = config.Notification_Tunnel_Approve_Subject;
            string email_to = config.Email_To;
            string email_cc = string.Empty;

            string email_date_Format = config.Notification_Tunnel_Approve_Date_Format;

            string data_company = string.Empty;
            string data_effective_date = string.Empty;
            string data_product = string.Empty;
            string data_tunnel = string.Empty;
            var isProductChange = false;

            try
            {
                using (MERCY_Ctx db = new MERCY_Ctx())
                {
                    var dataQuery =
                            (
                                from d in db.Tunnels
                                join p in db.Products on d.ProductId equals p.ProductId
                                where d.TunnelId == id
                                select new
                                {
                                    d.CompanyCode,
                                    p.ProductName,
                                    d.Name,
                                    p.ProductId,
                                    d.TunnelHistories
                                }
                            );

                    var data = dataQuery.SingleOrDefault();

                    var lastActiveData = data.TunnelHistories.Where(th => th.IsActive).OrderByDescending(th => th.CreatedOn).FirstOrDefault();
                    if (lastActiveData != null && data.ProductId != lastActiveData.ProductId)
                    {
                        isProductChange = true;
                    }

                    data_company = data.CompanyCode;
                    data_product = data.ProductName;
                    data_tunnel = data.Name;


                }
            }
            catch { }

            string email_To_Menu = "Tunnel";
            string permissionTo = "isadd";
            string permissionCc = "isedit";

            List<Model_View_UserList_access_Menu> userList_To_Menu = Notification3Controller.UserListApproveAndEditTunnel(email_To_Menu, permissionTo, data_company);

            string email_Cc_Menu = "Portion Blending";

            List<Model_View_UserList_access_Menu> userList_Cc_Menu = Notification3Controller.UserListApproveAndEditTunnel(email_Cc_Menu, permissionCc, data_company);

            if (isProductChange)
            {
                string email_body = System.IO.File.ReadAllText(TemplateFolder + @"\email_template_Tunnel_Edit.html", Encoding.UTF8);

                if (config.Is_Production)
                {
                    // -- this is Production

                    // To based on "Calculation" above
                    email_to = Notification3Controller.EmailAddress(userList_To_Menu);

                    // Cc based on "Calculation" above
                    email_cc = Notification3Controller.EmailAddress(userList_Cc_Menu);
                }
                else
                {
                    // -- Not Production

                    // To based on "Calculation" above
                    email_to = Notification3Controller.EmailAddress_and_Name(userList_To_Menu);

                    // Cc based on "Calculation" above
                    email_cc = Notification3Controller.EmailAddress_and_Name(userList_Cc_Menu);

                    // give information in Body of Email
                    email_body = "---------------------------------------------------------------------"
                             + "<br/>Email To: " + email_to
                             + "<br/><br/>Email Cc: " + email_cc
                             + "<br/>---------------------------------------------------------------------"
                             + "<br/><br/>" + email_body;

                    // "Testing - email Address"
                    email_to = config.Email_To;

                    // reset Cc
                    email_cc = string.Empty;
                }

                string url_detail = config.FrontEnd_Url + @"/Tunnelv";

                email_body = email_body.Replace("{Tunnel}", data_tunnel);
                email_body = email_body.Replace("{Company}", data_company);
                email_body = email_body.Replace("{ProductName}", data_product);
                email_body = email_body.Replace("{Url}", url_detail);

                email_subject = "Acknowledge Request New Product in Tunnel " + data_company;

                Attachment attachment = null;

                return Send(string.Empty, email_subject, email_to, email_cc, email_body, attachment);
            }

            return null;
        }

        public JsonResult Tunnel_Edit()
        {
            long id = OurUtility.ToInt64(OurUtility.ValueOf(Request, ".id"));

            Configuration config = new Configuration();

            string email_subject = config.Notification_Tunnel_Approve_Subject;
            string email_to = config.Email_To;
            string email_cc = string.Empty;

            string email_date_Format = config.Notification_Tunnel_Approve_Date_Format;

       

            string data_company = string.Empty;
            string data_effective_date = string.Empty;
            string data_product = string.Empty;
            string data_tunnel = string.Empty;

            //Dictionary<string, string> tunnels = new Dictionary<string, string>();

            try
            {
                using (MERCY_Ctx db = new MERCY_Ctx())
                {
                    var dataQuery =
                            (
                                from d in db.Tunnels
                                join p in db.Products on d.ProductId equals p.ProductId
                                where d.TunnelId == id
                                select new
                                {
                                    d.CompanyCode
                                    ,
                                    p.ProductName
                                    ,
                                    d.EffectiveDate
                                    ,
                                    d.Name
                                }
                            );

                    var data = dataQuery.SingleOrDefault();

                    data_company = data.CompanyCode;
                    data_effective_date = OurUtility.DateFormat(data.EffectiveDate, "dd-MMM-yyyy");
                    data_product = data.ProductName;
                    data_tunnel = data.Name;

                }
            }
            catch { }

            string email_To_Menu = "Portion Blending";
            string permissionTo = "isedit";
            string permissionCc = "isadd";

            List<Model_View_UserList_access_Menu> userList_To_Menu = Notification3Controller.UserListApproveAndEditTunnel(email_To_Menu, permissionTo, data_company);

            string email_Cc_Menu = "Tunnel";

            List<Model_View_UserList_access_Menu> userList_Cc_Menu = Notification3Controller.UserListApproveAndEditTunnel(email_Cc_Menu, permissionCc, data_company);

            string email_body = System.IO.File.ReadAllText(TemplateFolder + @"\email_template_Tunnel_Approve.html", Encoding.UTF8);

            if (config.Is_Production)
            {
                // -- this is Production

                // To based on "Calculation" above
                email_to = Notification3Controller.EmailAddress(userList_To_Menu);

                // Cc based on "Calculation" above
                email_cc = Notification3Controller.EmailAddress(userList_Cc_Menu);
            }
            else
            {
                // -- Not Production

                // To based on "Calculation" above
                email_to = Notification3Controller.EmailAddress_and_Name(userList_To_Menu);

                // Cc based on "Calculation" above
                email_cc = Notification3Controller.EmailAddress_and_Name(userList_Cc_Menu);

                // give information in Body of Email
                email_body = "---------------------------------------------------------------------"
                         + "<br/>Email To: " + email_to
                         + "<br/><br/>Email Cc: " + email_cc
                         + "<br/>---------------------------------------------------------------------"
                         + "<br/><br/>" + email_body;

                // "Testing - email Address"
                email_to = config.Email_To;

                // reset Cc
                email_cc = string.Empty;
            }

            string url_detail = config.FrontEnd_Url + @"/Tunnelv";

            email_body = email_body.Replace("{Tunnel}", data_tunnel);
            email_body = email_body.Replace("{Company}", data_company);
            email_body = email_body.Replace("{ProductName}", data_product);
            email_body = email_body.Replace("{EffectiveDate}", data_effective_date);
            email_body = email_body.Replace("{Url}", url_detail);

            email_subject = "New Product Assignment in Tunnel " + data_company;

            Attachment attachment = null;

            return Send(string.Empty, email_subject, email_to, email_cc, email_body, attachment);
        }

        public JsonResult Analysis_Result_Validate([Bind(Prefix = "sampleIds[]")] List<int> sampleIds)
        {
            long id = OurUtility.ToInt64(OurUtility.ValueOf(Request, ".id"));

            Configuration config = new Configuration();

            string email_subject = config.Notification_Analysis_Result_Validate_Subject;
            string email_to = config.Email_To;
            string email_cc = string.Empty;

            string email_To_Menu = "Validation & Approval";
            string permissionTo = "isapprove";

         

            string data_company = string.Empty;
            string data_client = string.Empty;
            string data_project = string.Empty;
            string data_date_of_job = string.Empty;
            string data_received_date = string.Empty;
            string data_job_number = string.Empty;

            string email_body = System.IO.File.ReadAllText(TemplateFolder + @"\email_template_Analysis_Result_Validation.html", Encoding.UTF8);
            Attachment attachment = null;
            try
            {
                var aaa = sampleIds[0];
                using (MERCY_Ctx db = new MERCY_Ctx())
                {
                    var dataQuery =
                            (
                                from s in db.Samples
                                join c in db.Clients on s.ClientId equals c.Id
                                join p in db.Projects on s.ProjectId equals p.Id
                                join aj in db.AnalysisJobs on s.JobNumberId equals aj.Id
                                where s.Id == aaa
                                select new
                                {
                                    s.CompanyCode,
                                    clientName = c.Name,
                                    projectName = p.Name,
                                    s.DateOfJob,
                                    aj.ReceivedDate,
                                    s.JobNumberId
                                }
                            );

                    var data = dataQuery.SingleOrDefault();

                    data_company = data.CompanyCode;
                    data_client = data.clientName;
                    data_project = data.projectName;
                    data_date_of_job = data.DateOfJob.ToString();
                    data_received_date = data.ReceivedDate.ToString();
                    data_job_number = data.JobNumberId.ToString();
                    List<Model_View_UserList_access_Menu> userList_To_Menu = Notification3Controller.UserListValidateAnalysisResult(email_To_Menu, permissionTo, data_company);
                    if (config.Is_Production)
                    {
                        // -- this is Production

                        // To based on "Calculation" above
                        email_to = Notification3Controller.EmailAddress(userList_To_Menu);
                    }
                    else
                    {
                        // -- Not Production

                        // To based on "Calculation" above
                        email_to = Notification3Controller.EmailAddress_and_Name(userList_To_Menu);

                        // give information in Body of Email
                        email_body = "---------------------------------------------------------------------"
                                 + "<br/>Email To: " + email_to
                                 + "<br/><br/>Email Cc: " + email_cc
                                 + "<br/>---------------------------------------------------------------------"
                                 + "<br/><br/>" + email_body;

                        // "Testing - email Address"
                        email_to = config.Email_To;

                        // reset Cc
                        email_cc = string.Empty;
                    }

                    string url_detail = config.FrontEnd_Url + @"/SampleValidationApprovalv";

                    email_body = email_body.Replace("{0}", data_company);
                    email_body = email_body.Replace("{1}", data_client);
                    email_body = email_body.Replace("{2}", data_project);
                    email_body = email_body.Replace("{3}", data_date_of_job);
                    email_body = email_body.Replace("{4}", data_received_date);

                    email_subject = "[MERCY] Need your approval for Job" + data_job_number + "Company" + data_company;

                    var dataQueryJob =
                            (
                                from aj in db.AnalysisJobs
                                join s in db.Samples on aj.Id equals s.JobNumberId
                                join u in db.Users on aj.ValidatedBy equals u.UserId
                                where sampleIds.Contains(s.Id)
                                select new
                                {
                                    s.ExternalId,
                                    s.Id,
                                    aj.ValidatedDate,
                                    u.FullName
                                }
                            );

                    var dataJob = dataQueryJob.ToList();
                    
                    string emailDetail = "";

                    string detailTemplate = System.IO.File.ReadAllText(TemplateFolder + @"\email_template_Analysis_Result_Validation_detail.html", Encoding.UTF8);

                    foreach (var item in dataJob)
                    {
                        emailDetail += string.Format(detailTemplate, item.ExternalId, item.Id, item.FullName, item.ValidatedDate);
                    }
                    email_body = email_body.Replace("##--Detail--##", emailDetail);
                }

            }
            catch (Exception ex)
            {
                var result = new { Success = false, ex.Message, MessageDetail = string.Empty, Version = Configuration.VERSION, Items = string.Empty, Total = 0 };
                return Json(result, JsonRequestBehavior.AllowGet);
            }

            return Send(string.Empty, email_subject, email_to, email_cc, email_body, attachment);
        }
        public JsonResult Analysis_Result_Approve([Bind(Prefix = "sampleIds[]")] List<int> sampleIds)
        {
            long id = OurUtility.ToInt64(OurUtility.ValueOf(Request, ".id"));

            Configuration config = new Configuration();

            string email_subject = config.Notification_Analysis_Result_Approve_Subject;
            string email_to = config.Email_To;
            string email_cc = string.Empty;

            string email_To_Menu = "Validation & Approval";
            string permissionTo = "isapprove";

            string data_company = string.Empty;
            string data_client = string.Empty;
            string data_project = string.Empty;
            string data_date_of_job = string.Empty;
            string data_received_date = string.Empty;
            string data_job_number = string.Empty;

            string email_body = System.IO.File.ReadAllText(TemplateFolder + @"\email_template_Analysis_Result_Approval.html", Encoding.UTF8);
            Attachment attachment = null;
            try
            {
                var aaa = sampleIds[0];
                using (MERCY_Ctx db = new MERCY_Ctx())
                {
                    var dataQuery =
                            (
                                from s in db.Samples
                                join c in db.Clients on s.ClientId equals c.Id
                                join p in db.Projects on s.ProjectId equals p.Id
                                join aj in db.AnalysisJobs on s.JobNumberId equals aj.Id
                                where s.Id == aaa
                                select new
                                {
                                    s.CompanyCode,
                                    clientName = c.Name,
                                    projectName = p.Name,
                                    s.DateOfJob,
                                    aj.ReceivedDate,
                                    s.JobNumberId
                                }
                            );

                    var data = dataQuery.SingleOrDefault();

                    data_company = data.CompanyCode;
                    data_client = data.clientName;
                    data_project = data.projectName;
                    data_date_of_job = data.DateOfJob.ToString();
                    data_received_date = data.ReceivedDate.ToString();
                    data_job_number = data.JobNumberId.ToString();

                    List<Model_View_UserList_access_Menu> userList_To_Menu = Notification3Controller.UserListApprovalAnalysisResult(email_To_Menu, permissionTo, data_company);

                    if (config.Is_Production)
                    {
                        // -- this is Production

                        // To based on "Calculation" above
                        email_to = Notification3Controller.EmailAddress(userList_To_Menu);
                    }
                    else
                    {
                        // -- Not Production

                        // To based on "Calculation" above
                        email_to = Notification3Controller.EmailAddress_and_Name(userList_To_Menu);

                        // give information in Body of Email
                        email_body = "---------------------------------------------------------------------"
                                 + "<br/>Email To: " + email_to
                                 + "<br/><br/>Email Cc: " + email_cc
                                 + "<br/>---------------------------------------------------------------------"
                                 + "<br/><br/>" + email_body;

                        // "Testing - email Address"
                        email_to = config.Email_To;

                        // reset Cc
                        email_cc = string.Empty;
                    }

                    string url_detail = config.FrontEnd_Url + @"/SampleValidationApprovalv";

                    email_body = email_body.Replace("{0}", data_company);
                    email_body = email_body.Replace("{1}", data_client);
                    email_body = email_body.Replace("{2}", data_project);
                    email_body = email_body.Replace("{3}", data_date_of_job);
                    email_body = email_body.Replace("{4}", data_received_date);

                    email_subject = "[MERCY] Was Approved for Job " + data_job_number + "Company " + data_company;

                    var dataQueryJob =
                            (
                                from aj in db.AnalysisJobs
                                join s in db.Samples on aj.Id equals s.JobNumberId
                                join u in db.Users on aj.ValidatedBy equals u.UserId
                                where sampleIds.Contains(s.Id)
                                select new
                                {
                                    s.ExternalId,
                                    s.Id,
                                    aj.ValidatedDate,
                                    u.FullName
                                }
                            );

                    var dataJob = dataQueryJob.ToList();

                    string emailDetail = "";

                    string detailTemplate = System.IO.File.ReadAllText(TemplateFolder + @"\email_template_Analysis_Result_Approval_detail.html", Encoding.UTF8);

                    foreach (var item in dataJob)
                    {
                        emailDetail += string.Format(detailTemplate, item.ExternalId, item.Id, item.FullName, item.ValidatedDate);
                    }
                    email_body = email_body.Replace("##--Detail--##", emailDetail);
                }

            }
            catch (Exception ex)
            {
                var result = new { Success = false, ex.Message, MessageDetail = string.Empty, Version = Configuration.VERSION, Items = string.Empty, Total = 0 };
                return Json(result, JsonRequestBehavior.AllowGet);
            }

            return Send(string.Empty, email_subject, email_to, email_cc, email_body, attachment);
        }

        public JsonResult RequestSubmission()
        {
            long id = OurUtility.ToInt64(OurUtility.ValueOf(Request, ".id"));

            Configuration config = new Configuration();

            string email_subject = config.Notification_Request_Submission_Subject;
            string email_to = config.Email_To;
            string email_cc = string.Empty;

            string email_To_Menu = "Loading Request";
            string permissionTo = "isemail";
            string companyCode = string.Empty;
            string email_body = System.IO.File.ReadAllText(TemplateFolder + @"\email_template_Request_Submission.html", Encoding.UTF8);
            string email_detail_fix = System.IO.File.ReadAllText(TemplateFolder + @"\email_template_Request_Submission_Detail.html", Encoding.UTF8);
            Attachment attachment = null;
            try
            {
                List<Model_Loading_Plan> items = new List<Model_Loading_Plan>();
                Model_Loading_Plan item = null;
                using (MERCY_Ctx db = new MERCY_Ctx())
                {
                    using (var connection = new SqlConnection(db.Database.Connection.ConnectionString))
                    {
                        connection.Open();

                        SqlCommand command_Request = connection.CreateCommand();

                        string sql = string.Empty;

                        sql = string.Format(@"
                            select p.RecordId
                                    , p.Loading_Plan_Id
                                    , s.SiteName as Site_Str
                                    , p.Company
                                    , p.Shipment_Type
                                    , p.Buyer
                                    , p.Vessel
                                    , p.Destination
                                    , '' as FinalProduct
                                    , '' as Customer
                                    , lp.ETA as ETA_Vessel_MBR
                                    , p.CreatedOn
									, lrlpdetail.Tug
									, lrlpdetail.Barge
									, Convert(varchar(20), lrlpdetail.EstimateStartLoading, 120) as EstimateStartLoading
                                    , lrlpdetail.Quantity
									, lrlpdetail.Product
                                    , lr.CreatedOn_Date_Only as CreatedDate
									, lpdb.RecordId as Detail_Barge_Id
                                from Loading_Request_Loading_Plan p
                                    , Site s
                                    , Company c
									, Loading_Plan lp
									, Loading_Request_Loading_Plan_Detail_Barge lrlpdetail
                                    , Loading_Request lr
									, Loading_Plan_Detail_Barge lpdb
                                where p.Request_Id = {0}
                                and p.SiteId = s.SiteId
                                and p.Company = c.CompanyCode
								and p.Loading_Plan_Id = lp.RecordId
								and p.Request_Id = lrlpdetail.Request_Id
								and p.Loading_Plan_Id = lrlpdetail.Loading_Plan_Id
                                and p.Request_Id = lr.RecordId
								and p.Loading_Plan_Id = lpdb.Loading_Plan_Id
								and lrlpdetail.Tug = lpdb.Tug
								and lrlpdetail.Barge = lpdb.Barge
                                order by p.Loading_Plan_Id", id);

                        command_Request.CommandText = sql;

                        using (SqlDataReader reader_Request = command_Request.ExecuteReader())
                        {
                            int request_Count = 0;
                            int previous_Request = request_Count;

                            while (reader_Request.Read())
                            {
                                request_Count++;

                                item = new Model_Loading_Plan
                                {
                                    Request_Id = OurUtility.ValueOf(reader_Request, "Request_Id")
                                };

                                item.RecordId = OurUtility.ToInt64(OurUtility.ValueOf(reader_Request, "RecordId"));
                                item.Loading_Plan_Id = OurUtility.ValueOf(reader_Request, "Loading_Plan_Id");
                                item.Site_Str = OurUtility.ValueOf(reader_Request, "Site_Str");
                                item.Company = OurUtility.ValueOf(reader_Request, "Company");
                                item.Shipment_Type = OurUtility.ValueOf(reader_Request, "Shipment_Type");
                                item.Destination = OurUtility.ValueOf(reader_Request, "Destination");
                                item.ETA_Vessel_MBR = OurUtility.ToDateTime(OurUtility.ValueOf(reader_Request, "ETA_Vessel_MBR")).Date.ToString("dd-MM-yyyy");
                                item.CreatedOn = OurUtility.ToDateTime(OurUtility.ValueOf(reader_Request, "CreatedOn"));
                                item.CreatedOn_Str = OurUtility.DateFormat(item.CreatedOn, @"dd-MMM-yyyy HH:mm:ss");
                                item.CreatedOn_Str2 = OurUtility.DateFormat(item.CreatedOn, "dd-MMM-yyyy");
                                item.Tug = OurUtility.ValueOf(reader_Request, "Tug");
                                item.Barge = OurUtility.ValueOf(reader_Request, "Barge");
                                item.Estimate_Start_Loading = OurUtility.ToDateTime(OurUtility.ValueOf(reader_Request, "EstimateStartLoading")).Date.ToString("dd-MMM-yyyy");
                                item.Estimate_Quantity = OurUtility.ValueOf(reader_Request, "Quantity");
                                item.Product = OurUtility.ValueOf(reader_Request, "Product");
                                item.Vessel = OurUtility.ValueOf(reader_Request, "Vessel");
                                item.Buyer = OurUtility.ValueOf(reader_Request, "Buyer");
                                item.CreatedOn = OurUtility.ToDateTime(OurUtility.ValueOf(reader_Request, "CreatedDate"));
                                item.Detail_Barge_Id = OurUtility.ToInt64(OurUtility.ValueOf(reader_Request, "Detail_Barge_Id"));

                                var loadingPlanId = Convert.ToInt64(item.Loading_Plan_Id);
                                var blendingFormulas = db.Loading_Plan_Detail_Barge_Blending_Formula
                                    .Where(bf => bf.Loading_Plan_Id == loadingPlanId && bf.Detail_Barge_Id == item.Detail_Barge_Id)
                                    .Select(x => new { Tunnel = x.Tunnel, Portion = x.Portion })
                                    .ToList();

                                foreach (var blendingPortion in blendingFormulas)
                                {
                                    switch (blendingPortion.Tunnel)
                                    {
                                        case "TN1":
                                            item.TN1 = blendingPortion.Portion;
                                            break;
                                        case "TN2":
                                            item.TN2 = blendingPortion.Portion;
                                            break;
                                        case "TN3":
                                            item.TN3 = blendingPortion.Portion;
                                            break;
                                        case "TN4":
                                            item.TN4 = blendingPortion.Portion;
                                            break;   
                                        case "TN5":  
                                            item.TN5 = blendingPortion.Portion;
                                            break;   
                                        case "TN6":  
                                            item.TN6 = blendingPortion.Portion;
                                            break;
                                        case "BTN1":
                                            item.BTN1 = blendingPortion.Portion;
                                            break;    
                                        case "BTN2":  
                                            item.BTN2 = blendingPortion.Portion;
                                            break;    
                                        case "BTN3":  
                                            item.BTN3 = blendingPortion.Portion;
                                            break;    
                                        case "BTN4":  
                                            item.BTN4 = blendingPortion.Portion;
                                            break;    
                                        case "BTN5":  
                                            item.BTN5 = blendingPortion.Portion;
                                            break;    
                                        case "BTN6":  
                                            item.BTN6 = blendingPortion.Portion;
                                            break;
                                    }
                                }
                                
                                if(companyCode != string.Empty)
                                {
                                    companyCode = item.Company;
                                }
                                items.Add(item);

                                previous_Request = request_Count;
                            }

                            email_subject = email_subject.Replace("{date}", items[0].CreatedOn.ToString("dd MMMM yyyy"));
                        }

                        connection.Close();


                        List<Model_View_UserList_access_Menu> userList_To_Menu = Notification3Controller.UserListApprovalAnalysisResult(email_To_Menu, permissionTo, companyCode);

                        if (config.Is_Production)
                        {
                            // -- this is Production

                            // To based on "Calculation" above
                            email_to = Notification3Controller.EmailAddress(userList_To_Menu);
                        }
                        else
                        {
                            // -- Not Production

                            // To based on "Calculation" above
                            email_to = Notification3Controller.EmailAddress_and_Name(userList_To_Menu);

                            // give information in Body of Email
                            email_body = "---------------------------------------------------------------------"
                                     + "<br/>Email To: " + email_to
                                     + "<br/><br/>Email Cc: " + email_cc
                                     + "<br/>---------------------------------------------------------------------"
                                     + "<br/><br/>" + email_body;

                            // "Testing - email Address"
                            email_to = config.Email_To;

                            // reset Cc
                            email_cc = string.Empty;
                        }

                        string url_detail = config.FrontEnd_Url + string.Format(@"/LoadingRequestv/Form?.id={0}&Loading_Plan_Id={1}&RecordId={2}", id, item.Loading_Plan_Id, item.RecordId);

                        int loadingPlanCount = 0;
                        string email_detail_add = "";
                        string email_detail = "";

                        var tunnelProduct =
                            (
                                from t in db.Tunnels
                                join p in db.Products on t.ProductId equals p.ProductId
                                where t.IsActive
                                select new
                                {
                                    TunnelName = t.Name,
                                    ProductName = p.ProductName
                                }
                            ).ToList();

                        var productDetail = tunnelProduct.GroupBy(g => g.ProductName)
                                    .Select(s => new {
                                        ProductName = s.Key,
                                        TunnelName = string.Join("-", s.Select(ss => ss.TunnelName)),
                                        ColumnCount = s.Count()
                                    }).ToList();

                        string productTable = "";
                        string tunnelTable = "";
                        string tunnelNameSort = "";
                        foreach(var product in productDetail)
                        {
                            productTable += string.Format("<td style=\"border-right: 1px solid black; border-bottom: 1px solid black; font-weight:bold; text-align:center; \" colspan=\"{0}\">{1}</td>", product.ColumnCount, product.ProductName);
                            string[] tunnel = product.TunnelName.Split('-');
                            tunnelNameSort += tunnelNameSort == "" ? product.TunnelName : "-" + product.TunnelName;
                            for (int i = 0; i < product.ColumnCount; i++)
                            {
                                tunnelTable += string.Format("<td style=\"border-right: 1px solid black; border-bottom: 1px solid black; font-weight:bold; text-align:center; \">{0}</td>", tunnel[i]);
                            }
                        }

                        email_body = email_body.Replace("##--Product--##", productTable).Replace("##--Tunnel--##", tunnelTable);
                        
                        var refBLU = db.UPLOAD_Barge_Line_Up.ToList();
                        var latestBLU = refBLU.Where(x => x.CreatedOn_Date_Only == refBLU.Max(y => y.CreatedOn_Date_Only));

                        for (int i = 0; i < items.Count(); i++)
                        {
                            email_detail = email_detail_fix;
                            if (i >= 1 && (items[i].Loading_Plan_Id == items[i - 1].Loading_Plan_Id))
                            {
                                email_detail = email_detail.Replace("companyCode", "");
                                email_detail = email_detail.Replace("ETA_Vessel_MBR", "");
                            }
                            else
                            {
                                loadingPlanCount = items.Where(x => x.Loading_Plan_Id == items[i].Loading_Plan_Id).Count(); 
                                email_detail = email_detail.Replace("companyCode", string.Format("<td style=\"border - right: 1px solid black; border - bottom: 1px solid black; font - weight:bold; text - align:center; \" rowspan=\"{0}\">{1}</td>", loadingPlanCount, items[i].Company));
                                email_detail = email_detail.Replace("ETA_Vessel_MBR", string.Format("<td style=\"border - right: 1px solid black; border - bottom: 1px solid black; font - weight:bold; text - align:center; \" rowspan=\"{0}\">{1}</td>", loadingPlanCount, items[i].ETA_Vessel_MBR));
                            }

                            var latestBLUVessel = latestBLU.Where(x => x.VesselName == items[i].Vessel).FirstOrDefault();
                            string product = items[i].Vessel == "" || latestBLUVessel == null ? "" : latestBLUVessel.Product;
                            email_detail = string.Format(email_detail, items[i].Tug, items[i].Barge, items[i].Estimate_Quantity, items[i].Destination, product,
                                            items[i].Product, items[i].Vessel, items[i].Buyer);

                            var tunnelPortion = tunnelNameSort;
                            tunnelPortion = tunnelPortion.Replace("BTN1", items[i].BTN1.ToString()).Replace("BTN2", items[i].BTN2.ToString()).Replace("BTN3", items[i].BTN3.ToString())
                                        .Replace("BTN4", items[i].BTN4.ToString()).Replace("BTN5", items[i].BTN5.ToString()).Replace("BTN6", items[i].BTN6.ToString())
                                        .Replace("TN1", items[i].TN1.ToString()).Replace("TN2", items[i].TN2.ToString()).Replace("TN3", items[i].TN3.ToString())
                                        .Replace("TN4", items[i].TN4.ToString()).Replace("TN5", items[i].TN5.ToString()).Replace("TN6", items[i].TN6.ToString());

                            string[] tunnelPortionSplit = tunnelPortion.Split('-');
                            tunnelTable = "";
                            for (int j = 0; j < tunnelPortionSplit.Count(); j++)
                            {
                                tunnelPortionSplit[j] = Convert.ToInt32(tunnelPortionSplit[j]) > 0 ? tunnelPortionSplit[j] + "%" : "";
                                tunnelTable += string.Format("<td style=\"border-right: 1px solid black; border-bottom: 1px solid black; text-align:center; \">{0}</td>", tunnelPortionSplit[j]);
                            }

                            email_detail = email_detail.Replace("#--TunnelData--##", tunnelTable);
                            email_detail_add += email_detail;
                        }

                        email_body = email_body.Replace("##--Detail--##", email_detail_add).Replace("##--Url--##", url_detail);
                    }
                }
            }
            catch (Exception ex)
            {
                var result = new { Success = false, ex.Message, MessageDetail = string.Empty, Version = Configuration.VERSION, Items = string.Empty, Total = 0 };
                return Json(result, JsonRequestBehavior.AllowGet);
            }

            return Send(string.Empty, email_subject, email_to, email_cc, email_body, attachment);
        }

        public JsonResult FinalizeCoalInventory()
        {
            Configuration config = new Configuration();

            string email_subject = config.Notification_Finalize_Coal_Inventory;
            string email_to = config.Email_To;
            string email_cc = string.Empty;

            string email_To_Menu = "Coal Inventory";
            string permissionTo = "isemail";

        

            string email_body = System.IO.File.ReadAllText(TemplateFolder + @"\email_template_Finalize_Coal_Inventory.html", Encoding.UTF8);
            string email_detail_fix = System.IO.File.ReadAllText(TemplateFolder + @"\email_template_Finalize_Coal_Inventory_Detail.html", Encoding.UTF8);
            Attachment attachment = null;
            try
            {
                var period = Convert.ToDateTime(OurUtility.ValueOf(Request, "period"));
                var companyCode = OurUtility.ValueOf(Request, "company");
                List<Model_View_UserList_access_Menu> userList_To_Menu = Notification3Controller.UserListApprovalAnalysisResult(email_To_Menu, permissionTo, companyCode);
                using (MERCY_Ctx db = new MERCY_Ctx())
                {
                    var finalCoalInventory = db.CoalInventories.Where(x => x.Period.Month == period.Month
                                            && x.Period.Year == period.Year && x.CompanyCode == companyCode).ToList();

                    if (config.Is_Production)
                    {
                        // -- this is Production

                        // To based on "Calculation" above
                        email_to = Notification3Controller.EmailAddress(userList_To_Menu);
                    }
                    else
                    {
                        // -- Not Production

                        // To based on "Calculation" above
                        email_to = Notification3Controller.EmailAddress_and_Name(userList_To_Menu);

                        // give information in Body of Email
                        email_body = "---------------------------------------------------------------------"
                                    + "<br/>Email To: " + email_to
                                    + "<br/><br/>Email Cc: " + email_cc
                                    + "<br/>---------------------------------------------------------------------"
                                    + "<br/><br/>" + email_body;

                        // "Testing - email Address"
                        email_to = config.Email_To;

                        // reset Cc
                        email_cc = string.Empty;
                    }

                    string url_detail = config.FrontEnd_Url + "/EndOfMonthInventoryv";

                    string email_detail = "";

                    foreach(var item in finalCoalInventory)
                    {
                        TimeSpan timespanStart = item.StartTime;
                        TimeSpan timespanEnd = item.EndTime;

                        email_detail += string.Format(email_detail_fix, item.ROMLocation.Name, item.Tonnage, item.SurveyDate.ToString("dd MMMM yyyy"), 
                                        String.Format("{0:00}:{1:00}", timespanStart.Hours, timespanStart.Minutes), 
                                        String.Format("{0:00}:{1:00}", timespanEnd.Hours, timespanEnd.Minutes));
                    }

                    email_body = email_body.Replace("##--Detail--##", email_detail).Replace("##--Url--##", url_detail).Replace("##--company--##", companyCode).Replace("##--period--##", period.ToString("MMMM yyyy"));
                    email_subject = email_subject.Replace("{date}", period.ToString("MMMM yyyy"));
                }
            }
            catch (Exception ex)
            {
                var result = new { Success = false, ex.Message, MessageDetail = string.Empty, Version = Configuration.VERSION, Items = string.Empty, Total = 0 };
                return Json(result, JsonRequestBehavior.AllowGet);
            }

            return Send(string.Empty, email_subject, email_to, email_cc, email_body, attachment);
        }

    }
}