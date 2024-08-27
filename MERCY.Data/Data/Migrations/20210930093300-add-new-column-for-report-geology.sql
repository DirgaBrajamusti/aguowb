/*Up Script */
ALTER TABLE dbo.UPLOAD_Geology_Explorasi ADD
	DateReceived datetime NULL,
	DateAnalysis datetime NULL,
	ThicknessFrom decimal(10,5) NULL,
	ThicknessTo decimal(10,5) NULL;


ALTER TABLE dbo.UPLOAD_Geology_Pit_Monitoring ADD
	DateReceived datetime NULL,
	DateAnalysis datetime NULL,
	ThicknessFrom decimal(10,5) NULL,
	ThicknessTo decimal(10,5) NULL;

ALTER TABLE dbo.UPLOAD_CRUSHING_PLANT_Header ADD ReportNumber varchar(100) NULL;


/*Drop Script*/

ALTER TABLE dbo.UPLOAD_Geology_Explorasi DROP COLUMN
	DateAnalysis, DateReceived, ThicknessTo, ThicknessFrom;

ALTER TABLE dbo.UPLOAD_Geology_Pit_Monitoring DROP COLUMN
	DateAnalysis, DateReceived, ThicknessTo, ThicknessFrom;

ALTER TABLE dbo.UPLOAD_CRUSHING_PLANT_Header DROP COLUMN ReportNumber ;

