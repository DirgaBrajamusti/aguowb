/* Up script. */
CREATE TABLE [dbo].[UPLOAD_Geology_AMD_Header](
	[RecordId] [int] IDENTITY(1,1) NOT NULL,
	[CompanyCode] [varchar](50) NOT NULL,
	[Date_Detail] [varchar](50) NOT NULL,
	[Job_No] [varchar](50) NOT NULL,
	[Report_To] [varchar](255) NOT NULL,
	[Date_Received] [varchar](50) NOT NULL,
	[Nomor] [varchar](100) NOT NULL,
	[CreatedBy] [int] NOT NULL,
	[CreatedOn] [datetime] NOT NULL,
	[LastModifiedBy] [int] NULL,
	[LastModifiedOn] [datetime] NULL,
 CONSTRAINT [PK_UPLOAD_Geology_AMD_Header] PRIMARY KEY CLUSTERED 
(
	[RecordId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY];
ALTER TABLE [dbo].[UPLOAD_Geology_AMD_Header] ADD  CONSTRAINT [DF__UploadGeologyAMDHeader__CreatedOn]  DEFAULT (getdate()) FOR [CreatedOn];
ALTER TABLE [dbo].[UPLOAD_Geology_AMD_Header] ADD  CONSTRAINT [DF__UploadGeologyAMDHeader__CreatedBy]  DEFAULT ((0)) FOR [CreatedBy];

CREATE TABLE [dbo].[UPLOAD_Geology_AMD](
	[RecordId] [int] IDENTITY(1,1) NOT NULL,
	[CompanyCode] [varchar](50) NOT NULL,
	[SampleId] [varchar](50) NOT NULL,
	[Sample_Id] [int] NOT NULL,
	[SampleType] [varchar](50) NOT NULL,
	[LaboratoryId] [varchar](50) NOT NULL,
	[MassSampleReceived] [decimal](18,4) NOT NULL,
	[TS] [decimal](18,5) NOT NULL,
	[MPA] [decimal](18,5) NOT NULL,
	[ANC] [decimal](18,5) NOT NULL,
	[NAPP] [decimal](18,5) NOT NULL,
	[NAG] [decimal](18,5) NOT NULL,
	[NAGPH45] [varchar](50) NOT NULL,
	[NAGPH70] [varchar](50) NOT NULL,
	[NAGType] [varchar](50) NOT NULL,
	[DateReceived] [datetime] NULL,
	[Header] [int] NOT NULL,
	[CreatedBy] [int] NOT NULL,
	[CreatedOn] [datetime] NOT NULL,
	[LastModifiedBy] [int] NULL,
	[LastModifiedOn] [datetime] NULL,
 CONSTRAINT [PK_UPLOAD_Geology_AMD] PRIMARY KEY CLUSTERED 
(
	[RecordId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY];
ALTER TABLE [dbo].[UPLOAD_Geology_AMD] ADD  CONSTRAINT [DF__UploadGeologyAMD__CreatedOn]  DEFAULT (getdate()) FOR [CreatedOn];
ALTER TABLE [dbo].[UPLOAD_Geology_AMD] ADD  CONSTRAINT [DF__UploadGeologyAMD__CreatedBy]  DEFAULT ((0)) FOR [CreatedBy];
ALTER TABLE [dbo].[UPLOAD_Geology_AMD] WITH CHECK
	ADD CONSTRAINT [FK_UPLOADGeologyAMD_UPLOADGeologyAMDHeader]
	FOREIGN KEY ([Header]) REFERENCES [dbo].[UPLOAD_Geology_AMD_Header]([RecordId]) ON DELETE CASCADE;
ALTER TABLE [dbo].[UPLOAD_Geology_AMD] CHECK CONSTRAINT [FK_UPLOADGeologyAMD_UPLOADGeologyAMDHeader];
/*Down script. */
ALTER TABLE [dbo].[UPLOAD_Geology_AMD] DROP CONSTRAINT [FK_UPLOADGeologyAMD_UPLOADGeologyAMDHeader];
DROP TABLE [dbo].[UPLOAD_Geology_AMD];
DROP TABLE [dbo].[UPLOAD_Geology_AMD_Header];