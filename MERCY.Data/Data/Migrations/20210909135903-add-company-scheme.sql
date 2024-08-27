/* Up Script */
CREATE TABLE [dbo].[CompanyScheme](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[CompanyCode] [varchar](32) NOT NULL,
	[SchemeId] [int] NOT NULL,
	[MinRepeatability] [decimal](18, 2) NULL,
	[MaxRepeatability] [decimal](18, 2) NULL,
	[Details] [varchar](max) NULL,
	[CreatedOn] [datetime] NOT NULL DEFAULT (getdate()),
	[CreatedBy] [int] NOT NULL DEFAULT ((0)),
	[LastModifiedOn] [datetime] NULL,
	[LastModifiedBy] [int] NULL,
  CONSTRAINT [FK_CompanyScheme_CompanyId] FOREIGN KEY([CompanyCode]) REFERENCES [dbo].[Company] ([CompanyCode]),
  CONSTRAINT [FK_CompanyScheme_SchemeId] FOREIGN KEY([SchemeId]) REFERENCES [dbo].[Scheme] ([Id]),
  CONSTRAINT [PK_CompanyScheme] PRIMARY KEY CLUSTERED
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

/* Down Script */
DROP TABLE [dbo].[CompanyScheme];