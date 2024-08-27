using System;

using MERCY.Data.EntityFramework;
using MERCY.Data.EntityFramework_BigData;

namespace MERCY.Web.BackEnd.Models
{
    public class Model_View_Menu : Menu
    {
        public string ParentName { get; set; }
        public string Name2 { get; set; }
        public bool isview { get; set; }
        public bool isadd { get; set; }
        public bool isdelete { get; set; }
        public bool isupdate { get; set; }
        public bool isacknowledge { get; set; }
        public bool isapprove { get; set; }
        public bool isemail { get; set; }
        public bool isactive { get; set; }
        public string hdr { get; set; }
    }

    class Model_View_AnalysisRequest_Detail : AnalysisRequest_Detail
    {
        public string CreatedOn_Str { get; set; }
        public string CreatedOn_Str2 { get; set; }
        public decimal From2 { get; set; }
        public decimal To2 { get; set; }
        public string DEPTH_FROM { get; set; }
        public string DEPTH_TO { get; set; }
        public string Status { get; set; }
        public bool SAMPLE_isvalid { get; set; }

        public string Total_Moisture { get; set; }
        public string Proximate_analysis { get; set; }
        public string Sulfur_content { get; set; }
        public string Calorific_value { get; set; }
        public string Relative_density { get; set; }
        public string CSN { get; set; }
        public string Ash_analysis { get; set; }
        public string HGI { get; set; }
        public string Ultimate_analysis { get; set; }
        public string Chlorine { get; set; }
        public string Phosphorous { get; set; }
        public string Fluorine { get; set; }
        public string Lead_ { get; set; }
        public string Zinc { get; set; }
        public string Form_of_Sulphur { get; set; }
        public string Ash_fusions_temperature { get; set; }
        public string TraceElement { get; set; }
    }

    class Model_View_AnalysisRequest_History : AnalysisRequest_History
    {
        public string CreatedOn_Str { get; set; }
        public string FullName { get; set; }
    }

    class Model_View_Company : Company
    {
        public string CreatedOn_Str { get; set; }
        public string CreatedOn_Str2 { get; set; }
        public string FullName { get; set; }
        public string SiteName { get; set; }
    }

    class Model_View_Sample_Registration : Sample
    {
        public string CompanyName { get; set; }
        public string SiteName { get; set; }
        public string ClientName { get; set; }
        public string ProjectName { get; set; }
        public string RefTypeName { get; set; }
        public string CreatedOn_Str { get; set; }
    }

    class Model_View_Sample_Scheme : SampleScheme
    {
        public string SchemeName { get; set; }
        public int CompanyClientProjectId { get; set; }
    }

   
    class Model_View_Group : Group
    {
        public string CreatedOn_Str { get; set; }
        public string CreatedOn_Str2 { get; set; }
        public string FullName { get; set; }
    }

    class Model_View_Instrument : Instrument
    {
        public string CreatedOn_Str { get; set; }
        public string CreatedOn_Str2 { get; set; }
        public string FullName { get; set; }
        public string SiteName { get; set; }
        public string CompanyName { get; set; }
    }

    class Model_View_MaintenanceActivity : MaintenanceActivity
    {
        public string CreatedOn_Str { get; set; }
        public string CreatedOn_Str2 { get; set; }
        public string FullName { get; set; }
        public string CompanyName { get; set; }
        public string SiteName { get; set; }
    }

    class Model_View_PortionBlending : PortionBlending
    {
        public string BlendingDate_Str { get; set; }
        public string ROM { get; set; }
        public string Portion { get; set; }
        public string Destination { get; set; }
        public string Remark_Str { get; set; }
    }

    class Model_View_PortionBlending_Details : PortionBlending_Details
    {
        public string CV_Str { get; set; }
        public int ROM_Id { get; set; }
        public string Names { get; set; }
        public string Block { get; set; }
    }

    class Model_View_LabMaintenance : LabMaintenance
    {
        public string CreatedOn_Str { get; set; }
        public string CreatedOn_Str2 { get; set; }
        public string FullName { get; set; }

