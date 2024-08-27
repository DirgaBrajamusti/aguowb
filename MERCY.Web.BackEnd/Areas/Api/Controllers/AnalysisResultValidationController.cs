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
using System.Collections.Specialized;
using Newtonsoft.Json;

namespace MERCY.Web.BackEnd.Areas.Api.Controllers
{
    public class AnalysisResultValidationController : Controller
    {
        // GET: Api/AnalysisResultValidation
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

            bool is_Show_User_Menu = OurUtility.ValueOf(Request, "u_menu").Equals("1");
            bool is_Show_User_Relation = OurUtility.ValueOf(Request, "u_relation").Equals("1");
            int analysisJobId = OurUtility.ToInt32(OurUtility.ValueOf(Request, "analysisJobId"));
            Configuration config = new Configuration();
            var PORNumber = OurUtility.ToDecimal(config.PYRATE_OXIDATION_REACTION_NUMBER);

            List<ListDictionary> listOfListDictionary = new List<ListDictionary>();
            var rules = "";
            try
            {
                using (MERCY_Ctx db = new MERCY_Ctx())
                {
                    var dataQuery =
                        (
                            from aj in db.AnalysisJobs
                            join s in db.Samples on aj.Id equals s.JobNumberId
                            join ss in db.SampleSchemes on s.Id equals ss.SampleId
                            join ar in db.AnalysisResults on ss.Id equals ar.SampleSchemeId
                            join sdg in db.SampleDetailGenerals on s.Id equals sdg.Sample_Id into ps
                            from sdg in ps.DefaultIfEmpty()
                            join sdl in db.SampleDetailLoadings on s.Id equals sdl.SampleId into pl
                            from sdl in pl.DefaultIfEmpty()
                            join sda in db.SampleDetailAMDs on s.Id equals sda.Sample_Id into pa
                            from sda in pa.DefaultIfEmpty()
                            join ccps in db.CompanyClientProjectSchemes on aj.ProjectId equals ccps.ProjectId /*new { s.ClientId, s.ProjectId, s.CompanyCode } equals new { ccps.ClientId, ccps.ProjectId, ccps.CompanyCode }*/
                            join c in db.Companies on ccps.CompanyCode equals c.CompanyCode
                            join p in db.Projects on aj.ProjectId equals p.Id
                            where
                                aj.Id == analysisJobId
                                && ar.Result != null
                                && ar.Type == "UNK"
                                && s.IsActive == true
                                && ccps.CompanyCode == s.CompanyCode
                                && ccps.ClientId == s.ClientId
                                && ccps.SchemeId == ss.SchemeId
                            select new
                            {
                                SampleId = sdg != null ? sdg.SampleId : (sda != null ? sda.SampleId : s.ExternalId),
                                Tonnage = sdg == null ? (sdl == null ? 0 : sdl.Tonnage) : sdg.Tonnage,
                                s.Id,
                                ar.Result,
                                ss.Scheme.Name,
                                Status = s.Stage,
                                ccps.SchemeOrder,
                                aj.ProjectId,
                                ccps.ClientId,
                                s.CompanyCode,
                                DateSampled = s.DateOfJob,
                                sdg.BargeName,
                                Shift = sdg != null ? sdg.Shift.ToString() : sda.Shift.ToString(),
                                Location = sda != null ? sda.Location : s.Location,
                                Remark = sda != null ? sda.Remark : s.Remark,
                                ar.Details,
                                s.ExternalId,
                                c.TonnageDecimalNumber,
                                MmStatus = s.mm_status ?? string.Empty,
                                MmRemark = s.mm_remark ?? string.Empty,
                                ProjectCode = p.Code,
                                MassSampleReceived = sda != null ? sda.MassSampleReceived : 0,
                                TS = sda != null ? sda.TS : 0,
                                ANC = sda != null ? sda.ANC : 0,
                                NAG = sda != null ? sda.NAG : 0,
                                NAGPH45 = sda != null ? sda.NAGPH45 : string.Empty, 
                                NAGPH70 = sda != null ? sda.NAGPH70 : string.Empty,
                                NAGType = sda != null ? sda.NAGType : string.Empty,
                                LaboratoryId = sda != null ? sda.LaboratoryId : string.Empty,
                                SampleType = sda != null ? sda.SampleType : string.Empty
                            }
                        ).Distinct().OrderBy(x => x.SchemeOrder).ToList();

                    var sampleList = dataQuery.Select(x => 
                        new { 
                            x.Id,
                            x.SampleId,
                            x.ExternalId,
                            x.Tonnage,
                            x.Status,
                            x.DateSampled,
                            x.BargeName,
                            x.Shift,
                            x.Location,
                            x.Remark,
                            x.MassSampleReceived,
                            x.TS,
                            x.ANC,
                            x.NAG,
                            x.NAGPH45,
                            x.NAGPH70,
                            x.NAGType,
                            x.LaboratoryId,
                            x.SampleType,
                            x.MmStatus,
                            x.MmRemark
                        }).Distinct().ToList();
                    var schemeNameList = dataQuery.Select(x => x.Name).Distinct().ToList();
                    var data = dataQuery.FirstOrDefault();

                    var rulesScheme = string.Empty;
                    
                    if (data.ProjectCode != "AMD")
                    {
                        for (int i = 0; i < schemeNameList.Count(); i++)
                        {
                            var schemeHeader = string.Empty;
                            var schemeName = schemeNameList[i];
                            if (schemeName == "CV" && data.CompanyCode != "JBG")
                            {
                                schemeHeader = schemeName + " cal/g";
                                rulesScheme += ",{" + string.Format(" \"header\": \"{0}\", \"attribute\": \"{1}\", \"input\": false", $"{schemeHeader} (adb)", $"{schemeName}") + "}";
                                rulesScheme += ",{" + string.Format(" \"header\": \"{0}\", \"attribute\": \"{1}\", \"input\": false", $"{schemeHeader} (ar)", $"{schemeName}Ar") + "}";
                                rulesScheme += ",{" + string.Format(" \"header\": \"{0}\", \"attribute\": \"{1}\", \"input\": false", $"{schemeHeader} (db)", $"{schemeName}Db") + "}";
                                schemeName = $"{schemeName}Daf";
                                schemeHeader = $"{schemeHeader} (daf)";
                            }

                            else if (schemeName == "SIZE")
                            {
                                rulesScheme += ",{" + string.Format(" \"header\": \"{0}\", \"attribute\": \"{1}\", \"input\": false", "Size +50 mm (%)", "Size50") + "}";
                                rulesScheme += ",{" + string.Format(" \"header\": \"{0}\", \"attribute\": \"{1}\", \"input\": false", "Size -50mm+2.0 mm (%)", "Size502") + "}";
                                schemeName = "Size2";
                                schemeHeader = "Size -2.0 mm (%)";
                            }
                            else if (schemeName == "RD")
                            {
                                schemeHeader = schemeName;
                            }
                            else if (schemeName == "TGA" && (data.CompanyCode == "TCM" || data.CompanyCode == "BEK"))
                            {
                                schemeHeader = "FC %";
                            }
                            else if (schemeName == "ADL")
                            {
                                continue;
                            }
                            else if (schemeName == "ASH" && data.CompanyCode == "JBG")
                            {
                                schemeHeader = schemeName;
                                rulesScheme += ",{" + string.Format(" \"header\": \"{0}\", \"attribute\": \"{1}\", \"input\": false", $"{schemeHeader}_AD", $"{schemeName}Ad") + "}";
                                if (schemeNameList.Any("TM".Contains))
                                {
                                    rulesScheme += ",{" + string.Format(" \"header\": \"{0}\", \"attribute\": \"{1}\", \"input\": false", $"{schemeHeader}_AR", $"{schemeName}Ar") + "}";
                                }
                                schemeName = $"{schemeName}Db";
                                schemeHeader = $"{schemeHeader}_DB";
                            }
                            else if (schemeName == "TS" && data.CompanyCode == "JBG")
                            {
                                schemeHeader = schemeName;
                                rulesScheme += ",{" + string.Format(" \"header\": \"{0}\", \"attribute\": \"{1}\", \"input\": false", $"{schemeHeader}_AD", $"{schemeName}Ad") + "}";
                                if (schemeNameList.Any("TM".Contains))
                                {
                                    rulesScheme += ",{" + string.Format(" \"header\": \"{0}\", \"attribute\": \"{1}\", \"input\": false", $"{schemeHeader}_AR", $"{schemeName}Ar") + "}";
                                }
                                schemeName = $"{schemeName}Db";
                                schemeHeader = $"{schemeHeader}_DB";
                            }
                            else if (schemeName == "CV" && data.CompanyCode == "JBG")
                            {
                                schemeHeader = schemeName;
                                rulesScheme += ",{" + string.Format(" \"header\": \"{0}\", \"attribute\": \"{1}\", \"input\": false", $"{schemeHeader}_AD CAL/G", $"{schemeName}Ad") + "}";
                                if (schemeNameList.Any("TM".Contains))
                                {
                                    rulesScheme += ",{" + string.Format(" \"header\": \"{0}\", \"attribute\": \"{1}\", \"input\": false", $"{schemeHeader}_AR CAL/G", $"{schemeName}Ar") + "}";
                                }
                                rulesScheme += ",{" + string.Format(" \"header\": \"{0}\", \"attribute\": \"{1}\", \"input\": false", $"{schemeHeader}_DB CAL/G", $"{schemeName}Db") + "}";
                                schemeName = $"{schemeName}Daf";
                                schemeHeader = $"{schemeHeader}_DAF CAL/G";
                            }
                            else
                            {
                                schemeHeader = schemeName + " %";
                            }

                            rulesScheme += ",{" + string.Format(" \"header\": \"{0}\", \"attribute\": \"{1}\", \"input\": false", schemeHeader, schemeName) + "}";
                        }
                    }
                    

                    rules = "{ \"rules\": [";
                    rules += "{ \"header\": \"Type\", \"attribute\": \"Type\", \"input\": false}";
                    rules += ",{ \"header\": \"Sample ID\", \"attribute\": \"Ident\", \"input\": false}";
                    rules += ",{ \"header\": \"External ID\", \"attribute\": \"ExtIdent\", \"input\": false}";
                    if (data.ProjectCode != "AMD")
                    {
                        rules += ",{ \"header\": \"Tonnage\", \"attribute\": \"Tonnage\", \"input\": false}";
                        if (data != null && data.ProjectId == 7) // Barge Loading
                        {
                            rules += ",{ \"header\": \"Name of Barge\", \"attribute\": \"BargeName\", \"input\": false}";
                        }

                        rules += ",{ \"header\": \"MM Status\", \"attribute\": \"MmStatus\", \"input\": false}";
                        rules += ",{ \"header\": \"MM Remark\", \"attribute\": \"MmRemark\", \"input\": false}";
                    } else
                    {
                        rules += ",{ \"header\": \"Laboratory\", \"attribute\": \"LaboratoryId\", \"input\": false}";
                        rules += ",{ \"header\": \"SampleType\", \"attribute\": \"SampleType\", \"input\": false}";
                        rules += ",{ \"header\": \"Location\", \"attribute\": \"Location\", \"input\": false}";
                        rules += ",{ \"header\": \"MassSampleReceived\", \"attribute\": \"MassSampleReceived\", \"input\": false}";
                        rules += ",{ \"header\": \"TS\", \"attribute\": \"TS\", \"input\": false}";
                        rules += ",{ \"header\": \"ANC\", \"attribute\": \"ANC\", \"input\": false}";
                        rules += ",{ \"header\": \"NAG\", \"attribute\": \"NAG\", \"input\": false}";
                        rules += ",{ \"header\": \"MPA\", \"attribute\": \"MPA\", \"input\": false}";
                        rules += ",{ \"header\": \"NAPP\", \"attribute\": \"NAPP\", \"input\": false}";
                        rules += ",{ \"header\": \"NAGPH45\", \"attribute\": \"NAGPH45\", \"input\": false}";
                        rules += ",{ \"header\": \"NAGPH70\", \"attribute\": \"NAGPH70\", \"input\": false}";
                        rules += ",{ \"header\": \"NAGType\", \"attribute\": \"NAGType\", \"input\": false}";
                        rules += ",{ \"header\": \"Remark\", \"attribute\": \"Remark\", \"input\": false}";
                    }
                    
                    rules += rulesScheme;

                    rules += "] }";



                    for (int i = 0; i < sampleList.Count(); i++)
                    {

                        ListDictionary listDictionary = new ListDictionary
                        {
                            { "Type", "AVG" },
                            { "Id", sampleList[i].Id.ToString() },
                            { "Ident", sampleList[i].SampleId },
                            { "ExtIdent", sampleList[i].ExternalId },
                            { "Tonnage", data.TonnageDecimalNumber == 0 ? Math.Round(sampleList[i].Tonnage, MidpointRounding.AwayFromZero).ToString() : Math.Round(sampleList[i].Tonnage, data.TonnageDecimalNumber).ToString() },
                            { "Status", sampleList[i].Status.ToString() },
                            { "DateSampled", sampleList[i].DateSampled.ToString() },
                            { "BargeName", sampleList[i].BargeName?.ToString() ?? string.Empty },
                            { "Shift", sampleList[i].Shift.ToString() },
                            { "Location", sampleList[i].Location?.ToString() ?? string.Empty },
                            { "Remark", sampleList[i].Remark?.ToString() ?? string.Empty },
                            { "MassSampleReceived", sampleList[i].MassSampleReceived.ToString() },
                            { "ANC", sampleList[i].ANC.ToString("0.0000") },
                            { "NAG", sampleList[i].NAG.ToString("0.0000") },
                            { "TS", sampleList[i].TS.ToString("0.0000") },
                            { "NAGPH45", sampleList[i].NAGPH45.ToString() },
                            { "NAGPH70", sampleList[i].NAGPH70.ToString() },
                            { "NAGType", sampleList[i].NAGType.ToString() },
                            { "LaboratoryId", sampleList[i].LaboratoryId.ToString() },
                            { "SampleType", sampleList[i].SampleType.ToString() }
                        };
                        
                        if (data.ProjectCode == "AMD")
                        {
                            decimal TS = sampleList[i].TS;
                            decimal ANC = sampleList[i].ANC;
                            decimal MPA = PORNumber * TS;
                            decimal NAPP = MPA - ANC;
                            listDictionary.Add($"MPA", MPA.ToString("0.0000"));
                            listDictionary.Add($"NAPP", NAPP.ToString("0.0000"));
                        }
                        var schemeList = dataQuery.Where(x => x.Id == sampleList[i].Id).ToList();
                        for (int j = 0; j < schemeList.Count(); j++)
                        {
                            if (schemeList[j].Name.ToString() == "CV" && data.CompanyCode != "JBG")
                            {
                                var IM = schemeList.Where(sc => sc.Name == "IM").FirstOrDefault();
                                var ASH = schemeList.Where(sc => sc.Name == "ASH").FirstOrDefault();
                                var TM = schemeList.Where(sc => sc.Name == "TM").FirstOrDefault();
                                var cvDb = Math.Round((schemeList[j]?.Result ?? 0) * (100 / (100 - (IM?.Result ?? 0))), 2);
                                var cvDaf = Math.Round((schemeList[j]?.Result ?? 0) * (100 / (100 - (IM?.Result ?? 0) - (ASH?.Result ?? 0))), 2);
                                var cvAr = Math.Round((schemeList[j]?.Result ?? 0) * ((100 - (TM?.Result ?? 0)) / (100 - (IM?.Result ?? 0))), 2);
                                listDictionary.Add($"{schemeList[j].Name}Ar", cvAr);
                                listDictionary.Add($"{schemeList[j].Name}Db", cvDb);
                                listDictionary.Add($"{schemeList[j].Name}Daf", cvDaf);
                            }
                            else if (schemeList[j].Name.ToString() == "SIZE")
                            {
                                if (!schemeList[j].Details.Contains("null"))
                                {
                                    SizeSchemeAttributeModelView sizeSchemeAttribute = JsonConvert.DeserializeObject<SizeSchemeAttributeModelView>(schemeList[j].Details);
                                    listDictionary.Add("Size50", value: Math.Round((sizeSchemeAttribute.A / sizeSchemeAttribute.M1 * 100), 2));
                                    listDictionary.Add("Size502", value: Math.Round((sizeSchemeAttribute.B / sizeSchemeAttribute.M1 * 100), 2));
                                    listDictionary.Add("Size2", value: Math.Round((sizeSchemeAttribute.C / sizeSchemeAttribute.M1 * 100), 2));
                                }
                                else
                                {
                                    listDictionary.Add("Size50", 0);
                                    listDictionary.Add("Size502", 0);
                                    listDictionary.Add("Size2", 0);
                                }
                            }
                            else if (schemeList[j].Name.ToString() == "CV" && data.CompanyCode == "JBG")
                            {
                                var IM = schemeList.Where(sc => sc.Name == "IM").FirstOrDefault();
                                var ASH = schemeList.Where(sc => sc.Name == "ASH").FirstOrDefault();
                                var TM = schemeList.Where(sc => sc.Name == "TM").FirstOrDefault();
                                var ashDb = Math.Round((100 / (100 - (IM?.Result ?? 0))) * (ASH?.Result ?? 0), 2);
                                var cvAd = schemeList[j]?.Result ?? 0;
                                if (TM?.Result > 0)
                                {
                                    var cvAr = Math.Round((100 - (TM?.Result ?? 0)) / (100 - (IM?.Result ?? 0)) * cvAd, 2);
                                    listDictionary.Add($"{schemeList[j].Name}Ar", cvAr);
                                }
                                var cvDb = Math.Round((100 / (100 - (IM?.Result ?? 0))) * cvAd, 2);
                                var cvDaf = Math.Round((100 / (100 - ashDb)) * cvDb, 2);
                                listDictionary.Add($"{schemeList[j].Name}Ad", cvAd);
                                listDictionary.Add($"{schemeList[j].Name}Db", cvDb);
                                listDictionary.Add($"{schemeList[j].Name}Daf", cvDaf);
                            }
                            else if (schemeList[j].Name.ToString() == "ASH" && data.CompanyCode == "JBG")
                            {
                                var IM = schemeList.Where(sc => sc.Name == "IM").FirstOrDefault();
                                var ASH = schemeList.Where(sc => sc.Name == "ASH").FirstOrDefault();
                                var TM = schemeList.Where(sc => sc.Name == "TM").FirstOrDefault();
                                var ashDb = Math.Round((100 / (100 - (IM?.Result ?? 0))) * (ASH?.Result ?? 0), 2);
                                var ashAd = schemeList[j]?.Result ?? 0;
                                if (TM?.Result > 0)
                                {
                                    var ashAr = Math.Round((100 - (TM?.Result ?? 0)) / (100 - (IM?.Result ?? 0)) * ashAd, 2);
                                    listDictionary.Add($"{schemeList[j].Name}Ar", ashAr);
                                }
                                listDictionary.Add($"{schemeList[j].Name}Ad", ashAd);
                                listDictionary.Add($"{schemeList[j].Name}Db", ashDb);
                            }
                            else if (schemeList[j].Name.ToString() == "TS" && data.CompanyCode == "JBG")
                            {
                                var IM = schemeList.Where(sc => sc.Name == "IM").FirstOrDefault();
                                var ASH = schemeList.Where(sc => sc.Name == "ASH").FirstOrDefault();
                                var TM = schemeList.Where(sc => sc.Name == "TM").FirstOrDefault();
                                var tsAd = schemeList[j]?.Result ?? 0;
                                if (TM?.Result > 0)
                                {
                                    var tsAr = Math.Round((100 - (TM?.Result ?? 0)) / (100 - (IM?.Result ?? 0)) * tsAd, 2);
                                    listDictionary.Add($"{schemeList[j].Name}Ar", tsAr);
                                }
                                var tsDb = Math.Round((100 / (100 - (IM?.Result ?? 0))) * tsAd, 2);
                                listDictionary.Add($"{schemeList[j].Name}Ad", tsAd);
                                listDictionary.Add($"{schemeList[j].Name}Db", tsDb);
                            }

                            listDictionary.Add(schemeList[j].Name.ToString(), schemeList[j].Result);
                        }
                        listDictionary.Add($"MmStatus", sampleList[i].MmStatus);
                        listDictionary.Add($"MmRemark", sampleList[i].MmRemark);
                        listOfListDictionary.Add(listDictionary);

                    }

                }

                var result = new { Success = true, Permission = permission_Item, User = UserX.Information(user, is_Show_User_Menu, is_Show_User_Relation), Message = string.Empty, MessageDetail = string.Empty, Version = Configuration.VERSION, rules = rules, Items = listOfListDictionary, Total = listOfListDictionary.Count };
                return Json(result, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                var result = new { Success = false, Permission = permission_Item, ex.Message, MessageDetail = ex.StackTrace, Version = Configuration.VERSION, Items = string.Empty, Total = 0 };
                return Json(result, JsonRequestBehavior.AllowGet);
            }
        }

