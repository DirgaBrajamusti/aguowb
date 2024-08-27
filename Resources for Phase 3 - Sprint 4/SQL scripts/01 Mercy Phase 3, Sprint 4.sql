CREATE TABLE Product
(
    ProductId INT NOT NULL IDENTITY(1,1)
    , CompanyCode VARCHAR(32) NOT NULL
    , ProductName VARCHAR(255) NOT NULL
    , CV NUMERIC(10,2) NOT NULL DEFAULT 0
    , TS NUMERIC(10,2) NOT NULL DEFAULT 0
    , ASH NUMERIC(10,2) NOT NULL DEFAULT 0
    , IM NUMERIC(10,2) NOT NULL DEFAULT 0
    , TM NUMERIC(10,2) NOT NULL DEFAULT 0
    , CreatedBy INT NOT NULL DEFAULT 0
    , CreatedOn datetime NOT NULL DEFAULT GetDate()
    , LastModifiedBy INT NOT NULL DEFAULT 0
    , LastModifiedOn datetime NULL
    , IsActive bit NOT NULL DEFAULT 0
    , CONSTRAINT PK_Product_TunnelId PRIMARY KEY (ProductId)
    , CONSTRAINT FK_Product_CompanyCode FOREIGN KEY (CompanyCode) REFERENCES Company(CompanyCode)
);

CREATE TABLE ROM
(
    ROMId INT NOT NULL IDENTITY(1,1)
    , CompanyCode VARCHAR(32) NOT NULL
    , ROMName VARCHAR(255) NOT NULL
    , CV NUMERIC(10,2) NOT NULL DEFAULT 0
    , TS NUMERIC(10,2) NOT NULL DEFAULT 0
    , ASH NUMERIC(10,2) NOT NULL DEFAULT 0
    , IM NUMERIC(10,2) NOT NULL DEFAULT 0
    , TM NUMERIC(10,2) NOT NULL DEFAULT 0
    , CreatedBy INT NOT NULL DEFAULT 0
    , CreatedOn datetime NOT NULL DEFAULT GetDate()
    , LastModifiedBy INT NOT NULL DEFAULT 0
    , LastModifiedOn datetime NULL
    , IsActive bit NOT NULL DEFAULT 0
    , CONSTRAINT PK_ROM_TunnelId PRIMARY KEY (ROMId)
    , CONSTRAINT FK_ROM_CompanyCode FOREIGN KEY (CompanyCode) REFERENCES Company(CompanyCode)
);

create table DirectShipment
(
    RecordId INT NOT NULL IDENTITY(1,1)
    , BuyerName VARCHAR(255) NOT NULL
    , Destination VARCHAR(8000) NOT NULL DEFAULT ''
    , CV NUMERIC(10,2) NOT NULL DEFAULT 0
    , TS NUMERIC(10,2) NOT NULL DEFAULT 0
    , ASH NUMERIC(10,2) NOT NULL DEFAULT 0
    , IM NUMERIC(10,2) NOT NULL DEFAULT 0
    , TM NUMERIC(10,2) NOT NULL DEFAULT 0
    , CreatedBy INT NOT NULL DEFAULT 0
    , CreatedOn datetime NOT NULL DEFAULT GetDate()
    , LastModifiedBy INT NOT NULL DEFAULT 0
    , LastModifiedOn datetime NULL
    , IsActive bit NOT NULL DEFAULT 0
    , CONSTRAINT PK_DirectShipment_RecordId PRIMARY KEY (RecordId)
);

/**

start from here to update

**/


create table TEMPORARY_Barge_Quality_Plan_Header
(
    RecordId BIGINT NOT NULL IDENTITY(1,1)
    , File_Physical BIGINT NOT NULL
    , Site INT NOT NULL DEFAULT 0
    , Sheet VARCHAR(255) NOT NULL DEFAULT ''
    , TripNo varchar(255) not null default ''
    , Vessel varchar(255) not null default ''
    , Buyer varchar(255) not null default ''
    , LaycanFrom varchar(255) not null default ''
    , LaycanTo varchar(255) not null default ''
    , CreatedBy INT NOT NULL DEFAULT 0
    , CreatedOn datetime NOT NULL DEFAULT GetDate()
    , CONSTRAINT PK_TEMPORARY_Barge_Quality_Plan_Header_RecordId PRIMARY KEY (RecordId)
    , CONSTRAINT FK_TEMPORARY_Barge_Quality_Plan_Header_File_Physical FOREIGN KEY (File_Physical) REFERENCES TEMPORARY_File (RecordId)
);

/**
 delete first this table
 
**/

