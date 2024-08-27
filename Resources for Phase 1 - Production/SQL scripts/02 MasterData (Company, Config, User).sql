SET DATEFORMAT ymd
SET ARITHABORT, ANSI_PADDING, ANSI_WARNINGS, CONCAT_NULL_YIELDS_NULL, QUOTED_IDENTIFIER, ANSI_NULLS, NOCOUNT ON
SET NUMERIC_ROUNDABORT, IMPLICIT_TRANSACTIONS, XACT_ABORT OFF
GO

INSERT Company(CompanyCode, Name, CreatedBy, CreatedOn, LastModifiedBy, LastModifiedOn, DeletedBy, DeletedOn, Is_DataROM_from_BI) VALUES (N'BEK', 'BEK', 1, '2020-04-21 11:09:12.627', 0, NULL, 0, NULL, CONVERT(bit, 'True'))
INSERT Company(CompanyCode, Name, CreatedBy, CreatedOn, LastModifiedBy, LastModifiedOn, DeletedBy, DeletedOn, Is_DataROM_from_BI) VALUES (N'TCM', 'TCM', 1, '2020-04-21 11:09:16.637', 0, NULL, 0, NULL, CONVERT(bit, 'True'))
GO

SET IDENTITY_INSERT Config ON
GO
INSERT Config(RecordId, Name, Value) VALUES (1, 'Email_Server', 'smtp.gmail.com')
INSERT Config(RecordId, Name, Value) VALUES (2, 'Email_Port', '587')
INSERT Config(RecordId, Name, Value) VALUES (3, 'Email_User', 'itm.mercyapp@gmail.com')
INSERT Config(RecordId, Name, Value) VALUES (4, 'Email_Password', 'Lo2GAdGA$#a')
INSERT Config(RecordId, Name, Value) VALUES (5, 'Email_Subject_Sampling', '[MERCY] {COMPANY} - Sampling Request - {SAMPLING_TYPE} - {LOCATION} #{ID}')
INSERT Config(RecordId, Name, Value) VALUES (6, 'Email_Subject_Analysis', '[MERCY] {COMPANY} - Analysis Request - {ANALYSIS_TYPE} - {LETTER_NO} #{ID}')
INSERT Config(RecordId, Name, Value) VALUES (7, 'Email_Server2', 'Internalmail.banpuindo.co.id')
INSERT Config(RecordId, Name, Value) VALUES (8, 'Email_Subject_Hauling', 'Request Hauling {DATE}')
INSERT Config(RecordId, Name, Value) VALUES (9, 'Email_User_Hauling', 'sudirman@sudirman.web.id')
INSERT Config(RecordId, Name, Value) VALUES (10, 'PRODUCT_TCM', 'HCV-LS,HCV-HS ,HCV-MS (SMI)')
INSERT Config(RecordId, Name, Value) VALUES (11, 'PRODUCT_BEK', 'MCV-LS,MCV-HS ')
INSERT Config(RecordId, Name, Value) VALUES (12, 'Email_User_Hauling_CC', 'mdsudirman@gmail.com')
INSERT Config(RecordId, Name, Value) VALUES (13, 'REMINDER_CONSUMABLE_1', '1')
INSERT Config(RecordId, Name, Value) VALUES (14, 'REMINDER_CONSUMABLE_2', '7')
INSERT Config(RecordId, Name, Value) VALUES (15, 'REMINDER_MAINTENANCE_1', '1')
INSERT Config(RecordId, Name, Value) VALUES (16, 'REMINDER_MAINTENANCE_2', '7')
INSERT Config(RecordId, Name, Value) VALUES (17, 'REMINDER_CONSUMABLE_CC', 'mdsudirman@gmail.com')
INSERT Config(RecordId, Name, Value) VALUES (18, 'REMINDER_MAINTENANCE_CC', 'mdsudirman@gmail.com')
INSERT Config(RecordId, Name, Value) VALUES (19, 'REMINDER_CONSUMABLE_SUBJECT', '[MERCY] Reminder of Critical Lab Spare Part Inventory')
INSERT Config(RecordId, Name, Value) VALUES (20, 'REMINDER_MAINTENANCE_SUBJECT', '[MERCY] Reminder of Next Lab Maintenance Schedule')
GO
SET IDENTITY_INSERT Config OFF
GO

SET IDENTITY_INSERT [User] ON
GO
INSERT [User](UserId, LoginName, FullName, Title, Department, Email, IsAdmin, CreatedBy, CreatedOn, LastModifiedBy, LastModifiedOn, DeletedBy, DeletedOn, UserInterface, IsCPL, IsLabMaintenance, IsUserBiasa, IsUserLab, IsMasterData, IsConsumable, Is_ActiveDirectory, Pwd_DB, IsReport) VALUES (1, 'admin', 'Administrator', 'Tester', 'User Departmeant', '', CONVERT(bit, 'True'), 0, '2020-04-20 03:02:10.040', 3, '2020-09-28 05:39:14.330', 0, NULL, '', CONVERT(bit, 'True'), CONVERT(bit, 'True'), CONVERT(bit, 'True'), CONVERT(bit, 'True'), CONVERT(bit, 'True'), CONVERT(bit, 'True'), CONVERT(bit, 'False'), 'Pass123', CONVERT(bit, 'True'))
INSERT [User](UserId, LoginName, FullName, Title, Department, Email, IsAdmin, CreatedBy, CreatedOn, LastModifiedBy, LastModifiedOn, DeletedBy, DeletedOn, UserInterface, IsCPL, IsLabMaintenance, IsUserBiasa, IsUserLab, IsMasterData, IsConsumable, Is_ActiveDirectory, Pwd_DB, IsReport) VALUES (2, 'user_biasa', 'User Biasa', 'Tester', 'User Departmeant', '', CONVERT(bit, 'False'), 0, '2020-04-20 03:02:10.040', 3, '2020-09-28 05:39:14.330', 0, NULL, '', CONVERT(bit, 'False'), CONVERT(bit, 'False'), CONVERT(bit, 'True'), CONVERT(bit, 'False'), CONVERT(bit, 'False'), CONVERT(bit, 'False'), CONVERT(bit, 'False'), 'Pass123', CONVERT(bit, 'False'))
INSERT [User](UserId, LoginName, FullName, Title, Department, Email, IsAdmin, CreatedBy, CreatedOn, LastModifiedBy, LastModifiedOn, DeletedBy, DeletedOn, UserInterface, IsCPL, IsLabMaintenance, IsUserBiasa, IsUserLab, IsMasterData, IsConsumable, Is_ActiveDirectory, Pwd_DB, IsReport) VALUES (3, 'user_lab', 'User Lab', 'Tester', 'User Departmeant', '', CONVERT(bit, 'False'), 0, '2020-04-20 03:02:10.040', 3, '2020-09-28 05:39:14.330', 0, NULL, '', CONVERT(bit, 'False'), CONVERT(bit, 'False'), CONVERT(bit, 'False'), CONVERT(bit, 'True'), CONVERT(bit, 'False'), CONVERT(bit, 'False'), CONVERT(bit, 'False'), 'Pass123', CONVERT(bit, 'False'))
GO
SET IDENTITY_INSERT [User] OFF
GO