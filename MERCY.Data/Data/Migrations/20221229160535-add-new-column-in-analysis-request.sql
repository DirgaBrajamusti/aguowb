/* Up Script*/
Alter Table dbo.AnalysisRequest
ADD [EstimatedEndDate] [date] NULL,
	[CUD] [bit] NOT NULL DEFAULT 0,
	[Moisture] [bit] NOT NULL DEFAULT 0,
	[DensityWet] [bit] NOT NULL DEFAULT 0,
	[DensityDry] [bit] NOT NULL DEFAULT 0,
	[UCS] [bit] NOT NULL DEFAULT 0,
	[DST] [bit] NOT NULL DEFAULT 0,
	[GPa] [bit] NOT NULL DEFAULT 0,
	[PoissionRatio] [bit] NOT NULL DEFAULT 0,
	[KPa] [bit] NOT NULL DEFAULT 0;
