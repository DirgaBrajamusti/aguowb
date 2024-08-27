CREATE TABLE Tunnel_History
(
    RecordId int IDENTITY(1,1) NOT NULL,
    TunnelId INT NOT NULL,
    CompanyCode VARCHAR(32) NOT NULL,
    Name VARCHAR(255) NOT NULL,
    CreatedBy INT NOT NULL DEFAULT 0,
    CreatedOn datetime NOT NULL DEFAULT GetDate(),
    LastModifiedBy INT NOT NULL DEFAULT 0,
    LastModifiedOn datetime NULL,
    IsActive bit NOT NULL DEFAULT 0,
    ProductId int NOT NULL,
    CONSTRAINT PK_Tunnel_History_RecordId PRIMARY KEY (RecordId)
);

insert into Tunnel_History(TunnelId,CompanyCode,Name ,CreatedBy
    ,CreatedOn ,LastModifiedBy ,LastModifiedOn ,IsActive,ProductId)
    select TunnelId ,CompanyCode ,Name ,CreatedBy ,CreatedOn
        ,LastModifiedBy ,LastModifiedOn ,IsActive,ProductId from Tunnel;

create table UPLOAD_LECO_CV
(
    RecordId int IDENTITY(1,1) NOT NULL
    , CreatedBy INT NOT NULL DEFAULT 0
    , CreatedOn datetime NOT NULL DEFAULT GetDate()
    
    , NAME  VARCHAR(255) NOT NULL default ''
    , HYDROGEN  VARCHAR(255) NOT NULL default ''
    , [OPERATOR]    VARCHAR(255) NOT NULL default ''
    , DISCRIPTION   VARCHAR(255) NOT NULL default ''
    , METHOD    VARCHAR(255) NOT NULL default ''
    , MASS_1_gram   VARCHAR(255) NOT NULL default ''
    , CV    VARCHAR(255) NOT NULL default ''
    , FUSE_LENGTH   VARCHAR(255) NOT NULL default ''
    , ANALYSIS_DATE VARCHAR(255) NOT NULL default ''
    , VESSEL    VARCHAR(255) NOT NULL default ''
    
    , CONSTRAINT PK_UPLOAD_LECO_CV_RecordId PRIMARY KEY (RecordId)
);

create table UPLOAD_LECO_TS
(
    RecordId int IDENTITY(1,1) NOT NULL
    , CreatedBy INT NOT NULL DEFAULT 0
    , CreatedOn datetime NOT NULL DEFAULT GetDate()
    
    , [NO]  VARCHAR(255) NOT NULL default ''
    , Name  VARCHAR(255) NOT NULL default ''
    , Description   VARCHAR(255) NOT NULL default ''
    , Weight    VARCHAR(255) NOT NULL default ''
    , Sulfur    VARCHAR(255) NOT NULL default ''
    , Analysis_Time VARCHAR(255) NOT NULL default ''
    , [Date]    VARCHAR(255) NOT NULL default ''
    , Method    VARCHAR(255) NOT NULL default ''
    , [Operator]    VARCHAR(255) NOT NULL default ''
    , Low_Sulfur    VARCHAR(255) NOT NULL default ''
    , High_Sulfur   VARCHAR(255) NOT NULL default ''

    , CONSTRAINT PK_UPLOAD_LECO_TS_RecordId PRIMARY KEY (RecordId)
);

create table UPLOAD_LECO_TGA
(
    RecordId int IDENTITY(1,1) NOT NULL
    , CreatedBy INT NOT NULL DEFAULT 0
    , CreatedOn datetime NOT NULL DEFAULT GetDate()
    
    , NAME  VARCHAR(255) NOT NULL default ''
    , MOISTURE  VARCHAR(255) NOT NULL default ''
    , VOLATILE  VARCHAR(255) NOT NULL default ''
    , ASH   VARCHAR(255) NOT NULL default ''
    , INITIAL_MASS  VARCHAR(255) NOT NULL default ''
    , CRUCIBLE_MASS VARCHAR(255) NOT NULL default ''
    , BATCH VARCHAR(255) NOT NULL default ''
    , LOCATION  VARCHAR(255) NOT NULL default ''
    , METHOD    VARCHAR(255) NOT NULL default ''
    , ANALYSIS_DATE VARCHAR(255) NOT NULL default ''
    , MAISTURE_MASS VARCHAR(255) NOT NULL default ''
    , VOLATILE_MASS VARCHAR(255) NOT NULL default ''
    , ASH_MASS  VARCHAR(255) NOT NULL default ''
    , FIXWD_CARBON  VARCHAR(255) NOT NULL default ''
    , VOLATILE_DRY  VARCHAR(255) NOT NULL default ''
    , ASH_DRY   VARCHAR(255) NOT NULL default ''
    , FIXWD_CARBON_DRY  VARCHAR(255) NOT NULL default ''
    
    , CONSTRAINT PK_UPLOAD_LECO_TGA_RecordId PRIMARY KEY (RecordId)
);

