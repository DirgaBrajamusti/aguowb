using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using System.Security.Cryptography;
using System.Text;
using System.IO;
using System.Web.Mvc;
using System.Data.SqlClient;

using MERCY.Data.EntityFramework;
using MERCY.Web.BackEnd.Security;

namespace MERCY.Web.BackEnd.Helpers
{
    public class OurUtility
    {
        internal const string INFO_SPRINT                   = "Sprint_8";

        public const string HEADER_RESPONSE                 = "MERCY_2-BackEnd";
        public const string MERCY_TOKEN                     = "MERCY-Token";
        public const string SERVER_MESSAGE                  = "Message from BackEnd";
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

        public const string Shipment_Type_Vessel            = "Trans";
        public const string Shipment_Type_Direct            = "Direct";


        public static int ToInt32(string p_str)
        {
            int result = 0;

            try
            {
                result = Convert.ToInt32(p_str);
            }
            catch {}

            return result;
        }

        public static int ToInt32(string p_str, int p_default)
        {
            int result = p_default;

            try
            {
                result = Convert.ToInt32(p_str);
            }
            catch {}

            return result;
        }

        public static long ToInt64(string p_str)
        {
            long result = 0;

            try
            {
                result = Convert.ToInt64(p_str);
            }
            catch {}

            return result;
        }

        public static decimal ToDecimal(string p_str)
        {
            decimal result = 0;

            try
            {
                p_str = p_str.Trim();

                result = Convert.ToDecimal(p_str);
            }
            catch {}

            return result;
        }

        public static decimal ToDecimal(string p_str, int i)
        {
            decimal result = 0;

            try
            {
                result = Decimal.Round(ToDecimal(p_str), i);
            }
            catch {}

            return result;
        }

        public static decimal ToDecimal(decimal p_decimal, int i)
        {
            decimal result = p_decimal;

            try
            {
                result = Decimal.Round(p_decimal, i);
            }
            catch {}

            return result;
        }

        public static decimal ToDecimal(string p_str, decimal p_default)
        {
            decimal result = p_default;

            try
            {
                p_str = p_str.Trim();

                result = Convert.ToDecimal(p_str);
            }
            catch {}

            return result;
        }

        public static string ToRoman(int number)
        {
            if ((number < 0) || (number > 3999)) throw new ArgumentOutOfRangeException("insert value betwheen 1 and 3999");
            if (number < 1) return string.Empty;
            if (number >= 1000) return "M" + ToRoman(number - 1000);
            if (number >= 900) return "CM" + ToRoman(number - 900);
            if (number >= 500) return "D" + ToRoman(number - 500);
            if (number >= 400) return "CD" + ToRoman(number - 400);
            if (number >= 100) return "C" + ToRoman(number - 100);
            if (number >= 90) return "XC" + ToRoman(number - 90);
            if (number >= 50) return "L" + ToRoman(number - 50);
            if (number >= 40) return "XL" + ToRoman(number - 40);
            if (number >= 10) return "X" + ToRoman(number - 10);
            if (number >= 9) return "IX" + ToRoman(number - 9);
            if (number >= 5) return "V" + ToRoman(number - 5);
            if (number >= 4) return "IV" + ToRoman(number - 4);
            if (number >= 1) return "I" + ToRoman(number - 1);
            throw new ArgumentOutOfRangeException("something bad happened");
        }

