/* Up Script*/
ALTER TABLE MaintenanceActivity 
ADD CompanyCode VARCHAR(32),
    SiteId INT DEFAULT NULL;

ALTER TABLE MaintenanceActivity 
ADD CONSTRAINT FK_MaintenanceActivitySiteId
FOREIGN KEY ([SiteId]) REFERENCES Site([SiteId]);

/*Down Script*/
-- Drop the foreign key constraint
ALTER TABLE MaintenanceActivity
DROP CONSTRAINT FK_MaintenanceActivitySiteId;

-- Drop the columns
ALTER TABLE MaintenanceActivity
DROP COLUMN CompanyCode;

ALTER TABLE MaintenanceActivity
DROP COLUMN SiteId;