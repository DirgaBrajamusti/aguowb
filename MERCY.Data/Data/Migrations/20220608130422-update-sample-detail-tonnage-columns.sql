/* Up Script*/
Alter Table dbo.SampleDetailGeneral
ALTER COLUMN Tonnage [decimal](18, 5) NOT NULL;

Alter Table dbo.SampleDetailLoading
ALTER COLUMN Tonnage [decimal](18, 5) NOT NULL;

/*Down Script*/
Alter Table dbo.AnalysisTonnage
ALTER COLUMN Tonnage int NOT NULL;

Alter Table dbo.AnalysisTonnageHistory
ALTER COLUMN Tonnage int NOT NULL;
