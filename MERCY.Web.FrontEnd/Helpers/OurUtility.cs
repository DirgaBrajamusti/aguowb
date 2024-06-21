using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using System.Net;
using System.IO;
using System.Text;

using System.Security.Cryptography.X509Certificates;

namespace MERCY.Web.FrontEnd.Helpers
{
    public class OurUtility
    {
        internal const string INFO_SPRINT                   = "Sprint_8";

        public const string HEADER_RESPONSE                 = "MERCY_1-FrontEnd";
        public const string SERVER_MESSAGE                  = "Message from FrontEnd";
        public const string END_USER_IP_ADDRESS             = "MERCY_END_USER_IP_ADDRESS";
        
        public const string UPLOAD_Sampling_ROM             = "Sampling_ROM";
        public const string UPLOAD_Geology_Pit_Monitoring   = "Geology_Pit_Monitoring";
        public const string UPLOAD_Geology_Explorasi        = "Geology_Explorasi";
        public const string UPLOAD_BARGE_LOADING            = "BARGE_LOADING";
        public const string UPLOAD_CRUSHING_PLANT           = "CRUSHING_PLANT";
        public const string UPLOAD_BargeQualityPlan         = "BargeQualityPlan";
        public const string UPLOAD_BargeLineUp              = "LineUp";
        public const string UPLOAD_SampleDetail             = "SampleDetail";
        public const string UPLOAD_HAC                      = "HAC";

        public const string Shipment_Type_Vessel            = "Vessel";
        public const string Shipment_Type_Direct            = "Direct";
        public const string Shipment_Type_Trans             = "Trans";

        public static void Copy_Request_Headers(HttpRequestBase p_request_fromUser, ref HttpWebRequest p_request_to_BackEnd)
        {
            foreach (string header in p_request_fromUser.Headers.AllKeys)
            {
                try
                {
                    p_request_to_BackEnd.Headers.Add(header, p_request_fromUser.Headers[header]);
                }
                catch
                {
                    try
                    {
                        p_request_to_BackEnd.Headers[header] = p_request_fromUser.Headers[header];
                    }
                    catch {}
                }
            }

            try
            {
                p_request_to_BackEnd.Method = p_request_fromUser.HttpMethod;
            }
            catch {}

            try
            {
                p_request_to_BackEnd.ContentType = p_request_fromUser.ContentType;
            }
            catch {}

            try
            {
                p_request_to_BackEnd.UserAgent = p_request_fromUser.UserAgent;
            }
            catch { }
        }

        public static bool Copy_Request_Body_Text(HttpRequestBase p_request_fromUser, ref HttpWebRequest p_request_to_BackEnd, ref string p_message)
        {
            bool result = false;
            string postData = GetDocumentContents(p_request_fromUser);

            p_message = string.Empty;

            if (string.IsNullOrEmpty(postData)) return true;

            try
            {
                ASCIIEncoding encoding = new ASCIIEncoding();
                byte[] data = encoding.GetBytes(postData);

                // Set the content type of the data being posted.
                //p_request_to_BackEnd.ContentType = "application/x-www-form-urlencoded";

                // Set the content length of the string being posted.
                p_request_to_BackEnd.ContentLength = data.Length;

                using (Stream newStream = p_request_to_BackEnd.GetRequestStream())
                {
                    newStream.Write(data, 0, data.Length);
                }

                result = true;
            }
            catch (Exception ex)
            {
                p_message = ex.Message;
            }

            return result;
        }

        public static bool Copy_Request_Body(string p_postData, ref HttpWebRequest p_request_to_BackEnd, ref string p_message)
        {
            bool result = false;

            p_message = string.Empty;

            if (string.IsNullOrEmpty(p_postData)) return true;

            try
            {
                ASCIIEncoding encoding = new ASCIIEncoding();
                byte[] data = encoding.GetBytes(p_postData);

                // Set the content type of the data being posted.
                //p_request_to_BackEnd.ContentType = "application/x-www-form-urlencoded";

                // Set the content length of the string being posted.
                p_request_to_BackEnd.ContentLength = data.Length;

                using (Stream newStream = p_request_to_BackEnd.GetRequestStream())
                {
                    newStream.Write(data, 0, data.Length);
                }

                result = true;
            }
            catch (Exception ex)
            {
                p_message = ex.Message;
            }

            return result;
        }

        public static bool Copy_Request_Body(HttpRequestBase p_request_fromUser, ref HttpWebRequest p_request_to_BackEnd, ref string p_message)
        {
            bool result = false;

            p_message = string.Empty;

            try
            {
                using (Stream receiveStream = p_request_fromUser.InputStream)
                {
                    using (Stream newStream = p_request_to_BackEnd.GetRequestStream())
                    {
                        receiveStream.CopyTo(newStream);
                        p_request_to_BackEnd.ContentLength = receiveStream.Length;
                    }
                }

                result = true;
            }
            catch (Exception ex)
            {
                p_message = ex.Message;
            }

            return result;
        }