create table TEMPORARY_Barge_Quality_Plan
(
    RecordId BIGINT NOT NULL IDENTITY(1,1)
    , Header BIGINT NOT NULL DEFAULT 0
    , Status VARCHAR(255) NOT NULL DEFAULT 'New'
    , Barge_Ton_plan varchar(255) not null default ''
    , Barge_Ton_actual varchar(255) not null default ''
    , TM_plan varchar(255) not null default ''
    , TM_actual varchar(255) not null default ''
    , M_plan varchar(255) not null default ''
    , M_actual varchar(255) not null default ''
    , ASH_plan varchar(255) not null default ''
    , ASH_actual varchar(255) not null default ''
    , TS_plan varchar(255) not null default ''
    , TS_actual varchar(255) not null default ''
    , CV_ADB_plan varchar(255) not null default ''
    , CV_ADB_actual varchar(255) not null default ''
    , CV_AR_plan varchar(255) not null default ''
    , CV_AR_actual varchar(255) not null default ''
    , Product varchar(255) not null default ''
    , TugName varchar(255) not null default ''
    , BargeName varchar(255) not null default ''
    
    , Barge_Ton_plan_isvalid bit NOT NULL DEFAULT 0
    , Barge_Ton_actual_isvalid bit NOT NULL DEFAULT 0
    , TM_plan_isvalid bit NOT NULL DEFAULT 0
    , TM_actual_isvalid bit NOT NULL DEFAULT 0
    , M_plan_isvalid bit NOT NULL DEFAULT 0
    , M_actual_isvalid bit NOT NULL DEFAULT 0
    , ASH_plan_isvalid bit NOT NULL DEFAULT 0
    , ASH_actual_isvalid bit NOT NULL DEFAULT 0
    , TS_plan_isvalid bit NOT NULL DEFAULT 0
    , TS_actual_isvalid bit NOT NULL DEFAULT 0
    , CV_ADB_plan_isvalid bit NOT NULL DEFAULT 0
    , CV_ADB_actual_isvalid bit NOT NULL DEFAULT 0
    , CV_AR_plan_isvalid bit NOT NULL DEFAULT 0
    , CV_AR_actual_isvalid bit NOT NULL DEFAULT 0
    , Product_isvalid bit NOT NULL DEFAULT 0
    , TugName_isvalid bit NOT NULL DEFAULT 0
    , BargeName_isvalid bit NOT NULL DEFAULT 0
    
    , CreatedBy INT NOT NULL DEFAULT 0
    , CreatedOn datetime NOT NULL DEFAULT GetDate()
    , CONSTRAINT PK_TEMPORARY_Barge_Quality_Plan_RecordId PRIMARY KEY (RecordId)
    , CONSTRAINT FK_TEMPORARY_Barge_Quality_Plan_Header FOREIGN KEY (Header) REFERENCES TEMPORARY_Barge_Quality_Plan_Header(RecordId)
);

alter table TEMPORARY_BARGE_LOADING add CV_Plan varchar(255) not null default '';
--alter table TEMPORARY_BARGE_LOADING add CV_Average varchar(255) not null default '';
alter table TEMPORARY_BARGE_LOADING add TM_Plan varchar(255) not null default '';
--alter table TEMPORARY_BARGE_LOADING add TM_Average varchar(255) not null default '';
alter table TEMPORARY_BARGE_LOADING add ASH_Plan varchar(255) not null default '';
--alter table TEMPORARY_BARGE_LOADING add ASH_Average varchar(255) not null default '';
alter table TEMPORARY_BARGE_LOADING add TS_Plan varchar(255) not null default '';
--alter table TEMPORARY_BARGE_LOADING add TS_Average varchar(255) not null default '';

alter table UPLOAD_BARGE_LOADING add CV_Plan numeric(10,2) NOT NULL DEFAULT 0;
--alter table UPLOAD_BARGE_LOADING add CV_Average numeric(10,2) NOT NULL DEFAULT 0;
alter table UPLOAD_BARGE_LOADING add TM_Plan numeric(10,2) NOT NULL DEFAULT 0;
--alter table UPLOAD_BARGE_LOADING add TM_Average numeric(10,2) NOT NULL DEFAULT 0;
alter table UPLOAD_BARGE_LOADING add ASH_Plan numeric(10,2) NOT NULL DEFAULT 0;
--alter table UPLOAD_BARGE_LOADING add ASH_Average numeric(10,2) NOT NULL DEFAULT 0;
alter table UPLOAD_BARGE_LOADING add TS_Plan numeric(10,2) NOT NULL DEFAULT 0;
--alter table UPLOAD_BARGE_LOADING add TS_Average numeric(10,2) NOT NULL DEFAULT 0;

alter table TEMPORARY_CRUSHING_PLANT add CV_Plan varchar(255) not null default '';
--alter table TEMPORARY_CRUSHING_PLANT add CV_Average varchar(255) not null default '';
alter table TEMPORARY_CRUSHING_PLANT add TM_Plan varchar(255) not null default '';
--alter table TEMPORARY_CRUSHING_PLANT add TM_Average varchar(255) not null default '';
alter table TEMPORARY_CRUSHING_PLANT add ASH_Plan varchar(255) not null default '';
--alter table TEMPORARY_CRUSHING_PLANT add ASH_Average varchar(255) not null default '';
alter table TEMPORARY_CRUSHING_PLANT add TS_Plan varchar(255) not null default '';
--alter table TEMPORARY_CRUSHING_PLANT add TS_Average varchar(255) not null default '';

