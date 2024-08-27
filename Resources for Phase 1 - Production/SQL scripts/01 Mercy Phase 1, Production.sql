CREATE TABLE [User]
(
    UserId INT NOT NULL IDENTITY(1,1),
    LoginName VARCHAR(32) NOT NULL unique,
    FullName VARCHAR(255) NOT NULL DEFAULT '',
    Title VARCHAR(255) NOT NULL DEFAULT '',
    Department VARCHAR(255) NOT NULL DEFAULT '',
    Email VARCHAR(255) NOT NULL DEFAULT '',
    IsAdmin bit NOT NULL DEFAULT 0,
    CreatedBy INT NOT NULL DEFAULT 0,
    CreatedOn datetime NOT NULL DEFAULT GetDate(),
    LastModifiedBy INT NOT NULL DEFAULT 0,
    LastModifiedOn datetime NULL,
    DeletedBy INT NOT NULL DEFAULT 0,
    DeletedOn datetime NULL,
    CONSTRAINT PK_User_UserId PRIMARY KEY (UserId)
);

CREATE TABLE User_Activity
(
    RecordId INT NOT NULL IDENTITY(1,1),
    LoginName VARCHAR(32) NOT NULL DEFAULT '',
    Token VARCHAR(255)  NOT NULL unique,
    LastActivity datetime NOT NULL DEFAULT GetDate(),
    UserAgent VARCHAR(255) NOT NULL DEFAULT '',
    IPAddress_of_EndUser VARCHAR(255) NOT NULL DEFAULT '',
    IsActive bit NOT NULL DEFAULT 1,
    CreatedOn datetime NOT NULL DEFAULT GetDate(),
    CONSTRAINT PK_User_Activity_RecordId PRIMARY KEY (RecordId)
);

CREATE TABLE Role
(
    RoleId INT NOT NULL IDENTITY(1,1),
    Name VARCHAR(255) NOT NULL DEFAULT '',
    CreatedBy INT NOT NULL DEFAULT 0,
    CreatedOn datetime NOT NULL DEFAULT GetDate(),
    LastModifiedBy INT NOT NULL DEFAULT 0,
    LastModifiedOn datetime NULL,
    DeletedBy INT NOT NULL DEFAULT 0,
    DeletedOn datetime NULL,
    CONSTRAINT PK_Role_RoleId PRIMARY KEY (RoleId)
);

CREATE TABLE API
(
    ApiId INT NOT NULL IDENTITY(1,1),
    Name VARCHAR(255) NOT NULL unique,
    Url VARCHAR(255) NOT NULL unique,
    CreatedBy INT NOT NULL DEFAULT 0,
    CreatedOn datetime NOT NULL DEFAULT GetDate(),
    LastModifiedBy INT NOT NULL DEFAULT 0,
    LastModifiedOn datetime NULL,
    DeletedBy INT NOT NULL DEFAULT 0,
    DeletedOn datetime NULL,
    CONSTRAINT PK_API_ApiId PRIMARY KEY (ApiId)
);

CREATE TABLE Menu
(
    MenuId INT NOT NULL IDENTITY(1,1),
    Name VARCHAR(32) NOT NULL,
    Link VARCHAR(255) NOT NULL,
    Parent INT NOT NULL DEFAULT 0,
    CreatedBy INT NOT NULL DEFAULT 0,
    CreatedOn datetime NOT NULL DEFAULT GetDate(),
    LastModifiedBy INT NOT NULL DEFAULT 0,
    LastModifiedOn datetime NULL,
    DeletedBy INT NOT NULL DEFAULT 0,
    DeletedOn datetime NULL,
    CONSTRAINT PK_Menu_MenuId PRIMARY KEY (MenuId)
);

CREATE TABLE User_Role
(
    RecordId INT NOT NULL IDENTITY(1,1),
    [User] INT NOT NULL,
    Role INT NOT NULL,
    CONSTRAINT PK_User_Role_RecordId PRIMARY KEY (RecordId),
    CONSTRAINT FK_User_Role_User FOREIGN KEY ([User]) REFERENCES [User] (UserId),
    CONSTRAINT FK_User_Role_Role FOREIGN KEY (Role) REFERENCES Role (RoleId)
);

CREATE TABLE Api_Role
(
    RecordId INT NOT NULL IDENTITY(1,1),
    Api INT NOT NULL,
    Role INT NOT NULL,
    CONSTRAINT PK_Api_Role_RecordId PRIMARY KEY (RecordId),
    CONSTRAINT FK_Api_Role_Api FOREIGN KEY (Api) REFERENCES Api (ApiId),
    CONSTRAINT FK_Api_Role_Role FOREIGN KEY (Role) REFERENCES Role (RoleId)
);

CREATE TABLE Menu_Role
(
    RecordId INT NOT NULL IDENTITY(1,1),
    Menu INT NOT NULL,
    Role INT NOT NULL,
    CONSTRAINT PK_Menu_Role_RecordId PRIMARY KEY (RecordId),
    CONSTRAINT FK_Menu_Role_Menu FOREIGN KEY (Menu) REFERENCES Menu (MenuId),
    CONSTRAINT FK_Menu_Role_Role FOREIGN KEY (Role) REFERENCES Role (RoleId)
);

CREATE TABLE Company
(
    CompanyCode VARCHAR(32) NOT NULL,
    Name VARCHAR(255) NOT NULL,
    CreatedBy INT NOT NULL DEFAULT 0,
    CreatedOn datetime NOT NULL DEFAULT GetDate(),
    LastModifiedBy INT NOT NULL DEFAULT 0,
    LastModifiedOn datetime NULL,
    DeletedBy INT NOT NULL DEFAULT 0,
    DeletedOn datetime NULL,
    CONSTRAINT PK_Company_CompanyCode PRIMARY KEY (CompanyCode)
);

CREATE TABLE SamplingRequest
(
    SamplingRequestId BIGINT NOT NULL IDENTITY(1,1),
    RequestDate date NOT NULL DEFAULT GetDate(),
    Company VARCHAR(32) NOT NULL DEFAULT '',
    SamplingType VARCHAR(32) NOT NULL DEFAULT '',
    HAC_Text VARCHAR(255) NOT NULL DEFAULT '',
    Tonnage varchar(255) not null default '',
    PICArea VARCHAR(255) NOT NULL DEFAULT '',
    Remark VARCHAR(255) NOT NULL DEFAULT '',
    CV bit NOT NULL DEFAULT 0,
    TS bit NOT NULL DEFAULT 0,
    TM bit NOT NULL DEFAULT 0,
    PROX bit NOT NULL DEFAULT 0,
    RD bit NOT NULL DEFAULT 0,
    [SIZE] bit NOT NULL DEFAULT 0,
    FLOW bit NOT NULL DEFAULT 0,
    CreatedBy INT NOT NULL DEFAULT 0,
    CreatedOn datetime NOT NULL DEFAULT GetDate(),
    LastModifiedBy INT NOT NULL DEFAULT 0,
    LastModifiedOn datetime NULL,
    DeletedBy INT NOT NULL DEFAULT 0,
    DeletedOn datetime NULL,
    CONSTRAINT PK_SamplingRequest_SamplingRequestId PRIMARY KEY (SamplingRequestId),
    CONSTRAINT FK_SamplingRequest_Company FOREIGN KEY (Company) REFERENCES Company (CompanyCode)
);

CREATE TABLE SamplingRequest_ROM
(
    RecordId BIGINT NOT NULL IDENTITY(1,1),
    SamplingRequest BIGINT NOT NULL
    , ROM_ID    int
    , Block nvarchar(10)
    , ROM_Name nvarchar(255)
    CONSTRAINT PK_SamplingRequest_ROM_RecordId PRIMARY KEY (RecordId),
    CONSTRAINT FK_SamplingRequest_ROM_SamplingRequest FOREIGN KEY (SamplingRequest) REFERENCES SamplingRequest (SamplingRequestId),
    CONSTRAINT CO_SamplingRequest_ROM_UNIQUE UNIQUE(SamplingRequest, ROM_ID)
);

CREATE TABLE SamplingRequest_SEAM
(
    RecordId BIGINT NOT NULL IDENTITY(1,1),
    SamplingRequest BIGINT NOT NULL
    , SEAM varchar(255)
    , COMPANY_PIT_SEAM varchar(255)
    CONSTRAINT PK_SamplingRequest_SEAM_RecordId PRIMARY KEY (RecordId),
    CONSTRAINT FK_SamplingRequest_SEAM_SamplingRequest FOREIGN KEY (SamplingRequest) REFERENCES SamplingRequest (SamplingRequestId),
    CONSTRAINT CO_SamplingRequest_SEAM_UNIQUE UNIQUE(SamplingRequest, COMPANY_PIT_SEAM)
);

