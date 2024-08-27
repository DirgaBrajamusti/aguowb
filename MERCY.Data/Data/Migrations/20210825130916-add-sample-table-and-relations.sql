--Scheme
/* Up script. */
CREATE TABLE [dbo].[Scheme](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[Name] [varchar](255) NULL,
	[CreatedOn] [datetime] NOT NULL,
	[CreatedBy] [int] NOT NULL,
	[LastModifiedOn] [datetime] NULL,
	[LastModifiedBy] [int] NULL,
 CONSTRAINT [PK_Scheme] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY];

ALTER TABLE [dbo].[Scheme]  ADD  CONSTRAINT [DF__Scheme__CreatedOn]  DEFAULT (getdate()) FOR [CreatedOn];
ALTER TABLE [dbo].[Scheme] ADD  CONSTRAINT [DF__Scheme__CreatedBy]  DEFAULT ((0)) FOR [CreatedBy];


CREATE TABLE [dbo].[Client](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[Name] [varchar](255) NULL,
	[CreatedOn] [datetime] NOT NULL,
	[CreatedBy] [int] NOT NULL,
	[LastModifiedOn] [datetime] NULL,
	[LastModifiedBy] [int] NULL,
 CONSTRAINT [PK_Client] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY];

ALTER TABLE [dbo].[Client] ADD  CONSTRAINT [DF__Client__CreatedOn]  DEFAULT (getdate()) FOR [CreatedOn];
ALTER TABLE [dbo].[Client] ADD  CONSTRAINT [DF__Client__CreatedBy]  DEFAULT ((0)) FOR [CreatedBy];

CREATE TABLE [dbo].[Project](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[Name] [varchar](255) NOT NULL,
	[CreatedOn] [datetime] NOT NULL,
	[CreatedBy] [int] NOT NULL,
	[LastModifiedOn] [datetime] NULL,
	[LastModifiedBy] [int] NULL,
 CONSTRAINT [PK_Project] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY];

ALTER TABLE [dbo].[Project] ADD CONSTRAINT [DF__Project__CreatedOn]  DEFAULT (getdate()) FOR [CreatedOn];
ALTER TABLE [dbo].[Project] ADD CONSTRAINT [DF__Project__CreatedBy]  DEFAULT ((0)) FOR [CreatedBy];

CREATE TABLE [dbo].[RefType](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[Name] [varchar](255) NULL,
	[CreatedOn] [datetime] NOT NULL,
	[CreatedBy] [int] NOT NULL,
	[LastModifiedOn] [datetime] NULL,
	[LastModifiedBy] [int] NULL,
 CONSTRAINT [PK_RefType] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY];

ALTER TABLE [dbo].[RefType] ADD CONSTRAINT [DF__RefType__CreatedOn]  DEFAULT (getdate()) FOR [CreatedOn];
ALTER TABLE [dbo].[RefType] ADD CONSTRAINT [DF__RefType__CreatedBy]  DEFAULT ((0)) FOR [CreatedBy];

CREATE TABLE [dbo].[CompanyRefType](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[CompanyCode] [varchar](32) NOT NULL,
	[RefTypeId] [int] NOT NULL,
	[CreatedOn] [datetime] NOT NULL,
	[CreatedBy] [int] NOT NULL,
	[LastModifiedOn] [datetime] NULL,
	[LastModifiedBy] [int] NULL,
 CONSTRAINT [PK_CompanyRefTypeId] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY];

ALTER TABLE [dbo].[CompanyRefType] ADD CONSTRAINT [DF__CompanyRefType__CreatedOn]  DEFAULT (getdate()) FOR [CreatedOn];
ALTER TABLE [dbo].[CompanyRefType] ADD CONSTRAINT [DF__CompanyRefType__CreatedBy]  DEFAULT ((0)) FOR [CreatedBy];
ALTER TABLE [dbo].[CompanyRefType]  WITH CHECK ADD  CONSTRAINT [FK_CompanyRefType_CompanyId] FOREIGN KEY([CompanyCode])
REFERENCES [dbo].[Company] ([CompanyCode]);
ALTER TABLE [dbo].[CompanyRefType] CHECK CONSTRAINT [FK_CompanyRefType_CompanyId];
ALTER TABLE [dbo].[CompanyRefType]  WITH CHECK ADD  CONSTRAINT [FK_CompanyRefType_RefTypeId] FOREIGN KEY([RefTypeId])
REFERENCES [dbo].[RefType] ([Id]);
ALTER TABLE [dbo].[CompanyRefType] CHECK CONSTRAINT [FK_CompanyRefType_RefTypeId];


