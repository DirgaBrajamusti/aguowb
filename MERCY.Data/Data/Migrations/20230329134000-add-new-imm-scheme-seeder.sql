/* Up Script*/

-- Add new schemes for IMM
SET IDENTITY_INSERT Scheme ON;
INSERT INTO Scheme
	(Id, Name, CreatedOn, CreatedBy, Type)
VALUES
	(49, 'AAS Coal', getdate(), 0, 'SIMPLO');
SET IDENTITY_INSERT Scheme OFF;

-- Add IMM new company scheme and formula
INSERT INTO CompanyScheme
	(CompanyCode, SchemeId, MinRepeatability, MaxRepeatability, Details)
VALUES
	/* AAS Coal */ ('IMM', 49, NULL, NULL, '{"externalAttributes":[],"rules":[{"header":"Type","attribute":"Type","input":false,"fn":null},{"header":"External ID","attribute":"Ident","input":false,"fn":null},{"header":"Sample ID","attribute":"ExtIdent","input":false,"fn":null},{"header":"AA Na2O %","attribute":"Total","input":true,"fn":null},{"header":"AA CaO%","attribute":"M2","input":true,"fn":null}],"rulesChild":null}')

-- Add IMM new  company client project scheme
INSERT INTO CompanyClientProjectScheme
	(CompanyCode, ClientId, ProjectId, SchemeId, IsRequired, CreatedOn, CreatedBy, SchemeOrder)
VALUES
	('IMM', 1, 3, 49, 0, getdate(), 0, 1),	 
	('IMM', 1, 4, 49, 0, getdate(), 0, 1),
	('IMM', 2, 6, 49, 0, getdate(), 0, 1),	
	('IMM', 4, 9, 49, 0, getdate(), 0, 1),	 
	('IMM', 4, 10, 49, 1, getdate(), 0, 1),	 
	('IMM', 4, 11, 49, 1, getdate(), 0, 1),	 
	('IMM', 5, 17, 49, 0, getdate(), 0, 1),	 
	('IMM', 7, 19, 49, 0, getdate(), 0, 1),	 
	('IMM', 8, 22, 49, 0, getdate(), 0, 1),	 
	('IMM', 9, 16, 49, 0, getdate(), 0, 1),	 
	('IMM', 10,24, 49, 0, getdate(), 0, 1),	 
	('IMM', 11, 25, 49, 0, getdate(), 0, 1)	 

DELETE FROM CompanyClientProjectScheme
WHERE CompanyCode = 'IMM'
AND SchemeId IN (1, 18)
/*Down Script*/

-- Delete IMM Scheme
DELETE FROM Scheme WHERE Id = 9;

-- Delete IMM Company Scheme
DELETE FROM CompanyScheme
WHERE CompanyCode = 'IMM'
AND SchemeId = 49;

-- Delete JBG CompanyClientProjectScheme
DELETE FROM CompanyClientProjectScheme
WHERE CompanyCode = 'IMM'
AND ClientId = 3
AND SchemeId = 49;

INSERT INTO CompanyClientProjectScheme
	(CompanyCode, ClientId, ProjectId, SchemeId, IsRequired, CreatedOn, CreatedBy, SchemeOrder)
VALUES
	('IMM', 1, 3, 1, 0, getdate(), 0, 1),	 
	('IMM', 1, 4, 1, 0, getdate(), 0, 1),	 
	('IMM', 2, 6, 1, 0, getdate(), 0, 1),	 
	('IMM', 4, 9, 1, 0, getdate(), 0, 1),	 
	('IMM', 4, 10, 1, 1, getdate(), 0, 1),	 
	('IMM', 4, 11, 1, 1, getdate(), 0, 1),	 
	('IMM', 5, 17, 1, 0, getdate(), 0, 1),	 
	('IMM', 7, 19, 1, 0, getdate(), 0, 1),	 
	('IMM', 8, 22, 1, 0, getdate(), 0, 1),	 
	('IMM', 9, 16, 1, 0, getdate(), 0, 1),	 
	('IMM', 10,24, 1, 0, getdate(), 0, 1),	 
	('IMM', 11, 25, 1, 0, getdate(), 0, 1),	 

	('IMM', 1, 3, 18, 0, getdate(), 0, 1),	 
	('IMM', 1, 4, 18, 0, getdate(), 0, 1),	 
	('IMM', 2, 6, 18, 0, getdate(), 0, 1),	 
	('IMM', 4, 9, 18, 0, getdate(), 0, 1),	 
	('IMM', 4, 10, 18, 1, getdate(), 0, 1),	 
	('IMM', 4, 11, 18, 1, getdate(), 0, 1),	 
	('IMM', 5, 17, 18, 0, getdate(), 0, 1),	 
	('IMM', 7, 19, 18, 0, getdate(), 0, 1),	 
	('IMM', 8, 22, 18, 0, getdate(), 0, 1),	 
	('IMM', 9, 16, 18, 0, getdate(), 0, 1),	 
	('IMM', 10,24, 18, 0, getdate(), 0, 1),	 
	('IMM', 11, 25, 18, 0, getdate(), 0, 1)	 
