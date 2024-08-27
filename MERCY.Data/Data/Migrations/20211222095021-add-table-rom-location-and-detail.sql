/*Script Up*/
CREATE TABLE [dbo].[ROMLocation](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[CompanyCode] [varchar](32) NOT NULL,
	[Name] [varchar](255) NOT NULL,
	[CreatedBy] [int] NOT NULL DEFAULT ((0)),
	[CreatedOn] [datetime] NOT NULL DEFAULT (getdate()),
	[LastModifiedBy] [int] NOT NULL  DEFAULT ((0)),
	[LastModifiedOn] [datetime] NULL,
	[IsActive] [bit] NOT NULL DEFAULT ((0)),
	CONSTRAINT [FK_ROMLocation_CompanyCode] FOREIGN KEY([CompanyCode])
	REFERENCES [dbo].[Company] ([CompanyCode]),
 CONSTRAINT [PK_ROMLocation_Id] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY],
UNIQUE NONCLUSTERED 
(
	[Name] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY];

CREATE TABLE [dbo].[ROMLocationDetail](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[ROMLocationId] [int] NOT NULL,
	[Name] [varchar](255) NOT NULL,
	[CreatedBy] [int] NOT NULL DEFAULT ((0)),
	[CreatedOn] [datetime] NOT NULL DEFAULT (getdate()),
	[LastModifiedBy] [int] NOT NULL DEFAULT ((0)),
	[LastModifiedOn] [datetime] NULL,
	[IsActive] [bit] NOT NULL DEFAULT ((0)),
	CONSTRAINT [FK_ROMLocationDetail_ROMLocationId] FOREIGN KEY([ROMLocationId])
	REFERENCES [dbo].[ROMLocation] ([Id]),
 CONSTRAINT [PK_ROMLocationDetail_Id] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY],
UNIQUE NONCLUSTERED 
(
	[ROMLocationId] ASC,
	[Name] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY];

/*ScriptDown */
DROP TABLE [dbo].[ROMLocation];
DROP TABLE [dbo].[ROMLocationDetail];