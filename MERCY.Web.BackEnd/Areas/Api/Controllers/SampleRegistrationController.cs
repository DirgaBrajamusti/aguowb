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
using MERCY.Data.EntityFramework_MineMarket;
using Newtonsoft.Json;
using System.Threading.Tasks;
using System.Data.Entity;
using DocumentFormat.OpenXml.Wordprocessing;
using System.Collections;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Linq.Expressions;

namespace MERCY.Web.BackEnd.Areas.Api.Controllers
{
    public class SampleRegistrationController : Controller
    {
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
            DateTime dateFrom = DateTime.Now;
            DateTime dateTo = DateTime.Now;
            bool is_Show_User_Menu = OurUtility.ValueOf(Request, "u_menu").Equals("1");
            bool is_Show_User_Relation = OurUtility.ValueOf(Request, "u_relation").Equals("1");
            bool all_company = OurUtility.ValueOf(Request, "company").Equals(string.Empty);
            string company_code = OurUtility.ValueOf(Request, "company");
            bool all_site = OurUtility.ValueOf(Request, "site").Equals(string.Empty);
            int site_id = OurUtility.ToInt32(OurUtility.ValueOf(Request, "site"));
            bool all_client = OurUtility.ValueOf(Request, "client").Equals(string.Empty);
            int client_id = OurUtility.ToInt32(OurUtility.ValueOf(Request, "client"));
            bool all_project = OurUtility.ValueOf(Request, "project").Equals(string.Empty);
            int project_id = OurUtility.ToInt32(OurUtility.ValueOf(Request, "project"));
            bool all_dateofjob = (OurUtility.ValueOf(Request, "dateFrom").Equals(string.Empty) && OurUtility.ValueOf(Request, "dateTo").Equals(string.Empty));
            bool all_search = OurUtility.ValueOf(Request, "search").Equals(string.Empty);
            string search = OurUtility.ValueOf(Request, "search");
            int pageSize = OurUtility.ValueOf(Request, "pageSize").Equals(string.Empty) ? 10 : OurUtility.ToInt32(OurUtility.ValueOf(Request, "pageSize"));
            int page = OurUtility.ValueOf(Request, "page").Equals(string.Empty) ? 1 : OurUtility.ToInt32(OurUtility.ValueOf(Request, "page"));
            string sortColumn = OurUtility.ValueOf(Request, "sortColumn").Equals(string.Empty) ? "DateSampleStart" : OurUtility.ValueOf(Request, "sortColumn");
            string sortBy = OurUtility.ValueOf(Request, "sortBy").Equals(string.Empty) ? "DESC" : OurUtility.ValueOf(Request, "sortBy");

            if (!all_dateofjob)
            {
                dateFrom = DateTime.Parse(OurUtility.ValueOf(Request, "dateFrom"));
                dateTo = DateTime.Parse(OurUtility.ValueOf(Request, "dateTo")).AddHours(23).AddMinutes(59);
            }