        public string NextSchedule_Str { get; set; }
        public string NextSchedule_Str2 { get; set; }
        public string AssignedTo_FullName { get; set; }
        public string InstrumentName { get; set; }
        public string NextSchedule_Hour { get; set; }
        public string NextSchedule_Minute { get; set; }
        public string NextSchedule_AmPm { get; set; }
        public string Status { get; set; }
        public string CategoryName { get; set; }
        public string CompanyName { get; set; }
        public string SiteName { get; set; }
    }

    class Model_View_LabMaintenance_History : LabMaintenance_History
    {
        public string CreatedOn_Str { get; set; }
        public string Date_Str { get; set; }
        public string FullName { get; set; }

        public string InstrumentName { get; set; }
        public string CategoryName { get; set; }
    }

    class Model_View_HaulingRequest : HaulingRequest
    {
        public string CreatedOn_Str { get; set; }
        public string Company { get; set; }
        public string Source { get; set; }
        public string Destination { get; set; }
        public string Quality { get; set; }
        public string Remark { get; set; }
    }

    class Model_View_Consumable_History : Consumable_History
    {
        public string CreatedOn_Str { get; set; }
        public string Date_Str { get; set; }
        public string FullName { get; set; }
    }

    class Model_View_Mercy_ROM_Quality : Mercy_ROM_Quality
    {
        public string Names { get; set; }
        public string Ton_Str { get; set; }
        public string CV_Str { get; set; }
        public string TS_Str { get; set; }
        public string ASH_Str { get; set; }
        public string IM_Str { get; set; }
        public string TM_Str { get; set; }
        public string Process_Date_Str { get; set; }
    }

    class Model_View_Local_ROM : ROM
    {
        public string Names { get; set; }
        public string Ton_Str { get; set; }
        public string CV_Str { get; set; }
        public string TS_Str { get; set; }
        public string ASH_Str { get; set; }
        public string IM_Str { get; set; }
        public string TM_Str { get; set; }
        public string Process_Date_Str { get; set; }
        public string Block { get; set; }
        public string ROM_Name { get; set; }
        public int ROM_Id { get; set; }
        public string company_code { get; set; }
        public Nullable<System.DateTime> Process_Date { get; set; }
    }

    class Model_View_Coal_Inventory : CoalInventory
    {
        public string StartTimeString { get; set; }
        public string EndTimeString { get; set; }
        public string ROMLocationName { get; set; }
        public string Code { get; set; }
    }

    class Model_View_Coal_Inventory_Detail : CoalInventoryDetail
    {
        public string ROMLocationDetailName { get; set; }
    }

    class Model_View_Mercy_quality_outlook : Mercy_quality_outlook
    {
        public string CV_Str { get; set; }
        public string TS_Str { get; set; }
        public string ASH_Str { get; set; }
        public string IM_Str { get; set; }
        public string TM_Str { get; set; }
        public double? ASH { get; set; }
    }

    class Model_View_Local_Product : Product
    {
        public string CV_Str { get; set; }
        public string TS_Str { get; set; }
        public string ASH_Str { get; set; }
        public string IM_Str { get; set; }
        public string TM_Str { get; set; }
        public int id { get; set; }
        public string Product_name { get; set; }
        public string FirstDate { get; set; }
        public string Year { get; set; }
        public string months { get; set; }
    }

    class Model_View_TEMPORARY_Sampling_ROM : TEMPORARY_Sampling_ROM
    {
        public long data_RecordId { get; set; }
        public string TM_Str { get; set; }
        public string M_Str { get; set; }
        public string ASH_Str { get; set; }
        public string TS_Str { get; set; }
        public string CV_Str { get; set; }
    }

    class Model_View_TEMPORARY_Geology_Pit_Monitoring : TEMPORARY_Geology_Pit_Monitoring
    {
        public long data_RecordId { get; set; }
        public string Mass_Spl_Str { get; set; }
        public string TM_Str { get; set; }
        public string M_Str { get; set; }
        public string VM_Str { get; set; }
        public string Ash_Str { get; set; }
        public string FC_Str { get; set; }
        public string TS_Str { get; set; }
        public string Cal_ad_Str { get; set; }
        public string Cal_db_Str { get; set; }
        public string Cal_ar_Str { get; set; }
        public string Cal_daf_Str { get; set; }
        public string RD_Str { get; set; }
    }

