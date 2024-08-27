/* Up Script */
ALTER TABLE dbo.PortionBlending
ADD 
	ROM_ID int default NULL,
	ROM_Name VARCHAR(255),
	TotalBucket int default NULL;

/* Down Script */
ALTER TABLE dbo.PortionBlending
DROP COLUMN
	ROM_ID,
    ROM_Name,
    TotalBucket;