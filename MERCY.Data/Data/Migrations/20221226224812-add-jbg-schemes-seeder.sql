/* Up Script*/

-- Add new projects for JBG
SET IDENTITY_INSERT Project ON;
INSERT INTO Project
	(Id, Name, CreatedOn, CreatedBy, Code, Type)
VALUES
	(27, 'Shipment', getdate(), 0, 'SH', 'General'),
	(28, 'Drill Hole', getdate(), 0, 'DH', 'General');
SET IDENTITY_INSERT Project OFF;

-- Add new clients for JBG
SET IDENTITY_INSERT Client ON;
INSERT INTO Client 
	(Id, Name, CreatedOn, CreatedBy)
VALUES
	(27, 'Shipment', getdate(), 0);
SET IDENTITY_INSERT Client OFF;

-- Add new schemes for JBG
SET IDENTITY_INSERT Scheme ON;
INSERT INTO Scheme
	(Id, Name, CreatedOn, CreatedBy, Type)
VALUES
	(48, 'FM', getdate(), 0, 'DUPLO');
SET IDENTITY_INSERT Scheme OFF;

-- Add JBG company projects
INSERT INTO CompanyProject
	(CompanyCode, ProjectId, ProjectType, CreatedOn, CreatedBy)
VALUES
	('JBG', 2, 'Crushing', getdate(), 0),	-- Crushing Plant 2
	('JBG', 7, 'Barge', getdate(), 0),		-- Barge Loading
	('JBG', 27, 'General', getdate(), 0),	-- Shipment
	('JBG', 10, 'General', getdate(), 0),	-- Exploration
	('JBG', 28, 'General', getdate(), 0),	-- Drill Hole
	('JBG', 26, 'General', getdate(), 0),	-- Inspection
	('JBG', 22, 'General', getdate(), 0),	-- Daily Check
	('JBG', 1, 'General', getdate(), 0),	-- Crushing Plant
	('JBG', 17, 'General', getdate(), 0),	-- Proficiency Test
	('JBG', 18, 'General', getdate(), 0),	-- Geo Services
	('JBG', 16, 'General', getdate(), 0);	-- Special Project

-- Add JBG company scheme and formula
INSERT INTO CompanyScheme
	(CompanyCode, SchemeId, MinRepeatability, MaxRepeatability, Details)
