/* Up Script*/
ALTER TABLE dbo.Consumable 
ALTER COLUMN SiteId INT NOT NULL;
ALTER TABLE dbo.Consumable 
ALTER COLUMN CompanyCode VARCHAR(32) NOT NULL;

/*Down Script*/
ALTER TABLE dbo.Consumable
ALTER COLUMN SiteId INT NULL;
ALTER TABLE dbo.Consumable 
ALTER COLUMN CompanyCode VARCHAR(32) NULL;
