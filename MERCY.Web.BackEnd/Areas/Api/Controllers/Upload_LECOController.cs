using System;
using System.Linq;
using System.Web.Mvc;

using MERCY.Data.EntityFramework;
using MERCY.Web.BackEnd.Helpers;
using MERCY.Web.BackEnd.Security;
using Permission = MERCY.Web.BackEnd.Security.Permission;

namespace MERCY.Web.BackEnd.Areas.Api.Controllers
{
    public class Upload_LECOController : Controller
    {
        public JsonResult CV(FormCollection p_collection)
        {
            // Check Permission based on Current Url
            UserX user = new UserX(Request);

            // -- Not necessary checking Permission
            //Permission.Check_API(Request, user, ref permission_Item);
            // -- just Logging User: is enough
            if (user.UserId <= 0)
            {
                var msg = new { Success = false, Message = Permission.ERROR_PERMISSION_READ + " [not Login]", MessageDetail = string.Empty, Version = Configuration.VERSION };
                return Json(msg, JsonRequestBehavior.AllowGet);
            }

            // -- Actual code
            try
            {
                using (MERCY_Ctx db = new MERCY_Ctx())
                {
                    string str = OurUtility.ValueOf(p_collection, "data");

                    string[] lines = str.Split(new[] { System.Environment.NewLine }, StringSplitOptions.None);
                    int line_Number = 0;

                    int record_Count = 0;
                    string record_Message = string.Empty;
                    string[] columns = null;
                    UPLOAD_LECO_CV data = null;

                    foreach (string line in lines)
                    {
                        line_Number++;

                        // Skip: "Header"
                        if (line_Number <= 1) continue;

                        try
                        {
                            // file .csv --> asumsi Separator adalah Comma
                            char csv_Separator = ',';

                            // check kembali: pakai Comma atau pakai TAB
                            if ( ! line.Contains(csv_Separator))
                            {
                                // berarti pakai TAB
                                csv_Separator = '\t';
                            }

                            // lakukan Splitting terhadap baris 
                            columns = line.Split(csv_Separator);

                            // Insert ke Table
                            data = new UPLOAD_LECO_CV
                            {
                                NAME = columns[0],
                                HYDROGEN = columns[1],
                                OPERATOR = columns[2],
                                DISCRIPTION = columns[3],
                                METHOD = columns[4],
                                MASS_1_gram = columns[5],
                                CV = columns[6],
                                FUSE_LENGTH = columns[7],
                                ANALYSIS_DATE = columns[8],
                                VESSEL = columns[9],

                                CreatedOn = DateTime.Now,
                                CreatedBy = user.UserId
                            };

                            db.UPLOAD_LECO_CV.Add(data);
                            db.SaveChanges();

                            record_Count++;
                        }
                        catch (Exception ex)
                        {
                            record_Message = ex.Message;
                        }
                    }

                    var result = new { Success = true, Jumlah = (record_Count.ToString() + @"/" + line_Number.ToString()), Message = record_Message, Version = Configuration.VERSION };
                    return Json(result, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                var result = new { Success = false, ex.Message, Version = Configuration.VERSION };
                return Json(result, JsonRequestBehavior.AllowGet);
            }
        }

        public JsonResult TS(FormCollection p_collection)
        {
            // Check Permission based on Current Url
            UserX user = new UserX(Request);

            // -- Not necessary checking Permission
            //Permission.Check_API(Request, user, ref permission_Item);
            // -- just Logging User: is enough
            if (user.UserId <= 0)
            {
                var msg = new { Success = false, Message = Permission.ERROR_PERMISSION_READ + " [not Login]", MessageDetail = string.Empty, Version = Configuration.VERSION };
                return Json(msg, JsonRequestBehavior.AllowGet);
            }

            // -- Actual code
            try
            {
                using (MERCY_Ctx db = new MERCY_Ctx())
                {


                    string str = OurUtility.ValueOf(p_collection, "data");

                    string[] lines = str.Split(new[] { System.Environment.NewLine }, StringSplitOptions.None);
                    int line_Number = 0;

                    int record_Count = 0;
                    string record_Message = string.Empty;
                    string[] columns = null;
                    UPLOAD_LECO_TS data = null;

                    foreach (string line in lines)
                    {
                        line_Number++;

                        // Skip: "Header"
                        if (line_Number <= 1) continue;

                        try
                        {
                            // file .csv --> asumsi Separator adalah Comma
                            char csv_Separator = ',';

                            // check kembali: pakai Comma atau pakai TAB
                            if (!line.Contains(csv_Separator))
                            {
                                // berarti pakai TAB
                                csv_Separator = '\t';
                            }

                            // lakukan Splitting terhadap baris 
                            columns = line.Split(csv_Separator);

                            // Insert ke Table
                            data = new UPLOAD_LECO_TS
                            {
                                NO = columns[0],
                                Name = columns[1],
                                Description = columns[2],
                                Weight = columns[3],
                                Sulfur = columns[4],
                                Analysis_Time = columns[5],
                                Date = columns[6],
                                Method = columns[7],
                                Operator = columns[8],
                                Low_Sulfur = columns[9],
                                High_Sulfur = columns[10],

                                CreatedOn = DateTime.Now,
                                CreatedBy = user.UserId
                            };

                            db.UPLOAD_LECO_TS.Add(data);
                            db.SaveChanges();

                            record_Count++;
                        }
                        catch (Exception ex)
                        {
                            record_Message = ex.Message;
                        }
                    }

                    var result = new { Success = true, Jumlah = (record_Count.ToString() + @"/" + line_Number.ToString()), Message = record_Message, Version = Configuration.VERSION };
                    return Json(result, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                var result = new { Success = false, ex.Message, Version = Configuration.VERSION};
                return Json(result, JsonRequestBehavior.AllowGet);
            }
        }

        public JsonResult TGA(FormCollection p_collection)
        {
            // Check Permission based on Current Url
            UserX user = new UserX(Request);

            // -- Not necessary checking Permission
            //Permission.Check_API(Request, user, ref permission_Item);
            // -- just Logging User: is enough
            if (user.UserId <= 0)
            {
                var msg = new { Success = false, Message = Permission.ERROR_PERMISSION_READ + " [not Login]", MessageDetail = string.Empty, Version = Configuration.VERSION };
                return Json(msg, JsonRequestBehavior.AllowGet);
            }

            // -- Actual code
            try
            {
                using (MERCY_Ctx db = new MERCY_Ctx())
                {


                    string str = OurUtility.ValueOf(p_collection, "data");

                    string[] lines = str.Split(new[] { System.Environment.NewLine }, StringSplitOptions.None);
                    int line_Number = 0;

                    int record_Count = 0;
                    string record_Message = string.Empty;
                    string[] columns = null;
                    UPLOAD_LECO_TGA data = null;

                    foreach (string line in lines)
                    {
                        line_Number++;

                        // Skip: "Header"
                        if (line_Number <= 1) continue;

                        try
                        {
                            // file .csv --> asumsi Separator adalah Comma
                            char csv_Separator = ',';

                            // check kembali: pakai Comma atau pakai TAB
                            if (!line.Contains(csv_Separator))
                            {
                                // berarti pakai TAB
                                csv_Separator = '\t';
                            }

                            // lakukan Splitting terhadap baris 
                            columns = line.Split(csv_Separator);

                            // Insert ke Table
                            data = new UPLOAD_LECO_TGA
                            {
                                NAME = columns[1],
                                MOISTURE = columns[2],
                                VOLATILE = columns[3],
                                ASH = columns[4],
                                INITIAL_MASS = columns[5],
                                CRUCIBLE_MASS = columns[6],
                                BATCH = columns[7],
                                LOCATION = columns[8],
                                METHOD = columns[9],
                                ANALYSIS_DATE = columns[10],
                                MAISTURE_MASS = columns[11],
                                VOLATILE_MASS = columns[12],
                                ASH_MASS = columns[13],
                                FIXWD_CARBON = columns[14],
                                VOLATILE_DRY = columns[15],
                                ASH_DRY = columns[16],
                                FIXWD_CARBON_DRY = columns[17],

                                CreatedOn = DateTime.Now,
                                CreatedBy = user.UserId
                            };

                            db.UPLOAD_LECO_TGA.Add(data);
                            db.SaveChanges();

                            record_Count++;
                        }
                        catch (Exception ex)
                        {
                            record_Message = ex.Message;
                        }
                    }

                    var result = new { Success = true, Jumlah = (record_Count.ToString() + @"/" + line_Number.ToString()), Message = record_Message, Version = Configuration.VERSION };
                    return Json(result, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                var result = new { Success = false, ex.Message, Version = Configuration.VERSION };
                return Json(result, JsonRequestBehavior.AllowGet);
            }
        }
    }
}