        public static string GetDocumentContents(HttpRequestBase Request)
        {
            string result = string.Empty;

            try
            {
                using (Stream receiveStream = Request.InputStream)
                {
                    using (StreamReader readStream = new StreamReader(receiveStream, Encoding.UTF8))
                    {
                        result = readStream.ReadToEnd();
                    }
                }

            }
            catch {}

            return result;
        }

        public static void Copy_Response_Headers(HttpWebResponse p_response_from_BackEnd, HttpResponseBase p_response_to_User)
        {
            foreach (string header in p_response_from_BackEnd.Headers.AllKeys)
            {
                try
                {
                    p_response_to_User.Headers.Add(header, p_response_from_BackEnd.Headers[header]);
                }
                catch
                {
                    try
                    {
                        p_response_to_User.Headers[header] = p_response_from_BackEnd.Headers[header];
                    }
                    catch {}
                }
            }

            try
            {
                p_response_to_User.ContentType = p_response_from_BackEnd.ContentType;
            }
            catch {}
        }

        public static bool GetResponse_from_BackEnd(HttpWebRequest p_request_to_BackEnd, ref HttpWebResponse p_response_from_BackEnd, HttpResponseBase p_response_to_User)
        {
            bool result = false;

            p_response_from_BackEnd = null;

            try
            {
                p_response_from_BackEnd = (HttpWebResponse)p_request_to_BackEnd.GetResponse();

                result = true;
            }
            catch (Exception ex)
            {
                //remote url not found, send 404 to client 
                p_response_to_User.StatusCode = 404;
                p_response_to_User.StatusDescription = "Not Found";
                p_response_to_User.Write("Error [GetResponse_from_BackEnd]: " + ex.Message);
            }

            return result;
        }

        public static void Set_Response_Header(HttpResponseBase p_response_to_User, string p_key, string p_value)
        {
            try
            {
                p_response_to_User.Headers.Add(p_key, p_value);
            }
            catch
            {
                try
                {
                    p_response_to_User.Headers[p_key] = p_value;
                }
                catch {}
            }
        }

        public static void Set_Request_Header(HttpWebRequest request_to_BackEnd, string p_key, string p_value)
        {
            try
            {
                request_to_BackEnd.Headers.Add(p_key, p_value);
            }
            catch
            {
                try
                {
                    request_to_BackEnd.Headers[p_key] = p_value;
                }
                catch {}
            }
        }

        public static void Set_Response_ServerMessage(HttpResponseBase p_response_to_User, string p_additional_Key, string p_additional_Value)
        {
            Set_Response_Header(p_response_to_User, HEADER_RESPONSE + p_additional_Key, SERVER_MESSAGE + p_additional_Value);
        }

        public static void Set_Response_ServerMessage(HttpResponseBase p_response_to_User, string p_additional_Key)
        {
            string additional_Value = " on " + DateTime.Now.ToString("dd-MMM-yyyy HH:mm:ss.ffff");
            Set_Response_ServerMessage(p_response_to_User, p_additional_Key, additional_Value);
        }

        public static void Set_Response_ServerMessage(HttpResponseBase p_response_to_User)
        {
            string additional_Key = string.Empty;
            Set_Response_ServerMessage(p_response_to_User, additional_Key);
        }

        public static void Set_Response_PrintTiming(HttpResponseBase p_response_to_User, string p_time_begin)
        {
            // Print "Timing"
            OurUtility.Set_Response_ServerMessage(p_response_to_User, "_Begin", " on " + p_time_begin);
            string time_end = DateTime.Now.ToString("dd-MMM-yyyy HH:mm:ss.ffff");
            OurUtility.Set_Response_ServerMessage(p_response_to_User, "_End", " on " + time_end);
        }

        public static bool Response_to_User(HttpWebResponse p_response_from_BackEnd, HttpResponseBase p_response_to_User)
        {
            bool result = false;

            try
            {
                Stream receiveStream = p_response_from_BackEnd.GetResponseStream();

                byte[] buff = new byte[1024];
                int bytes = 0;
                while ((bytes = receiveStream.Read(buff, 0, 1024)) > 0)
                {
                    //Write the stream directly to the client 
                    p_response_to_User.OutputStream.Write(buff, 0, bytes);
                }
                //close streams
                //response.Close();
                //Response.End();

                Copy_Response_Headers(p_response_from_BackEnd, p_response_to_User);

                result = true;
            }
            catch (Exception ex)
            {
                p_response_to_User.StatusCode = 404;
                p_response_to_User.StatusDescription = "Not Found";
                p_response_to_User.Write("Error [Response_to_User]: " + ex.Message);
            }

            return result;
        }