VALUES
	/* FM */ ('JBG', 48, NULL, NULL, '{"externalAttributes":[],"rules":[{"header":"Type","attribute":"Type","input":false,"fn":null},{"header":"Sample ID","attribute":"Ident","input":false,"fn":null},{"header":"External ID","attribute":"ExtIdent","input":false,"fn":null},{"header":"FM WT_TRAY","attribute":"M1","input":false,"fn":{"arguments":"data","body":"return (data.Child[0].M1 + data.Child[1].M1) \/ 2;"}},{"header":"FM WT_TRAY_SMP_BEF","attribute":"M2","input":false,"fn":{"arguments":"data","body":"return (data.Child[0].M2 + data.Child[1].M2) \/ 2;"}},{"header":"FM WT_SMP","attribute":"M4","input":false,"fn":{"arguments":"data","body":"return ((parseFloat(data.Child[0].M4) + parseFloat(data.Child[1].M4)) \/ 2).toFixed(4);"}},{"header":"FM WT_TRAY_SMP_AFT","attribute":"M3","input":false,"fn":{"arguments":"data","body":"return (data.Child[0].M3 + data.Child[1].M3) \/ 2;"}},{"header":"FM %","attribute":"Total","input":false,"fn":{"arguments":"data","body":"return (data.M2 - data.M3)/(data.M2 - data.M1)*100;"}},{"header":"Repeatibility","attribute":"Repeatability","input":false,"fn":{"arguments":"data","body":"return Math.abs(data.Child[0].Total - data.Child[1].Total);"}}],"rulesChild":[{"header":"Type","attribute":"Type","input":false,"fn":null},{"header":"Sample ID","attribute":"Ident","input":false,"fn":null},{"header":"External ID","attribute":"ExtIdent","input":false,"fn":null},{"header":"FM WT_TRAY","attribute":"M1","input":true,"fn":null},{"header":"FM WT_TRAY_SMP_BEF","attribute":"M2","input":true,"fn":null},{"header":"FM WT_SMP","attribute":"M4","input":false,"fn":{"arguments":"data","body":"return parseFloat(data.M2 - data.M1).toFixed(4);"}},{"header":"FM WT_TRAY_SMP_AFT","attribute":"M3","input":true,"fn":null},{"header":"FM %","attribute":"Total","input":false,"fn":{"arguments":"data","body":"return ((data.M2 - data.M3)/(data.M2 - data.M1))*100;"}},{"header":"Repeatibility","attribute":"Repeatability","input":false,"fn":null}]}'),
	/* RM */ ('JBG', 24, NULL, 0.3, '{"externalAttributes":[],"rules":[{"header":"Type","attribute":"Type","input":false,"fn":null},{"header":"Sample ID","attribute":"Ident","input":false,"fn":null},{"header":"External ID","attribute":"ExtIdent","input":false,"fn":null},{"header":"RM WT_TRAY","attribute":"M1","input":false,"fn":{"arguments":"data","body":"return (data.Child[0].M1 + data.Child[1].M1) \/ 2;"}},{"header":"RM WT_TRAY_SMP_BEF","attribute":"M2","input":false,"fn":{"arguments":"data","body":"return (data.Child[0].M2 + data.Child[1].M2) \/ 2;"}},{"header":"RM WT_SMP","attribute":"M4","input":false,"fn":{"arguments":"data","body":"return ((parseFloat(data.Child[0].M4) + parseFloat(data.Child[1].M4)) \/ 2).toFixed(4);"}},{"header":"RM WT_TRAY_SMP_AFT","attribute":"M3","input":false,"fn":{"arguments":"data","body":"return (data.Child[0].M3 + data.Child[1].M3) \/ 2;"}},{"header":"RM %","attribute":"Total","input":false,"fn":{"arguments":"data","body":"return (data.M2 - data.M3)/(data.M2 - data.M1)*100;"}},{"header":"Repeatibility","attribute":"Repeatability","input":false,"fn":{"arguments":"data","body":"return Math.abs(data.Child[0].Total - data.Child[1].Total);"}}],"rulesChild":[{"header":"Type","attribute":"Type","input":false,"fn":null},{"header":"Sample ID","attribute":"Ident","input":false,"fn":null},{"header":"External ID","attribute":"ExtIdent","input":false,"fn":null},{"header":"RM WT_TRAY","attribute":"M1","input":true,"fn":null},{"header":"RM WT_TRAY_SMP_BEF","attribute":"M2","input":true,"fn":null},{"header":"RM WT_SMP","attribute":"M4","input":false,"fn":{"arguments":"data","body":"return parseFloat(data.M2 - data.M1).toFixed(4);"}},{"header":"RM WT_TRAY_SMP_AFT","attribute":"M3","input":true,"fn":null},{"header":"RM %","attribute":"Total","input":false,"fn":{"arguments":"data","body":"return ((data.M2 - data.M3)/(data.M2 - data.M1))*100;"}},{"header":"Repeatibility","attribute":"Repeatability","input":false,"fn":null}]}'),
	/* TM */ ('JBG', 28, NULL, NULL, '{"externalAttributes":["FM","RM"],"rules":[{"header":"Type","attribute":"Type","input":false,"fn":null},{"header":"Sample ID","attribute":"Ident","input":false,"fn":null},{"header":"External ID","attribute":"ExtIdent","input":false,"fn":null},{"header":"TM % ar","attribute":"Total","input":false,"fn":{"arguments":"data","body":"return data.FM + (data.RM * (1 - data.FM/100));"}}],"rulesChild":null}'),
	/* IM */ ('JBG', 12, NULL, 0.15, '{"externalAttributes":[],"rules":[{"header":"Type","attribute":"Type","input":false,"fn":null},{"header":"Sample ID","attribute":"Ident","input":false,"fn":null},{"header":"External ID","attribute":"ExtIdent","input":false,"fn":null},{"header":"IM WT_CRUC","attribute":"M1","input":false,"fn":{"arguments":"data","body":"return (data.Child[0].M1 + data.Child[1].M1) \/ 2;"}},{"header":"IM WT_CRUC_SMP_BEF","attribute":"M2","input":false,"fn":{"arguments":"data","body":"return (data.Child[0].M2 + data.Child[1].M2) \/ 2;"}},{"header":"IM WT_SMP","attribute":"M4","input":false,"fn":{"arguments":"data","body":"return ((parseFloat(data.Child[0].M4) + parseFloat(data.Child[1].M4)) \/ 2).toFixed(4);"}},{"header":"IM WT_CRUC_SMP_AFT","attribute":"M3","input":false,"fn":{"arguments":"data","body":"return (data.Child[0].M3 + data.Child[1].M3) \/ 2;"}},{"header":"IM %","attribute":"Total","input":false,"fn":{"arguments":"data","body":"return (data.M2 - data.M3)/(data.M2 - data.M1)*100;"}},{"header":"Repeatibility","attribute":"Repeatability","input":false,"fn":{"arguments":"data","body":"return Math.abs(data.Child[0].Total - data.Child[1].Total);"}}],"rulesChild":[{"header":"Type","attribute":"Type","input":false,"fn":null},{"header":"Sample ID","attribute":"Ident","input":false,"fn":null},{"header":"External ID","attribute":"ExtIdent","input":false,"fn":null},{"header":"IM WT_CRUC","attribute":"M1","input":true,"fn":null},{"header":"IM WT_CRUC_SMP_BEF","attribute":"M2","input":true,"fn":null},{"header":"IM WT_SMP","attribute":"M4","input":false,"fn":{"arguments":"data","body":"return parseFloat(data.M2 - data.M1).toFixed(4);"}},{"header":"IM WT_CRUC_SMP_AFT","attribute":"M3","input":true,"fn":null},{"header":"IM %","attribute":"Total","input":false,"fn":{"arguments":"data","body":"return parseFloat(((data.M2 - data.M3)/(data.M2 - data.M1))*100).toFixed(4);"}},{"header":"Repeatibility","attribute":"Repeatability","input":false,"fn":null}]}'),
	/* ASH */ ('JBG', 4, NULL, NULL, '{"externalAttributes":["TM","IM"],"rules":[{"header":"Type","attribute":"Type","input":false,"fn":null},{"header":"Sample ID","attribute":"Ident","input":false,"fn":null},{"header":"External ID","attribute":"ExtIdent","input":false,"fn":null},{"header":"ASH WT_CRUC","attribute":"M1","input":false,"fn":{"arguments":"data","body":"return parseFloat((data.Child[0].M1 + data.Child[1].M1) \/ 2).toFixed(4);"}},{"header":"ASH WT_CRUC_SMP_BEF","attribute":"M2","input":false,"fn":{"arguments":"data","body":"return (data.Child[0].M2 + data.Child[1].M2) \/ 2;"}},{"header":"ASH WT_SMP","attribute":"M4","input":false,"fn":{"arguments":"data","body":"return ((parseFloat(data.Child[0].M4) + parseFloat(data.Child[1].M4)) \/ 2).toFixed(4);"}},{"header":"ASH WT_CRUC_ASH","attribute":"M3","input":false,"fn":{"arguments":"data","body":"return (data.Child[0].M3 + data.Child[1].M3) \/ 2;"}},{"header":"ASH_AD %","attribute":"Total","input":false,"fn":{"arguments":"data","body":"return (data.Child[0].Total + data.Child[1].Total) \/ 2;"}},{"header":"ASH_AR %","attribute":"AR","input":false,"fn":{"arguments":"data","body":"return (data.Child[0].AR + data.Child[1].AR) \/ 2;"}},{"header":"ASH_DB %","attribute":"DB","input":false,"fn":{"arguments":"data","body":"return (data.Child[0].DB + data.Child[1].DB) \/ 2;"}},{"header":"Repeatibility","attribute":"Repeatability","input":false,"fn":{"arguments":"data","body":"return Math.abs(data.Child[0].Total - data.Child[1].Total);"}}],"rulesChild":[{"header":"Type","attribute":"Type","input":false,"fn":null},{"header":"Sample ID","attribute":"Ident","input":false,"fn":null},{"header":"External ID","attribute":"ExtIdent","input":false,"fn":null},{"header":"ASH WT_CRUC","attribute":"M1","input":true,"fn":null},{"header":"ASH WT_CRUC_SMP_BEF","attribute":"M2","input":true,"fn":null},{"header":"ASH WT_SMP","attribute":"M4","input":false,"fn":{"arguments":"data","body":"return parseFloat(data.M2 - data.M1).toFixed(4);"}},{"header":"ASH WT_CRUC_ASH","attribute":"M3","input":true,"fn":null},{"header":"ASH_AD %","attribute":"Total","input":false,"fn":{"arguments":"data","body":"return ((data.M3-data.M1)/(data.M2-data.M1))*100;"}},{"header":"ASH_AR %","attribute":"AR","input":false,"fn":{"arguments":"data","body":"return data.hasOwnProperty(''TM'') ? ((100-data.TM)/(100-data.IM))*data.Total : 0;"}},{"header":"ASH_DB %","attribute":"DB","input":false,"fn":{"arguments":"data","body":"return (100/(100-data.IM))*data.Total;"}},{"header":"Repeatibility","attribute":"Repeatability","input":false,"fn":null}]}'),
	/* TS */ ('JBG', 29, NULL, 0.05, '{"externalAttributes":["TM","IM"],"rules":[{"header":"Type","attribute":"Type","input":false,"fn":null},{"header":"Sample ID","attribute":"Ident","input":false,"fn":null},{"header":"External ID","attribute":"ExtIdent","input":false,"fn":null},{"header":"TS_AD %","attribute":"Total","input":false,"fn":{"arguments":"data","body":"return (data.Child[0].Total + data.Child[1].Total) \/ 2;"}},{"header":"TS_AR %","attribute":"AR","input":false,"fn":{"arguments":"data","body":"return (data.Child[0].AR + data.Child[1].AR) \/ 2;"}},{"header":"TS_DB %","attribute":"DB","input":false,"fn":{"arguments":"data","body":"return (data.Child[0].DB + data.Child[1].DB) \/ 2;"}},{"header":"Repeatibility","attribute":"Repeatability","input":false,"fn":{"arguments":"data","body":"return Math.abs(data.Child[0].Total - data.Child[1].Total);"}}],"rulesChild":[{"header":"Type","attribute":"Type","input":false,"fn":null},{"header":"Sample ID","attribute":"Ident","input":false,"fn":null},{"header":"External ID","attribute":"ExtIdent","input":false,"fn":null},{"header":"TS_AD %","attribute":"Total","input":true,"fn":null},{"header":"TS_AR %","attribute":"AR","input":false,"fn":{"arguments":"data","body":"return data.hasOwnProperty(''TM'') ? ((100-data.TM)/(100-data.IM))*data.Total : 0;"}},{"header":"TS_DB %","attribute":"DB","input":false,"fn":{"arguments":"data","body":"return (100/(100-data.IM))*data.Total;"}},{"header":"Repeatibility","attribute":"Repeatability","input":false,"fn":null}]}'),
	/* CV */ ('JBG', 7, NULL, NULL, '{"externalAttributes":["TM","IM","ASH"],"rules":[{"header":"Type","attribute":"Type","input":false,"fn":null},{"header":"Sample ID","attribute":"Ident","input":false,"fn":null},{"header":"External ID","attribute":"ExtIdent","input":false,"fn":null},{"header":"CV_AD CAL/G","attribute":"Total","input":false,"fn":{"arguments":"data","body":"return (data.Child[0].Total + data.Child[1].Total) \/ 2;"}},{"header":"CV_AR CAL/G","attribute":"AR","input":false,"fn":{"arguments":"data","body":"return (data.Child[0].AR + data.Child[1].AR) \/ 2;"}},{"header":"CV_DB CAL/G","attribute":"DB","input":false,"fn":{"arguments":"data","body":"return (data.Child[0].DB + data.Child[1].DB) \/ 2;"}},{"header":"CV_DAF CAL/G","attribute":"DAF","input":false,"fn":{"arguments":"data","body":"return (data.Child[0].DAF + data.Child[1].DAF) \/ 2;"}},{"header":"Repeatibility","attribute":"Repeatability","input":false,"fn":{"arguments":"data","body":"return Math.abs(data.Child[0].Total - data.Child[1].Total);"}}],"rulesChild":[{"header":"Type","attribute":"Type","input":false,"fn":null},{"header":"Sample ID","attribute":"Ident","input":false,"fn":null},{"header":"External ID","attribute":"ExtIdent","input":false,"fn":null},{"header":"CV_AD CAL/G","attribute":"Total","input":true,"fn":null},{"header":"CV_AR CAL/G","attribute":"AR","input":false,"fn":{"arguments":"data","body":"return data.hasOwnProperty(''TM'') ? ((100-data.TM)/(100-data.IM))*data.Total : 0;"}},{"header":"CV_DB CAL/G","attribute":"DB","input":false,"fn":{"arguments":"data","body":"return (100/(100-data.IM))*data.Total;"}},{"header":"CV_DAF CAL/G","attribute":"DAF","input":false,"fn":{"arguments":"data","body":"return (100/(100-((100/(100-data.IM))*data.ASH)))*data.DB;"}},{"header":"Repeatibility","attribute":"Repeatability","input":false,"fn":null}]}');