alter table Loading_Actual add Shipment_Type [varchar](32) NOT NULL default '';

alter table Tunnel_Actual add [Status] [varchar](32) NOT NULL default 'Draft';

CREATE TABLE Tunnel_Actual_History(
    RecordId_ bigint IDENTITY(1,1) NOT NULL
    , CreatedOn_ datetime NOT NULL DEFAULT GetDate()
    
    , Tunnel_Actual_Id bigint
    , HaulingRequest_Reference bigint NOT NULL
    , TunnelId int NOT NULL
    , CreatedBy int NOT NULL
    , CreatedOn datetime NOT NULL
    , LastModifiedBy int NOT NULL
    , LastModifiedOn datetime NULL
    , [Time] datetime NOT NULL
    , Remark varchar(255) NOT NULL
    , Status varchar(32) NOT NULL
    , Changed_Tunnel varchar(255) NOT NULL default ''
    , CONSTRAINT PK_Tunnel_Actual_History_RecordId_ PRIMARY KEY (RecordId_)
);

alter table TEMPORARY_Sampling_ROM_Header add CreatedOn_Date_Only varchar(10) not null default '';
alter table TEMPORARY_Sampling_ROM_Header add CreatedOn_Year_Only int not null default 0;

alter table TEMPORARY_Sampling_ROM add Company varchar(32) not null default '';
alter table TEMPORARY_Sampling_ROM add CreatedOn_Date_Only varchar(10) not null default '';
alter table TEMPORARY_Sampling_ROM add CreatedOn_Year_Only int not null default 0;

alter table UPLOAD_Sampling_ROM_Header add CreatedOn_Date_Only varchar(10) not null default '';
alter table UPLOAD_Sampling_ROM_Header add CreatedOn_Year_Only int not null default 0;

--alter table UPLOAD_Sampling_ROM add Company varchar(32) not null default '';
alter table UPLOAD_Sampling_ROM add CreatedOn_Date_Only varchar(10) not null default '';
alter table UPLOAD_Sampling_ROM add CreatedOn_Year_Only int not null default 0;

alter table TEMPORARY_Geology_Pit_Monitoring_Header add CreatedOn_Date_Only varchar(10) not null default '';
alter table TEMPORARY_Geology_Pit_Monitoring_Header add CreatedOn_Year_Only int not null default 0;

alter table TEMPORARY_Geology_Pit_Monitoring add Company varchar(32) not null default '';
alter table TEMPORARY_Geology_Pit_Monitoring add CreatedOn_Date_Only varchar(10) not null default '';
alter table TEMPORARY_Geology_Pit_Monitoring add CreatedOn_Year_Only int not null default 0;

alter table UPLOAD_Geology_Pit_Monitoring_Header add CreatedOn_Date_Only varchar(10) not null default '';
alter table UPLOAD_Geology_Pit_Monitoring_Header add CreatedOn_Year_Only int not null default 0;

--alter table UPLOAD_Geology_Pit_Monitoring add Company varchar(32) not null default '';
alter table UPLOAD_Geology_Pit_Monitoring add CreatedOn_Date_Only varchar(10) not null default '';
alter table UPLOAD_Geology_Pit_Monitoring add CreatedOn_Year_Only int not null default 0;

alter table TEMPORARY_Geology_Explorasi_Header add CreatedOn_Date_Only varchar(10) not null default '';
alter table TEMPORARY_Geology_Explorasi_Header add CreatedOn_Year_Only int not null default 0;

