using System;
using System.Collections.Generic;

using System.DirectoryServices;
using System.DirectoryServices.Protocols;

using MERCY.Web.BackEnd.Helpers;

namespace MERCY.Web.BackEnd.Security
{
    public class ActiveDirectory
    {
        //Source: https://stackoverflow.com/questions/290548/validate-a-username-and-password-against-active-directory/499716
        public static bool Authenticate(string p_loginName, string p_password, string p_server, ref string p_message)
        {
            bool result = false;

            p_message = string.Empty;

            try
            {
                DirectoryEntry entry = null;
                entry = new DirectoryEntry(p_server, p_loginName, p_password);
                object nativeObject = entry.NativeObject;
                result = true;
            }
            catch (LdapException lexc)
            {
                p_message = lexc.ServerErrorMessage + " [LdapException]";
            }
            catch (Exception ex)
            {
                p_message = ex.Message;
            }

            return result;
        }

        public static bool Authenticate(string p_loginName, string p_password, ref int p_DC, ref string p_message)
        {
            string server_DC_1 = Configuration.AD_Server;
            string server_DC_2 = Configuration.AD_Server_2;

            // -- Check Password in ActiveDirectory, DomainController :: 1
            p_DC = 1;
            bool result = Authenticate(p_loginName, p_password, server_DC_1, ref p_message);

            // only check in "Domain Controller: 2" if not Empty
            if ( !result &&
                 !string.IsNullOrEmpty(server_DC_2))
            {
                // -- Check Password in ActiveDirectory, DomainController :: 2
                p_DC = 2;
                result = Authenticate(p_loginName, p_password, server_DC_2, ref p_message);
            }

            return result;
        }

        private static bool Info(string p_loginName, string p_password, string p_server, string p_search_User
                                    , ref string p_detail_Name, ref string p_detail_Title, ref string p_detail_Department, ref string p_detail_Email
                                    , ref string p_message)
        {
            bool result = false;

            p_message = string.Empty;
            p_detail_Name = string.Empty;
            p_detail_Title = string.Empty;
            p_detail_Department = string.Empty;
            p_detail_Email = string.Empty;

            try
            {
                DirectoryEntry entry = new DirectoryEntry(p_server, p_loginName, p_password);

                DirectorySearcher searcher = new DirectorySearcher(entry)
                {
                    Filter = "(sAMAccountName=" + p_search_User + ")"
                };
                searcher.PropertiesToLoad.Add("title");
                searcher.PropertiesToLoad.Add("DisplayName");
                searcher.PropertiesToLoad.Add("department");
                searcher.PropertiesToLoad.Add("mail");

                SearchResult sr = searcher.FindOne();
                if (sr != null)
                {
                    DirectoryEntry entryToFind = sr.GetDirectoryEntry();

                    p_detail_Title = entryToFind.Properties["title"].Value as string;
                    p_detail_Name = entryToFind.Properties["DisplayName"].Value as string;
                    p_detail_Department = entryToFind.Properties["department"].Value as string;
                    p_detail_Email = entryToFind.Properties["mail"].Value as string;

                    if (string.IsNullOrEmpty(p_detail_Title)) p_detail_Title = string.Empty;
                    if (string.IsNullOrEmpty(p_detail_Name)) p_detail_Name = string.Empty;
                    if (string.IsNullOrEmpty(p_detail_Department)) p_detail_Department = string.Empty;
                    if (string.IsNullOrEmpty(p_detail_Email)) p_detail_Email = string.Empty;

                    result = true;
                }
            }
            catch (Exception ex)
            {
                p_message = ex.Message;
            }

            return result;
        }

        public static bool Info(string p_search_User, ref int p_DC, ref string p_Detail_Name, ref string p_Detail_Title, ref string p_detail_Department, ref string p_detail_Email, ref string p_message)
        {
            string server_DC_1 = Configuration.AD_Server;
            string server_DC_2 = Configuration.AD_Server_2;
            string loginName = Configuration.AD_User;
            string password = Configuration.AD_Password;

            // -- Check Password in ActiveDirectory, DomainController :: 1
            p_DC = 1;
            bool result = Info(loginName, password, server_DC_1, p_search_User, ref p_Detail_Name, ref p_Detail_Title, ref p_detail_Department, ref p_detail_Email, ref p_message);

            // only check in "Domain Controller: 2" if not Empty
            if ( !result &&
                 !string.IsNullOrEmpty(server_DC_2))
            {
                // -- Check Password in ActiveDirectory, DomainController :: 2
                p_DC = 2;
                result = Info(loginName, password, server_DC_2, p_search_User, ref p_Detail_Name, ref p_Detail_Title, ref p_detail_Department, ref p_detail_Email, ref p_message);
            }

            return result;
        }

