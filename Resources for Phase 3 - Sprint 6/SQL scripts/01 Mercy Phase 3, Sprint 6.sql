alter table Tunnel add ProductId int NOT NULL DEFAULT 0;
/*
update Tunnel set ProductId = (select Min(ProductId) from Product where CompanyCode = 'BEK')
where CompanyCode = 'BEK';

update Tunnel set ProductId = (select Min(ProductId) from Product where CompanyCode = 'TCM')
where CompanyCode = 'TCM';

*/

ALTER TABLE Tunnel ADD UNIQUE (Name);

create table Loading_Plan
(
    RecordId BIGINT NOT NULL IDENTITY(1,1)
    , SiteId int not null
    , Company varchar(32) NOT NULL DEFAULT ''
    , Shipment_Type varchar(32) NOT NULL DEFAULT ''
    , Buyer varchar(255) NOT NULL DEFAULT ''
    , Vessel varchar(255) NOT NULL DEFAULT ''
    , Destination varchar(255) NOT NULL DEFAULT ''
    , Trip_No varchar(255) NOT NULL DEFAULT ''
    , Remark VARCHAR(255) NOT NULL DEFAULT ''
    
    , CreatedBy INT NOT NULL DEFAULT 0
    , CreatedOn datetime NOT NULL DEFAULT GetDate()
    , CreatedOn_Date_Only varchar(10) not null default ''
    
    , LastModifiedBy INT NOT NULL DEFAULT 0
    , LastModifiedOn datetime NULL
    , CONSTRAINT PK_Loading_Plan_RecordId PRIMARY KEY (RecordId)
    , CONSTRAINT FK_Loading_Plan_Site FOREIGN KEY (SiteId) REFERENCES Site (SiteId) 
);

create table Loading_Plan_Detail_Barge
(
    RecordId BIGINT NOT NULL IDENTITY(1,1)
    , Loading_Plan_Id BIGINT NOT NULL DEFAULT 0
    
    , Tug varchar(255) NOT NULL DEFAULT ''
    , Barge varchar(255) NOT NULL DEFAULT ''
    , Trip_ID varchar(255) NOT NULL DEFAULT ''
    , EstimateStartLoading datetime NOT NULL
    , Quantity INT NOT NULL DEFAULT 0
    , Product varchar(255) NOT NULL DEFAULT ''
    
    , CreatedBy INT NOT NULL DEFAULT 0
    , CreatedOn datetime NOT NULL DEFAULT GetDate()
    , CreatedOn_Date_Only varchar(10) not null default ''
    
    , LastModifiedBy INT NOT NULL DEFAULT 0
    , LastModifiedOn datetime NULL
    , CONSTRAINT PK_Loading_Plan_Detail_Barge_RecordId PRIMARY KEY (RecordId)
    , CONSTRAINT FK_Loading_Plan_Detail_Barge_Loading_Plan_Id FOREIGN KEY (Loading_Plan_Id) REFERENCES Loading_Plan (RecordId) 
);

create table Loading_Plan_Detail_Barge_Quality
(
    RecordId BIGINT NOT NULL IDENTITY(1,1)
    , Loading_Plan_Id BIGINT NOT NULL DEFAULT 0
    , Detail_Barge_Id BIGINT NOT NULL DEFAULT 0
    
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
    
    , CONSTRAINT PK_Loading_Plan_Detail_Barge_Quality_RecordId PRIMARY KEY (RecordId)
    , CONSTRAINT CO_Loading_Plan_Detail_Barge_Quality_UNIQUE UNIQUE(Detail_Barge_Id)
);

create table Loading_Plan_Detail_Barge_Blending_Formula
(
    RecordId BIGINT NOT NULL IDENTITY(1,1)
    , Loading_Plan_Id BIGINT NOT NULL DEFAULT 0
    , Detail_Barge_Id BIGINT NOT NULL DEFAULT 0
    
    , Tunnel varchar(255) NOT NULL DEFAULT ''
    , Stock INT NOT NULL DEFAULT 0
    , CV NUMERIC(10,2) NOT NULL DEFAULT 0
    , TS NUMERIC(10,2) NOT NULL DEFAULT 0
    , ASH NUMERIC(10,2) NOT NULL DEFAULT 0
    , IM NUMERIC(10,2) NOT NULL DEFAULT 0
    , TM NUMERIC(10,2) NOT NULL DEFAULT 0
    , Portion INT NOT NULL DEFAULT 0
    
    , CreatedBy INT NOT NULL DEFAULT 0
    , CreatedOn datetime NOT NULL DEFAULT GetDate()
    
    , CONSTRAINT PK_Loading_Plan_Detail_Barge_Blending_Formula_RecordId PRIMARY KEY (RecordId)
);