CREATE TABLE SamplingRequest_Lab
(
    RecordId BIGINT NOT NULL IDENTITY(1,1),
    SamplingRequest BIGINT NOT NULL,
    LabId VARCHAR(255) NOT NULL unique,
    CreatedBy INT NOT NULL DEFAULT 0,
    CreatedOn datetime NOT NULL DEFAULT GetDate(),
    LastModifiedBy INT NOT NULL DEFAULT 0,
    LastModifiedOn datetime NULL,
    DeletedBy INT NOT NULL DEFAULT 0,
    DeletedOn datetime NULL,
    CONSTRAINT PK_SamplingRequest_Lab_RecordId PRIMARY KEY (RecordId),
    CONSTRAINT FK_SamplingRequest_Lab_SamplingRequest FOREIGN KEY (SamplingRequest) REFERENCES SamplingRequest (SamplingRequestId)
);

CREATE TABLE AnalysisRequest
(
    AnalysisRequestId BIGINT NOT NULL IDENTITY(1,1),
    Company VARCHAR(32) NOT NULL,
    AnalysisType VARCHAR(32) NOT NULL DEFAULT '',
    DeliveryDate date NOT NULL DEFAULT GetDate(),
    Sender VARCHAR(255) NOT NULL DEFAULT '',
    LetterNo VARCHAR(255) NOT NULL DEFAULT '',
    TM bit NOT NULL DEFAULT 0,
    PROX bit NOT NULL DEFAULT 0,
    TS bit NOT NULL DEFAULT 0,
    CV bit NOT NULL DEFAULT 0,
    RD bit NOT NULL DEFAULT 0,
    CSN bit NOT NULL DEFAULT 0,
    AA bit NOT NULL DEFAULT 0,
    HGI bit NOT NULL DEFAULT 0,
    Ultimate bit NOT NULL DEFAULT 0,
    Chlorine bit NOT NULL DEFAULT 0,
    Phosporus bit NOT NULL DEFAULT 0,
    Fluorine bit NOT NULL DEFAULT 0,
    Lead bit NOT NULL DEFAULT 0,
    Zink bit NOT NULL DEFAULT 0,
    AFT bit NOT NULL DEFAULT 0,
    TraceElement bit NOT NULL DEFAULT 0,
    Others bit NOT NULL DEFAULT 0,
    Others_Text VARCHAR(255) NOT NULL DEFAULT '',
    CreatedBy INT NOT NULL DEFAULT 0,
    CreatedOn datetime NOT NULL DEFAULT GetDate(),
    LastModifiedBy INT NOT NULL DEFAULT 0,
    LastModifiedOn datetime NULL,
    DeletedBy INT NOT NULL DEFAULT 0,
    DeletedOn datetime NULL,
    CONSTRAINT PK_AnalysisRequest_AnalysisRequestId PRIMARY KEY (AnalysisRequestId),
    CONSTRAINT FK_AnalysisRequest_Company FOREIGN KEY (Company) REFERENCES Company (CompanyCode)
);

CREATE TABLE AnalysisRequest_Detail
(
    RecordId BIGINT NOT NULL IDENTITY(1,1),
    AnalysisRequest BIGINT NOT NULL,
    SampleID VARCHAR(255) NOT NULL DEFAULT '',
    [From] INT NOT NULL DEFAULT 0,
    [To] INT NOT NULL DEFAULT 0,
    Thick VARCHAR(50) NOT NULL DEFAULT '',
    SampleType VARCHAR(32) NOT NULL DEFAULT '',
    SEAM VARCHAR(32) NOT NULL DEFAULT '',
    LabId VARCHAR(32) NOT NULL DEFAULT '',
    CreatedBy INT NOT NULL DEFAULT 0,
    CreatedOn datetime NOT NULL DEFAULT GetDate(),
    LastModifiedBy INT NOT NULL DEFAULT 0,
    LastModifiedOn datetime NULL,
    DeletedBy INT NOT NULL DEFAULT 0,
    DeletedOn datetime NULL,
    CONSTRAINT PK_AnalysisRequest_Detail_RecordId PRIMARY KEY (RecordId),
    CONSTRAINT FK_AnalysisRequest_Detail_AnalysisRequest FOREIGN KEY (AnalysisRequest) REFERENCES AnalysisRequest (AnalysisRequestId)
);

CREATE TABLE AnalysisRequest_Result
(
    RecordId BIGINT NOT NULL IDENTITY(1,1),
    AnalysisRequest BIGINT NOT NULL,
    CreatedBy INT NOT NULL DEFAULT 0,
    CreatedOn datetime NOT NULL DEFAULT GetDate(),
    LastModifiedBy INT NOT NULL DEFAULT 0,
    LastModifiedOn datetime NULL,
    DeletedBy INT NOT NULL DEFAULT 0,
    DeletedOn datetime NULL,
    CONSTRAINT PK_AnalysisRequest_Result_RecordId PRIMARY KEY (RecordId),
    CONSTRAINT FK_AnalysisRequest_Result_AnalysisRequest FOREIGN KEY (AnalysisRequest) REFERENCES AnalysisRequest (AnalysisRequestId)
);

create table TEMPORARY_File
(
    RecordId BIGINT NOT NULL IDENTITY(1,1)
    , CreatedBy INT NOT NULL DEFAULT 0
    , CreatedOn datetime NOT NULL DEFAULT GetDate()
    , FileName VARCHAR(255) NOT NULL DEFAULT ''
    , Link VARCHAR(255) NOT NULL DEFAULT ''
    , FileType VARCHAR(255) NOT NULL DEFAULT ''
    , CONSTRAINT PK_TEMPORARY_File_RecordId PRIMARY KEY (RecordId),
);

create table TEMPORARY_Geology_Pit_Monitoring_Header
(
    RecordId BIGINT NOT NULL IDENTITY(1,1)
    , File_Physical BIGINT NOT NULL UNIQUE
    , Company varchar(32) not null default ''
    , CreatedBy INT NOT NULL DEFAULT 0
    , CreatedOn datetime NOT NULL DEFAULT GetDate()
    , Date_Detail VARCHAR(255) NOT NULL DEFAULT ''
    , Job_No VARCHAR(255) NOT NULL DEFAULT ''
    , Report_To VARCHAR(255) NOT NULL DEFAULT ''
    , Date_Received VARCHAR(255) NOT NULL DEFAULT ''
    , Nomor VARCHAR(255) NOT NULL DEFAULT ''
    , CONSTRAINT PK_TEMPORARY_Geology_Pit_Monitoring_Header_RecordId PRIMARY KEY (RecordId)
    , CONSTRAINT FK_TEMPORARY_Geology_Pit_Monitoring_Header_File_Physical FOREIGN KEY (File_Physical) REFERENCES TEMPORARY_File (RecordId)
);

create table TEMPORARY_Geology_Pit_Monitoring
(
    RecordId BIGINT NOT NULL IDENTITY(1,1)
    , Header BIGINT NOT NULL DEFAULT 0
    , Status VARCHAR(255) NOT NULL DEFAULT 'New'
    , CreatedBy INT NOT NULL DEFAULT 0
    , CreatedOn datetime NOT NULL DEFAULT GetDate()
    , Sample_ID VARCHAR(255) NOT NULL DEFAULT ''
    , SampleType VARCHAR(255) NOT NULL DEFAULT ''
    , Lab_ID  VARCHAR(255) NOT NULL DEFAULT ''
    , Mass_Spl VARCHAR(255) NOT NULL DEFAULT ''
    , TM VARCHAR(255) NOT NULL DEFAULT ''
    , M VARCHAR(255) NOT NULL DEFAULT ''
    , VM VARCHAR(255) NOT NULL DEFAULT ''
    , Ash VARCHAR(255) NOT NULL DEFAULT ''
    , FC NUMERIC(10,2) NOT NULL DEFAULT 0
    , TS VARCHAR(255) NOT NULL DEFAULT ''
    , Cal_ad VARCHAR(255) NOT NULL DEFAULT ''
    , Cal_db NUMERIC(10,2) NOT NULL DEFAULT 0
    , Cal_ar NUMERIC(10,2) NOT NULL DEFAULT 0
    , Cal_daf NUMERIC(10,2) NOT NULL DEFAULT 0
    , RD VARCHAR(255) NOT NULL DEFAULT ''
   , CONSTRAINT PK_TEMPORARY_Geology_Pit_Monitoring_RecordId PRIMARY KEY (RecordId)
   , CONSTRAINT FK_TEMPORARY_Geology_Pit_Monitoring_Header FOREIGN KEY (Header) REFERENCES TEMPORARY_Geology_Pit_Monitoring_Header (RecordId)
);

