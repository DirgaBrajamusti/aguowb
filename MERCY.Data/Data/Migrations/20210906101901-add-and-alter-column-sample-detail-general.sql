/*Up Script */
ALTER TABLE dbo.Sample_Detail_General 
ALTER COLUMN BargeName varchar(50) COLLATE SQL_Latin1_General_CP1_CI_AS NULL;
ALTER TABLE dbo.Sample_Detail_General 
ALTER COLUMN Destination varchar(50) COLLATE SQL_Latin1_General_CP1_CI_AS NULL;
ALTER TABLE dbo.Sample_Detail_General 
ALTER COLUMN TripNumber varchar(50) COLLATE SQL_Latin1_General_CP1_CI_AS NULL;
ALTER TABLE dbo.Sample_Detail_General 
ALTER COLUMN GeoPrefix varchar(50) COLLATE SQL_Latin1_General_CP1_CI_AS NULL;

CREATE TABLE dbo.JobNumber (
	id int IDENTITY(0,1) NOT NULL,
	Name varchar(50) NOT NULL
);

ALTER TABLE dbo.JobNumber 
ADD CONSTRAINT PK_JobNumber PRIMARY KEY (Id);

ALTER TABLE dbo.Sample 
ADD JobNumberId int NOT NULL ;

ALTER TABLE dbo.Sample 
WITH CHECK ADD CONSTRAINT FK_Sample_JobNumberId
FOREIGN KEY(JobNumberId) REFERENCES dbo.JobNumber(Id);
ALTER TABLE dbo.Sample CHECK CONSTRAINT FK_Sample_JobNumberId;

EXEC sys.sp_rename N'dbo.Sample.SampleId' , N'ExternalId', 'COLUMN';

EXEC sys.sp_rename N'dbo.Sample_Detail_General.SampleId' , N'Sample_Id', 'COLUMN';

EXEC sys.sp_rename N'dbo.Sample_Detail_General.ExternalId' , N'SampleId', 'COLUMN';


/*Down Script*/

ALTER TABLE dbo.Sample_Detail_General 
ALTER COLUMN BargeName varchar(50) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL;
ALTER TABLE dbo.Sample_Detail_General 
ALTER COLUMN Destination varchar(50) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL;
ALTER TABLE dbo.Sample_Detail_General 
ALTER COLUMN TripNumber varchar(50) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL;
ALTER TABLE dbo.Sample_Detail_General 
ALTER COLUMN GeoPrefix varchar(50) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL;

ALTER TABLE dbo.Sample
DROP CONSTRAINT FK_Sample_JobNumberId;

ALTER TABLE dbo.Sample
DROP COLUMN JobNumberId;

DROP TABLE dbo.JobNumber;

EXEC sys.sp_rename N'dbo.Sample.ExternalId' , N'SampleId', 'COLUMN';

EXEC sys.sp_rename N'dbo.Sample_Detail_General.SampleId' , N'ExternalId', 'COLUMN';


EXEC sys.sp_rename N'dbo.Sample_Detail_General.Sample_Id' , N'SampleId', 'COLUMN';
