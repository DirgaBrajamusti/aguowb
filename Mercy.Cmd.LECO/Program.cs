using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Net;
using System.IO;

namespace Mercy.Cmd.LECO
{
    class Program
    {
        static string api_login = @"/API/User/Login";
        static string api_CV = @"/API/Upload_LECO/CV";
        static string api_TS = @"/API/Upload_LECO/TS";
        static string api_TGA = @"/API/Upload_LECO/TGA";

        static void Main(string[] args)
        {
            OurUtility.Write_Log("LECO", "Activity", "** Begin [version: " + Configuration.VERSION + "]");

            string ts_web = Configuration.Server_BackEnd;
            string ts_user = Configuration.MERCY_User;
            string ts_password = Configuration.MERCY_Password;
            string login = "u=" + ts_user + "&p=" + ts_password;

            string msg = string.Empty;

            HttpWebResponse response = null;
            string result = OurUtility.Login(ts_web + api_login + "?" + login, ref response, ref msg);
            if ( ! string.IsNullOrEmpty(msg))
            {
                OurUtility.Write_Log("LECO", "Activity", "Error Login : " + msg);
            }
            else if (!result.ToLower().Contains("\"success\":true"))
            {
                OurUtility.Write_Log("LECO", "Activity", "Error Login : " + result + " aaa");
            }
            else
            {
                string token = OurUtility.Get_Token(response);

                string file_content = string.Empty;
                string data_return = string.Empty;

                string[] file_CVs = Directory.GetFiles(Configuration.Folder_CV);
                foreach (string fileName in file_CVs)
                {
                    OurUtility.Write_Log("LECO", "Activity", "Processing CV, for file [" + fileName  + "] Server: [" + ts_web + api_CV + "]");

                    file_content = OurUtility.File_Read(Configuration.Folder_CV, fileName);

                    data_return = OurUtility.Post(ts_web + api_CV, token, "data=" + file_content, ref msg);
                    OurUtility.Write_Log("LECO", "Activity", "Data return: " + data_return);

                    OurUtility.File_Delete(Configuration.Folder_CV, fileName);
                }

                string[] file_TSs = Directory.GetFiles(Configuration.Folder_TS);
                foreach (string fileName in file_TSs)
                {
                    OurUtility.Write_Log("LECO", "Activity", "Processing TS, for file [" + fileName + "] Server: [" + ts_web + api_TS + "]");

                    file_content = OurUtility.File_Read(Configuration.Folder_TS, fileName);

                    data_return = OurUtility.Post(ts_web + api_TS, token, "data=" + file_content, ref msg);
                    OurUtility.Write_Log("LECO", "Activity", "Data return: " + data_return);

                    OurUtility.File_Delete(Configuration.Folder_TS, fileName);
                }

                string[] file_TGAs = Directory.GetFiles(Configuration.Folder_TGA);
                foreach (string fileName in file_TGAs)
                {
                    OurUtility.Write_Log("LECO", "Activity", "Processing TGA, for file [" + fileName + "] Server: [" + ts_web + api_TGA + "]");

                    file_content = OurUtility.File_Read(Configuration.Folder_TGA, fileName);

                    data_return = OurUtility.Post(ts_web + api_TGA, token, "data=" + file_content, ref msg);
                    OurUtility.Write_Log("LECO", "Activity", "Data return: " + data_return);

                    OurUtility.File_Delete(Configuration.Folder_TGA, fileName);
                }
            }

            OurUtility.Write_Log("LECO", "Activity", "** End");
        }
    }
}
