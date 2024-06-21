using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MERCY.Web.FrontEnd.Helpers
{
    public class UserInterface
    {
        // by default, Let's use value from Web.config
        string ui_name = Configuration.Default_UserInterface;

        public UserInterface()
        {
            // Try to get value from Cookie
            HttpRequest request = HttpContext.Current.Request;
            string ui_for_CurrentUser = OurUtility.Cookie(request, "mercy_ui");

            if (string.IsNullOrEmpty(ui_for_CurrentUser)) return;

            ui_for_CurrentUser = ui_for_CurrentUser.Replace(@"/", "").Replace(@"\", "");
            ui_name = ui_for_CurrentUser;

            if ( ! System.IO.Directory.Exists(Folder_ServerSide))
            {
                // reset
                // Let's use value from Web.config
                ui_name = Configuration.Default_UserInterface;
            }
        }

        public UserInterface(bool p_noUser)
        {
            ui_name = Configuration.Default_UserInterface;
        }

        public UserInterface(string p_ui_name)
        {
            ui_name = p_ui_name;
        }

        public string Folder_ServerSide
        {
            get
            {
                return @"~/Views/UserInterface/" + ui_name + @"/";
            }
        }

        public string Folder_ClientSide
        {
            get
            {
                return @"/UserInterface/" + ui_name;
            }
        }
    }
}