alter table TEMPORARY_Geology_Explorasi add Company varchar(32) not null default '';
alter table TEMPORARY_Geology_Explorasi add CreatedOn_Date_Only varchar(10) not null default '';
alter table TEMPORARY_Geology_Explorasi add CreatedOn_Year_Only int not null default 0;

alter table UPLOAD_Geology_Explorasi_Header add CreatedOn_Date_Only varchar(10) not null default '';
alter table UPLOAD_Geology_Explorasi_Header add CreatedOn_Year_Only int not null default 0;

--alter table UPLOAD_Geology_Explorasi add Company varchar(32) not null default '';
alter table UPLOAD_Geology_Explorasi add CreatedOn_Date_Only varchar(10) not null default '';
alter table UPLOAD_Geology_Explorasi add CreatedOn_Year_Only int not null default 0;

alter table TEMPORARY_BARGE_LOADING_Header add CreatedOn_Date_Only varchar(10) not null default '';
alter table TEMPORARY_BARGE_LOADING_Header add CreatedOn_Year_Only int not null default 0;

alter table TEMPORARY_BARGE_LOADING add Company varchar(32) not null default '';
alter table TEMPORARY_BARGE_LOADING add CreatedOn_Date_Only varchar(10) not null default '';
alter table TEMPORARY_BARGE_LOADING add CreatedOn_Year_Only int not null default 0;

alter table UPLOAD_BARGE_LOADING_Header add CreatedOn_Date_Only varchar(10) not null default '';
alter table UPLOAD_BARGE_LOADING_Header add CreatedOn_Year_Only int not null default 0;

--alter table UPLOAD_BARGE_LOADING add Company varchar(32) not null default '';
alter table UPLOAD_BARGE_LOADING add CreatedOn_Date_Only varchar(10) not null default '';
alter table UPLOAD_BARGE_LOADING add CreatedOn_Year_Only int not null default 0;

--alter table TEMPORARY_BARGE_LOADING_COA add Company varchar(32) not null default '';
alter table TEMPORARY_BARGE_LOADING_COA add CreatedOn_Date_Only varchar(10) not null default '';
alter table TEMPORARY_BARGE_LOADING_COA add CreatedOn_Year_Only int not null default 0;

--alter table UPLOAD_BARGE_LOADING_COA add Company varchar(32) not null default '';
alter table UPLOAD_BARGE_LOADING_COA add CreatedOn_Date_Only varchar(10) not null default '';
alter table UPLOAD_BARGE_LOADING_COA add CreatedOn_Year_Only int not null default 0;

alter table TEMPORARY_CRUSHING_PLANT_Header add CreatedOn_Date_Only varchar(10) not null default '';
alter table TEMPORARY_CRUSHING_PLANT_Header add CreatedOn_Year_Only int not null default 0;

alter table TEMPORARY_CRUSHING_PLANT add Company varchar(32) not null default '';
alter table TEMPORARY_CRUSHING_PLANT add CreatedOn_Date_Only varchar(10) not null default '';
alter table TEMPORARY_CRUSHING_PLANT add CreatedOn_Year_Only int not null default 0;

alter table UPLOAD_CRUSHING_PLANT_Header add CreatedOn_Date_Only varchar(10) not null default '';
alter table UPLOAD_CRUSHING_PLANT_Header add CreatedOn_Year_Only int not null default 0;

--alter table UPLOAD_CRUSHING_PLANT add Company varchar(32) not null default '';
alter table UPLOAD_CRUSHING_PLANT add CreatedOn_Date_Only varchar(10) not null default '';
alter table UPLOAD_CRUSHING_PLANT add CreatedOn_Year_Only int not null default 0;

alter table TEMPORARY_HAC_Header add CreatedOn_Date_Only varchar(10) not null default '';
alter table TEMPORARY_HAC_Header add CreatedOn_Year_Only int not null default 0;

alter table TEMPORARY_HAC add Company varchar(32) not null default '';
alter table TEMPORARY_HAC add CreatedOn_Date_Only varchar(10) not null default '';
alter table TEMPORARY_HAC add CreatedOn_Year_Only int not null default 0;

