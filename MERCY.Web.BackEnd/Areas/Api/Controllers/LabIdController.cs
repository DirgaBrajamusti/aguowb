using System.Linq;
using System.Web.Mvc;

using MERCY.Data.EntityFramework;
using MERCY.Web.BackEnd.Models;
using MERCY.Web.BackEnd.Helpers;
using MERCY.Web.BackEnd.Security;
using Permission = MERCY.Web.BackEnd.Security.Permission;

namespace MERCY.Web.BackEnd.Areas.Api.Controllers
{
    public class LabIdController : Controller
    {
        public ActionResult Index()
        {
            return View();
        }

        public JsonResult GetBySamplingRequest()
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

            long sampleRequestId = -1;

            // -- Actual code
            using (MERCY_Ctx db = new MERCY_Ctx())
            {
                var dataQuery =
                        (
                            from dt in db.SamplingRequest_Lab
                            where dt.SamplingRequest == sampleRequestId
                            select new
                            {
                                dt.RecordId
                                , dt.LabId
                                , LabTime = ""
                            }
                        );

                var items = dataQuery.ToList();

                var result = new { Success = true, Permission = permission_Item, Message = string.Empty, MessageDetail = string.Empty, Version = Configuration.VERSION, Items = items, Total = items.Count };
                return Json(result, JsonRequestBehavior.AllowGet);
            }
        }
    }
}