        public static string ValueOf(FormCollection p_collection, string p_key)
        {
            string result = string.Empty;

            try
            {
                result = p_collection[p_key].Trim();
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

        public static string DateFormat(DateTime p_date, string p_format)
        {
            string result = "0000-00-00";

            // {0:dd-MMM-yyyy HH:mm}
            // yyyy-MM-dd HHmmssfff
            try
            {
                //result = String.Format(p_format, p_date);
                result = p_date.ToString(p_format);
            }
            catch {}

            return result;
        }

        public static string DateFormat(DateTime? p_date, string p_format)
        {
            string result = "0000-00-00";
            try
            {
                result = DateFormat(p_date.Value, p_format);
            }
            catch {}

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

        public static string Sha256_Hash(string p_plainText)
        {
            string result = string.Empty;

            if (string.IsNullOrEmpty(p_plainText)) return result;

            try
            {
                // Create a SHA256   
                using (SHA256 sha256Hash = SHA256.Create())
                {
                    // ComputeHash - returns byte array  
                    byte[] bytes = sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(p_plainText));

                    // Convert byte array to a string   
                    StringBuilder builder = new StringBuilder();
                    for (int i = 0; i < bytes.Length; i++)
                    {
                        builder.Append(bytes[i].ToString("x2"));
                    }

                    result = builder.ToString();
                }

            }
            catch {}

            return result;
        }

        public static string Get_Header(HttpRequestBase p_request, string p_key)
        {
            string result = string.Empty;

            try
            {
                result = p_request.Headers[p_key];
            }
            catch {}

            if (string.IsNullOrEmpty(result)) result = string.Empty;

            return result;
        }

        public static bool Upload(HttpRequestBase p_request, string p_uploadFolder, ref string p_fileName, ref string p_fileName2, ref string p_message)
        {
            bool result = false;
            p_fileName = string.Empty;
            p_fileName2 = string.Empty;
            p_message = string.Empty; ;

            try
            {
                HttpPostedFileBase file = p_request.Files[0];

                // Verify that the user selected a file
                if (file != null && file.ContentLength > 0)
                {
                    using (BinaryReader b = new BinaryReader(file.InputStream))
                    {
                        DateTime now = DateTime.Now;
                        byte[] binData = b.ReadBytes(file.ContentLength);

                        p_fileName = file.FileName;
                        p_fileName2 = now.ToString("yyyy-MM-dd HHmmssfff--") + p_fileName;
                        string fileName = p_uploadFolder + p_fileName2;

                        using (FileStream fs = new FileStream(fileName, FileMode.Create))
                        {
                            using (BinaryWriter bw = new BinaryWriter(fs))
                            {
                                bw.Write(binData);

                                result = true;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                p_message = "Error: " + ex.Message;
            }

            return result;
        }

        public static bool Upload_Record(UserX p_executedBy, string p_fileName, string p_link, string p_fileType, ref long p_recordId, ref string p_message)
        {
            bool result = false;

            p_recordId = 0;
            p_message = string.Empty;

            try
            {
                using (MERCY_Ctx db = new MERCY_Ctx())
                {
                    var data = new TEMPORARY_File
                    {
                        FileName = p_fileName,
                        Link = p_link,
                        FileType = p_fileType,

                        CreatedOn = DateTime.Now,
                        CreatedBy = p_executedBy.UserId
                    };

                    db.TEMPORARY_File.Add(data);
                    db.SaveChanges();

                    p_recordId = data.RecordId;

                    result = true;
                }
            }
            catch (Exception ex)
            {
                p_message = ex.Message;
            }

            return result;
        }

        public static string Url(HttpRequestBase p_request)
        {
            //string url = p_request.Url.AbsolutePath.ToLower();

            String strPathAndQuery = p_request.Url.PathAndQuery;
            //String strUrl = p_request.Url.AbsoluteUri.Replace(strPathAndQuery, @"/");
            String strUrl = p_request.Url.AbsoluteUri.Replace(strPathAndQuery, "");

            return strUrl;
        }

        public static double ToDouble(string p_str)
        {
            double result = 0;

            try
            {
                result = Convert.ToDouble(p_str);
            }
            catch {}

            return result;
        }

        public static bool Upload(HttpRequestBase p_request, string p_indexFile, string p_uploadFolder, ref string p_fileName, ref string p_fileName2, ref string p_message)
        {
            bool result = false;
            p_fileName = string.Empty;
            p_fileName2 = string.Empty;
            p_message = string.Empty; ;

            try
            {
                HttpPostedFileBase file = p_request.Files[p_indexFile];

                // Verify that the user selected a file
                if (file != null && file.ContentLength > 0)
                {
                    using (BinaryReader b = new BinaryReader(file.InputStream))
                    {
                        DateTime now = DateTime.Now;
                        byte[] binData = b.ReadBytes(file.ContentLength);

                        p_fileName = file.FileName;
                        p_fileName2 = now.ToString("yyyy-MM-dd HHmmssfff--") + p_fileName;
                        string fileName = p_uploadFolder + p_fileName2;

                        using (FileStream fs = new FileStream(fileName, FileMode.Create))
                        {
                            using (BinaryWriter bw = new BinaryWriter(fs))
                            {
                                bw.Write(binData);

                                result = true;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                p_message = "Error: " + ex.Message;
            }

            return result;
        }

        public static double Round(double p_value, int i)
        {
            double result = p_value;

            try
            {
                result = Math.Round(p_value, i);
            }
            catch {}

            return result;
        }

        public static double Round(double? p_value, int i)
        {
            double result = 0.0;

            try
            {
                result = Math.Round(p_value.Value, i);
            }
            catch {}

            return result;
        }

        public static decimal Round(decimal p_value, int i)
        {
            decimal result = 0.0M;

            try
            {
                result = Math.Round(p_value, i);
            }
            catch {}

            return result;
        }

        public static string Round(string p_value, int i)
        {
            decimal x = ToDecimal(p_value);
            string result = Round(x, i).ToString();

            return result;
        }

        public static string FlattenException(Exception p_exception)
        {
            var stringBuilder = new StringBuilder();

            while (p_exception != null)
            {
                stringBuilder.AppendLine(p_exception.Message);
                stringBuilder.AppendLine(p_exception.StackTrace);

                p_exception = p_exception.InnerException;
            }

            return stringBuilder.ToString();
        }

        public static string FlattenException(Exception p_exception, int p_length)
        {
            string result = FlattenException(p_exception);

            try
            {
                result = result.Substring(0, p_length);
            }
            catch {}

            return result;
        }

        public static DateTime ToDateTime(string p_str)
        {
            DateTime result;

            try
            {
                //p_str = p_str.Substring(0, 10);
                result = DateTime.Parse(p_str);
                //result = DateTime.ParseExact(p_str, "ddd MMM dd yyyy HH:mm:ss 'GMT'K '(SE Asia Standard Time)'", CultureInfo.InvariantCulture);
            }
            catch
            {
                result = DateTime.Now;
            }

            return result;
        }

        public static string DateFormat(string p_str, string p_format)
        {
            return DateFormat(ToDateTime(p_str), p_format);
        }

        public static string Handle_Enter(string p_str)
        {
            string result = string.Empty;

            try
            {
                foreach (char c in p_str)
                {
                    if (c == '\n')
                    {
                        result += @"<br/>";
                    }
                    else
                    {
                        result += c;
                    }
                }
            }
            catch {}

            return result;
        }


        public static string Substring(string p_str, int p_index)
        {
            string result = string.Empty;
            try
            {
                result = p_str.Substring(p_index);
            }
            catch {}

            return result;
        }

        public static string ValueOf(SqlDataReader p_reader, string p_key)
        {
            string result = string.Empty;

            try
            {
                result = p_reader[p_key].ToString();
            }
            catch {}

            return result;
        }

        public static bool Is_Alphanumeric(string p_str)
        {
            bool result = false;

            try
            {
                if (string.IsNullOrEmpty(p_str)) return result;

                char[] myChars = p_str.ToCharArray();
                foreach (char myChr in myChars)
                {
                    if (!char.IsLetterOrDigit(myChr))
                    {
                        return result;
                    }
                }

                result = true;
            }
            catch {}

            return result;
        }

        public static void Validate_DecimalValue(ref string p_value, ref bool p_isValid)
        {
            p_isValid = false;
            if (p_value == @"n/a")
            {
                p_isValid = true;
            }
            else
            {
                decimal value_Decimal = ToDecimal(p_value, -9999.0M);
                if (value_Decimal > -9999.0M)
                {
                    p_isValid = true;
                }
            }
        }

        public static string ValueOf(Dictionary<string, string> p_values, string p_key)
        {
            string result = string.Empty;

            try
            {
                result = p_values[p_key].ToString();
            }
            catch {}

            return result;
        }
    }
}