/*Up Script.*/
CREATE TABLE SampleDetailGeneral (
	Id int IDENTITY(0,1) NOT NULL,
	GeoPrefix varchar(50) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
	External_Id varchar(50) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
	Shift int NOT NULL,
	[Sequence] int NOT NULL,
	DateSampleStart datetime NOT NULL,
	DateSampleEnd datetime NOT NULL,
	Receive datetime NOT NULL,
	Tonnage varchar(50) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
	CoalType varchar(50) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
	Sample_Id int NOT NULL,
	BargeName varchar(50) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
	Destination varchar(50) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
	Customer varchar(50) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
	Vessel varchar(50) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
	Surveyor varchar(50) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
	ShipmentNumber varchar(50) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
	CreatedBy int NOT NULL,
	CreatedOn datetime NOT NULL,
	LastModifiedBy int NULL,
	LastModifiedOn datetime NULL,
	CONSTRAINT PK_Sample_Detail_General PRIMARY KEY (Id),
);
ALTER TABLE SampleDetailGeneral WITH CHECK ADD CONSTRAINT FK_SampleDetailGeneral_Sample_SampleId 
FOREIGN KEY (Sample_Id) REFERENCES Sample(Id) ON DELETE CASCADE;
ALTER TABLE SampleDetailGeneral CHECK CONSTRAINT FK_SampleDetailGeneral_Sample_SampleId;

/*Down Script*/
ALTER TABLE SampleDetailGeneral
DROP CONSTRAINT FK_SampleDetailGeneral_Sample_SampleId;

DROP TABLE SampleDetailGeneral;