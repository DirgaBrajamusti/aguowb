/* Up script. */
CREATE TABLE [dbo].[AnalysisRequestSample](
	[Id] [int] IDENTITY(0,1) NOT NULL,
	[AnalysisRequestId] [bigint] NOT NULL,
	[SampleId] [int] NOT NULL,
	[CreatedOn] [datetime] NOT NULL,
	[CreatedBy] [int] NOT NULL,
	[LastModifiedOn] [datetime] NULL,
	[LastModifiedBy] [int] NULL,
 CONSTRAINT [PK_AnalysisRequestSampleId] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY];

ALTER TABLE [dbo].[AnalysisRequestSample] ADD  DEFAULT (getdate()) FOR [CreatedOn];

ALTER TABLE [dbo].[AnalysisRequestSample] ADD  DEFAULT ((0)) FOR [CreatedBy];

ALTER TABLE [dbo].[AnalysisRequestSample]  WITH CHECK ADD  CONSTRAINT [FK_AnalysisRequestSample_AnalysisRequestId] FOREIGN KEY([AnalysisRequestId])
REFERENCES [dbo].[AnalysisRequest] ([AnalysisRequestId]);
ALTER TABLE [dbo].[AnalysisRequestSample] CHECK CONSTRAINT [FK_AnalysisRequestSample_AnalysisRequestId];

ALTER TABLE [dbo].[AnalysisRequestSample]  WITH CHECK ADD  CONSTRAINT [FK_AnalysisRequestSample_SampleId] FOREIGN KEY([SampleId])
REFERENCES [dbo].[Sample] ([Id]);
ALTER TABLE [dbo].[AnalysisRequestSample] CHECK CONSTRAINT [FK_AnalysisRequestSample_SampleId];

/* Down script. */
DROP TABLE [dbo].[AnalysisRequestSample];