    class Model_View_TEMPORARY_TEMPORARY_Geology_Explorasi : TEMPORARY_Geology_Explorasi
    {
        public long data_RecordId { get; set; }
        public string Mass_Spl_Str { get; set; }
        public string TM_Str { get; set; }
        public string M_Str { get; set; }
        public string VM_Str { get; set; }
        public string Ash_Str { get; set; }
        public string FC_Str { get; set; }
        public string TS_Str { get; set; }
        public string Cal_ad_Str { get; set; }
        public string Cal_db_Str { get; set; }
        public string Cal_ar_Str { get; set; }
        public string Cal_daf_Str { get; set; }
        public string RD_Str { get; set; }
    }

    class Model_View_TEMPORARY_TEMPORARY_BARGE_LOADING : TEMPORARY_BARGE_LOADING
    {
        public long data_RecordId { get; set; }
        public string Tonnage_Str { get; set; }
        public string Temperature_Str { get; set; }
        public string TM_Str { get; set; }
        public string M_Str { get; set; }
        public string ASH_adb_Str { get; set; }
        public string ASH_arb_Str { get; set; }
        public string VM_adb_Str { get; set; }
        public string VM_arb_Str { get; set; }
        public string FC_adb_Str { get; set; }
        public string FC_arb_Str { get; set; }
        public string TS_adb_Str { get; set; }
        public string TS_arb_Str { get; set; }
        public string CV_adb_Str { get; set; }
        public string CV_db_Str { get; set; }
        public string CV_arb_Str { get; set; }
        public string CV_daf_Str { get; set; }
        public string CV_ad_15_Str { get; set; }
        public string CV_ad_16_Str { get; set; }
        public string CV_ad_17_Str { get; set; }
    }

    class Model_View_TEMPORARY_CRUSHING_PLANT : TEMPORARY_CRUSHING_PLANT
    {
        public long data_RecordId { get; set; }
        public string Tonnage_Str { get; set; }
        public string TM_Str { get; set; }
        public string M_Str { get; set; }
        public string ASH_adb_Str { get; set; }
        public string ASH_arb_Str { get; set; }
        public string VM_adb_Str { get; set; }
        public string VM_arb_Str { get; set; }
        public string FC_adb_Str { get; set; }
        public string FC_arb_Str { get; set; }
        public string TS_adb_Str { get; set; }
        public string TS_arb_Str { get; set; }
        public string CV_adb_Str { get; set; }
        public string CV_db_Str { get; set; }
        public string CV_arb_Str { get; set; }
        public string CV_daf_Str { get; set; }
        public string CV_ad_15_Str { get; set; }
        public string CV_ad_16_Str { get; set; }
        public string CV_ad_17_Str { get; set; }
    }

    class Model_View_ROMTransfer : ROMTransfer
    {
        public string TransferDate_Str { get; set; }
        public string Source { get; set; }
        public string Destination { get; set; }
    }

    class Model_View_SamplingRequest_Lab : SamplingRequest_Lab
    {
        public string CreatedOn_Str { get; set; }
        public string CreatedOn_Str2 { get; set; }
    }

    class Model_View_SamplingRequest_History : SamplingRequest_History
    {
        public string CreatedOn_Str { get; set; }
        public string FullName { get; set; }
    }

    class Model_View_Site : Site
    {
        public string CreatedOn_Str { get; set; }
        public string CreatedOn_Str2 { get; set; }
        public string FullName { get; set; }
    }

    class Model_View_UnitMeasurement : UnitMeasurement
    {
        public string CreatedOn_Str { get; set; }
        public string CreatedOn_Str2 { get; set; }
        public string FullName { get; set; }
    }

    class Model_View_Feedback : Feedback
    {
        public string CreatedOn_Str { get; set; }
        public string CreatedOn_Str2 { get; set; }
        public string FullName { get; set; }
    }

    public class Model_View_Site2
    {
        public int SiteId { get; set; }
        public string SiteName { get; set; }
    }

