SET DATEFORMAT ymd
SET ARITHABORT, ANSI_PADDING, ANSI_WARNINGS, CONCAT_NULL_YIELDS_NULL, QUOTED_IDENTIFIER, ANSI_NULLS, NOCOUNT ON
SET NUMERIC_ROUNDABORT, IMPLICIT_TRANSACTIONS, XACT_ABORT OFF
GO

SET IDENTITY_INSERT Tunnel ON
GO
INSERT Tunnel(TunnelId, CompanyCode, Name, CreatedBy, CreatedOn, LastModifiedBy, LastModifiedOn, IsActive) VALUES (1, 'BEK', 'BTN1', 2, '2020-09-23 05:08:57.887', 2, '2020-09-23 05:11:14.347', CONVERT(bit, 'True'))
INSERT Tunnel(TunnelId, CompanyCode, Name, CreatedBy, CreatedOn, LastModifiedBy, LastModifiedOn, IsActive) VALUES (2, 'BEK', 'BTN2', 2, '2020-09-23 05:09:05.667', 2, '2020-09-23 05:11:22.557', CONVERT(bit, 'True'))
INSERT Tunnel(TunnelId, CompanyCode, Name, CreatedBy, CreatedOn, LastModifiedBy, LastModifiedOn, IsActive) VALUES (3, 'BEK', 'BTN3', 2, '2020-09-23 05:09:15.890', 2, '2020-09-23 05:11:34.400', CONVERT(bit, 'True'))
INSERT Tunnel(TunnelId, CompanyCode, Name, CreatedBy, CreatedOn, LastModifiedBy, LastModifiedOn, IsActive) VALUES (4, 'BEK', 'BTN4', 2, '2020-09-23 05:11:46.883', 0, NULL, CONVERT(bit, 'True'))
INSERT Tunnel(TunnelId, CompanyCode, Name, CreatedBy, CreatedOn, LastModifiedBy, LastModifiedOn, IsActive) VALUES (5, 'BEK', 'BTN5', 2, '2020-09-23 05:11:54.517', 0, NULL, CONVERT(bit, 'True'))
INSERT Tunnel(TunnelId, CompanyCode, Name, CreatedBy, CreatedOn, LastModifiedBy, LastModifiedOn, IsActive) VALUES (6, 'BEK', 'BTN6', 2, '2020-09-23 05:12:03.263', 0, NULL, CONVERT(bit, 'True'))
INSERT Tunnel(TunnelId, CompanyCode, Name, CreatedBy, CreatedOn, LastModifiedBy, LastModifiedOn, IsActive) VALUES (7, 'TCM', 'TN1', 2, '2020-09-23 05:12:15.183', 0, NULL, CONVERT(bit, 'True'))
INSERT Tunnel(TunnelId, CompanyCode, Name, CreatedBy, CreatedOn, LastModifiedBy, LastModifiedOn, IsActive) VALUES (8, 'TCM', 'TN2', 2, '2020-09-23 05:12:22.127', 0, NULL, CONVERT(bit, 'True'))
INSERT Tunnel(TunnelId, CompanyCode, Name, CreatedBy, CreatedOn, LastModifiedBy, LastModifiedOn, IsActive) VALUES (9, 'TCM', 'TN3', 2, '2020-09-23 05:12:27.827', 0, NULL, CONVERT(bit, 'True'))
INSERT Tunnel(TunnelId, CompanyCode, Name, CreatedBy, CreatedOn, LastModifiedBy, LastModifiedOn, IsActive) VALUES (10, 'TCM', 'TN4', 2, '2020-09-23 05:12:33.523', 0, NULL, CONVERT(bit, 'True'))
INSERT Tunnel(TunnelId, CompanyCode, Name, CreatedBy, CreatedOn, LastModifiedBy, LastModifiedOn, IsActive) VALUES (11, 'TCM', 'TN5', 2, '2020-09-23 05:12:52.820', 0, NULL, CONVERT(bit, 'True'))
INSERT Tunnel(TunnelId, CompanyCode, Name, CreatedBy, CreatedOn, LastModifiedBy, LastModifiedOn, IsActive) VALUES (12, 'TCM', 'TN6', 2, '2020-09-23 05:13:00.977', 1137, '2020-09-29 13:59:02.370', CONVERT(bit, 'True'))
GO
SET IDENTITY_INSERT Tunnel OFF
GO