alter table UPLOAD_CRUSHING_PLANT add CV_Plan numeric(10,2) NOT NULL DEFAULT 0;
--alter table UPLOAD_CRUSHING_PLANT add CV_Average numeric(10,2) NOT NULL DEFAULT 0;
alter table UPLOAD_CRUSHING_PLANT add TM_Plan numeric(10,2) NOT NULL DEFAULT 0;
--alter table UPLOAD_CRUSHING_PLANT add TM_Average numeric(10,2) NOT NULL DEFAULT 0;
alter table UPLOAD_CRUSHING_PLANT add ASH_Plan numeric(10,2) NOT NULL DEFAULT 0;
--alter table UPLOAD_CRUSHING_PLANT add ASH_Average numeric(10,2) NOT NULL DEFAULT 0;
alter table UPLOAD_CRUSHING_PLANT add TS_Plan numeric(10,2) NOT NULL DEFAULT 0;
--alter table UPLOAD_CRUSHING_PLANT add TS_Average numeric(10,2) NOT NULL DEFAULT 0;

CREATE TABLE [UPLOAD_BARGE_LOADING_COA](
	[RecordId] [bigint] IDENTITY(1,1) NOT NULL,
    [TEMPORARY] BIGINT NOT NULL DEFAULT 0,
	[Sheet] [nvarchar](100) NOT NULL DEFAULT '',
	[Job_Number] [nvarchar](100) NOT NULL DEFAULT '',
	[Lab_ID] [nvarchar](100) NOT NULL DEFAULT '',
	[Port_of_Loading] [nvarchar](100) NOT NULL DEFAULT '',
	[Port_Destination] [nvarchar](100) NOT NULL DEFAULT '',
	[Service_Trip_No] [nvarchar](100) NOT NULL DEFAULT '',
	[Tug_Boat] [nvarchar](100) NOT NULL DEFAULT '',
	[Quantity_Draft_Survey] [nvarchar](100) NOT NULL DEFAULT '',
	[Loading_Date] [nvarchar](100) NOT NULL DEFAULT '',
	[Report_Date] [nvarchar](100) NOT NULL DEFAULT '',
	[Report_To] [nvarchar](200) NOT NULL DEFAULT '',
	[Total_Moisture] [numeric](10, 2) NOT NULL DEFAULT 0,
	[Moisture] [numeric](10, 2) NOT NULL  DEFAULT 0,
	[Ash] [numeric](10, 2) NOT NULL  DEFAULT 0,
	[Volatile_Matter] [numeric](10, 2) NOT NULL DEFAULT 0,
	[Fixed_Carbon] [numeric](10, 2) NOT NULL DEFAULT 0,
	[Total_Sulfur] [numeric](10, 2) NOT NULL DEFAULT 0,
	[Gross_Caloric_adb] [numeric](10, 2) NOT NULL DEFAULT 0,
	[Gross_Caloric_db] [numeric](10, 2) NOT NULL DEFAULT 0,
	[Gross_Caloric_ar] [numeric](10, 2) NOT NULL DEFAULT 0,
	[Gross_Caloric_daf] [numeric](10, 2) NOT NULL DEFAULT 0,
	[Report_Location_Date] [nvarchar](100) NOT NULL DEFAULT '',
	[Report_Creator] [nvarchar](100) NOT NULL DEFAULT '',
	[Position] [nvarchar](100) NOT NULL DEFAULT '',
	[Company] [nvarchar](32) NOT NULL DEFAULT ''
	, CreatedBy INT NOT NULL DEFAULT 0
    , CreatedOn datetime NOT NULL DEFAULT GetDate()
	, LastModifiedBy INT NOT NULL DEFAULT 0
    , LastModifiedOn datetime NULL
    , CONSTRAINT PK_UPLOAD_BARGE_LOADING_COA PRIMARY KEY (RecordId)
);

create table UPLOAD_Barge_Quality_Plan_Header
(
    RecordId BIGINT NOT NULL IDENTITY(1,1)
    , File_Physical BIGINT NOT NULL
    , [TEMPORARY] BIGINT NOT NULL DEFAULT 0
    , Site INT NOT NULL DEFAULT 0
    , Sheet VARCHAR(255) NOT NULL DEFAULT ''
    , TripNo varchar(255) not null default ''
    , Vessel varchar(255) not null default ''
    , Buyer varchar(255) not null default ''
    , LaycanFrom varchar(255) not null default ''
    , LaycanTo varchar(255) not null default ''
    , CreatedBy INT NOT NULL DEFAULT 0
    , CreatedOn datetime NOT NULL DEFAULT GetDate()
    , CONSTRAINT PK_UPLOAD_Barge_Quality_Plan_Header_RecordId PRIMARY KEY (RecordId)
);