            try
            {
                using (MERCY_Ctx db = new MERCY_Ctx())
                {
                    var dataQuery =
                        (
                            from d in db.Samples
                            join s in db.Sites on d.SiteId equals s.SiteId
                            join c in db.Clients on d.ClientId equals c.Id
                            join p in db.Projects on d.ProjectId equals p.Id
                            join jn in db.AnalysisJobs on d.JobNumberId equals jn.Id
                            join sdg in db.SampleDetailGenerals on d.Id equals sdg.Sample_Id
                            where (all_company || d.CompanyCode == company_code)
                                && (all_site || d.SiteId == site_id)
                                && (all_client || d.ClientId == client_id)
                                && (all_project || d.ProjectId == project_id)
                                && (all_dateofjob || (d.DateOfJob >= dateFrom && d.DateOfJob <= dateTo))
                                && (all_search || d.ExternalId.Contains(search))
                                
                                && d.DeletedBy == null
                            orderby d.Id
                            select new Model_View_Sample
                            {
                                Id = d.Id,
                                CompanyCode = d.CompanyCode,
                                SiteName = s.SiteName,
                                ExternalId = d.ExternalId,
                                SampleId = sdg.SampleId,
                                ClientId = c.Id,
                                ClientName = c.Name,
                                ProjectId = p.Id,
                                ProjectName = p.Name,
                                ProjectType = p.Type,
                                DateOfJob = d.DateOfJob,
                                Stage = d.Stage,
                                DateSampleEnd = sdg.DateSampleEnd,
                                DateSampleStart = sdg.DateSampleStart,
                                Tonnage = sdg.Tonnage,
                                Shift = sdg.Shift,
                                Receive = sdg.Receive,
                                JobName = jn.JobNumber,
                                IsActive = d.IsActive,
                                TunnelId = d.TunnelId,
                                Tunnel = d.Tunnel
                            }
                        );
                    var dataQuery2 =
                        (
                            from d in db.Samples
                            join s in db.Sites on d.SiteId equals s.SiteId
                            join c in db.Clients on d.ClientId equals c.Id
                            join p in db.Projects on d.ProjectId equals p.Id
                            join jn in db.AnalysisJobs on d.JobNumberId equals jn.Id
                            join sdl in db.SampleDetailLoadings on d.Id equals sdl.SampleId
                            where (all_company || d.CompanyCode == company_code)
                                && (all_site || d.SiteId == site_id)
                                && (all_client || d.ClientId == client_id)
                                && (all_project || d.ProjectId == project_id)
                                && (all_dateofjob || (d.DateOfJob >= dateFrom && d.DateOfJob <= dateTo))
                                && (all_search || d.ExternalId.Contains(search))
                                && d.DeletedBy == null
                            orderby d.Id
                            select new Model_View_Sample
                            {
                                Id = d.Id,
                                CompanyCode = d.CompanyCode,
                                SiteName = s.SiteName,
                                ExternalId = d.ExternalId,
                                ClientId = c.Id,
                                ClientName = c.Name,
                                ProjectId = p.Id,
                                ProjectName = p.Name,
                                ProjectType = p.Type,
                                DateOfJob = d.DateOfJob,
                                Stage = d.Stage,
                                DateSampleEnd = sdl.SamplingEnd,
                                DateSampleStart = sdl.SamplingStart,
                                Tonnage = sdl.Tonnage,
                                JobName = jn.JobNumber,
                                IsActive = d.IsActive,
                                TunnelId = d.TunnelId,
                                Tunnel = d.Tunnel
                            }
                        );

                    var dataQuery3 =
                        (
                            from d in db.Samples
                            join s in db.Sites on d.SiteId equals s.SiteId
                            join c in db.Clients on d.ClientId equals c.Id
                            join p in db.Projects on d.ProjectId equals p.Id
                            join jn in db.AnalysisJobs on d.JobNumberId equals jn.Id
                            join sda in db.SampleDetailAMDs on d.Id equals sda.Sample_Id
                            where (all_company || d.CompanyCode == company_code)
                                && (all_site || d.SiteId == site_id)
                                && (all_client || d.ClientId == client_id)
                                && (all_project || d.ProjectId == project_id)
                                && (all_dateofjob || (d.DateOfJob >= dateFrom && d.DateOfJob <= dateTo))
                                && (all_search || d.ExternalId.Contains(search))
                                && d.DeletedBy == null
                            orderby d.Id
                            select new Model_View_Sample
                            {
                                Id = d.Id,
                                CompanyCode = d.CompanyCode,
                                SiteName = s.SiteName,
                                ExternalId = d.ExternalId,
                                SampleId = sda.SampleId,
                                ClientId = c.Id,
                                ClientName = c.Name,
                                ProjectId = p.Id,
                                ProjectName = p.Name,
                                ProjectType = p.Type,
                                DateOfJob = d.DateOfJob,
                                Stage = d.Stage,
                                DateSampleEnd = sda.DateSampleEnd,
                                DateSampleStart = sda.DateSampleStart,
                                Tonnage = 0,
                                Shift = sda.Shift,
                                Receive = sda.Receive,
                                JobName = jn.JobNumber,
                                IsActive = d.IsActive,
                                TunnelId = d.TunnelId,
                                Tunnel = d.Tunnel
                            }
                        );
                    var items = dataQuery.ToList();
                    var items2 = dataQuery2.ToList();
                    var items3 = dataQuery3.ToList();
                    List<SampleListModelView> datas = new List<SampleListModelView>();
                    foreach (var item in items)
                    {
                        datas.Add(new SampleListModelView
                        {
                            Id = item.Id,
                            CompanyCode = item.CompanyCode,
                            SiteName = item.SiteName,
                            ExternalId = item.ExternalId,
                            SampleId = item.SampleId,
                            ClientId = item.ClientId,
                            ClientName = item.ClientName,
                            ProjectId = item.ProjectId,
                            ProjectName = item.ProjectName,
                            ProjectType = item.ProjectType,
                            DateOfJob = item.DateOfJob.ToString("yyyy-MM-dd"),
                            Stage = item.Stage,
                            DateSampleEnd = item.DateSampleEnd.ToString("yyyy-MM-dd HH:mm:ss"),
                            DateSampleStart = item.DateSampleStart.ToString("yyyy-MM-dd HH:mm:ss"),
                            Tonnage = item.Tonnage,
                            Shift = item.Shift,
                            Receive = item.Receive.ToString("yyyy-MM-dd"),
                            JobNumber = item.JobName,
                            IsActive = item.IsActive ?? true,
                            TunnelId = item?.TunnelId,
                            TunnelName = item?.Tunnel?.Name
                        });
                    }
                    foreach (var item in items2)
                    {
                        datas.Add(new SampleListModelView
                        {
                            Id = item.Id,
                            CompanyCode = item.CompanyCode,
                            SiteName = item.SiteName,
                            ExternalId = item.ExternalId,
                            SampleId = " ",
                            ClientId = item.ClientId,
                            ClientName = item.ClientName,
                            ProjectId = item.ProjectId,
                            ProjectName = item.ProjectName,
                            ProjectType = item.ProjectType,
                            DateOfJob = item.DateOfJob.ToString("yyyy-MM-dd"),
                            Stage = item.Stage,
                            DateSampleEnd = item.DateSampleEnd.ToString("yyyy-MM-dd HH:mm:ss"),
                            DateSampleStart = item.DateSampleStart.ToString("yyyy-MM-dd HH:mm:ss"),
                            Tonnage = item.Tonnage,
                            Shift = 0,
                            Receive = " ",
                            JobNumber = item.JobName,
                            IsActive = item.IsActive ?? true,
                            TunnelId = item?.TunnelId,
                            TunnelName = item?.Tunnel?.Name
                        });
                    }
                    foreach( var item in items3) {
                        datas.Add(new SampleListModelView
                        {
                            Id = item.Id,
                            CompanyCode = item.CompanyCode,
                            SiteName = item.SiteName,
                            ExternalId = item.ExternalId,
                            SampleId = item.SampleId,
                            ClientId = item.ClientId,
                            ClientName = item.ClientName,
                            ProjectId = item.ProjectId,
                            ProjectName = item.ProjectName,
                            ProjectType = item.ProjectType,
                            DateOfJob = item.DateOfJob.ToString("yyyy-MM-dd"),
                            Stage = item.Stage,
                            DateSampleEnd = item.DateSampleEnd.ToString("yyyy-MM-dd HH:mm:ss"),
                            DateSampleStart = item.DateSampleStart.ToString("yyyy-MM-dd HH:mm:ss"),
                            Tonnage = item.Tonnage,
                            Shift = item.Shift,
                            Receive = item.Receive.ToString("yyyy-MM-dd"),
                            JobNumber = item.JobName,
                            IsActive = item.IsActive ?? true,
                            TunnelId = item?.TunnelId,
                            TunnelName = item?.Tunnel?.Name
                        });
                    }

                    var newDatas = DynamicSortHelper.OrderBy(datas, sortColumn + ' ' + sortBy).Skip((page-1)*pageSize).Take(pageSize).ToList();
                    var result = new { Success = true, Permission = permission_Item, User = UserX.Information(user, is_Show_User_Menu, is_Show_User_Relation), Message = string.Empty, MessageDetail = string.Empty, Version = Configuration.VERSION, Items = newDatas, meta = new { Total = datas.Count, currentPage = page, totalPages = datas.Count / pageSize + 1, limit = pageSize } };
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
            if (!permission_Item.Is_View)
            {
                var msg = new { Success = false, Permission = permission_Item, Message = Permission.ERROR_PERMISSION_READ, MessageDetail = string.Empty, Version = Configuration.VERSION };
                return Json(msg, JsonRequestBehavior.AllowGet);
            }

            bool is_Show_User_Menu = OurUtility.ValueOf(Request, "u_menu").Equals("1");
            bool is_Show_User_Relation = OurUtility.ValueOf(Request, "u_relation").Equals("1");

            long id = OurUtility.ToInt64(Request["id"]);
            if (id <= 0)
            {
                // -- special purpose
                // this Id is only for Checking CurrentUser:Info\

                var result = new { Success = true, Permission = permission_Item, User = UserX.Information(user, is_Show_User_Menu, is_Show_User_Relation), Version = Configuration.VERSION };
                return Json(result, JsonRequestBehavior.AllowGet);
            }

            try
            {
                var sample = new Model_View_Sample_Registration();
                List<SampleScheme> sampleSchemeList = new List<SampleScheme>();

                using (MERCY_Ctx db = new MERCY_Ctx())
                {
                    var item = db.Samples.Find(id);
                    var detailGeneral = item.SampleDetailGenerals.OfType<SampleDetailGeneral>().FirstOrDefault();
                    var detailLoading = item.SampleDetailLoadings.OfType<SampleDetailLoading>().FirstOrDefault();
                    var sample_Scheme = item.SampleSchemes.OfType<SampleScheme>().ToList();

                    foreach (SampleScheme sscheme in sample_Scheme)
                    {
                        sampleSchemeList.Add(sscheme);
                    }

                    var queryDatas =
                        (
                            from s in db.Samples
                            join cl in db.Clients on s.ClientId equals cl.Id
                            join st in db.Sites on s.SiteId equals st.SiteId
                            join p in db.Projects on s.ProjectId equals p.Id
                            join r in db.RefTypes on s.RefTypeId equals r.Id
                            join cp in db.CompanyProjects on s.ProjectId equals cp.ProjectId
                            where s.Id == item.Id
                            select new
                            {
                                ClientName = cl.Name,
                                ProjectName = p.Name,
                                RefTypeName = r.Name,
                                ProjectTypeName = cp.ProjectType

                            }
                        ).FirstOrDefault();

                    SampleDetailModelView sampleDetail = new SampleDetailModelView();
                    if (detailGeneral != null)
                    {
                        sampleDetail = new SampleDetailModelView
                        {
                            Id = item.Id,
                            CompanyCode = item.CompanyCode,
                            SiteId = item.SiteId,
                            ExternalId = item.ExternalId,
                            DateOfJob = item.DateOfJob.ToString("yyyy-MM-dd HH:mm:ss"),
                            ClientId = item.ClientId,
                            ProjectId = item.ProjectId,
                            RefTypeId = item.RefTypeId,
                            CompanyName = item.Company.Name,
                            ProjectName = queryDatas.ProjectName,
                            RefTypeName = queryDatas.RefTypeName,
                            CreatedOn_Str = item.CreatedOn.ToString("yyyy-MM-dd HH:mm:ss"),
                            SiteName = item.Site.SiteName,
                            ClientName = queryDatas.ClientName,
                            ReceivedBy = item.ReceivedBy,
                            IsActive = item.IsActive ?? true,
                            Remark = item.Remark,
                            Location = item.Location,
                            ThicknessFrom = item.ThicknessFrom ?? 0,
                            ThicknessTo = item.ThicknessTo ?? 0,
                            ProjectTypeName = queryDatas.ProjectTypeName,
                            TunnelId = item.TunnelId,
                            DetailGeneral = new DetailGeneralModelView
                            {
                                GeoPrefix = detailGeneral.GeoPrefix,
                                SampleId = detailGeneral.SampleId,
                                Shift = detailGeneral.Shift,
                                Sequence = detailGeneral.Sequence,
                                DateSampleEnd = detailGeneral.DateSampleEnd.ToString("yyyy-MM-dd HH:mm:ss"),
                                DateSampleStart = detailGeneral.DateSampleStart.ToString("yyyy-MM-dd HH:mm:ss"),
                                Receive = detailGeneral.Receive.ToString("yyyy-MM-dd HH:mm:ss"),
                                Tonnage = detailGeneral.Tonnage,
                                Destination = detailGeneral.Destination,
                                BargeName = detailGeneral.BargeName,
                                TripNumber = detailGeneral.TripNumber
                            },
                            SampleSchemes = sample_Scheme.Select(
                                x => new SampleSchemeModelView
                                {
                                    SampleSchemeId = x.Id,
                                    SchemeId = x.SchemeId,
                                    SchemeName = x.Scheme.Name
                                }
                                ).ToList()
                        };
                    }
                    else
                    {
                        sampleDetail = new SampleDetailModelView
                        {
                            Id = item.Id,
                            CompanyCode = item.CompanyCode,
                            SiteId = item.SiteId,
                            ExternalId = item.ExternalId,
                            DateOfJob = item.DateOfJob.ToString("yyyy-MM-dd HH:mm:ss"),
                            ClientId = item.ClientId,
                            ProjectId = item.ProjectId,
                            RefTypeId = item.RefTypeId,
                            CompanyName = item.Company.Name,
                            ProjectName = queryDatas.ProjectName,
                            RefTypeName = queryDatas.RefTypeName,
                            CreatedOn_Str = item.CreatedOn.ToString("yyyy-MM-dd HH:mm:ss"),
                            SiteName = item.Site.SiteName,
                            ClientName = queryDatas.ClientName,
                            ReceivedBy = item.ReceivedBy,
                            IsActive = item.IsActive ?? true,
                            Remark = item.Remark,
                            Location = item.Location,
                            ThicknessFrom = item.ThicknessFrom ?? 0,
                            ThicknessTo = item.ThicknessTo ?? 0,
                            ProjectTypeName = queryDatas.ProjectTypeName,
                            TunnelId = item.TunnelId,
                            DetailLoading = new DetailLoadingModelView
                            {
                                LoadingNumber = detailLoading.LoadingNumber,
                                Tonnage = detailLoading.Tonnage,
                                DispatchId = detailLoading.DispatchId,
                                Customer = detailLoading.Customer,
                                ETA = detailLoading.ETA.ToString("yyyy-MM-dd HH:mm:ss"),
                                ATA = detailLoading.ATA.ToString("yyyy-MM-dd HH:mm:ss"),
                                Contract = detailLoading.Contract,
                                Product = detailLoading.Product,
                                LotNumber = detailLoading.LotNumber,
                                SamplingEnd = detailLoading.SamplingEnd.ToString("yyyy-MM-dd HH:mm:ss"),
                                SamplingStart = detailLoading.SamplingStart.ToString("yyyy-MM-dd HH:mm:ss"),
                                VesselName = detailLoading.VesselName,
                                LotSamples = detailLoading.LotSamples

                            }
                            ,
                            SampleSchemes = sample_Scheme.Select(
                                x => new SampleSchemeModelView
                                {
                                    SampleSchemeId = x.Id,
                                    SchemeId = x.SchemeId,
                                    SchemeName = x.Scheme.Name
                                }
                                ).ToList()
                        };
                    }
                    var result = new { Success = true, Permission = permission_Item, User = UserX.Information(user, is_Show_User_Menu, is_Show_User_Relation), Message = string.Empty, MessageDetail = string.Empty, Version = Configuration.VERSION, Item = sampleDetail };
                    return Json(result, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                var result = new { Success = false, Permission = permission_Item, ex.Message, MessageDetail = string.Empty, Version = Configuration.VERSION, Items = string.Empty, Total = 0 };
                return Json(result, JsonRequestBehavior.AllowGet);
            }
        }

        public JsonResult Create(List<SampleModelView> samples)
        {
            UserX user = new UserX(Request);
            Permission_Item permission_Item = null;

            Permission.Check_API(Request, user, ref permission_Item);
            if (!permission_Item.Is_Add)
            {
                var msg = new { Success = false, Permission = permission_Item, Message = Permission.ERROR_PERMISSION_ADD, MessageDetail = string.Empty, Version = Configuration.VERSION };
                return Json(msg, JsonRequestBehavior.AllowGet);
            }
            using (MERCY_Ctx db = new MERCY_Ctx())
            {
                var transaction = db.Database.BeginTransaction();

                try
                {
                    var datas = new List<SampleListModelView>();
                    var invalidSampleId = new List<string>();
                    foreach (var sample in samples)
                    {
                        if (sample.DetailGeneral != null)
                        {
                            SampleValidateModelView val = new SampleValidateModelView
                            {
                                CompanyCode = sample.CompanyCode,
                                SampleId = sample.DetailGeneral.SampleId,
                                Shift = sample.DetailGeneral.Shift,
                                Sequence = sample.DetailGeneral.Sequence,
                                ClientId = sample.ClientId,
                                ProjectId = sample.ProjectId,
                                DateOfJob = sample.DateOfJob
                            };

                            if (!ValidateSample(db, val))
                            {
                                invalidSampleId.Add(sample.DetailGeneral.SampleId);
                                continue;
                            }
                        }
                        else if (sample.DetailAMD != null) 
                        {
                            SampleValidateModelView val = new SampleValidateModelView
                            {
                                CompanyCode = sample.CompanyCode,
                                SampleId = sample.DetailAMD.SampleId,
                                Shift = sample.DetailAMD.Shift,
                                Sequence = sample.DetailAMD.Sequence,
                                ClientId = sample.ClientId,
                                ProjectId = sample.ProjectId,
                                DateOfJob = sample.DateOfJob
                            };
                            if (!ValidateSampleAMD(db, val))
                            {
                                invalidSampleId.Add(sample.DetailAMD.SampleId);
                                continue;
                            }
                        }
                        else if(sample.CompanyCode.Contains("IMM"))
                        {
                            var tempExternalId = $"{sample.DetailLoading.LoadingNumber} - {sample.DetailLoading.VesselName} - Lot {sample.DetailLoading.LotNumber}";
                            if (!ValidateSampleLoading(db, tempExternalId))
                            {
                                throw new Exception($"Failed to Store Samples, Sample Loading ExternalId Duplicate in {tempExternalId}");
                            }

                        }

                        int last = 1;
                        decimal thickness = 0;
                        bool isDetailGeneral = sample.DetailGeneral != null;
                        bool isDetailAMD = sample.DetailAMD != null;
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

                        if (isDetailGeneral && !isDetailAMD)
                        {
                            if (existingExternalId != null)
                            {
                                last = OurUtility.ToInt32(existingExternalId.Substring(codeProject.Length)) + 1;
                            }

                            receivedDate = sample.DetailGeneral.Receive;
                            externalId = codeProject + last.ToString();
                        }
                        else if (isDetailAMD && !isDetailGeneral)
                        {
                            if (existingExternalId != null)
                            {
                                last = OurUtility.ToInt32(existingExternalId.Substring(codeProject.Length)) + 1;
                            }
                            receivedDate = sample.DetailAMD.Receive;
                            externalId = codeProject + last.ToString();

                        }
                        else
                        {
                            receivedDate = sample.DetailLoading.SamplingStart;
                            externalId = sample.CompanyCode.Contains("IMM") ? 
                                $"{sample.DetailLoading.LoadingNumber} - {sample.DetailLoading.VesselName} - Lot {sample.DetailLoading.LotNumber}" : 
                                $"{sample.DetailLoading.LoadingNumber} - Lot {sample.DetailLoading.LotNumber}";
                        }


                        // Generate temporary analyst job model 
                        var analystTemp = new AnalystJobModel
                        {
                            CompanyCode = sample.CompanyCode,
                            JobNumber = codeProject,
                            Status = isDetailAMD ? "Completed" : "register",
                            JobDate = sample.DateOfJob,
                            ReceivedDate = receivedDate,
                            ProjectId = sample.ProjectId,
                            IsDetailGeneral = isDetailGeneral
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
                            Stage = isDetailAMD ? "Completed" : "register",
                            TunnelId = sample.TunnelId,
                            JobNumberId = GetJobNumberId(analystTemp, user, db, isDetailAMD)
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
                            if (!StoreAnalysisResult(db, sampleScheme.SchemeId, data.Id, sample.CompanyCode, user, isDetailAMD))
                            {
                                throw new Exception("Failed to Store Analysis Result ");
                            }
                        }

                        if (isDetailGeneral)
                        {
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
                            datas.Add(GetListSample(data.Id, sampleDetailGeneral.Id.ToString(), string.Empty, string.Empty, db));
                        }
                        else if (isDetailAMD)
                        {
                            SampleDetailAMD sampleDetailAMD = new SampleDetailAMD
                            {
                                GeoPrefix = sample.DetailAMD.GeoPrefix,
                                SampleId = sample.DetailAMD.SampleId,
                                Shift = sample.DetailAMD.Shift,
                                Sequence = sample.DetailAMD.Sequence,
                                LaboratoryId = sample.DetailAMD.LaboratoryId,
                                SampleType = sample.DetailAMD.SampleType,
                                Location = sample.DetailAMD.Location,
                                DateSampleStart = sample.DetailAMD.DateSampleStart,
                                DateSampleEnd = sample.DetailAMD.DateSampleEnd,
                                Receive = sample.DetailAMD.Receive,
                                MassSampleReceived = sample.DetailAMD.MassSampleReceived,
                                TS = sample.DetailAMD.TS,
                                ANC = sample.DetailAMD.ANC,
                                NAG = sample.DetailAMD.NAG,
                                NAGPH45 = sample.DetailAMD.NAGPH45,
                                NAGPH70 = sample.DetailAMD.NAGPH70,
                                NAGType = sample.DetailAMD.NAGType,
                                Remark = sample.DetailAMD.Remark,
                                Sample_Id = data.Id,
                                CreatedBy = user.UserId,
                                CreatedOn = DateTime.Now
                            };

                            db.SampleDetailAMDs.Add(sampleDetailAMD);
                            db.SaveChanges();
                            datas.Add(GetListSample(data.Id, string.Empty, string.Empty, sampleDetailAMD.Id.ToString(), db));
                        }
                        else
                        {
                            SampleDetailLoading sampleDetailLoading = new SampleDetailLoading
                            {
                                SampleId = data.Id,
                                LoadingNumber = sample.DetailLoading.LoadingNumber,
                                VesselName = sample.DetailLoading.VesselName,
                                DispatchId = sample.DetailLoading.DispatchId,
                                Customer = sample.DetailLoading.Customer,
                                ETA = sample.DetailLoading.ETA ?? throw new Exception("ETA value cannot be empty"),
                                ATA = sample.DetailLoading.ATA ?? throw new Exception("ATA value cannot be empty"),
                                Contract = sample.DetailLoading.Contract ?? "",
                                Product = sample.DetailLoading.Product,
                                LotNumber = sample.DetailLoading.LotNumber,
                                LotSamples = sample.DetailLoading.LotSamples,
                                Tonnage = sample.DetailLoading.Tonnage,
                                SamplingStart = sample.DetailLoading.SamplingStart,
                                SamplingEnd = sample.DetailLoading.SamplingEnd,
                                CreatedBy = user.UserId,
                                CreatedOn = DateTime.Now
                            };

                            db.SampleDetailLoadings.Add(sampleDetailLoading);
                            db.SaveChanges();
                            datas.Add(GetListSample(data.Id, string.Empty, sampleDetailLoading.Id.ToString(), string.Empty, db));
                        }
                    }

                    transaction.Commit();

                    var message = $"Successfuly registered {samples.Count - invalidSampleId.Count} of {samples.Count} samples.";
                    if (invalidSampleId.Count > 0)
                    {
                        message += $" Duplicate sequence for sample " + string.Join(", ", invalidSampleId.Select(sampleId => $"'{sampleId}'"));
                    }

                    var result = new { Success = true, Permission = permission_Item, Message = message, MessageDetail = string.Empty, Data = datas, Version = Configuration.VERSION };
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

        public async Task<JsonResult> Delete()
        {
            UserX user = new UserX(Request);
            Permission_Item permission_Item = null;

            string msg = string.Empty;
            bool isSuccess = false;
            int sampleId = OurUtility.ToInt32(OurUtility.ValueOf(Request, "sampleId"));
            // check user permission
            Permission.Check_API(Request, user, ref permission_Item);
            if (!permission_Item.Is_Delete)
            {
                var result = new { Success = false, Permission = permission_Item, Message = Permission.ERROR_PERMISSION_DELETE, MessageDetail = string.Empty, Version = Configuration.VERSION };
                return Json(result, JsonRequestBehavior.AllowGet);
            }
            try
            {
                // Will be allowed to delete with sample status are "Registered" or "In Progress"
                // Data will be removed and the user does not view sample analysis results on the current job.
                // if the sample is last on the current job, then the job will be removed.
                using (MERCY_Ctx db = new MERCY_Ctx())
                {
                    var sample =  await db.Samples.FirstOrDefaultAsync(u=> u.Id == sampleId);
                    //check sample status
                    if (sample == null)
                    {
                        msg = "Sample Registration not found";
                    }
                    else if(sample.Stage != "In Progress" && sample.Stage != "register")
                    {
                        msg = "Sample cannot be deleted because the analysis result has been completed.";
                    }
                    else
                    {
                        db.Samples.Remove(sample);
                        await db.SaveChangesAsync();

                        var analysisJob = await db.AnalysisJobs.Include(u => u.Samples).FirstOrDefaultAsync(u => u.Id == sample.JobNumberId);
                        if (analysisJob.Samples.Count == 0)
                        {
                            db.AnalysisJobs.Remove(analysisJob);
                        }
                        await db.SaveChangesAsync();
                        isSuccess = true;
                        msg = "Delete success!";
                    }
                }
              
                var result = new { Success = isSuccess, Message = msg, MessageDetail = string.Empty, Version = Configuration.VERSION };
                return Json(result, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                var result = new { Success = false, Permission = permission_Item, ex.Message, MessageDetail = ex.StackTrace, Version = Configuration.VERSION, Items = string.Empty, Total = 0 };
                return Json(result, JsonRequestBehavior.AllowGet);
            }
        }

        public JsonResult Update(SampleUpdateModelView sampleUpdate)
        {
            UserX user = new UserX(Request);
            Permission_Item permission_Item = null;

            string msgError = string.Empty;

            Permission.Check_API(Request, user, ref permission_Item);
            if (!permission_Item.Is_Edit)
            {
                var msg = new { Success = false, Permission = permission_Item, Message = Permission.ERROR_PERMISSION_ADD, MessageDetail = string.Empty, Version = Configuration.VERSION };
                return Json(msg, JsonRequestBehavior.AllowGet);
            }
            try
            {
                using(MERCY_Ctx db = new MERCY_Ctx())
                {
                    var item = db.Samples.Find(sampleUpdate.Id);
                    if (sampleUpdate.IsActive != null)
                    {
                        msgError = "Going to Update Status Sample";
                        item.IsActive = sampleUpdate.IsActive;
                        db.SaveChanges();

                        var report = new
                        {
                            Id = item.Id,
                            StatusUpdate = "Update Status For " + item.ExternalId + " Success"
                        };
                        var result = new { Success = true,  Message = BaseConstants.MESSAGE_CREATE_SUCCESS, MessageDetail = string.Empty, Data = report, Version = Configuration.VERSION };
                        return Json(result, JsonRequestBehavior.AllowGet);
                    }
                    else
                    {
                        msgError = "Going To Update Sample Registration";

                        //Update Sample
                        item.Thickness = sampleUpdate.ThicknessTo - sampleUpdate.ThicknessFrom;
                        item.ThicknessFrom = sampleUpdate.ThicknessFrom;
                        item.ThicknessTo = sampleUpdate.ThicknessTo;
                        item.Remark = sampleUpdate.Remark;
                        item.Location = sampleUpdate.Location;
                        item.LastModifiedBy = user.UserId;
                        item.LastModifiedOn = DateTime.Now;
                        item.TunnelId = sampleUpdate.TunnelId;
                        db.SaveChanges();

                        msgError = "Going to Update Sample Detail";
                        if(sampleUpdate.DetailGeneral != null)
                        {
                            msgError = "Going to Update Sample Detail General";
                            var detailGeneral = item.SampleDetailGenerals.OfType<SampleDetailGeneral>().FirstOrDefault();
                            var sampleGeneral = db.SampleDetailGenerals.Find(detailGeneral.Id);
                            sampleGeneral.SampleId = sampleUpdate.DetailGeneral.SampleId;
                            sampleGeneral.Tonnage = sampleUpdate.DetailGeneral.Tonnage;
                            sampleGeneral.TripNumber = sampleUpdate.DetailGeneral.TripNumber;
                            sampleGeneral.BargeName = sampleUpdate.DetailGeneral.BargeName;
                            sampleGeneral.DateSampleStart = sampleUpdate.DetailGeneral.DateSampleStart;
                            sampleGeneral.DateSampleEnd = sampleUpdate.DetailGeneral.DateSampleEnd;
                            sampleGeneral.Receive = sampleUpdate.DetailGeneral.Receive;
                            sampleGeneral.Destination = sampleUpdate.DetailGeneral.Destination;
                            sampleGeneral.LastModifiedBy = user.UserId;
                            sampleGeneral.LastModifiedOn = DateTime.Now;
                            sampleGeneral.Shift = sampleUpdate.DetailGeneral.Shift;
                            sampleGeneral.GeoPrefix = sampleUpdate.DetailGeneral.GeoPrefix;
                            db.SaveChanges();
                            msgError = "Success To Update Sample Detail General";

                        }
                        else if(sampleUpdate.DetailLoading != null)
                        {
                            msgError = "Going to Update Sample Detail Loading";
                            var detailLoading = item.SampleDetailLoadings.OfType<SampleDetailLoading>().FirstOrDefault();
                            var sampleLoading = db.SampleDetailLoadings.Find(detailLoading.Id);
                            sampleLoading.ETA = sampleUpdate.DetailLoading.ETA ?? throw new Exception("ETA value cannot be empty");
                            sampleLoading.ATA = sampleUpdate.DetailLoading.ATA ?? throw new Exception("ATA value cannot be empty");
                            sampleLoading.Contract = sampleUpdate.DetailLoading.Contract;
                            sampleLoading.Customer = sampleUpdate.DetailLoading.Customer;
                            sampleLoading.DispatchId = sampleUpdate.DetailLoading.DispatchId;
                            sampleLoading.LotSamples = sampleUpdate.DetailLoading.LotSamples;
                            sampleLoading.Product = sampleUpdate.DetailLoading.Product;
                            sampleLoading.SamplingEnd = sampleUpdate.DetailLoading.SamplingEnd;
                            sampleLoading.Tonnage = sampleUpdate.DetailLoading.Tonnage;
                            sampleLoading.VesselName = sampleUpdate.DetailLoading.VesselName;
                            sampleLoading.SamplingStart = sampleUpdate.DetailLoading.SamplingStart;
                            sampleLoading.LastModifiedBy = user.UserId;
                            sampleLoading.LastModifiedOn = DateTime.Now;
                            db.SaveChanges();
                            msgError = "After Update Sample Detail Loading";
                        } else
                        {
                            msgError = "Goint to Update Sample Detail AMD";
                            var detailAMD = item.SampleDetailAMDs.OfType<SampleDetailAMD>().FirstOrDefault();
                            var sampleDetailAMD = db.SampleDetailAMDs.Find(detailAMD.Id);
                            sampleDetailAMD.ANC = detailAMD.ANC;
                            sampleDetailAMD.TS = detailAMD.TS;
                            sampleDetailAMD.NAG = detailAMD.NAG;
                            sampleDetailAMD.MassSampleReceived = detailAMD.MassSampleReceived;

                            sampleDetailAMD.DateSampleEnd = detailAMD.DateSampleEnd;
                            sampleDetailAMD.DateSampleStart = detailAMD.DateSampleStart;
                            sampleDetailAMD.Receive = detailAMD.Receive;

                            sampleDetailAMD.SampleId = detailAMD.SampleId;
                            sampleDetailAMD.GeoPrefix = detailAMD.GeoPrefix;
                            sampleDetailAMD.Location = detailAMD.Location;
                            sampleDetailAMD.LaboratoryId = detailAMD.LaboratoryId;


                            sampleDetailAMD.NAGPH45 = detailAMD.NAGPH45;
                            sampleDetailAMD.NAGPH70 = detailAMD.NAGPH70;
                            sampleDetailAMD.NAGType = detailAMD.NAGType;
                            sampleDetailAMD.SampleType = detailAMD.SampleType;

                            sampleDetailAMD.Remark = detailAMD.Remark;
                            sampleDetailAMD.LastModifiedBy = user.UserId;
                            sampleDetailAMD.LastModifiedOn = DateTime.Now;
                            db.SaveChanges();
                            msgError = "After Detail Update Sample Detail AMD";
                        }
                        //Waiting Confirmation from PO

    /*                        if(!CheckSampleScheme(sample_Scheme, sampleUpdate.Sample.Schemes))
                            {
                                if(!UpdateSampleScheme(sample_Scheme, sampleUpdate.Sample.Schemes, (int)sampleUpdate.Id, sampleUpdate.Sample.CompanyCode, user, db))
                                {
                                    msgError = "Failed Update Scheme";
                                    throw new Exception(msgError);
                                }
                            }*/
                        var report = new
                        {
                            Id = item.Id,
                            StatusUpdate = "Update For " + item.ExternalId + " Success"
                        };
                        var result = new { Success = true, Message = BaseConstants.MESSAGE_CREATE_SUCCESS, MessageDetail = string.Empty, Data = report, Version = Configuration.VERSION };

                        return Json(result, JsonRequestBehavior.AllowGet);
                    }
                    
                }
            }catch(Exception ex) 
            {
                var result = new { Success = false, Permission = permission_Item, ex.Message, MessageDetail = ex.StackTrace, Version = Configuration.VERSION, Items = string.Empty, Total = 0 };
                return Json(result, JsonRequestBehavior.AllowGet);
            }
        }

        public bool CheckSampleScheme(List<SampleScheme> sampleSchemes, List<Models.Scheme> schemes)
        {
            if(schemes.Count != sampleSchemes.Count)
            {
                return false;
            }
            foreach(var item in schemes)
            {
                if(!sampleSchemes.Any(x=>x.SchemeId == item.SchemeId))
                {
                    return false;
                }
            }
            return true;
        }

        public bool UpdateSampleScheme(List<SampleScheme> sampleSchemes, List<Models.Scheme> schemes, int sampleId, string companyCode, UserX users, MERCY_Ctx db)
        {
            try
            {
                foreach(var samples in sampleSchemes)
                {
                    if(!RemoveAnalystResult(samples.Id, db))
                    {
                        return false;
                    }
                    db.SampleSchemes.Remove(samples);
                }
                db.SaveChanges();

                var sampleScheme = new List<SampleScheme>();
                foreach (var sampleSchemeItem in schemes)
                {
                    var sampleSchem = new SampleScheme
                    {
                        SampleId = sampleId,
                        SchemeId = sampleSchemeItem.SchemeId,
                        CreatedBy = users.UserId,
                        CreatedOn = DateTime.Now
                    };
                    db.SampleSchemes.Add(sampleSchem);
                    db.SaveChanges();
                    if (!StoreAnalysisResult(db, sampleSchem.SchemeId, sampleId, companyCode, users))
                    {
                        throw new Exception("Failed to Store Analyst Result ");
                    }
                }
                return true;
            }
            catch
            {
                return false;
            }
        }
       
        public bool RemoveAnalystResult(int sampleSchemeId, MERCY_Ctx db)
        {
            try
            {
                var analystResults = db.AnalysisResults.Where(x => x.SampleSchemeId == sampleSchemeId).ToList();
                foreach(var item in analystResults)
                {
                    if(!RemoveAnalystResultHistory(item.Id, db))
                    {
                        return false;
                    }
                    db.AnalysisResults.Remove(item);
                }
                db.SaveChanges();
                return true;
            }
            catch
            {
                return false;
            }
        }
        public bool RemoveAnalystResultHistory(int analystResult, MERCY_Ctx db)
        {
            try
            {
                var analystResultHistory = db.AnalysisResultHistories.Where(x => x.AnalysisResultId == analystResult).ToList();
                foreach(var item in analystResultHistory)
                {
                    db.AnalysisResultHistories.Remove(item);

                }
                db.SaveChanges();
                return true;
            }
            catch
            {
                return false;
            }
        }
        public JsonResult Get_ddl_Client_ByCompany(string companyCode)
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

            // -- Actual code
            try
            {
                using (MERCY_Ctx db = new MERCY_Ctx())
                {
                    var dataQuery =
                            (
                                from d in db.CompanyClientProjectSchemes
                                where d.CompanyCode == companyCode
                                orderby d.Id
                                select new
                                {
                                    id = d.ClientId,
                                    name = d.Client.Name
                                }
                            ).Distinct();

                    var items = dataQuery.ToList();
                    var result = new { Success = true, Permission = permission_Item, Message = string.Empty, MessageDetail = string.Empty, Version = Configuration.VERSION, Items = items, Total = items.Count };
                    return Json(result, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                var result = new { Success = false, Permission = permission_Item, ex.Message, MessageDetail = string.Empty, Version = Configuration.VERSION, Items = string.Empty, Total = 0 };
                return Json(result, JsonRequestBehavior.AllowGet);
            }
        }

        public JsonResult Get_ddl_Project_ByClient(string companyCode, int clientId)
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

            // -- Actual code
            try
            {
                using (MERCY_Ctx db = new MERCY_Ctx())
                {
                    var dataQuery =
                            (
                                from d in db.CompanyProjects
                                join ccps in db.CompanyClientProjectSchemes on d.CompanyCode equals ccps.CompanyCode 
                                where d.CompanyCode == companyCode
                                    && ccps.ClientId == clientId
                                    && ccps.ProjectId == d.ProjectId
                                orderby d.Id
                                select new
                                {
                                    id = d.ProjectId,
                                    name = d.Project.Name,
                                    type = d.ProjectType
                                }
                            ).Distinct();

                    var items = dataQuery.ToList();
                    var result = new { Success = true, Permission = permission_Item, Message = string.Empty, MessageDetail = string.Empty, Version = Configuration.VERSION, Items = items, Total = items.Count };
                    return Json(result, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                var result = new { Success = false, Permission = permission_Item, ex.Message, MessageDetail = string.Empty, Version = Configuration.VERSION, Items = string.Empty, Total = 0 };
                return Json(result, JsonRequestBehavior.AllowGet);
            }
        }

        public JsonResult Get_ddl_RefType_ByCompany(string companyCode)
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

            // -- Actual code
            try
            {
                using (MERCY_Ctx db = new MERCY_Ctx())
                {
                    var dataQuery =
                            (
                                from d in db.CompanyRefTypes
                                where d.CompanyCode == companyCode
                                orderby d.Id
                                select new
                                {
                                    id = d.RefType.Id,
                                    name = d.RefType.Name
                                }
                            );

                    var items = dataQuery.ToList();
                    var result = new { Success = true, Permission = permission_Item, Message = string.Empty, MessageDetail = string.Empty, Version = Configuration.VERSION, Items = items, Total = items.Count };
                    return Json(result, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                var result = new { Success = false, Permission = permission_Item, ex.Message, MessageDetail = string.Empty, Version = Configuration.VERSION, Items = string.Empty, Total = 0 };
                return Json(result, JsonRequestBehavior.AllowGet);
            }
        }

        public JsonResult Get_All_Scheme()
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

            // -- Actual code
            try
            {
                using (MERCY_Ctx db = new MERCY_Ctx())
                {
                    var dataQuery =
                            (
                                from d in db.Schemes
                                orderby d.Name
                                select new
                                {
                                    d.Id,
                                    d.Name,
                                    d.CompanySchemes.FirstOrDefault().Details
                                }
                            );

                    var items = dataQuery.ToList();
                    var result = new { Success = true, Permission = permission_Item, Message = string.Empty, MessageDetail = string.Empty, Version = Configuration.VERSION, Items = items, Total = items.Count };
                    return Json(result, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                var result = new { Success = false, Permission = permission_Item, ex.Message, MessageDetail = string.Empty, Version = Configuration.VERSION, Items = string.Empty, Total = 0 };
                return Json(result, JsonRequestBehavior.AllowGet);
            }
        }

        public JsonResult Get_Scheme(string companyCode, int clientId, int projectId)
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

            // -- Actual code
            try
            {
                List<Model_View_Sample_Scheme> sampleDefaultSchemeList = new List<Model_View_Sample_Scheme>();

                using (MERCY_Ctx db = new MERCY_Ctx())
                {
                    var items =
                            (
                                from d in db.Schemes
                                join ccps in db.CompanyClientProjectSchemes on d.Id equals ccps.SchemeId
                                join cs in db.CompanySchemes on d.Id equals cs.SchemeId
                                where ccps.CompanyCode == companyCode
                                    && ccps.ClientId == clientId
                                    && ccps.ProjectId == projectId
                                    && cs.CompanyCode == companyCode
                                orderby d.Name
                                select new SchemeModelView
                                {
                                    Id = d.Id,
                                    Name = d.Name,
                                    Type = d.Type,
                                    MinRepeatability = cs.MinRepeatability,
                                    MaxRepeatability = cs.MaxRepeatability,
                                    IsRequired = ccps.IsRequired,
                                    Details = cs.Details
                                }
                            ).Distinct().ToList();

                    var result = new { Success = true, Permission = permission_Item, Message = string.Empty, MessageDetail = string.Empty, Version = Configuration.VERSION, Items = items, Total = items.Count };
                    return Json(result, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                var result = new { Success = false, Permission = permission_Item, ex.Message, MessageDetail = string.Empty, Version = Configuration.VERSION, Items = string.Empty, Total = 0 };
                return Json(result, JsonRequestBehavior.AllowGet);
            }
        }


        private bool StoreAnalysisResult(MERCY_Ctx db, int schemeId, int sampleId, string companyCode, UserX users, bool isDetailAMD = false)
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
                if(schemeDetails == null)
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
                Dictionary<string, object> attributesDictionary = rules.Where(r => r.Input)
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
                        Result = isDetailAMD ? (decimal?)1 : null,
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
            catch{  }

            return false;
        }

        private bool ValidateSample(MERCY_Ctx db, SampleValidateModelView validate) 
        {
            bool result = false;
            
            try
            {
                var query =
                (
                    from s in db.Samples
                    join sdg in db.SampleDetailGenerals on s.Id equals sdg.Sample_Id
                    where s.ClientId == validate.ClientId &&
                        s.ProjectId == validate.ProjectId &&
                        s.DateOfJob == validate.DateOfJob &&
                        s.CompanyCode == validate.CompanyCode &&
                        sdg.Shift == validate.Shift &&
                        sdg.Sequence == validate.Sequence
                    select new
                    {
                        s.Id,
                        DetailId = sdg.Id
                    }
                ).FirstOrDefault();

                result = query == null; 
            }
            catch { }

            return result;
        }
        private bool ValidateSampleLoading(MERCY_Ctx db, string externalId)
        {
            bool result = false;
            try
            {
                var query =
                    (
                        from s in db.Samples
                        where s.ExternalId.Contains(externalId)
                        select new
                        {
                            s.Id,
                            s.ExternalId
                        }
                    ).FirstOrDefault();
                result = query == null;
            }
            catch { }

            return result;
        }
        private bool ValidateSampleAMD(MERCY_Ctx db, SampleValidateModelView validate)
        {

            bool result = false;

            try
            {
                var query =
                (
                    from s in db.Samples
                    join sda in db.SampleDetailAMDs on s.Id equals sda.Sample_Id
                    where s.ClientId == validate.ClientId &&
                        s.ProjectId == validate.ProjectId &&
                        s.DateOfJob == validate.DateOfJob &&
                        s.CompanyCode == validate.CompanyCode &&
                        sda.Shift == validate.Shift &&
                        sda.Sequence == validate.Sequence
                    select new
                    {
                        s.Id,
                        DetailId = sda.Id
                    }
                ).FirstOrDefault();

                result = query == null;
            }
            catch { }

            return result;
        }
        private int GetJobNumberId(AnalystJobModel job, UserX user, MERCY_Ctx db, bool isAMD = false)
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
                            jn.JobNumber,
                            jn.Status
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
                    
                    if(query.Status == "Completed" && !isAMD)
                    {
                        var analysisJob = db.AnalysisJobs.Find(query.Id);
                        analysisJob.Status = "In Progress";
                        db.SaveChanges();
                    }
                    result = query.Id;
                }
            }
            catch { }

            return result;
        }
        private SampleListModelView GetListSample(int sample_id, string sampleGeneral_id, string sampleLoading_id, string sampleAMD_id, MERCY_Ctx db)
        {
            bool is_general = false;
            bool is_amd = false;
            int sampleDetail = 0;
            if(!string.IsNullOrEmpty(sampleGeneral_id))
            {
                is_general = true;
                sampleDetail = int.Parse(sampleGeneral_id);
            }
            else if (!string.IsNullOrEmpty(sampleAMD_id))
            {
                is_general = false;
                is_amd = true;
                sampleDetail= int.Parse(sampleAMD_id);
            }
            else 
            {
                sampleDetail = int.Parse(sampleLoading_id);
            }

            try
            {
                var datas = new SampleListModelView();
                if (is_general)
                {
                    
                    var dataQuery =
                        (
                            from d in db.Samples
                            join s in db.Sites on d.SiteId equals s.SiteId
                            join c in db.Clients on d.ClientId equals c.Id
                            join p in db.Projects on d.ProjectId equals p.Id
                            join sdg in db.SampleDetailGenerals on d.Id equals sdg.Sample_Id
                            join aj in db.AnalysisJobs on d.JobNumberId equals aj.Id
                            where d.Id == sample_id
                                && sdg.Id == sampleDetail
                            orderby d.Id
                            select new Model_View_Sample
                            {
                                Id = d.Id,
                                CompanyCode = d.CompanyCode,
                                SiteName = s.SiteName,
                                SampleId = sdg.SampleId,
                                ExternalId = d.ExternalId,
                                ClientName = c.Name,
                                ProjectName = p.Name,
                                DateOfJob = d.DateOfJob,
                                Stage = d.Stage,
                                ProjectType = p.Type,
                                DateSampleEnd = sdg.DateSampleEnd,
                                DateSampleStart = sdg.DateSampleStart,
                                Tonnage = sdg.Tonnage,
                                Shift = sdg.Shift,
                                Receive = sdg.Receive,
                                JobName = aj.JobNumber,
                                IsActive = d.IsActive
                            }
                        );

                    var items = dataQuery.FirstOrDefault();

                    datas = new SampleListModelView()
                    {
                        Id = items.Id,
                        CompanyCode = items.CompanyCode,
                        SiteName = items.SiteName,
                        SampleId = items.SampleId,
                        ExternalId = items.ExternalId,
                        ClientName = items.ClientName,
                        ProjectName = items.ProjectName,
                        DateOfJob = items.DateOfJob.ToString("yyyy-MM-dd"),
                        Stage = items.Stage,
                        DateSampleEnd = items.DateSampleEnd.ToString("yyyy-MM-dd"),
                        DateSampleStart = items.DateSampleStart.ToString("yyyy-MM-dd"),
                        Tonnage = items.Tonnage,
                        Shift = items.Shift,
                        Receive = items.Receive.ToString("yyyy-MM-dd"),
                        JobNumber = items.JobName,
                        IsActive = items.IsActive ?? true,
                        ProjectType = items.ProjectType
                            
                    };

                    
                }else if (is_amd)
                {
                    var dataQuery =
                        (
                            from d in db.Samples
                            join s in db.Sites on d.SiteId equals s.SiteId
                            join c in db.Clients on d.ClientId equals c.Id
                            join p in db.Projects on d.ProjectId equals p.Id
                            join sda in db.SampleDetailAMDs on d.Id equals sda.Sample_Id
                            join aj in db.AnalysisJobs on d.JobNumberId equals aj.Id
                            where d.Id == sample_id
                                && sda.Id == sampleDetail
                            orderby d.Id
                            select new Model_View_Sample
                            {
                                Id = d.Id,
                                CompanyCode = d.CompanyCode,
                                SiteName = s.SiteName,
                                SampleId = sda.SampleId,
                                ExternalId = d.ExternalId,
                                ClientName = c.Name,
                                ProjectName = p.Name,
                                DateOfJob = d.DateOfJob,
                                Stage = d.Stage,
                                ProjectType = p.Type,
                                DateSampleEnd = sda.DateSampleEnd,
                                DateSampleStart = sda.DateSampleStart,
                                Tonnage = 0,
                                Shift = sda.Shift,
                                Receive = sda.Receive,
                                JobName = aj.JobNumber,
                                IsActive = d.IsActive
                            }
                        );

                    var items = dataQuery.FirstOrDefault();

                    datas = new SampleListModelView()
                    {
                        Id = items.Id,
                        CompanyCode = items.CompanyCode,
                        SiteName = items.SiteName,
                        SampleId = items.SampleId,
                        ExternalId = items.ExternalId,
                        ClientName = items.ClientName,
                        ProjectName = items.ProjectName,
                        DateOfJob = items.DateOfJob.ToString("yyyy-MM-dd"),
                        Stage = items.Stage,
                        DateSampleEnd = items.DateSampleEnd.ToString("yyyy-MM-dd"),
                        DateSampleStart = items.DateSampleStart.ToString("yyyy-MM-dd"),
                        Tonnage = items.Tonnage,
                        Shift = items.Shift,
                        Receive = items.Receive.ToString("yyyy-MM-dd"),
                        JobNumber = items.JobName,
                        IsActive = items.IsActive ?? true,
                        ProjectType = items.ProjectType

                    };

                }
                else
                {
                    
                    var dataQuery =
                        (
                            from d in db.Samples
                            join s in db.Sites on d.SiteId equals s.SiteId
                            join c in db.Clients on d.ClientId equals c.Id
                            join p in db.Projects on d.ProjectId equals p.Id
                            join sdl in db.SampleDetailLoadings on d.Id equals sdl.SampleId
                            join aj in db.AnalysisJobs on d.JobNumberId equals aj.Id
                            where d.Id == sample_id
                                && sdl.Id == sampleDetail
                            orderby d.Id
                            select new Model_View_Sample
                            {
                                Id = d.Id,
                                CompanyCode = d.CompanyCode,
                                SiteName = s.SiteName,
                                SampleId =  " ",
                                ExternalId = d.ExternalId,
                                ClientName = c.Name,
                                ProjectName = p.Name,
                                ProjectType = p.Type,
                                DateOfJob = d.DateOfJob,
                                Stage = d.Stage,
                                DateSampleEnd = sdl.SamplingEnd,
                                DateSampleStart = sdl.SamplingStart,
                                Tonnage = sdl.Tonnage,
                                Shift = 0,
                                JobName = aj.JobNumber,
                                IsActive = d.IsActive
                            }
                        );

                    var items = dataQuery.FirstOrDefault();

                    datas = new SampleListModelView()
                    {
                        Id = items.Id,
                        CompanyCode = items.CompanyCode,
                        SiteName = items.SiteName,
                        ExternalId = items.ExternalId,
                        SampleId = items.SampleId,
                        ClientName = items.ClientName,
                        ProjectName = items.ProjectName,
                        DateOfJob = items.DateOfJob.ToString("yyyy-MM-dd"),
                        Stage = items.Stage,
                        DateSampleEnd = items.DateSampleEnd.ToString("yyyy-MM-dd"),
                        DateSampleStart = items.DateSampleStart.ToString("yyyy-MM-dd"),
                        Tonnage = items.Tonnage,
                        Shift = 0,
                        Receive = " ",
                        JobNumber = items.JobName,
                        IsActive = items.IsActive ?? true,
                        ProjectType = items.ProjectType

                    };
                }

                return datas;
            }
            catch (Exception)
            {
                return null;
            }
        }

        public JsonResult GetBarge()
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
            string companyCode = OurUtility.ValueOf(Request, "company");
            DateTime date = DateTime.Now.AddDays(-30);
            try
            {
                using(MERCY_MineMarket_Ctx db = new MERCY_MineMarket_Ctx())
                {

                    var query =
                        (
                            from brg in db.VW_MM_MERCY_BARGE
                            where brg.START_ARRIVAL_DATE >= date
                                && (companyCode == string.Empty || brg.COMPANYCODE == companyCode || brg.COMPANYCODE == "DBS")  
                            orderby brg.SERVICE_TRIP
                            select new Barge
                            {
                              TripNumber = brg.SERVICE_TRIP,
                              Name = brg.BARGENAME
                            }
                        );
                    var datas = query.ToList();
                    var result = new { Success = true, Permission = permission_Item, User = UserX.Information(user, is_Show_User_Menu, is_Show_User_Relation), Message = string.Empty, MessageDetail = string.Empty, Version = Configuration.VERSION, Items = datas,  };
                    return Json(result, JsonRequestBehavior.AllowGet);
                }
            }catch(Exception ex)
            {
                var result = new { Success = false, Permission = permission_Item, ex.Message, MessageDetail = string.Empty, Version = Configuration.VERSION, Items = string.Empty, Total = 0 };
                return Json(result, JsonRequestBehavior.AllowGet);
            }

        }

        public JsonResult GetDestination()
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
            int project_id = OurUtility.ToInt32(OurUtility.ValueOf(Request, "project_id"));
            try
            {
                string code = string.Empty;
                using(MERCY_Ctx db = new MERCY_Ctx())
                {
                    var query =
                        (
                            from p in db.Projects
                            where p.Id == project_id
                            select new
                            {
                                p.Code
                            }
                        ).FirstOrDefault();
                    code = query.Code;
                }
                using (MERCY_MineMarket_Ctx db = new MERCY_MineMarket_Ctx())
                {
                    var query =
                        (
                            from stc in db.VW_MM_MERCY_STOCKPILE
                            where stc.ALIAS1.Contains(code)
                            select new Destination
                            {
                                Name= stc.NAME,
                                Code= stc.ALIAS1
                            }
                        );
                    var datas = query.ToList();
                    var result = new { Success = true, Permission = permission_Item, User = UserX.Information(user, is_Show_User_Menu, is_Show_User_Relation), Message = string.Empty, MessageDetail = string.Empty, Version = Configuration.VERSION, Items = datas, };
                    return Json(result, JsonRequestBehavior.AllowGet);
                }
            }
            catch(Exception ex)
            {
                var result = new { Success = false, Permission = permission_Item, ex.Message, MessageDetail = string.Empty, Version = Configuration.VERSION, Items = string.Empty, Total = 0 };
                return Json(result, JsonRequestBehavior.AllowGet);
            }

        }

        public JsonResult GetVesselDetail(string companyCode)
        {
            List<VesselDetail> vesselDetail= new List<VesselDetail>();
            using (MERCY_MineMarket_Ctx db = new MERCY_MineMarket_Ctx())
            {
                var twoMonthsAgo = DateTime.Today.AddDays(-61);
                var dataQuery =
                        (
                            from d in db.VW_MM_MERCY_VESSEL
                            where d.ETA > twoMonthsAgo
                            select new VesselDetail
                            {
                                Name = d.VESSEL_NAME,
                                Customer = d.CUSTOMER_NAME,
                                ShipmentNumber = d.SHIPMENT_IDENTIFIER,
                                LoadingNumber = d.SHIPMENT_IDENTIFIER,
                                DispatchId = d.DO,
                                ETA = d.ETA.ToString(),
                                ATA = d.ATA.ToString(),
                                Contract = d.CONTRACT,
                                Product = d.PRODUCT
                            }
                        ).Distinct();

                vesselDetail = dataQuery.Distinct().ToList();
            }
            using (MERCY_Ctx db = new MERCY_Ctx())
            {
                foreach(VesselDetail vessel in vesselDetail)
                {
                    var dataQuery =
                        (
                            from sdg in db.SampleDetailLoadings
                            join s in db.Samples on sdg.SampleId equals s.Id
                            where sdg.LoadingNumber == vessel.LoadingNumber &&
                                s.CompanyCode == companyCode
                            orderby sdg.LotNumber descending
                            select new
                            {
                                Number = sdg.LotNumber
                            }
                        ).FirstOrDefault();
                    if(dataQuery != null)
                    {
                        vessel.LotNumber = dataQuery.Number;
                    }else
                    {
                        vessel.LotNumber = 0;
                    }
                }

                var result = new { Data = vesselDetail };
                return Json(result, JsonRequestBehavior.AllowGet);
            }
        }
        public JsonResult GetLastSequence(SequenceShiftModelView seqShift)
        {
            try
            {
                using (MERCY_Ctx db = new MERCY_Ctx())
                {
                    var sequenceShift =
                        (
                            from s in db.Samples
                            join sdg in db.SampleDetailGenerals on s.Id equals sdg.Sample_Id
                            where
                                // * Saved logic to find minimum missing value
                                //!db.Samples.Any(s2 =>
                                //    s2.SampleDetailGenerals.FirstOrDefault().Sequence == sdg.Sequence + 1 &&
                                //    s2.SampleDetailGenerals.FirstOrDefault().Shift == sdg.Shift &&
                                //    s2.JobNumberId == s.JobNumberId
                                //) &&
                                s.CompanyCode == seqShift.CompanyCode &&
                                s.ClientId == seqShift.ClientId &&
                                s.ProjectId == seqShift.ProjectId &&
                                s.DateOfJob == seqShift.DateOfJob
                            select new SequenceShift
                            {
                                Shift = sdg.Shift,
                                Sequence = sdg.Sequence
                            }
                        ).ToList();

                    var list = sequenceShift
                        .GroupBy(x => x.Shift)
                        .Select(x => new  { Shift = x.Key, Sequence = x.Max(s => s.Sequence) });

                    var result = new { Data = list };
                    return Json(result, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                return Json(ex.Message, JsonRequestBehavior.AllowGet); ;
            }
        }
   
        public class Destination
        {
            public string Name { get; set; }   
            public string Code { get; set; }
        }
        public class Vessel
        {
            public string VesselName{ get; set; }
        }

        public class VesselDetail
        {
            public string Name { get; set; }
            public string Customer { get; set; }
            public string LoadingNumber { get; set; }
            public int LotNumber { get; set; }
            public string ShipmentNumber { get; set; }
            public string DispatchId { get; set; }
            public string ETA { get; set; }
            public string ATA { get; set; }
            public string Contract { get; set; }
            public string Product { get; set; }
        }
        public class Barge
        {
            public string Name { get; set; }
            public string TripNumber { get; set; }
        }
        
        
        
    }
}