SET IDENTITY_INSERT Hopper ON
GO
INSERT Hopper(HopperId, CompanyCode, Name, CreatedBy, CreatedOn, LastModifiedBy, LastModifiedOn, IsActive) VALUES (1, 'BEK', 'CR 02 - HOPPER 03 BEK', 2, '2020-09-23 05:15:41.120', 2, '2020-09-23 05:17:01.123', CONVERT(bit, 'True'))
INSERT Hopper(HopperId, CompanyCode, Name, CreatedBy, CreatedOn, LastModifiedBy, LastModifiedOn, IsActive) VALUES (2, 'TCM', 'HOPPER 01 TCM (SMI)', 2, '2020-09-23 05:15:51.450', 0, NULL, CONVERT(bit, 'True'))
INSERT Hopper(HopperId, CompanyCode, Name, CreatedBy, CreatedOn, LastModifiedBy, LastModifiedOn, IsActive) VALUES (3, 'TCM', 'CR 03 - HOPPER 02 TCM', 2, '2020-09-23 05:16:39.223', 0, NULL, CONVERT(bit, 'True'))
GO
SET IDENTITY_INSERT Hopper OFF
GO

SET IDENTITY_INSERT [Group] ON
GO
INSERT [Group](GroupId, GroupName, IsActive, CreatedBy, CreatedOn, LastModifiedBy, LastModifiedOn) VALUES (1, 'Administrator', CONVERT(bit, 'True'), 2, '2020-09-02 12:44:13.743', 2, '2020-09-14 06:03:09.650')
INSERT [Group](GroupId, GroupName, IsActive, CreatedBy, CreatedOn, LastModifiedBy, LastModifiedOn) VALUES (20, 'Master Data', CONVERT(bit, 'True'), 1159, '2020-09-08 15:45:18.207', 0, NULL)
INSERT [Group](GroupId, GroupName, IsActive, CreatedBy, CreatedOn, LastModifiedBy, LastModifiedOn) VALUES (24, 'Lab PIC - Sampling Request', CONVERT(bit, 'True'), 2, '2020-10-01 07:53:59.827', 0, NULL)
INSERT [Group](GroupId, GroupName, IsActive, CreatedBy, CreatedOn, LastModifiedBy, LastModifiedOn) VALUES (25, 'Requester - Sampling Request', CONVERT(bit, 'True'), 2, '2020-10-01 07:54:10.540', 0, NULL)
INSERT [Group](GroupId, GroupName, IsActive, CreatedBy, CreatedOn, LastModifiedBy, LastModifiedOn) VALUES (26, 'Lab PIC - Analysis Request', CONVERT(bit, 'True'), 2, '2020-10-01 07:54:19.010', 2, '2020-10-01 07:54:33.033')
INSERT [Group](GroupId, GroupName, IsActive, CreatedBy, CreatedOn, LastModifiedBy, LastModifiedOn) VALUES (27, 'Requester - Analysis Request', CONVERT(bit, 'True'), 2, '2020-10-01 07:54:28.327', 0, NULL)
INSERT [Group](GroupId, GroupName, IsActive, CreatedBy, CreatedOn, LastModifiedBy, LastModifiedOn) VALUES (28, 'CPL', CONVERT(bit, 'True'), 2, '2020-10-01 07:58:06.427', 0, NULL)
INSERT [Group](GroupId, GroupName, IsActive, CreatedBy, CreatedOn, LastModifiedBy, LastModifiedOn) VALUES (29, 'Lab Maintenance', CONVERT(bit, 'True'), 2, '2020-10-01 08:23:28.697', 0, NULL)
INSERT [Group](GroupId, GroupName, IsActive, CreatedBy, CreatedOn, LastModifiedBy, LastModifiedOn) VALUES (30, 'Consumable', CONVERT(bit, 'True'), 2, '2020-10-01 08:23:43.367', 0, NULL)
INSERT [Group](GroupId, GroupName, IsActive, CreatedBy, CreatedOn, LastModifiedBy, LastModifiedOn) VALUES (31, 'Report - All', CONVERT(bit, 'True'), 2, '2020-10-01 08:26:50.233', 2, '2020-10-01 08:26:59.497')
INSERT [Group](GroupId, GroupName, IsActive, CreatedBy, CreatedOn, LastModifiedBy, LastModifiedOn) VALUES (32, 'Report - ROM Sampling', CONVERT(bit, 'True'), 2, '2020-10-01 08:27:20.863', 0, NULL)
INSERT [Group](GroupId, GroupName, IsActive, CreatedBy, CreatedOn, LastModifiedBy, LastModifiedOn) VALUES (33, 'Report - Geology Exploration', CONVERT(bit, 'True'), 2, '2020-10-01 08:27:41.940', 0, NULL)
INSERT [Group](GroupId, GroupName, IsActive, CreatedBy, CreatedOn, LastModifiedBy, LastModifiedOn) VALUES (34, 'Report - Geology Pit Monitoring', CONVERT(bit, 'True'), 2, '2020-10-01 08:28:10.017', 0, NULL)
INSERT [Group](GroupId, GroupName, IsActive, CreatedBy, CreatedOn, LastModifiedBy, LastModifiedOn) VALUES (35, 'Report - Crushing Plant', CONVERT(bit, 'True'), 2, '2020-10-01 08:28:32.890', 0, NULL)
INSERT [Group](GroupId, GroupName, IsActive, CreatedBy, CreatedOn, LastModifiedBy, LastModifiedOn) VALUES (36, 'Report - Barge Loading', CONVERT(bit, 'True'), 2, '2020-10-01 08:28:51.977', 0, NULL)
GO
SET IDENTITY_INSERT [Group] OFF
GO