create table UPLOAD_Barge_Quality_Plan
(
    RecordId BIGINT NOT NULL IDENTITY(1,1)
    , [TEMPORARY] BIGINT NOT NULL DEFAULT 0
    , Site INT NOT NULL DEFAULT 0
    , Sheet VARCHAR(255) NOT NULL DEFAULT ''
    , Header BIGINT NOT NULL DEFAULT 0
    , Barge_Ton_plan NUMERIC(10,2) NOT NULL DEFAULT 0
    , Barge_Ton_actual NUMERIC(10,2) NOT NULL DEFAULT 0
    , TM_plan NUMERIC(10,2) NOT NULL DEFAULT 0
    , TM_actual NUMERIC(10,2) NOT NULL DEFAULT 0
    , M_plan NUMERIC(10,2) NOT NULL DEFAULT 0
    , M_actual NUMERIC(10,2) NOT NULL DEFAULT 0
    , ASH_plan NUMERIC(10,2) NOT NULL DEFAULT 0
    , ASH_actual NUMERIC(10,2) NOT NULL DEFAULT 0
    , TS_plan NUMERIC(10,2) NOT NULL DEFAULT 0
    , TS_actual NUMERIC(10,2) NOT NULL DEFAULT 0
    , CV_ADB_plan NUMERIC(10,2) NOT NULL DEFAULT 0
    , CV_ADB_actual NUMERIC(10,2) NOT NULL DEFAULT 0
    , CV_AR_plan NUMERIC(10,2) NOT NULL DEFAULT 0
    , CV_AR_actual NUMERIC(10,2) NOT NULL DEFAULT 0
    , Product varchar(255) not null default ''
    , TugName varchar(255) not null default ''
    , BargeName varchar(255) not null default ''
    
    , CreatedBy INT NOT NULL DEFAULT 0
    , CreatedOn datetime NOT NULL DEFAULT GetDate()
    , LastModifiedBy INT NOT NULL DEFAULT 0
    , LastModifiedOn datetime NULL
    , CONSTRAINT PK_UPLOAD_Barge_Quality_Plan_RecordId PRIMARY KEY (RecordId)
    , CONSTRAINT CO_TEMPORARY_Barge_Quality_Plan_UNIQUE UNIQUE(TugName, BargeName)
);

create table TEMPORARY_Barge_Line_Up
(
    RecordId BIGINT NOT NULL IDENTITY(1,1)
    , File_Physical BIGINT NOT NULL
    , Site INT NOT NULL DEFAULT 0
    , Sheet VARCHAR(255) NOT NULL DEFAULT ''
    , Status VARCHAR(255) NOT NULL DEFAULT 'New'
    , EstimateStartLoading varchar(255) not null default ''
    , Port_of_Loading varchar(255) not null default ''
    , VesselName varchar(255) not null default ''
    , TripID varchar(255) not null default ''
    , TugBoat varchar(255) not null default ''
    , Barge varchar(255) not null default ''
    , Product varchar(255) not null default ''
    , Capacity varchar(255) not null default ''
    , FinalDestinantion varchar(255) not null default ''
    , Remark varchar(255) not null default ''
    
    , EstimateStartLoading_isvalid bit NOT NULL DEFAULT 0
    , Port_of_Loading_isvalid bit NOT NULL DEFAULT 0
    , VesselName_isvalid bit NOT NULL DEFAULT 0
    , TripID_isvalid bit NOT NULL DEFAULT 0
    , TugBoat_isvalid bit NOT NULL DEFAULT 0
    , Barge_isvalid bit NOT NULL DEFAULT 0
    , Product_isvalid bit NOT NULL DEFAULT 0
    , Capacity_isvalid bit NOT NULL DEFAULT 0
    , FinalDestinantion_isvalid bit NOT NULL DEFAULT 0
    , Remark_isvalid bit NOT NULL DEFAULT 0
    
    , CreatedBy INT NOT NULL DEFAULT 0
    , CreatedOn datetime NOT NULL DEFAULT GetDate()
    , CONSTRAINT PK_TEMPORARY_Barge_Line_Up_RecordId PRIMARY KEY (RecordId)
);

create table UPLOAD_Barge_Line_Up
(
    RecordId BIGINT NOT NULL IDENTITY(1,1)
    , [TEMPORARY] BIGINT NOT NULL DEFAULT 0
    , Site INT NOT NULL DEFAULT 0
    , Sheet VARCHAR(255) NOT NULL DEFAULT ''
    , EstimateStartLoading varchar(255) not null default ''
    , Port_of_Loading varchar(255) not null default ''
    , VesselName varchar(255) not null default ''
    , TripID varchar(255) not null UNIQUE
    , TugBoat varchar(255) not null default ''
    , Barge varchar(255) not null default ''
    , Product varchar(255) not null default ''
    , Capacity int not null default 0
    , FinalDestinantion varchar(255) not null default ''
    , Remark varchar(255) not null default ''
    
    , CreatedBy INT NOT NULL DEFAULT 0
    , CreatedOn datetime NOT NULL DEFAULT GetDate()
    , LastModifiedBy INT NOT NULL DEFAULT 0
    , LastModifiedOn datetime NULL
    , CONSTRAINT PK_UPLOAD_Barge_Line_Up_RecordId PRIMARY KEY (RecordId)
);