create table TEMPORARY_Geology_Explorasi_Header
(
    RecordId BIGINT NOT NULL IDENTITY(1,1)
    , File_Physical BIGINT NOT NULL UNIQUE
    , Company varchar(32) not null default ''
    , CreatedBy INT NOT NULL DEFAULT 0
    , CreatedOn datetime NOT NULL DEFAULT GetDate()
    , Date_Detail VARCHAR(255) NOT NULL DEFAULT ''
    , Job_No VARCHAR(255) NOT NULL DEFAULT ''
    , Report_To VARCHAR(255) NOT NULL DEFAULT ''
    , Date_Received VARCHAR(255) NOT NULL DEFAULT ''
    , Nomor VARCHAR(255) NOT NULL DEFAULT ''
    , CONSTRAINT PK_TEMPORARY_Geology_Explorasi_Header_RecordId PRIMARY KEY (RecordId)
    , CONSTRAINT FK_TEMPORARY_Geology_Explorasi_Header_File_Physical FOREIGN KEY (File_Physical) REFERENCES TEMPORARY_File (RecordId)
);

create table TEMPORARY_Geology_Explorasi
(
    RecordId BIGINT NOT NULL IDENTITY(1,1)
    , Header BIGINT NOT NULL DEFAULT 0
    , Status VARCHAR(255) NOT NULL DEFAULT 'New'
    , CreatedBy INT NOT NULL DEFAULT 0
    , CreatedOn datetime NOT NULL DEFAULT GetDate()
    , Sample_ID VARCHAR(255) NOT NULL DEFAULT ''
    , SampleType VARCHAR(255) NOT NULL DEFAULT ''
    , Lab_ID  VARCHAR(255) NOT NULL DEFAULT ''
    , Mass_Spl VARCHAR(255) NOT NULL DEFAULT ''
    , TM VARCHAR(255) NOT NULL DEFAULT ''
    , M VARCHAR(255) NOT NULL DEFAULT ''
    , VM VARCHAR(255) NOT NULL DEFAULT ''
    , Ash VARCHAR(255) NOT NULL DEFAULT ''
    , FC NUMERIC(10,2) NOT NULL DEFAULT 0
    , TS VARCHAR(255) NOT NULL DEFAULT ''
    , Cal_ad VARCHAR(255) NOT NULL DEFAULT ''
    , Cal_db NUMERIC(10,2) NOT NULL DEFAULT 0
    , Cal_ar NUMERIC(10,2) NOT NULL DEFAULT 0
    , Cal_daf NUMERIC(10,2) NOT NULL DEFAULT 0
    , RD VARCHAR(255) NOT NULL DEFAULT ''
   , CONSTRAINT PK_TEMPORARY_Geology_Explorasi_RecordId PRIMARY KEY (RecordId)
   , CONSTRAINT FK_TEMPORARY_Geology_Explorasi_Header FOREIGN KEY (Header) REFERENCES TEMPORARY_Geology_Explorasi_Header (RecordId)
);

create table TEMPORARY_Sampling_ROM_Header
(
    RecordId BIGINT NOT NULL IDENTITY(1,1)
    , File_Physical BIGINT NOT NULL UNIQUE
    , Company varchar(32) not null default ''
    , CreatedBy INT NOT NULL DEFAULT 0
    , CreatedOn datetime NOT NULL DEFAULT GetDate()
    , Date_Detail VARCHAR(255) NOT NULL DEFAULT ''
    , Job_No VARCHAR(255) NOT NULL DEFAULT ''
    , Report_To VARCHAR(255) NOT NULL DEFAULT ''
    , Method1 VARCHAR(255) NOT NULL DEFAULT ''
    , Method2 VARCHAR(255) NOT NULL DEFAULT ''
    , Method3 VARCHAR(255) NOT NULL DEFAULT ''
    , Method4 VARCHAR(255) NOT NULL DEFAULT ''
    , CONSTRAINT PK_TEMPORARY_Sampling_ROM_Header_RecordId PRIMARY KEY (RecordId)
    , CONSTRAINT FK_TEMPORARY_Sampling_ROM_Header_File_Physical FOREIGN KEY (File_Physical) REFERENCES TEMPORARY_File (RecordId)
);

create table TEMPORARY_Sampling_ROM
(
    RecordId BIGINT NOT NULL IDENTITY(1,1)
    , Header BIGINT NOT NULL DEFAULT 0
    , Status VARCHAR(255) NOT NULL DEFAULT 'New'
    , CreatedBy INT NOT NULL DEFAULT 0
    , CreatedOn datetime NOT NULL DEFAULT GetDate()
    , Date_Request VARCHAR(255) NOT NULL DEFAULT ''
    , Date_Sampling VARCHAR(255) NOT NULL DEFAULT ''
    , Day_work VARCHAR(255) NOT NULL DEFAULT ''
    , LOT  VARCHAR(255) NOT NULL DEFAULT ''
    , Lab_ID  VARCHAR(255) NOT NULL DEFAULT ''
    , TM varchar(255) not null default ''
    , M varchar(255) not null default ''
    , ASH varchar(255) not null default ''
    , TS varchar(255) not null default ''
    , CV varchar(255) not null default ''
    , Remark VARCHAR(255) NOT NULL DEFAULT ''
    , Seam VARCHAR(255) NOT NULL DEFAULT ''
   , CONSTRAINT PK_TEMPORARY_Sampling_ROM_RecordId PRIMARY KEY (RecordId)
   , CONSTRAINT FK_TEMPORARY_Sampling_ROM_Header FOREIGN KEY (Header) REFERENCES TEMPORARY_Sampling_ROM_Header (RecordId)
);

create table TEMPORARY_CRUSHING_PLANT_Header
(
    RecordId BIGINT NOT NULL IDENTITY(1,1)
    , File_Physical BIGINT NOT NULL
    , Company varchar(32) not null default ''
    , Sheet VARCHAR(255) NOT NULL DEFAULT ''
    , CreatedBy INT NOT NULL DEFAULT 0
    , CreatedOn datetime NOT NULL DEFAULT GetDate()
    , Date_Detail VARCHAR(255) NOT NULL DEFAULT ''
    , Job_No VARCHAR(255) NOT NULL DEFAULT ''
    , Report_To VARCHAR(255) NOT NULL DEFAULT ''
    , Method1 VARCHAR(255) NOT NULL DEFAULT ''
    , Method2 VARCHAR(255) NOT NULL DEFAULT ''
    , Method3 VARCHAR(255) NOT NULL DEFAULT ''
    , Method4 VARCHAR(255) NOT NULL DEFAULT ''
    , CONSTRAINT PK_TEMPORARY_CRUSHING_PLANT_Header_RecordId PRIMARY KEY (RecordId)
    , CONSTRAINT FK_TEMPORARY_CRUSHING_PLANT_Header_File_Physical FOREIGN KEY (File_Physical) REFERENCES TEMPORARY_File (RecordId)
    , CONSTRAINT CO_TEMPORARY_CRUSHING_PLANT_Header_UNIQUE UNIQUE(File_Physical, Sheet)
);

create table TEMPORARY_CRUSHING_PLANT
(
    RecordId BIGINT NOT NULL IDENTITY(1,1)
    , Header BIGINT NOT NULL DEFAULT 0
    , Status VARCHAR(255) NOT NULL DEFAULT 'New'
    , CreatedBy INT NOT NULL DEFAULT 0
    , CreatedOn datetime NOT NULL DEFAULT GetDate()
    , Date_Production VARCHAR(255) NOT NULL DEFAULT ''
    , Shift_Work VARCHAR(255) NOT NULL DEFAULT ''
    , Tonnage  VARCHAR(255) NOT NULL DEFAULT ''
    , Sample_ID  VARCHAR(255) NOT NULL DEFAULT ''
    , TM VARCHAR(255) NOT NULL DEFAULT ''
    , M VARCHAR(255) NOT NULL DEFAULT ''
    , ASH_adb VARCHAR(255) NOT NULL DEFAULT ''
    , ASH_arb NUMERIC(10,2) NOT NULL DEFAULT 0
    , VM_adb VARCHAR(255) NOT NULL DEFAULT ''
    , VM_arb NUMERIC(10,2) NOT NULL DEFAULT 0
    , FC_adb NUMERIC(10,2) NOT NULL DEFAULT 0
    , FC_arb NUMERIC(10,2) NOT NULL DEFAULT 0
    , TS_adb VARCHAR(255) NOT NULL DEFAULT ''
    , TS_arb NUMERIC(10,2) NOT NULL DEFAULT 0
    , CV_adb VARCHAR(255) NOT NULL DEFAULT ''
    , CV_db NUMERIC(10,2) NOT NULL DEFAULT 0
    , CV_arb NUMERIC(10,2) NOT NULL DEFAULT 0
    , CV_daf NUMERIC(10,2) NOT NULL DEFAULT 0
    , CV_ad_15 NUMERIC(10,2) NOT NULL DEFAULT 0
    , CV_ad_16 NUMERIC(10,2) NOT NULL DEFAULT 0
    , CV_ad_17 NUMERIC(10,2) NOT NULL DEFAULT 0
    , Remark VARCHAR(255) NOT NULL DEFAULT ''
   , CONSTRAINT PK_TEMPORARY_CRUSHING_PLANT_RecordId PRIMARY KEY (RecordId)
   , CONSTRAINT FK_TEMPORARY_CRUSHING_PLANT_Header FOREIGN KEY (Header) REFERENCES TEMPORARY_CRUSHING_PLANT_Header (RecordId)
);

