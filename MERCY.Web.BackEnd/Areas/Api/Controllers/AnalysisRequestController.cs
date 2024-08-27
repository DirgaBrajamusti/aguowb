using System;
using System.Linq;
using System.Web.Mvc;

using System.Data;
using System.Data.SqlClient;

using MERCY.Data.EntityFramework;
using MERCY.Web.BackEnd.Models;
using MERCY.Web.BackEnd.Helpers;
using MERCY.Web.BackEnd.Security;
using Permission = MERCY.Web.BackEnd.Security.Permission;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace MERCY.Web.BackEnd.Areas.Api.Controllers
{
    public class AnalysisRequestController : Controller
    {
        public ActionResult Index2()
        {
            return View();
        }

        public JsonResult Index()
        {
            OurUtility.Set_Response_ServerMessage(Response);

            // Check Permission based on Current Url
            UserX user = new UserX(Request);
            Permission_Item permission_Item = null;

            Permission.Check_API(Request, user, ref permission_Item);
            if ( ! permission_Item.Is_View)
            {
                var msg = new { Success = false, Permission = permission_Item, Message = Permission.ERROR_PERMISSION_READ, MessageDetail = string.Empty, Version = Configuration.VERSION };
                return Json(msg, JsonRequestBehavior.AllowGet);
            }

            bool is_Show_User_Menu = OurUtility.ValueOf(Request, "u_menu").Equals("1");
            bool is_Show_User_Relation = OurUtility.ValueOf(Request, "u_relation").Equals("1");

            string company = OurUtility.ValueOf(Request, "c");
            string analysisType = OurUtility.ValueOf(Request, "ty");

            string dateFrom = OurUtility.ValueOf(Request, "dateFrom");
            string dateTo = OurUtility.ValueOf(Request, "dateTo");

            bool is_company_ALL = (string.IsNullOrEmpty(company) || company == "all");
            bool is_analysisType_ALL = (string.IsNullOrEmpty(analysisType) || analysisType == "all");
            
            try
            {
                DateTime dateFrom_O = DateTime.Now;
                DateTime dateTo_O = DateTime.Now.AddDays(1);

                try
                {
                    dateFrom_O = DateTime.Parse(dateFrom);
                }
                catch {}
                try
                {
                    dateTo_O = DateTime.Parse(dateTo).AddDays(1);
                }
                catch {}

                using (MERCY_Ctx db = new MERCY_Ctx())
                {
                    var dataQuery =
                            (
                                from dt in db.AnalysisRequests
                                join usr in db.Users on dt.CreatedBy equals usr.UserId
                                //orderby dt.Company, dt.SamplingType, dt.AnalysisRequestId descending
                                orderby dt.DeliveryDate descending
                                where (is_company_ALL || dt.Company == company)
                                        && (is_analysisType_ALL || dt.AnalysisType == analysisType)
                                        && dt.DeliveryDate >= dateFrom_O
                                        && dt.DeliveryDate < dateTo_O
                                select new Model_View_AnalysisRequest
                                {
                                    AnalysisRequestId = dt.AnalysisRequestId
                                    , Company = dt.Company
                                    , AnalysisType = dt.AnalysisType
                                    , Requestor = usr.FullName
                                    , Department = usr.Department
                                    , Email = usr.Email
                                    , Sender = dt.Sender
                                    , LetterNo = dt.LetterNo
                                    , CreatedBy_Str = usr.FullName
                                    , CreatedOn = dt.CreatedOn
                                    , DeliveryDate = dt.DeliveryDate
                                }
                            );


                    //var data = dataQuery.Skip(skip).Take(pageSize).ToList();
                    var items = dataQuery.ToList();

                    try
                    {
                        items.ForEach(c =>
                        {
                            c.DeliveryDate_Str = OurUtility.DateFormat(c.DeliveryDate, "dd-MMM-yyyy HH:mm");
                            c.DeliveryDate_Str2 = OurUtility.DateFormat(c.DeliveryDate, "dd-MMM-yyyy");

                            c.CreatedOn_Str = OurUtility.DateFormat(c.CreatedOn, "dd-MMM-yyyy HH:mm");
                        });
                    }
                    catch {}

                    var draw = OurUtility.ValueOf(Request, "draw");

                    var result = new { Success = true, Permission = permission_Item, User = UserX.Information(user, is_Show_User_Menu, is_Show_User_Relation), Message = string.Empty, MessageDetail = string.Empty, Version = Configuration.VERSION, Items = items, Total = items.Count, draw };
                    return Json(result, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                var result = new { Success = false, Permission = permission_Item, ex.Message, MessageDetail = string.Empty, Version = Configuration.VERSION, Items = string.Empty, Total = 0 };
                return Json(result, JsonRequestBehavior.AllowGet);
            }
        }

        public JsonResult Create(FormCollection p_collection)
        {
            // Check Permission based on Current Url
            UserX user = new UserX(Request);
            Permission_Item permission_Item = null;

            Permission.Check_API(Request, user, ref permission_Item);
            if ( ! permission_Item.Is_Add)
            {
                var msg = new { Success = false, Permission = permission_Item, Message = Permission.ERROR_PERMISSION_ADD, MessageDetail = string.Empty, Version = Configuration.VERSION };
                return Json(msg, JsonRequestBehavior.AllowGet);
            }

            try
            {
                string msg2 = string.Empty;
                string coverLetter = string.Empty;
                string coverLetter2 = string.Empty;

                OurUtility.Upload(Request, "file", UploadFolder, ref coverLetter, ref coverLetter2, ref msg2);
                AnalysisRequest data;
                using (MERCY_Ctx db = new MERCY_Ctx())
                {
                    var isAMD = OurUtility.ValueOf(p_collection, "analysisType") == "Acid Mine Drainage";
                    if (isAMD)
                    {
                        data = new AnalysisRequest
                        {
                            Company = OurUtility.ValueOf(p_collection, "company"),
                            AnalysisType = OurUtility.ValueOf(p_collection, "analysisType"),
                            DeliveryDate = DateTime.Parse(OurUtility.ValueOf(p_collection, "deliveryDate")),
                            Sender = OurUtility.ValueOf(p_collection, "sender"),
                            LetterNo = OurUtility.ValueOf(p_collection, "letterNo"),
                            Others = OurUtility.ValueOf(p_collection, "Others").Equals("1"),
                            Others_Text = OurUtility.ValueOf(p_collection, "Others_T"),
                            CoverLetter = coverLetter,
                            CoverLetter2 = coverLetter2,
                            EstimatedEndDate = null,
                            TS = OurUtility.ValueOf(p_collection, "TS").Equals("1"),
                            ANC = OurUtility.ValueOf(p_collection, "ANC").Equals("1"),
                            NAG = OurUtility.ValueOf(p_collection, "NAG").Equals("1"),

                            CreatedOn = DateTime.Now,
                            CreatedBy = user.UserId
                        };

                        db.AnalysisRequests.Add(data);
                        db.SaveChanges();
                    }
                    else
                    {
                        data = new AnalysisRequest
                        {
                            Company = OurUtility.ValueOf(p_collection, "company"),
                            AnalysisType = OurUtility.ValueOf(p_collection, "analysisType"),
                            DeliveryDate = DateTime.Parse(OurUtility.ValueOf(p_collection, "deliveryDate")),
                            Sender = OurUtility.ValueOf(p_collection, "sender"),
                            LetterNo = OurUtility.ValueOf(p_collection, "letterNo"),
                            TM = OurUtility.ValueOf(p_collection, "TM").Equals("1"),
                            PROX = OurUtility.ValueOf(p_collection, "PROX").Equals("1"),
                            TS = OurUtility.ValueOf(p_collection, "TS").Equals("1"),
                            CV = OurUtility.ValueOf(p_collection, "CV").Equals("1"),
                            RD = OurUtility.ValueOf(p_collection, "RD").Equals("1"),
                            CSN = OurUtility.ValueOf(p_collection, "CSN").Equals("1"),
                            AA = OurUtility.ValueOf(p_collection, "AA").Equals("1"),
                            HGI = OurUtility.ValueOf(p_collection, "HGI").Equals("1"),
                            Ultimate = OurUtility.ValueOf(p_collection, "Ultimate").Equals("1"),
                            Chlorine = OurUtility.ValueOf(p_collection, "Chlorine").Equals("1"),
                            Phosporus = OurUtility.ValueOf(p_collection, "Phosporus").Equals("1"),
                            Fluorine = OurUtility.ValueOf(p_collection, "Fluorine").Equals("1"),
                            Lead = OurUtility.ValueOf(p_collection, "Lead").Equals("1"),
                            Zink = OurUtility.ValueOf(p_collection, "Zink").Equals("1"),
                            AFT = OurUtility.ValueOf(p_collection, "AFT").Equals("1"),
                            TraceElement = OurUtility.ValueOf(p_collection, "TraceElement").Equals("1"),
                            Others = OurUtility.ValueOf(p_collection, "Others").Equals("1"),
                            Others_Text = OurUtility.ValueOf(p_collection, "Others_T"),
                            CoverLetter = coverLetter,
                            CoverLetter2 = coverLetter2,
                            EstimatedEndDate = null,

                            CreatedOn = DateTime.Now,
                            CreatedBy = user.UserId
                        };

                        db.AnalysisRequests.Add(data);
                        db.SaveChanges();
                    }

                    long analysisId = data.AnalysisRequestId;

                    var dataH = new AnalysisRequest_History
                    {
                        CreatedOn = DateTime.Now,
                        CreatedBy = user.UserId,
                        AnalysisRequestId = analysisId,
                        Description = "Submit"
                    };
                    db.AnalysisRequest_History.Add(dataH);
                    db.SaveChanges();

                    int recordNumber = OurUtility.ToInt32(Request["recordNumber"]);
                    string labId = string.Empty;

                    string labIds = string.Empty;
                    string separator = string.Empty;
                 
                    for (int i=1;i<= recordNumber; i++)
                    {
                        try
                        {
                            string x = OurUtility.ValueOf(p_collection, "sample" + i.ToString());
                            if (string.IsNullOrEmpty(x)) continue;
                            if (x == "undefined") continue;

                            labId = OurUtility.ValueOf(p_collection, "labId" + i.ToString());
                            if (labId == "undefined")
                            {
                                labId = string.Empty;
                            }

                            var sample = new AnalysisRequest_Detail
                            {
                                AnalysisRequest = analysisId,
                                SampleID = x,
                                From = OurUtility.ToDecimal(OurUtility.ValueOf(p_collection, "from" + i.ToString())),
                                To = OurUtility.ToDecimal(OurUtility.ValueOf(p_collection, "to" + i.ToString())),
                                Thick = OurUtility.ValueOf(p_collection, "thick" + i.ToString()),
                                SampleType = OurUtility.ValueOf(p_collection, "sampleType" + i.ToString()),
                                SEAM = OurUtility.ValueOf(p_collection, "seam" + i.ToString()),
                                LabId = labId,
                                RockType = isAMD ? OurUtility.ValueOf(p_collection, "rockType" + i.ToString()) : null,

                                Verification = OurUtility.ValueOf(p_collection, "verification" + i.ToString()).Equals("1"),
                                Verification_Comment = OurUtility.ValueOf(p_collection, "comment" + i.ToString()),

                                CreatedOn = DateTime.Now,
                                CreatedBy = user.UserId
                            };
                            if (isAMD)
                            {
                                sample.TS = OurUtility.ToDecimal(OurUtility.ValueOf(p_collection, "TS" + i.ToString()));
                                sample.MPA = OurUtility.ToDecimal(OurUtility.ValueOf(p_collection, "MPA" + i.ToString()));
                                sample.PH = OurUtility.ToDecimal(OurUtility.ValueOf(p_collection, "PH" + i.ToString()));
                                sample.ANC = OurUtility.ToDecimal(OurUtility.ValueOf(p_collection, "ANC" + i.ToString()));
                                sample.NAPP = OurUtility.ToDecimal(OurUtility.ValueOf(p_collection, "NAPP" + i.ToString()));
                                sample.NAG = OurUtility.ToDecimal(OurUtility.ValueOf(p_collection, "NAG" + i.ToString()));
                            }
                            sample.CreatedOn_Date_Only = sample.CreatedOn.ToString("yyyy-MM-dd");
                            sample.CreatedOn_Year_Only = sample.CreatedOn.Year;
                            sample.Company = data.Company;

                            db.AnalysisRequest_Detail.Add(sample);
                            db.SaveChanges();


                            // for Data History: only Add if necessary
                            if ( ! string.IsNullOrEmpty(sample.LabId))
                            {
                                labIds += separator + sample.LabId;
                                separator = ",";
                            }
                           
                        }
                        catch {}
                    }
                    // for Data History: only Add if necessary
                    if (!string.IsNullOrEmpty(labIds))
                    {
                        var dataH2 = new AnalysisRequest_History();
                        dataH.CreatedOn = DateTime.Now;
                        dataH.CreatedBy = user.UserId;
                        dataH.AnalysisRequestId = analysisId;
                        dataH.Description = "Update Lab ID: " + labIds;
                        db.AnalysisRequest_History.Add(dataH);
                        db.SaveChanges();
                    }

                    var result = new { Success = true, Permission = permission_Item, Message = BaseConstants.MESSAGE_CREATE_SUCCESS, MessageDetail = string.Empty, Version = Configuration.VERSION, Data = analysisId };
                    return Json(result, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                var result = new { Success = false, Permission = permission_Item, ex.Message, MessageDetail = string.Empty, Version = Configuration.VERSION, Items = string.Empty, Total = 0 };
                return Json(result, JsonRequestBehavior.AllowGet);
            }
        }
        
        [HttpGet]
        public JsonResult Get()
        {
            // Check Permission based on Current Url
            UserX user = new UserX(Request);
            Permission_Item permission_Item = null;

            Permission.Check_API(Request, user, ref permission_Item);
            if ( ! permission_Item.Is_View)
            {
                var msg = new { Success = false, Permission = permission_Item, Message = Permission.ERROR_PERMISSION_READ, MessageDetail = string.Empty, Version = Configuration.VERSION };
                return Json(msg, JsonRequestBehavior.AllowGet);
            }

            bool is_Show_User_Menu = OurUtility.ValueOf(Request, "u_menu").Equals("1");
            bool is_Show_User_Relation = OurUtility.ValueOf(Request, "u_relation").Equals("1");

            long id = OurUtility.ToInt64(Request[".id"]);
            if (id <= 0)
            {
                // -- special purpose
                // this Id is only for Checking CurrentUser:Info\

                var result = new { Success = true, Permission = permission_Item, User = UserX.Information(user, is_Show_User_Menu, is_Show_User_Relation), Version = Configuration.VERSION };
                return Json(result, JsonRequestBehavior.AllowGet);
            }

            try
            {
                using (MERCY_Ctx db = new MERCY_Ctx())
                {
                    var dataQuery =
                            (
                                from dt in db.AnalysisRequests
                                join usr in db.Users on dt.CreatedBy equals usr.UserId
                                where dt.AnalysisRequestId == id
                                select new Model_View_AnalysisRequest
                                {
                                    AnalysisRequestId = dt.AnalysisRequestId
                                    , Company = dt.Company
                                    , AnalysisType = dt.AnalysisType
                                    , Requestor = usr.FullName
                                    , Department = usr.Department
                                    , Email = usr.Email
                                    , Sender = dt.Sender
                                    , LetterNo = dt.LetterNo
                                    , CreatedBy_Str = usr.FullName
                                    , CreatedOn = dt.CreatedOn
                                    , DeliveryDate = dt.DeliveryDate
                                    , TM = dt.TM
                                    , PROX = dt.PROX
                                    , TS = dt.TS
                                    , CV = dt.CV 
                                    , RD = dt.RD 
                                    , CSN = dt.CSN 
                                    , AA = dt.AA 
                                    , HGI = dt.HGI 
                                    , Ultimate = dt.Ultimate 
                                    , Chlorine = dt.Chlorine 
                                    , Phosporus = dt.Phosporus 
                                    , Fluorine = dt.Fluorine 
                                    , Lead = dt.Lead 
                                    , Zink = dt.Zink 
                                    , AFT = dt.AFT
                                    , TraceElement = dt.TraceElement 
                                    , Others = dt.Others 
                                    , Others_Text = dt.Others_Text
                                    , Moisture = dt.Moisture
                                    , DensityDry = dt.DensityDry
                                    , DensityWet = dt.DensityWet
                                    , UCS = dt.UCS
                                    , DST = dt.DST
                                    , GPa = dt.GPa
                                    , PoissionRatio = dt.PoissionRatio
                                    , KPa = dt.KPa
                                    , CUD = dt.CUD
                                    , EstimatedEndDate = dt.EstimatedEndDate
                                    , CoverLetter = dt.CoverLetter
                                    , CoverLetter2 = dt.CoverLetter2
                                }
                            );
                            
                    var data = dataQuery.ToList();
                    if (data == null)
                    {
                        var result_NotFound = new { Success = false, Permission = permission_Item, User = UserX.Information(user, is_Show_User_Menu, is_Show_User_Relation), Message = "Id: " + id.ToString() + " is not found", MessageDetail = string.Empty, Version = Configuration.VERSION};
                        return Json(result_NotFound, JsonRequestBehavior.AllowGet);
                    }
                    
                    try
                    {
                        data.ForEach(c =>
                        {
                            c.DeliveryDate_Str = OurUtility.DateFormat(c.DeliveryDate, "dd-MMM-yyyy HH:mm");
                            c.DeliveryDate_Str2 = OurUtility.DateFormat(c.DeliveryDate, "MM/dd/yyyy");

                            c.CreatedOn_Str = OurUtility.DateFormat(c.CreatedOn, "dd-MMM-yyyy HH:mm");
                            
                            c.EstimatedEndDate_Str = c.EstimatedEndDate != null ? OurUtility.DateFormat(c.EstimatedEndDate, "dd-MMM-yyyy HH:mm") : null;
                            c.EstimatedEndDate_Str2 = c.EstimatedEndDate != null ? OurUtility.DateFormat(c.EstimatedEndDate, "MM/dd/yyyy") : null;
                        });
                    }
                    catch {}
                    
                    var dataQuery_SamplesId =
                            (
                                from dt in db.AnalysisRequest_Detail
                                where dt.AnalysisRequest == id
                                select new Model_View_AnalysisRequest_Detail
                                {
                                    RecordId = dt.RecordId
                                    , SampleID = dt.SampleID
                                    , From = dt.From
                                    , To = dt.To
                                    , From2 = dt.From
                                    , To2 = dt.To
                                    , Thick = dt.Thick
                                    , SampleType = dt.SampleType
                                    , SEAM = dt.SEAM
                                    , LabId = dt.LabId
                                    , CreatedOn = dt.CreatedOn
                                    , LastModifiedOn = dt.LastModifiedOn
                                    , Verification = dt.Verification
                                    , Verification_Comment = dt.Verification_Comment
                                    , ReTest = dt.ReTest
                                    , RockType = dt.RockType
                                }
                            );
                            
                    var data_SamplesId = dataQuery_SamplesId.ToList();
                    try
                    {
                        data_SamplesId.ForEach(c =>
                        {
                            c.CreatedOn_Str = OurUtility.DateFormat(c.CreatedOn, "dd-MMM-yyyy - HH:mm");
                            c.CreatedOn_Str2 = OurUtility.DateFormat(c.CreatedOn, "dd-MMM-yyyy");

                            if (c.LastModifiedOn != null)
                            {
                                c.CreatedOn_Str = OurUtility.DateFormat(c.LastModifiedOn, "dd-MMM-yyyy - HH:mm");
                            }
                        });
                    }
                    catch {}

                    var dataQuery_History =
                                (
                                    from d in db.AnalysisRequest_History
                                    join u in db.Users on d.CreatedBy equals u.UserId
                                    where d.AnalysisRequestId == id
                                    orderby d.RecordId descending
                                    select new Model_View_AnalysisRequest_History
                                    {
                                        CreatedOn = d.CreatedOn
                                        , CreatedBy = d.CreatedBy
                                        , FullName = u.FullName
                                        , Description = d.Description
                                    }
                                );

                    var data_History = dataQuery_History.ToList();
                    try
                    {
                        data_History.ForEach(c =>
                        {
                            c.CreatedOn_Str = OurUtility.DateFormat(c.CreatedOn, @"dd/MM/yyyy HH:mm:ss");
                        });
                    }
                    catch {}

                    var data_Discussions = DiscussionController.Get("AnalysisRequest", id);

                    return Json(new { data, Success = true, Permission = permission_Item, User = UserX.Information(user, is_Show_User_Menu, is_Show_User_Relation), SamplesId = data_SamplesId, Histories = data_History, Discussions = data_Discussions }, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                var result = new { Success = false, Permission = permission_Item, ex.Message, MessageDetail = string.Empty, Version = Configuration.VERSION, Items = string.Empty, Total = 0 };
                return Json(result, JsonRequestBehavior.AllowGet);
            }
        }

        public JsonResult Edit(FormCollection p_collection)
        {
            // Check Permission based on Current Url
            UserX user = new UserX(Request);
            Permission_Item permission_Item = null;

            Permission.Check_API(Request, user, ref permission_Item);
            if ( ! permission_Item.Is_Edit)
            {
                var msg = new { Success = false, Permission = permission_Item, Message = Permission.ERROR_PERMISSION_EDIT, MessageDetail = string.Empty, Version = Configuration.VERSION };
                return Json(msg, JsonRequestBehavior.AllowGet);
            }

            // -- Actual code
            long id = OurUtility.ToInt64(Request[".id"]);
            using (MERCY_Ctx db = new MERCY_Ctx())
            {
                var transaction = db.Database.BeginTransaction();
                try
                {
                    string msg2 = string.Empty;
                    string coverLetter = string.Empty;
                    string coverLetter2 = string.Empty;

                    var isUploaded = OurUtility.Upload(Request, "file", UploadFolder, ref coverLetter, ref coverLetter2, ref msg2);

                    var isGeoTechnical = OurUtility.ValueOf(p_collection, "analysisType") == "Geo Technical" ? true : false;
                    
                    var dataQuery =
                        (
                            from d in db.AnalysisRequests
                            where d.AnalysisRequestId == id
                            select d
                        );

                    var data = dataQuery.SingleOrDefault();

                    data.Company = OurUtility.ValueOf(p_collection, "company");
                    data.AnalysisType = OurUtility.ValueOf(p_collection, "analysisType");
                    data.DeliveryDate = DateTime.Parse(OurUtility.ValueOf(p_collection, "deliveryDate"));
                    data.Sender = OurUtility.ValueOf(p_collection, "sender");
                    data.LetterNo = OurUtility.ValueOf(p_collection, "letterNo");

                    if (isGeoTechnical)
                    {
                        data.Moisture = OurUtility.ValueOf(p_collection, "Moisture").Equals("1");
                        data.DensityDry = OurUtility.ValueOf(p_collection, "DensityDry").Equals("1");
                        data.DensityWet = OurUtility.ValueOf(p_collection, "DensityWet").Equals("1");
                        data.UCS = OurUtility.ValueOf(p_collection, "UCS").Equals("1");
                        data.DST = OurUtility.ValueOf(p_collection, "DST").Equals("1");
                        data.GPa = OurUtility.ValueOf(p_collection, "GPa").Equals("1");
                        data.PoissionRatio = OurUtility.ValueOf(p_collection, "PoissionRatio").Equals("1");
                        data.KPa = OurUtility.ValueOf(p_collection, "KPa").Equals("1");
                        data.CUD = OurUtility.ValueOf(p_collection, "CUD").Equals("1");
                        data.EstimatedEndDate = DateTime.Parse(OurUtility.ValueOf(p_collection, "estimatedEndDate"));
                    }
                    else
                    {
                        data.TM = OurUtility.ValueOf(p_collection, "TM").Equals("1");
                        data.PROX = OurUtility.ValueOf(p_collection, "PROX").Equals("1");
                        data.TS = OurUtility.ValueOf(p_collection, "TS").Equals("1");
                        data.CV = OurUtility.ValueOf(p_collection, "CV").Equals("1");
                        data.RD = OurUtility.ValueOf(p_collection, "RD").Equals("1");
                        data.CSN = OurUtility.ValueOf(p_collection, "CSN").Equals("1");
                        data.AA = OurUtility.ValueOf(p_collection, "AA").Equals("1");
                        data.HGI = OurUtility.ValueOf(p_collection, "HGI").Equals("1");
                        data.Ultimate = OurUtility.ValueOf(p_collection, "Ultimate").Equals("1");
                        data.Chlorine = OurUtility.ValueOf(p_collection, "Chlorine").Equals("1");
                        data.Phosporus = OurUtility.ValueOf(p_collection, "Phosporus").Equals("1");
                        data.Fluorine = OurUtility.ValueOf(p_collection, "Fluorine").Equals("1");
                        data.Lead = OurUtility.ValueOf(p_collection, "Lead").Equals("1");
                        data.Zink = OurUtility.ValueOf(p_collection, "Zink").Equals("1");
                        data.AFT = OurUtility.ValueOf(p_collection, "AFT").Equals("1");
                        data.TraceElement = OurUtility.ValueOf(p_collection, "TraceElement").Equals("1");
                    }

                    data.Others = OurUtility.ValueOf(p_collection, "Others").Equals("1");
                    data.Others_Text = OurUtility.ValueOf(p_collection, "Others_T");
                    if (isUploaded)
                    {

                        data.CoverLetter = coverLetter;
                        data.CoverLetter2 = coverLetter2;
                    }

                    data.LastModifiedOn = DateTime.Now;
                    data.LastModifiedBy = user.UserId;

                    db.SaveChanges();

                    long analysisId = data.AnalysisRequestId;
                    int recordNumber = OurUtility.ToInt32(Request["recordNumber"]);
                    string labId = string.Empty;

                    string labIds = string.Empty;
                    string separator = string.Empty;

                    long recordId = 0;
                    //SampleRegistrationModelView
                    List<SampleModelView> samples = new List<SampleModelView>();
                    for (int i = 1; i <= recordNumber; i++)
                    {
                        try
                        {
                            string x = OurUtility.ValueOf(p_collection, "sample" + i.ToString());
                            if (string.IsNullOrEmpty(x)) continue;
                            if (x == "undefined") continue;

                            labId = OurUtility.ValueOf(p_collection, "labId" + i.ToString());
                            if (labId == "undefined")
                            {
                                labId = string.Empty;
                            }

                            recordId = OurUtility.ToInt64(OurUtility.ValueOf(p_collection, "sampleIdx_" + i.ToString()));

                            // Check data from Table Detail
                            var dataQueryx =
                            (
                                from d in db.AnalysisRequest_Detail
                                where d.RecordId == recordId
                                select d
                            );

                            var sample = dataQueryx.SingleOrDefault();
                            if (sample == null)
                            {
                                // -- Mode ADD
                                sample = new AnalysisRequest_Detail
                                {
                                    AnalysisRequest = analysisId,
                                    SampleID = x,
                                    From = OurUtility.ToDecimal(OurUtility.ValueOf(p_collection, "from" + i.ToString())),
                                    To = OurUtility.ToDecimal(OurUtility.ValueOf(p_collection, "to" + i.ToString())),
                                    Thick = OurUtility.ValueOf(p_collection, "thick" + i.ToString()),
                                    SampleType = OurUtility.ValueOf(p_collection, "sampleType" + i.ToString()),
                                    SEAM = OurUtility.ValueOf(p_collection, "seam" + i.ToString()),
                                    LabId = labId,
                                    RockType = isGeoTechnical ? OurUtility.ValueOf(p_collection, "rockType" + i.ToString()) : null,

                                    Verification = OurUtility.ValueOf(p_collection, "verification" + i.ToString()).Equals("1"),
                                    Verification_Comment = OurUtility.ValueOf(p_collection, "comment" + i.ToString()),

                                    CreatedOn = DateTime.Now,
                                    CreatedBy = user.UserId
                                };
                                sample.CreatedOn_Date_Only = sample.CreatedOn.ToString("yyyy-MM-dd");
                                sample.CreatedOn_Year_Only = sample.CreatedOn.Year;
                                sample.Company = data.Company;

                                db.AnalysisRequest_Detail.Add(sample);
                                db.SaveChanges();

                                // for Data History: only Add if necessary
                                if (!string.IsNullOrEmpty(labId))
                                {
                                    labIds += separator + labId;
                                    separator = ",";
                                }
                            }
                            else
                            {
                                // -- Mode Edit

                                // jika salah satu Nilai berubah
                                // baru dilakukan Save - Changed
                                if ((sample.AnalysisRequest != analysisId)
                                    || (sample.SampleID != x)
                                    || (sample.From != OurUtility.ToInt32(OurUtility.ValueOf(p_collection, "from" + i.ToString())))
                                    || (sample.To != OurUtility.ToInt32(OurUtility.ValueOf(p_collection, "to" + i.ToString())))
                                    || (sample.Thick != OurUtility.ValueOf(p_collection, "thick" + i.ToString()))
                                    || (sample.SampleType != OurUtility.ValueOf(p_collection, "sampleType" + i.ToString()))
                                    || (sample.SEAM != OurUtility.ValueOf(p_collection, "seam" + i.ToString()))
                                    || (sample.LabId != labId)
                                    || (sample.Verification != OurUtility.ValueOf(p_collection, "verification" + i.ToString()).Equals("1"))
                                    || (sample.Verification_Comment != OurUtility.ValueOf(p_collection, "comment" + i.ToString()))
                                    || (isGeoTechnical && sample.RockType != OurUtility.ValueOf(p_collection, "rockType" + i.ToString()))
                                    )
                                {
                                    // for Data History: only Add if necessary
                                    if (!string.IsNullOrEmpty(labId))
                                    {
                                        if (sample.LabId != labId)
                                        {
                                            labIds += separator + labId;
                                            separator = ",";
                                        }
                                    }

                                    sample.AnalysisRequest = analysisId;
                                    sample.SampleID = x;
                                    sample.From = OurUtility.ToDecimal(OurUtility.ValueOf(p_collection, "from" + i.ToString()));
                                    sample.To = OurUtility.ToDecimal(OurUtility.ValueOf(p_collection, "to" + i.ToString()));
                                    sample.Thick = OurUtility.ValueOf(p_collection, "thick" + i.ToString());
                                    sample.SampleType = OurUtility.ValueOf(p_collection, "sampleType" + i.ToString());
                                    sample.SEAM = OurUtility.ValueOf(p_collection, "seam" + i.ToString());
                                    sample.LabId = labId;
                                    sample.RockType = isGeoTechnical ? OurUtility.ValueOf(p_collection, "rockType" + i.ToString()) : null;

                                    sample.Verification = OurUtility.ValueOf(p_collection, "verification" + i.ToString()).Equals("1");
                                    sample.Verification_Comment = OurUtility.ValueOf(p_collection, "comment" + i.ToString());

                                    sample.LastModifiedOn = DateTime.Now;
                                    sample.LastModifiedBy = user.UserId;

                                    db.SaveChanges();
                                }
                            }

                            //Initiate Datas For Generate SampleRegistrations
                            if (!string.IsNullOrEmpty(labId))
                            {
                                int projectId = 0;
                                if (data.AnalysisType.Contains("Exploration"))
                                {
                                    projectId = GetProjectId("Exploration", db);
                                }
                                else
                                {
                                    projectId = GetProjectId("Pit Mon", db);
                                }
                                int refId = GetRefTypeId("ASTM", db);
                                int clientId = GetClientId("Geology", db);
                                int siteId = GetSiteId("Melak", db);

                                samples.Add(new SampleModelView
                                {
                                    CompanyCode = data.Company,
                                    DateOfJob = DateTime.Now,
                                    ThicknessFrom = sample.From,
                                    ThicknessTo = sample.To,
                                    ProjectId = projectId,
                                    ClientId = clientId,
                                    RefTypeId = refId,
                                    SiteId = siteId,
                                    ReceivedBy = "",
                                    Location = "",
                                    Remark = "",
                                    DetailGeneral = new SampleDetailGeneralModelView
                                    {
                                        GeoPrefix = "",
                                        SampleId = labId + " - " + sample.SampleID,
                                        DateSampleEnd = DateTime.Now,
                                        DateSampleStart = DateTime.Now,
                                        Receive = DateTime.Parse(data.DeliveryDate.ToString("yyyy-MM-hh") + " 07:00:00"),
                                        Shift = 1,
                                        Tonnage = 0,
                                        Sequence = 1,
                                        Destination = "",
                                        BargeName = "",
                                        TripNumber = ""
                                    },
                                    DetailLoading = null,
                                    Schemes = GetSchemesId(data.Company, clientId, projectId, db)
                                });
                            }

                        }
                        catch { }
                    }

                    // for Data History: only Add if necessary
                    if (!string.IsNullOrEmpty(labIds))
                    {
                        var dataH = new AnalysisRequest_History
                        {
                            CreatedOn = DateTime.Now,
                            CreatedBy = user.UserId,
                            AnalysisRequestId = id,
                            Description = "Update Lab ID: " + labIds
                        };
                        db.AnalysisRequest_History.Add(dataH);
                        db.SaveChanges();

                    }

                    if (!CreateSample(db, samples, user, id))
                    {
                        throw new Exception("Fail Generate Sample");
                    }
                    transaction.Commit();
                    var result = new { Success = true, Permission = permission_Item, Message = BaseConstants.MESSAGE_UPDATE_SUCCESS, MessageDetail = string.Empty, Version = Configuration.VERSION, Data = id };
                    return Json(result, JsonRequestBehavior.AllowGet);

                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    transaction.Dispose();
                    var result = new { Success = false, Permission = permission_Item, ex.Message, MessageDetail = string.Empty, Version = Configuration.VERSION, Items = string.Empty, Total = 0 };
                    return Json(result, JsonRequestBehavior.AllowGet);
                }
            }
        }

        public ActionResult CheckReport()
        {
            // Check Permission based on Current Url
            UserX user = new UserX(Request);
            Permission_Item permission_Item = null;

            // -- Not necessary checking Permission
            //Permission.Check_API(Request, user, ref permission_Item);
            // -- just Logging User: is enough
            if (user.UserId <= 0)
            {
                var msg = new { Success = false, Permission = permission_Item, Message = Permission.ERROR_PERMISSION_READ + " [not Login]", MessageDetail = string.Empty, Version = Configuration.VERSION };
                return Json(msg, JsonRequestBehavior.AllowGet);
            }

            bool success = false;
            string message = string.Empty;
            int count = 0;

            string id = Request[".id"];
            string analysisType = Request["analysisType"];

            try
            {
                using (MERCY_Ctx db = new MERCY_Ctx())
                {
                    using (var connection = new SqlConnection(db.Database.Connection.ConnectionString))
                    {
                        connection.Open();

                        SqlCommand command = connection.CreateCommand();

                        string sql = string.Format(@"exec Report_Form_Analysis_Exploration_Check {0}", id);
                        switch (analysisType)
                        {
                            case "Geo Exploration":
                                sql = string.Format(@"exec Report_Form_Analysis_Exploration_Check {0}", id);
                                break;
                            case "Geo Pitmon":
                                sql = string.Format(@"exec Report_Form_Analysis_Pit_Check {0}", id);
                                break;
                        }

                        command.CommandText = sql;

                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                count = OurUtility.ToInt32(reader["Count2"].ToString());

                                success = true;
                                message = string.Empty;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                message = ex.Message;
            }

            var result = new { Success = success, Permission = permission_Item, Count = count, Message = message, Time = DateTime.Now.ToString("dd-MMM-yyyy HH:mm:ss.ffff"), Version = Configuration.VERSION };
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        private string UploadFolder
        {
            get
            {
                string result = @"c:\temp";

                try
                {
                    result = Server.MapPath("~") + @"\" + Configuration.FolderUpload + @"\";
                    result = result.Replace(@"\\", @"\");
                }
                catch {}

                return result;
            }
        }

        public JsonResult ReTest(FormCollection p_collection)
        {
            // Check Permission based on Current Url
            UserX user = new UserX(Request);
            Permission_Item permission_Item = null;

            Permission.Check_API(Request, user, ref permission_Item);
            if ( ! permission_Item.Is_Add)
            {
                var msg = new { Success = false, Permission = permission_Item, Message = Permission.ERROR_PERMISSION_ADD, MessageDetail = string.Empty, Version = Configuration.VERSION };
                return Json(msg, JsonRequestBehavior.AllowGet);
            }

            // -- Actual code
            long analysisId = OurUtility.ToInt64(Request[".id"]);

            try
            {
                /*string msg2 = string.Empty;
                string coverLetter = string.Empty;
                string coverLetter2 = string.Empty;

                OurUtility.Upload(Request, "file", UploadFolder, ref coverLetter, ref coverLetter2, ref msg2);*/

                using (MERCY_Ctx db = new MERCY_Ctx())
                {
                    int recordNumber = OurUtility.ToInt32(Request["recordNumber"]);
                    string labId = string.Empty;

                    string labIds = string.Empty;
                    string separator = string.Empty;

                    long recordId = 0;

                    for (int i = 1; i <= recordNumber; i++)
                    {
                        try
                        {
                            labId = OurUtility.ValueOf(p_collection, "labId" + i.ToString());
                            if (string.IsNullOrEmpty(labId)) continue;
                            if (labId == "undefined") continue;

                            recordId = OurUtility.ToInt64(OurUtility.ValueOf(p_collection, "sampleIdx_" + i.ToString()));

                            // Check data from Table Detail
                            var dataQueryx =
                            (
                                from d in db.AnalysisRequest_Detail
                                where d.RecordId == recordId
                                select d
                            );

                            var sample = dataQueryx.SingleOrDefault();
                            if (sample == null) continue;

                            sample.ReTest = OurUtility.ValueOf(p_collection, "labId_retest" + i.ToString()).Equals("1");

                            sample.LastModifiedOn = DateTime.Now;
                            sample.LastModifiedBy = user.UserId;

                            db.SaveChanges();

                            if (sample.ReTest)
                            {
                                labIds += separator + labId;
                                separator = ",";
                            }
                        }
                        catch {}
                    }

                    // for Data History: only Add if necessary
                    if ( ! string.IsNullOrEmpty(labIds))
                    {
                        var dataH = new AnalysisRequest_History
                        {
                            CreatedOn = DateTime.Now,
                            CreatedBy = user.UserId,
                            AnalysisRequestId = analysisId,
                            Description = "Request retest: " + labIds
                        };
                        db.AnalysisRequest_History.Add(dataH);
                        db.SaveChanges();
                    }

                    var result = new { Success = true, Permission = permission_Item, Message = BaseConstants.MESSAGE_UPDATE_SUCCESS, MessageDetail = string.Empty, Version = Configuration.VERSION, Data = analysisId };
                    return Json(result, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                var result = new { Success = false, Permission = permission_Item, ex.Message, MessageDetail = string.Empty, Version = Configuration.VERSION, Items = string.Empty, Total = 0 };
                return Json(result, JsonRequestBehavior.AllowGet);
            }
        }

        private bool CreateSample(MERCY_Ctx db, List<SampleModelView> samples, UserX user, long analystId)
        {

            try
            {
                var datas = new List<SampleListModelView>();
                var validateSample = samples.Where(s => s.DetailGeneral != null).FirstOrDefault();

                if (validateSample != null)
                {
                    SampleValidateModelView val = new SampleValidateModelView
                    {
                        SampleId = validateSample.DetailGeneral.SampleId,
                        Shift = validateSample.DetailGeneral.Shift,
                        ClientId = validateSample.ClientId,
                        ProjectId = validateSample.ProjectId,
                        DateOfJob = validateSample.DateOfJob
                    };
                    if (!ValidateSample(val))
                    {
                        throw new Exception("SampleId, Shift, DateOfJob, ClientId, And ProjectId Was Registered.");
                    }
                }

                foreach (var sample in samples)
                {
                    int last = 1;
                    decimal thickness = 0;
                    bool isDetailGeneral = sample.DetailGeneral != null;
                    string codeProject = db.Projects.Where(p => p.Id == sample.ProjectId).Select(p => p.Code).FirstOrDefault();
                    var existingExternalId =
                        (
                            from s in db.Samples
                            where s.ClientId == sample.ClientId
                                && s.ProjectId == sample.ProjectId
                                && s.CompanyCode == sample.CompanyCode
                            orderby s.ExternalId.Length descending, s.ExternalId descending
                            select s.ExternalId
                        ).FirstOrDefault();
                    string externalId = string.Empty;
                    DateTime receivedDate = DateTime.Now;

                    if (existingExternalId != null)
                    {
                        last = OurUtility.ToInt32(existingExternalId.Substring(codeProject.Length)) + 1;
                    }

                    receivedDate = sample.DetailGeneral.Receive;
                    externalId = codeProject + last.ToString();


                    // Generate temporary analyst job model 
                    var analystTemp = new AnalystJobModel
                    {
                        CompanyCode = sample.CompanyCode,
                        JobNumber = codeProject,
                        Status = "register",
                        JobDate = sample.DateOfJob,
                        ReceivedDate = receivedDate,
                        ProjectId = sample.ProjectId
                    };
                    if (sample.ThicknessTo != 0 && sample.ThicknessFrom != 0)
                    {
                        thickness = (sample.ThicknessTo - sample.ThicknessFrom) ?? 0;
                    }

                    var data = new Sample
                    {
                        ClientId = sample.ClientId,
                        SiteId = sample.SiteId,
                        CompanyCode = sample.CompanyCode,
                        DateOfJob = sample.DateOfJob,
                        ProjectId = sample.ProjectId,
                        RefTypeId = sample.RefTypeId,
                        ReceivedBy = sample.ReceivedBy,
                        CreatedOn = DateTime.Now,
                        CreatedBy = user.UserId,
                        ExternalId = externalId,
                        Remark = sample.Remark,
                        Location = sample.Location,
                        ThicknessTo = sample.ThicknessTo,
                        ThicknessFrom = sample.ThicknessFrom,
                        Thickness = thickness,
                        IsActive = true,
                        Stage = "register",
                        JobNumberId = GetJobNumberId(analystTemp, user, db)
                    };

                    db.Samples.Add(data);
                    db.SaveChanges();

                    // Generate sample scheme data
                    var sampleSchemes = new List<SampleScheme>();
                    foreach (var sampleSchemeItem in sample.Schemes)
                    {
                        var sampleScheme = new SampleScheme
                        {
                            SampleId = data.Id,
                            SchemeId = sampleSchemeItem.SchemeId,
                            CreatedBy = user.UserId,
                            CreatedOn = DateTime.Now
                        };
                        db.SampleSchemes.Add(sampleScheme);
                        db.SaveChanges();
                        if (!StoreAnalysisResult(db, sampleScheme.SchemeId, data.Id, sample.CompanyCode, user))
                        {
                            throw new Exception("Failed to Store Analyst Result ");
                        }
                    }


                    SampleDetailGeneral sampleDetailGeneral = new SampleDetailGeneral
                    {
                        GeoPrefix = sample.DetailGeneral.GeoPrefix,
                        Shift = sample.DetailGeneral.Shift,
                        Sequence = sample.DetailGeneral.Sequence,
                        DateSampleStart = sample.DetailGeneral.DateSampleStart,
                        DateSampleEnd = sample.DetailGeneral.DateSampleEnd,
                        Receive = sample.DetailGeneral.Receive,
                        Tonnage = sample.DetailGeneral.Tonnage,
                        Sample_Id = data.Id,
                        SampleId = sample.DetailGeneral.SampleId,
                        BargeName = sample.DetailGeneral.BargeName,
                        Destination = sample.DetailGeneral.Destination,
                        TripNumber = sample.DetailGeneral.TripNumber,
                        CreatedBy = user.UserId,
                        CreatedOn = DateTime.Now
                    };

                    db.SampleDetailGenerals.Add(sampleDetailGeneral);
                    db.SaveChanges();

                    if (!StoreSampleAnalystRequest(db, data.Id, analystId, user))
                    {
                        throw new Exception("Fail Store Relation AnalystRequest and Sample");
                    }

                }

                return true;
            }
            catch
            {

                return false;
            }
            
        }
        private bool StoreSampleAnalystRequest(MERCY_Ctx db, int sampleId, long analystId, UserX user)
        {
            try
            {
              
                AnalysisRequestSample temp = new AnalysisRequestSample
                {
                    AnalysisRequestId = analystId,
                    SampleId = sampleId,
                    CreatedBy = user.UserId,
                    CreatedOn = DateTime.Now
                };
                db.AnalysisRequestSamples.Add(temp);
                db.SaveChanges();
                return true;

            }
            catch
            {
                return false;
            }
        }
        private bool StoreAnalysisResult(MERCY_Ctx db, int schemeId, int sampleId, string companyCode, UserX users)
        {
            try
            {
                var scheme =
                (
                    from d in db.Schemes
                    join cs in db.CompanySchemes on d.Id equals cs.SchemeId
                    where d.Id == schemeId && cs.CompanyCode == companyCode
                    select new SchemeModelView
                    {
                        Id = d.Id,
                        Name = d.Name,
                        Type = d.Type,
                        MinRepeatability = cs.MinRepeatability,
                        MaxRepeatability = cs.MaxRepeatability,
                        Details = cs.Details
                    }
                ).FirstOrDefault();

                var schemeDetails = scheme.Details != null ? JsonConvert.DeserializeObject<SchemeDetailModelView>(scheme.Details) : null;
                var rules = new List<SchemeDetailRuleModelView>();
                if (schemeDetails == null)
                {
                    return true;
                }
                if (scheme.Type == "SIMPLO")
                {
                    rules = schemeDetails.Rules;
                }
                else
                {
                    rules = schemeDetails.RulesChild;
                }
                var attributesDictionary = rules.Where(r => r.Input)
                   .Select(r => r.Attribute)
                   .ToDictionary(a => a, a => (object)null);

                if (schemeDetails.ExternalAttributes.Count > 0)
                {
                    schemeDetails.ExternalAttributes.ForEach(attribute =>
                    {
                        attributesDictionary.Add(attribute, 0);
                    });
                }

                var attributes = JsonConvert.SerializeObject(attributesDictionary);

                var sampleSchemeId =
                    (
                        from ss in db.SampleSchemes
                        where ss.SchemeId == scheme.Id &&
                        ss.SampleId == sampleId
                        select ss.Id
                    ).FirstOrDefault();

                if (scheme.Type == "SIMPLO")
                {
                    AnalysisResult analyst = new AnalysisResult
                    {
                        Type = "UNK",
                        Result = null,
                        Details = attributes,
                        SampleSchemeId = sampleSchemeId,
                        CreatedBy = users.UserId,
                        CreatedOn = DateTime.Now
                    };

                    db.AnalysisResults.Add(analyst);
                }
                else
                {
                    var analysisResults = new List<AnalysisResult>
                    {
                        new AnalysisResult
                        {
                            Type = "UNK",
                            Result = null,
                            Details = null,
                            SampleSchemeId = sampleSchemeId,
                            CreatedBy = users.UserId,
                            CreatedOn = DateTime.Now
                        },
                        new AnalysisResult
                        {
                            Type = "REP",
                            Result = null,
                            Details = attributes,
                            SampleSchemeId = sampleSchemeId,
                            CreatedBy = users.UserId,
                            CreatedOn = DateTime.Now
                        },
                        new AnalysisResult
                        {
                            Type = "REP",
                            Result = null,
                            Details = attributes,
                            SampleSchemeId = sampleSchemeId,
                            CreatedBy = users.UserId,
                            CreatedOn = DateTime.Now
                        },
                    };

                    db.AnalysisResults.AddRange(analysisResults);
                }

                db.SaveChanges();

                return true;
            }
            catch { }

            return false;
        }
        private bool ValidateSample(SampleValidateModelView validate)
        {
            bool result = false;

            try
            {
                using (MERCY_Ctx db = new MERCY_Ctx())
                {
                    var query =
                   (
                       from s in db.Samples
                       join sdg in db.SampleDetailGenerals on s.Id equals sdg.Sample_Id
                       where s.ClientId == validate.ClientId &&
                           s.ProjectId == validate.ProjectId &&
                           sdg.Shift == validate.Shift &&
                           sdg.SampleId == validate.SampleId
                       select new
                       {
                           s.Id,
                           DetialID = sdg.Id
                       }
                   ).FirstOrDefault();
                    if (query == null)
                    {
                        result = true;
                    }
                }

            }
            catch { }

            return result;
        }
        private int GetJobNumberId(AnalystJobModel job, UserX user, MERCY_Ctx db)
        {
            int result = 0;
            try
            {
                job.JobNumber += job.JobDate.ToString("ddMMyy");
                var query =
                    (
                        from jn in db.AnalysisJobs
                        where jn.JobNumber == job.JobNumber && jn.CompanyCode == job.CompanyCode
                        select new
                        {
                            jn.Id,
                            jn.JobNumber
                        }
                    ).FirstOrDefault();

                if (query == null)
                {
                    var analystJob = new AnalysisJob
                    {
                        CompanyCode = job.CompanyCode,
                        JobNumber = job.JobNumber,
                        ProjectId = job.ProjectId,
                        JobDate = job.JobDate,
                        ReceivedDate = job.ReceivedDate,
                        Status = job.Status,
                        CreatedAt = DateTime.Now,
                        CreatedBy = user.UserId
                    };

                    db.AnalysisJobs.Add(analystJob);
                    db.SaveChanges();

                    result = analystJob.Id;
                }
                else
                {
                    result = query.Id;
                }
            }
            catch { }

            return result;
        }
        private int GetProjectId(string projectName, MERCY_Ctx db)
        {
            int result = 0;
            try
            {
                var project = db.Projects.Where(x => x.Name.Contains(projectName)).FirstOrDefault();
                result = project.Id;
            }
            catch { }
            return result;
        }

        private int GetClientId(string clientName, MERCY_Ctx db)
        {
            int result = 0;
            try
            {
                var client = db.Clients.Where(x => x.Name.Contains(clientName)).FirstOrDefault();
                result = client.Id;
            }
            catch { }
            return result;
        }
        private int GetRefTypeId(string refTypeName, MERCY_Ctx db)
        {
            int result = 0;
            try
            {
                var refType = db.RefTypes.Where(x => x.Name.Contains(refTypeName)).FirstOrDefault();
                result = refType.Id;
            }
            catch { }
            return result;
        }
        private int GetSiteId(string siteName, MERCY_Ctx db)
        {
            int result = 0;
            try
            {
                var site = db.Sites.Where(x => x.SiteName.Contains(siteName)).FirstOrDefault();
                result = site.SiteId;
            }
            catch { }
            return result;
        }
        private List<Models.Scheme> GetSchemesId(string companyCode, int clientId, int projectId, MERCY_Ctx db)
        {
            List<Models.Scheme> tempSchemes = new List<Models.Scheme>();
            try
            {

            }
            catch { }
            return tempSchemes;
        }
    }
}