SET IDENTITY_INSERT Menu ON
GO
INSERT Menu(MenuId, MenuName, Description, Url, Logo, ParentId, IsActive, CreatedBy, CreatedOn, LastModifiedBy, LastModifiedOn, Ordering, Level) VALUES (1, 'Master Data', '', 'mstr', '', 0, CONVERT(bit, 'True'), 2, '2020-09-03 07:20:45.660', 2, '2020-09-04 07:25:47.030', 1, 10)
INSERT Menu(MenuId, MenuName, Description, Url, Logo, ParentId, IsActive, CreatedBy, CreatedOn, LastModifiedBy, LastModifiedOn, Ordering, Level) VALUES (2, 'Unit Measurement', '', '/UnitMeasurementsv', '', 1, CONVERT(bit, 'True'), 2, '2020-09-03 07:29:18.400', 2, '2020-09-03 16:19:43.820', 1, 11)
INSERT Menu(MenuId, MenuName, Description, Url, Logo, ParentId, IsActive, CreatedBy, CreatedOn, LastModifiedBy, LastModifiedOn, Ordering, Level) VALUES (3, 'Instrument', '', '/Instrumentsv', '', 1, CONVERT(bit, 'True'), 1141, '2020-09-03 07:34:38.537', 2, '2020-09-03 16:20:04.180', 2, 12)
INSERT Menu(MenuId, MenuName, Description, Url, Logo, ParentId, IsActive, CreatedBy, CreatedOn, LastModifiedBy, LastModifiedOn, Ordering, Level) VALUES (4, 'Maintenance Activity', '', '/MaintenanceActivityv', '', 1, CONVERT(bit, 'True'), 2, '2020-09-03 16:20:34.910', 0, NULL, 3, 13)
INSERT Menu(MenuId, MenuName, Description, Url, Logo, ParentId, IsActive, CreatedBy, CreatedOn, LastModifiedBy, LastModifiedOn, Ordering, Level) VALUES (5, 'Site', '', '/Sitev', '', 1, CONVERT(bit, 'True'), 2, '2020-09-03 16:21:01.593', 0, NULL, 4, 14)
INSERT Menu(MenuId, MenuName, Description, Url, Logo, ParentId, IsActive, CreatedBy, CreatedOn, LastModifiedBy, LastModifiedOn, Ordering, Level) VALUES (6, 'Company', '', '/Companyv', '', 1, CONVERT(bit, 'True'), 2, '2020-09-03 16:21:21.530', 0, NULL, 5, 15)
INSERT Menu(MenuId, MenuName, Description, Url, Logo, ParentId, IsActive, CreatedBy, CreatedOn, LastModifiedBy, LastModifiedOn, Ordering, Level) VALUES (7, 'Lab Request', '', 'labrequest', '', 0, CONVERT(bit, 'True'), 2, '2020-09-03 16:21:38.743', 2, '2020-09-03 16:22:53.613', 2, 20)
INSERT Menu(MenuId, MenuName, Description, Url, Logo, ParentId, IsActive, CreatedBy, CreatedOn, LastModifiedBy, LastModifiedOn, Ordering, Level) VALUES (8, 'Sampling Request', '', '/SamplingRequestv', '', 7, CONVERT(bit, 'True'), 2, '2020-09-03 16:22:00.583', 0, NULL, 1, 21)
INSERT Menu(MenuId, MenuName, Description, Url, Logo, ParentId, IsActive, CreatedBy, CreatedOn, LastModifiedBy, LastModifiedOn, Ordering, Level) VALUES (9, 'Analysis Request', '', '/AnalysisRequestv', '', 7, CONVERT(bit, 'True'), 2, '2020-09-03 16:22:21.347', 0, NULL, 2, 22)
INSERT Menu(MenuId, MenuName, Description, Url, Logo, ParentId, IsActive, CreatedBy, CreatedOn, LastModifiedBy, LastModifiedOn, Ordering, Level) VALUES (10, 'Laboratory', '', 'laboratory', '', 0, CONVERT(bit, 'True'), 2, '2020-09-03 16:22:47.070', 0, NULL, 3, 30)
INSERT Menu(MenuId, MenuName, Description, Url, Logo, ParentId, IsActive, CreatedBy, CreatedOn, LastModifiedBy, LastModifiedOn, Ordering, Level) VALUES (11, 'Upload Lab Analysis Request', '', '/UploadLabAnalysisResultv', '', 10, CONVERT(bit, 'True'), 2, '2020-09-03 16:23:31.627', 0, NULL, 1, 31)
INSERT Menu(MenuId, MenuName, Description, Url, Logo, ParentId, IsActive, CreatedBy, CreatedOn, LastModifiedBy, LastModifiedOn, Ordering, Level) VALUES (12, 'Coal Quality Report', '', '/QualityReportv', '', 0, CONVERT(bit, 'True'), 2, '2020-09-03 16:25:01.547', 2, '2020-09-14 05:29:59.137', 4, 40)
INSERT Menu(MenuId, MenuName, Description, Url, Logo, ParentId, IsActive, CreatedBy, CreatedOn, LastModifiedBy, LastModifiedOn, Ordering, Level) VALUES (13, 'Lab Consumable', '', '/Consumablev', '', 10, CONVERT(bit, 'True'), 2, '2020-09-03 16:25:29.757', 2, '2020-09-14 05:36:19.790', 2, 32)
INSERT Menu(MenuId, MenuName, Description, Url, Logo, ParentId, IsActive, CreatedBy, CreatedOn, LastModifiedBy, LastModifiedOn, Ordering, Level) VALUES (14, 'Lab Maintenance', '', '/LabMaintenancev', '', 10, CONVERT(bit, 'True'), 2, '2020-09-03 16:25:57.320', 2, '2020-09-26 01:13:16.623', 3, 33)
INSERT Menu(MenuId, MenuName, Description, Url, Logo, ParentId, IsActive, CreatedBy, CreatedOn, LastModifiedBy, LastModifiedOn, Ordering, Level) VALUES (15, 'Blending & Hauling', '', 'blending', '', 0, CONVERT(bit, 'True'), 2, '2020-09-03 16:26:23.620', 2, '2020-09-14 05:30:52.837', 5, 50)
INSERT Menu(MenuId, MenuName, Description, Url, Logo, ParentId, IsActive, CreatedBy, CreatedOn, LastModifiedBy, LastModifiedOn, Ordering, Level) VALUES (16, 'Portion Blending', '', '/PortionBlendingv', '', 15, CONVERT(bit, 'True'), 2, '2020-09-03 16:26:52.763', 2, '2020-09-14 05:33:18.770', 1, 51)
INSERT Menu(MenuId, MenuName, Description, Url, Logo, ParentId, IsActive, CreatedBy, CreatedOn, LastModifiedBy, LastModifiedOn, Ordering, Level) VALUES (17, 'ROM Transfer', '', '/RomTransferv', '', 15, CONVERT(bit, 'True'), 2, '2020-09-03 16:27:14.823', 2, '2020-09-14 05:41:51.540', 3, 53)
INSERT Menu(MenuId, MenuName, Description, Url, Logo, ParentId, IsActive, CreatedBy, CreatedOn, LastModifiedBy, LastModifiedOn, Ordering, Level) VALUES (18, 'Hauling Request', '', '/Haulingv', '', 15, CONVERT(bit, 'True'), 2, '2020-09-03 16:27:44.027', 2, '2020-09-26 01:17:17.300', 4, 54)
INSERT Menu(MenuId, MenuName, Description, Url, Logo, ParentId, IsActive, CreatedBy, CreatedOn, LastModifiedBy, LastModifiedOn, Ordering, Level) VALUES (19, 'Tool', '', 'tool', '', 0, CONVERT(bit, 'True'), 2, '2020-09-03 16:28:07.223', 2, '2020-09-14 05:44:31.473', 7, 70)
INSERT Menu(MenuId, MenuName, Description, Url, Logo, ParentId, IsActive, CreatedBy, CreatedOn, LastModifiedBy, LastModifiedOn, Ordering, Level) VALUES (20, 'User''s Activity', '', '/UserActivities', '', 19, CONVERT(bit, 'True'), 2, '2020-09-03 16:28:43.937', 2, '2020-09-14 05:44:55.493', 1, 71)
INSERT Menu(MenuId, MenuName, Description, Url, Logo, ParentId, IsActive, CreatedBy, CreatedOn, LastModifiedBy, LastModifiedOn, Ordering, Level) VALUES (21, 'All Upload Excel files', '', '/Filev', '', 19, CONVERT(bit, 'True'), 2, '2020-09-03 16:29:09.720', 2, '2020-09-14 05:45:02.380', 2, 72)
INSERT Menu(MenuId, MenuName, Description, Url, Logo, ParentId, IsActive, CreatedBy, CreatedOn, LastModifiedBy, LastModifiedOn, Ordering, Level) VALUES (22, 'Authorization', '', 'auth', '', 0, CONVERT(bit, 'True'), 2, '2020-09-03 16:29:32.960', 2, '2020-09-14 05:44:48.100', 8, 80)
INSERT Menu(MenuId, MenuName, Description, Url, Logo, ParentId, IsActive, CreatedBy, CreatedOn, LastModifiedBy, LastModifiedOn, Ordering, Level) VALUES (23, 'User', '', '/Userv', '', 22, CONVERT(bit, 'True'), 2, '2020-09-03 16:29:54.667', 2, '2020-09-21 10:20:02.437', 1, 81)
INSERT Menu(MenuId, MenuName, Description, Url, Logo, ParentId, IsActive, CreatedBy, CreatedOn, LastModifiedBy, LastModifiedOn, Ordering, Level) VALUES (24, 'Group', '', '/Groupv', '', 22, CONVERT(bit, 'True'), 2, '2020-09-03 16:30:09.393', 2, '2020-09-14 05:45:30.037', 2, 82)
INSERT Menu(MenuId, MenuName, Description, Url, Logo, ParentId, IsActive, CreatedBy, CreatedOn, LastModifiedBy, LastModifiedOn, Ordering, Level) VALUES (25, 'Menu', '', '/Menuv', '', 22, CONVERT(bit, 'True'), 2, '2020-09-03 16:30:27.113', 2, '2020-09-14 05:45:36.297', 3, 83)
INSERT Menu(MenuId, MenuName, Description, Url, Logo, ParentId, IsActive, CreatedBy, CreatedOn, LastModifiedBy, LastModifiedOn, Ordering, Level) VALUES (26, 'Permission', '', '/Permissionv', '', 22, CONVERT(bit, 'True'), 2, '2020-09-03 16:30:47.610', 2, '2020-09-14 05:45:41.497', 4, 84)
INSERT Menu(MenuId, MenuName, Description, Url, Logo, ParentId, IsActive, CreatedBy, CreatedOn, LastModifiedBy, LastModifiedOn, Ordering, Level) VALUES (30, 'Tunnel', '', '/Tunnelv', '', 1, CONVERT(bit, 'True'), 2, '2020-09-14 05:27:03.427', 0, NULL, 6, 16)
INSERT Menu(MenuId, MenuName, Description, Url, Logo, ParentId, IsActive, CreatedBy, CreatedOn, LastModifiedBy, LastModifiedOn, Ordering, Level) VALUES (31, 'Hopper', '', '/Hopperv', '', 1, CONVERT(bit, 'True'), 2, '2020-09-14 05:27:23.830', 0, NULL, 7, 17)
INSERT Menu(MenuId, MenuName, Description, Url, Logo, ParentId, IsActive, CreatedBy, CreatedOn, LastModifiedBy, LastModifiedOn, Ordering, Level) VALUES (32, 'ROM Sampling', '', '/ROMSamplingv', '', 12, CONVERT(bit, 'True'), 2, '2020-09-14 05:34:44.590', 0, NULL, 1, 41)
INSERT Menu(MenuId, MenuName, Description, Url, Logo, ParentId, IsActive, CreatedBy, CreatedOn, LastModifiedBy, LastModifiedOn, Ordering, Level) VALUES (33, 'Geology Exploration', '', '/GeologyExplorationv', '', 12, CONVERT(bit, 'True'), 2, '2020-09-14 05:35:04.887', 0, NULL, 2, 42)
INSERT Menu(MenuId, MenuName, Description, Url, Logo, ParentId, IsActive, CreatedBy, CreatedOn, LastModifiedBy, LastModifiedOn, Ordering, Level) VALUES (34, 'Geology Pit Monitoring', '', '/GeologyPitMonitoringv', '', 12, CONVERT(bit, 'True'), 2, '2020-09-14 05:35:23.907', 2, '2020-09-14 05:48:59.333', 3, 43)
INSERT Menu(MenuId, MenuName, Description, Url, Logo, ParentId, IsActive, CreatedBy, CreatedOn, LastModifiedBy, LastModifiedOn, Ordering, Level) VALUES (35, 'Crushing Plant', '', '/CrushingPlantv', '', 12, CONVERT(bit, 'True'), 2, '2020-09-14 05:35:39.953', 0, NULL, 4, 44)
INSERT Menu(MenuId, MenuName, Description, Url, Logo, ParentId, IsActive, CreatedBy, CreatedOn, LastModifiedBy, LastModifiedOn, Ordering, Level) VALUES (36, 'Barge Loading', '', '/BargeLoadingv', '', 12, CONVERT(bit, 'True'), 2, '2020-09-14 05:35:55.563', 0, NULL, 5, 45)
INSERT Menu(MenuId, MenuName, Description, Url, Logo, ParentId, IsActive, CreatedBy, CreatedOn, LastModifiedBy, LastModifiedOn, Ordering, Level) VALUES (37, 'Tunnel Management', '', '/TunnelManagementv', '', 15, CONVERT(bit, 'True'), 2, '2020-09-14 05:41:01.960', 0, NULL, 2, 52)
INSERT Menu(MenuId, MenuName, Description, Url, Logo, ParentId, IsActive, CreatedBy, CreatedOn, LastModifiedBy, LastModifiedOn, Ordering, Level) VALUES (38, 'Customer Feedback', '', '/Feedbackv', '', 15, CONVERT(bit, 'True'), 2, '2020-09-14 05:41:28.093', 2, '2020-09-21 10:21:25.257', 5, 55)
GO
SET IDENTITY_INSERT Menu OFF
GO

