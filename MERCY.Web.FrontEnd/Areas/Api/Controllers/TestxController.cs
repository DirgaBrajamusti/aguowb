using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

using System.Net;
using System.IO;

using MERCY.Web.FrontEnd.Helpers;

namespace MERCY.Web.FrontEnd.Areas.Api.Controllers
{
    public class TestxController : Controller
    {
        public string Index()
        {
            string result = string.Empty;
            string data_from_BackEnd = string.Empty;
            string server_BackEnd = Configuration.Server_BackEnd;

            string dateTime_FrontEnd = DateTime.Now.ToString("yyyy - MM - dd HH: mm: ss.fff");

            try
            {
                OurUtility.Ignore_Certificate_Error();

                HttpWebRequest request_to_BackEnd = (HttpWebRequest)WebRequest.Create(server_BackEnd + @"/api/test");
                HttpWebResponse response_from_BackEnd = (HttpWebResponse)request_to_BackEnd.GetResponse();

                using (StreamReader responseReader = new StreamReader(response_from_BackEnd.GetResponseStream()))
                {
                    data_from_BackEnd = responseReader.ReadToEnd();
                }   
            }
            catch (Exception ex)
            {
                data_from_BackEnd = "Error while request to Server: " + ex.Message;
            }

            return "DateTime on<strong> FrontEnd</strong> is: " + dateTime_FrontEnd + "<br/>Access to <strong>BackEnd</strong> [" + server_BackEnd + "]<br/>&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;{" + data_from_BackEnd  + "}.";
        }
    }
}