    public class Model_View_Company2
    {
        public string CompanyCode { get; set; }
        public string CompanyName { get; set; }
        public int SiteId { get; set; }
    }
        
    public class Model_View_Group2
    {
        public int GroupId { get; set; }
        public string GroupName { get; set; }
    }

    class Model_View_Tunnel : Tunnel
    {
        public string CreatedOn_Str { get; set; }
        public string CreatedOn_Str2 { get; set; }
        public string FullName { get; set; }
        public string CompanyName { get; set; }
        public string Product_Str { get; set; }
        public string EffectiveDate_Str { get; set; }
        public int UserId { get; set; }
        public string Title { get; set; }
        public string Department { get; set; }
    }

    class Model_View_Hopper : Hopper
    {
        public string CreatedOn_Str { get; set; }
        public string CreatedOn_Str2 { get; set; }
        public string FullName { get; set; }
        public string CompanyName { get; set; }
    }

    class Model_View_TunnelManagement
    {
        public long RecordIdx { get; set; }
        public DateTime BlendingDate { get; set; }
        public string BlendingDate_Str { get; set; }
        public string BlendingDate_Str2 { get; set; }
        public string BlendingDate_Str3 { get; set; }
        public int Shift { get; set; }
        public string Site { get; set; }
        public string Company { get; set; }
        public string Source { get; set; }
        public string Hopper { get; set; }
        public string Tunnel { get; set; }
        public string Product { get; set; }
        public string Quality { get; set; }
        public string Tunnel_Actual_Text { get; set; }
        public string Tunnel_Actual_Time { get; set; }
        public string Tunnel_Actual_Remark { get; set; }
        public string Remark { get; set; }
        public string NoHauling { get; set; }
        public string RecordId_Snapshot { get; set; }
        public bool Editable { get; set; }
    }

    class Model_View_TunnelManagement_ROM
    {
        public string Block { get; set; }
        public string ROM_Name { get; set; }
        public string ROM_ID { get; set; }
        public string CV { get; set; }
        public string TS { get; set; }
        public string ASH { get; set; }
        public string IM { get; set; }
        public string TM { get; set; }
        public string Ton { get; set; }
        public int Portion { get; set; }
        public string PortionBlending { get; set; }
    }

    class Model_View_Tunnel_Actual : Tunnel_Actual
    {
        public string Tunnel_Name { get; set; }
        public string Time_Str { get; set; }
        public string Time_Str2 { get; set; }
        public string Time_Hour { get; set; }
        public string Time_Minute { get; set; }
    }

    class Model_View_UserList_access_Menu
    {
        public string UserId { get; set; }
        public string LoginName { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }
        public bool Is_ActiveDirectory { get; set; }
        public string GroupId { get; set; }
        public string GroupName { get; set; }
        public bool Is_View { get; set; }
        public bool Is_Add { get; set; }
        public bool Is_Delete { get; set; }
        public bool Is_Update { get; set; }
        public bool Is_Acknowledge { get; set; }
        public bool Is_Approve { get; set; }
        public bool Is_Email { get; set; }
        public bool Is_Active { get; set; }
    }

    class Model_View_DirectShipment : DirectShipment
    {
        public string CV_Str { get; set; }
        public string TS_Str { get; set; }
        public string ASH_Str { get; set; }
        public string IM_Str { get; set; }
        public string TM_Str { get; set; }
        public string Destination_Str { get; set; }
    }

    class Model_View_Product : Product
    {
        public string CV_Str { get; set; }
        public string TS_Str { get; set; }
        public string ASH_Str { get; set; }
        public string IM_Str { get; set; }
        public string TM_Str { get; set; }
        public string CompanyName { get; set; }
    }

    class Model_View_ROM : ROM
    {
        public string CV_Str { get; set; }
        public string TS_Str { get; set; }
        public string ASH_Str { get; set; }
        public string IM_Str { get; set; }
        public string TM_Str { get; set; }
        public string CompanyName { get; set; }
    }

