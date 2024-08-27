using System.Web.Mvc;

namespace MERCY.Web.BackEnd.Areas.Api.Controllers
{
    public class TestxController : Controller
    {
        public string Index()
        {
            return (new TestController()).Index();
        }
    }
}