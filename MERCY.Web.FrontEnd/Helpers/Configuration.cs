using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using System.Configuration;

namespace MERCY.Web.FrontEnd.Helpers
{
    public class Configuration
    {
        internal const string INFO_VERSION = "20231013-1322";

        public static string VERSION
        {
            get { return INFO_VERSION; }
        }

        public static string AppSettings(string p_key)
        {
            string result = string.Empty;

            try
            {
                result = ConfigurationManager.AppSettings[p_key];
            }
            catch {}

            return result;
        }

        public static string Server_BackEnd
        {
            get { return AppSettings("MERCY_Server_BackEnd"); }
        }

        public static string Server_Reporting
        {
            get { return AppSettings("MERCY_Report"); }
        }

        public static string Default_UserInterface
        {
            get { return AppSettings("MERCY_Default_UserInterface"); }
        }

        public static string MERCY_Mode
        {
            get { return AppSettings("MERCY_Mode"); }
        }

        public static string Google_Analytic
        {
            get { return AppSettings("MERCY_Google_Analytic"); }
        }
    }
}