CREATE TABLE [dbo].[Sample](
	[Id] [int] NOT NULL,
	[CompanyCode] [varchar] (32) NOT NULL,
	[SiteId] [int] NOT NULL,
	[SampleId] [varchar](50) NOT NULL,
	[DateOfJob] [datetime] NOT NULL,
	[ClientId] [int] NOT NULL,
	[ProjectId] [int] NOT NULL,
	[RefTypeId] [int] NOT NULL,
	[ReceivedBy] [varchar](50) NOT NULL,
	[CreatedOn] [datetime] NOT NULL,
	[CreatedBy] [int] NOT NULL,
	[LastModifiedOn] [datetime] NULL,
	[LastModifiedBy] [int] NULL,
 CONSTRAINT [PK_Sample] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY];

ALTER TABLE [dbo].[Sample] ADD  CONSTRAINT [DF__Sample__CreatedOn]  DEFAULT (getdate()) FOR [CreatedOn];
ALTER TABLE [dbo].[Sample] ADD  CONSTRAINT [DF__Sample__CreatedBy]  DEFAULT ((0)) FOR [CreatedBy];
ALTER TABLE [dbo].[Sample]  WITH CHECK ADD  CONSTRAINT [FK_Sample_CompanyId] FOREIGN KEY([CompanyCode]) REFERENCES [dbo].[Company] ([CompanyCode]);
ALTER TABLE [dbo].[Sample] CHECK CONSTRAINT [FK_Sample_CompanyId];
ALTER TABLE [dbo].[Sample]  WITH CHECK ADD  CONSTRAINT [FK_Sample_SiteId] FOREIGN KEY([SiteId]) REFERENCES [dbo].[Site] ([SiteId]);
ALTER TABLE [dbo].[Sample] CHECK CONSTRAINT [FK_Sample_SiteId];
ALTER TABLE [dbo].[Sample]  WITH CHECK ADD  CONSTRAINT [FK_SampleClient_ClientId] FOREIGN KEY([ClientId]) REFERENCES [dbo].[Client] ([Id]);
ALTER TABLE [dbo].[Sample] CHECK CONSTRAINT [FK_SampleClient_ClientId];
ALTER TABLE [dbo].[Sample]  WITH CHECK ADD  CONSTRAINT [FK_SampleProject_ProjectId] FOREIGN KEY([ProjectId]) REFERENCES [dbo].[Project] ([Id]);
ALTER TABLE [dbo].[Sample] CHECK CONSTRAINT [FK_SampleProject_ProjectId];
ALTER TABLE [dbo].[Sample]  WITH CHECK ADD  CONSTRAINT [FK_SampleRefType_RefTypeId] FOREIGN KEY([RefTypeId]) REFERENCES [dbo].[RefType] ([Id]);
ALTER TABLE [dbo].[Sample] CHECK CONSTRAINT [FK_SampleRefType_RefTypeId];

CREATE TABLE [dbo].[SampleScheme](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[SampleId] [int] NOT NULL,
	[SchemeId] [int] NOT NULL,
	[CreatedOn] [datetime] NOT NULL,
	[CreatedBy] [int] NOT NULL,
	[LastModifiedOn] [datetime] NULL,
	[LastModifiedBy] [int] NULL,
 CONSTRAINT [PK_SampleSchemeId] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY];

ALTER TABLE [dbo].[SampleScheme] ADD CONSTRAINT [DF__SampleScheme__CreatedOn]  DEFAULT (getdate()) FOR [CreatedOn];
ALTER TABLE [dbo].[SampleScheme] ADD CONSTRAINT [DF__SampleScheme__CreatedBy]  DEFAULT (0) FOR [CreatedBy];
ALTER TABLE [dbo].[SampleScheme]  WITH CHECK 
	ADD CONSTRAINT [FK_SampleScheme_SampleId] 
	FOREIGN KEY([SampleId]) REFERENCES [dbo].[Sample] ([Id]) ON DELETE CASCADE;
ALTER TABLE [dbo].[SampleScheme] CHECK CONSTRAINT [FK_SampleScheme_SampleId];
ALTER TABLE [dbo].[SampleScheme]  WITH CHECK 
	ADD CONSTRAINT [FK_SampleScheme_SchemeId]
	FOREIGN KEY([SchemeId]) REFERENCES [dbo].[Scheme] ([Id]);
ALTER TABLE [dbo].[SampleScheme] CHECK CONSTRAINT [FK_SampleScheme_SchemeId];

