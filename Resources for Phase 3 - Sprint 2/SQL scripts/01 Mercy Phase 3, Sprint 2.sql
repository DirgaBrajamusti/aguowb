drop table User_Role;
drop table Api_Role;
drop table Menu_Role;
drop table [Role];
drop table API;
drop table Menu;
go

CREATE TABLE Menu
(
    MenuId INT NOT NULL IDENTITY(1,1),
    MenuName VARCHAR(255) NOT NULL UNIQUE,
    Description VARCHAR(255) NOT NULL,
    Url VARCHAR(255) NOT NULL UNIQUE,
    Logo VARCHAR(255) NOT NULL,
    ParentId integer NOT NULL DEFAULT 0,
    IsActive bit NOT NULL DEFAULT 1,
    CreatedBy INT NOT NULL DEFAULT 0,
    CreatedOn datetime NOT NULL DEFAULT GetDate(),
    LastModifiedBy INT NOT NULL DEFAULT 0,
    LastModifiedOn datetime NULL,
    Ordering integer DEFAULT 0,
    [Level] integer DEFAULT 0,
    CONSTRAINT PK_Menu_MenuId PRIMARY KEY (MenuId)
);

CREATE TABLE Apii
(
    ApiId INT NOT NULL IDENTITY(1,1),
    MenuId integer NOT NULL,
    ApiName VARCHAR(255) NOT NULL,
    Url VARCHAR(255) NOT NULL UNIQUE,
    CreatedBy INT NOT NULL DEFAULT 0,
    CreatedOn datetime NOT NULL DEFAULT GetDate(),
    LastModifiedBy INT NOT NULL DEFAULT 0,
    LastModifiedOn datetime NULL,
    CONSTRAINT PK_Apii_ApiId PRIMARY KEY (ApiId),
    CONSTRAINT FK_Apii_MenuId FOREIGN KEY (MenuId) REFERENCES Menu (MenuId) 
);

CREATE TABLE [Group]
(
    GroupId INT NOT NULL IDENTITY(1,1),
    GroupName VARCHAR(255) NOT NULL UNIQUE,
    IsActive bit NOT NULL DEFAULT 1,
    CreatedBy INT NOT NULL DEFAULT 0,
    CreatedOn datetime NOT NULL DEFAULT GetDate(),
    LastModifiedBy INT NOT NULL DEFAULT 0,
    LastModifiedOn datetime NULL,
    CONSTRAINT PK_Group_GroupId PRIMARY KEY (GroupId)
);

CREATE TABLE UserGroup
(
    UserGroupId INT NOT NULL IDENTITY(1,1),
    UserId integer NOT NULL,
    GroupId integer NOT NULL,
    CreatedBy INT NOT NULL DEFAULT 0,
    CreatedOn datetime NOT NULL DEFAULT GetDate(),
    CONSTRAINT PK_UserGroup_UserGroupId PRIMARY KEY (UserGroupId),
    CONSTRAINT UQ_UserGroup_UserId_GroupId UNIQUE (UserId, GroupId),
    CONSTRAINT FK_UserGroup_UserId FOREIGN KEY (UserId) REFERENCES [User] (UserId),
    CONSTRAINT FK_UserGroup_GroupId FOREIGN KEY (GroupId) REFERENCES [Group](GroupId)
);

CREATE TABLE UserCompany
(
    UserCompanyId INT NOT NULL IDENTITY(1,1),
    UserId integer NOT NULL,
    CompanyCode VARCHAR(32) NOT NULL,
    CreatedBy INT NOT NULL DEFAULT 0,
    CreatedOn datetime NOT NULL DEFAULT GetDate(),
    CONSTRAINT PK_UserCompany_UserCompanyId PRIMARY KEY (UserCompanyId),
    CONSTRAINT UQ_UserCompany_UserId_CompanyCode UNIQUE (UserId, CompanyCode),
    CONSTRAINT FK_UserCompany_UserId FOREIGN KEY (UserId) REFERENCES [User] (UserId),
    CONSTRAINT FK_UserCompany_CompanyCode FOREIGN KEY (CompanyCode) REFERENCES Company (CompanyCode)
);

CREATE TABLE Permission
(
    PermissionId INT NOT NULL IDENTITY(1,1),
    MenuId integer NOT NULL,
    GroupId integer NOT NULL,
    IsView bit NOT NULL DEFAULT 0,
    IsAdd bit NOT NULL DEFAULT 0,
    IsDelete bit NOT NULL DEFAULT 0,
    IsUpdate bit NOT NULL DEFAULT 0,
    IsActive bit NOT NULL DEFAULT 0,
    CreatedBy INT NOT NULL DEFAULT 0,
    CreatedOn datetime NOT NULL DEFAULT GetDate(),
    CONSTRAINT PK_Permission_PermissionId PRIMARY KEY (PermissionId),
    CONSTRAINT UQ_Permission_MenuId_GroupId UNIQUE (MenuId, GroupId),
    CONSTRAINT FK_Permission_MenuId FOREIGN KEY (MenuId) REFERENCES Menu (MenuId),
    CONSTRAINT FK_Permission_GroupId FOREIGN KEY (GroupId) REFERENCES [Group] (GroupId)
);

ALTER TABLE Company ADD IsActive bit NOT NULL DEFAULT 1;
go
update Company set IsActive = 1;