alter table UPLOAD_HAC_Header add CreatedOn_Date_Only varchar(10) not null default '';
alter table UPLOAD_HAC_Header add CreatedOn_Year_Only int not null default 0;

--alter table UPLOAD_HAC add Company varchar(32) not null default '';
alter table UPLOAD_HAC add CreatedOn_Date_Only varchar(10) not null default '';
alter table UPLOAD_HAC add CreatedOn_Year_Only int not null default 0;

go

update TEMPORARY_Sampling_ROM_Header set CreatedOn_Date_Only = Convert(varchar(10), CreatedOn, 120), CreatedOn_Year_Only = Year(CreatedOn);
update TEMPORARY_Sampling_ROM set CreatedOn_Date_Only = Convert(varchar(10), CreatedOn, 120), CreatedOn_Year_Only = Year(CreatedOn);
update UPLOAD_Sampling_ROM_Header set CreatedOn_Date_Only = Convert(varchar(10), CreatedOn, 120), CreatedOn_Year_Only = Year(CreatedOn);
update UPLOAD_Sampling_ROM set CreatedOn_Date_Only = Convert(varchar(10), CreatedOn, 120), CreatedOn_Year_Only = Year(CreatedOn);

update TEMPORARY_Geology_Pit_Monitoring_Header set CreatedOn_Date_Only = Convert(varchar(10), CreatedOn, 120), CreatedOn_Year_Only = Year(CreatedOn);
update TEMPORARY_Geology_Pit_Monitoring set CreatedOn_Date_Only = Convert(varchar(10), CreatedOn, 120), CreatedOn_Year_Only = Year(CreatedOn);
update UPLOAD_Geology_Pit_Monitoring_Header set CreatedOn_Date_Only = Convert(varchar(10), CreatedOn, 120), CreatedOn_Year_Only = Year(CreatedOn);
update UPLOAD_Geology_Pit_Monitoring set CreatedOn_Date_Only = Convert(varchar(10), CreatedOn, 120), CreatedOn_Year_Only = Year(CreatedOn);

update TEMPORARY_Geology_Explorasi_Header set CreatedOn_Date_Only = Convert(varchar(10), CreatedOn, 120), CreatedOn_Year_Only = Year(CreatedOn);
update TEMPORARY_Geology_Explorasi set CreatedOn_Date_Only = Convert(varchar(10), CreatedOn, 120), CreatedOn_Year_Only = Year(CreatedOn);
update UPLOAD_Geology_Explorasi_Header set CreatedOn_Date_Only = Convert(varchar(10), CreatedOn, 120), CreatedOn_Year_Only = Year(CreatedOn);
update UPLOAD_Geology_Explorasi set CreatedOn_Date_Only = Convert(varchar(10), CreatedOn, 120), CreatedOn_Year_Only = Year(CreatedOn);

update TEMPORARY_BARGE_LOADING_Header set CreatedOn_Date_Only = Convert(varchar(10), CreatedOn, 120), CreatedOn_Year_Only = Year(CreatedOn);
update TEMPORARY_BARGE_LOADING set CreatedOn_Date_Only = Convert(varchar(10), CreatedOn, 120), CreatedOn_Year_Only = Year(CreatedOn);
update UPLOAD_BARGE_LOADING_Header set CreatedOn_Date_Only = Convert(varchar(10), CreatedOn, 120), CreatedOn_Year_Only = Year(CreatedOn);
update UPLOAD_BARGE_LOADING set CreatedOn_Date_Only = Convert(varchar(10), CreatedOn, 120), CreatedOn_Year_Only = Year(CreatedOn);

update TEMPORARY_BARGE_LOADING_COA set CreatedOn_Date_Only = Convert(varchar(10), CreatedOn, 120), CreatedOn_Year_Only = Year(CreatedOn);
update UPLOAD_BARGE_LOADING_COA set CreatedOn_Date_Only = Convert(varchar(10), CreatedOn, 120), CreatedOn_Year_Only = Year(CreatedOn);