create table TEMPORARY_BARGE_LOADING_Header
(
    RecordId BIGINT NOT NULL IDENTITY(1,1)
    , File_Physical BIGINT NOT NULL
    , Company varchar(32) not null default ''
    , Sheet VARCHAR(255) NOT NULL DEFAULT ''
    , CreatedBy INT NOT NULL DEFAULT 0
    , CreatedOn datetime NOT NULL DEFAULT GetDate()
    , Date_Detail VARCHAR(255) NOT NULL DEFAULT ''
    , Job_No VARCHAR(255) NOT NULL DEFAULT ''
    , Report_To VARCHAR(255) NOT NULL DEFAULT ''
    , Method1 VARCHAR(255) NOT NULL DEFAULT ''
    , Method2 VARCHAR(255) NOT NULL DEFAULT ''
    , Method3 VARCHAR(255) NOT NULL DEFAULT ''
    , Method4 VARCHAR(255) NOT NULL DEFAULT ''
    , CONSTRAINT PK_TEMPORARY_BARGE_LOADING_Header_RecordId PRIMARY KEY (RecordId)
    , CONSTRAINT FK_TEMPORARY_BARGE_LOADING_Header_File_Physical FOREIGN KEY (File_Physical) REFERENCES TEMPORARY_File (RecordId)
    , CONSTRAINT CO_TEMPORARY_BARGE_LOADING_Header_UNIQUE UNIQUE(File_Physical, Sheet)
);

create table TEMPORARY_BARGE_LOADING
(
    RecordId BIGINT NOT NULL IDENTITY(1,1)
    , Header BIGINT NOT NULL DEFAULT 0
    , Status VARCHAR(255) NOT NULL DEFAULT 'New'
    , CreatedBy INT NOT NULL DEFAULT 0
    , CreatedOn datetime NOT NULL DEFAULT GetDate()
    , JOB_Number VARCHAR(255) NOT NULL DEFAULT ''
    , ID_Number VARCHAR(255) NOT NULL
    , Service_Trip_Number VARCHAR(255) NOT NULL DEFAULT ''
    , Date_Sampling  VARCHAR(255) NOT NULL DEFAULT ''
    , Date_Report  VARCHAR(255) NOT NULL DEFAULT ''
    , Tonnage VARCHAR(255) NOT NULL DEFAULT ''
    , Name  VARCHAR(255) NOT NULL DEFAULT ''
    , Destination  VARCHAR(255) NOT NULL DEFAULT ''
    , Temperature VARCHAR(255) NOT NULL DEFAULT ''
    , TM VARCHAR(255) NOT NULL DEFAULT ''
    , M VARCHAR(255) NOT NULL DEFAULT ''
    , ASH_adb VARCHAR(255) NOT NULL DEFAULT ''
    , ASH_arb NUMERIC(10,2) NOT NULL DEFAULT 0
    , VM_adb VARCHAR(255) NOT NULL DEFAULT ''
    , VM_arb NUMERIC(10,2) NOT NULL DEFAULT 0
    , FC_adb NUMERIC(10,2) NOT NULL DEFAULT 0
    , FC_arb NUMERIC(10,2) NOT NULL DEFAULT 0
    , TS_adb VARCHAR(255) NOT NULL DEFAULT ''
    , TS_arb NUMERIC(10,2) NOT NULL DEFAULT 0
    , CV_adb VARCHAR(255) NOT NULL DEFAULT ''
    , CV_db NUMERIC(10,2) NOT NULL DEFAULT 0
    , CV_arb NUMERIC(10,2) NOT NULL DEFAULT 0
    , CV_daf NUMERIC(10,2) NOT NULL DEFAULT 0
    , CV_ad_15 NUMERIC(10,2) NOT NULL DEFAULT 0
    , CV_ad_16 NUMERIC(10,2) NOT NULL DEFAULT 0
    , CV_ad_17 NUMERIC(10,2) NOT NULL DEFAULT 0
    , Remark VARCHAR(255) NOT NULL DEFAULT ''
   , CONSTRAINT PK_TEMPORARY_BARGE_LOADING_RecordId PRIMARY KEY (RecordId)
   , CONSTRAINT FK_TEMPORARY_BARGE_LOADING_Header FOREIGN KEY (Header) REFERENCES TEMPORARY_BARGE_LOADING_Header (RecordId)
);

create table UPLOAD_Geology_Pit_Monitoring_Header
(
    RecordId BIGINT NOT NULL IDENTITY(1,1)
    , File_Physical BIGINT NOT NULL DEFAULT 0
    , [TEMPORARY] BIGINT NOT NULL DEFAULT 0
    , Company varchar(32) not null default ''
    , CreatedBy INT NOT NULL DEFAULT 0
    , CreatedOn datetime NOT NULL DEFAULT GetDate()
    , Date_Detail VARCHAR(255) NOT NULL DEFAULT ''
    , Job_No VARCHAR(255) NOT NULL DEFAULT ''
    , Report_To VARCHAR(255) NOT NULL DEFAULT ''
    , Date_Received VARCHAR(255) NOT NULL DEFAULT ''
    , Nomor VARCHAR(255) NOT NULL DEFAULT ''
    , CONSTRAINT PK_UPLOAD_Geology_Pit_Monitoring_Header_RecordId PRIMARY KEY (RecordId)
);

create table UPLOAD_Geology_Pit_Monitoring
(
    RecordId BIGINT NOT NULL IDENTITY(1,1)
    , [TEMPORARY] BIGINT NOT NULL DEFAULT 0
    , Header BIGINT NOT NULL DEFAULT 0
    , CreatedBy INT NOT NULL DEFAULT 0
    , CreatedOn datetime NOT NULL DEFAULT GetDate()
    , LastModifiedBy INT NOT NULL DEFAULT 0
    , LastModifiedOn datetime NULL
    , Sample_ID VARCHAR(255) NOT NULL DEFAULT ''
    , SampleType VARCHAR(255) NOT NULL DEFAULT ''
    , Lab_ID  VARCHAR(255) NOT NULL UNIQUE -- ?? SampleID atau Lab_ID yang Unik
    , Mass_Spl NUMERIC(10,2) NOT NULL DEFAULT 0
    , TM NUMERIC(10,2) NOT NULL DEFAULT 0
    , M NUMERIC(10,2) NOT NULL DEFAULT 0
    , VM NUMERIC(10,2) NOT NULL DEFAULT 0
    , Ash NUMERIC(10,2) NOT NULL DEFAULT 0
    , FC NUMERIC(10,2) NOT NULL DEFAULT 0
    , TS NUMERIC(10,2) NOT NULL DEFAULT 0
    , Cal_ad NUMERIC(10,2) NOT NULL DEFAULT 0
    , Cal_db NUMERIC(10,2) NOT NULL DEFAULT 0
    , Cal_ar NUMERIC(10,2) NOT NULL DEFAULT 0
    , Cal_daf NUMERIC(10,2) NOT NULL DEFAULT 0
    , RD NUMERIC(10,2) NOT NULL DEFAULT 0
   , CONSTRAINT PK_UPLOAD_Geology_Pit_Monitoring_RecordId PRIMARY KEY (RecordId)
);

create table UPLOAD_Geology_Explorasi_Header
(
    RecordId BIGINT NOT NULL IDENTITY(1,1)
    , File_Physical BIGINT NOT NULL DEFAULT 0
    , [TEMPORARY] BIGINT NOT NULL DEFAULT 0
    , Company varchar(32) not null default ''
    , CreatedBy INT NOT NULL DEFAULT 0
    , CreatedOn datetime NOT NULL DEFAULT GetDate()
    , Date_Detail VARCHAR(255) NOT NULL DEFAULT ''
    , Job_No VARCHAR(255) NOT NULL DEFAULT ''
    , Report_To VARCHAR(255) NOT NULL DEFAULT ''
    , Date_Received VARCHAR(255) NOT NULL DEFAULT ''
    , Nomor VARCHAR(255) NOT NULL DEFAULT ''
    , CONSTRAINT PK_UPLOAD_Geology_Explorasi_Header_RecordId PRIMARY KEY (RecordId)
);

