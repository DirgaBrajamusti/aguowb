/* Up Script*/
-- Add the columns
ALTER TABLE dbo.Consumable 
ADD SiteId INT DEFAULT NULL,
    CompanyCode VARCHAR(32);

-- Add the foreign key constraint
ALTER TABLE dbo.Consumable 
ADD CONSTRAINT FK_Consumable_SiteId
FOREIGN KEY (SiteId) REFERENCES Site(SiteId);

/*Down Script*/
-- Drop the foreign key constraint
ALTER TABLE dbo.Consumable
DROP CONSTRAINT FK_Consumable_SiteId;

-- Drop the columns
ALTER TABLE dbo.Consumable
DROP COLUMN SiteId;
ALTER TABLE dbo.Consumable
DROP COLUMN CompanyCode;
