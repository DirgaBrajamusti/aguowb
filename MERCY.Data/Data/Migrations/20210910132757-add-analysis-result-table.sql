/* Up Script */
CREATE TABLE [dbo].[AnalysisResult](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[Result] [decimal](18, 2) NOT NULL,
	[Details] [varchar](max) NULL,
	[SampleSchemeId] [int] NOT NULL,
	[CreatedOn] [datetime] NOT NULL DEFAULT (getdate()),
	[CreatedBy] [int] NOT NULL DEFAULT 0,
	[LastModifiedOn] [datetime] NULL,
	[LastModifiedBy] [int] NULL,
	CONSTRAINT [FK_AnalysisResult_SampleSchemeId] FOREIGN KEY([SampleSchemeId]) REFERENCES [dbo].[Sample_Scheme] ([Id]),
  CONSTRAINT [PK_AnalysisResult] PRIMARY KEY CLUSTERED ([Id] ASC) WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

/* Down Script */
DROP TABLE [dbo].[AnalysisResult];