create table UPLOAD_Geology_Explorasi
(
    RecordId BIGINT NOT NULL IDENTITY(1,1)
    , [TEMPORARY] BIGINT NOT NULL DEFAULT 0
    , Header BIGINT NOT NULL DEFAULT 0
    , CreatedBy INT NOT NULL DEFAULT 0
    , CreatedOn datetime NOT NULL DEFAULT GetDate()
    , LastModifiedBy INT NOT NULL DEFAULT 0
    , LastModifiedOn datetime NULL
    , Sample_ID VARCHAR(255) NOT NULL DEFAULT ''
    , SampleType VARCHAR(255) NOT NULL DEFAULT ''
    , Lab_ID  VARCHAR(255) NOT NULL UNIQUE -- ?? SampleID atau Lab_ID yang Unik
    , Mass_Spl NUMERIC(10,2) NOT NULL DEFAULT 0
    , TM NUMERIC(10,2) NOT NULL DEFAULT 0
    , M NUMERIC(10,2) NOT NULL DEFAULT 0
    , VM NUMERIC(10,2) NOT NULL DEFAULT 0
    , Ash NUMERIC(10,2) NOT NULL DEFAULT 0
    , FC NUMERIC(10,2) NOT NULL DEFAULT 0
    , TS NUMERIC(10,2) NOT NULL DEFAULT 0
    , Cal_ad NUMERIC(10,2) NOT NULL DEFAULT 0
    , Cal_db NUMERIC(10,2) NOT NULL DEFAULT 0
    , Cal_ar NUMERIC(10,2) NOT NULL DEFAULT 0
    , Cal_daf NUMERIC(10,2) NOT NULL DEFAULT 0
    , RD NUMERIC(10,2) NOT NULL DEFAULT 0
   , CONSTRAINT PK_UPLOAD_Geology_Explorasi_RecordId PRIMARY KEY (RecordId)
);

create table UPLOAD_Sampling_ROM_Header
(
    RecordId BIGINT NOT NULL IDENTITY(1,1)
    , File_Physical BIGINT NOT NULL DEFAULT 0
    , [TEMPORARY] BIGINT NOT NULL DEFAULT 0
    , Company varchar(32) not null default ''
    , CreatedBy INT NOT NULL DEFAULT 0
    , CreatedOn datetime NOT NULL DEFAULT GetDate()
    , Date_Detail VARCHAR(255) NOT NULL DEFAULT ''
    , Job_No VARCHAR(255) NOT NULL DEFAULT ''
    , Report_To VARCHAR(255) NOT NULL DEFAULT ''
    , Method1 VARCHAR(255) NOT NULL DEFAULT ''
    , Method2 VARCHAR(255) NOT NULL DEFAULT ''
    , Method3 VARCHAR(255) NOT NULL DEFAULT ''
    , Method4 VARCHAR(255) NOT NULL DEFAULT ''
    , CONSTRAINT PK_UPLOAD_Sampling_ROM_Header_RecordId PRIMARY KEY (RecordId)
);

create table UPLOAD_Sampling_ROM
(
    RecordId BIGINT NOT NULL IDENTITY(1,1)
    , [TEMPORARY] BIGINT NOT NULL DEFAULT 0
    , Header BIGINT NOT NULL DEFAULT 0
    , CreatedBy INT NOT NULL DEFAULT 0
    , CreatedOn datetime NOT NULL DEFAULT GetDate()
    , LastModifiedBy INT NOT NULL DEFAULT 0
    , LastModifiedOn datetime NULL
    , Date_Request VARCHAR(255) NOT NULL DEFAULT ''
    , Date_Sampling VARCHAR(255) NOT NULL DEFAULT ''
    , Day_work VARCHAR(255) NOT NULL DEFAULT ''
    , LOT  VARCHAR(255) NOT NULL DEFAULT ''
    , Lab_ID  VARCHAR(255) NOT NULL UNIQUE
    , TM NUMERIC(10,2) NOT NULL DEFAULT 0
    , M NUMERIC(10,2) NOT NULL DEFAULT 0
    , ASH NUMERIC(10,2) NOT NULL DEFAULT 0
    , TS NUMERIC(10,2) NOT NULL DEFAULT 0
    , CV INT NOT NULL DEFAULT 0
    , Remark VARCHAR(255) NOT NULL DEFAULT ''
    , Seam VARCHAR(255) NOT NULL DEFAULT ''
   , CONSTRAINT PK_UPLOAD_Sampling_ROM_RecordId PRIMARY KEY (RecordId)
);

create table UPLOAD_CRUSHING_PLANT_Header
(
    RecordId BIGINT NOT NULL IDENTITY(1,1)
    , File_Physical BIGINT NOT NULL DEFAULT 0
    , [TEMPORARY] BIGINT NOT NULL DEFAULT 0
    , Company varchar(32) not null default ''
    , Sheet VARCHAR(255) NOT NULL DEFAULT ''
    , CreatedBy INT NOT NULL DEFAULT 0
    , CreatedOn datetime NOT NULL DEFAULT GetDate()
    , Date_Detail VARCHAR(255) NOT NULL DEFAULT ''
    , Job_No VARCHAR(255) NOT NULL DEFAULT ''
    , Report_To VARCHAR(255) NOT NULL DEFAULT ''
    , Method1 VARCHAR(255) NOT NULL DEFAULT ''
    , Method2 VARCHAR(255) NOT NULL DEFAULT ''
    , Method3 VARCHAR(255) NOT NULL DEFAULT ''
    , Method4 VARCHAR(255) NOT NULL DEFAULT ''
    , CONSTRAINT PK_UPLOAD_CRUSHING_PLANT_Header_RecordId PRIMARY KEY (RecordId)
);

create table UPLOAD_CRUSHING_PLANT
(
    RecordId BIGINT NOT NULL IDENTITY(1,1)
    , [TEMPORARY] BIGINT NOT NULL DEFAULT 0
    , Header BIGINT NOT NULL DEFAULT 0
    , Sheet VARCHAR(255) NOT NULL DEFAULT ''
    , CreatedBy INT NOT NULL DEFAULT 0
    , CreatedOn datetime NOT NULL DEFAULT GetDate()
    , LastModifiedBy INT NOT NULL DEFAULT 0
    , LastModifiedOn datetime NULL
    , Date_Production VARCHAR(255) NOT NULL DEFAULT ''
    , Shift_Work VARCHAR(255) NOT NULL DEFAULT ''
    , Tonnage  NUMERIC(10,3) NOT NULL DEFAULT 0
    , Sample_ID  VARCHAR(255) NOT NULL UNIQUE
    , TM NUMERIC(10,2) NOT NULL DEFAULT 0
    , M NUMERIC(10,2) NOT NULL DEFAULT 0
    , ASH_adb NUMERIC(10,2) NOT NULL DEFAULT 0
    , ASH_arb NUMERIC(10,2) NOT NULL DEFAULT 0
    , VM_adb NUMERIC(10,2) NOT NULL DEFAULT 0
    , VM_arb NUMERIC(10,2) NOT NULL DEFAULT 0
    , FC_adb NUMERIC(10,2) NOT NULL DEFAULT 0
    , FC_arb NUMERIC(10,2) NOT NULL DEFAULT 0
    , TS_adb NUMERIC(10,2) NOT NULL DEFAULT 0
    , TS_arb NUMERIC(10,2) NOT NULL DEFAULT 0
    , CV_adb NUMERIC(10,2) NOT NULL DEFAULT 0
    , CV_db NUMERIC(10,2) NOT NULL DEFAULT 0
    , CV_arb NUMERIC(10,2) NOT NULL DEFAULT 0
    , CV_daf NUMERIC(10,2) NOT NULL DEFAULT 0
    , CV_ad_15 NUMERIC(10,2) NOT NULL DEFAULT 0
    , CV_ad_16 NUMERIC(10,2) NOT NULL DEFAULT 0
    , CV_ad_17 NUMERIC(10,2) NOT NULL DEFAULT 0
    , Remark VARCHAR(255) NOT NULL DEFAULT ''
   , CONSTRAINT PK_UPLOAD_CRUSHING_PLANT_RecordId PRIMARY KEY (RecordId)
);

