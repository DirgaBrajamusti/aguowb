/*Script Up*/
CREATE TABLE [dbo].[CoalInventory](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[CompanyCode] [varchar](32) NOT NULL,
	[ROMLocationId] [int] NOT NULL,
	[Period] [date] NOT NULL,
	[SurveyDate] [date] NOT NULL,
	[StartTime] [time] NOT NULL,
	[EndTime] [time] NOT NULL,
	[Tonnage] [decimal](18,4) NULL,
	[CreatedBy] [int] NOT NULL DEFAULT ((0)),
	[CreatedOn] [datetime] NOT NULL DEFAULT (getdate()),
	[LastModifiedBy] [int] NOT NULL DEFAULT ((0)),
	[LastModifiedOn] [datetime] NULL,
	CONSTRAINT [FK_CoalInventory_CompanyCode] FOREIGN KEY([CompanyCode])
	REFERENCES [dbo].[Company] ([CompanyCode]),
	CONSTRAINT [FK_CoalInventory_ROMLocationId] FOREIGN KEY([ROMLocationId])
	REFERENCES [dbo].[ROMLocation] ([Id]),
 CONSTRAINT [PK_CoalInventory_Id] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY];


CREATE TABLE [dbo].[CoalInventoryDetail](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[CoalInventoryId] [int] NOT NULL,
	[ROMLocationDetailId] [int] NOT NULL,
	[Volume] [decimal](18,4) NOT NULL,
	[FactorScale] [decimal](18,4) NOT NULL,
	[Tonnage] [decimal](18,4) NOT NULL,
	[Remark] [varchar] (255) NULL,
	[CreatedBy] [int] NOT NULL DEFAULT ((0)),
	[CreatedOn] [datetime] NOT NULL DEFAULT (getdate()),
	[LastModifiedBy] [int] NOT NULL DEFAULT ((0)),
	[LastModifiedOn] [datetime] NULL,
	CONSTRAINT [FK_CoalInventoryDetail_CoalInventoryId] FOREIGN KEY([CoalInventoryId])
	REFERENCES [dbo].[CoalInventory] ([Id]),
	CONSTRAINT [FK_CoalInventoryDetail_ROMLocationDetailId] FOREIGN KEY([ROMLocationDetailId])
	REFERENCES [dbo].[ROMLocationDetail] ([Id]),
 CONSTRAINT [PK_CoalInventoryDetail_Id] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY];

/*ScriptDown */
DROP TABLE [dbo].[CoalInventory];
DROP TABLE [dbo].[CoalInventoryDetail];