/*UP SCRIPT*/
CREATE TABLE Analyst_Job (
	Id int IDENTITY(0,1) NOT NULL,
	JobNumber varchar(50) NOT NULL,
	JobDate date NOT NULL,
	Status varchar(50) NOT NULL,
	ReceivedDate datetime NULL,
	CompletedDate datetime NULL,
	ApprovedDate datetime NULL,
	ValidatedDate datetime NULL,
	ProjectId int NULL,
	CreatedAt datetime NOT NULL,
	CreatedBy int NOT NULL DEFAULT 0,
	LastModifiedBy int NULL,
	LastModifiedAt datetime NULL,
	CONSTRAINT PK_Analyst_Job PRIMARY KEY (Id),
	CONSTRAINT FK_Sample_Analyst_Job_Project FOREIGN KEY (ProjectId) REFERENCES dbo.Project(Id)
);

ALTER TABLE dbo.Sample
DROP CONSTRAINT FK_Sample_JobNumberId;

DROP TABLE JobNumber;

ALTER TABLE dbo.Sample WITH CHECK ADD CONSTRAINT FK_Sample_AnalystJob FOREIGN KEY (JobNumberId) REFERENCES Analyst_Job(Id);
ALTER TABLE dbo.Sample CHECK CONSTRAINT FK_Sample_AnalystJob;

/*Down Script*/

ALTER TABLE dbo.Analyst_Job
DROP CONSTRAINT FK_Sample_Analyst_Job_Project;

ALTER TABLE dbo.Sample
DROP CONSTRAINT FK_Sample_AnalystJob;


CREATE TABLE JobNumber(
	Id int IDENTITY(0,1) NOT NULL,
	Name varchar(50) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
)

ALTER TABLE dbo.Sample WITH CHECK ADD CONSTRAINT FK_Sample_JobNumberId 
FOREIGN KEY (JobNumberId) REFERENCES JobNumber(Id);
ALTER TABLE dbo.Sample CHECK CONSTRAINT FK_Sample_JobNumberId;

DROP TABLE Analyst_Job;