create table UPLOAD_BARGE_LOADING_Header
(
    RecordId BIGINT NOT NULL IDENTITY(1,1)
    , File_Physical BIGINT NOT NULL
    , [TEMPORARY] BIGINT NOT NULL DEFAULT 0
    , Company varchar(32) not null default ''
    , Sheet VARCHAR(255) NOT NULL DEFAULT ''
    , CreatedBy INT NOT NULL DEFAULT 0
    , CreatedOn datetime NOT NULL DEFAULT GetDate()
    , Date_Detail VARCHAR(255) NOT NULL DEFAULT ''
    , Job_No VARCHAR(255) NOT NULL DEFAULT ''
    , Report_To VARCHAR(255) NOT NULL DEFAULT ''
    , Method1 VARCHAR(255) NOT NULL DEFAULT ''
    , Method2 VARCHAR(255) NOT NULL DEFAULT ''
    , Method3 VARCHAR(255) NOT NULL DEFAULT ''
    , Method4 VARCHAR(255) NOT NULL DEFAULT ''
    , CONSTRAINT PK_UPLOAD_BARGE_LOADING_Header_RecordId PRIMARY KEY (RecordId)
);

create table UPLOAD_BARGE_LOADING
(
    RecordId BIGINT NOT NULL IDENTITY(1,1)
    , [TEMPORARY] BIGINT NOT NULL DEFAULT 0
    , Header BIGINT NOT NULL DEFAULT 0
    , Sheet VARCHAR(255) NOT NULL DEFAULT ''
    , CreatedBy INT NOT NULL DEFAULT 0
    , CreatedOn datetime NOT NULL DEFAULT GetDate()
    , LastModifiedBy INT NOT NULL DEFAULT 0
    , LastModifiedOn datetime NULL
    , JOB_Number VARCHAR(255) NOT NULL DEFAULT ''
    , ID_Number VARCHAR(255) NOT NULL UNIQUE
    , Service_Trip_Number VARCHAR(255) NOT NULL DEFAULT ''
    , Date_Sampling  VARCHAR(255) NOT NULL DEFAULT ''
    , Date_Report  VARCHAR(255) NOT NULL DEFAULT ''
    , Tonnage NUMERIC(10,3) NOT NULL DEFAULT 0
    , Name  VARCHAR(255) NOT NULL DEFAULT ''
    , Destination  VARCHAR(255) NOT NULL DEFAULT ''
    , Temperature NUMERIC(10,2) NOT NULL DEFAULT 0
    , TM NUMERIC(10,2) NOT NULL DEFAULT 0
    , M NUMERIC(10,2) NOT NULL DEFAULT 0
    , ASH_adb NUMERIC(10,2) NOT NULL DEFAULT 0
    , ASH_arb NUMERIC(10,2) NOT NULL DEFAULT 0
    , VM_adb NUMERIC(10,2) NOT NULL DEFAULT 0
    , VM_arb NUMERIC(10,2) NOT NULL DEFAULT 0
    , FC_adb NUMERIC(10,2) NOT NULL DEFAULT 0
    , FC_arb NUMERIC(10,2) NOT NULL DEFAULT 0
    , TS_adb NUMERIC(10,2) NOT NULL DEFAULT 0
    , TS_arb NUMERIC(10,2) NOT NULL DEFAULT 0
    , CV_adb NUMERIC(10,2) NOT NULL DEFAULT 0
    , CV_db NUMERIC(10,2) NOT NULL DEFAULT 0
    , CV_arb NUMERIC(10,2) NOT NULL DEFAULT 0
    , CV_daf NUMERIC(10,2) NOT NULL DEFAULT 0
    , CV_ad_15 NUMERIC(10,2) NOT NULL DEFAULT 0
    , CV_ad_16 NUMERIC(10,2) NOT NULL DEFAULT 0
    , CV_ad_17 NUMERIC(10,2) NOT NULL DEFAULT 0
    , Remark VARCHAR(255) NOT NULL DEFAULT ''
   , CONSTRAINT PK_UPLOAD_BARGE_LOADING_RecordId PRIMARY KEY (RecordId)
);

/*
delete from TEMPORARY_BARGE_LOADING;
delete from TEMPORARY_BARGE_LOADING_Header;
delete from TEMPORARY_CRUSHING_PLANT;
delete from TEMPORARY_CRUSHING_PLANT_Header;
delete from TEMPORARY_Sampling_ROM;
delete from TEMPORARY_Sampling_ROM_Header;
delete from TEMPORARY_Geology_Pit_Monitoring;
delete from TEMPORARY_Geology_Pit_Monitoring_Header;
delete from TEMPORARY_Geology_Explorasi;
delete from TEMPORARY_Geology_Explorasi_Header;
delete from TEMPORARY_Barge_Quality_Plan;
delete from TEMPORARY_Barge_Quality_Plan_Header;
delete from TEMPORARY_Barge_Line_Up;
delete from TEMPORARY_BARGE_LOADING_COA;

delete from TEMPORARY_File;

delete from UPLOAD_BARGE_LOADING_Header;
delete from UPLOAD_BARGE_LOADING;
delete from UPLOAD_CRUSHING_PLANT_Header;
delete from UPLOAD_CRUSHING_PLANT;
delete from UPLOAD_Sampling_ROM_Header;
delete from UPLOAD_Sampling_ROM;
delete from UPLOAD_Geology_Explorasi_Header;
delete from UPLOAD_Geology_Explorasi;
delete from UPLOAD_Geology_Pit_Monitoring_Header;
delete from UPLOAD_Geology_Pit_Monitoring;
delete from UPLOAD_Barge_Quality_Plan;
delete from UPLOAD_Barge_Quality_Plan_Header;
delete from UPLOAD_Barge_Line_Up;
delete from UPLOAD_BARGE_LOADING_COA;

*/

create table dbo.ReadRequest(
    RecordId BIGINT NOT NULL IDENTITY(1,1)
    , RequestType varchar(255)
    , RequestId BIGINT NOT NULL DEFAULT 0
    , CreatedBy INT NOT NULL DEFAULT 0
    , CreatedOn datetime NOT NULL DEFAULT GetDate()
    , CONSTRAINT PK_ReadRequest_RecordId PRIMARY KEY (RecordId)
    , unique(RequestType, RequestId, CreatedBy)
);

create table dbo.PortionBlending(
    RecordId        bigint identity(1,1)
    , Company       varchar(255) not null default ''
    , Product       varchar(255) not null default ''
    , BlendingDate  date not null default GetDate()
    , Shift         int not null default 1
    , NoHauling     bit NOT NULL DEFAULT 0
    , Remark        varchar(255) not null default ''
    , CV            float not null default 0
    , TS            float not null default 0
    , ASH           float not null default 0
    , IM            float not null default 0
    , TM            float not null default 0
    , Ton           decimal(10,2) not null default 0
    , Hopper        varchar(255) not null default ''
    , Tunnel        varchar(255) not null default ''
    , CreatedBy     INT NOT NULL DEFAULT 0
    , CreatedOn     datetime NOT NULL DEFAULT GetDate()
    , LastModifiedBy INT NOT NULL DEFAULT 0
    , LastModifiedOn datetime NULL
    , DeletedBy     INT NOT NULL DEFAULT 0
    , DeletedOn     datetime NULL
    , CONSTRAINT PK_PortionBlending_RecordId PRIMARY KEY (RecordId)
);

create table dbo.PortionBlending_Details(
    RecordId        bigint identity(1,1)
    , PortionBlending bigint not null 
    , ROMQuality_RecordId bigint not null 
    , block         nvarchar(10) not null default ''
    , ROM_Name      nvarchar(255)  not null default ''  
    , ROM_ID        int not null default 1
    , CV            float not null default 0
    , TS            float not null default 0
    , ASH           float not null default 0
    , IM            float not null default 0
    , TM            float not null default 0
    , Ton           decimal(10,2) not null default 0
    , Portion       float not null default 0
    , CONSTRAINT PK_PortionBlending_Details_RecordId PRIMARY KEY (RecordId)
    , CONSTRAINT PK_PortionBlending_Details_PortionBlending FOREIGN KEY (PortionBlending) REFERENCES PortionBlending (RecordId)
);

create table dbo.ROMTransfer(
    RecordId        bigint identity(1,1)
    , Company       varchar(255) not null default ''
    , TransferDate  date not null default GetDate()
    , Shift         int not null default 1
    , Source_ROM_ID int
    , Source_Block nvarchar(255)
    , Source_ROM_Name nvarchar(255)
    , Destination_ROM_ID    int
    , Destination_Block nvarchar(255)
    , Destination_ROM_Name nvarchar(255)
    , Remark        varchar(255) not null default ''
    , CreatedBy     INT NOT NULL DEFAULT 0
    , CreatedOn     datetime NOT NULL DEFAULT GetDate()
    , LastModifiedBy INT NOT NULL DEFAULT 0
    , LastModifiedOn datetime NULL
    , DeletedBy     INT NOT NULL DEFAULT 0
    , DeletedOn     datetime NULL
    , CONSTRAINT PK_ROMTransfer_RecordId PRIMARY KEY (RecordId)
);