-- Add JBG company client project scheme
INSERT INTO CompanyClientProjectScheme
	(CompanyCode, ClientId, ProjectId, SchemeId, IsRequired, CreatedOn, CreatedBy, SchemeOrder)
VALUES
	('JBG', 1, 2, 48, 1, getdate(), 0, 1),	-- Crushing Crushing Plant 2 FM
	('JBG', 1, 2, 12, 1, getdate(), 0, 2),	-- IM
	('JBG', 1, 2, 24, 1, getdate(), 0, 3),	-- RM
	('JBG', 1, 2, 28, 1, getdate(), 0, 4),	-- TM
	('JBG', 1, 2, 4, 1, getdate(), 0, 5),	-- ASH
	('JBG', 1, 2, 29, 1, getdate(), 0, 6),	-- TS
	('JBG', 1, 2, 7, 1, getdate(), 0, 7),	-- CV
	('JBG', 3, 7, 48, 1, getdate(), 0, 1),	-- Loading Barge Loading FM
	('JBG', 3, 7, 12, 1, getdate(), 0, 2),	-- IM
	('JBG', 3, 7, 24, 1, getdate(), 0, 3),	-- RM
	('JBG', 3, 7, 28, 1, getdate(), 0, 4),	-- TM
	('JBG', 3, 7, 4, 1, getdate(), 0, 5),	-- ASH
	('JBG', 3, 7, 29, 1, getdate(), 0, 6),	-- TS
	('JBG', 3, 7, 7, 1, getdate(), 0, 7),	-- CV
	('JBG', 27, 27, 48, 1, getdate(), 0, 1),	-- Shipment Shipment FM
	('JBG', 27, 27, 12, 1, getdate(), 0, 2),	-- IM
	('JBG', 27, 27, 24, 1, getdate(), 0, 3),	-- RM
	('JBG', 27, 27, 28, 1, getdate(), 0, 4),	-- TM
	('JBG', 27, 27, 4, 1, getdate(), 0, 5),		-- ASH
	('JBG', 27, 27, 29, 1, getdate(), 0, 6),	-- TS
	('JBG', 27, 27, 7, 1, getdate(), 0, 7),		-- CV
	('JBG', 4, 10, 48, 1, getdate(), 0, 2),	-- Geology Exploration FM
	('JBG', 4, 10, 12, 1, getdate(), 0, 3),	-- IM
	('JBG', 4, 10, 28, 1, getdate(), 0, 4),	-- TM
	('JBG', 4, 10, 4, 1, getdate(), 0, 5),	-- ASH
	('JBG', 4, 10, 29, 1, getdate(), 0, 6),	-- TS
	('JBG', 4, 10, 7, 1, getdate(), 0, 7),	-- CV
	('JBG', 4, 28, 48, 1, getdate(), 0, 2),	-- Geology Drill Hole FM
	('JBG', 4, 28, 12, 1, getdate(), 0, 3),	-- IM
	('JBG', 4, 28, 28, 1, getdate(), 0, 4),	-- TM
	('JBG', 4, 28, 4, 1, getdate(), 0, 5),	-- ASH
	('JBG', 4, 28, 29, 1, getdate(), 0, 6),	-- TS
	('JBG', 4, 28, 7, 1, getdate(), 0, 7),	-- CV
	('JBG', 12, 26, 48, 1, getdate(), 0, 1),	-- ROM Inspection FM
	('JBG', 12, 26, 12, 1, getdate(), 0, 2),	-- IM
	('JBG', 12, 26, 24, 1, getdate(), 0, 3),	-- RM
	('JBG', 12, 26, 28, 1, getdate(), 0, 4),	-- TM
	('JBG', 12, 26, 4, 1, getdate(), 0, 5),		-- ASH
	('JBG', 12, 26, 29, 1, getdate(), 0, 6),	-- TS
	('JBG', 12, 26, 7, 1, getdate(), 0, 7),		-- CV
	('JBG', 8, 22, 12, 1, getdate(), 0, 1),	-- Check Daily  IM
	('JBG', 8, 22, 4, 1, getdate(), 0, 2),	-- ASH
	('JBG', 8, 22, 29, 1, getdate(), 0, 3),	-- TS
	('JBG', 8, 22, 7, 1, getdate(), 0, 4),	-- CV
	('JBG', 13, 1, 48, 1, getdate(), 0, 1),	-- Rapid Test Crushing Plant FM
	('JBG', 13, 1, 12, 1, getdate(), 0, 2),	-- IM
	('JBG', 13, 1, 24, 1, getdate(), 0, 3),	-- RM
	('JBG', 13, 1, 28, 1, getdate(), 0, 4),	-- TM
	('JBG', 13, 1, 4, 1, getdate(), 0, 5),	-- ASH
	('JBG', 13, 1, 29, 1, getdate(), 0, 6),	-- TS
	('JBG', 13, 1, 7, 1, getdate(), 0, 7),	-- CV
	('JBG', 13, 7, 48, 1, getdate(), 0, 1),	-- Rapid Test Barge Loading FM
	('JBG', 13, 7, 12, 1, getdate(), 0, 2),	-- IM
	('JBG', 13, 7, 24, 1, getdate(), 0, 3),	-- RM
	('JBG', 13, 7, 28, 1, getdate(), 0, 4),	-- TM
	('JBG', 13, 7, 4, 1, getdate(), 0, 5),	-- ASH
	('JBG', 13, 7, 29, 1, getdate(), 0, 6),	-- TS
	('JBG', 13, 7, 7, 1, getdate(), 0, 7),	-- CV
	('JBG', 18, 17, 12, 1, getdate(), 0, 1),	-- Round Robin Proficiency Test IM
	('JBG', 18, 17, 4, 1, getdate(), 0, 2),	-- ASH
	('JBG', 18, 17, 29, 1, getdate(), 0, 3),	-- TS
	('JBG', 18, 17, 7, 1, getdate(), 0, 4),	-- CV
	('JBG', 18, 18, 12, 1, getdate(), 0, 1),	-- Round Robin Geoservices IM
	('JBG', 18, 18, 4, 1, getdate(), 0, 2),	-- ASH
	('JBG', 18, 18, 29, 1, getdate(), 0, 3),	-- TS
	('JBG', 18, 18, 7, 1, getdate(), 0, 4),	-- CV
	('JBG', 22, 16, 48, 1, getdate(), 0, 1),	-- Special Special FM
	('JBG', 22, 16, 12, 1, getdate(), 0, 2),	-- IM
	('JBG', 22, 16, 24, 1, getdate(), 0, 3),	-- RM
	('JBG', 22, 16, 28, 1, getdate(), 0, 4),	-- TM
	('JBG', 22, 16, 4, 1, getdate(), 0, 5),		-- ASH
	('JBG', 22, 16, 29, 1, getdate(), 0, 6),	-- TS
	('JBG', 22, 16, 7, 1, getdate(), 0, 7);		-- CV

/*Down Script*/

-- Delete JBG Project
DELETE FROM Project WHERE Id IN (27, 28);

-- Delete JBG Client
DELETE FROM Client WHERE Id IN (27);

-- Delete JBG Scheme
DELETE FROM Scheme WHERE Id IN (48, 4, 29, 7);

-- Delete JBG Company Project
DELETE FROM CompanyProject
WHERE CompanyCode = 'JBG'
AND ProjectId IN (2, 7, 27, 10, 28, 26, 22, 1, 17, 18, 16);

-- Delete JBG Company Scheme
DELETE FROM CompanyScheme
WHERE CompanyCode = 'JBG'
AND SchemeId IN (48, 24, 28, 12, 4, 29, 7, 4, 29, 7);

-- Delete JBG CompanyClientProjectScheme
DELETE FROM CompanyClientProjectScheme
WHERE CompanyCode = 'JBG'
AND ClientId IN (1, 3, 27, 4, 12, 8, 13, 18, 22);