create table dbo.Loading_Request(
    RecordId        bigint identity(1,1)
    , CreatedBy     INT NOT NULL DEFAULT 0
    , CreatedOn     datetime NOT NULL DEFAULT GetDate()
    , CreatedOn_Date_Only varchar(10) not null default ''
    , LastModifiedBy INT NOT NULL DEFAULT 0
    , LastModifiedOn datetime NULL
    , DeletedBy     INT NOT NULL DEFAULT 0
    , DeletedOn     datetime NULL
    , CONSTRAINT PK_Loading_Request_RecordId PRIMARY KEY (RecordId)
);

create table Loading_Request_Loading_Plan
(
    RecordId BIGINT NOT NULL IDENTITY(1,1)
    , Request_Id BIGINT NOT NULL DEFAULT 0
    , Loading_Plan_Id BIGINT NOT NULL DEFAULT 0
    
    , SiteId int not null
    , Company varchar(32) NOT NULL DEFAULT ''
    , Shipment_Type varchar(32) NOT NULL DEFAULT ''
    , Buyer varchar(255) NOT NULL DEFAULT ''
    , Vessel varchar(255) NOT NULL DEFAULT ''
    , Destination varchar(255) NOT NULL DEFAULT ''
    , Trip_No varchar(255) NOT NULL DEFAULT ''
    , Remark VARCHAR(255) NOT NULL DEFAULT ''
    
    , CreatedBy INT NOT NULL DEFAULT 0
    , CreatedOn datetime NOT NULL DEFAULT GetDate()
    , CreatedOn_Date_Only varchar(10) not null default ''
    
    , LastModifiedBy INT NOT NULL DEFAULT 0
    , LastModifiedOn datetime NULL
    , CONSTRAINT PK_Loading_Request_Loading_Plan_RecordId PRIMARY KEY (RecordId)
    , CONSTRAINT FK_Loading_Request_Loading_Plan_Request_Id FOREIGN KEY (Request_Id) REFERENCES Loading_Request (RecordId) 
);

create table Loading_Request_Loading_Plan_Detail_Barge
(
    RecordId BIGINT NOT NULL IDENTITY(1,1)
    , Request_Id BIGINT NOT NULL DEFAULT 0
    , Loading_Plan_Id BIGINT NOT NULL DEFAULT 0
    
    , Tug varchar(255) NOT NULL DEFAULT ''
    , Barge varchar(255) NOT NULL DEFAULT ''
    , Trip_ID varchar(255) NOT NULL DEFAULT ''
    , EstimateStartLoading datetime NOT NULL
    , Quantity INT NOT NULL DEFAULT 0
    , Product varchar(255) NOT NULL DEFAULT ''
    
    , CreatedBy INT NOT NULL DEFAULT 0
    , CreatedOn datetime NOT NULL DEFAULT GetDate()
    , CreatedOn_Date_Only varchar(10) not null default ''
    
    , LastModifiedBy INT NOT NULL DEFAULT 0
    , LastModifiedOn datetime NULL
    , CONSTRAINT PK_Loading_Request_Loading_Plan_Detail_Barge_RecordId PRIMARY KEY (RecordId)
);

create table Loading_Request_Loading_Plan_Detail_Barge_Quality
(
    RecordId BIGINT NOT NULL IDENTITY(1,1)
    , Request_Id BIGINT NOT NULL DEFAULT 0
    , Loading_Plan_Id BIGINT NOT NULL DEFAULT 0
    , Detail_Barge_Id BIGINT NOT NULL DEFAULT 0
    
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
    
    , CONSTRAINT PK_Loading_Request_Loading_Plan_Detail_Barge_Quality_RecordId PRIMARY KEY (RecordId)
);

create table Loading_Request_Loading_Plan_Detail_Barge_Blending_Formula
(
    RecordId BIGINT NOT NULL IDENTITY(1,1)
    , Request_Id BIGINT NOT NULL DEFAULT 0
    , Loading_Plan_Id BIGINT NOT NULL DEFAULT 0
    , Detail_Barge_Id BIGINT NOT NULL DEFAULT 0
    
    , Tunnel varchar(255) NOT NULL DEFAULT ''
    , Stock INT NOT NULL DEFAULT 0
    , CV NUMERIC(10,2) NOT NULL DEFAULT 0
    , TS NUMERIC(10,2) NOT NULL DEFAULT 0
    , ASH NUMERIC(10,2) NOT NULL DEFAULT 0
    , IM NUMERIC(10,2) NOT NULL DEFAULT 0
    , TM NUMERIC(10,2) NOT NULL DEFAULT 0
    , Portion INT NOT NULL DEFAULT 0
    
    , CreatedBy INT NOT NULL DEFAULT 0
    , CreatedOn datetime NOT NULL DEFAULT GetDate()
    
    , CONSTRAINT PK_Loading_Request_Loading_Plan_Detail_Barge_Blending_Formula_RecordId PRIMARY KEY (RecordId)
);