create table dbo.HaulingRequest(
    RecordId        bigint identity(1,1)
    , DateFrom      date NOT NULL DEFAULT GetDate()
    , DateTo      date NOT NULL DEFAULT GetDate()
    , CreatedBy     INT NOT NULL DEFAULT 0
    , CreatedOn     datetime NOT NULL DEFAULT GetDate()
    , LastModifiedBy INT NOT NULL DEFAULT 0
    , LastModifiedOn datetime NULL
    , DeletedBy     INT NOT NULL DEFAULT 0
    , DeletedOn     datetime NULL
    , CONSTRAINT PK_HaulingRequest_RecordId PRIMARY KEY (RecordId)
);

create table dbo.HaulingRequest_Detail(
    RecordId        bigint identity(1,1)
    , HaulingRequest bigint not null
    , DataType      varchar(255) default 'PortionBlending'   /* PortionBlending, ROM_Transfer */
    , DataId        bigint not null
    , CONSTRAINT PK_HaulingRequest_Detail_RecordId PRIMARY KEY (RecordId)
    , CONSTRAINT FK_HaulingRequest_Detail_HaulingRequest FOREIGN KEY (HaulingRequest) REFERENCES HaulingRequest (RecordId)
);

ALTER TABLE [User] ADD UserInterface varchar not null default '';
go
update [User] set UserInterface = '';

ALTER TABLE [User] ADD IsCPL bit NOT NULL DEFAULT 0;
go
update [User] set IsCPL = 0;

ALTER TABLE [User] ADD IsLabMaintenance bit NOT NULL DEFAULT 0;
go
update [User] set IsLabMaintenance = 0;

ALTER TABLE [User] ADD IsUserBiasa bit NOT NULL DEFAULT 0;
go
update [User] set IsUserBiasa = 0;

ALTER TABLE [User] ADD IsUserLab bit NOT NULL DEFAULT 0;
go
update [User] set IsUserLab = 0;

ALTER TABLE [User] ADD IsMasterData bit NOT NULL DEFAULT 0;
go
update [User] set IsMasterData = 0;

ALTER TABLE [User] ADD IsConsumable bit NOT NULL DEFAULT 0;
go
update [User] set IsConsumable = 0;

ALTER TABLE [User] ADD Is_ActiveDirectory bit NOT NULL DEFAULT 0;
go
update [User] set Is_ActiveDirectory = 1;

ALTER TABLE [User] ADD Pwd_DB varchar(255) default '*****';
go
update [User] set Pwd_DB = '*****';

create table dbo.Config(
    RecordId int identity(1,1)
    , Name   varchar(255) not null unique
    , Value  varchar(255) not null default ''
    , CONSTRAINT PK_Config_RecordId PRIMARY KEY (RecordId)
);

create table dbo.UnitMeasurement(
    RecordId int identity(1,1)
    , UnitName   varchar(255) not null unique
    , Status bit NOT NULL DEFAULT 0
    , CreatedBy INT NOT NULL DEFAULT 0
    , CreatedOn datetime NOT NULL DEFAULT GetDate()
    , LastModifiedBy INT NOT NULL DEFAULT 0
    , LastModifiedOn datetime NULL
    , CONSTRAINT PK_UnitMeasurement_RecordId PRIMARY KEY (RecordId)
);

create table dbo.Instrument(
    RecordId int identity(1,1)
    , InstrumentName   varchar(255) not null unique
    , Status bit NOT NULL DEFAULT 0
    , CreatedBy INT NOT NULL DEFAULT 0
    , CreatedOn datetime NOT NULL DEFAULT GetDate()
    , LastModifiedBy INT NOT NULL DEFAULT 0
    , LastModifiedOn datetime NULL
    , CONSTRAINT PK_Instrument_RecordId PRIMARY KEY (RecordId)
);

create table dbo.MaintenanceActivity(
    RecordId int identity(1,1)
    , Category   varchar(255) not null unique
    , Status bit NOT NULL DEFAULT 0
    , CreatedBy INT NOT NULL DEFAULT 0
    , CreatedOn datetime NOT NULL DEFAULT GetDate()
    , LastModifiedBy INT NOT NULL DEFAULT 0
    , LastModifiedOn datetime NULL
    , CONSTRAINT PK_MaintenanceActivity_RecordId PRIMARY KEY (RecordId)
);

CREATE TABLE SamplingRequest_History
(
    RecordId BIGINT NOT NULL IDENTITY(1,1)
    , SamplingRequestId BIGINT NOT NULL
    , Description varchar(255) default ''
    , CreatedBy INT NOT NULL DEFAULT 0
    , CreatedOn datetime NOT NULL DEFAULT GetDate()
    , CONSTRAINT PK_SamplingRequest_History_RecordId PRIMARY KEY (RecordId)
);

CREATE TABLE AnalysisRequest_History
(
    RecordId BIGINT NOT NULL IDENTITY(1,1)
    , AnalysisRequestId BIGINT NOT NULL
    , Description varchar(255) default ''
    , CreatedBy INT NOT NULL DEFAULT 0
    , CreatedOn datetime NOT NULL DEFAULT GetDate()
    , CONSTRAINT PK_AnalysisRequest_History_RecordId PRIMARY KEY (RecordId)
);

CREATE TABLE Consumable
(
    RecordId BIGINT NOT NULL IDENTITY(1,1)
    , Instrument int not null
    , PartName varchar(255) not null default ''
    , PartNumber varchar(255) not null default ''
    , MSDS_Code varchar(255) not null default ''
    , CurrentQuantity int not null default 0
    , MinimumQuantity int not null default 0
    , InputQuantity int not null default 0
    , OutputQuantity int not null default 0
    , UnitType int not null
    , ReoderDays int not null default 0
    , Price float not null default 0
    , NotesFile varchar(255) not null default ''
    , NotesFile2 varchar(255) not null default ''
    , PictureFile varchar(255) not null default ''
    , PictureFile2 varchar(255) not null default ''
    , Remark varchar(255) not null default ''
    , CreatedBy INT NOT NULL DEFAULT 0
    , CreatedOn datetime NOT NULL DEFAULT GetDate()
    , LastModifiedBy INT NOT NULL DEFAULT 0
    , LastModifiedOn datetime NULL
    , CONSTRAINT PK_Consumable_RecordId PRIMARY KEY (RecordId)
    , CONSTRAINT FK_Consumable_Instrument FOREIGN KEY (Instrument) REFERENCES Instrument (RecordId)
    , CONSTRAINT FK_Consumable_UnitType FOREIGN KEY (UnitType) REFERENCES UnitMeasurement (RecordId)
);

CREATE TABLE Consumable_History
(
    RecordId BIGINT NOT NULL IDENTITY(1,1)
    , Consumable_Data BIGINT NOT NULL default 0
    , Instrument int not null
    , PartName varchar(255) not null default ''
    , PartNumber varchar(255) not null default ''
    , MSDS_Code varchar(255) not null default ''
    , CurrentQuantity int not null default 0
    , MinimumQuantity int not null default 0
    , InputQuantity int not null default 0
    , OutputQuantity int not null default 0
    , UnitType int not null
    , ReoderDays int not null default 0
    , Price float not null default 0
    , NotesFile varchar(255) not null default ''
    , NotesFile2 varchar(255) not null default ''
    , PictureFile varchar(255) not null default ''
    , PictureFile2 varchar(255) not null default ''
    , Remark varchar(255) not null default ''
    , CreatedBy INT NOT NULL DEFAULT 0
    , CreatedOn datetime NOT NULL DEFAULT GetDate()
    , LastModifiedBy INT NOT NULL DEFAULT 0
    , LastModifiedOn datetime NULL
    , CONSTRAINT PK_Consumable_History_RecordId PRIMARY KEY (RecordId)
);