        public JsonResult History(AnalysisHistoryModelView analysisHistory)
        {
            UserX user = new UserX(Request);
            Permission_Item permission_Item = null;

            Permission.Check_API(Request, user, ref permission_Item);
            if (!permission_Item.Is_Acknowledge || !permission_Item.Is_Approve)
            {
                var msg = new { Success = false, Permission = permission_Item, Message = Permission.ERROR_PERMISSION_READ, MessageDetail = string.Empty, Version = Configuration.VERSION };
                return Json(msg, JsonRequestBehavior.AllowGet);
            }

            bool is_Show_User_Menu = OurUtility.ValueOf(Request, "u_menu").Equals("1");
            bool is_Show_User_Relation = OurUtility.ValueOf(Request, "u_relation").Equals("1");

            List<ListDictionary> results = new List<ListDictionary>();
            var rules = "";
            try
            {
                using (MERCY_Ctx db = new MERCY_Ctx())
                {
                    var dataQuery =
                        (
                            from s in db.Samples
                            join ss in db.SampleSchemes on s.Id equals ss.SampleId
                            join ar in db.AnalysisResults on ss.Id equals ar.SampleSchemeId
                            join arh in db.AnalysisResultHistories on ar.Id equals arh.AnalysisResultId
                            join sdg in db.SampleDetailGenerals on s.Id equals sdg.Sample_Id into ps
                            from sdg in ps.DefaultIfEmpty()
                            join sdl in db.SampleDetailLoadings on s.Id equals sdl.SampleId into pl
                            join aj in db.AnalysisJobs on s.JobNumberId equals aj.Id
                            from sdl in pl.DefaultIfEmpty()
                            where s.Id == analysisHistory.SampleId && ar.Result != null && ar.Type == "UNK"
                            orderby arh.CreatedOn ascending
                            select new
                            {
                                s.Id,
                                SampleId = sdg != null ? sdg.SampleId : s.ExternalId,
                                s.ExternalId,
                                Tonnage = sdg == null ? (sdl == null ? 0 : sdl.Tonnage) : sdg.Tonnage,
                                arh.Result,
                                ss.Scheme.Name,
                                Status = s.Stage,
                                arh.CreatedOn,
                                s.ValidatedBy,
                                s.ValidatedDate,
                                aj.ApprovedBy,
                                aj.ApprovedDate

                            }
                        ).Distinct().OrderBy(x => x.CreatedOn).ToList();

                    var sampleList = dataQuery.Select(x => new { x.Id, x.SampleId, x.ExternalId, x.Tonnage, x.Status }).Distinct().ToList();
                    var schemeNameList = dataQuery.Select(x => x.Name).Distinct().ToList();

                    rules = "{ \"rules\": [";
                    rules += "{ \"header\": \"Type\", \"attribute\": \"Type\", \"input\": false}";
                    rules += ",{ \"header\": \"Ident\", \"attribute\": \"Ident\", \"input\": false}";
                    rules += ",{ \"header\": \"SampleId\", \"attribute\": \"ExtIdent\", \"input\": false}";
                    rules += ",{ \"header\": \"Tonnage\", \"attribute\": \"Tonnage\", \"input\": false}";

                    schemeNameList.ForEach(schemeName =>
                    {
                        rules = rules + ",{" + string.Format(" \"header\": \"{0} %\", \"attribute\": \"{0}\", \"input\": false", schemeName) + "}";
                    });

                    rules += ",{ \"header\": \"Validate By\", \"attribute\": \"ValidateBy\", \"input\": false}";
                    rules += ",{ \"header\": \"Validate Time\", \"attribute\": \"ValidateTime\", \"input\": false}";
                    if (analysisHistory.IsApproval)
                    {
                        rules += ",{ \"header\": \"Approval By\", \"attribute\": \"ApprovalBy\", \"input\": false}";
                        rules += ",{ \"header\": \"Approval Time\", \"attribute\": \"ApprovalTime\", \"input\": false}";
                    }

                    rules += "] }";

                    // generate history data
                    var analysisResultHistories = new List<ListDictionary>();
                    dataQuery.ForEach(sample =>
                    {
                        var sampleHistory = analysisResultHistories.FirstOrDefault();
                        var userValidated = sample.ValidatedBy == null ? " " : db.Users.Find(sample.ValidatedBy).FullName;
                        var userApproved = sample.ApprovedBy == null ? " " : db.Users.Find(sample.ApprovedBy).FullName;
                        if (sampleHistory != null && !sampleHistory.Contains(sample.Name))
                        {
                            sampleHistory.Add(sample.Name, sample.Result);
                            DateTime convertedDate = DateTime.Parse($"{sampleHistory["Date"]} {sampleHistory["Time"]}");
                            if (convertedDate <= sample.CreatedOn)
                            {
                                sampleHistory["Date"] = sample.CreatedOn.ToString("yyyy-MM-dd");
                                sampleHistory["Time"] = sample.CreatedOn.ToString("HH:mm");
                            }
                        }
                        else
                        {
                            if (!analysisHistory.IsApproval)
                            {
                                analysisResultHistories.Add(new ListDictionary
                                {
                                    { "Type", "AVG" },
                                    { "Id", sample.Id.ToString() },
                                    { "Ident", sample.SampleId },
                                    { "ExtIdent", sample.ExternalId },
                                    { "Tonnage", sample.Tonnage.ToString() },
                                    { "Status", sample.Status.ToString() },
                                    { sample.Name, sample.Result },
                                    { "Date", sample.CreatedOn.ToString("yyyy-MM-dd") },
                                    { "Time", sample.CreatedOn.ToString("HH:mm") },
                                    { "ValidateBy", userValidated},
                                    { "ValidateTime", sample.ValidatedDate?.ToString("yyyy-MM-dd HH:mm:ss")}
                                });
                            }
                            else
                            {
                                analysisResultHistories.Add(new ListDictionary
                                {
                                    { "Type", "AVG" },
                                    { "Id", sample.Id.ToString() },
                                    { "Ident", sample.SampleId },
                                    { "ExtIdent", sample.ExternalId },
                                    { "Tonnage", sample.Tonnage.ToString() },
                                    { "Status", sample.Status.ToString() },
                                    { sample.Name, sample.Result },
                                    { "Date", sample.CreatedOn.ToString("yyyy-MM-dd") },
                                    { "Time", sample.CreatedOn.ToString("HH:mm") },
                                    { "ValidateBy", userValidated},
                                    { "ValidateTime", sample.ValidatedDate?.ToString("yyyy-MM-dd HH:mm:ss")},
                                    { "ApprovalBy", userApproved},
                                    { "ApprovalTime", sample.ApprovedDate?.ToString("yyyy-MM-dd HH:mm:ss")}
                                });
                            }

                        }
                    });

                    // Populate non updated values
                    for (int i = 0; i < analysisResultHistories.Count(); i++)
                    {
                        var newAnalysisResult = analysisResultHistories[i];
                        schemeNameList.ForEach(schemeName =>
                        {
                            if (!analysisResultHistories[i].Contains(schemeName) && i > 0 && analysisResultHistories[i - 1].Contains(schemeName))
                            {
                                newAnalysisResult.Add(schemeName, analysisResultHistories[i - 1][schemeName]);
                            }
                        });

                        results.Add(newAnalysisResult);
                    }
                }

                var result = new { Success = true, Permission = permission_Item, User = UserX.Information(user, is_Show_User_Menu, is_Show_User_Relation), Message = string.Empty, MessageDetail = string.Empty, Version = Configuration.VERSION, rules = rules, Items = results, Total = results.Count };
                return Json(result, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                var result = new { Success = false, Permission = permission_Item, ex.Message, MessageDetail = string.Empty, Version = Configuration.VERSION, Items = string.Empty, Total = 0 };
                return Json(result, JsonRequestBehavior.AllowGet);
            }
        }

        // POST: Api/AnalysisResultValidation/Validate
        public JsonResult Validate(List<int> sampleIds)
        {
            UserX user = new UserX(Request);
            Permission_Item permission_Item = null;

            Permission.Check_API(Request, user, ref permission_Item);
            if (!permission_Item.Is_Acknowledge)
            {
                var msg = new { Success = false, Permission = permission_Item, Message = Permission.ERROR_PERMISSION_READ, MessageDetail = string.Empty, Version = Configuration.VERSION };
                return Json(msg, JsonRequestBehavior.AllowGet);
            }

            bool is_Show_User_Menu = OurUtility.ValueOf(Request, "u_menu").Equals("1");
            bool is_Show_User_Relation = OurUtility.ValueOf(Request, "u_relation").Equals("1");

            using (MERCY_Ctx db = new MERCY_Ctx())
            {
                var transaction = db.Database.BeginTransaction();
                try
                {
                    sampleIds.ForEach(s =>
                    {
                        var sampleById = db.Samples.Find(s);
                        sampleById.Stage = "Validated";
                        sampleById.LastModifiedOn = DateTime.Now;
                        sampleById.LastModifiedBy = user.UserId;
                        sampleById.ValidatedBy = user.UserId;
                        sampleById.ValidatedDate = DateTime.Now;
                        db.SaveChanges();

                        var analysisJobId = sampleById.JobNumberId;

                        var isJobValidated = (
                            from aj in db.AnalysisJobs
                            join sm in db.Samples on aj.Id equals sm.JobNumberId
                            where aj.Id == analysisJobId
                            select sm
                        ).All(sm => sm.Stage == "Validated");

                        if (isJobValidated)
                        {
                            sampleById.AnalysisJob.Status = "Validated";
                            sampleById.AnalysisJob.ValidatedDate = DateTime.Now;
                            sampleById.AnalysisJob.ValidatedBy = user.UserId;
                            sampleById.AnalysisJob.LastModifiedAt = DateTime.Now;
                            sampleById.AnalysisJob.LastModifiedBy = user.UserId;

                        }
                    });

                    db.SaveChanges();
                    transaction.Commit();
                    var result = new { Success = true, Permission = permission_Item, User = UserX.Information(user, is_Show_User_Menu, is_Show_User_Relation), Message = string.Empty, MessageDetail = string.Empty, Version = Configuration.VERSION, };
                    return Json(result, JsonRequestBehavior.AllowGet);
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    transaction.Dispose();

                    var result = new { Success = false, Permission = permission_Item, ex.Message, MessageDetail = string.Empty, Version = Configuration.VERSION };
                    return Json(result, JsonRequestBehavior.AllowGet);
                }

            }
        }
        public JsonResult Approve(List<int> sampleIds)
        {

            UserX user = new UserX(Request);
            Permission_Item permission_Item = null;

            Permission.Check_API(Request, user, ref permission_Item);
            if (!permission_Item.Is_Approve)
            {
                var msg = new { Success = false, Permission = permission_Item, Message = Permission.ERROR_PERMISSION_READ, MessageDetail = string.Empty, Version = Configuration.VERSION };
                return Json(msg, JsonRequestBehavior.AllowGet);
            }

            bool is_Show_User_Menu = OurUtility.ValueOf(Request, "u_menu").Equals("1");
            bool is_Show_User_Relation = OurUtility.ValueOf(Request, "u_relation").Equals("1");

            using (MERCY_Ctx db = new MERCY_Ctx())
            {
                var transaction = db.Database.BeginTransaction();
                try
                {
                    List<string> arr = new List<string>();
                    sampleIds.ForEach(s =>
                    {
                        var sampleById = db.Samples.Find(s);

                        sampleById.Stage = "Approved";
                        sampleById.LastModifiedOn = DateTime.Now;
                        sampleById.LastModifiedBy = user.UserId;

                        db.SaveChanges();

                        var analysisJobId = sampleById.JobNumberId;
                        arr.Add(sampleById.ExternalId);

                        var isJobApproved = (
                            from aj in db.AnalysisJobs
                            join sm in db.Samples on aj.Id equals sm.JobNumberId
                            where aj.Id == analysisJobId
                            select sm
                           ).All(sm => sm.Stage == "Approved");

                        if (isJobApproved)
                        {
                            var isJobAlreadyApproved = sampleById.AnalysisJob.ApprovedDate != null;
                            sampleById.AnalysisJob.Status = "Approved";
                            sampleById.AnalysisJob.ApprovedBy = user.UserId;
                            sampleById.AnalysisJob.ApprovedDate = DateTime.Now;
                            sampleById.AnalysisJob.LastModifiedAt = DateTime.Now;
                            sampleById.AnalysisJob.LastModifiedBy = user.UserId;


                            var project = db.Projects.Find(sampleById.ProjectId);
                            var client = db.Clients.Find(sampleById.ClientId);
                            switch (client.Name)
                            {
                                case "Crushing":
                                    if (!UploadCrushingPlant(db, analysisJobId, user, isJobAlreadyApproved))
                                    {
                                        throw new Exception("Failed Upload Crushing Plant ");
                                    }
                                    break;
                                case "Geology":
                                    if (project.Name.Equals("Development"))
                                    {
                                        if (!UploadGeologyExploration(db, analysisJobId, user, isJobAlreadyApproved))
                                        {
                                            throw new Exception("Failed Upload Crushing Plant ");
                                        }
                                    }
                                    else if (project.Code.Equals("AMD"))
                                    {
                                        if(!UploadGeologyAMD(db, analysisJobId, user, isJobAlreadyApproved))
                                        {
                                            throw new Exception("Failed Upload Geology AMD");
                                        }
                                    }
                                    else
                                    {
                                        if (!UploadGeologyPitMonitoring(db, analysisJobId, user, isJobAlreadyApproved))
                                        {
                                            throw new Exception("Failed Upload Geology Pit Monitoring");
                                        }

                                    }
                                    break;
                                case "Loading":
                                    if (project.Name.Equals("Ship Loading"))
                                    {
                                        if (!UploadLoadingReport(db, analysisJobId, user, isJobAlreadyApproved))
                                        {
                                            throw new Exception("Failed Upload Vessel Loading Report ");
                                        }
                                    }
                                    else
                                    {
                                        if (!UploadBargeLoading(db, analysisJobId, user, isJobAlreadyApproved))
                                        {
                                            throw new Exception("Failed Upload Barge Loading");
                                        }
                                    }
                                    break;
                            }
                        }
                    });


                    db.SaveChanges();
                    transaction.Commit();
                    string msg = "Samples with ExternalIdx = [" + string.Join(", ", arr.ToArray()) + "] was Approved";
                    var result = new { Success = true, Permission = permission_Item, User = UserX.Information(user, is_Show_User_Menu, is_Show_User_Relation), Message = "Approval API", MessageDetail = msg, Version = Configuration.VERSION, };
                    return Json(result, JsonRequestBehavior.AllowGet);
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    transaction.Dispose();

                    var result = new { Success = false, Permission = permission_Item, ex.Message, MessageDetail = string.Empty, Version = Configuration.VERSION };
                    return Json(result, JsonRequestBehavior.AllowGet);
                }
            }
        }
        private bool UploadCrushingPlant(MERCY_Ctx db, int analysisJobId, User user, bool isJobAlreadyApproved)
        {
            try
            {
                var analysisJob = db.AnalysisJobs.Find(analysisJobId);
                var approvedBy = db.Users.Where(x => x.UserId == analysisJob.ApprovedBy).FirstOrDefault().FullName;
                var tempCPH = db.UPLOAD_CRUSHING_PLANT_Header.FirstOrDefault(x => x.Job_No == analysisJob.JobNumber);
                if (!isJobAlreadyApproved)
                {
                    tempCPH = new UPLOAD_CRUSHING_PLANT_Header
                    {
                        Job_No = analysisJob.JobNumber,
                        Date_Detail = "Date : " + analysisJob.JobDate.ToString("dd/MM/yyyy"),
                        Report_To = string.Empty,
                        Method1 = string.Empty,
                        Method2 = string.Empty,
                        Method3 = string.Empty,
                        Method4 = string.Empty,
                        Sheet = string.Empty,
                        Company = analysisJob.CompanyCode,
                        CreatedBy = user.UserId,
                        CreatedOn = DateTime.Now,
                        CreatedOn_Date_Only = DateTime.Now.ToString("yyyy-MM-dd"),
                        CreatedOn_Year_Only = DateTime.Now.Year,
                        ReportNumber = GenerateReportNumber(db, analysisJob),
                        DateReported = analysisJob.CreatedAt.ToString("dd/MM/yyyy"),
                        ApprovedBy = approvedBy
                    };
                    db.UPLOAD_CRUSHING_PLANT_Header.Add(tempCPH);
                    db.SaveChanges();
                }

                analysisJob.Samples.ToList().ForEach(sample =>
                {
                    AddOrUpdateCrushingPlant(sample, db, tempCPH.RecordId, user);
                    db.SaveChanges();
                });

                return true;
            }
            catch
            {
                return false;
            }
        }
        private bool UploadGeologyExploration(MERCY_Ctx db, int analysisJobId, User user, bool isJobAlreadyApproved)
        {
            try
            {
                var analysisJob = db.AnalysisJobs.Find(analysisJobId);

                UPLOAD_Geology_Explorasi_Header tempCPH = db.UPLOAD_Geology_Explorasi_Header.Where(geh => geh.Job_No == analysisJob.JobNumber).FirstOrDefault();
                if (!isJobAlreadyApproved)
                {
                    tempCPH = new UPLOAD_Geology_Explorasi_Header
                    {
                        Job_No = analysisJob.JobNumber,
                        Date_Detail = "Date : " + analysisJob.JobDate.ToString("dd/MM/yyyy"),
                        Report_To = string.Empty,
                        Company = analysisJob.CompanyCode,
                        CreatedBy = user.UserId,
                        CreatedOn = DateTime.Now,
                        CreatedOn_Year_Only = DateTime.Now.Year,
                        CreatedOn_Date_Only = DateTime.Now.ToString("yyyy-MM-dd"),
                        Date_Received = analysisJob.ReceivedDate?.ToString("MMMM dd yyyy"),
                        Nomor = GenerateReportNumber(db, analysisJob)
                    };

                    db.UPLOAD_Geology_Explorasi_Header.Add(tempCPH);
                    db.SaveChanges();
                }

                analysisJob.Samples.ToList().ForEach(sample =>
                {
                    var sampleDG = sample.SampleDetailGenerals.OfType<SampleDetailGeneral>().FirstOrDefault();
                    var analysResult = db.AnalysisResults.Where(x => x.SampleScheme.SampleId == sample.Id).OrderBy(x => x.CreatedOn).FirstOrDefault();
                    var calculateSample = GetCalculateSample(db, sample.Id);
                    UPLOAD_Geology_Explorasi tempCP = db.UPLOAD_Geology_Explorasi.Where(ge => ge.Header == tempCPH.RecordId && ge.Sample_ID == sampleDG.SampleId).FirstOrDefault();
                    if (tempCP == null)
                    {
                        tempCP = new UPLOAD_Geology_Explorasi();
                    }

                    tempCP.Header = tempCPH.RecordId;
                    tempCP.SampleType = "COAL";
                    tempCP.Sheet = string.Empty;
                    tempCP.Lab_ID = sample.ExternalId;
                    tempCP.LECO_Stamp = string.Empty;
                    tempCP.TM = calculateSample.TM;
                    tempCP.Sample_ID = sampleDG.SampleId;
                    tempCP.TS = calculateSample.TS;
                    tempCP.Cal_ad = calculateSample.CV;
                    tempCP.Ash = calculateSample.ASH;
                    tempCP.FC = sample.CompanyCode == "IMM" ? calculateSample.FC : calculateSample.TGA;
                    tempCP.VM = calculateSample.VM;
                    tempCP.M = calculateSample.IM;
                    tempCP.Cal_db = calculateSample.CV * (100 / (100 - calculateSample.IM));
                    tempCP.Cal_daf = calculateSample.CV * (100 / (100 - calculateSample.IM - tempCP.Ash));
                    tempCP.Cal_ar = calculateSample.CV * ((100 - calculateSample.TM) / (100 - calculateSample.IM));
                    tempCP.Na2O = calculateSample.Na2O;
                    tempCP.CaO = calculateSample.CaO;
                    tempCP.RD = calculateSample.RD;
                    tempCP.Company = sample.CompanyCode;
                    tempCP.CreatedBy = user.UserId;
                    tempCP.CreatedOn = DateTime.Now;
                    tempCP.CreatedOn_Date_Only = DateTime.Now.ToString("yyyy-MM-dd");
                    tempCP.CreatedOn_Year_Only = DateTime.Now.Year;
                    tempCP.DateReceived = sampleDG.Receive;
                    tempCP.DateAnalysis = analysResult.CreatedOn;
                    tempCP.ThicknessFrom = sample.ThicknessFrom;
                    tempCP.ThicknessTo = sample.ThicknessTo;

                    if (!isJobAlreadyApproved)
                    {
                        db.UPLOAD_Geology_Explorasi.Add(tempCP);
                    }

                    db.SaveChanges();
                });

                return true;
            }
            catch
            {
                return false;
            }
        }
        private bool UploadGeologyPitMonitoring(MERCY_Ctx db, int analysisJobId, User user, bool isJobAlreadyApproved)
        {
            try
            {
                var analysisJob = db.AnalysisJobs.Find(analysisJobId);
                var project = db.Projects.Find(analysisJob.ProjectId);
                UPLOAD_Geology_Pit_Monitoring_Header tempCPH = db.UPLOAD_Geology_Pit_Monitoring_Header.Where(geh => geh.Job_No == analysisJob.JobNumber).FirstOrDefault();
                if (!isJobAlreadyApproved)
                {
                    tempCPH = new UPLOAD_Geology_Pit_Monitoring_Header
                    {
                        Job_No = analysisJob.JobNumber,
                        Date_Detail = "Date : " + analysisJob.JobDate.ToString("dd/MM/yyyy"),
                        Report_To = string.Empty,
                        Company = analysisJob.CompanyCode,
                        CreatedBy = user.UserId,
                        CreatedOn = DateTime.Now,
                        CreatedOn_Year_Only = DateTime.Now.Year,
                        CreatedOn_Date_Only = DateTime.Now.ToString("yyyy-MM-dd"),
                        Date_Received = analysisJob.ReceivedDate?.ToString("MMMM dd yyyy"),
                        Nomor = GenerateReportNumber(db, analysisJob)
                    };

                    db.UPLOAD_Geology_Pit_Monitoring_Header.Add(tempCPH);
                    db.SaveChanges();
                }

                analysisJob.Samples.ToList().ForEach(sample =>
                {
                    var sampleDG = sample.SampleDetailGenerals.OfType<SampleDetailGeneral>().FirstOrDefault();
                    var analysResult = db.AnalysisResults.Where(x => x.SampleScheme.SampleId == sample.Id).OrderBy(x => x.CreatedOn).FirstOrDefault();
                    var calculateSample = GetCalculateSample(db, sample.Id);
                    UPLOAD_Geology_Pit_Monitoring tempCP = db.UPLOAD_Geology_Pit_Monitoring.Where(ge => ge.Header == tempCPH.RecordId && ge.Sample_ID == sampleDG.SampleId).FirstOrDefault();
                    if (tempCP == null)
                    {
                        tempCP = new UPLOAD_Geology_Pit_Monitoring();
                    }

                    tempCP.Header = tempCPH.RecordId;
                    tempCP.SampleType = "COAL";
                    tempCP.Sheet = string.Empty;
                    tempCP.Lab_ID = sample.ExternalId;
                    tempCP.LECO_Stamp = string.Empty;
                    tempCP.TM = calculateSample.TM;
                    tempCP.Sample_ID = sampleDG.SampleId;
                    tempCP.TS = calculateSample.TS;
                    tempCP.Cal_ad = calculateSample.CV;
                    tempCP.Ash = calculateSample.ASH;
                    tempCP.FC = sample.CompanyCode == "IMM" ? calculateSample.FC : calculateSample.TGA;
                    tempCP.VM = calculateSample.VM;
                    tempCP.M = calculateSample.IM;
                    tempCP.Cal_db = calculateSample.CV * (100 / (100 - calculateSample.IM));
                    tempCP.Cal_daf = calculateSample.CV * (100 / (100 - calculateSample.IM - (tempCP.Ash.Equals(null) ? tempCP.Ash : 0)));
                    tempCP.Cal_ar = calculateSample.CV * ((100 - calculateSample.TM) / (100 - calculateSample.IM));
                    tempCP.Na2O = calculateSample.Na2O;
                    tempCP.CaO = calculateSample.CaO;
                    tempCP.RD = calculateSample.RD;
                    tempCP.Company = sample.CompanyCode;
                    tempCP.CreatedBy = user.UserId;
                    tempCP.CreatedOn = DateTime.Now;
                    tempCP.CreatedOn_Date_Only = DateTime.Now.ToString("yyyy-MM-dd");
                    tempCP.CreatedOn_Year_Only = DateTime.Now.Year;
                    tempCP.DateReceived = sampleDG.Receive;
                    tempCP.DateAnalysis = analysResult.CreatedOn;
                    tempCP.ThicknessFrom = sample.ThicknessFrom;
                    tempCP.ThicknessTo = sample.ThicknessTo;

                    if (!isJobAlreadyApproved)
                    {
                        db.UPLOAD_Geology_Pit_Monitoring.Add(tempCP);
                    }

                    db.SaveChanges();
                });

                return true;
            }
            catch
            {
                return false;
            }
        }
        private bool UploadGeologyAMD(MERCY_Ctx db, int analysisJobId, User user, bool isJobAlreadyApproved)
        {
            try
            {
                Configuration config = new Configuration();
                var PORNumber = OurUtility.ToDecimal(config.PYRATE_OXIDATION_REACTION_NUMBER);
                var analysisJob = db.AnalysisJobs.Find(analysisJobId);
                var project = db.Projects.Find(analysisJob.ProjectId);
                var dateDetail = analysisJob.JobDate.ToString("MMMM yyyy");
                var tempGAH = db.UPLOAD_Geology_AMD_Header.Where(ga => ga.Job_No.Contains(project.Code) && ga.Date_Detail == dateDetail).FirstOrDefault();
                if (!isJobAlreadyApproved)
                {
                    var JobNo = $"{analysisJob.CompanyCode}.{project.Code}.{analysisJob.JobDate.Day}/{analysisJob.JobDate:MMMM}/{analysisJob.JobDate.Year}";
                    var Nomor = $"{analysisJob.CompanyCode}/GEO/{project.Code}/{analysisJob.JobDate.Year}/{OurUtility.ToRoman(analysisJob.JobDate.Month)}/{analysisJob.JobDate:dd}";
                    tempGAH = new UPLOAD_Geology_AMD_Header
                    {
                        Nomor = Nomor,
                        Job_No = JobNo,
                        Report_To = string.Empty,
                        Date_Received = analysisJob.ReceivedDate?.ToString("MMMM dd, yyyy"),
                        Date_Detail = $"Date: {analysisJob.JobDate:dd MMMM, yyyy}",
                        CompanyCode = analysisJob.CompanyCode,
                        CreatedBy = user.UserId,
                        CreatedOn = DateTime.Now,
                    };
                    db.UPLOAD_Geology_AMD_Header.Add(tempGAH);
                    db.SaveChanges();
                }

                analysisJob.Samples.ToList().ForEach(sample =>
                {
                    var sampleDA = sample.SampleDetailAMDs.OfType<SampleDetailAMD>().FirstOrDefault();
                    var analysResult = db.AnalysisResults.Where(x => x.SampleScheme.SampleId == sample.Id).OrderBy(x => x.CreatedOn).FirstOrDefault();
                    UPLOAD_Geology_AMD tempAMD = db.UPLOAD_Geology_AMD.Where(ge => ge.Header == tempGAH.RecordId && ge.Sample_Id == sampleDA.Sample_Id).FirstOrDefault();
                    if (tempAMD == null)
                    {
                        tempAMD = new UPLOAD_Geology_AMD();
                    }

                    tempAMD.SampleId = sampleDA.SampleId;
                    tempAMD.SampleType = sampleDA.SampleType;
                    tempAMD.LaboratoryId = sampleDA.LaboratoryId;
                    tempAMD.MassSampleReceived = sampleDA.MassSampleReceived;
                    tempAMD.TS = sampleDA.TS;
                    tempAMD.MPA = PORNumber * sampleDA.TS;
                    tempAMD.ANC = sampleDA.ANC;
                    tempAMD.NAPP = (PORNumber * sampleDA.TS) - sampleDA.ANC;
                    tempAMD.NAG = sampleDA.NAG;
                    tempAMD.NAGPH45 = sampleDA.NAGPH45;
                    tempAMD.NAGPH70 = sampleDA.NAGPH70;
                    tempAMD.NAGType = sampleDA.NAGType;
                    tempAMD.DateReceived = sampleDA.Receive;
                    tempAMD.Header = tempGAH.RecordId;
                    tempAMD.Sample_Id = sampleDA.Sample_Id;
                    tempAMD.CompanyCode = analysisJob.CompanyCode;
                    tempAMD.CreatedOn = DateTime.Now;
                    tempAMD.CreatedBy = user.UserId;

                    if (!isJobAlreadyApproved)
                    {
                        db.UPLOAD_Geology_AMD.Add(tempAMD);
                    }
                    db.SaveChanges();

                });


                return true;
            }catch
            {
                return false;
            }
        }
        private bool UploadBargeLoading(MERCY_Ctx db, int analysisJobId, User user, bool isJobAlreadyApproved)
        {
            try
            {
                var analysisJob = db.AnalysisJobs.Find(analysisJobId);
                var project = db.Projects.Find(analysisJob.ProjectId);
                var dateDetail = analysisJob.JobDate.ToString("MMMM yyyy");

                var tempBLH = db.UPLOAD_BARGE_LOADING_Header.Where(blh => blh.Job_No.Contains(project.Code) && blh.Date_Detail == dateDetail).FirstOrDefault();
                if (!isJobAlreadyApproved)
                {
                    tempBLH = new UPLOAD_BARGE_LOADING_Header
                    {
                        Job_No = GenerateReportNumber(db, analysisJob),
                        Date_Detail = dateDetail,
                        Report_To = string.Empty,
                        Method1 = string.Empty,
                        Method2 = string.Empty,
                        Method3 = string.Empty,
                        Method4 = string.Empty,
                        Sheet = string.Empty,
                        Company = analysisJob.CompanyCode,
                        CreatedBy = user.UserId,
                        CreatedOn = DateTime.Now,
                        CreatedOn_Date_Only = DateTime.Now.ToString("yyyy-MM-dd"),
                        CreatedOn_Year_Only = DateTime.Now.Year
                    };
                    db.UPLOAD_BARGE_LOADING_Header.Add(tempBLH);
                    db.SaveChanges();
                }

                analysisJob.Samples.ToList().ForEach(sample =>
                {
                    var sampleDG = sample.SampleDetailGenerals.OfType<SampleDetailGeneral>().FirstOrDefault();
                    var analysResult = db.AnalysisResults.Where(x => x.SampleScheme.SampleId == sample.Id).OrderBy(x => x.CreatedOn).FirstOrDefault();
                    var calculateSample = GetCalculateSample(db, sample.Id);
                    var arbMultiplier = (100 - calculateSample.TM) / (100 - calculateSample.IM);

                    var tempBL = db.UPLOAD_BARGE_LOADING.Where(bl => bl.Header == tempBLH.RecordId && bl.ID_Number == sampleDG.SampleId).FirstOrDefault();
                    if (tempBL == null)
                    {
                        tempBL = new UPLOAD_BARGE_LOADING();
                    }

                    tempBL.Header = tempBLH.RecordId;
                    tempBL.Sheet = string.Empty;
                    tempBL.JOB_Number = sample.AnalysisJob.JobNumber;
                    tempBL.ID_Number = sampleDG.SampleId;
                    tempBL.Name = sampleDG.BargeName;
                    tempBL.Service_Trip_Number = sampleDG.TripNumber;
                    tempBL.Destination = sampleDG.Destination ?? string.Empty;
                    tempBL.Temperature = 0;
                    tempBL.Date_Sampling = sampleDG.DateSampleEnd.ToString("yyyy-MM-dd");
                    tempBL.Date_Report = sampleDG.Receive.ToString("yyyy-MM-dd");
                    tempBL.Tonnage = sampleDG.Tonnage;
                    tempBL.LECO_Stamp = string.Empty;
                    tempBL.TM = calculateSample.TM;
                    tempBL.TS_adb = calculateSample.TS;
                    tempBL.TS_arb = calculateSample.TS * arbMultiplier;
                    tempBL.CV_adb = calculateSample.CV;
                    tempBL.CV_daf = calculateSample.CV * (100 / (100 - calculateSample.IM - tempBL.ASH_adb));
                    tempBL.CV_arb = calculateSample.CV * arbMultiplier;
                    tempBL.CV_ad_15 = calculateSample.CV * (100 - 15) / (100 - calculateSample.IM);
                    tempBL.CV_ad_16 = calculateSample.CV * (100 - 16) / (100 - calculateSample.IM);
                    tempBL.CV_ad_17 = calculateSample.CV * (100 - 17) / (100 - calculateSample.IM);
                    tempBL.ASH_adb = calculateSample.ASH;
                    tempBL.ASH_arb = calculateSample.ASH * arbMultiplier;
                    tempBL.FC_adb = sample.CompanyCode == "IMM" ? calculateSample.FC : calculateSample.TGA;
                    tempBL.FC_arb = (sample.CompanyCode == "IMM" ? calculateSample.FC : calculateSample.TGA) * arbMultiplier;
                    tempBL.VM_adb = calculateSample.VM;
                    tempBL.VM_arb = calculateSample.VM * arbMultiplier;
                    tempBL.M = calculateSample.IM;
                    tempBL.CV_db = calculateSample.CV * (100 / (100 - calculateSample.IM));
                    tempBL.Remark = "Sample sudah teranalisa";
                    tempBL.Company = sample.CompanyCode;
                    tempBL.CreatedBy = user.UserId;
                    tempBL.CreatedOn = DateTime.Now;
                    tempBL.CreatedOn_Date_Only = DateTime.Now.ToString("yyyy-MM-dd");
                    tempBL.CreatedOn_Year_Only = DateTime.Now.Year;

                    if (!isJobAlreadyApproved)
                    {
                        db.UPLOAD_BARGE_LOADING.Add(tempBL);
                    }

                    db.SaveChanges();
                });

                return true;
            }
            catch
            {
                return false;
            }
        }
        private bool UploadLoadingReport(MERCY_Ctx db, int analysisJobId, User user, bool isJobAlreadyApproved)
        {
            try
            {
                var analysisJob = db.AnalysisJobs.Find(analysisJobId);
                var project = db.Projects.Find(analysisJob.ProjectId);

                var loadingReportHeader = db.LoadingReportHeaders.Where(lrh => lrh.ReportNumber.Contains(project.Code) && lrh.ReportedDate == analysisJob.JobDate).FirstOrDefault();
                if (!isJobAlreadyApproved)
                {
                    loadingReportHeader = new LoadingReportHeader
                    {
                        ReportNumber = GenerateReportNumber(db, analysisJob),
                        ReportTo = string.Empty,
                        ReportedDate = analysisJob.JobDate,
                        StandardMethod = string.Empty, // TODO: update it from custom report header
                        CompanyCode = analysisJob.CompanyCode,
                        CreatedBy = user.UserId,
                        CreatedOn = DateTime.Now,
                        CreatedOnDate = DateTime.Now.ToString("yyyy-MM-dd"),
                        CreatedOnYear = DateTime.Now.Year
                    };

                    db.LoadingReportHeaders.Add(loadingReportHeader);
                    db.SaveChanges();
                }

                analysisJob.Samples.ToList().ForEach(sample =>
                {
                    var sampleDL = sample.SampleDetailLoadings.OfType<SampleDetailLoading>().FirstOrDefault();
                    var refType = db.RefTypes.Find(sample.RefTypeId);
                    var analysResult = db.AnalysisResults.Where(x => x.SampleScheme.SampleId == sample.Id).OrderBy(x => x.CreatedOn).FirstOrDefault();
                    var calculateSample = GetCalculateSample(db, sample.Id);

                    var loadingReport = db.LoadingReports.Where(lr => lr.HeaderId == loadingReportHeader.Id && lr.LotNumber == sampleDL.LotNumber).FirstOrDefault();
                    if (loadingReport == null)
                    {
                        loadingReport = new LoadingReport();
                    }

                    loadingReport.HeaderId = loadingReportHeader.Id;
                    loadingReport.VesselName = sampleDL.VesselName;
                    loadingReport.LoadingNumber = sampleDL.LoadingNumber;
                    loadingReport.LotNumber = sampleDL.LotNumber;
                    loadingReport.SamplingDateStart = sampleDL.SamplingStart;
                    loadingReport.SamplingDateEnd = sampleDL.SamplingEnd;
                    loadingReport.AnalysisDateStart = analysResult.CreatedOn;
                    loadingReport.AnalysisDateEnd = sample.AnalysisJob.ValidatedDate.Value;
                    loadingReport.Tonnage = sampleDL.Tonnage;
                    loadingReport.TM = calculateSample.TM;
                    loadingReport.TS = calculateSample.TS;
                    loadingReport.CVAdb = calculateSample.CV;
                    loadingReport.ASH = calculateSample.ASH;
                    loadingReport.FC = sample.CompanyCode == "IMM" ? calculateSample.FC : calculateSample.TGA;
                    loadingReport.VM = calculateSample.VM;
                    loadingReport.IM = calculateSample.IM;
                    loadingReport.CVDb = calculateSample.CV * (100 / (100 - calculateSample.IM));
                    loadingReport.CVDaf = calculateSample.CV * (100 / (100 - calculateSample.IM - calculateSample.ASH));
                    loadingReport.CVArb = calculateSample.CV * ((100 - calculateSample.TM) - (100 - calculateSample.IM));
                    loadingReport.Size50 = calculateSample.Size50;
                    loadingReport.Size502 = calculateSample.Size502;
                    loadingReport.Size2 = calculateSample.Size2;
                    loadingReport.CreatedBy = user.UserId;
                    loadingReport.CreatedOn = DateTime.Now;
                    loadingReport.CreatedOnDate = DateTime.Now.ToString("yyyy-MM-dd");
                    loadingReport.CreatedOnYear = DateTime.Now.Year;

                    if (!isJobAlreadyApproved)
                    {
                        db.LoadingReports.Add(loadingReport);
                    }

                    db.SaveChanges();
                });

                return true;
            }
            catch
            {
                return false;
            }
        }
        private CalculateSample GetCalculateSample(MERCY_Ctx db, int sampleId)
        {
            try
            {
                CalculateSample result = new CalculateSample();
                var sample = db.Samples.Find(sampleId);
                var analysisResults = db.AnalysisResults.Where(x => x.SampleScheme.SampleId == sampleId).ToList();
                var CV = analysisResults.Where(x => x.SampleScheme.Scheme.Name == "CV").FirstOrDefault()?.Result ?? 0;
                var IM = analysisResults.Where(x => x.SampleScheme.Scheme.Name == "IM").FirstOrDefault()?.Result ?? 0;
                var TS = analysisResults.Where(x => x.SampleScheme.Scheme.Name == "TS").FirstOrDefault()?.Result ?? 0;
                var TM = analysisResults.Where(x => x.SampleScheme.Scheme.Name == "TM").FirstOrDefault()?.Result ?? 0;
                var ASH = analysisResults.Where(x => x.SampleScheme.Scheme.Name == "ASH").FirstOrDefault()?.Result ?? 0;
                var M = analysisResults.Where(x => x.SampleScheme.Scheme.Name == "M").FirstOrDefault()?.Result ?? 0;
                var FC = analysisResults.Where(x => x.SampleScheme.Scheme.Name == "FC").FirstOrDefault()?.Result ?? 0;
                var TGA = analysisResults.Where(x => x.SampleScheme.Scheme.Name == "TGA").FirstOrDefault()?.Result ?? 0;
                var VM = analysisResults.Where(x => x.SampleScheme.Scheme.Name == "VM").FirstOrDefault()?.Result ?? 0;
                var CaO = analysisResults.Where(x => x.SampleScheme.Scheme.Name == "AA CaO").FirstOrDefault()?.Result ?? 0;
                var Na2O = analysisResults.Where(x => x.SampleScheme.Scheme.Name == "AA Na2O").FirstOrDefault()?.Result ?? 0;
                var RD = analysisResults.Where(x => x.SampleScheme.Scheme.Name == "RD").FirstOrDefault()?.Result ?? 0;
                var size = analysisResults.Where(x => x.SampleScheme.Scheme.Name == "SIZE").FirstOrDefault();

                if (size != null)
                {
                    SizeSchemeAttributeModelView sizeSchemeAttribute = JsonConvert.DeserializeObject<SizeSchemeAttributeModelView>(size.Details);
                    result.Size50 = (sizeSchemeAttribute.A / sizeSchemeAttribute.M1) * 100;
                    result.Size502 = (sizeSchemeAttribute.B / sizeSchemeAttribute.M1) * 100;
                    result.Size2 = (sizeSchemeAttribute.C / sizeSchemeAttribute.M1) * 100;
                }

                result.CV = CV;
                result.TM = TM;
                result.IM = IM;
                result.TS = TS;
                result.ASH = ASH;
                result.M = M;
                result.Na2O = Na2O;
                result.VM = VM;
                result.CaO = CaO;
                result.FC = FC;
                result.RD = RD;
                result.TGA = TGA;
                return result;
            }
            catch
            {
                return null;
            }
        }
        private string GenerateReportNumber(MERCY_Ctx db, AnalysisJob analysisJob)
        {
            try
            {
                var companyClientProjectScheme = db.CompanyClientProjectSchemes
                    .Where(ccps => ccps.ProjectId == analysisJob.ProjectId)
                    .Where(ccps => ccps.CompanyCode == analysisJob.CompanyCode)
                    .FirstOrDefault();
                var client = db.Clients.Find(companyClientProjectScheme.ClientId);
                var project = db.Projects.Find(analysisJob.ProjectId);
                var reportNumber = 1;
                var reportNumberSuffix = $"/LAB-{project.Code}/{OurUtility.ToRoman(analysisJob.JobDate.Month)}/{analysisJob.JobDate.Year}";

                switch (client.Name)
                {
                    case "Geology":
                        if (project.Name == "Development" || project.Name.Contains("Exploration"))
                        {
                            var lastGeologyReportHeader = db.UPLOAD_Geology_Explorasi_Header
                                .Where(ge => ge.Nomor.Contains(reportNumberSuffix))
                                .OrderByDescending(ge => ge.Nomor)
                                .FirstOrDefault();
                            if (lastGeologyReportHeader != null)
                            {
                                reportNumber = int.Parse(lastGeologyReportHeader.Nomor.Substring(0, 3)) + 1;
                            }
                        }
                        else
                        {
                            var lastGeologyReportHeader = db.UPLOAD_Geology_Pit_Monitoring_Header
                                .Where(ge => ge.Nomor.Contains(reportNumberSuffix))
                                .OrderByDescending(ge => ge.Nomor)
                                .FirstOrDefault();
                            if (lastGeologyReportHeader != null)
                            {
                                reportNumber = int.Parse(lastGeologyReportHeader.Nomor.Substring(0, 3)) + 1;
                            }
                        }

                        break;
                    case "Loading":
                        if (project.Name == "Ship Loading")
                        {
                            var lastLoadingReportHeader = db.LoadingReportHeaders
                                .Where(lrh => lrh.ReportNumber.Contains(reportNumberSuffix))
                                .OrderByDescending(lrh => lrh.ReportNumber)
                                .FirstOrDefault();

                            if (lastLoadingReportHeader != null)
                            {
                                reportNumber = int.Parse(lastLoadingReportHeader.ReportNumber.Substring(0, 3)) + 1;
                            }
                        }
                        else
                        {

                        }
                        break;
                }

                return $"{reportNumber.ToString().PadLeft(3, '0')}{reportNumberSuffix}";
            }
            catch
            {
                return string.Empty;
            }
        }
        private bool AddOrUpdateCrushingPlant(Sample sample, MERCY_Ctx db, long header, User user)
        {
            var sampleDG = sample.SampleDetailGenerals.OfType<SampleDetailGeneral>().FirstOrDefault();
            var analysResult = db.AnalysisResults.Where(x => x.SampleScheme.SampleId == sample.Id).OrderBy(x => x.CreatedOn).FirstOrDefault();
            var calculateSample = GetCalculateSample(db, sample.Id);
            var arbMultiplier = (100 - calculateSample.TM) / (100 - calculateSample.IM);
            var uploadCrushingPlant = db.UPLOAD_CRUSHING_PLANT.FirstOrDefault(x => x.Sample_ID == sampleDG.SampleId && x.Header == header);
            if (uploadCrushingPlant == null)
            {
                uploadCrushingPlant = new UPLOAD_CRUSHING_PLANT
                {
                    Header = header,
                    Date_Production = DateTime.Now.ToString("yyyy-MM-dd"),
                    Sheet = string.Empty,
                    Shift_Work = OurUtility.ToRoman(sampleDG.Shift),
                    Tonnage = sampleDG.Tonnage,
                    LECO_Stamp = string.Empty,
                    Sample_ID = sampleDG.SampleId,
                    Remark = sample?.Tunnel?.Name ?? string.Empty,
                    Company = sample.CompanyCode,
                    SamplingEnd = sampleDG.DateSampleEnd,
                    SamplingStart = sampleDG.DateSampleStart,
                    AnalysisStart = analysResult.CreatedOn,
                    AnalysisEnd = sample.AnalysisJob.ValidatedDate,
                    CreatedBy = user.UserId,
                    CreatedOn = DateTime.Now,
                    CreatedOn_Date_Only = DateTime.Now.ToString("yyyy-MM-dd"),
                    CreatedOn_Year_Only = DateTime.Now.Year
                };

                db.UPLOAD_CRUSHING_PLANT.Add(uploadCrushingPlant);
            }

            uploadCrushingPlant.TM = calculateSample.TM;
            uploadCrushingPlant.TS_adb = calculateSample.TS;
            uploadCrushingPlant.TS_arb = calculateSample.TS * arbMultiplier;
            uploadCrushingPlant.CV_adb = calculateSample.CV;
            uploadCrushingPlant.ASH_adb = calculateSample.ASH;
            uploadCrushingPlant.ASH_arb = calculateSample.ASH * arbMultiplier;
            uploadCrushingPlant.FC_adb = sample.CompanyCode == "IMM" ? calculateSample.FC : calculateSample.TGA;
            uploadCrushingPlant.VM_adb = calculateSample.VM;
            uploadCrushingPlant.VM_arb = calculateSample.VM * arbMultiplier;
            uploadCrushingPlant.M = calculateSample.IM;
            uploadCrushingPlant.CV_db = calculateSample.CV * (100 / (100 - calculateSample.IM));
            uploadCrushingPlant.FC_arb = uploadCrushingPlant.FC_adb * arbMultiplier;
            uploadCrushingPlant.CV_daf = calculateSample.CV * (100 / (100 - calculateSample.IM - uploadCrushingPlant.ASH_adb));
            uploadCrushingPlant.CV_arb = calculateSample.CV * arbMultiplier;
            uploadCrushingPlant.CV_ad_15 = calculateSample.CV * (100 - 15) / (100 - calculateSample.IM);
            uploadCrushingPlant.CV_ad_16 = calculateSample.CV * (100 - 16) / (100 - calculateSample.IM);
            uploadCrushingPlant.CV_ad_17 = calculateSample.CV * (100 - 17) / (100 - calculateSample.IM);

            return true;
        }
    }
}