ALTER TABLE Product ADD UNIQUE (ProductName);
ALTER TABLE ROM ADD UNIQUE (ROMName);
ALTER TABLE DirectShipment ADD UNIQUE (BuyerName);

alter table TEMPORARY_Sampling_ROM add Date_Request_isvalid bit NOT NULL DEFAULT 0;
alter table TEMPORARY_Sampling_ROM add Date_Sampling_isvalid bit NOT NULL DEFAULT 0;
alter table TEMPORARY_Sampling_ROM add Day_work_isvalid bit NOT NULL DEFAULT 0;
alter table TEMPORARY_Sampling_ROM add LOT_isvalid bit NOT NULL DEFAULT 0;
alter table TEMPORARY_Sampling_ROM add Lab_ID_isvalid bit NOT NULL DEFAULT 0;
alter table TEMPORARY_Sampling_ROM add TM_isvalid bit NOT NULL DEFAULT 0;
alter table TEMPORARY_Sampling_ROM add M_isvalid bit NOT NULL DEFAULT 0;
alter table TEMPORARY_Sampling_ROM add ASH_isvalid bit NOT NULL DEFAULT 0;
alter table TEMPORARY_Sampling_ROM add TS_isvalid bit NOT NULL DEFAULT 0;
alter table TEMPORARY_Sampling_ROM add CV_isvalid bit NOT NULL DEFAULT 0;
alter table TEMPORARY_Sampling_ROM add Remark_isvalid bit NOT NULL DEFAULT 0;
alter table TEMPORARY_Sampling_ROM add Seam_isvalid bit NOT NULL DEFAULT 0;

alter table TEMPORARY_Geology_Pit_Monitoring add Sample_ID_isvalid bit NOT NULL DEFAULT 0;
alter table TEMPORARY_Geology_Pit_Monitoring add SampleType_isvalid bit NOT NULL DEFAULT 0;
alter table TEMPORARY_Geology_Pit_Monitoring add Lab_ID_isvalid bit NOT NULL DEFAULT 0;
alter table TEMPORARY_Geology_Pit_Monitoring add Mass_Spl_isvalid bit NOT NULL DEFAULT 0;
alter table TEMPORARY_Geology_Pit_Monitoring add TM_isvalid bit NOT NULL DEFAULT 0;
alter table TEMPORARY_Geology_Pit_Monitoring add M_isvalid bit NOT NULL DEFAULT 0;
alter table TEMPORARY_Geology_Pit_Monitoring add VM_isvalid bit NOT NULL DEFAULT 0;
alter table TEMPORARY_Geology_Pit_Monitoring add Ash_isvalid bit NOT NULL DEFAULT 0;
alter table TEMPORARY_Geology_Pit_Monitoring add FC_isvalid bit NOT NULL DEFAULT 0;
alter table TEMPORARY_Geology_Pit_Monitoring add TS_isvalid bit NOT NULL DEFAULT 0;
alter table TEMPORARY_Geology_Pit_Monitoring add Cal_ad_isvalid bit NOT NULL DEFAULT 0;
alter table TEMPORARY_Geology_Pit_Monitoring add Cal_db_isvalid bit NOT NULL DEFAULT 0;
alter table TEMPORARY_Geology_Pit_Monitoring add Cal_ar_isvalid bit NOT NULL DEFAULT 0;
alter table TEMPORARY_Geology_Pit_Monitoring add Cal_daf_isvalid bit NOT NULL DEFAULT 0;
alter table TEMPORARY_Geology_Pit_Monitoring add RD_isvalid bit NOT NULL DEFAULT 0;

alter table TEMPORARY_Geology_Explorasi add Sample_ID_isvalid bit NOT NULL DEFAULT 0;
alter table TEMPORARY_Geology_Explorasi add SampleType_isvalid bit NOT NULL DEFAULT 0;
alter table TEMPORARY_Geology_Explorasi add Lab_ID_isvalid bit NOT NULL DEFAULT 0;
alter table TEMPORARY_Geology_Explorasi add Mass_Spl_isvalid bit NOT NULL DEFAULT 0;
alter table TEMPORARY_Geology_Explorasi add TM_isvalid bit NOT NULL DEFAULT 0;
alter table TEMPORARY_Geology_Explorasi add M_isvalid bit NOT NULL DEFAULT 0;
alter table TEMPORARY_Geology_Explorasi add VM_isvalid bit NOT NULL DEFAULT 0;
alter table TEMPORARY_Geology_Explorasi add Ash_isvalid bit NOT NULL DEFAULT 0;
alter table TEMPORARY_Geology_Explorasi add FC_isvalid bit NOT NULL DEFAULT 0;
alter table TEMPORARY_Geology_Explorasi add TS_isvalid bit NOT NULL DEFAULT 0;
alter table TEMPORARY_Geology_Explorasi add Cal_ad_isvalid bit NOT NULL DEFAULT 0;
alter table TEMPORARY_Geology_Explorasi add Cal_db_isvalid bit NOT NULL DEFAULT 0;
alter table TEMPORARY_Geology_Explorasi add Cal_ar_isvalid bit NOT NULL DEFAULT 0;
alter table TEMPORARY_Geology_Explorasi add Cal_daf_isvalid bit NOT NULL DEFAULT 0;
alter table TEMPORARY_Geology_Explorasi add RD_isvalid bit NOT NULL DEFAULT 0;

