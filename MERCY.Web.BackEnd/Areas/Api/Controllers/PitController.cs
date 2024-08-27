using System.Web.Mvc;

namespace MERCY.Web.BackEnd.Areas.Api.Controllers
{
    public class PitController : Controller
    {
        public JsonResult Index()
        {
            var controllerB = new ExternalDataController();
            controllerB.InitializeController(this.Request.RequestContext);

            return controllerB.GetPits();
        }

        public JsonResult Get_ddl_All()
        {
            var controllerB = new ExternalDataController();
            controllerB.InitializeController(this.Request.RequestContext);

            return controllerB.GetPits();
        }

        public JsonResult Get_ddl()
        {
            var controllerB = new ExternalDataController();
            controllerB.InitializeController(this.Request.RequestContext);

            return controllerB.GetPitsByCompany();
        }
    }
}