update TEMPORARY_CRUSHING_PLANT_Header set CreatedOn_Date_Only = Convert(varchar(10), CreatedOn, 120), CreatedOn_Year_Only = Year(CreatedOn);
update TEMPORARY_CRUSHING_PLANT set CreatedOn_Date_Only = Convert(varchar(10), CreatedOn, 120), CreatedOn_Year_Only = Year(CreatedOn);
update UPLOAD_CRUSHING_PLANT_Header set CreatedOn_Date_Only = Convert(varchar(10), CreatedOn, 120), CreatedOn_Year_Only = Year(CreatedOn);
update UPLOAD_CRUSHING_PLANT set CreatedOn_Date_Only = Convert(varchar(10), CreatedOn, 120), CreatedOn_Year_Only = Year(CreatedOn);

update TEMPORARY_HAC_Header set CreatedOn_Date_Only = Convert(varchar(10), CreatedOn, 120), CreatedOn_Year_Only = Year(CreatedOn);
update TEMPORARY_HAC set CreatedOn_Date_Only = Convert(varchar(10), CreatedOn, 120), CreatedOn_Year_Only = Year(CreatedOn);
update UPLOAD_HAC_Header set CreatedOn_Date_Only = Convert(varchar(10), CreatedOn, 120), CreatedOn_Year_Only = Year(CreatedOn);
update UPLOAD_HAC set CreatedOn_Date_Only = Convert(varchar(10), CreatedOn, 120), CreatedOn_Year_Only = Year(CreatedOn);

update TEMPORARY_Sampling_ROM set TEMPORARY_Sampling_ROM.company = h.Company
from TEMPORARY_Sampling_ROM t1 inner join TEMPORARY_Sampling_ROM_Header h on t1.header = h.RecordId;

update TEMPORARY_Geology_Pit_Monitoring set TEMPORARY_Geology_Pit_Monitoring.company = h.Company
from TEMPORARY_Geology_Pit_Monitoring t1 inner join TEMPORARY_Geology_Pit_Monitoring_Header h on t1.header = h.RecordId;

update TEMPORARY_Geology_Explorasi set TEMPORARY_Geology_Explorasi.company = h.Company
from TEMPORARY_Geology_Explorasi t1 inner join TEMPORARY_Geology_Explorasi_Header h on t1.header = h.RecordId;

update TEMPORARY_BARGE_LOADING set TEMPORARY_BARGE_LOADING.company = h.Company
from TEMPORARY_BARGE_LOADING t1 inner join TEMPORARY_BARGE_LOADING_Header h on t1.header = h.RecordId;

update TEMPORARY_CRUSHING_PLANT set TEMPORARY_CRUSHING_PLANT.company = h.Company
from TEMPORARY_CRUSHING_PLANT t1 inner join TEMPORARY_CRUSHING_PLANT_Header h on t1.header = h.RecordId;

update TEMPORARY_HAC set TEMPORARY_HAC.company = h.Company
from TEMPORARY_HAC t1 inner join TEMPORARY_HAC_Header h on t1.header = h.RecordId;

/*

-- drop unique Key dari Table UPLOAD_BARGE_LOADING
-- drop unique Key dari Table UPLOAD_CRUSHING_PLANT
-- drop unique Key dari Table UPLOAD_Geology_Explorasi
-- drop unique Key dari Table UPLOAD_Geology_Pit_Monitoring
-- drop unique Key dari Table UPLOAD_Sampling_ROM
-- drop unique Key dari Table UPLOAD_HAC

*/

alter table TEMPORARY_BARGE_LOADING add Sheet varchar(32) not null default '';
alter table TEMPORARY_CRUSHING_PLANT add Sheet varchar(32) not null default '';
alter table TEMPORARY_Geology_Explorasi add Sheet varchar(32) not null default '';
alter table TEMPORARY_Geology_Pit_Monitoring add Sheet varchar(32) not null default '';
alter table TEMPORARY_Sampling_ROM add Sheet varchar(32) not null default '';
alter table TEMPORARY_HAC add Sheet varchar(32) not null default '';

go

update TEMPORARY_BARGE_LOADING set TEMPORARY_BARGE_LOADING.Sheet = h.Sheet
from TEMPORARY_BARGE_LOADING t1 inner join TEMPORARY_BARGE_LOADING_Header h on t1.header = h.RecordId;