CREATE TABLE LabMaintenance
(
    RecordId BIGINT NOT NULL IDENTITY(1,1)
    , Category int not null
    , Instrument int not null
    , InstrumentCode varchar(255) not null default ''
    , Description varchar(255) not null default ''
    , AssignedTo INT NOT NULL DEFAULT 0
    , Remark varchar(255) not null default ''
    , NextSchedule datetime NOT NULL DEFAULT GetDate()
    , RepeatEvery varchar(255) not null default ''
    , RepeatCount int not null default 0
    , NotesFile varchar(255) not null default ''
    , NotesFile2 varchar(255) not null default ''
    , RunWhat varchar(255) not null default ''
    , RunWhat2 varchar(255) not null default ''
    , CreatedBy INT NOT NULL DEFAULT 0
    , CreatedOn datetime NOT NULL DEFAULT GetDate()
    , LastModifiedBy INT NOT NULL DEFAULT 0
    , LastModifiedOn datetime NULL
    , CONSTRAINT PK_LabMaintenance_RecordId PRIMARY KEY (RecordId)
    , CONSTRAINT FK_LabMaintenance_Category FOREIGN KEY (Category) REFERENCES MaintenanceActivity (RecordId)
    , CONSTRAINT FK_LabMaintenance_Instrument FOREIGN KEY (Instrument) REFERENCES Instrument (RecordId)
);

CREATE TABLE LabMaintenance_History
(
    RecordId BIGINT NOT NULL IDENTITY(1,1)
    , LabMaintenance_Data BIGINT NOT NULL default 0
    , Category int not null
    , Instrument int not null
    , InstrumentCode varchar(255) not null default ''
    , Description varchar(255) not null default ''
    , AssignedTo INT NOT NULL DEFAULT 0
    , Remark varchar(255) not null default ''
    , NextSchedule datetime NOT NULL DEFAULT GetDate()
    , RepeatEvery varchar(255) not null default ''
    , RepeatCount int not null default 0
    , NotesFile varchar(255) not null default ''
    , NotesFile2 varchar(255) not null default ''
    , RunWhat varchar(255) not null default ''
    , RunWhat2 varchar(255) not null default ''
    , CreatedBy INT NOT NULL DEFAULT 0
    , CreatedOn datetime NOT NULL DEFAULT GetDate()
    , LastModifiedBy INT NOT NULL DEFAULT 0
    , LastModifiedOn datetime NULL
    , CONSTRAINT PK_LabMaintenance_History_RecordId PRIMARY KEY (RecordId)
);

ALTER TABLE Company ADD Is_DataROM_from_BI bit NOT NULL DEFAULT 1;
go
update Company set Is_DataROM_from_BI = 1;
  
ALTER TABLE [User] ADD IsReport bit NOT NULL DEFAULT 0;
go
update [User] set IsReport = 0;

ALTER TABLE AnalysisRequest ADD CoverLetter varchar(255) not null default '';
go
update AnalysisRequest set CoverLetter = '';

ALTER TABLE AnalysisRequest ADD CoverLetter2 varchar(255) not null default '';
go
update AnalysisRequest set CoverLetter2 = '';

create table dbo.HaulingRequest_Detail_PortionBlending(
    RecordIdx        bigint identity(1,1)
    , HaulingRequest bigint not null
    , RecordId_Snapshot bigint not null
    , Company       varchar(255) not null default ''
    , Product       varchar(255) not null default ''
    , BlendingDate  date not null default GetDate()
    , Shift         int not null default 1
    , NoHauling     bit NOT NULL DEFAULT 0
    , Remark        varchar(255) not null default ''
    , CV            float not null default 0
    , TS            float not null default 0
    , ASH           float not null default 0
    , IM            float not null default 0
    , TM            float not null default 0
    , Ton           decimal(10,2) not null default 0
    , Hopper        varchar(255) not null default ''
    , Tunnel        varchar(255) not null default ''
    , CreatedBy     INT NOT NULL DEFAULT 0
    , CreatedOn     datetime NOT NULL DEFAULT GetDate()
    , LastModifiedBy INT NOT NULL DEFAULT 0
    , LastModifiedOn datetime NULL
    , DeletedBy     INT NOT NULL DEFAULT 0
    , DeletedOn     datetime NULL
    , CONSTRAINT PK_HaulingRequest_Detail_PortionBlending_RecordIdx PRIMARY KEY (RecordIdx)
    , CONSTRAINT FK_HaulingRequest_Detail_PortionBlending_HaulingRequest FOREIGN KEY (HaulingRequest) REFERENCES HaulingRequest (RecordId)
);

create table dbo.HaulingRequest_Detail_PortionBlending_Details(
    RecordIdx        bigint identity(1,1)
    , HaulingRequest bigint not null
    , RecordId_Snapshot bigint not null
    , PortionBlending bigint not null 
    , ROMQuality_RecordId bigint not null 
    , block         nvarchar(10) not null default ''
    , ROM_Name      nvarchar(255)  not null default ''  
    , ROM_ID        int not null default 1
    , CV            float not null default 0
    , TS            float not null default 0
    , ASH           float not null default 0
    , IM            float not null default 0
    , TM            float not null default 0
    , Ton           decimal(10,2) not null default 0
    , Portion       float not null default 0
    , CONSTRAINT PK_HaulingRequest_Detail_PortionBlending_Details_RecordIdx PRIMARY KEY (RecordIdx)
    , CONSTRAINT FK_HaulingRequest_Detail_PortionBlending_Details_HaulingRequest FOREIGN KEY (HaulingRequest) REFERENCES HaulingRequest (RecordId)
);

create table dbo.HaulingRequest_Detail_ROMTransfer(
    RecordIdx        bigint identity(1,1)
    , HaulingRequest bigint not null
    , RecordId_Snapshot bigint not null
    , Company       varchar(255) not null default ''
    , TransferDate  date not null default GetDate()
    , Shift         int not null default 1
    , Source_ROM_ID int
    , Source_Block nvarchar(255)
    , Source_ROM_Name nvarchar(255)
    , Destination_ROM_ID    int
    , Destination_Block nvarchar(255)
    , Destination_ROM_Name nvarchar(255)
    , Remark        varchar(255) not null default ''
    , CreatedBy     INT NOT NULL DEFAULT 0
    , CreatedOn     datetime NOT NULL DEFAULT GetDate()
    , LastModifiedBy INT NOT NULL DEFAULT 0
    , LastModifiedOn datetime NULL
    , DeletedBy     INT NOT NULL DEFAULT 0
    , DeletedOn     datetime NULL
    , CONSTRAINT PK_HaulingRequest_Detail_ROMTransfer_RecordIdx PRIMARY KEY (RecordIdx)
    , CONSTRAINT FK_HaulingRequest_Detail_ROMTransfer_HaulingRequest FOREIGN KEY (HaulingRequest) REFERENCES HaulingRequest (RecordId)
);

ALTER TABLE Consumable_History ADD Description varchar(8000) not null default '';
go
update Consumable_History set Description = '';

ALTER TABLE dbo.Config ALTER COLUMN [Value] VARCHAR (8000) not null;
go

ALTER TABLE LabMaintenance ADD IsActive bit NOT NULL DEFAULT 1;
go
ALTER TABLE LabMaintenance_History ADD IsActive bit NOT NULL DEFAULT 1;
go

create table Reminder_History_Lab_Consumable(
    RecordId            bigint identity(1,1)
    , CreatedOn datetime NOT NULL DEFAULT GetDate()
    , Consumable_Data   BIGINT
    , Period            int
    , CONSTRAINT PK_Reminder_History_Lab_Consumable_RecordId PRIMARY KEY (RecordId)
);

create table Reminder_History_Lab_Maintenance(
    RecordId            bigint identity(1,1)
    , CreatedOn datetime NOT NULL DEFAULT GetDate()
    , Maintenance_Data   BIGINT
    , Period            int
    , CONSTRAINT PK_Reminder_History_Lab_Maintenance_RecordId PRIMARY KEY (RecordId)
);

CREATE TABLE LabMaintenance_Complete
(
    RecordId BIGINT NOT NULL IDENTITY(1,1)
    , LabMaintenance_Data BIGINT NOT NULL default 0
    , IsActive bit NOT NULL DEFAULT 0
    , Category int not null
    , Instrument int not null
    , InstrumentCode varchar(255) not null default ''
    , Description varchar(255) not null default ''
    , AssignedTo INT NOT NULL DEFAULT 0
    , Remark varchar(255) not null default ''
    , NextSchedule datetime NOT NULL DEFAULT GetDate()
    , RepeatEvery varchar(255) not null default ''
    , RepeatCount int not null default 0
    , NotesFile varchar(255) not null default ''
    , NotesFile2 varchar(255) not null default ''
    , RunWhat varchar(255) not null default ''
    , RunWhat2 varchar(255) not null default ''
    , CreatedBy INT NOT NULL DEFAULT 0
    , CreatedOn datetime NOT NULL DEFAULT GetDate()
    , LastModifiedBy INT NOT NULL DEFAULT 0
    , LastModifiedOn datetime NULL
    , CompletedBy INT NOT NULL DEFAULT 0
    , CompletedOn datetime NOT NULL DEFAULT GetDate()
    , CONSTRAINT PK_LabMaintenance_Complete_RecordId PRIMARY KEY (RecordId)
);

