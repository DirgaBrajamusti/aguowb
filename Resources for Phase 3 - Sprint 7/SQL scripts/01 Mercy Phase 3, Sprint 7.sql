create table Loading_Actual
(
    RecordId bigint identity(1,1)
    , TugName varchar(255) not null default ''
    , RequestId int NOT NULL DEFAULT 0
    , No_Ref_Report varchar(255) not null default ''
    , SiteId int NOT NULL DEFAULT 0
    , No_Services_Trip varchar(255) not null default ''
    , Barge_Name varchar(255) not null default ''
    , Barge_Size int NOT NULL DEFAULT 0
    , Route varchar(255) not null default ''
    , Load_Type varchar(255) not null default ''
    
    , Arrival_Time datetime not NULL
    , Initial_Draft datetime not NULL
    , Anchor_Up datetime not NULL
    , Berthed_Time datetime not NULL
    , Commenced_Loading datetime not NULL
    , Completed_Loading datetime not NULL
    , Unberthing datetime not NULL
    , Departure datetime not NULL

    , Coal_Quality_Spec varchar(255) not null default ''
    , Delay_Cause_of_Barge_Changing varchar(255) not null default ''
    , Surveyor_Name varchar(255) not null default ''
    , Weather_Condition varchar(255) not null default ''
    
    , Water_Level_During_Loading NUMERIC(10,2) NOT NULL DEFAULT 0
    , Daily_Water_Level NUMERIC(10,2) NOT NULL DEFAULT 0
    , Water_Level_at_Jetty NUMERIC(10,2) NOT NULL DEFAULT 0
    
    , Status varchar(255) not null default ''
    
    , CreatedBy INT NOT NULL DEFAULT 0
    , CreatedOn datetime NOT NULL DEFAULT GetDate()
    , LastModifiedBy INT NOT NULL DEFAULT 0
    , LastModifiedOn datetime NULL
    , CONSTRAINT PK_Loading_Actual_RecordId PRIMARY KEY (RecordId)
);

create table Fuel_Rob
(
    RecordId int identity(1,1)
    , Fuel_Rob_Name varchar(255) not null default ''
    , CONSTRAINT PK_Fuel_Rob_RecordId PRIMARY KEY (RecordId)
);

create table Draft_Survey
(
    RecordId int identity(1,1)
    , Draft_Survey_Name varchar(255) not null default ''
    , CONSTRAINT PK_Draft_Survey_RecordId PRIMARY KEY (RecordId)
);

create table Barge_Survey_Condition
(
    RecordId int identity(1,1)
    , Barge_Survey_Condition_Name varchar(255) not null default ''
    , CONSTRAINT PK_Barge_Survey_Condition_RecordId PRIMARY KEY (RecordId)
);

create table Loading_Actual_Cargo_Loaded
(
    RecordId int identity(1,1)
    , ActualId bigint not null
    , TunnelId int not null
    , Belt NUMERIC(10,2) NOT NULL DEFAULT 0
    , Draft NUMERIC(10,2) NOT NULL DEFAULT 0
    
    , CreatedBy INT NOT NULL DEFAULT 0
    , CreatedOn datetime NOT NULL DEFAULT GetDate()
    , LastModifiedBy INT NOT NULL DEFAULT 0
    , LastModifiedOn datetime NULL
    , CONSTRAINT PK_Loading_Actual_Cargo_Loaded_RecordId PRIMARY KEY (RecordId)
    , CONSTRAINT FK_Loading_Actual_Cargo_Loaded_ActualId FOREIGN KEY (ActualId) REFERENCES Loading_Actual(RecordId)
    , CONSTRAINT FK_Loading_Actual_Cargo_Loaded_TunnelId FOREIGN KEY (TunnelId) REFERENCES Tunnel(TunnelId)
);

create table Loading_Actual_Fuel_Consumption
(
    RecordId int identity(1,1)
    , ActualId bigint not null
    , Fuel_Rob_Id int not null
    , Total_Consumption int NOT NULL DEFAULT 0
    
    , CreatedBy INT NOT NULL DEFAULT 0
    , CreatedOn datetime NOT NULL DEFAULT GetDate()
    , LastModifiedBy INT NOT NULL DEFAULT 0
    , LastModifiedOn datetime NULL
    , CONSTRAINT PK_Loading_Actual_Fuel_Consumption_RecordId PRIMARY KEY (RecordId)
    , CONSTRAINT FK_Loading_Actual_Fuel_Consumption_ActualId FOREIGN KEY (ActualId) REFERENCES Loading_Actual(RecordId)
    , CONSTRAINT FK_Loading_Actual_Fuel_Consumption_Fuel_Rob_Id FOREIGN KEY (Fuel_Rob_Id) REFERENCES Fuel_Rob(RecordId)
);

