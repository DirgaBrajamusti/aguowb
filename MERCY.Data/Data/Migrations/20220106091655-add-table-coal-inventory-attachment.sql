/*Script Up*/
CREATE TABLE [dbo].[CoalInventoryAttachment](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[CoalInventoryId] [int] NOT NULL,
	[OriginalName] [varchar](255) NOT NULL DEFAULT (('')),
	[FileName] [varchar](255) NOT NULL DEFAULT (('')),
	[CreatedBy] [int] NOT NULL DEFAULT ((0)),
	[CreatedOn] [datetime] NOT NULL DEFAULT (getdate()),
	[LastModifiedBy] [int] NOT NULL DEFAULT ((0)),
	[LastModifiedOn] [datetime] NULL,
	CONSTRAINT [FK_CoalInventoryAttachmen_CoalInventoryId] FOREIGN KEY([CoalInventoryId])
	REFERENCES [dbo].[CoalInventory] ([Id]),
 CONSTRAINT [PK_CoalInventoryAttachment_Id] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY];

/*ScriptDown */
DROP TABLE [dbo].[CoalInventoryAttachment];