alter table TEMPORARY_BARGE_LOADING add JOB_Number_isvalid bit NOT NULL DEFAULT 0;
alter table TEMPORARY_BARGE_LOADING add ID_Number_isvalid bit NOT NULL DEFAULT 0;
alter table TEMPORARY_BARGE_LOADING add Service_Trip_Number_isvalid bit NOT NULL DEFAULT 0;
alter table TEMPORARY_BARGE_LOADING add Date_Sampling_isvalid bit NOT NULL DEFAULT 0;
alter table TEMPORARY_BARGE_LOADING add Date_Report_isvalid bit NOT NULL DEFAULT 0;
alter table TEMPORARY_BARGE_LOADING add Tonnage_isvalid bit NOT NULL DEFAULT 0;
alter table TEMPORARY_BARGE_LOADING add Name_isvalid bit NOT NULL DEFAULT 0;
alter table TEMPORARY_BARGE_LOADING add Destination_isvalid bit NOT NULL DEFAULT 0;
alter table TEMPORARY_BARGE_LOADING add Temperature_isvalid bit NOT NULL DEFAULT 0;
alter table TEMPORARY_BARGE_LOADING add TM_isvalid bit NOT NULL DEFAULT 0;
alter table TEMPORARY_BARGE_LOADING add M_isvalid bit NOT NULL DEFAULT 0;
alter table TEMPORARY_BARGE_LOADING add ASH_adb_isvalid bit NOT NULL DEFAULT 0;
alter table TEMPORARY_BARGE_LOADING add ASH_arb_isvalid bit NOT NULL DEFAULT 0;
alter table TEMPORARY_BARGE_LOADING add VM_adb_isvalid bit NOT NULL DEFAULT 0;
alter table TEMPORARY_BARGE_LOADING add VM_arb_isvalid bit NOT NULL DEFAULT 0;
alter table TEMPORARY_BARGE_LOADING add FC_adb_isvalid bit NOT NULL DEFAULT 0;
alter table TEMPORARY_BARGE_LOADING add FC_arb_isvalid bit NOT NULL DEFAULT 0;
alter table TEMPORARY_BARGE_LOADING add TS_adb_isvalid bit NOT NULL DEFAULT 0;
alter table TEMPORARY_BARGE_LOADING add TS_arb_isvalid bit NOT NULL DEFAULT 0;
alter table TEMPORARY_BARGE_LOADING add CV_adb_isvalid bit NOT NULL DEFAULT 0;
alter table TEMPORARY_BARGE_LOADING add CV_db_isvalid bit NOT NULL DEFAULT 0;
alter table TEMPORARY_BARGE_LOADING add CV_arb_isvalid bit NOT NULL DEFAULT 0;
alter table TEMPORARY_BARGE_LOADING add CV_daf_isvalid bit NOT NULL DEFAULT 0;
alter table TEMPORARY_BARGE_LOADING add CV_ad_15_isvalid bit NOT NULL DEFAULT 0;
alter table TEMPORARY_BARGE_LOADING add CV_ad_16_isvalid bit NOT NULL DEFAULT 0;
alter table TEMPORARY_BARGE_LOADING add CV_ad_17_isvalid bit NOT NULL DEFAULT 0;
alter table TEMPORARY_BARGE_LOADING add Remark_isvalid bit NOT NULL DEFAULT 0;
alter table TEMPORARY_BARGE_LOADING add CV_Plan_isvalid bit NOT NULL DEFAULT 0;
alter table TEMPORARY_BARGE_LOADING add TM_Plan_isvalid bit NOT NULL DEFAULT 0;
alter table TEMPORARY_BARGE_LOADING add ASH_Plan_isvalid bit NOT NULL DEFAULT 0;
alter table TEMPORARY_BARGE_LOADING add TS_Plan_isvalid bit NOT NULL DEFAULT 0;

