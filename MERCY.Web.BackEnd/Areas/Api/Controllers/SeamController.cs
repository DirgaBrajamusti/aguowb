using System;
using System.Linq;
using System.Web.Mvc;

using System.Data;
using System.Data.SqlClient;

using MERCY.Data.EntityFramework;
using MERCY.Web.BackEnd.Models;
using MERCY.Web.BackEnd.Helpers;
using MERCY.Web.BackEnd.Security;
using Permission = MERCY.Web.BackEnd.Security.Permission;

namespace MERCY.Web.BackEnd.Areas.Api.Controllers
{
    public class SeamController : Controller
    {
        public JsonResult Index()
        {
            var controllerB = new ExternalDataController();
            controllerB.InitializeController(this.Request.RequestContext);

            return controllerB.GetSeams();
        }

        public JsonResult Get_ddl_All()
        {
            var controllerB = new ExternalDataController();
            controllerB.InitializeController(this.Request.RequestContext);

            return controllerB.GetSeams();
        }

        public JsonResult Get_ddl()
        {
            var controllerB = new ExternalDataController();
            controllerB.InitializeController(this.Request.RequestContext);

            return controllerB.GetSeamsByCompanyAndPit();
        }

        internal static bool Add(MERCY_Ctx p_db, long p_samplingRequest, string p_seams, ref string p_message)
        {
            bool result = false;

            p_message = string.Empty;

            try
            {
                p_seams = System.Uri.UnescapeDataString(p_seams);
                p_seams = System.Uri.UnescapeDataString(p_seams);
                p_seams = System.Web.HttpUtility.HtmlDecode(p_seams);

                string[] words = p_seams.Split('&');
                string seam_Part = words[2];

                string[] seam_Parts = seam_Part.Split('=');
                string data_seam = seam_Parts[1];

                string[] seams = data_seam.Split(',');
                foreach (string seam in seams)
                {
                    try
                    {
                        var data = new SamplingRequest_SEAM
                        {
                            SamplingRequest = p_samplingRequest,
                            SEAM = seam
                        };

                        p_db.SamplingRequest_SEAM.Add(data);
                        p_db.SaveChanges();
                    }
                    catch {}
                }

                result = true;
            }
            catch {}

            return result;
        }

        internal static bool Add(SqlConnection p_db, long p_samplingRequest, string p_seams, ref string p_message)
        {
            bool result = false;

            p_message = string.Empty;

            try
            {
                p_seams = System.Uri.UnescapeDataString(p_seams);
                p_seams = System.Uri.UnescapeDataString(p_seams);
                p_seams = System.Web.HttpUtility.HtmlDecode(p_seams);

                string[] words = p_seams.Split('&');
                string seam_Part = words[2];

                string[] seam_Parts = seam_Part.Split('=');
                string data_seam = seam_Parts[1];

                string[] seams = data_seam.Split(',');

                SqlCommand command = p_db.CreateCommand();
                string sql = string.Empty;

                string seam = string.Empty;
                foreach (string seamId in seams)
                {
                    try
                    {
                        seam = string.Empty;
                        try
                        {
                            string[] seamIds = seamId.Split(new string[] { "__" }, StringSplitOptions.None);
                            seam = seamIds[2];
                        }
                        catch {}

                        sql = string.Format(@"insert into SamplingRequest_SEAM(SamplingRequest, SEAM, COMPANY_PIT_SEAM) values({0}, '{1}', '{2}')", p_samplingRequest, seam, seamId);
                        command.CommandText = sql;
                        command.ExecuteNonQuery();
                    }
                    catch {}
                }

                result = true;
            }
            catch {}

            return result;
        }

        internal static bool Delete(SqlConnection p_db, long p_samplingRequest, ref string p_message)
        {
            bool result = false;

            p_message = string.Empty;

            try
            {
                SqlCommand command = p_db.CreateCommand();
                string sql = string.Format(@"delete from SamplingRequest_SEAM where SamplingRequest = {0}", p_samplingRequest);
                command.CommandText = sql;
                command.ExecuteNonQuery();

                result = true;
            }
            catch {}

            return result;
        }

        public JsonResult Get_by_Sampling()
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

            // -- Actual code
            long sampling = OurUtility.ToInt64(Request["sampling"]);
            try
            {
                using (MERCY_Ctx db = new MERCY_Ctx())
                {
                    var dataQuery =
                            (
                                from d in db.SamplingRequest_SEAM
                                where d.SamplingRequest == sampling
                                orderby d.COMPANY_PIT_SEAM
                                select new
                                {
                                    d.SEAM
                                    , d.COMPANY_PIT_SEAM
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