    class Model_TEMPORARY_Barge_Quality_Plan : TEMPORARY_Barge_Quality_Plan
    {
        public string Barge_Ton_plan_Str { get; set; }
        public string Barge_Ton_actual_Str { get; set; }
        public string TM_plan_Str { get; set; }
        public string TM_actual_Str { get; set; }
        public string M_plan_Str { get; set; }
        public string M_actual_Str { get; set; }
        public string ASH_plan_Str { get; set; }
        public string ASH_actual_Str { get; set; }
        public string TS_plan_Str { get; set; }
        public string TS_actual_Str { get; set; }
        public string CV_ADB_plan_Str { get; set; }
        public string CV_ADB_actual_Str { get; set; }
        public string CV_AR_plan_Str { get; set; }
        public string CV_AR_actual_Str { get; set; }

        public string Sheet { get; set; }
        public string TripNo { get; set; }
        public string Vessel { get; set; }
        public string Buyer { get; set; }
        public string LaycanFrom { get; set; }
        public string LaycanTo { get; set; }
    }

    class Model_TEMPORARY_Barge_Line_Up : TEMPORARY_Barge_Line_Up
    {
        public string Capacity_Str { get; set; }
        public string EstimateStartLoading_Str { get; set; }
    }

    class Model_View_Discussion : Discussion
    {
        public string CreatedOn_Str { get; set; }
        public string CreatedOn_Str2 { get; set; }
        public string FullName { get; set; }
    }

    class Model_View_Values
    {
        public string Name { get; set; }
        public string Description { get; set; }
    }

    class Model_View_File
    {
        public string RecordId { get; set; }
        public string CreatedBy { get; set; }
        public string CreatedBy_Str { get; set; }
        public string CreatedOn { get; set; }
        public string CreatedOn_Str { get; set; }
        public string FileName { get; set; }
        public string Link { get; set; }
        public string FileType { get; set; }
        public string Company { get; set; }
    }

    class Model_View_ID
    {
        public string RecordId { get; set; }
        public string Id { get; set; }
        public string CreatedOn { get; set; }
        public string CreatedOn_Str { get; set; }
        public string LastModifiedOn { get; set; }
        public string LastModifiedOn_Str { get; set; }
        public string Type { get; set; }
        public string Company { get; set; }
    }

    class Model_View_Parameter_Analysis_Request
    {
        public bool TM { get; set; }
        public bool Prox { get; set; }
        public bool TS { get; set; }
        public bool CV { get; set; }
        public bool RD { get; set; }
        public bool CSN { get; set; }
        public bool AA { get; set; }
        public bool HGI { get; set; }
        public bool Ultimate { get; set; }
        public bool Chlorine { get; set; }
        public bool Phosporus { get; set; }
        public bool Fluorine { get; set; }
        public bool Lead { get; set; }
        public bool Zinc { get; set; }
        public bool AFT { get; set; }
        public bool TraceElement { get; set; }
    }

    class Model_View_TEMPORARY_HAC : TEMPORARY_HAC
    {
        public long data_RecordId { get; set; }
        public string TM_Str { get; set; }
        public string M_Str { get; set; }
        public string ASH_Str { get; set; }
        public string TS_Str { get; set; }
        public string CV_Str { get; set; }
        public string Tonnage_Str { get; set; }
    }

    class Model_TEMPORARY_VLU : TEMPORARY_VLU
    {
        public string LAYCAN_From_Str { get; set; }
        public string LAYCAN_To_Str { get; set; }
        public string ETA_Str { get; set; }
        public string ATA_Str { get; set; }
        public string Commenced_Loading_Str { get; set; }
        public string Completed_Loading_Str { get; set; }
        public string DEM_Str { get; set; }
    }

