create table TEMPORARY_HAC_Header
(
    RecordId BIGINT NOT NULL IDENTITY(1,1)
    , File_Physical BIGINT NOT NULL UNIQUE
    , Company varchar(32) not null default ''
    , Sheet varchar(32) not null default ''
    , CreatedBy INT NOT NULL DEFAULT 0
    , CreatedOn datetime NOT NULL DEFAULT GetDate()
    , Date_Detail VARCHAR(255) NOT NULL DEFAULT ''
    , Job_No VARCHAR(255) NOT NULL DEFAULT ''
    , Report_To VARCHAR(255) NOT NULL DEFAULT ''
    , Method1 VARCHAR(255) NOT NULL DEFAULT ''
    , Method2 VARCHAR(255) NOT NULL DEFAULT ''
    , Method3 VARCHAR(255) NOT NULL DEFAULT ''
    , Method4 VARCHAR(255) NOT NULL DEFAULT ''
    , CONSTRAINT PK_TEMPORARY_HAC_Header_RecordId PRIMARY KEY (RecordId)
    , CONSTRAINT FK_TEMPORARY_HAC_Header_File_Physical FOREIGN KEY (File_Physical) REFERENCES TEMPORARY_File (RecordId)
);

create table TEMPORARY_HAC
(
    RecordId BIGINT NOT NULL IDENTITY(1,1)
    , Header BIGINT NOT NULL DEFAULT 0
    , Status VARCHAR(255) NOT NULL DEFAULT 'New'
    , CreatedBy INT NOT NULL DEFAULT 0
    , CreatedOn datetime NOT NULL DEFAULT GetDate()
    , Date_Sampling VARCHAR(255) NOT NULL DEFAULT ''
    , Day_work VARCHAR(255) NOT NULL DEFAULT ''
    , Tonnage  VARCHAR(255) NOT NULL DEFAULT ''
    , LOT  VARCHAR(255) NOT NULL DEFAULT ''
    , Lab_ID  VARCHAR(255) NOT NULL DEFAULT ''
    , TM varchar(255) not null default ''
    , M varchar(255) not null default ''
    , ASH varchar(255) not null default ''
    , TS varchar(255) not null default ''
    , CV varchar(255) not null default ''
    , Remark VARCHAR(255) NOT NULL DEFAULT ''
    
    , Date_Sampling_isvalid bit NOT NULL DEFAULT 0
    , Day_work_isvalid bit NOT NULL DEFAULT 0
    , Tonnage_isvalid bit NOT NULL DEFAULT 0
    , LOT_isvalid bit NOT NULL DEFAULT 0
    , Lab_ID_isvalid bit NOT NULL DEFAULT 0
    , TM_isvalid bit NOT NULL DEFAULT 0
    , M_isvalid bit NOT NULL DEFAULT 0
    , ASH_isvalid bit NOT NULL DEFAULT 0
    , TS_isvalid bit NOT NULL DEFAULT 0
    , CV_isvalid bit NOT NULL DEFAULT 0
    , Remark_isvalid bit NOT NULL DEFAULT 0
    
    , CONSTRAINT PK_TEMPORARY_HAC_RecordId PRIMARY KEY (RecordId)
    , CONSTRAINT FK_TEMPORARY_HAC_Header FOREIGN KEY (Header) REFERENCES TEMPORARY_HAC_Header (RecordId)
);

create table UPLOAD_HAC_Header
(
    RecordId BIGINT NOT NULL IDENTITY(1,1)
    , File_Physical BIGINT NOT NULL DEFAULT 0
    , [TEMPORARY] BIGINT NOT NULL DEFAULT 0
    , Company varchar(32) not null default ''
    , Sheet varchar(32) not null default ''
    , CreatedBy INT NOT NULL DEFAULT 0
    , CreatedOn datetime NOT NULL DEFAULT GetDate()
    , Date_Detail VARCHAR(255) NOT NULL DEFAULT ''
    , Job_No VARCHAR(255) NOT NULL DEFAULT ''
    , Report_To VARCHAR(255) NOT NULL DEFAULT ''
    , Method1 VARCHAR(255) NOT NULL DEFAULT ''
    , Method2 VARCHAR(255) NOT NULL DEFAULT ''
    , Method3 VARCHAR(255) NOT NULL DEFAULT ''
    , Method4 VARCHAR(255) NOT NULL DEFAULT ''
    , CONSTRAINT PK_UPLOAD_HAC_Header_RecordId PRIMARY KEY (RecordId)
);

create table UPLOAD_HAC
(
    RecordId BIGINT NOT NULL IDENTITY(1,1)
    , [TEMPORARY] BIGINT NOT NULL DEFAULT 0
    , Header BIGINT NOT NULL DEFAULT 0
    , Company varchar(32) not null default ''
    , Sheet varchar(32) not null default ''
    , CreatedBy INT NOT NULL DEFAULT 0
    , CreatedOn datetime NOT NULL DEFAULT GetDate()
    , LastModifiedBy INT NOT NULL DEFAULT 0
    , LastModifiedOn datetime NULL
    , Date_Sampling VARCHAR(255) NOT NULL DEFAULT ''
    , Day_work VARCHAR(255) NOT NULL DEFAULT ''
    , Tonnage NUMERIC(10,3) NOT NULL DEFAULT 0
    , LOT  VARCHAR(255) NOT NULL DEFAULT ''
    , Lab_ID  VARCHAR(255) NOT NULL UNIQUE
    , TM NUMERIC(10,2) NOT NULL DEFAULT 0
    , M NUMERIC(10,2) NOT NULL DEFAULT 0
    , ASH NUMERIC(10,2) NOT NULL DEFAULT 0
    , TS NUMERIC(10,2) NOT NULL DEFAULT 0
    , CV INT NOT NULL DEFAULT 0
    , Remark VARCHAR(255) NOT NULL DEFAULT ''
    , CONSTRAINT PK_UPLOAD_HAC_RecordId PRIMARY KEY (RecordId)
);

