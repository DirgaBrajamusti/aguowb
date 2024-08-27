/* Up Script*/
ALTER TABLE dbo.MaintenanceActivity  
ALTER COLUMN SiteId INT NOT NULL;
ALTER TABLE dbo.MaintenanceActivity 
ALTER COLUMN CompanyCode VARCHAR(32) NOT NULL;


ALTER TABLE dbo.LabMaintenance  
ALTER COLUMN SiteId INT NOT NULL;
ALTER TABLE dbo.LabMaintenance 
ALTER COLUMN CompanyCode VARCHAR(32) NOT NULL;

/*Down Script*/
ALTER TABLE dbo.MaintenanceActivity
ALTER COLUMN SiteId INT NULL;
ALTER TABLE dbo.MaintenanceActivity 
ALTER COLUMN CompanyCode VARCHAR(32) NULL;

ALTER TABLE dbo.LabMaintenance
ALTER COLUMN SiteId INT NULL;
ALTER TABLE dbo.LabMaintenance 
ALTER COLUMN CompanyCode VARCHAR(32) NULL;