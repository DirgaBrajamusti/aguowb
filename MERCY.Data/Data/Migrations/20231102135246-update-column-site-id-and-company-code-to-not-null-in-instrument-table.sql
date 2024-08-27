/* Up Script*/
ALTER TABLE dbo.Instrument 
ALTER COLUMN SiteId INT NOT NULL;
ALTER TABLE dbo.Instrument 
ALTER COLUMN CompanyCode VARCHAR(32) NOT NULL;

/*Down Script*/
ALTER TABLE dbo.Instrument
ALTER COLUMN SiteId INT NULL;
ALTER TABLE dbo.Instrument 
ALTER COLUMN CompanyCode VARCHAR(32) NULL;
