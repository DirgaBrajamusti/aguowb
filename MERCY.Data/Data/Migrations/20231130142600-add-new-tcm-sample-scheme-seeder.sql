/* Up Script*/

-- Add new projects for TCM
SET IDENTITY_INSERT Project ON;
INSERT INTO Project
	(Id, Name, CreatedOn, CreatedBy, Code, Type)
VALUES
	(29, 'AMD Laboratory', getdate(), 0, 'AMD', 'AMD');
SET IDENTITY_INSERT Project OFF;

-- Add new schemes for TCM
SET IDENTITY_INSERT Scheme ON;
INSERT INTO Scheme
	(Id, Name, CreatedOn, CreatedBy, Type)
VALUES
	(50, 'AMD Other', getdate(), 0, 'SIMPLO');
SET IDENTITY_INSERT Scheme OFF;

-- Add TCM company projects
INSERT INTO CompanyProject
	(CompanyCode, ProjectId, ProjectType, CreatedOn, CreatedBy)
VALUES
	('TCM', 29, 'AMD', getdate(), 0);	-- AMD Laboratory

-- Add TCM company scheme and formula
INSERT INTO CompanyScheme
	(CompanyCode, SchemeId, MinRepeatability, MaxRepeatability, Details)
VALUES
	/* AMD Other */ ('TCM', 50, NULL, NULL, '{ "externalAttributes": [], "rules": [{ "header": "Type", "attribute": "Type", "input": false, "fn": null }, { "header": "Sample ID", "attribute": "Ident", "input": false, "fn": null }, { "header": "External ID", "attribute": "ExtIdent", "input": false, "fn": null }], "rulesChild": null }');

-- Add TCM company client project scheme
INSERT INTO CompanyClientProjectScheme
	(CompanyCode, ClientId, ProjectId, SchemeId, IsRequired, CreatedOn, CreatedBy, SchemeOrder)
VALUES
	('TCM', 4, 29, 50, 1, getdate(), 0, 1);		-- AMD Other
SET IDENTITY_INSERT RefType ON;
INSERT INTO RefType
	(Id, Name, CreatedOn, CreatedBy)
VALUES
	(3, 'EGI', getdate(), 0); -- EGI Ref Type

SET IDENTITY_INSERT RefType OFF;

INSERT INTO CompanyRefType
	(CompanyCode, RefTypeId, CreatedOn, CreatedBy)
VALUES 
	('TCM', 3, getdate(), 0);

/*Down Script*/

-- Delete TCM Company Project
DELETE FROM CompanyProject
WHERE CompanyCode = 'TCM'
AND ProjectId IN (29);

-- Delete TCM Company Scheme
DELETE FROM CompanyScheme
WHERE CompanyCode = 'TCM'
AND SchemeId IN (50);

-- Delete TCM CompanyClientProjectScheme
DELETE FROM CompanyClientProjectScheme
WHERE CompanyCode = 'TCM'
AND ClientId IN (4);

-- Delete TCM Project
DELETE FROM Project WHERE Id IN (29);

-- Delete TCM Scheme
DELETE FROM Scheme WHERE Id IN (50);

-- Delete TCM CompanyRefType
DELETE FROM CompanyRefType WHERE RefTypeId = 3;

-- Delete TCM RefType
DELETE FROM RefType WHERE Id = 3;