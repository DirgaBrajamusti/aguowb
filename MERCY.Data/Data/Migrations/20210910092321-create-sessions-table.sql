/* Up Script */
CREATE TABLE [dbo].[Session](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[Menu] [varchar](32) NOT NULL,
	[Token] [varchar](max) NULL,
	[CreatedOn] [datetime] NOT NULL DEFAULT (getdate()),
	[CreatedBy] [int] NOT NULL DEFAULT ((0)),
	[LastModifiedOn] [datetime] NULL,
	[LastModifiedBy] [int] NULL,
 CONSTRAINT [PK_Session] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

/* Down Script */
DROP TABLE [dbo].[Session];