insert into UserSite(UserId, SiteId, CreatedBy)
select UserId, 1, 1
from [User];
go

insert into UserCompany(UserId, CompanyCode, CreatedBy)
select UserId, 'TCM', 1
from [User];
go

insert into UserCompany(UserId, CompanyCode, CreatedBy)
select UserId, 'BEK', 1
from [User];
go

declare @v_groupId int;
declare @v_groupName varchar(32);

set @v_groupName = 'Administrator';
select @v_groupId = GroupId from [Group] where GroupName = @v_groupName;

insert into UserGroup(UserId, GroupId, CreatedBy)
select UserId, @v_groupId, 1
from [User]
where IsAdmin = 1;

set @v_groupName = 'Master Data';
select @v_groupId = GroupId from [Group] where GroupName = @v_groupName;

insert into UserGroup(UserId, GroupId, CreatedBy)
select UserId, @v_groupId, 1
from [User]
where IsMasterData = 1;

set @v_groupName = 'Lab PIC - Sampling Request';
select @v_groupId = GroupId from [Group] where GroupName = @v_groupName;

insert into UserGroup(UserId, GroupId, CreatedBy)
select UserId, @v_groupId, 1
from [User]
where IsUserLab = 1;

set @v_groupName = 'Requester - Sampling Request';
select @v_groupId = GroupId from [Group] where GroupName = @v_groupName;

