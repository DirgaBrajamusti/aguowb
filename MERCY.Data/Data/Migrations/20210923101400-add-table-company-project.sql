/*Up Script */
CREATE TABLE CompanyProject (

	Id int IDENTITY(0,1) NOT NULL,
	CompanyCode varchar(32) NOT NULL,
	ProjectId int NOT NULL,
	CreatedOn datetime DEFAULT getdate() NOT NULL,
	CreatedBy int DEFAULT 0 NOT NULL,
	LastModifiedOn datetime NULL,
	LastModifiedBy int NULL,

	CONSTRAINT PK_CompanyProjectId PRIMARY KEY (Id),
	CONSTRAINT FK_CompanyProject_CompanyCode FOREIGN KEY (CompanyCode) REFERENCES dbo.Company(CompanyCode),
	CONSTRAINT FK_CompanyProject_ProjectId FOREIGN KEY (ProjectId) REFERENCES dbo.Project(Id)

);

/*Drop Script */

ALTER TABLE dbo.CompanyProject
DROP CONSTRAINT FK_CompanyProject_ProjectId, FK_CompanyProject_CompanyCode;

DROP TABLE CompanyProject;
