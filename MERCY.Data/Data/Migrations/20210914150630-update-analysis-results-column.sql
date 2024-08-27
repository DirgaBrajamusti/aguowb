/* Up Script*/
Alter Table dbo.AnalysisResult
ALTER COLUMN Result [decimal](18, 2) NULL;

Alter Table dbo.AnalysisResultHistory
ALTER COLUMN Result [decimal](18, 2) NULL;

/*Down Script*/
Alter Table dbo.AnalysisResult
ALTER COLUMN Result [decimal](18, 2) NOT NULL;

Alter Table dbo.AnalysisResultHistory
ALTER COLUMN Result [decimal](18, 2) NOT NULL;
