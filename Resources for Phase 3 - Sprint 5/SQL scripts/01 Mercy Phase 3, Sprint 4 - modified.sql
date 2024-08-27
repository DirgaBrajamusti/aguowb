/*
drop table TEMPORARY_Barge_Quality_Plan;
drop table TEMPORARY_Barge_Quality_Plan_Header;
drop table UPLOAD_Barge_Quality_Plan;
drop table UPLOAD_Barge_Quality_Plan_Header;
drop table TEMPORARY_Barge_Line_Up;
drop table UPLOAD_Barge_Line_Up;
go
*/

create table TEMPORARY_Barge_Quality_Plan_Header
(
    RecordId BIGINT NOT NULL IDENTITY(1,1)
    , File_Physical BIGINT NOT NULL
    , Site INT NOT NULL DEFAULT 0
    , Sheet VARCHAR(255) NOT NULL DEFAULT ''
    , TripNo varchar(255) not null default ''
    , Vessel varchar(255) not null default ''
    , Buyer varchar(255) not null default ''
    , Laycan_From varchar(255) not null default ''
    , Laycan_To varchar(255) not null default ''
    , CreatedBy INT NOT NULL DEFAULT 0
    , CreatedOn datetime NOT NULL DEFAULT GetDate()
    , CreatedOn_Date_Only varchar(10) not null default ''
    , CONSTRAINT PK_TEMPORARY_Barge_Quality_Plan_Header_RecordId PRIMARY KEY (RecordId)
    , CONSTRAINT FK_TEMPORARY_Barge_Quality_Plan_Header_File_Physical FOREIGN KEY (File_Physical) REFERENCES TEMPORARY_File (RecordId)
);

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
    , CreatedOn_Date_Only varchar(10) not null default ''
    , CONSTRAINT PK_TEMPORARY_Barge_Quality_Plan_RecordId PRIMARY KEY (RecordId)
    , CONSTRAINT FK_TEMPORARY_Barge_Quality_Plan_Header FOREIGN KEY (Header) REFERENCES TEMPORARY_Barge_Quality_Plan_Header(RecordId)
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
    , Laycan_From datetime NOT NULL
    , Laycan_To datetime NOT NULL
    , CreatedBy INT NOT NULL DEFAULT 0
    , CreatedOn datetime NOT NULL DEFAULT GetDate()
    , CreatedOn_Date_Only varchar(10) not null default ''
    , CONSTRAINT PK_UPLOAD_Barge_Quality_Plan_Header_RecordId PRIMARY KEY (RecordId)
    , CONSTRAINT CO_UPLOAD_Barge_Quality_Plan_Header_UNIQUE UNIQUE(CreatedOn_Date_Only, TripNo)
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
    , CreatedOn_Date_Only varchar(10) not null default ''
    , LastModifiedBy INT NOT NULL DEFAULT 0
    , LastModifiedOn datetime NULL
    , CONSTRAINT PK_UPLOAD_Barge_Quality_Plan_RecordId PRIMARY KEY (RecordId)
    , CONSTRAINT CO_TEMPORARY_Barge_Quality_Plan_UNIQUE UNIQUE(CreatedOn_Date_Only, TugName, BargeName)
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
    , CreatedOn_Date_Only varchar(10) not null default ''
    , CONSTRAINT PK_TEMPORARY_Barge_Line_Up_RecordId PRIMARY KEY (RecordId)
);

create table UPLOAD_Barge_Line_Up
(
    RecordId BIGINT NOT NULL IDENTITY(1,1)
    , [TEMPORARY] BIGINT NOT NULL DEFAULT 0
    , Site INT NOT NULL DEFAULT 0
    , Sheet VARCHAR(255) NOT NULL DEFAULT ''
    , EstimateStartLoading datetime NOT NULL
    , Port_of_Loading varchar(255) not null default ''
    , VesselName varchar(255) not null default ''
    , TripID varchar(255) not null default ''
    , TugBoat varchar(255) not null default ''
    , Barge varchar(255) not null default ''
    , Product varchar(255) not null default ''
    , Capacity int not null default 0
    , FinalDestinantion varchar(255) not null default ''
    , Remark varchar(255) not null default ''
    
    , CreatedBy INT NOT NULL DEFAULT 0
    , CreatedOn datetime NOT NULL DEFAULT GetDate()
    , CreatedOn_Date_Only varchar(10) not null default ''
    , LastModifiedBy INT NOT NULL DEFAULT 0
    , LastModifiedOn datetime NULL
    , CONSTRAINT PK_UPLOAD_Barge_Line_Up_RecordId PRIMARY KEY (RecordId)
    --, CONSTRAINT CO_UPLOAD_Barge_Line_Up_UNIQUE UNIQUE(CreatedOn_Date_Only, TripID)
);