create table Loading_Actual_Draft_Survey
(
    RecordId int identity(1,1)
    , ActualId bigint not null
    , Draft_Survey_Id int not null
    , [Initial] NUMERIC(10,3) NOT NULL DEFAULT 0
    , Final NUMERIC(10,3) NOT NULL DEFAULT 0
    
    , CreatedBy INT NOT NULL DEFAULT 0
    , CreatedOn datetime NOT NULL DEFAULT GetDate()
    , LastModifiedBy INT NOT NULL DEFAULT 0
    , LastModifiedOn datetime NULL
    , CONSTRAINT PK_Loading_Actual_Draft_Survey_RecordId PRIMARY KEY (RecordId)
    , CONSTRAINT FK_Loading_Actual_Draft_Survey_ActualId FOREIGN KEY (ActualId) REFERENCES Loading_Actual(RecordId)
    , CONSTRAINT FK_Loading_Actual_Draft_Survey_Draft_Survey_Id FOREIGN KEY (Draft_Survey_Id) REFERENCES Draft_Survey(RecordId)
);

create table Loading_Actual_Barge_Survey_Condition
(
    RecordId int identity(1,1)
    , ActualId bigint not null
    , Barge_Survey_Condition_Id int not null
    , Condition varchar(255) not null default ''
    , Remark varchar(255) not null default ''
    
    , CreatedBy INT NOT NULL DEFAULT 0
    , CreatedOn datetime NOT NULL DEFAULT GetDate()
    , LastModifiedBy INT NOT NULL DEFAULT 0
    , LastModifiedOn datetime NULL
    , CONSTRAINT PK_Loading_Actual_Barge_Survey_Condition_RecordId PRIMARY KEY (RecordId)
    , CONSTRAINT FK_Loading_Actual_Barge_Survey_Condition_ActualId FOREIGN KEY (ActualId) REFERENCES Loading_Actual(RecordId)
    , CONSTRAINT FK_Loading_Actual_Barge_Survey_Condition_Barge_Survey_Condition_Id FOREIGN KEY (Barge_Survey_Condition_Id) REFERENCES Barge_Survey_Condition(RecordId)
);

create table Loading_Actual_Coal_Temperature
(
    RecordId int identity(1,1)
    , ActualId bigint not null
    , Temperature NUMERIC(10,2) NOT NULL DEFAULT 0
    
    , CreatedBy INT NOT NULL DEFAULT 0
    , CreatedOn datetime NOT NULL DEFAULT GetDate()
    , LastModifiedBy INT NOT NULL DEFAULT 0
    , LastModifiedOn datetime NULL
    , CONSTRAINT PK_Loading_Actual_Coal_Temperature_RecordId PRIMARY KEY (RecordId)
    , CONSTRAINT FK_Loading_Actual_Coal_Temperature_ActualId FOREIGN KEY (ActualId) REFERENCES Loading_Actual(RecordId)
);

create table Loading_Actual_Attachments
(
    RecordId int identity(1,1)
    , ActualId bigint not null
    , FileName varchar(255) not null default ''
    , FileName2 varchar(255) not null default ''
    
    , CreatedBy INT NOT NULL DEFAULT 0
    , CreatedOn datetime NOT NULL DEFAULT GetDate()
    , LastModifiedBy INT NOT NULL DEFAULT 0
    , LastModifiedOn datetime NULL
    , CONSTRAINT PK_Loading_Actual_Attachments_RecordId PRIMARY KEY (RecordId)
    , CONSTRAINT FK_Loading_Actual_Attachments_ActualId FOREIGN KEY (ActualId) REFERENCES Loading_Actual(RecordId)
);

CREATE TABLE Port_Loading
(
    Record_Id INT NOT NULL IDENTITY(1,1)
    , CompanyCode VARCHAR(32) NOT NULL
    , Periode varchar(10) not null default ''
    , [Type] varchar(10) not null default 'budget'
    , ProductId int not null default 0
    , ProductName VARCHAR(255) NOT NULL
    , Tonnage float NOT NULL DEFAULT 0
    , CV float NOT NULL DEFAULT 0
    , TS NUMERIC(10,2) NOT NULL DEFAULT 0
    , ASH NUMERIC(10,2) NOT NULL DEFAULT 0
    , IM NUMERIC(10,2) NOT NULL DEFAULT 0
    , TM NUMERIC(10,2) NOT NULL DEFAULT 0
    , CreatedBy INT NOT NULL DEFAULT 0
    , CreatedOn datetime NOT NULL DEFAULT GetDate()
    , LastModifiedBy INT NOT NULL DEFAULT 0
    , LastModifiedOn datetime NULL
    , IsActive bit NOT NULL DEFAULT 0
    , CONSTRAINT PK_Port_Loading_Record_Id PRIMARY KEY (Record_Id)
    , CONSTRAINT FK_Port_Loading_CompanyCode FOREIGN KEY (CompanyCode) REFERENCES Company(CompanyCode)
);