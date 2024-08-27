/*UpScript*/
ALTER TABLE dbo.CompanyProject ADD 
	ProjectType varchar(50) NULL;
/*DownScript*/
ALTER TABLE dbo.CompanyProject DROP COLUMN
	ProjectType;