update TEMPORARY_CRUSHING_PLANT set TEMPORARY_CRUSHING_PLANT.Sheet = h.Sheet
from TEMPORARY_CRUSHING_PLANT t1 inner join TEMPORARY_CRUSHING_PLANT_Header h on t1.header = h.RecordId;

update TEMPORARY_Geology_Explorasi set TEMPORARY_Geology_Explorasi.Sheet = h.Sheet
from TEMPORARY_Geology_Explorasi t1 inner join TEMPORARY_Geology_Explorasi_Header h on t1.header = h.RecordId;

update TEMPORARY_Geology_Pit_Monitoring set TEMPORARY_Geology_Pit_Monitoring.Sheet = h.Sheet
from TEMPORARY_Geology_Pit_Monitoring t1 inner join TEMPORARY_Geology_Pit_Monitoring_Header h on t1.header = h.RecordId;

update TEMPORARY_Sampling_ROM set TEMPORARY_Sampling_ROM.Sheet = h.Sheet
from TEMPORARY_Sampling_ROM t1 inner join TEMPORARY_Sampling_ROM_Header h on t1.header = h.RecordId;

update TEMPORARY_HAC set TEMPORARY_HAC.Sheet = h.Sheet
from TEMPORARY_HAC t1 inner join TEMPORARY_HAC_Header h on t1.header = h.RecordId;

/*

-- drop unique Key dari Table SamplingRequest_Lab
-- note:
--     tidak ada unique Key pada Table AnalysisRequest_Detail

*/

alter table SamplingRequest_Lab add Company varchar(32) not null default '';
alter table SamplingRequest_Lab add CreatedOn_Date_Only varchar(10) not null default '';
alter table SamplingRequest_Lab add CreatedOn_Year_Only int not null default 0;

alter table AnalysisRequest_Detail add Company varchar(32) not null default '';
alter table AnalysisRequest_Detail add CreatedOn_Date_Only varchar(10) not null default '';
alter table AnalysisRequest_Detail add CreatedOn_Year_Only int not null default 0;

go

update SamplingRequest_Lab set CreatedOn_Date_Only = Convert(varchar(10), CreatedOn, 120), CreatedOn_Year_Only = Year(CreatedOn);
update SamplingRequest_Lab set CreatedOn_Date_Only = Convert(varchar(10), CreatedOn, 120), CreatedOn_Year_Only = Year(CreatedOn);
update AnalysisRequest_Detail set CreatedOn_Date_Only = Convert(varchar(10), CreatedOn, 120), CreatedOn_Year_Only = Year(CreatedOn);
update AnalysisRequest_Detail set CreatedOn_Date_Only = Convert(varchar(10), CreatedOn, 120), CreatedOn_Year_Only = Year(CreatedOn);

go

update SamplingRequest_Lab set SamplingRequest_Lab.company = s.Company
from SamplingRequest_Lab t1 inner join SamplingRequest s on t1.SamplingRequest = s.SamplingRequestId;

update AnalysisRequest_Detail set AnalysisRequest_Detail.company = a.Company
from AnalysisRequest_Detail t1 inner join AnalysisRequest a on t1.AnalysisRequest = a.AnalysisRequestId;

go

alter table UPLOAD_Sampling_ROM add LECO_Stamp varchar(255) not null default '';
alter table UPLOAD_Geology_Pit_Monitoring add LECO_Stamp varchar(255) not null default '';
alter table UPLOAD_Geology_Explorasi add LECO_Stamp varchar(255) not null default '';
alter table UPLOAD_BARGE_LOADING add LECO_Stamp varchar(255) not null default '';
alter table UPLOAD_CRUSHING_PLANT add LECO_Stamp varchar(255) not null default '';

go

update UPLOAD_Sampling_ROM set LECO_Stamp = '';
update UPLOAD_Geology_Pit_Monitoring set LECO_Stamp = '';
update UPLOAD_Geology_Explorasi set LECO_Stamp = '';
update UPLOAD_BARGE_LOADING set LECO_Stamp = '';
update UPLOAD_CRUSHING_PLANT set LECO_Stamp = '';

go

alter table Loading_Plan_Detail_Barge_Blending_Formula add TunnelID int not null default 0;
go

alter table Loading_Request_Loading_Plan_Detail_Barge_Blending_Formula add TunnelID int not null default 0;
go