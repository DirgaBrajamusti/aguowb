using MERCY.Data.EntityFramework;
using System;
using System.Collections.Generic;

namespace MERCY.Web.BackEnd.Models
{
    public class SampleDetailLoadingModelView
    {
        public string LoadingNumber { get; set; }
        public string VesselName { get; set; }
        public string DispatchId { get; set; }
        public string Customer { get; set; }
        public DateTime? ETA { get; set; }
        public DateTime? ATA { get; set; }
        public string Contract { get; set; }
        public string Product { get; set; }
        public int LotNumber { get; set; }
        public decimal Tonnage { get; set; }
        public DateTime SamplingStart { get; set; }
        public DateTime SamplingEnd { get; set; }
        public string LotSamples { get; set; }
    }
    public class SampleDetailGeneralModelView
    {
        public string GeoPrefix { get; set; }
        public string SampleId { get; set; }
        public int Shift { get; set; }
        public int Sequence { get; set; }
        public DateTime DateSampleStart { get; set; }
        public DateTime DateSampleEnd { get; set; }
        public DateTime Receive { get; set; }
        public decimal Tonnage { get; set; }
        public string Destination { get; set; }
        public string BargeName { get; set; }
        public string TripNumber { get; set; }
    }
    public class SampleDetailAMDModelView
    {
        public string GeoPrefix { get; set; }
        public string SampleId { get; set; }
        public int Shift { get; set; }
        public int Sequence { get; set; }
        public string LaboratoryId { get; set; }
        public string SampleType { get; set; }
        public DateTime DateSampleStart { get; set;}
        public DateTime DateSampleEnd { get; set;}
        public DateTime Receive { get; set; }
        public decimal MassSampleReceived { get; set; }
        public decimal TS { get; set; }
        public decimal ANC { get; set; }
        public decimal NAG { get; set; }
        public string NAGPH45 { get; set; }
        public string NAGPH70 { get; set; }
        public string NAGType { get; set; }
        public string Location { get; set; }
        public string Remark { get; set; }
    }

    public class SampleModelView
    {
        public string CompanyCode { get; set; }
        public int SiteId { get; set; }
        public DateTime DateOfJob { get; set; }
        public int ClientId { get; set; }
        public int ProjectId { get; set; }
        public int RefTypeId { get; set; }
        public string ReceivedBy { get; set; }
        public string Location { get; set; }
        public string Remark { get; set; }
        public decimal? ThicknessFrom { get; set; }
        public decimal? ThicknessTo { get; set; }
        public int? TunnelId { get; set; }
        public SampleDetailGeneralModelView DetailGeneral { get; set; }
        public SampleDetailLoadingModelView DetailLoading { get; set; }
        public SampleDetailAMDModelView DetailAMD { get; set; }

        public List<Scheme> Schemes { get; set; }
    }
    public class Scheme
    {
        public int SchemeId { get; set; }
    }

    public class AnalystJobModel
    {
        public int Id { get; set; }
        public string CompanyCode { get; set; }
        public string JobNumber { get; set; }
        public int ProjectId { get; set; }
        public string Status { get; set; }
        public DateTime JobDate { get; set; }
        public DateTime ReceivedDate { get; set; }
        public bool IsDetailGeneral { get; set; }

    }

    public class AnalysisResultModelView
    {
        public int Id { get; set; }
        public string Type { get; set; }
        public string Ident { get; set; }
        public string ExtIdent { get; set; }
        public string Attributes { get; set; }
        public decimal? Total { get; set; }
        public decimal? MinRepeatability { get; set; }
        public decimal? MaxRepeatability { get; set; }
        public int? SampleRegistrationId { get; set; }
        public List<AnalysisResultModelView> Child { get; set; }
    }

    public class AnalystModelView
    {
        public int Id { get; set; }
        public string JobNumber { get; set; }
        public string ClientCode { get; set; }
        public int ClientId { get; set; }
        public string ProjectName { get; set; }
        public int ProjectId { get; set; }
        public string Received { get; set; }
        public string ValidatedDate { get; set; }
        public string ApprovedDate { get; set; }

        public string MmStatus { get; set; }
        public string Status { get; set; }
    }

    public class SampleListModelView
    {
        public int Id { get; set; }
        public string CompanyCode { get; set; }
        public string JobNumber { get; set; }
        public string DateOfJob { get; set; }
        public string Stage { get; set; }
        public string SiteName { get; set; }
        public int ClientId { get; set; }
        public string ClientName { get; set; }
        public string SampleId { get; set; }
        public int ProjectId { get; set; }
        public string ProjectName { get; set; }
        public string ProjectType { get; set; }
        public string ExternalId { get; set; }
        public decimal Tonnage { get; set; }
        public string DateSampleStart { get; set; }
        public string DateSampleEnd { get; set; }
        public string Receive { get; set; }
        public int Shift { get; set; }
        public bool IsActive { get; set; }
        public int? TunnelId { get; set; }
        public string TunnelName { get; set; }
    }
    public class Model_View_Sample : Sample
    {
        public string SiteName { get; set; }
        public string ClientName { get; set; }
        public string ProjectName { get; set; }
        public string SampleId { get; set; }
        public decimal Tonnage { get; set; }
        public string JobName { get; set; }
        public string ProjectType { get; set; }
        public DateTime DateSampleStart { get; set; }
        public DateTime DateSampleEnd { get; set; }
        public DateTime Receive { get; set; }
        public int Shift { get; set; }
    }
    