alter table TEMPORARY_CRUSHING_PLANT add Date_Production_isvalid bit NOT NULL DEFAULT 0;
alter table TEMPORARY_CRUSHING_PLANT add Shift_Work_isvalid bit NOT NULL DEFAULT 0;
alter table TEMPORARY_CRUSHING_PLANT add Tonnage_isvalid bit NOT NULL DEFAULT 0;
alter table TEMPORARY_CRUSHING_PLANT add Sample_ID_isvalid bit NOT NULL DEFAULT 0;
alter table TEMPORARY_CRUSHING_PLANT add TM_isvalid bit NOT NULL DEFAULT 0;
alter table TEMPORARY_CRUSHING_PLANT add M_isvalid bit NOT NULL DEFAULT 0;
alter table TEMPORARY_CRUSHING_PLANT add ASH_adb_isvalid bit NOT NULL DEFAULT 0;
alter table TEMPORARY_CRUSHING_PLANT add ASH_arb_isvalid bit NOT NULL DEFAULT 0;
alter table TEMPORARY_CRUSHING_PLANT add VM_adb_isvalid bit NOT NULL DEFAULT 0;
alter table TEMPORARY_CRUSHING_PLANT add VM_arb_isvalid bit NOT NULL DEFAULT 0;
alter table TEMPORARY_CRUSHING_PLANT add FC_adb_isvalid bit NOT NULL DEFAULT 0;
alter table TEMPORARY_CRUSHING_PLANT add FC_arb_isvalid bit NOT NULL DEFAULT 0;
alter table TEMPORARY_CRUSHING_PLANT add TS_adb_isvalid bit NOT NULL DEFAULT 0;
alter table TEMPORARY_CRUSHING_PLANT add TS_arb_isvalid bit NOT NULL DEFAULT 0;
alter table TEMPORARY_CRUSHING_PLANT add CV_adb_isvalid bit NOT NULL DEFAULT 0;
alter table TEMPORARY_CRUSHING_PLANT add CV_db_isvalid bit NOT NULL DEFAULT 0;
alter table TEMPORARY_CRUSHING_PLANT add CV_arb_isvalid bit NOT NULL DEFAULT 0;
alter table TEMPORARY_CRUSHING_PLANT add CV_daf_isvalid bit NOT NULL DEFAULT 0;
alter table TEMPORARY_CRUSHING_PLANT add CV_ad_15_isvalid bit NOT NULL DEFAULT 0;
alter table TEMPORARY_CRUSHING_PLANT add CV_ad_16_isvalid bit NOT NULL DEFAULT 0;
alter table TEMPORARY_CRUSHING_PLANT add CV_ad_17_isvalid bit NOT NULL DEFAULT 0;
alter table TEMPORARY_CRUSHING_PLANT add Remark_isvalid bit NOT NULL DEFAULT 0;
alter table TEMPORARY_CRUSHING_PLANT add CV_Plan_isvalid bit NOT NULL DEFAULT 0;
alter table TEMPORARY_CRUSHING_PLANT add TM_Plan_isvalid bit NOT NULL DEFAULT 0;
alter table TEMPORARY_CRUSHING_PLANT add ASH_Plan_isvalid bit NOT NULL DEFAULT 0;
alter table TEMPORARY_CRUSHING_PLANT add TS_Plan_isvalid bit NOT NULL DEFAULT 0;

CREATE TABLE [TEMPORARY_BARGE_LOADING_COA](  
	[RecordId] [bigint] IDENTITY(1,1) NOT NULL
    , File_Physical BIGINT NOT NULL
    , Status VARCHAR(255) NOT NULL DEFAULT 'New'
	,[Sheet] [nvarchar](100) NOT NULL DEFAULT '',
	[Job_Number] [nvarchar](100) NOT NULL DEFAULT '',
	[Lab_ID] [nvarchar](100) NOT NULL DEFAULT '',
	[Port_of_Loading] [nvarchar](100) NOT NULL DEFAULT '',
	[Port_Destination] [nvarchar](100) NOT NULL DEFAULT '',
	[Service_Trip_No] [nvarchar](100) NOT NULL DEFAULT '',
	[Tug_Boat] [nvarchar](100) NOT NULL DEFAULT '',
	[Quantity_Draft_Survey] [nvarchar](100) NOT NULL DEFAULT '',
	[Loading_Date] [nvarchar](100) NOT NULL DEFAULT '',
	[Report_Date] [nvarchar](100) NOT NULL DEFAULT '',
	[Report_To] [nvarchar](200) NOT NULL DEFAULT '',
	[Total_Moisture] [nvarchar](100) NOT NULL DEFAULT '',
	[Moisture] [nvarchar](100) NOT NULL DEFAULT '',
	[Ash] [nvarchar](100) NOT NULL DEFAULT '',
	[Volatile_Matter] [nvarchar](100) NOT NULL DEFAULT '',
	[Fixed_Carbon] [nvarchar](100) NOT NULL DEFAULT '',
	[Total_Sulfur] [nvarchar](100) NOT NULL DEFAULT '',
	[Gross_Caloric_adb] [nvarchar](100) NOT NULL DEFAULT '',
	[Gross_Caloric_db] [nvarchar](100) NOT NULL DEFAULT '',
	[Gross_Caloric_ar] [nvarchar](100) NOT NULL DEFAULT '',
	[Gross_Caloric_daf] [nvarchar](100) NOT NULL DEFAULT '',
	[Report_Location_Date] [nvarchar](100) NOT NULL DEFAULT '',
	[Report_Creator] [nvarchar](100) NOT NULL DEFAULT '',
	[Position] [nvarchar](100) NOT NULL DEFAULT '',
	[Company] [nvarchar](32) NOT NULL DEFAULT ''
	, CreatedBy INT NOT NULL DEFAULT 0
    , CreatedOn datetime NOT NULL DEFAULT GetDate()
	, LastModifiedBy INT NOT NULL DEFAULT 0
    , LastModifiedOn datetime NULL
    
    , Total_Moisture_isvalid bit NOT NULL DEFAULT 0
    , Moisture_isvalid bit NOT NULL DEFAULT 0
    , Ash_isvalid bit NOT NULL DEFAULT 0
    , Volatile_Matter_isvalid bit NOT NULL DEFAULT 0
    , Fixed_Carbon_isvalid bit NOT NULL DEFAULT 0
    , Total_Sulfur_isvalid bit NOT NULL DEFAULT 0
    , Gross_Caloric_adb_isvalid bit NOT NULL DEFAULT 0
    , Gross_Caloric_db_isvalid bit NOT NULL DEFAULT 0
    , Gross_Caloric_ar_isvalid bit NOT NULL DEFAULT 0
    , Gross_Caloric_daf_isvalid bit NOT NULL DEFAULT 0
    
    , CONSTRAINT PK_TEMPORARY_BARGE_LOADING_COA PRIMARY KEY (RecordId)
);

