
ALTER TABLE dbo.Sample ADD 
	IsActive bit NULL DEFAULT 1,
	Location varchar(50) NULL,
	Remark varchar(255) NULL,
	ThicknessTo decimal(9,5) NULL,
	ThicknessFrom decimal(9,5) NULL,
	Thickness decimal(9,5) NULL;


/*DownScript*/

ALTER TABLE dbo.Sample DROP COLUMN 
	IsActive,
	Remark,
	Location,
	Thickness,
	ThicknessFrom,
	ThicknessTo;