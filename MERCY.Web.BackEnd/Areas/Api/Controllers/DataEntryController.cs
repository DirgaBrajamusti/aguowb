using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

using System.Data;

using MERCY.Data.EntityFramework;
using MERCY.Web.BackEnd.Models;
using MERCY.Web.BackEnd.Helpers;
using MERCY.Web.BackEnd.Security;
using Permission = MERCY.Web.BackEnd.Security.Permission;
using Newtonsoft.Json;

namespace MERCY.Web.BackEnd.Areas.Api.Controllers
{
    public class DataEntryController : Controller
    {
        // GET: Api/DataEntry
        public JsonResult Index()
        {
            UserX user = new UserX(Request);
            Permission_Item permission_Item = null;

            Permission.Check_API(Request, user, ref permission_Item);
            if (!permission_Item.Is_View)
            {
                var msg = new { Success = false, Permission = permission_Item, Message = Permission.ERROR_PERMISSION_READ, MessageDetail = string.Empty, Version = Configuration.VERSION };
                return Json(msg, JsonRequestBehavior.AllowGet);
            }

            try
            {
                bool is_Show_User_Menu = OurUtility.ValueOf(Request, "u_menu").Equals("1");
                bool is_Show_User_Relation = OurUtility.ValueOf(Request, "u_relation").Equals("1");
                var datas = GetAnalysisData(Request);
                var result = new { Success = true, Permission = permission_Item, User = UserX.Information(user, is_Show_User_Menu, is_Show_User_Relation), Message = string.Empty, MessageDetail = string.Empty, Version = Configuration.VERSION, Items = datas, Total = datas.Count };
                return Json(result, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                var result = new { Success = false, Permission = permission_Item, ex.Message, MessageDetail = string.Empty, Version = Configuration.VERSION, Items = string.Empty, Total = 0 };
                return Json(result, JsonRequestBehavior.AllowGet);
            }
        }

        public JsonResult GetCompletedJob()
        {
            UserX user = new UserX(Request);
            Permission_Item permission_Item = null;

            Permission.Check_API(Request, user, ref permission_Item);
            if (!permission_Item.Is_View)
            {
                var msg = new { Success = false, Permission = permission_Item, Message = Permission.ERROR_PERMISSION_IS_ACKNOWLEDGE, MessageDetail = string.Empty, Version = Configuration.VERSION };
                return Json(msg, JsonRequestBehavior.AllowGet);
            }

            try
            {
                bool is_Show_User_Menu = OurUtility.ValueOf(Request, "u_menu").Equals("1");
                bool is_Show_User_Relation = OurUtility.ValueOf(Request, "u_relation").Equals("1");
                var datas = GetAnalysisData(Request);
                var result = new { Success = true, Permission = permission_Item, User = UserX.Information(user, is_Show_User_Menu, is_Show_User_Relation), Message = string.Empty, MessageDetail = string.Empty, Version = Configuration.VERSION, Items = datas, Total = datas.Count };
                return Json(result, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                var result = new { Success = false, Permission = permission_Item, ex.Message, MessageDetail = string.Empty, Version = Configuration.VERSION, Items = string.Empty, Total = 0 };
                return Json(result, JsonRequestBehavior.AllowGet);
            }
        }

        public JsonResult GetAnalysisResults([Bind(Prefix = "analysisJobIds[]")] List<string> analysisJobIds, int schemeId, string companyCode)
        {
            UserX user = new UserX(Request);
            Permission_Item permission_Item = null;

            // User must be logged in
            if (user.LoginName == null)
            {
                var msg = new { Success = false, Permission = permission_Item, Message = Permission.ERROR_PERMISSION_LOGIN, MessageDetail = string.Empty, Version = Configuration.VERSION };
                return Json(msg, JsonRequestBehavior.AllowGet);
            }

            bool is_Show_User_Menu = OurUtility.ValueOf(Request, "u_menu").Equals("1");
            bool is_Show_User_Relation = OurUtility.ValueOf(Request, "u_relation").Equals("1");
            try
            {
                var data = new List<AnalysisResultModelView>();
                var jobNumbers = new List<string>();
                using (MERCY_Ctx db = new MERCY_Ctx())
                {
                    var analysisResults = (
                            from ar in db.AnalysisResults
                            join ss in db.SampleSchemes on ar.SampleSchemeId equals ss.Id
                            join sc in db.Schemes on ss.SchemeId equals sc.Id
                            join s in db.Samples on ss.SampleId equals s.Id
                            join sdg in db.SampleDetailGenerals on s.Id equals sdg.Sample_Id into ps
                            from sdg in ps.DefaultIfEmpty()
                            join aj in db.AnalysisJobs on s.JobNumberId equals aj.Id
                            join cs in db.CompanySchemes on s.CompanyCode equals cs.CompanyCode
                            where ss.SchemeId == schemeId
                                && analysisJobIds.Any(p => p == aj.Id + "-" + s.ClientId + "-" + s.ProjectId)
                                && cs.CompanyCode == companyCode
                                && cs.SchemeId == ss.SchemeId
                                && s.IsActive == true
                            orderby aj.Id
                            select new
                            {
                                ar.Id,
                                JobNumber = s.SampleDetailGenerals.Count > 0 ? aj.JobNumber : (s.SampleDetailAMDs.Count > 0 ? aj.JobNumber: s.SampleDetailLoadings.FirstOrDefault().LoadingNumber),
                                ar.Type,
                                s.ExternalId,
                                sdg.SampleId,
                                ar.SampleSchemeId,
                                ar.Details,
                                Result = ar.Result == 0 ? null : ar.Result,
                                SchemeType = ss.Scheme.Type,
                                SampleRegistrationId = s.Id,
                                SchemeDetails = cs.Details,
                                cs.MaxRepeatability,
                                cs.MinRepeatability
                            }
                    ).Distinct().ToList();

                    foreach (var item in analysisResults)
                    {
                        var analysisResultItem = new AnalysisResultModelView();
                        var schemeDetails = item.SchemeDetails != null ? JsonConvert.DeserializeObject<SchemeDetailModelView>(item.SchemeDetails) : null;
                        jobNumbers.Add(item.JobNumber);
                        if (item.SchemeType == "DUPLO")
                        {
                            if (item.Type == "UNK")
                            {
                                var analysisResultChilds = analysisResults.Where(x => x.SampleSchemeId == item.SampleSchemeId && x.Type == "REP").ToList();
                                var childData = analysisResultChilds.Select(ar => new AnalysisResultModelView
                                {
                                    Id = ar.Id,
                                    Type = ar.Type,
                                    Ident = item.SampleId ?? item.ExternalId,
                                    ExtIdent = item.ExternalId,
                                    Attributes = ar.Details,
                                    MinRepeatability = item.MinRepeatability,
                                    MaxRepeatability = item.MaxRepeatability,
                                    SampleRegistrationId = item.SampleRegistrationId,
                                    Total = ar.Result
                                }).ToList();

                                if (schemeDetails != null)
                                {
                                    var rules = schemeDetails.RulesChild;
                                    var attributesDictionary = rules.Where(r => r.Input)
                                       .Select(r => r.Attribute)
                                       .ToDictionary(a => a, a => (object)null);
                                    var childDataAttributesDictionaries = childData.Select(c => new
                                    {
                                        c.Id,
                                        Attributes = JsonConvert.DeserializeObject<Dictionary<string, decimal?>>(c.Attributes)
                                    }).ToList();

                                    if (schemeDetails.ExternalAttributes.Count > 0)
                                    {
                                        schemeDetails.ExternalAttributes.ForEach(attribute =>
                                        {
                                            attributesDictionary.Add(attribute, 0);
                                        });

                                        schemeDetails.ExternalAttributes.Select(attribute =>
                                        {
                                            return (
                                                from ar in db.AnalysisResults
                                                join ss in db.SampleSchemes on ar.SampleSchemeId equals ss.Id
                                                join sc in db.Schemes on ss.SchemeId equals sc.Id
                                                where sc.Name == attribute
                                                    && ss.SampleId == item.SampleRegistrationId
                                                    && ar.Type == "UNK"
                                                select new
                                                {
                                                    Key = attribute,
                                                    Value = ar.Result
                                                }
                                            ).FirstOrDefault();
                                        }).ToList().ForEach(attr =>
                                        {
                                            if (attr != null)
                                            {
                                                attributesDictionary[attr.Key] = attr.Value ?? 0;
                                                childDataAttributesDictionaries.ForEach(ca =>
                                                {
                                                    ca.Attributes[attr.Key] = attr.Value ?? 0;
                                                });
                                            }
                                        });
                                    }

                                    var attributes = JsonConvert.SerializeObject(attributesDictionary);
                                    analysisResultItem.Attributes = attributes;

                                    childData.ForEach(c =>
                                    {
                                        var childDataAttributesDictionary = childDataAttributesDictionaries.Where(cd => cd.Id == c.Id).FirstOrDefault();
                                        if (childDataAttributesDictionary != null)
                                        {
                                            c.Attributes = JsonConvert.SerializeObject(childDataAttributesDictionary.Attributes);
                                        }
                                    });
                                }

                                analysisResultItem.Id = item.Id;
                                analysisResultItem.Type = item.Type;
                                analysisResultItem.Ident = item.SampleId ?? item.ExternalId;
                                analysisResultItem.ExtIdent = item.ExternalId;
                                analysisResultItem.Total = item.Result;
                                analysisResultItem.MinRepeatability = item.MinRepeatability;
                                analysisResultItem.MaxRepeatability = item.MaxRepeatability;
                                analysisResultItem.SampleRegistrationId = item.SampleRegistrationId;
                                analysisResultItem.Child = childData;

                                data.Add(analysisResultItem);
                            }
                        }
                        else
                        {
                            analysisResultItem.Id = item.Id;
                            analysisResultItem.Type = item.Type;
                            analysisResultItem.Ident = item.SampleId ?? item.ExternalId;
                            analysisResultItem.ExtIdent = item.ExternalId;
                            analysisResultItem.Attributes = item.Details;
                            analysisResultItem.Total = item.Result;
                            analysisResultItem.MinRepeatability = item.MinRepeatability;
                            analysisResultItem.MaxRepeatability = item.MaxRepeatability;
                            analysisResultItem.SampleRegistrationId = item.SampleRegistrationId;
                            analysisResultItem.Child = null;

                            if (schemeDetails != null && schemeDetails.ExternalAttributes.Count > 0)
                            {
                                var additionalAttributes = schemeDetails.ExternalAttributes.Select(attribute =>
                                {
                                    return (
                                        from ar in db.AnalysisResults
                                        join ss in db.SampleSchemes on ar.SampleSchemeId equals ss.Id
                                        join sc in db.Schemes on ss.SchemeId equals sc.Id
                                        where sc.Name == attribute
                                            && ss.SampleId == item.SampleRegistrationId
                                            && ar.Type == "UNK"
                                        select new
                                        {
                                            Key = attribute,
                                            Value = ar.Result
                                        }
                                    ).FirstOrDefault();
                                }).ToDictionary(a => a.Key, a => a.Value);

                                analysisResultItem.Attributes = JsonConvert.SerializeObject(additionalAttributes);
                            }

                            data.Add(analysisResultItem);
                        }
                    };
                }

                var result = new { Success = true, Permission = permission_Item, User = UserX.Information(user, is_Show_User_Menu, is_Show_User_Relation), Message = string.Empty, MessageDetail = string.Empty, Version = Configuration.VERSION, Items = data.OrderBy(x => x.SampleRegistrationId), JobNumbers = jobNumbers.Distinct() };
                return Json(result, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                var result = new { Success = false, Permission = permission_Item, ex.Message, MessageDetail = string.Empty, Version = Configuration.VERSION, Items = string.Empty, Total = 0 };
                return Json(result, JsonRequestBehavior.AllowGet);
            }
        }

        public JsonResult UpdateAnalysisResult(List<AnalysisResultUpdateModelView> analysisResults)
        {
            UserX user = new UserX(Request);
            Permission_Item permission_Item = null;

            Permission.Check_API(Request, user, ref permission_Item);
            if (!permission_Item.Is_Edit)
            {
                var msg = new { Success = false, Permission = permission_Item, Message = Permission.ERROR_PERMISSION_EDIT, MessageDetail = string.Empty, Version = Configuration.VERSION };
                return Json(msg, JsonRequestBehavior.AllowGet);
            }

            bool is_Show_User_Menu = OurUtility.ValueOf(Request, "u_menu").Equals("1");
            bool is_Show_User_Relation = OurUtility.ValueOf(Request, "u_relation").Equals("1");

            using (MERCY_Ctx db = new MERCY_Ctx())
            {
                var transaction = db.Database.BeginTransaction();
                var listSamples = new List<int> { }; 
                try
                {
                    analysisResults.ForEach(ar =>
                    {
                        var analysisResult = db.AnalysisResults.Find(ar.Id);
                        analysisResult.Details = ar.Attributes;
                        analysisResult.Result = ar.Total;

                        var analysisResultHistory = new AnalysisResultHistory
                        {
                            AnalysisResultId = ar.Id,
                            Result = ar.Total,
                            Details = ar.Attributes,
                            CreatedBy = user.UserId,
                            CreatedOn = DateTime.Now,
                            Type = analysisResult.Type
                        };
                        db.AnalysisResultHistories.Add(analysisResultHistory);
                        db.SaveChanges();

                        var sample = (
                            from s in db.Samples
                            join ss in db.SampleSchemes on s.Id equals ss.SampleId
                            join ar2 in db.AnalysisResults on ss.Id equals ar2.SampleSchemeId
                            where ar2.Id == ar.Id
                            select s
                        ).FirstOrDefault();

                        if (!listSamples.Contains(sample.Id))
                        {
                            listSamples.Add(sample.Id);
                        }
                    });

                    var listJobs = new List<AnalysisResultValidationModel> { };
                    foreach (var sampleId in listSamples)
                    {
                        int index = 0;
                        var sampleStage = IsSampleCompleted(sampleId, db);
                        var sample = db.Samples.Find(sampleId);
                        if (!listJobs.Any(x=>x.JobNumberId == sample.JobNumberId))
                        {
                            listJobs.Add(new AnalysisResultValidationModel { JobNumberId= sample.JobNumberId, IsJobCompleted = true });
                        }

                        if (!sampleStage)
                        {
                            index = listJobs.FindIndex(x => x.JobNumberId == sample.JobNumberId);
                            listJobs[index].IsJobCompleted = false;
                            
                        }
                        sample.Stage = sampleStage ? "Completed" : "In Progress";
                        sample.LastModifiedBy = user.UserId;
                        sample.LastModifiedOn = DateTime.Now;
                        db.SaveChanges();
                    }
                    foreach(var jobNumber in listJobs)
                    {
                        var job = db.AnalysisJobs.Find(jobNumber.JobNumberId);
                        job.Status = jobNumber.IsJobCompleted ? "Completed" : "In Progress";
                        job.LastModifiedBy = user.UserId;
                        job.LastModifiedAt = DateTime.Now;
                        job.ValidatedBy = null;
                        job.ValidatedBy = null;
                        if (jobNumber.IsJobCompleted)
                        {
                            job.CompletedDate = DateTime.Now;
                        }
                        db.SaveChanges();
                    }
                    
                    transaction.Commit();
                    var result = new { Success = true, Permission = permission_Item, User = UserX.Information(user, is_Show_User_Menu, is_Show_User_Relation), Message = string.Empty, MessageDetail = string.Empty, Version = Configuration.VERSION, Items = analysisResults };
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
        private bool IsSampleCompleted(int sampleId, MERCY_Ctx db)
        {
            try
            {
                var listAnalysistResults =
                    (
                        from s in db.Samples
                        join ss in db.SampleSchemes on s.Id equals ss.SampleId
                        join ar in db.AnalysisResults on ss.Id equals ar.SampleSchemeId
                        join cs in db.CompanySchemes on ss.SchemeId equals cs.SchemeId
                        where s.Id == sampleId
                          && cs.CompanyCode == s.CompanyCode
                          && ar.Type == "UNK"
                        select new
                        {
                            csDetails = cs.Details,
                            arDetail = ar.Details,
                            arResult = ar.Result
                        }
                    ).Distinct().ToList();

                var result = listAnalysistResults.Where(ar => ar.arResult == null).Count() > 0 ? false : true ;
                return result;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }

        }

        public JsonResult GetAvailableSchemes(string companyCode, [Bind(Prefix = "analysisJobIds[]")] List<string> analysisJobIds)
        {
            UserX user = new UserX(Request);
            Permission_Item permission_Item = null;

            // User must be logged in
            if (user.LoginName == null)
            {
                var msg = new { Success = false, Permission = permission_Item, Message = Permission.ERROR_PERMISSION_LOGIN, MessageDetail = string.Empty, Version = Configuration.VERSION };
                return Json(msg, JsonRequestBehavior.AllowGet);
            }

            bool is_Show_User_Menu = OurUtility.ValueOf(Request, "u_menu").Equals("1");
            bool is_Show_User_Relation = OurUtility.ValueOf(Request, "u_relation").Equals("1");
            try
            {
                using (MERCY_Ctx db = new MERCY_Ctx())
                {
                    var data =
                            (
                                from d in db.Schemes
                                join cs in db.CompanySchemes on d.Id equals cs.SchemeId
                                join ss in db.SampleSchemes on d.Id equals ss.SchemeId
                                join ccps in db.CompanyClientProjectSchemes on new { Id = d.Id, CompanyCode = companyCode } equals new { Id = ccps.SchemeId, CompanyCode = ccps.CompanyCode }
                                join s in db.Samples on ss.SampleId equals s.Id
                                join aj in db.AnalysisJobs on s.JobNumberId equals aj.Id
                                join ar in db.AnalysisResults on ss.Id equals ar.SampleSchemeId
                                orderby d.Name
                                where cs.CompanyCode == companyCode
                                    && analysisJobIds.Any(p => p == aj.Id + "-" + s.ClientId + "-" + s.ProjectId)
                                    && s.IsActive == true
                                select new SchemeModelView
                                {
                                    Id = d.Id,
                                    Name = d.Name,
                                    Type = d.Type,
                                    MinRepeatability = cs.MinRepeatability,
                                    MaxRepeatability = cs.MaxRepeatability,
                                    Details = cs.Details,
                                    IsCompleted = true,
                                    AnalysisResultDetail = ar.Details,
                                    Result = ar.Result,
                                    AnalysisResultId = ar.Id,
                                    SchemeOrder = ccps.SchemeOrder,
                                }
                            ).Distinct().ToList();
                    var schemes = new List<SchemeModelView> { };

                    foreach (var sample in data)
                    {
                        // Make new data or just find the id
                        if (!schemes.Distinct().Any(u => u.Id == sample.Id))
                        {
                            schemes.Add(new SchemeModelView { Id = sample.Id, IsCompleted = sample.Result != null });
                        }
                        
                    }
                    var resultSchemes = new List<SchemeModelView> { };

                    // Get each parameter sample
                    foreach (var d in data.Select(u => u.Id).Distinct().ToList())
                    {
                        var ar = data.Where(u => u.Id == d).Select(u => u.AnalysisResultDetail).ToList();
                        var isNotYetComplete = schemes.Any(u => u.Id == d && u.IsCompleted == false);
                        var r = data.Where(u => u.Id == d).Select(u => u.Result).ToList();
                        var s = data.Where(u => u.Id == d).FirstOrDefault();
                        resultSchemes.Add(new SchemeModelView
                        {
                            Id = s.Id,
                            Name = s.Name,
                            Type = s.Type,
                            MinRepeatability = s.MinRepeatability,
                            MaxRepeatability = s.MaxRepeatability,
                            Details = s.Details,
                            IsCompleted = !isNotYetComplete,
                            AnalysisResultDetails = ar,
                            Results = r,
                            SchemeOrder = s.SchemeOrder
                        });
                    }
                    resultSchemes.Distinct().ToList();

                    var result = new { Success = true, Permission = permission_Item, User = UserX.Information(user, is_Show_User_Menu, is_Show_User_Relation), Message = string.Empty, MessageDetail = string.Empty, Version = Configuration.VERSION, Items = resultSchemes.OrderBy(x => x.SchemeOrder), };
                    return Json(result, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                var result = new { Success = false, Permission = permission_Item, ex.Message, MessageDetail = string.Empty, Version = Configuration.VERSION, Items = string.Empty, Total = 0 };
                return Json(result, JsonRequestBehavior.AllowGet);
            }
        }

        private static List<AnalystModelView> GetAnalysisData(System.Web.HttpRequestBase Request)
        {
            DateTime dateFrom = DateTime.Now;
            DateTime dateTo = DateTime.Now;
            bool all_company = OurUtility.ValueOf(Request, "company").Equals(string.Empty);
            string company_code = OurUtility.ValueOf(Request, "company");
            bool all_site = OurUtility.ValueOf(Request, "site").Equals(string.Empty);
            int site_id = OurUtility.ToInt32(OurUtility.ValueOf(Request, "site"));
            bool all_client = OurUtility.ValueOf(Request, "client").Equals(string.Empty);
            int client_id = OurUtility.ToInt32(OurUtility.ValueOf(Request, "client"));
            bool all_project = OurUtility.ValueOf(Request, "project").Equals(string.Empty);
            int project_id = OurUtility.ToInt32(OurUtility.ValueOf(Request, "project"));
            bool all_dateofjob = (OurUtility.ValueOf(Request, "dateFrom").Equals(string.Empty) && OurUtility.ValueOf(Request, "dateTo").Equals(string.Empty));
            bool all_search = OurUtility.ValueOf(Request, "sample").Equals(string.Empty);
            string sample_id = OurUtility.ValueOf(Request, "sample");
            bool all_status = OurUtility.ValueOf(Request, "status").Equals(string.Empty);

            if (!all_dateofjob)
            {
                dateFrom = DateTime.Parse(OurUtility.ValueOf(Request, "dateFrom"));
                dateTo = DateTime.Parse(OurUtility.ValueOf(Request, "dateTo")).AddHours(23).AddMinutes(59);
            }
            List<AnalystModelView> datas = new List<AnalystModelView>();
            try
            {
                using (MERCY_Ctx db = new MERCY_Ctx())
                {
                    var dataQuery =
                        (
                            from s in db.Samples
                            join aj in db.AnalysisJobs on s.JobNumberId equals aj.Id
                            join p in db.Projects on s.ProjectId equals p.Id
                            join cl in db.Clients on s.ClientId equals cl.Id
                            join c in db.Companies on s.CompanyCode equals c.CompanyCode
                            join st in db.Sites on s.SiteId equals st.SiteId
                            where (all_company || s.CompanyCode == company_code)
                               && (all_site || s.SiteId == site_id)
                               && (all_client || s.ClientId == client_id)
                               && (all_project || s.ProjectId == project_id)
                               && (all_dateofjob || (aj.JobDate >= dateFrom && aj.JobDate <= dateTo))
                               && (all_search || s.ExternalId.Contains(sample_id))
                               && (all_status || aj.Status.Contains("Completed") || aj.Status.Contains("Validated") || aj.Status.Contains("Approved"))
                               && s.DeletedBy == null
                               && s.IsActive == true
                            select new
                            {
                                JobNumber = s.SampleDetailGenerals.Count > 0 ? aj.JobNumber : (s.SampleDetailAMDs.Count > 0 ? aj.JobNumber : s.SampleDetailLoadings.FirstOrDefault().LoadingNumber),
                                ClientName = cl.Name,
                                ProductName = p.Name,
                                Receive = aj.ReceivedDate,
                                s.ClientId,
                                s.ProjectId,
                                aj.Status,
                                aj.ValidatedDate,
                                aj.ApprovedDate,
                                aj.CompletedDate,
                                DateOfjob = aj.JobDate,
                                aj.Id

                            }
                        ).Distinct().OrderByDescending(x => x.Receive).ToList();

                    dataQuery.ForEach(
                        x => datas.Add(new AnalystModelView
                        {
                            Id = x.Id,
                            JobNumber = x.JobNumber,
                            ClientCode = x.ClientName,
                            ProjectId = x.ProjectId,
                            ClientId = x.ClientId,
                            ProjectName = x.ProductName,
                            Received = x.Receive == null ? string.Empty : x.Receive.Value.ToString("dd-MMM-yyyy hh:mm:ss"),
                            ValidatedDate = x.ValidatedDate == null ? string.Empty : x.ValidatedDate.Value.ToString("dd-MMM-yyyy"),
                            ApprovedDate = x.ApprovedDate == null ? string.Empty : x.ApprovedDate.Value.ToString("dd-MMM-yyyy"),
                            Status = x.Status,
                            MmStatus = GetMMStatus(x.Id),
                        }));
                }

                return datas;
            }
            catch
            {
                return null;
            }
        }
        private static string GetMMStatus(int analysisJobId)
        {
            var isSuccess = 0;
            var isFailed = 0;
            var isNull = 0;
            try
            {
               
                using (MERCY_Ctx db = new MERCY_Ctx())
                {
                    var dataQuery =
                        (
                            from s in db.Samples
                            where s.JobNumberId == analysisJobId
                            select new
                            {
                                s.Id,
                                s.mm_status
                            }
                        ).ToList();

                    for (int i = 0; i < dataQuery.Count(); i++)
                    {
                        if (dataQuery[i].mm_status != null)
                        {
                            switch (dataQuery[i].mm_status.ToLower())
                            {
                                case "failed":
                                    isFailed += 1;
                                    break;

                                case "success":
                                    isSuccess += 1;
                                    break;
                            }
                        } else
                        {
                            isNull += 1;
                        }
                    }
                }
                
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
            string mStatus = string.Empty;
            if ((isFailed > 0 || isNull > 0) && isSuccess > 0)
            {
                mStatus = "Partially Synchronized";
            }
            else if (isSuccess == 0 && (isFailed > 0 || isNull > 0))
            {
                mStatus = "Not Synchronized";
            }
            else
            {
                mStatus = "Synchronized";
            }
            return mStatus;
        }

    }

}