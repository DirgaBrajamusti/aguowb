﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Configuration;

namespace Mercy.Cmd.Scheduler
{
    class Configuration
    {
        public const string VERSION = "20201013-1203";

        public static string AppSettings(string p_key)
        {
            string result = string.Empty;

            try
            {
                result = ConfigurationManager.AppSettings[p_key];
            }
            catch { }

            return result;
        }

        public static string Server_BackEnd
        {
            get { return AppSettings("MERCY_Server_BackEnd"); }
        }

        public static string MERCY_User
        {
            get { return AppSettings("MERCY_User"); }
        }

        public static string MERCY_Password
        {
            get { return AppSettings("MERCY_Password"); }
        }
    }
}