create table dbo.Site(
    SiteId int identity(1,1)
    , SiteName   varchar(255) not null unique
    , IsActive bit NOT NULL DEFAULT 1
    , CreatedBy INT NOT NULL DEFAULT 0
    , CreatedOn datetime NOT NULL DEFAULT GetDate()
    , LastModifiedBy INT NOT NULL DEFAULT 0
    , LastModifiedOn datetime NULL
    , CONSTRAINT PK_Site_SiteId PRIMARY KEY (SiteId)
);

insert into Site(SiteName, IsActive, CreatedBy)
values ('MELAK', 1, 1);
go

ALTER TABLE [User] ADD IsActive bit NOT NULL DEFAULT 1;
go
update [User] set IsActive = 1;

CREATE TABLE UserSite
(
    UserSiteId INT NOT NULL IDENTITY(1,1),
    UserId integer NOT NULL,
    SiteId integer NOT NULL,
    CreatedBy INT NOT NULL DEFAULT 0,
    CreatedOn datetime NOT NULL DEFAULT GetDate(),
    CONSTRAINT PK_UserSite_UserSiteId PRIMARY KEY (UserSiteId),
    CONSTRAINT UQ_UserSite_UserId_SiteId UNIQUE (UserId, SiteId),
    CONSTRAINT FK_UserSite_UserId FOREIGN KEY (UserId) REFERENCES [User] (UserId),
    CONSTRAINT FK_UserSite_SiteId FOREIGN KEY (SiteId) REFERENCES Site(SiteId)
);

ALTER TABLE Company ADD SiteId integer NOT NULL DEFAULT 1;-- FOREIGN KEY (SiteId) REFERENCES Site(SiteId);
go
update Company set SiteId = 1;
go
ALTER TABLE Company ADD FOREIGN KEY (SiteId) REFERENCES Site(SiteId);
go

CREATE TABLE Tunnel
(
    TunnelId INT NOT NULL IDENTITY(1,1),
    CompanyCode VARCHAR(32) NOT NULL,
    Name VARCHAR(255) NOT NULL,
    CreatedBy INT NOT NULL DEFAULT 0,
    CreatedOn datetime NOT NULL DEFAULT GetDate(),
    LastModifiedBy INT NOT NULL DEFAULT 0,
    LastModifiedOn datetime NULL,
    IsActive bit NOT NULL DEFAULT 0,
    CONSTRAINT PK_Tunnel_TunnelId PRIMARY KEY (TunnelId),
    CONSTRAINT FK_Tunnel_CompanyCode FOREIGN KEY (CompanyCode) REFERENCES Company(CompanyCode)
);

CREATE TABLE Hopper
(
    HopperId INT NOT NULL IDENTITY(1,1),
    CompanyCode VARCHAR(32) NOT NULL,
    Name VARCHAR(255) NOT NULL,
    CreatedBy INT NOT NULL DEFAULT 0,
    CreatedOn datetime NOT NULL DEFAULT GetDate(),
    LastModifiedBy INT NOT NULL DEFAULT 0,
    LastModifiedOn datetime NULL,
    IsActive bit NOT NULL DEFAULT 0,
    CONSTRAINT PK_Hopper_HopperId PRIMARY KEY (HopperId),
    CONSTRAINT FK_Hopper_CompanyCode FOREIGN KEY (CompanyCode) REFERENCES Company(CompanyCode)
);

CREATE TABLE Tunnel_Actual
(
    RecordId BIGINT NOT NULL IDENTITY(1,1),
    HaulingRequest_Reference bigint not null,
    TunnelId integer NOT NULL,
    CreatedBy INT NOT NULL DEFAULT 0,
    CreatedOn datetime NOT NULL DEFAULT GetDate(),
    LastModifiedBy INT NOT NULL DEFAULT 0,
    LastModifiedOn datetime NULL,
    [Time] datetime NOT NULL,
    Remark varchar(255) not null default '',
    CONSTRAINT PK_Tunnel_Actual_RecordId PRIMARY KEY (RecordId),
    CONSTRAINT FK_Tunnel_Actual_HaulingRequest_Reference FOREIGN KEY (HaulingRequest_Reference) REFERENCES HaulingRequest_Detail_PortionBlending(RecordIdx),
    CONSTRAINT FK_Tunnel_Actual_TunnelId FOREIGN KEY (TunnelId) REFERENCES Tunnel(TunnelId)
);

CREATE TABLE Feedback
(
    FeedbackId INT NOT NULL IDENTITY(1,1),
    Accuracy INT NOT NULL DEFAULT 0,
    EasyToUnderstand INT NOT NULL DEFAULT 0,
    Punctuality INT NOT NULL DEFAULT 0,
    Objectivity INT NOT NULL DEFAULT 0,
    Detailed INT NOT NULL DEFAULT 0,
    Remark varchar(255) not null default '',
    Page varchar(255) not null default '',
    CreatedBy INT NOT NULL DEFAULT 0,
    CreatedOn datetime NOT NULL DEFAULT GetDate(),
    LastModifiedBy INT NOT NULL DEFAULT 0,
    LastModifiedOn datetime NULL,
    CONSTRAINT PK_Feedback_FeedbackId PRIMARY KEY (FeedbackId)
);

update [User]
set Pwd_DB = '08fa299aecc0c034e037033e3b0bbfaef26b78c742f16cf88ac3194502d6c394'
where Is_ActiveDirectory = 0;
go

update [User]
set Pwd_DB = 'a26bdf0183a69f0525c187f30822055dfd69c7495ac19618c04dbc4c132a5f65'
where LoginName = 'scheduler';
go