CREATE TABLE [dbo].[CompanyClientProjectScheme](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[CompanyCode] [varchar](32) NOT NULL,
	[ClientId] [int] NOT NULL,
	[ProjectId] [int] NOT NULL,
	[SchemeId] [int] NOT NULL,
	[IsRequired] [bit] NOT NULL,
	[CreatedOn] [datetime] NOT NULL,
	[CreatedBy] [int] NOT NULL,
	[LastModifiedOn] [datetime] NULL,
	[LastModifiedBy] [int] NULL,
 CONSTRAINT [PK_Company_Client_Project_Scheme] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY];

ALTER TABLE [dbo].[CompanyClientProjectScheme] ADD CONSTRAINT [DF__CompClientProScheme__CreatedOn]  DEFAULT (getdate()) FOR [CreatedOn];
ALTER TABLE [dbo].[CompanyClientProjectScheme] ADD CONSTRAINT [DF__CompClientProScheme__CreatedBy]  DEFAULT ((0)) FOR [CreatedBy];

ALTER TABLE [dbo].[CompanyClientProjectScheme]  WITH CHECK ADD  CONSTRAINT [FK_CompanyClientProjectScheme_CompanyId] FOREIGN KEY([CompanyCode]) REFERENCES [dbo].[Company] ([CompanyCode]);
ALTER TABLE [dbo].[CompanyClientProjectScheme] CHECK CONSTRAINT [FK_CompanyClientProjectScheme_CompanyId];
ALTER TABLE [dbo].[CompanyClientProjectScheme]  WITH CHECK ADD  CONSTRAINT [FK_CompanyClientProjectScheme_ClientId] FOREIGN KEY([ClientId]) REFERENCES [dbo].[Client] ([Id]);
ALTER TABLE [dbo].[CompanyClientProjectScheme] CHECK CONSTRAINT [FK_CompanyClientProjectScheme_ClientId];
ALTER TABLE [dbo].[CompanyClientProjectScheme]  WITH CHECK ADD  CONSTRAINT [FK_CompanyClientProjectScheme_ProjectId] FOREIGN KEY([ProjectId]) REFERENCES [dbo].[Project] ([Id]);
ALTER TABLE [dbo].[CompanyClientProjectScheme] CHECK CONSTRAINT [FK_CompanyClientProjectScheme_ProjectId];
ALTER TABLE [dbo].[CompanyClientProjectScheme]  WITH CHECK ADD  CONSTRAINT [FK_CompanyClientProjectScheme_SchemeId] FOREIGN KEY([SchemeId]) REFERENCES [dbo].[Scheme] ([Id]);
ALTER TABLE [dbo].[CompanyClientProjectScheme] CHECK CONSTRAINT [FK_CompanyClientProjectScheme_SchemeId];

/*Down script. */
DROP TABLE [dbo].[Scheme];
DROP TABLE [dbo].[Client];
DROP TABLE [dbo].[Project];
DROP TABLE [dbo].[RefType];
ALTER TABLE [dbo].[CompanyRefType] DROP CONSTRAINT [FK_CompanyRefType_CompanyId];
ALTER TABLE [dbo].[CompanyRefType] DROP CONSTRAINT [FK_CompanyRefType_RefTypeId];
DROP TABLE [dbo].[CompanyRefType];
DROP TABLE [dbo].[Sample];

ALTER TABLE [dbo].[Sample] DROP CONSTRAINT [FK_Sample_CompanyId];
ALTER TABLE [dbo].[Sample] DROP CONSTRAINT [FK_Sample_SiteId];
ALTER TABLE [dbo].[Sample] DROP CONSTRAINT [FK_SampleClient_ClientId];
ALTER TABLE [dbo].[Sample] DROP CONSTRAINT [FK_SampleProject_ProjectId];
ALTER TABLE [dbo].[Sample] DROP CONSTRAINT [FK_SampleRefType_RefTypeId];

ALTER TABLE [dbo].[SampleScheme] DROP CONSTRAINT [FK_SampleScheme_SampleId];
ALTER TABLE [dbo].[SampleScheme] DROP CONSTRAINT [FK_SampleScheme_SchemeId];
DROP TABLE [dbo].[SampleScheme];

ALTER TABLE [dbo].[CompanyClientProjectScheme] DROP CONSTRAINT [FK_CompanyClientProjectScheme_CompanyId];
ALTER TABLE [dbo].[CompanyClientProjectScheme] DROP CONSTRAINT [FK_CompanyClientProjectScheme_ClientId];
ALTER TABLE [dbo].[CompanyClientProjectScheme] DROP CONSTRAINT [FK_CompanyClientProjectScheme_ProjectId];
ALTER TABLE [dbo].[CompanyClientProjectScheme] DROP CONSTRAINT [FK_CompanyClientProjectScheme_SchemeId];
DROP TABLE [dbo].[CompanyClientProjectScheme];
