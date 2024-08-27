/* Up Script*/
Alter Table dbo.Sample_Detail_General
ALTER COLUMN Tonnage int NOT NULL;

/*Down Script*/
ALTER TABLE dbo.Sample_Detail_General
ALTER COLUMN Tonnage varchar(50) NOT NULL;