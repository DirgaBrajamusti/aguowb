/* Up Script*/
Alter Table dbo.AnalysisRequest
ADD [ANC] [bit] NOT NULL DEFAULT 0,
	[NAG] [bit] NOT NULL DEFAULT 0;

Alter Table dbo.AnalysisRequest_Detail
ADD [Length][int] NOT NULL DEFAULT 0,
	[MPA][decimal](18, 2) NOT NULL DEFAULT 0,
	[TS][decimal](18, 2) NOT NULL DEFAULT 0,
	[PH][decimal](18, 2) NOT NULL DEFAULT 0,
	[NAPP][decimal](18, 4) NOT NULL DEFAULT 0,
	[ANC][decimal](18, 4) NOT NULL DEFAULT 0,
	[NAG][decimal](18, 4) NOT NULL DEFAULT 0;

/* Down Script */

Alter Table dbo.AnalysisRequest
DROP COLUMN [ANC], [NAG];

Alter Table dbo.AnalysisRequest_Detail
DROP COLUMN [Length],[MPA],[TS],[PH],[NAPP],[ANC],[NAG];