CREATE TABLE [TEMPORARY_SampleDetail](  
	[RecordId] [bigint] IDENTITY(1,1) NOT NULL
    , File_Physical BIGINT NOT NULL
    , [Status] VARCHAR(255) NOT NULL DEFAULT 'New'
	,[Sheet] [nvarchar](100) NOT NULL DEFAULT ''
    , SAMPLE VARCHAR(255) NOT NULL DEFAULT ''
    , GEOLOGIST VARCHAR(255) NOT NULL DEFAULT ''
    , SEAM VARCHAR(255) NOT NULL DEFAULT ''
    , SAMPLE_TYPE VARCHAR(255) NOT NULL DEFAULT ''
    , DEPTH_FROM VARCHAR(255) NOT NULL DEFAULT ''
    , DEPTH_TO VARCHAR(255) NOT NULL DEFAULT ''
    , Total_Moisture VARCHAR(255) NOT NULL DEFAULT ''
    , Proximate_analysis VARCHAR(255) NOT NULL DEFAULT ''
    , Sulfur_content VARCHAR(255) NOT NULL DEFAULT ''
    , Calorific_value VARCHAR(255) NOT NULL DEFAULT ''
    , Relative_density VARCHAR(255) NOT NULL DEFAULT ''
    , CSN VARCHAR(255) NOT NULL DEFAULT ''
    , Ash_analysis VARCHAR(255) NOT NULL DEFAULT ''
    , HGI VARCHAR(255) NOT NULL DEFAULT ''
    , Ultimate_analysis VARCHAR(255) NOT NULL DEFAULT ''
    , Chlorine VARCHAR(255) NOT NULL DEFAULT ''
    , Phosphorous VARCHAR(255) NOT NULL DEFAULT ''
    , Fluorine VARCHAR(255) NOT NULL DEFAULT ''
    , Lead_ VARCHAR(255) NOT NULL DEFAULT ''
    , Zinc VARCHAR(255) NOT NULL DEFAULT ''
    , Form_of_Sulphur VARCHAR(255) NOT NULL DEFAULT ''
    , Ash_fusions_temperature VARCHAR(255) NOT NULL DEFAULT ''
    , TraceElement VARCHAR(255) NOT NULL DEFAULT ''
    , CreatedBy INT NOT NULL DEFAULT 0
    , CreatedOn datetime NOT NULL DEFAULT GetDate()
    
    , SAMPLE_isvalid bit NOT NULL DEFAULT 0
    
    , CONSTRAINT PK_TEMPORARY_SampleDetail_RecordId PRIMARY KEY (RecordId)
);

alter table AnalysisRequest_Detail add Verification bit NOT NULL DEFAULT 0;
go
update AnalysisRequest_Detail set Verification = 1;
go

alter table AnalysisRequest_Detail add Verification_Comment VARCHAR(255) NOT NULL DEFAULT '';
go
update AnalysisRequest_Detail set Verification_Comment = '';
go

alter table AnalysisRequest_Detail add ReTest bit NOT NULL DEFAULT 0;
go
update AnalysisRequest_Detail set ReTest = 0;
go

alter table TEMPORARY_Sampling_ROM_Header add Sheet varchar(32) not null default '';
go

update TEMPORARY_Sampling_ROM_Header set Sheet = '';
go

alter table UPLOAD_Sampling_ROM add Sheet varchar(32) not null default '';
go

update UPLOAD_Sampling_ROM set Sheet = '';
go

alter table TEMPORARY_Geology_Pit_Monitoring_Header add Sheet varchar(32) not null default '';
go

update TEMPORARY_Geology_Pit_Monitoring_Header set Sheet = '';
go

alter table UPLOAD_Geology_Pit_Monitoring add Sheet varchar(32) not null default '';
go

update UPLOAD_Geology_Pit_Monitoring set Sheet = '';
go

alter table TEMPORARY_Geology_Explorasi_Header add Sheet varchar(32) not null default '';
go

update TEMPORARY_Geology_Explorasi_Header set Sheet = '';
go

alter table UPLOAD_Geology_Explorasi add Sheet varchar(32) not null default '';
go

update UPLOAD_Geology_Explorasi set Sheet = '';
go