        public static bool Info(string p_search_User, int p_DC, ref string p_message)
        {
            string server_DC_1 = Configuration.AD_Server;
            string server_DC_2 = Configuration.AD_Server_2;
            string loginName = Configuration.AD_User;
            string password = Configuration.AD_Password;

            string p_Detail_Name = string.Empty;
            string p_Detail_Title = string.Empty;
            string p_detail_Department = string.Empty;
            string p_detail_Email = string.Empty;

            if (p_DC == 1)
            {
                return Info(loginName, password, server_DC_1, p_search_User, ref p_Detail_Name, ref p_Detail_Title, ref p_detail_Department, ref p_detail_Email, ref p_message);
            }
            else
            {
                return Info(loginName, password, server_DC_2, p_search_User, ref p_Detail_Name, ref p_Detail_Title, ref p_detail_Department, ref p_detail_Email, ref p_message);
            }
        }

        public static bool Info_Himself(string p_search_User, string p_password, ref int p_DC, ref string p_Detail_Name, ref string p_Detail_Title, ref string p_detail_Department, ref string p_detail_Email, ref string p_message)
        {
            string server_DC_1 = Configuration.AD_Server;
            string server_DC_2 = Configuration.AD_Server_2;

            // -- Check Password in ActiveDirectory, DomainController :: 1
            p_DC = 1;
            bool result = Info(p_search_User, p_password, server_DC_1, p_search_User, ref p_Detail_Name, ref p_Detail_Title, ref p_detail_Department, ref p_detail_Email, ref p_message);

            // only check in "Domain Controller: 2" if not Empty
            if ( !result &&
                 !string.IsNullOrEmpty(server_DC_2))
            {
                // -- Check Password in ActiveDirectory, DomainController :: 2
                p_DC = 2;
                result = Info(p_search_User, p_password, server_DC_2, p_search_User, ref p_Detail_Name, ref p_Detail_Title, ref p_detail_Department, ref p_detail_Email, ref p_message);
            }

            return result;
        }

        private static IList<string> FindAll(string p_loginName, string p_password, string p_server, string p_search_User
                                            , ref string p_message)
        {
            IList<string> result = null;

            p_message = string.Empty;

            try
            {
                DirectoryEntry entry = new DirectoryEntry(p_server, p_loginName, p_password);

                DirectorySearcher searcher = new DirectorySearcher(entry)
                {
                    Filter = "(sAMAccountName=" + p_search_User + ")"
                };

                result = new List<string>();
                string account = string.Empty;

                foreach (SearchResult sr in searcher.FindAll())
                {
                    DirectoryEntry entryToFind = sr.GetDirectoryEntry();

                    account = entryToFind.Properties["sAMAccountName"].Value as string;

                    if (string.IsNullOrEmpty(account)) account = "kosong"; // string.Empty;
                    result.Add(account);
                }
            }
            catch (DirectoryServicesCOMException ex)
            {
                p_message = ex.Message;
            }

            return result;
        }

        public static IList<string> FindAll(string p_search_User, ref int p_DC, ref string p_message)
        {
            string server_DC_1 = Configuration.AD_Server;
            string server_DC_2 = Configuration.AD_Server_2;
            string loginName = Configuration.AD_User;
            string password = Configuration.AD_Password;

            p_search_User += "*";

            // -- Check User in ActiveDirectory, DomainController :: 1
            p_DC = 1;
            IList<string> result = FindAll(loginName, password, server_DC_1, p_search_User, ref p_message);
            if (result == null)
            {
                // -- Check User in ActiveDirectory, DomainController :: 2
                p_DC = 2;
                result = FindAll(loginName, password, server_DC_2, p_search_User, ref p_message);
            }

            return result;
        }

        public static bool Emails(string p_loginName, string p_password, string p_server, string p_search_User
                                    , ref string p_detail_Email, ref string p_message)
        {
            bool result = false;

            p_message = string.Empty;
            p_detail_Email = string.Empty;

            string[] users = p_search_User.Split(',');
            string email = string.Empty;
            string separator = string.Empty;

            try
            {
                DirectoryEntry entry = new DirectoryEntry(p_server, p_loginName, p_password);

                foreach (string user in users)
                {
                    // skip this User
                    if (user.Contains("-"))
                    {
                        p_detail_Email += separator + "";
                        separator = ",";

                        continue;
                    }

                    try
                    {
                        DirectorySearcher searcher = new DirectorySearcher(entry)
                        {
                            Filter = "(sAMAccountName=" + user + ")"
                        };

                        searcher.PropertiesToLoad.Add("mail");

                        SearchResult sr = searcher.FindOne();
                        if (sr != null)
                        {
                            DirectoryEntry entryToFind = sr.GetDirectoryEntry();

                            email = entryToFind.Properties["mail"].Value as string;

                            if (!string.IsNullOrEmpty(email))
                            {
                                p_detail_Email += separator + email;
                                separator = ",";
                            }
                        }
                    }
                    catch {}
                }
            }
            catch (Exception ex)
            {
                p_message = ex.Message;
            }

            return result;
        }
    }
}