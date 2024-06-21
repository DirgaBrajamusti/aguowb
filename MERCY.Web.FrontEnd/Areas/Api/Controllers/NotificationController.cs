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
    public class NotificationController : Controller
    {
        public string Index()
        {
            OurUtility.Transparent_Layer_to_BackEnd(Request, Response);

            return string.Empty;
        }

        public string Send()
        {
            string url = OurUtility.Url(Request);
            string pathAndQuery = Request.Url.PathAndQuery;

            if (pathAndQuery.Contains("?")) pathAndQuery += "&";
            else pathAndQuery += "?";
            pathAndQuery += "requestFrom=" + url;

            string result = string.Empty;
            string data_from_BackEnd = string.Empty;

            string server_BackEnd = Configuration.Server_BackEnd;

            OurUtility.Ignore_Certificate_Error();

            try
            {
                string time_begin = DateTime.Now.ToString("dd-MMM-yyyy HH:mm:ss.ffff");
                string ip = OurUtility.GetClientIP_From_EndUser(Request);
                string msg = string.Empty;

                HttpWebRequest request_to_BackEnd = (HttpWebRequest)WebRequest.Create(server_BackEnd + pathAndQuery);
                OurUtility.Copy_Request_Headers(Request, ref request_to_BackEnd);
                OurUtility.Set_Request_Header(request_to_BackEnd, OurUtility.END_USER_IP_ADDRESS, ip);

                HttpWebResponse response_from_BackEnd = null;
                if (OurUtility.GetResponse_from_BackEnd(request_to_BackEnd, ref response_from_BackEnd, Response))
                {
                    OurUtility.Response_to_User(response_from_BackEnd, Response);
                }

                // Print Execution "Timing"
                OurUtility.Set_Response_PrintTiming(Response, time_begin);

                // stop
                Response.End();
            }
            catch (Exception ex)
            {
                data_from_BackEnd = "Error while request to Server: " + ex.Message;
            }

            return data_from_BackEnd;
        }

        public ActionResult Picture()
        {
            OurUtility.Transparent_Layer_to_BackEnd(Request, Response);

            return null;
        }

        public string Get()
        {
            OurUtility.Transparent_Layer_to_BackEnd(Request, Response);

            return string.Empty;
        }

        public string GetAll
()
        {
            OurUtility.Transparent_Layer_to_BackEnd(Request, Response);

            return string.Empty;
        }

        public string ReadRequest()
        {
            OurUtility.Transparent_Layer_to_BackEnd(Request, Response);

            return string.Empty;
        }

        public string Upload_ROM_Sampling_General_Email()
        {
            OurUtility.Transparent_Layer_to_BackEnd(Request, Response);

            return string.Empty;
        }

        public string Upload_ROM_Sampling_Specific_Email()
        {
            OurUtility.Transparent_Layer_to_BackEnd(Request, Response);

            return string.Empty;
        }

        public string Upload_Geology_Pit_Monitoring_General_Email()
        {
            OurUtility.Transparent_Layer_to_BackEnd(Request, Response);

            return string.Empty;
        }

        public string Upload_Geology_Pit_Monitoring_Specific_Email()
        {
            OurUtility.Transparent_Layer_to_BackEnd(Request, Response);

            return string.Empty;
        }

        public string Upload_Geology_Exploration_General_Email()
        {
            OurUtility.Transparent_Layer_to_BackEnd(Request, Response);

            return string.Empty;
        }

        public string Upload_Geology_Exploration_Specific_Email()
        {
            OurUtility.Transparent_Layer_to_BackEnd(Request, Response);

            return string.Empty;
        }

        public string Upload_FC_Crushing()
        {
            OurUtility.Transparent_Layer_to_BackEnd(Request, Response);

            return string.Empty;
        }

        public string Upload_FC_Barging()
        {
            OurUtility.Transparent_Layer_to_BackEnd(Request, Response);

            return string.Empty;
        }

        public string SamplingRequest()
        {
            OurUtility.Transparent_Layer_to_BackEnd(Request, Response);

            return string.Empty;
        }

        public string AnalysisRequest()
        {
            OurUtility.Transparent_Layer_to_BackEnd(Request, Response);

            return string.Empty;
        }

        public string Discussion()
        {
            OurUtility.Transparent_Layer_to_BackEnd(Request, Response);

            return string.Empty;
        }

        public string AnalysisRequest_Verification()
        {
            OurUtility.Transparent_Layer_to_BackEnd(Request, Response);

            return string.Empty;
        }

        public string AnalysisRequest_ReTest()
        {
            OurUtility.Transparent_Layer_to_BackEnd(Request, Response);

            return string.Empty;
        }

        public string Upload_HAC_General_Email()
        {
            OurUtility.Transparent_Layer_to_BackEnd(Request, Response);

            return string.Empty;
        }

        public string Upload_HAC_Specific_Email()
        {
            OurUtility.Transparent_Layer_to_BackEnd(Request, Response);

            return string.Empty;
        }

        public string ActualLoading_Finalize()
        {
            OurUtility.Transparent_Layer_to_BackEnd(Request, Response);

            return string.Empty;
        }

        public string ActualTunnel_Approve()
        {
            OurUtility.Transparent_Layer_to_BackEnd(Request, Response);

            return string.Empty;
        }

        public string ActualTunnel_Draft()
        {
            OurUtility.Transparent_Layer_to_BackEnd(Request, Response);

            return string.Empty;
        }

        public string Analysis_Result_Validate()
        {
            OurUtility.Transparent_Layer_to_BackEnd(Request, Response);

            return string.Empty;
        }

        public string Analysis_Result_Approve()
        {
            OurUtility.Transparent_Layer_to_BackEnd(Request, Response);

            return string.Empty;
        }

        public string RequestSubmission()
        {
            OurUtility.Transparent_Layer_to_BackEnd(Request, Response);
            
            return string.Empty;
        }

        public string FinalizeCoalInventory()
        {
            OurUtility.Transparent_Layer_to_BackEnd(Request, Response);

            return string.Empty;
        }
    }
}