create table TEMPORARY_VLU
(
    RecordId BIGINT NOT NULL IDENTITY(1,1)
    , File_Physical BIGINT NOT NULL
    , Site INT NOT NULL DEFAULT 0
    , Sheet VARCHAR(255) NOT NULL DEFAULT ''
    , Status_Upload VARCHAR(255) NOT NULL DEFAULT 'New'
    , [Month] VARCHAR(255) NOT NULL DEFAULT ''
    , Shipper VARCHAR(255) NOT NULL DEFAULT ''
    , [NO] VARCHAR(255) NOT NULL DEFAULT ''
    , VESSEL VARCHAR(255) NOT NULL DEFAULT ''
    , LAYCAN_From VARCHAR(255) NOT NULL DEFAULT ''
    , LAYCAN_To VARCHAR(255) NOT NULL DEFAULT ''
    , DESTINATION VARCHAR(255) NOT NULL DEFAULT ''
    , BUYER VARCHAR(255) NOT NULL DEFAULT ''
    , ETA VARCHAR(255) NOT NULL DEFAULT ''
    , ATA VARCHAR(255) NOT NULL DEFAULT ''
    , Commenced_Loading VARCHAR(255) NOT NULL DEFAULT ''
    , Completed_Loading VARCHAR(255) NOT NULL DEFAULT ''
    , PORT VARCHAR(255) NOT NULL DEFAULT ''
    , DEM VARCHAR(255) NOT NULL DEFAULT ''
    , STATUS VARCHAR(255) NOT NULL DEFAULT ''
    
    , Month_isvalid bit NOT NULL DEFAULT 0
    , Shipper_isvalid bit NOT NULL DEFAULT 0
    , NO_isvalid bit NOT NULL DEFAULT 0
    , VESSEL_isvalid bit NOT NULL DEFAULT 0
    , LAYCAN_isvalid bit NOT NULL DEFAULT 0
    , LAYCAN_2_isvalid bit NOT NULL DEFAULT 0
    , DESTINATION_isvalid bit NOT NULL DEFAULT 0
    , BUYER_isvalid bit NOT NULL DEFAULT 0
    , ETA_isvalid bit NOT NULL DEFAULT 0
    , ATA_isvalid bit NOT NULL DEFAULT 0
    , Commenced_Loading_isvalid bit NOT NULL DEFAULT 0
    , Completed_Loading_isvalid bit NOT NULL DEFAULT 0
    , PORT_isvalid bit NOT NULL DEFAULT 0
    , DEM_isvalid bit NOT NULL DEFAULT 0
    , STATUS_isvalid bit NOT NULL DEFAULT 0
    
    , CreatedBy INT NOT NULL DEFAULT 0
    , CreatedOn datetime NOT NULL DEFAULT GetDate()
    , CreatedOn_Date_Only varchar(10) not null default ''
    
    , CONSTRAINT PK_TEMPORARY_VLU_RecordId PRIMARY KEY (RecordId)
    , CONSTRAINT FK_TEMPORARY_VLU_File_Physical FOREIGN KEY (File_Physical) REFERENCES TEMPORARY_File (RecordId)
);

create table UPLOAD_VLU
(
    RecordId BIGINT NOT NULL IDENTITY(1,1)
    , [TEMPORARY] BIGINT NOT NULL DEFAULT 0
    , Site INT NOT NULL DEFAULT 0
    , Sheet VARCHAR(255) NOT NULL DEFAULT ''
    , [Month] VARCHAR(255) NOT NULL DEFAULT ''
    , Shipper VARCHAR(255) NOT NULL DEFAULT ''
    , [NO] VARCHAR(255) NOT NULL DEFAULT ''
    , VESSEL VARCHAR(255) NOT NULL DEFAULT ''
    , LAYCAN_From datetime not null
    , LAYCAN_To datetime not null
    , DESTINATION VARCHAR(255) NOT NULL DEFAULT ''
    , BUYER VARCHAR(255) NOT NULL DEFAULT ''
    , ETA datetime not null
    , ATA datetime not null
    , Commenced_Loading datetime not null
    , Completed_Loading datetime not null
    , PORT VARCHAR(255) NOT NULL DEFAULT ''
    , DEM NUMERIC(10,0) NOT NULL DEFAULT 0
    , STATUS VARCHAR(255) NOT NULL DEFAULT ''
    
    , CreatedBy INT NOT NULL DEFAULT 0
    , CreatedOn datetime NOT NULL DEFAULT GetDate()
    , CreatedOn_Date_Only varchar(10) not null default ''
    , LastModifiedBy INT NOT NULL DEFAULT 0
    , LastModifiedOn datetime NULL
    
    , CONSTRAINT PK_UPLOAD_VLU_RecordId PRIMARY KEY (RecordId)
    , CONSTRAINT CO_UPLOAD_VLU_UNIQUE UNIQUE(CreatedOn_Date_Only, VESSEL, LAYCAN_From, LAYCAN_To)
);

alter table ROM add Ton int not null default 0;
go
update ROM set Ton = 0;
go
