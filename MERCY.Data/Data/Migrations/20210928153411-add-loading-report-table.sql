/*Up Script */
CREATE TABLE LoadingReportHeader (
	Id int IDENTITY(0, 1) NOT NULL,
	ReportNumber varchar(255) NOT NULL,
	ReportTo varchar(255) NOT NULL,
	ReportedDate datetime NOT NULL,
	StandardMethod varchar(255) NOT NULL,
	CompanyCode varchar(32) NOT NULL,
	CreatedOn datetime DEFAULT getdate() NOT NULL,
	CreatedOnDate varchar(255) NOT NULL,
	CreatedOnYear int NOT NULL,
	CreatedBy int DEFAULT 0 NOT NULL,
	LastModifiedOn datetime NULL,
	LastModifiedBy int NULL,
  CONSTRAINT [PK_LoadingReportHeaderId] PRIMARY KEY CLUSTERED ([Id] ASC)
		WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
);

CREATE TABLE LoadingReport (
	Id int IDENTITY(0,1) NOT NULL,
	HeaderId int NOT NULL,
	VesselName varchar(255) NOT NULL,
	LoadingNumber varchar(255) NOT NULL,
	LotNumber int NOT NULL,
	SamplingDateStart datetime NOT NULL,
	SamplingDateEnd datetime NOT NULL,
	AnalysisDateStart datetime NOT NULL,
	AnalysisDateEnd datetime NOT NULL,
	TM numeric(18, 5) NOT NULL DEFAULT 0,
	IM numeric(18, 5) NOT NULL DEFAULT 0,
	ASH numeric(18, 5) NOT NULL DEFAULT 0,
	VM numeric(18, 5) NOT NULL DEFAULT 0,
	FC numeric(18, 5) NOT NULL DEFAULT 0,
	TS numeric(18, 5) NOT NULL DEFAULT 0,
	CVAdb numeric(18, 5) NOT NULL DEFAULT 0,
	CVArb numeric(18, 5) NOT NULL DEFAULT 0,
	CVDb numeric(18, 5) NOT NULL DEFAULT 0,
	CVDaf numeric(18, 5) NOT NULL DEFAULT 0,
	Tonnage numeric(18, 5) NOT NULL DEFAULT 0,
	Size50 numeric(18, 5) NOT NULL DEFAULT 0,
	Size502 numeric(18, 5) NOT NULL DEFAULT 0,
	Size2 numeric(18, 5) NOT NULL DEFAULT 0,
	CreatedOn datetime DEFAULT getdate() NOT NULL,
	CreatedOnDate varchar(255) NOT NULL,
	CreatedOnYear int NOT NULL,
	CreatedBy int DEFAULT 0 NOT NULL,
	LastModifiedOn datetime NULL,
	LastModifiedBy int NULL,
	CONSTRAINT [FK_LoadingReport_HeaderId] FOREIGN KEY ([HeaderId]) REFERENCES [dbo].[LoadingReportHeader] ([Id]),
  CONSTRAINT [PK_LoadingReport_Id] PRIMARY KEY CLUSTERED ([Id] ASC)
		WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
);

/*Drop Script */
DROP TABLE LoadingReport;
DROP TABLE LoadingReportHeader;
