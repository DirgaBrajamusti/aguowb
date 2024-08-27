/* Up Script*/
ALTER TABLE LabMaintenance  
ADD CompanyCode VARCHAR(32),
    SiteId INT DEFAULT NULL;

ALTER TABLE LabMaintenance 
ADD CONSTRAINT FK_LabMaintenanceSiteId
FOREIGN KEY ([SiteId]) REFERENCES Site([SiteId]);

/*Down Script*/
-- Drop the foreign key constraint
ALTER TABLE LabMaintenance
DROP CONSTRAINT FK_LabMaintenanceSiteId;

-- Drop the columns
ALTER TABLE LabMaintenance
DROP COLUMN CompanyCode;

ALTER TABLE LabMaintenance
DROP COLUMN SiteId;