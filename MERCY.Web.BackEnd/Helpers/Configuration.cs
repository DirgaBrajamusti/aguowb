using System.Collections.Generic;
using System.Linq;

using System.Configuration;
using MERCY.Data.EntityFramework;

namespace MERCY.Web.BackEnd.Helpers
{
    public class Configuration
    {
        internal const string INFO_VERSION = "20210706-1341";

        public static string VERSION
        {
            get { return OurUtility.INFO_SPRINT + "--" + INFO_VERSION; }
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

        public static string AD_Server
        {
            get { return AppSettings("AD_Server"); }
        }

        public static string AD_Server_2
        {
            get { return AppSettings("AD_Server_2"); }
        }

        public static string AD_User
        {
            get { return AppSettings("AD_User"); }
        }

        public static string AD_Password
        {
            get { return AppSettings("AD_Password"); }
        }

        public static string FolderUpload
        {
            get { return AppSettings("FolderUpload"); }
        }

        Dictionary<string, string> values = new Dictionary<string, string>();

        public Configuration()
        {
            try
            {
                using (MERCY_Ctx db = new MERCY_Ctx())
                {
                    var dataQuery =
                                    (
                                        from d in db.Configs
                                        select d
                                    );
                    var items = dataQuery.ToList();

                    items.ForEach(c =>
                    {
                        try
                        {
                            values.Add(c.Name, c.Value);
                        }
                        catch {}
                    });
                }
            }
            catch {}
        }

        public string this [string p_key]
        {
            get{
                string result = string.Empty;

                try
                {
                    result = values[p_key];
                }
                catch {}

                return result;
            }
        }

        private const string CONFIG_KEY_EMAIL_SERVER   = "Email_Server";
        private const string CONFIG_KEY_EMAIL_PORT     = "Email_Port";
        private const string CONFIG_KEY_EMAIL_USER     = "Email_User";
        private const string CONFIG_KEY_EMAIL_PASSWORD = "Email_Password";
        private const string CONFIG_KEY_EMAIL_SAMPLING = "Email_Subject_Sampling";
        private const string CONFIG_KEY_EMAIL_ANALYSIS = "Email_Subject_Analysis";
        private const string CONFIG_KEY_EMAIL_HAULING  = "Email_Subject_Hauling";
        private const string CONFIG_KEY_EMAIL_HAULING_USER    = "Email_User_Hauling";
        private const string CONFIG_KEY_EMAIL_HAULING_USER_CC = "Email_User_Hauling_CC";
        private const string CONFIG_KEY_EXCEL_PRODUCT_TCM = "PRODUCT_TCM";
        private const string CONFIG_KEY_EXCEL_PRODUCT_BEK = "PRODUCT_BEK";
        private const string CONFIG_KEY_REMINDER_CONSUMABLE_1 = "REMINDER_CONSUMABLE_1";
        private const string CONFIG_KEY_REMINDER_CONSUMABLE_2 = "REMINDER_CONSUMABLE_2";
        private const string CONFIG_KEY_REMINDER_MAINTENANCE_1 = "REMINDER_MAINTENANCE_1";
        private const string CONFIG_KEY_REMINDER_MAINTENANCE_2 = "REMINDER_MAINTENANCE_2";
        private const string CONFIG_KEY_REMINDER_CONSUMABLE_CC = "REMINDER_CONSUMABLE_CC";
        private const string CONFIG_KEY_REMINDER_MAINTENANCE_CC = "REMINDER_MAINTENANCE_CC";
        private const string CONFIG_KEY_REMINDER_CONSUMABLE_SUBJECT = "REMINDER_CONSUMABLE_SUBJECT";
        private const string CONFIG_KEY_REMINDER_MAINTENANCE_SUBJECT = "REMINDER_MAINTENANCE_SUBJECT";
        private const string CONFIG_KEY_EMAIL_FEEDBACK = "Email_Subject_Feedback";
        private const string CONFIG_KEY_EMAIL_FEEDBACK_USER = "Email_User_Feedback";
        private const string CONFIG_KEY_EMAIL_FEEDBACK_USER_CC = "Email_User_Feedback_CC";
        private const string CONFIG_KEY_EMAIL_USER_DOMAIN = "Email_User_Domain";
        
        public string Email_Server
        {
            get { return this[CONFIG_KEY_EMAIL_SERVER]; }
        }

        public string Email_Port
        {
            get { return this[CONFIG_KEY_EMAIL_PORT]; }
        }

        public string Email_User
        {
            get { return this[CONFIG_KEY_EMAIL_USER]; }
        }

        public string Email_Password
        {
            get { return this[CONFIG_KEY_EMAIL_PASSWORD]; }
        }

        public string Email_Sampling_Subject
        {
            get { return this[CONFIG_KEY_EMAIL_SAMPLING]; }
        }

        public string Email_Analysis_Subject
        {
            get { return this[CONFIG_KEY_EMAIL_ANALYSIS]; }
        }

        public string Email_Hauling_Subject
        {
            get { return this[CONFIG_KEY_EMAIL_HAULING]; }
        }

        public string Email_Hauling_User
        {
            get { return this[CONFIG_KEY_EMAIL_HAULING_USER]; }
        }

        public string Email_Hauling_User_CC
        {
            get { return this[CONFIG_KEY_EMAIL_HAULING_USER_CC]; }
        }

        public string Excel_Product_TCM
        {
            get { return this[CONFIG_KEY_EXCEL_PRODUCT_TCM]; }
        }

        public string Excel_Product_BEK
        {
            get { return this[CONFIG_KEY_EXCEL_PRODUCT_BEK]; }
        }

        public string Reminder_Consumable_1
        {
            get { return this[CONFIG_KEY_REMINDER_CONSUMABLE_1]; }
        }

        public string Reminder_Consumable_2
        {
            get { return this[CONFIG_KEY_REMINDER_CONSUMABLE_2]; }
        }

        public string Reminder_Maintenance_1
        {
            get { return this[CONFIG_KEY_REMINDER_MAINTENANCE_1]; }
        }

        public string Reminder_Maintenance_2
        {
            get { return this[CONFIG_KEY_REMINDER_MAINTENANCE_2]; }
        }

        public string Reminder_Consumable_CC
        {
            get { return this[CONFIG_KEY_REMINDER_CONSUMABLE_CC]; }
        }

        public string Reminder_Maintenance_CC
        {
            get { return this[CONFIG_KEY_REMINDER_MAINTENANCE_CC]; }
        }

        public string Reminder_Consumable_Subject
        {
            get { return this[CONFIG_KEY_REMINDER_CONSUMABLE_SUBJECT]; }
        }

        public string Reminder_Maintenance_Subject
        {
            get { return this[CONFIG_KEY_REMINDER_MAINTENANCE_SUBJECT]; }
        }

        public string Email_Feedback_Subject
        {
            get { return this[CONFIG_KEY_EMAIL_FEEDBACK]; }
        }

        public string Email_Feedback_User
        {
            get { return this[CONFIG_KEY_EMAIL_FEEDBACK_USER]; }
        }

        public string Email_Feedback_User_CC
        {
            get { return this[CONFIG_KEY_EMAIL_FEEDBACK_USER_CC]; }
        }

        public string Email_User_Domain
        {
            get { return this[CONFIG_KEY_EMAIL_USER_DOMAIN]; }
        }

        private const string CONFIG_KEY_PRODUCTION = "PRODUCTION";
        private const string CONFIG_KEY_FrontEnd_Url = "FrontEnd_Url";
        private const string CONFIG_KEY_Email_To = "Email_To";

        public bool Is_Production
        {
            get { return (this[CONFIG_KEY_PRODUCTION].ToLower() == "yes"); }
        }

        public string FrontEnd_Url
        {
            get { return this[CONFIG_KEY_FrontEnd_Url]; }
        }

        public string Email_To
        {
            get { return this[CONFIG_KEY_Email_To]; }
        }

        

        private const string CONFIG_KEY_Notification_Upload_ROM_Sampling_Date_Format = "Notification_Upload_ROM_Sampling_Date_Format";
        private const string CONFIG_KEY_Notification_Upload_ROM_Sampling_General_Email_Subject = "Notification_Upload_ROM_Sampling_General_Email_Subject";
        private const string CONFIG_KEY_Notification_Upload_ROM_Sampling_General_Email_To_Menu = "Notification_Upload_ROM_Sampling_General_Email_To_Menu";
        private const string CONFIG_KEY_Notification_Hauling_Request_Email_To_Menu = "Notification_Hauling_Request_Email_To_Menu";
        private const string CONFIG_KEY_Notification_Upload_ROM_Sampling_General_Email_To_Access = "Notification_Upload_ROM_Sampling_General_Email_To_Access";
        private const string CONFIG_KEY_Notification_Hauling_Request_Email_To_Access = "Notification_Hauling_Request_Email_To_Access";
        private const string CONFIG_KEY_Notification_Hauling_Request_Email_Cc_Access = "Notification_Hauling_Request_Email_Cc_Access";
        private const string CONFIG_KEY_Notification_Upload_ROM_Sampling_General_Email_Cc_Menu = "Notification_Upload_ROM_Sampling_General_Email_Cc_Menu";
        private const string CONFIG_KEY_Notification_Upload_ROM_Sampling_General_Email_Cc_Access = "Notification_Upload_ROM_Sampling_General_Email_Cc_Access";
        private const string CONFIG_KEY_Notification_Upload_ROM_Sampling_Specific_Email_Subject = "Notification_Upload_ROM_Sampling_Specific_Email_Subject";
        private const string CONFIG_KEY_Notification_Upload_ROM_Sampling_Specific_Email_To_Menu = "Notification_Upload_ROM_Sampling_Specific_Email_To_Menu";
        private const string CONFIG_KEY_Notification_Upload_ROM_Sampling_Specific_Email_To_Access = "Notification_Upload_ROM_Sampling_Specific_Email_To_Access";
        private const string CONFIG_KEY_Notification_Upload_ROM_Sampling_Specific_Email_Cc_Menu = "Notification_Upload_ROM_Sampling_Specific_Email_Cc_Menu";
        private const string CONFIG_KEY_Notification_Upload_ROM_Sampling_Specific_Email_Cc_Access = "Notification_Upload_ROM_Sampling_Specific_Email_Cc_Access";

        public string Notification_Upload_ROM_Sampling_Date_Format
        {
            get { return this[CONFIG_KEY_Notification_Upload_ROM_Sampling_Date_Format]; }
        }

        public string Notification_Upload_ROM_Sampling_General_Email_Subject
        {
            get { return this[CONFIG_KEY_Notification_Upload_ROM_Sampling_General_Email_Subject]; }
        }

        public string Notification_Upload_ROM_Sampling_General_Email_To_Menu
        {
            get { return this[CONFIG_KEY_Notification_Upload_ROM_Sampling_General_Email_To_Menu]; }
        }

        public string Notification_Upload_ROM_Sampling_General_Email_To_Access
        {
            get { return this[CONFIG_KEY_Notification_Upload_ROM_Sampling_General_Email_To_Access]; }
        }

        public string Notification_Upload_ROM_Sampling_General_Email_Cc_Menu
        {
            get { return this[CONFIG_KEY_Notification_Upload_ROM_Sampling_General_Email_Cc_Menu]; }
        }

        public string Notification_Upload_ROM_Sampling_General_Email_Cc_Access
        {
            get { return this[CONFIG_KEY_Notification_Upload_ROM_Sampling_General_Email_Cc_Access]; }
        }

        public string Notification_Upload_ROM_Sampling_Specific_Email_Subject
        {
            get { return this[CONFIG_KEY_Notification_Upload_ROM_Sampling_Specific_Email_Subject]; }
        }

        public string Notification_Upload_ROM_Sampling_Specific_Email_To_Menu
        {
            get { return this[CONFIG_KEY_Notification_Upload_ROM_Sampling_Specific_Email_To_Menu]; }
        }

        public string Notification_Upload_ROM_Sampling_Specific_Email_To_Access
        {
            get { return this[CONFIG_KEY_Notification_Upload_ROM_Sampling_Specific_Email_To_Access]; }
        }

        public string Notification_Upload_ROM_Sampling_Specific_Email_Cc_Menu
        {
            get { return this[CONFIG_KEY_Notification_Upload_ROM_Sampling_Specific_Email_Cc_Menu]; }
        }

        public string Notification_Upload_ROM_Sampling_Specific_Email_Cc_Access
        {
            get { return this[CONFIG_KEY_Notification_Upload_ROM_Sampling_Specific_Email_Cc_Access]; }
        }

        public string Notification_Hauling_Request_Email_To_Menu
        {
            get { return this[CONFIG_KEY_Notification_Hauling_Request_Email_To_Menu]; }
        } 
        
        public string Notification_Hauling_Request_Email_To_Access
        {
            get { return this[CONFIG_KEY_Notification_Hauling_Request_Email_To_Access]; }
        }
        
        public string Notification_Hauling_Request_Email_Cc_Access
        {
            get { return this[CONFIG_KEY_Notification_Hauling_Request_Email_Cc_Access]; }
        }

        private const string CONFIG_KEY_Notification_Upload_Geology_Pit_Monitoring_Date_Format = "Notification_Upload_Geology_Pit_Monitoring_Date_Format";
        private const string CONFIG_KEY_Notification_Upload_Geology_Pit_Monitoring_General_Email_Subject = "Notification_Upload_Geology_Pit_Monitoring_General_Email_Subject";
        private const string CONFIG_KEY_Notification_Upload_Geology_Pit_Monitoring_General_Email_To_Menu = "Notification_Upload_Geology_Pit_Monitoring_General_Email_To_Menu";
        private const string CONFIG_KEY_Notification_Upload_Geology_Pit_Monitoring_General_Email_To_Access = "Notification_Upload_Geology_Pit_Monitoring_General_Email_To_Access";
        private const string CONFIG_KEY_Notification_Upload_Geology_Pit_Monitoring_General_Email_Cc_Menu = "Notification_Upload_Geology_Pit_Monitoring_General_Email_Cc_Menu";
        private const string CONFIG_KEY_Notification_Upload_Geology_Pit_Monitoring_General_Email_Cc_Access = "Notification_Upload_Geology_Pit_Monitoring_General_Email_Cc_Access";
        private const string CONFIG_KEY_Notification_Upload_Geology_Pit_Monitoring_Specific_Email_Subject = "Notification_Upload_Geology_Pit_Monitoring_Specific_Email_Subject";
        private const string CONFIG_KEY_Notification_Upload_Geology_Pit_Monitoring_Specific_Email_To_Menu = "Notification_Upload_Geology_Pit_Monitoring_Specific_Email_To_Menu";
        private const string CONFIG_KEY_Notification_Upload_Geology_Pit_Monitoring_Specific_Email_To_Access = "Notification_Upload_Geology_Pit_Monitoring_Specific_Email_To_Access";
        private const string CONFIG_KEY_Notification_Upload_Geology_Pit_Monitoring_Specific_Email_Cc_Menu = "Notification_Upload_Geology_Pit_Monitoring_Specific_Email_Cc_Menu";
        private const string CONFIG_KEY_Notification_Upload_Geology_Pit_Monitoring_Specific_Email_Cc_Access = "Notification_Upload_Geology_Pit_Monitoring_Specific_Email_Cc_Access";

        public string Notification_Upload_Geology_Pit_Monitoring_Date_Format
        {
            get { return this[CONFIG_KEY_Notification_Upload_Geology_Pit_Monitoring_Date_Format]; }
        }

        public string Notification_Upload_Geology_Pit_Monitoring_General_Email_Subject
        {
            get { return this[CONFIG_KEY_Notification_Upload_Geology_Pit_Monitoring_General_Email_Subject]; }
        }

        public string Notification_Upload_Geology_Pit_Monitoring_General_Email_To_Menu
        {
            get { return this[CONFIG_KEY_Notification_Upload_Geology_Pit_Monitoring_General_Email_To_Menu]; }
        }

        public string Notification_Upload_Geology_Pit_Monitoring_General_Email_To_Access
        {
            get { return this[CONFIG_KEY_Notification_Upload_Geology_Pit_Monitoring_General_Email_To_Access]; }
        }

        public string Notification_Upload_Geology_Pit_Monitoring_General_Email_Cc_Menu
        {
            get { return this[CONFIG_KEY_Notification_Upload_Geology_Pit_Monitoring_General_Email_Cc_Menu]; }
        }

        public string Notification_Upload_Geology_Pit_Monitoring_General_Email_Cc_Access
        {
            get { return this[CONFIG_KEY_Notification_Upload_Geology_Pit_Monitoring_General_Email_Cc_Access]; }
        }

        public string Notification_Upload_Geology_Pit_Monitoring_Specific_Email_Subject
        {
            get { return this[CONFIG_KEY_Notification_Upload_Geology_Pit_Monitoring_Specific_Email_Subject]; }
        }

        public string Notification_Upload_Geology_Pit_Monitoring_Specific_Email_To_Menu
        {
            get { return this[CONFIG_KEY_Notification_Upload_Geology_Pit_Monitoring_Specific_Email_To_Menu]; }
        }

        public string Notification_Upload_Geology_Pit_Monitoring_Specific_Email_To_Access
        {
            get { return this[CONFIG_KEY_Notification_Upload_Geology_Pit_Monitoring_Specific_Email_To_Access]; }
        }

        public string Notification_Upload_Geology_Pit_Monitoring_Specific_Email_Cc_Menu
        {
            get { return this[CONFIG_KEY_Notification_Upload_Geology_Pit_Monitoring_Specific_Email_Cc_Menu]; }
        }

        public string Notification_Upload_Geology_Pit_Monitoring_Specific_Email_Cc_Access
        {
            get { return this[CONFIG_KEY_Notification_Upload_Geology_Pit_Monitoring_Specific_Email_Cc_Access]; }
        }

        private const string CONFIG_KEY_Notification_Upload_Geology_Exploration_Date_Format = "Notification_Upload_Geology_Exploration_Date_Format";
        private const string CONFIG_KEY_Notification_Upload_Geology_Exploration_General_Email_Subject = "Notification_Upload_Geology_Exploration_General_Email_Subject";
        private const string CONFIG_KEY_Notification_Upload_Geology_Exploration_General_Email_To_Menu = "Notification_Upload_Geology_Exploration_General_Email_To_Menu";
        private const string CONFIG_KEY_Notification_Upload_Geology_Exploration_General_Email_To_Access = "Notification_Upload_Geology_Exploration_General_Email_To_Access";
        private const string CONFIG_KEY_Notification_Upload_Geology_Exploration_General_Email_Cc_Menu = "Notification_Upload_Geology_Exploration_General_Email_Cc_Menu";
        private const string CONFIG_KEY_Notification_Upload_Geology_Exploration_General_Email_Cc_Access = "Notification_Upload_Geology_Exploration_General_Email_Cc_Access";
        private const string CONFIG_KEY_Notification_Upload_Geology_Exploration_Specific_Email_Subject = "Notification_Upload_Geology_Exploration_Specific_Email_Subject";
        private const string CONFIG_KEY_Notification_Upload_Geology_Exploration_Specific_Email_To_Menu = "Notification_Upload_Geology_Exploration_Specific_Email_To_Menu";
        private const string CONFIG_KEY_Notification_Upload_Geology_Exploration_Specific_Email_To_Access = "Notification_Upload_Geology_Exploration_Specific_Email_To_Access";
        private const string CONFIG_KEY_Notification_Upload_Geology_Exploration_Specific_Email_Cc_Menu = "Notification_Upload_Geology_Exploration_Specific_Email_Cc_Menu";
        private const string CONFIG_KEY_Notification_Upload_Geology_Exploration_Specific_Email_Cc_Access = "Notification_Upload_Geology_Exploration_Specific_Email_Cc_Access";

        public string Notification_Upload_Geology_Exploration_Date_Format
        {
            get { return this[CONFIG_KEY_Notification_Upload_Geology_Exploration_Date_Format]; }
        }

        public string Notification_Upload_Geology_Exploration_General_Email_Subject
        {
            get { return this[CONFIG_KEY_Notification_Upload_Geology_Exploration_General_Email_Subject]; }
        }

        public string Notification_Upload_Geology_Exploration_General_Email_To_Menu
        {
            get { return this[CONFIG_KEY_Notification_Upload_Geology_Exploration_General_Email_To_Menu]; }
        }

        public string Notification_Upload_Geology_Exploration_General_Email_To_Access
        {
            get { return this[CONFIG_KEY_Notification_Upload_Geology_Exploration_General_Email_To_Access]; }
        }

        public string Notification_Upload_Geology_Exploration_General_Email_Cc_Menu
        {
            get { return this[CONFIG_KEY_Notification_Upload_Geology_Exploration_General_Email_Cc_Menu]; }
        }

        public string Notification_Upload_Geology_Exploration_General_Email_Cc_Access
        {
            get { return this[CONFIG_KEY_Notification_Upload_Geology_Exploration_General_Email_Cc_Access]; }
        }

        public string Notification_Upload_Geology_Exploration_Specific_Email_Subject
        {
            get { return this[CONFIG_KEY_Notification_Upload_Geology_Exploration_Specific_Email_Subject]; }
        }

        public string Notification_Upload_Geology_Exploration_Specific_Email_To_Menu
        {
            get { return this[CONFIG_KEY_Notification_Upload_Geology_Exploration_Specific_Email_To_Menu]; }
        }

        public string Notification_Upload_Geology_Exploration_Specific_Email_To_Access
        {
            get { return this[CONFIG_KEY_Notification_Upload_Geology_Exploration_Specific_Email_To_Access]; }
        }

        public string Notification_Upload_Geology_Exploration_Specific_Email_Cc_Menu
        {
            get { return this[CONFIG_KEY_Notification_Upload_Geology_Exploration_Specific_Email_Cc_Menu]; }
        }

        public string Notification_Upload_Geology_Exploration_Specific_Email_Cc_Access
        {
            get { return this[CONFIG_KEY_Notification_Upload_Geology_Exploration_Specific_Email_Cc_Access]; }
        }

        private const string CONFIG_KEY_Notification_Upload_FC_Crushing_Date_Format = "Notification_Upload_FC_Crushing_Date_Format";
        private const string CONFIG_KEY_Notification_Upload_FC_Crushing_Subject = "Notification_Upload_FC_Crushing_Subject";
        private const string CONFIG_KEY_Notification_Upload_FC_Crushing_To_Menu = "Notification_Upload_FC_Crushing_To_Menu";
        private const string CONFIG_KEY_Notification_Upload_FC_Crushing_To_Access = "Notification_Upload_FC_Crushing_To_Access";
        private const string CONFIG_KEY_Notification_Upload_FC_Crushing_Cc_Menu = "Notification_Upload_FC_Crushing_Cc_Menu";
        private const string CONFIG_KEY_Notification_Upload_FC_Crushing_Cc_Access = "Notification_Upload_FC_Crushing_Cc_Access";

        public string Notification_Upload_FC_Crushing_Date_Format
        {
            get { return this[CONFIG_KEY_Notification_Upload_FC_Crushing_Date_Format]; }
        }

        public string Notification_Upload_FC_Crushing_Subject
        {
            get { return this[CONFIG_KEY_Notification_Upload_FC_Crushing_Subject]; }
        }

        public string Notification_Upload_FC_Crushing_To_Menu
        {
            get { return this[CONFIG_KEY_Notification_Upload_FC_Crushing_To_Menu]; }
        }

        public string Notification_Upload_FC_Crushing_To_Access
        {
            get { return this[CONFIG_KEY_Notification_Upload_FC_Crushing_To_Access]; }
        }

        public string Notification_Upload_FC_Crushing_Cc_Menu
        {
            get { return this[CONFIG_KEY_Notification_Upload_FC_Crushing_Cc_Menu]; }
        }

        public string Notification_Upload_FC_Crushing_Cc_Access
        {
            get { return this[CONFIG_KEY_Notification_Upload_FC_Crushing_Cc_Access]; }
        }

        private const string CONFIG_KEY_Notification_Upload_FC_Barging_Date_Format = "Notification_Upload_FC_Barging_Date_Format";
        private const string CONFIG_KEY_Notification_Upload_FC_Barging_Subject = "Notification_Upload_FC_Barging_Subject";
        private const string CONFIG_KEY_Notification_Upload_FC_Barging_To_Menu = "Notification_Upload_FC_Barging_To_Menu";
        private const string CONFIG_KEY_Notification_Upload_FC_Barging_To_Access = "Notification_Upload_FC_Barging_To_Access";
        private const string CONFIG_KEY_Notification_Upload_FC_Barging_Cc_Menu = "Notification_Upload_FC_Barging_Cc_Menu";
        private const string CONFIG_KEY_Notification_Upload_FC_Barging_Cc_Access = "Notification_Upload_FC_Barging_Cc_Access";

        public string Notification_Upload_FC_Barging_Date_Format
        {
            get { return this[CONFIG_KEY_Notification_Upload_FC_Barging_Date_Format]; }
        }

        public string Notification_Upload_FC_Barging_Subject
        {
            get { return this[CONFIG_KEY_Notification_Upload_FC_Barging_Subject]; }
        }

        public string Notification_Upload_FC_Barging_To_Menu
        {
            get { return this[CONFIG_KEY_Notification_Upload_FC_Barging_To_Menu]; }
        }

        public string Notification_Upload_FC_Barging_To_Access
        {
            get { return this[CONFIG_KEY_Notification_Upload_FC_Barging_To_Access]; }
        }

        public string Notification_Upload_FC_Barging_Cc_Menu
        {
            get { return this[CONFIG_KEY_Notification_Upload_FC_Barging_Cc_Menu]; }
        }

        public string Notification_Upload_FC_Barging_Cc_Access
        {
            get { return this[CONFIG_KEY_Notification_Upload_FC_Barging_Cc_Access]; }
        }

        private const string CONFIG_KEY_Notification_Sampling_Request_Date_Format = "Notification_Sampling_Request_Date_Format";
        private const string CONFIG_KEY_Notification_Sampling_Request_Subject = "Notification_Sampling_Request_Subject";
        private const string CONFIG_KEY_Notification_Sampling_Request_To_Menu = "Notification_Sampling_Request_To_Menu";
        private const string CONFIG_KEY_Notification_Sampling_Request_To_Access = "Notification_Sampling_Request_To_Access";
        private const string CONFIG_KEY_Notification_Sampling_Request_Cc_Menu = "Notification_Sampling_Request_Cc_Menu";
        private const string CONFIG_KEY_Notification_Sampling_Request_Cc_Access = "Notification_Sampling_Request_Cc_Access";

        public string Notification_Sampling_Request_Date_Format
        {
            get { return this[CONFIG_KEY_Notification_Sampling_Request_Date_Format]; }
        }

        public string Notification_Sampling_Request_Subject
        {
            get { return this[CONFIG_KEY_Notification_Sampling_Request_Subject]; }
        }

        public string Notification_Sampling_Request_To_Menu
        {
            get { return this[CONFIG_KEY_Notification_Sampling_Request_To_Menu]; }
        }

        public string Notification_Sampling_Request_To_Access
        {
            get { return this[CONFIG_KEY_Notification_Sampling_Request_To_Access]; }
        }

        public string Notification_Sampling_Request_Cc_Menu
        {
            get { return this[CONFIG_KEY_Notification_Sampling_Request_Cc_Menu]; }
        }

        public string Notification_Sampling_Request_Cc_Access
        {
            get { return this[CONFIG_KEY_Notification_Sampling_Request_Cc_Access]; }
        }

        private const string CONFIG_KEY_Notification_Analysis_Request_Date_Format = "Notification_Analysis_Request_Date_Format";
        private const string CONFIG_KEY_Notification_Analysis_Request_Subject = "Notification_Analysis_Request_Subject";
        private const string CONFIG_KEY_Notification_Analysis_Request_To_Menu = "Notification_Analysis_Request_To_Menu";
        private const string CONFIG_KEY_Notification_Analysis_Request_To_Access = "Notification_Analysis_Request_To_Access";
        private const string CONFIG_KEY_Notification_Analysis_Request_Cc_Menu = "Notification_Analysis_Request_Cc_Menu";
        private const string CONFIG_KEY_Notification_Analysis_Request_Cc_Access = "Notification_Analysis_Request_Cc_Access";


        public string Notification_AnalysisRequest_Date_Format
        {
            get { return this[CONFIG_KEY_Notification_Analysis_Request_Date_Format]; }
        }

        public string Notification_Analysis_Request_Subject
        {
            get { return this[CONFIG_KEY_Notification_Analysis_Request_Subject]; }
        }

        public string Notification_Analysis_Request_To_Menu
        {
            get { return this[CONFIG_KEY_Notification_Analysis_Request_To_Menu]; }
        }

        public string Notification_Analysis_Request_To_Access
        {
            get { return this[CONFIG_KEY_Notification_Analysis_Request_To_Access]; }
        }

        public string Notification_Analysis_Request_Cc_Menu
        {
            get { return this[CONFIG_KEY_Notification_Analysis_Request_Cc_Menu]; }
        }

        public string Notification_Analysis_Request_Cc_Access
        {
            get { return this[CONFIG_KEY_Notification_Analysis_Request_Cc_Access]; }
        }

        private const string CONFIG_KEY_Notification_Discussion_Date_Format = "Notification_Discussion_Date_Format";
        private const string CONFIG_KEY_Notification_Discussion_Subject = "Notification_Discussion_Subject";
        private const string CONFIG_KEY_Notification_Discussion_To_Menu = "Notification_Discussion_To_Menu";
        private const string CONFIG_KEY_Notification_Discussion_To_Access = "Notification_Discussion_To_Access";
        private const string CONFIG_KEY_Notification_Discussion_Cc_Menu = "Notification_Discussion_Cc_Menu";
        private const string CONFIG_KEY_Notification_Discussion_Cc_Access = "Notification_Discussion_Cc_Access";

        public string Notification_Discussion_Date_Format
        {
            get { return this[CONFIG_KEY_Notification_Discussion_Date_Format]; }
        }

        public string Notification_Discussion_Subject
        {
            get { return this[CONFIG_KEY_Notification_Discussion_Subject]; }
        }

        public string Notification_Discussion_To_Menu
        {
            get { return this[CONFIG_KEY_Notification_Discussion_To_Menu]; }
        }

        public string Notification_Discussion_To_Access
        {
            get { return this[CONFIG_KEY_Notification_Discussion_To_Access]; }
        }

        public string Notification_Discussion_Cc_Menu
        {
            get { return this[CONFIG_KEY_Notification_Discussion_Cc_Menu]; }
        }

        public string Notification_Discussion_Cc_Access
        {
            get { return this[CONFIG_KEY_Notification_Discussion_Cc_Access]; }
        }

        private const string CONFIG_KEY_Notification_Analysis_Request_Verification_Date_Format = "Notification_Analysis_Request_Verification_Date_Format";
        private const string CONFIG_KEY_Notification_Analysis_Request_Verification_Subject = "Notification_Analysis_Request_Verification_Subject";
        private const string CONFIG_KEY_Notification_Analysis_Request_Verification_To_Menu = "Notification_Analysis_Request_Verification_To_Menu";
        private const string CONFIG_KEY_Notification_Analysis_Request_Verification_To_Access = "Notification_Analysis_Request_Verification_To_Access";
        private const string CONFIG_KEY_Notification_Analysis_Request_Verification_Cc_Menu = "Notification_Analysis_Request_Verification_Cc_Menu";
        private const string CONFIG_KEY_Notification_Analysis_Request_Verification_Cc_Access = "Notification_Analysis_Request_Verification_Cc_Access";


        public string Notification_AnalysisRequest_Verification_Date_Format
        {
            get { return this[CONFIG_KEY_Notification_Analysis_Request_Verification_Date_Format]; }
        }

        public string Notification_Analysis_Request_Verification_Subject
        {
            get { return this[CONFIG_KEY_Notification_Analysis_Request_Verification_Subject]; }
        }

        public string Notification_Analysis_Request_Verification_To_Menu
        {
            get { return this[CONFIG_KEY_Notification_Analysis_Request_Verification_To_Menu]; }
        }

        public string Notification_Analysis_Request_Verification_To_Access
        {
            get { return this[CONFIG_KEY_Notification_Analysis_Request_Verification_To_Access]; }
        }

        public string Notification_Analysis_Request_Verification_Cc_Menu
        {
            get { return this[CONFIG_KEY_Notification_Analysis_Request_Verification_Cc_Menu]; }
        }

        public string Notification_Analysis_Request_Verification_Cc_Access
        {
            get { return this[CONFIG_KEY_Notification_Analysis_Request_Verification_Cc_Access]; }
        }

        private const string CONFIG_KEY_Notification_Upload_HAC_Date_Format = "Notification_Upload_HAC_Date_Format";
        private const string CONFIG_KEY_Notification_Upload_HAC_General_Email_Subject = "Notification_Upload_HAC_General_Email_Subject";
        private const string CONFIG_KEY_Notification_Upload_HAC_General_Email_To_Menu = "Notification_Upload_HAC_General_Email_To_Menu";
        private const string CONFIG_KEY_Notification_Upload_HAC_General_Email_To_Access = "Notification_Upload_HAC_General_Email_To_Access";
        private const string CONFIG_KEY_Notification_Upload_HAC_General_Email_Cc_Menu = "Notification_Upload_HAC_General_Email_Cc_Menu";
        private const string CONFIG_KEY_Notification_Upload_HAC_General_Email_Cc_Access = "Notification_Upload_HAC_General_Email_Cc_Access";
        private const string CONFIG_KEY_Notification_Upload_HAC_Specific_Email_Subject = "Notification_Upload_HAC_Specific_Email_Subject";
        private const string CONFIG_KEY_Notification_Upload_HAC_Specific_Email_To_Menu = "Notification_Upload_HAC_Specific_Email_To_Menu";
        private const string CONFIG_KEY_Notification_Upload_HAC_Specific_Email_To_Access = "Notification_Upload_HAC_Specific_Email_To_Access";
        private const string CONFIG_KEY_Notification_Upload_HAC_Specific_Email_Cc_Menu = "Notification_Upload_HAC_Specific_Email_Cc_Menu";
        private const string CONFIG_KEY_Notification_Upload_HAC_Specific_Email_Cc_Access = "Notification_Upload_HAC_Specific_Email_Cc_Access";

        public string Notification_Upload_HAC_Date_Format
        {
            get { return this[CONFIG_KEY_Notification_Upload_HAC_Date_Format]; }
        }

        public string Notification_Upload_HAC_General_Email_Subject
        {
            get { return this[CONFIG_KEY_Notification_Upload_HAC_General_Email_Subject]; }
        }

        public string Notification_Upload_HAC_General_Email_To_Menu
        {
            get { return this[CONFIG_KEY_Notification_Upload_HAC_General_Email_To_Menu]; }
        }

        public string Notification_Upload_HAC_General_Email_To_Access
        {
            get { return this[CONFIG_KEY_Notification_Upload_HAC_General_Email_To_Access]; }
        }

        public string Notification_Upload_HAC_General_Email_Cc_Menu
        {
            get { return this[CONFIG_KEY_Notification_Upload_HAC_General_Email_Cc_Menu]; }
        }

        public string Notification_Upload_HAC_General_Email_Cc_Access
        {
            get { return this[CONFIG_KEY_Notification_Upload_HAC_General_Email_Cc_Access]; }
        }

        public string Notification_Upload_HAC_Specific_Email_Subject
        {
            get { return this[CONFIG_KEY_Notification_Upload_HAC_Specific_Email_Subject]; }
        }

        public string Notification_Upload_HAC_Specific_Email_To_Menu
        {
            get { return this[CONFIG_KEY_Notification_Upload_HAC_Specific_Email_To_Menu]; }
        }

        public string Notification_Upload_HAC_Specific_Email_To_Access
        {
            get { return this[CONFIG_KEY_Notification_Upload_HAC_Specific_Email_To_Access]; }
        }

        public string Notification_Upload_HAC_Specific_Email_Cc_Menu
        {
            get { return this[CONFIG_KEY_Notification_Upload_HAC_Specific_Email_Cc_Menu]; }
        }

        public string Notification_Upload_HAC_Specific_Email_Cc_Access
        {
            get { return this[CONFIG_KEY_Notification_Upload_HAC_Specific_Email_Cc_Access]; }
        }

        private const string CONFIG_KEY_Notification_Actual_Loading_Finalize_Date_Format = "Notification_Actual_Loading_Finalize_Date_Format";
        private const string CONFIG_KEY_Notification_Actual_Loading_Finalize_Subject = "Notification_Actual_Loading_Finalize_Subject";
        private const string CONFIG_KEY_Notification_Actual_Loading_Finalize_To_Menu = "Notification_Actual_Loading_Finalize_To_Menu";
        private const string CONFIG_KEY_Notification_Actual_Loading_Finalize_To_Access = "Notification_Actual_Loading_Finalize_To_Access";
        private const string CONFIG_KEY_Notification_Actual_Loading_Finalize_Cc_Menu = "Notification_Actual_Loading_Finalize_Cc_Menu";
        private const string CONFIG_KEY_Notification_Actual_Loading_Finalize_Cc_Access = "Notification_Actual_Loading_Finalize_Cc_Access";

        public string Notification_Actual_Loading_Finalize_Date_Format
        {
            get { return this[CONFIG_KEY_Notification_Actual_Loading_Finalize_Date_Format]; }
        }

        public string Notification_Actual_Loading_Finalize_Subject
        {
            get { return this[CONFIG_KEY_Notification_Actual_Loading_Finalize_Subject]; }
        }

        public string Notification_Actual_Loading_Finalize_To_Menu
        {
            get { return this[CONFIG_KEY_Notification_Actual_Loading_Finalize_To_Menu]; }
        }

        public string Notification_Actual_Loading_Finalize_To_Access
        {
            get { return this[CONFIG_KEY_Notification_Actual_Loading_Finalize_To_Access]; }
        }

        public string Notification_Actual_Loading_Finalize_Cc_Menu
        {
            get { return this[CONFIG_KEY_Notification_Actual_Loading_Finalize_Cc_Menu]; }
        }

        public string Notification_Actual_Loading_Finalize_Cc_Access
        {
            get { return this[CONFIG_KEY_Notification_Actual_Loading_Finalize_Cc_Access]; }
        }

        private const string CONFIG_KEY_Notification_Actual_Tunnel_Approve_Date_Format = "Notification_Actual_Tunnel_Approve_Date_Format";
        private const string CONFIG_KEY_Notification_Actual_Tunnel_Approve_Subject = "Notification_Actual_Tunnel_Approve_Subject";
        private const string CONFIG_KEY_Notification_Actual_Tunnel_Approve_To_Menu = "Notification_Actual_Tunnel_Approve_To_Menu";
        private const string CONFIG_KEY_Notification_Actual_Tunnel_Approve_To_Access = "Notification_Actual_Tunnel_Approve_To_Access";
        private const string CONFIG_KEY_Notification_Actual_Tunnel_Approve_Cc_Menu = "Notification_Actual_Tunnel_Approve_Cc_Menu";
        private const string CONFIG_KEY_Notification_Actual_Tunnel_Approve_Cc_Access = "Notification_Actual_Tunnel_Approve_Cc_Access";

        public string Notification_Actual_Tunnel_Approve_Date_Format
        {
            get { return this[CONFIG_KEY_Notification_Actual_Tunnel_Approve_Date_Format]; }
        }

        public string Notification_Actual_Tunnel_Approve_Subject
        {
            get { return this[CONFIG_KEY_Notification_Actual_Tunnel_Approve_Subject]; }
        }

        public string Notification_Actual_Tunnel_Approve_To_Menu
        {
            get { return this[CONFIG_KEY_Notification_Actual_Tunnel_Approve_To_Menu]; }
        }

        public string Notification_Actual_Tunnel_Approve_To_Access
        {
            get { return this[CONFIG_KEY_Notification_Actual_Tunnel_Approve_To_Access]; }
        }

        public string Notification_Actual_Tunnel_Approve_Cc_Menu
        {
            get { return this[CONFIG_KEY_Notification_Actual_Tunnel_Approve_Cc_Menu]; }
        }

        public string Notification_Actual_Tunnel_Approve_Cc_Access
        {
            get { return this[CONFIG_KEY_Notification_Actual_Tunnel_Approve_Cc_Access]; }
        }

        private const string CONFIG_KEY_Notification_Actual_Tunnel_Draft_Date_Format = "Notification_Actual_Tunnel_Draft_Date_Format";
        private const string CONFIG_KEY_Notification_Actual_Tunnel_Draft_Subject = "Notification_Actual_Tunnel_Draft_Subject";
        private const string CONFIG_KEY_Notification_Actual_Tunnel_Draft_To_Menu = "Notification_Actual_Tunnel_Draft_To_Menu";
        private const string CONFIG_KEY_Notification_Actual_Tunnel_Draft_To_Access = "Notification_Actual_Tunnel_Draft_To_Access";
        private const string CONFIG_KEY_Notification_Actual_Tunnel_Draft_Cc_Menu = "Notification_Actual_Tunnel_Draft_Cc_Menu";
        private const string CONFIG_KEY_Notification_Actual_Tunnel_Draft_Cc_Access = "Notification_Actual_Tunnel_Draft_Cc_Access";

        public string Notification_Actual_Tunnel_Draft_Date_Format
        {
            get { return this[CONFIG_KEY_Notification_Actual_Tunnel_Draft_Date_Format]; }
        }

        public string Notification_Actual_Tunnel_Draft_Subject
        {
            get { return this[CONFIG_KEY_Notification_Actual_Tunnel_Draft_Subject]; }
        }

        public string Notification_Actual_Tunnel_Draft_To_Menu
        {
            get { return this[CONFIG_KEY_Notification_Actual_Tunnel_Draft_To_Menu]; }
        }

        public string Notification_Actual_Tunnel_Draft_To_Access
        {
            get { return this[CONFIG_KEY_Notification_Actual_Tunnel_Draft_To_Access]; }
        }

        public string Notification_Actual_Tunnel_Draft_Cc_Menu
        {
            get { return this[CONFIG_KEY_Notification_Actual_Tunnel_Draft_Cc_Menu]; }
        }

        public string Notification_Actual_Tunnel_Draft_Cc_Access
        {
            get { return this[CONFIG_KEY_Notification_Actual_Tunnel_Draft_Cc_Access]; }
        }

        private const string CONFIG_KEY_Notification_Discussion_TunnelManagement_Date_Format = "Notification_Discussion_TunnelManagement_Date_Format";
        private const string CONFIG_KEY_Notification_Discussion_TunnelManagement_Subject = "Notification_Discussion_TunnelManagement_Subject";
        private const string CONFIG_KEY_Notification_Discussion_TunnelManagement_To_Menu = "Notification_Discussion_TunnelManagement_To_Menu";
        private const string CONFIG_KEY_Notification_Discussion_TunnelManagement_To_Access = "Notification_Discussion_TunnelManagement_To_Access";
        private const string CONFIG_KEY_Notification_Discussion_TunnelManagement_Cc_Menu = "Notification_Discussion_TunnelManagement_Cc_Menu";
        private const string CONFIG_KEY_Notification_Discussion_TunnelManagement_Cc_Access = "Notification_Discussion_TunnelManagement_Cc_Access";

        public string Notification_Discussion_TunnelManagement_Date_Format
        {
            get { return this[CONFIG_KEY_Notification_Discussion_TunnelManagement_Date_Format]; }
        }

        public string Notification_Discussion_TunnelManagement_Subject
        {
            get { return this[CONFIG_KEY_Notification_Discussion_TunnelManagement_Subject]; }
        }

        public string Notification_Discussion_TunnelManagement_To_Menu
        {
            get { return this[CONFIG_KEY_Notification_Discussion_TunnelManagement_To_Menu]; }
        }

        public string Notification_Discussion_TunnelManagement_To_Access
        {
            get { return this[CONFIG_KEY_Notification_Discussion_TunnelManagement_To_Access]; }
        }

        public string Notification_Discussion_TunnelManagement_Cc_Menu
        {
            get { return this[CONFIG_KEY_Notification_Discussion_TunnelManagement_Cc_Menu]; }
        }

        public string Notification_Discussion_TunnelManagement_Cc_Access
        {
            get { return this[CONFIG_KEY_Notification_Discussion_TunnelManagement_Cc_Access]; }
        }

        private const string CONFIG_KEY_Notification_Tunnel_Approve_Date_Format = "Notification_Tunnel_Approve_Date_Format";
        private const string CONFIG_KEY_Notification_Tunnel_Approve_Subject = "Notification_Tunnel_Approve_Subject";
        private const string CONFIG_KEY_Notification_Tunnel_Approve_To_Menu = "Notification_Tunnel_Approve_To_Menu";
        private const string CONFIG_KEY_Notification_Tunnel_Approve_To_Access = "Notification_Tunnel_Approve_To_Access";
        private const string CONFIG_KEY_Notification_Tunnel_Approve_Cc_Menu = "Notification_Tunnel_Approve_Cc_Menu";
        private const string CONFIG_KEY_Notification_Tunnel_Approve_Cc_Access = "Notification_Tunnel_Approve_Cc_Access";

        public string Notification_Tunnel_Approve_Date_Format
        {
            get { return this[CONFIG_KEY_Notification_Tunnel_Approve_Date_Format]; }
        }

        public string Notification_Tunnel_Approve_Subject
        {
            get { return this[CONFIG_KEY_Notification_Tunnel_Approve_Subject]; }
        }

        public string Notification_Tunnel_Approve_To_Menu
        {
            get { return this[CONFIG_KEY_Notification_Tunnel_Approve_To_Menu]; }
        }

        public string Notification_Tunnel_Approve_To_Access
        {
            get { return this[CONFIG_KEY_Notification_Tunnel_Approve_To_Access]; }
        }

        public string Notification_Tunnel_Approve_Cc_Menu
        {
            get { return this[CONFIG_KEY_Notification_Tunnel_Approve_Cc_Menu]; }
        }

        public string Notification_Tunnel_Approve_Cc_Access
        {
            get { return this[CONFIG_KEY_Notification_Tunnel_Approve_Cc_Access]; }
        }

        private const string CONFIG_KEY_SESSION_SECURITY_KEY = "Session_Security_Key";
        private const string CONFIG_KEY_SESSION_EXPIRED_TIME = "Session_Expired_Time_in_Minutes";

        public string Session_Security_Key
        {
            get { return this[CONFIG_KEY_SESSION_SECURITY_KEY]; }
        }

        public string Session_Expired_Time_in_Minutes
        {
            get { return this[CONFIG_KEY_SESSION_EXPIRED_TIME]; }
        }

        private const string CONFIG_KEY_Notification_Analysis_Result_Validate_Subject = "Notification_Analysis_Result_Validate_Subject";
        private const string CONFIG_KEY_Notification_Analysis_Result_Validate_To_Menu = "Notification_Analysis_Result_Validate_To_Menu";
        private const string CONFIG_KEY_Notification_Analysis_Result_Validate_To_Access = "Notification_Analysis_Result_Validate_To_Access";
        private const string CONFIG_KEY_Notification_Analysis_Result_Validate_Cc_Menu = "Notification_Analysis_Result_Validate_Cc_Menu";
        private const string CONFIG_KEY_Notification_Analysis_Result_Validate_Cc_Access = "Notification_Analysis_Result_Validate_Cc_Access";

        public string Notification_Analysis_Result_Validate_Subject
        {
            get { return this[CONFIG_KEY_Notification_Analysis_Result_Validate_Subject]; }
        }

        public string Notification_Analysis_Result_Validate_To_Menu
        {
            get { return this[CONFIG_KEY_Notification_Analysis_Result_Validate_To_Menu]; }
        }

        public string Notification_Analysis_Result_Validate_To_Access
        {
            get { return this[CONFIG_KEY_Notification_Analysis_Result_Validate_To_Access]; }
        }

        public string Notification_Analysis_Result_Validate_Cc_Menu
        {
            get { return this[CONFIG_KEY_Notification_Analysis_Result_Validate_Cc_Menu]; }
        }

        public string Notification_Analysis_Result_Validate_Cc_Access
        {
            get { return this[CONFIG_KEY_Notification_Analysis_Result_Validate_Cc_Access]; }
        }

        private const string CONFIG_KEY_Notification_Analysis_Result_Approve_Subject = "Notification_Analysis_Result_Approve_Subject";
        private const string CONFIG_KEY_Notification_Analysis_Result_Approve_To_Menu = "Notification_Analysis_Result_Approve_To_Menu";
        private const string CONFIG_KEY_Notification_Analysis_Result_Approve_To_Access = "Notification_Analysis_Result_Approve_To_Access";
        private const string CONFIG_KEY_Notification_Analysis_Result_Approve_Cc_Menu = "Notification_Analysis_Result_Approve_Cc_Menu";
        private const string CONFIG_KEY_Notification_Analysis_Result_Approve_Cc_Access = "Notification_Analysis_Result_Approve_Cc_Access";
        private const string CONFIG_KEY_Notification_Request_Submission_Subject = "Notification_Request_Submission_Subject";
        private const string CONFIG_KEY_Notification_Finalize_Coal_Inventory = "Notification_Finalize_Coal_Inventory";

        public string Notification_Analysis_Result_Approve_Subject
        {
            get { return this[CONFIG_KEY_Notification_Analysis_Result_Approve_Subject]; }
        }

        public string Notification_Analysis_Result_Approve_To_Menu
        {
            get { return this[CONFIG_KEY_Notification_Analysis_Result_Approve_To_Menu]; }
        }

        public string Notification_Analysis_ResultApprove_To_Access
        {
            get { return this[CONFIG_KEY_Notification_Analysis_Result_Approve_To_Access]; }
        }

        public string Notification_Analysis_Result_Approve_Cc_Menu
        {
            get { return this[CONFIG_KEY_Notification_Analysis_Result_Approve_Cc_Menu]; }
        }

        public string Notification_Analysis_Result_Approve_Cc_Access
        {
            get { return this[CONFIG_KEY_Notification_Analysis_Result_Approve_Cc_Access]; }
        }

        public string Notification_Request_Submission_Subject
        {
            get { return this[CONFIG_KEY_Notification_Request_Submission_Subject]; }
        }

        public string Notification_Finalize_Coal_Inventory
        {
            get { return this[CONFIG_KEY_Notification_Finalize_Coal_Inventory]; }
        }

        private const string CONFIG_COAL_INVENTORY_INPUT_PERIOD = "COAL_INVENTORY_INPUT_PERIOD";
        public string COAL_INVENTORY_INPUT_PERIOD
        {
            get { return this[CONFIG_COAL_INVENTORY_INPUT_PERIOD]; }
        }

        private const string CONFIG_LOADING_REQUEST_NTH_DAY = "LOADING_REQUEST_NTH_DAY";
        public string LOADING_REQUEST_NTH_DAY
        {
            get { return this[CONFIG_LOADING_REQUEST_NTH_DAY]; }
        }

        private const string CONFIG_PYRATE_OXIDATION_REACTION_NUMBER = "PYRATE_OXIDATION_REACTION_NUMBER";
        public string PYRATE_OXIDATION_REACTION_NUMBER
        {
            get { return this[CONFIG_PYRATE_OXIDATION_REACTION_NUMBER]; }
        }
    }
}