using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Net;
using System.IO;

namespace Mercy.Cmd.Scheduler
{
    class OurUtility
    {
        public const string MERCY_TOKEN = "MERCY-Token";

        public static string Get(string p_url, string p_token, ref string p_message)
        {
            string result = string.Empty;

            p_message = string.Empty;

            try
            {
                var request = (HttpWebRequest)WebRequest.Create(p_url);
                Set_Request_Header(request, MERCY_TOKEN, p_token);

                var response = (HttpWebResponse)request.GetResponse();

                result = new StreamReader(response.GetResponseStream()).ReadToEnd();

                switch (response.StatusCode)
                {
                    case HttpStatusCode.OK:
                        break;
                }
            }
            catch (Exception ex)
            {
                p_message = ex.Message;
            }

            return result;
        }

        public static string Post(string p_url, string p_token, string p_data, ref string p_message)
        {
            string result = string.Empty;

            p_message = string.Empty;

            try
            {
                var request = (HttpWebRequest)WebRequest.Create(p_url);
                Set_Request_Header(request, MERCY_TOKEN, p_token);

                var data = Encoding.ASCII.GetBytes(p_data);

                request.Method = "POST";
                request.ContentType = "application/x-www-form-urlencoded";
                request.ContentLength = data.Length;

                using (var stream = request.GetRequestStream())
                {
                    stream.Write(data, 0, data.Length);
                }

                var response = (HttpWebResponse)request.GetResponse();

                result = new StreamReader(response.GetResponseStream()).ReadToEnd();
            }
            catch (Exception ex)
            {
                p_message = ex.Message;
            }

            return result;
        }

        public static string Login(string p_url, ref HttpWebResponse p_response, ref string p_message)
        {
            string result = string.Empty;

            p_message = string.Empty;

            try
            {
                var request = (HttpWebRequest)WebRequest.Create(p_url);
                request.UserAgent = "Mercy: scheduler";

                p_response = (HttpWebResponse)request.GetResponse();

                result = new StreamReader(p_response.GetResponseStream()).ReadToEnd();

                switch (p_response.StatusCode)
                {
                    case HttpStatusCode.OK:
                        break;
                }
            }
            catch (Exception ex)
            {
                p_message = ex.Message;
                p_response = null;
            }

            return result;
        }

        public static void Write_Log(bool p_Timestamp_in_Data, bool p_isNewLine, string p_folder, string p_fileName, string p_text, ref string p_message)
        {
            p_message = string.Empty;

            try
            {
                DateTime now = DateTime.Now;

                string dir = Directory_Logs("") + @"\" + now.ToString("yyyyMM") + "-" + p_folder;
                CreateDirectory(dir);

                string fileName = dir + @"\" + p_fileName + "-" + now.ToString("yyyyMMdd") + ".txt";

                string data = now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " - " + p_text;
                if (!p_Timestamp_in_Data) data = p_text;

                if (p_isNewLine)
                {
                    Write_to_File(data, fileName, ref p_message);
                }
                else
                {
                    Write_to_File2(data, fileName, ref p_message);
                }
            }
            catch (Exception ex)
            {
                p_message = ex.Message;
            }
        }

        public static void Write_Log(bool p_Timestamp_in_Data, string p_folder, string p_fileName, string p_text, ref string p_message)
        {
            Write_Log(p_Timestamp_in_Data, true, p_folder, p_fileName, p_text, ref p_message);
        }


        public static void Write_Log(string p_folder, string p_fileName, string p_text, ref string p_message)
        {
            Write_Log(true, p_folder, p_fileName, p_text, ref p_message);
        }

        public static void Write_Log(string p_folder, string p_fileName, string p_text)
        {
            string msg = string.Empty;

            Write_Log(p_folder, p_fileName, p_text, ref msg);
        }

        public static string Directory_Logs(string p_directory)
        {
            if (string.IsNullOrEmpty(p_directory)) p_directory = Directory.GetCurrentDirectory();

            string dir = p_directory + @"\Logs";

            CreateDirectory(dir);

            return dir;
        }

        public static string CreateDirectory(string p_directory)
        {
            try
            {
                Directory.CreateDirectory(p_directory);
            }
            catch { }

            return string.Empty;
        }

        public static bool Write_to_File(string p_data, string p_fileName, ref string p_message)
        {
            bool result = false;

            p_message = string.Empty;

            try
            {
                using (StreamWriter writer = new StreamWriter(p_fileName, true))
                {
                    writer.WriteLine(p_data);
                }

                result = true;
            }
            catch (Exception ex)
            {
                p_message = ex.Message;
            }

            return result;
        }

        public static bool Write_to_File2(string p_data, string p_fileName, ref string p_message)
        {
            bool result = false;

            p_message = string.Empty;

            try
            {
                using (StreamWriter writer = new StreamWriter(p_fileName, true))
                {
                    writer.Write(p_data);
                }

                result = true;
            }
            catch (Exception ex)
            {
                p_message = ex.Message;
            }

            return result;
        }

        public static void Set_Request_Header(HttpWebRequest p_request, string p_key, string p_value)
        {
            try
            {
                p_request.Headers.Add(p_key, p_value);
            }
            catch
            {
                try
                {
                    p_request.Headers[p_key] = p_value;
                }
                catch {}
            }
        }

        public static string Get_Token(HttpWebResponse p_response)
        {
            string result = string.Empty;

            try
            {
                result = p_response.Headers[MERCY_TOKEN];
            }
            catch {}

            return result;
        }
    }
}
