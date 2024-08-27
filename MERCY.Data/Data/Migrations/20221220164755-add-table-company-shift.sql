/*Script Up*/
CREATE TABLE [dbo].[CompanyShift](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[CompanyCode] [varchar](32) NOT NULL DEFAULT (('')),
	[ShiftId] [int] NOT NULL,	
	[CreatedBy] [int] NOT NULL DEFAULT ((0)),
	[CreatedOn] [datetime] NOT NULL DEFAULT (getdate()),
	[LastModifiedBy] [int] NOT NULL DEFAULT ((0)),
	[LastModifiedOn] [datetime] NULL,
	CONSTRAINT [FK_CompanyShiftCompany_CompanyCode] FOREIGN KEY([CompanyCode])
	REFERENCES [dbo].[Company] ([CompanyCode]),
	CONSTRAINT [FK_CompanyShiftShift_Shift] FOREIGN KEY([ShiftId])
	REFERENCES [dbo].[Shift] ([Id]),
 CONSTRAINT [PK_CompanyShift_Id] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY];

/*ScriptDown */
DROP TABLE [dbo].[CompanyShift];