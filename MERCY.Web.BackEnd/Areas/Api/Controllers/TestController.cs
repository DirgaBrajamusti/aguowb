using System;
using System.Web.Mvc;

namespace MERCY.Web.BackEnd.Areas.Api.Controllers
{
    public class TestController : Controller
    {
        public string Index()
        {
            return "Respond from API in <strong>BackEnd</strong>. DateTime on Server is: " + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");
        }
    }
}