    public class Model_Loading_Plan : Loading_Plan
    {
        public string Request_Id { get; set; }
        public string Loading_Plan_Id { get; set; }
        public string Site_Str { get; set; }
        public string CreatedOn_Str { get; set; }
        public string CreatedOn_Str2 { get; set; }
        public DateTime SubmittedOn { get; set; }
        public string SubmittedOn_Str { get; set; }
        public string SubmittedOn_Str2 { get; set; }
        public string Submitted_By { get; set; }
        public string FullName { get; set; }
        public string Tug { get; set; }
        public string Barge { get; set; }
        public string ETA_Vessel_MBR { get; set; }
        public string Estimate_Start_Loading { get; set; }
        public string Estimate_Quantity { get; set; }
        public string Product { get; set; }
        public string Customer { get; set; }
        public string Customer_1 { get; set; }
        public string Customer_2 { get; set; }
        public string PlanBlendingLoading { get; set; }
        public int TN1 { get; set; }
        public int TN2 { get; set; }
        public int TN3 { get; set; }
        public int TN4 { get; set; }
        public int TN5 { get; set; }
        public int TN6 { get; set; }
        public int BTN1 { get; set; }
        public int BTN2 { get; set; }
        public int BTN3 { get; set; }
        public int BTN4 { get; set; }
        public int BTN5 { get; set; }
        public int BTN6 { get; set; }
        public string TN1_Str { get; set; }
        public string TN2_Str { get; set; }
        public string TN3_Str { get; set; }
        public string TN4_Str { get; set; }
        public string TN5_Str { get; set; }
        public string TN6_Str { get; set; }
        public string BTN1_Str { get; set; }
        public string BTN2_Str { get; set; }
        public string BTN3_Str { get; set; }
        public string BTN4_Str { get; set; }
        public string BTN5_Str { get; set; }
        public string BTN6_Str { get; set; }
        public long Detail_Barge_Id { get; set; }
    }

    class Model_Loading_Plan__Detail_Barge
    {
        public string RecordId { get; set; }
        public string Tug { get; set; }
        public string Barge { get; set; }
        public string TripID { get; set; }
        public string EstimateStartLoading { get; set; }
        public DateTime EstimateStartLoading_D { get; set; }
        public string EstimateStartLoading_Str { get; set; }
        public string Product { get; set; }
        public string Capacity { get; set; }
        public string CV_ADB_plan { get; set; }
        public string CV_ADB_actual { get; set; }
        public string TS_plan { get; set; }
        public string TS_actual { get; set; }
        public string ASH_plan { get; set; }
        public string ASH_actual { get; set; }
        public string M_plan { get; set; }
        public string M_actual { get; set; }
        public string TM_plan { get; set; }
        public string TM_actual { get; set; }
    }

    class Model_Loading_Plan__Buyer
    {
        public string BuyerName { get; set; }
    }

    class Model_Id_Text
    {
        public string id { get; set; }
        public string text { get; set; }
        public string parent_id { get; set; }
    }

    class Model_View_Loading_Request_Loading_Plan : Loading_Request_Loading_Plan_Detail_Barge
    {
        public int No_Ref_Report { get; set; }
        public int Barge_Size { get; set; }
        public string Coal_Quality { get; set; }
        public string Shipment_Type { get; set; }
        public string Route { get; set; }
    }

    class Model_View_Loading_Actual : Loading_Actual
    {
        public string RequestedOn_Str { get; set; }
        public string SiteName { get; set; }
        public string Arrival_Str { get; set; }
        public string Departure_Str { get; set; }
        public string Cargo_Loaded { get; set; }
    }

    class Model_View_Loading_Actual_Detail : Loading_Actual
    {
        public string RequestedOn_Str { get; set; }
        public string SiteName { get; set; }
        public string Cargo_Loaded { get; set; }
        public string Arrival_Str { get; set; }
        public string Initial_Draft_Str { get; set; }
        public string Anchor_Up_Str { get; set; }
        public string Berthed_Time_Str { get; set; }
        public string Commenced_Loading_Str { get; set; }
        public string Completed_Loading_Str { get; set; }
        public string Unberthing_Str { get; set; }
        public string Departure_Str { get; set; }

        public string Arrival_Str2 { get; set; }
        public string Initial_Draft_Str2 { get; set; }
        public string Anchor_Up_Str2 { get; set; }
        public string Berthed_Time_Str2 { get; set; }
        public string Commenced_Loading_Str2 { get; set; }
        public string Completed_Loading_Str2 { get; set; }
        public string Unberthing_Str2 { get; set; }
        public string Departure_Str2 { get; set; }
    }

