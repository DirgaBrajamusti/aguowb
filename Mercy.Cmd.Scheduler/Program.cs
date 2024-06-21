using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Net;

namespace Mercy.Cmd.Scheduler
{
    class Program
    {
        static string api_login = @"/API/User/Login";
        static string api_LabMaintenance = @"/API/LabMaintenance/Scheduler?email=yes&now=&history=yes";
        static string api_Consumable = @"/API/Consumable/Scheduler?email=yes&now=&history=yes";

        static void Main(string[] args)
        {
            OurUtility.Write_Log("Scheduler", "Activity", "** Begin [version: " + Configuration.VERSION + "]");

            string ts_web = Configuration.Server_BackEnd;
            string ts_user = Configuration.MERCY_User;
            string ts_password = Configuration.MERCY_Password;
            string login = "u=" + ts_user + "&p=" + ts_password;

            string msg = string.Empty;

            HttpWebResponse response = null;
            string result = OurUtility.Login(ts_web + api_login + "?" + login, ref response, ref msg);
            if ( ! string.IsNullOrEmpty(msg))
            {
                OurUtility.Write_Log("Scheduler", "Activity", "Error Login : " + msg);
            }
            else if ( ! result.ToLower().Contains("\"success\":true"))
            {
                OurUtility.Write_Log("Scheduler", "Activity", "Error Login : " + result + " aaa");
            }
            else
            {
                string token = OurUtility.Get_Token(response);

                OurUtility.Write_Log("Scheduler", "Activity", "Scheduler, for Lab Maintenance [" + ts_web + api_LabMaintenance + "]");
                string data = OurUtility.Get(ts_web + api_LabMaintenance, token, ref msg);
                OurUtility.Write_Log("Scheduler", "Activity", "Data return: " + data);

                OurUtility.Write_Log("Scheduler", "Activity", "Scheduler, for Lab Consumable [" + ts_web + api_Consumable + "]");
                data = OurUtility.Get(ts_web + api_Consumable, token, ref msg);
                OurUtility.Write_Log("Scheduler", "Activity", "Data return: " + data);
            }

            OurUtility.Write_Log("Scheduler", "Activity", "** End");
        }
    }
}
