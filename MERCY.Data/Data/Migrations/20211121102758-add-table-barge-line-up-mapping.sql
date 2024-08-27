/*Script Up*/
CREATE TABLE [dbo].[BargeLineUpMapping](
	[Id] [int] IDENTITY(0,1) NOT NULL,
	[DataRequired] [varchar](50) NOT NULL,
	[TemplateColumn] [varchar](20) NOT NULL,
	[Sheet] [varchar](10) NOT NULL,
	[CreatedAt] [datetime] NOT NULL,
	[LastModifiedAt] [datetime] NULL,
	[CreatedBy] [int] NOT NULL,
	[LastModifiedBy] [int] NULL,
 CONSTRAINT [PK_BargeLineUpMapping] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY];

/*ScriptDown */
DROP TABLE [BargeLineUpMapping];