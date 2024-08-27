/* Up Script*/
WITH site_melak AS (
    SELECT s.SiteId 
    FROM Site s 
    WHERE SiteName = 'Melak'
)

UPDATE dbo.Instrument 
SET SiteId = (SELECT SiteId FROM site_melak), CompanyCode = 'TCM'
WHERE SiteId is NULL and CompanyCode is NULL 