    class Model_View_Loading_Actual_Cargo_Loaded : Loading_Actual_Cargo_Loaded
    {
        public string TunnelName { get; set; }
    }

    class Model_View_Loading_Actual_Fuel_Consumption : Loading_Actual_Fuel_Consumption
    {
        public string Fuel_Rob_Name { get; set; }
    }

    class Model_View_Loading_Actual_Draft_Survey : Loading_Actual_Draft_Survey
    {
        public string Draft_Survey_Name { get; set; }
    }

    class Model_View_Loading_Actual_Barge_Survey_Condition : Loading_Actual_Barge_Survey_Condition
    {
        public string Barge_Survey_Name { get; set; }
    }

    class Model_View_Port_Loading
    {
        public string ProductId { get; set; }
        public string ProductName { get; set; }
        public string CompanyCode { get; set; }
        public string Periode { get; set; }
        public string RecordId { get; set; }
        public string Type { get; set; }
        public string Tonnage { get; set; }
        public string CV { get; set; }
        public string TS { get; set; }
        public string ASH { get; set; }
        public string IM { get; set; }
        public string TM { get; set; }
    }

    class Model_Default_Actual_Barge_Loading
    {
        public string Request_Id { get; set; }
        public string CompanyCode { get; set; }
        public string Tug_Name { get; set; }
        public string No_Service_Trip { get; set; }
        public string No_Ref_Report { get; set; }
        public string BargeName { get; set; }
        public string BargeSize { get; set; }
        public string Route { get; set; }
        public string LoadType { get; set; }
        public string Coal_Quality { get; set; }
        public string Shipment_Type { get; set; }
    }

    public class Model_Product : Mercy_quality_outlook
    {
        public string CompanyCode { get; set; }
        public double? ASH { get; set; }
    }

    class Model_View_Tunnel_History
    {
        public int RecordId { get; set; }
        public string Company { get; set; }
        public string Name { get; set; }
        public string Product { get; set; }
        public string EffectiveDate { get; set; }
        public DateTime? EffectiveDate_Date { get; set; }
        public DateTime CreatedOn { get; set; }
        public DateTime? LastModifiedOn { get; set; }
        public string Remark { get; set; }
    }

    class Model_Tunnel_Quality
    {
        public string TunnelID { get; set; }
        public string Company_Code { get; set; }
        public string LatestDate { get; set; }
        public string DateToday { get; set; }
        public string Tunnel { get; set; }
        public string Ton { get; set; }
        public string Portion { get; set; }
        public string CV { get; set; }
        public string TS { get; set; }
        public string ASH { get; set; }
        public string IM { get; set; }
        public string TM { get; set; }

        public string CV_Str { get; set; }
        public string TS_Str { get; set; }
        public string ASH_Str { get; set; }
        public string IM_Str { get; set; }
        public string TM_Str { get; set; }

        public string Detail_Barge_Id { get; set; }
    }

    class ExternalPitModelView
    {
        public string Company_code { get; set; }

        public string pit { get; set; }

        public string Names { get; set; }
    }

    class ExternalRomModelView
    {
        public int ROM_id { get; set; }

        public string company_code { get; set; }

        public string Block { get; set; }

        public string ROM_Name { get; set; }

        public string Names { get; set; }
    }

    class ExternalRomSimpleModelView
    {
        public int ROM_id { get; set; }

        public string ROM_Name { get; set; }
    }

    class ExternalBlockSimpleModelView
    {
        public string company_code { get; set; }

        public string Block { get; set; }
    }

    class ExternalSeamModelView
    {
        public string Company_code { get; set; }

        public string pit { get; set; }

        public string seam { get; set; }

        public string Names { get; set; }
    }

    class SizeSchemeAttributeModelView
    {
        public decimal M1 { get; set; }

        public decimal A { get; set; }

        public decimal B { get; set; }

        public decimal C { get; set; }
    }

    class Model_View_Input_Period
    {
        public DateTime MaxDate { get; set; }
        public bool IsDateValid { get; set; }
    }

    public class AnalysisResultValidationModel
    {
        public int JobNumberId { get; set; }
        public bool IsJobCompleted { get; set; }
    }
}
