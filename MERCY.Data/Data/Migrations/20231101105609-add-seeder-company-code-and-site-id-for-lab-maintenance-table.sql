WITH site_melak AS (
    SELECT s.SiteId 
    FROM Site s 
    WHERE SiteName = 'Melak'
)

update LabMaintenance  
set SiteId = (SELECT SiteId from site_melak), CompanyCode = 'TCM'
WHERE SiteId is NULL and CompanyCode is NULL 