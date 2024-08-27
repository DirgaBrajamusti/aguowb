/*Up Script*/
ALTER TABLE dbo.Sample_Detail_General
DROP COLUMN CoalType;

ALTER TABLE dbo.Sample_Detail_General
DROP CONSTRAINT FK_SampleDetailGeneral_Sample_SampleId;

EXEC sp_rename 'dbo.Sample_Detail_General.Sample_Id', 'SampleId', 'COLUMN';
EXEC sp_rename 'dbo.Sample_Detail_General.External_Id', 'ExternalId', 'COLUMN';

ALTER TABLE dbo.Sample
ADD CONSTRAINT UN_Sample UNIQUE (SampleId);

ALTER TABLE dbo.Sample_Detail_General WITH CHECK ADD CONSTRAINT FK_SampleDetailGeneral_Sample_SampleId 
FOREIGN KEY (SampleId) REFERENCES dbo.Sample(Id);
ALTER TABLE dbo.Sample_Detail_General CHECK CONSTRAINT FK_SampleDetailGeneral_Sample_SampleId;

/*Down Script*/
ALTER TABLE dbo.Sample_Detail_General
ADD COLUMN 	CoalType varchar(50) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL;

ALTER TABLE dbo.Sample_Detail_General
DROP CONSTRAINT FK_SampleDetailGeneral_Sample_SampleId;

ALTER TABLE dbo.Sample
DROP CONSTRAINT UN_Sample;

EXEC sp_rename 'dbo.Sample_Detail_General.SampleId', 'Sample_Id', 'COLUMN';
EXEC sp_rename 'dbo.Sample_Detail_General.ExternalId', 'External_Id', 'COLUMN';

ALTER TABLE dbo.Sample_Detail_General WITH CHECK ADD CONSTRAINT FK_SampleDetailGeneral_Sample_SampleId
FOREIGN KEY(Sample_Id) REFERENCES dbo.Sample(Id);
ALTER TABLE dbo.Sample_Detail_General CHECK CONSTRAINT FK_SampleDetailGeneral_Sample_SampleId;
