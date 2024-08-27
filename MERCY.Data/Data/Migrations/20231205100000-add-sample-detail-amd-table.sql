/* Up script. */
CREATE TABLE [dbo].[SampleDetailAMD](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[GeoPrefix] [varchar](50) NOT NULL,
	[SampleId] [varchar](50) NOT NULL,
	[Shift] [int] NOT NULL,
	[Sequence] [int] NOT NULL,
	[LaboratoryId] [varchar](50) NOT NULL,
	[SampleType] [varchar](50) NOT NULL,
	[DateSampleeStart] [datetime] NOT NULL,
	[DateSampleEnd] [datetime] NOT NULL,
	[Receive] [datetime] NOT NULL,
	[MassSampleReceived] [decimal](18, 5) NOT NULL,
	[TS] [decimal](18,5) NOT NULL,
	[ANC] [decimal](18,5) NOT NULL,
	[NAG] [decimal](18,5) NOT NULL,
	[NAGPH45] [varchar](50) NOT NULL,
	[NAGPH70] [varchar](50) NOT NULL,
	[NAGType] [varchar](50) NOT NULL,
	[Location] [varchar](50) NOT NULL,
	[Remark] [varchar](255) NULL,
	[Sample_Id] [int] NOT NULL,
	[CreatedBy] [int] NOT NULL,
	[CreatedOn] [datetime] NOT NULL,
	[LastModifiedBy] [int] NULL,
	[LastModifiedOn] [datetime] NULL,
 CONSTRAINT [PK_Sample_Detail_AMD] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY];
ALTER TABLE [dbo].[SampleDetailAMD] ADD  CONSTRAINT [DF__SampleAMD__CreatedOn]  DEFAULT (getdate()) FOR [CreatedOn];
ALTER TABLE [dbo].[SampleDetailAMD] ADD  CONSTRAINT [DF__SampleAMD__CreatedBy]  DEFAULT ((0)) FOR [CreatedBy];
ALTER TABLE [dbo].[SampleDetailAMD]  WITH CHECK
	ADD CONSTRAINT [FK_SampleAMD_SampleId]
	FOREIGN KEY([Sample_Id]) REFERENCES [dbo].[Sample] ([Id]) ON DELETE CASCADE;
ALTER TABLE [dbo].[SampleDetailAMD] CHECK CONSTRAINT [FK_SampleAMD_SampleId];

/*Down script. */
ALTER TABLE [dbo].[SampleDetailAMD] DROP CONSTRAINT [FK_SampleAMD_SampleId];
DROP TABLE [dbo].[SampleDetailAMD];