        public static void Transparent_Layer_to_BackEnd(HttpRequestBase p_request_fromUser, HttpResponseBase p_response_to_User)
        {
            string server_BackEnd = Configuration.Server_BackEnd;

            string url = p_request_fromUser.Url.AbsolutePath.ToLower();
            string url2 = p_request_fromUser.Url.PathAndQuery;
            string url_to_BackEnd = server_BackEnd + url2;

            Transparent_Layer_to_BackEnd(url_to_BackEnd, p_request_fromUser, p_response_to_User);
        }

        public static void Transparent_Layer_to_BackEnd(string p_url_to_BackEnd, HttpRequestBase p_request_fromUser, HttpResponseBase p_response_to_User)
        {
            Ignore_Certificate_Error();

            try
            {
                string time_begin = DateTime.Now.ToString("dd-MMM-yyyy HH:mm:ss.ffff");
                string ip = GetClientIP_From_EndUser(p_request_fromUser);
                string msg = string.Empty;

                HttpWebRequest request_to_BackEnd = (HttpWebRequest)WebRequest.Create(p_url_to_BackEnd);
                OurUtility.Copy_Request_Headers(p_request_fromUser, ref request_to_BackEnd);
                OurUtility.Set_Request_Header(request_to_BackEnd, END_USER_IP_ADDRESS, ip);

                OurUtility.Copy_Request_Body(p_request_fromUser, ref request_to_BackEnd, ref msg);

                HttpWebResponse response_from_BackEnd = null;
                if (OurUtility.GetResponse_from_BackEnd(request_to_BackEnd, ref response_from_BackEnd, p_response_to_User))
                {
                    OurUtility.Response_to_User(response_from_BackEnd, p_response_to_User);
                }

                // Print Execution "Timing"
                OurUtility.Set_Response_PrintTiming(p_response_to_User, time_begin);

                // stop
                p_response_to_User.End();
            }
            catch {}
        }

        public static string GetClientIP_From_EndUser(HttpRequestBase p_request_fromUser)
        {
            string result = string.Empty;
            string _ipList = string.Empty;

            try
            {
                if (p_request_fromUser.Headers.AllKeys.Contains("CF-CONNECTING-IP"))
                {
                    _ipList = p_request_fromUser.Headers["CF-CONNECTING-IP"].ToString();
                }

                if (!string.IsNullOrWhiteSpace(_ipList))
                {
                    result = _ipList.Split(',')[0];
                }
                else
                {
                    _ipList = p_request_fromUser.ServerVariables["HTTP_X_CLUSTER_CLIENT_IP"];
                    if (!string.IsNullOrWhiteSpace(_ipList))
                    {
                        result = _ipList.Split(',')[0];
                    }
                    else
                    {
                        _ipList = p_request_fromUser.ServerVariables["HTTP_X_FORWARDED_FOR"];
                        if (!string.IsNullOrWhiteSpace(_ipList))
                        {
                            result = _ipList.Split(',')[0];
                        }
                        else
                        {
                            result = p_request_fromUser.ServerVariables["REMOTE_ADDR"].ToString();
                        }
                    }
                }
            }
            catch {}

            return result.Trim();
        }

        public static void Ignore_Certificate_Error()
        {
            try
            {
                ServicePointManager.ServerCertificateValidationCallback += new System.Net.Security.RemoteCertificateValidationCallback(CertCheck);
            }
            catch {}
        }

        private static bool CertCheck(object sender, X509Certificate cert,
                X509Chain chain, System.Net.Security.SslPolicyErrors error)
        {
            return true;
        }

        public static string Url(HttpRequestBase p_request)
        {
            //string url = p_request.Url.AbsolutePath.ToLower();

            String strPathAndQuery = p_request.Url.PathAndQuery;
            //String strUrl = p_request.Url.AbsoluteUri.Replace(strPathAndQuery, @"/");
            String strUrl = p_request.Url.AbsoluteUri.Replace(strPathAndQuery, "");

            return strUrl;
        }
        
        public static string Cookie(HttpRequest p_request, string p_name)
        {
            string result = string.Empty;

            try
            {
                result = p_request.Cookies[p_name].Value;
            }
            catch {}

            return result;
        }

        public static string ValueOf(HttpRequestBase p_request, string p_key)
        {
            string result = string.Empty;

            try
            {
                if (p_request.Params.AllKeys.Contains(p_key))
                {
                    result = p_request[p_key].Trim();
                }
            }
            catch {}

            return result;
        }
    }
}