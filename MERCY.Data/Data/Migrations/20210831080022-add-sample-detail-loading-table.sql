/* Up script. */
CREATE TABLE [dbo].[SampleDetailLoading](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[SampleId] [int] NOT NULL,
	[LoadingNumber] [varchar](50) NOT NULL,
	[VesselName] [varchar](100) NOT NULL,
	[DispatchId] [varchar](50) NOT NULL,
	[Customer] [varchar](50) NOT NULL,
	[ETA] [datetime] NOT NULL,
	[ATA] [datetime] NOT NULL,
	[Contract] [varchar](100) NOT NULL,
	[Product] [varchar](50) NOT NULL,
	[LotNumber] [int] NOT NULL,
	[LotSamples] [varchar](50) NOT NULL,
	[Tonnage] [int] NOT NULL,
	[SamplingStart] [datetime] NOT NULL,
	[SamplingEnd] [datetime] NOT NULL,
	[CreatedBy] [int] NOT NULL,
	[CreatedOn] [datetime] NOT NULL,
	[LastModifiedBy] [int] NULL,
	[LastModifiedOn] [datetime] NULL,
 CONSTRAINT [PK_Sample_Detail_Loading] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY];
ALTER TABLE [dbo].[SampleDetailLoading] ADD  CONSTRAINT [DF__SampleLoading__CreatedOn]  DEFAULT (getdate()) FOR [CreatedOn];
ALTER TABLE [dbo].[SampleDetailLoading] ADD  CONSTRAINT [DF__SampleLoading__CreatedBy]  DEFAULT ((0)) FOR [CreatedBy];
ALTER TABLE [dbo].[SampleDetailLoading]  WITH CHECK
	ADD CONSTRAINT [FK_SampleLoading_SampleId]
	FOREIGN KEY([SampleId]) REFERENCES [dbo].[Sample] ([Id]) ON DELETE CASCADE;
ALTER TABLE [dbo].[SampleDetailLoading] CHECK CONSTRAINT [FK_SampleLoading_SampleId];

/*Down script. */
ALTER TABLE [dbo].[SampleDetailLoading] DROP CONSTRAINT [FK_SampleLoading_SampleId];
DROP TABLE [dbo].[SampleDetailLoading];