insert into UserGroup(UserId, GroupId, CreatedBy)
select UserId, @v_groupId, 1
from [User]
where IsUserBiasa = 1;

set @v_groupName = 'Lab PIC - Analysis Request';
select @v_groupId = GroupId from [Group] where GroupName = @v_groupName;

insert into UserGroup(UserId, GroupId, CreatedBy)
select UserId, @v_groupId, 1
from [User]
where IsUserLab = 1;

set @v_groupName = 'Requester - Analysis Request';
select @v_groupId = GroupId from [Group] where GroupName = @v_groupName;

insert into UserGroup(UserId, GroupId, CreatedBy)
select UserId, @v_groupId, 1
from [User]
where IsUserBiasa = 1;

set @v_groupName = 'CPL';
select @v_groupId = GroupId from [Group] where GroupName = @v_groupName;

insert into UserGroup(UserId, GroupId, CreatedBy)
select UserId, @v_groupId, 1
from [User]
where IsCPL = 1;

set @v_groupName = 'Lab Maintenance';
select @v_groupId = GroupId from [Group] where GroupName = @v_groupName;

insert into UserGroup(UserId, GroupId, CreatedBy)
select UserId, @v_groupId, 1
from [User]
where IsLabMaintenance = 1;

set @v_groupName = 'Consumable';
select @v_groupId = GroupId from [Group] where GroupName = @v_groupName;

insert into UserGroup(UserId, GroupId, CreatedBy)
select UserId, @v_groupId, 1
from [User]
where IsConsumable = 1;

set @v_groupName = 'Report - All';
select @v_groupId = GroupId from [Group] where GroupName = @v_groupName;

insert into UserGroup(UserId, GroupId, CreatedBy)
select UserId, @v_groupId, 1
from [User]
where IsReport = 1;
go