    public class SampleValidateModelView
    {
        public string CompanyCode { get; set; }
        public string SampleId { get; set; }
        public int ProjectId { get; set; }
        public int ClientId { get; set; }
        public int Shift { get; set; }
        public int Sequence { get; set; }
        public DateTime DateOfJob { get; set; }
    }

    public class AnalysisResultUpdateModelView
    {
        public int Id { get; set; }
        public string Attributes { get; set; }
        public decimal Total { get; set; }
    }
    public class SampleUpdateModelView
    {
        public long Id { get; set; }
        public decimal? ThicknessTo { get; set; }
        public decimal? ThicknessFrom { get; set; }
        public string Location { get; set; }
        public string Remark { get; set; }
        public SampleDetailGeneralModelView DetailGeneral { get; set; }
        public SampleDetailLoadingModelView DetailLoading { get; set; }
        public bool? IsActive { get; set; }
        public int? TunnelId { get; set; }
    }
    public class SampleDetailModelView
    {
        public int Id { get; set; }
        public string CompanyCode { get; set; }
        public int SiteId { get; set; }
        public string ExternalId { get; set; }
        public string DateOfJob { get; set; }
        public int ClientId { get; set; }
        public int ProjectId { get; set; }
        public int RefTypeId { get; set; }
        public string CompanyName { get; set; }
        public string SiteName { get; set; }
        public string ClientName { get; set; }
        public string ProjectName { get; set; }
        public string RefTypeName { get; set; }
        public string CreatedOn_Str { get; set; }
        public string ReceivedBy { get; set; }
        public bool IsActive { get; set; }
        public string Remark { get; set; }
        public string Location { get; set; }
        public decimal ThicknessTo { get; set; }
        public decimal ThicknessFrom { get; set; }
        public string ProjectTypeName { get; set; }
        public int? TunnelId { get; set; }
        public DetailGeneralModelView DetailGeneral { get; set; }
        public DetailLoadingModelView DetailLoading { get; set; }
        public List<SampleSchemeModelView> SampleSchemes { get; set; }

    }
    public class SampleSchemeModelView
    {
        public int SampleSchemeId { get; set; }
        public int SchemeId { get; set; }
        public string SchemeName { get; set; }
    }
    public class DetailGeneralModelView
    {
        public string GeoPrefix { get; set; }
        public string SampleId { get; set; }
        public int Shift { get; set; }
        public int Sequence { get; set; }
        public string DateSampleStart { get; set; }
        public string DateSampleEnd { get; set; }
        public string Receive { get; set; }
        public decimal Tonnage { get; set; }
        public string Destination { get; set; }
        public string BargeName { get; set; }
        public string TripNumber { get; set; }
    
    }
    public class DetailLoadingModelView
    {
        public string LoadingNumber { get; set; }
        public string VesselName { get; set; }
        public string DispatchId { get; set; }
        public string Customer { get; set; }
        public string ETA { get; set; }
        public string ATA { get; set; }
        public string Contract { get; set; }
        public string Product { get; set; }
        public int LotNumber { get; set; }
        public decimal Tonnage { get; set; }
        public string SamplingStart { get; set; }
        public string SamplingEnd { get; set; }
        public string LotSamples { get; set; }
    }

    public class CalculateSample
    {
        public decimal TM { get; set; }
        public decimal IM { get; set; }
        public decimal CV { get; set; }
        public decimal TS { get; set; }
        public decimal ASH { get; set; }
        public decimal VM { get; set; }
        public decimal M { get; set; }
        public decimal FC { get; set; }
        public decimal CaO { get; set; }
        public decimal Na2O { get; set; }
        public decimal RD { get; set; }
        public decimal Size50 { get; set; }
        public decimal Size502 { get; set; }
        public decimal Size2 { get; set; }
        public decimal TGA { get; set; }
    }
    public class AnalysisHistoryModelView
    {
        public int SampleId { get; set; }
        public bool IsApproval { get; set; }
    }

    public class SequenceShiftModelView
    {
        public string CompanyCode { get; set; }
        public int ClientId { get; set; }
        public int ProjectId { get; set; }
        public DateTime DateOfJob { get; set; }
    }
    public class SequenceShift
    {
        public int Shift { get; set; }
        public int Sequence { get; set; }
    }
}