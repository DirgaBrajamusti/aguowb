/* Up Script*/
-- Add the columns
ALTER TABLE dbo.Instrument 
ADD SiteId INT DEFAULT NULL,
    CompanyCode VARCHAR(32);

-- Add the foreign key constraint
ALTER TABLE dbo.Instrument 
ADD CONSTRAINT FK_Instrument_SiteId
FOREIGN KEY (SiteId) REFERENCES Site(SiteId);

/*Down Script*/
-- Drop the foreign key constraint
ALTER TABLE dbo.Instrument
DROP CONSTRAINT FK_Instrument_SiteId;

-- Drop the columns
ALTER TABLE dbo.Instrument
DROP COLUMN SiteId;
ALTER TABLE dbo.Instrument
DROP COLUMN CompanyCode;
