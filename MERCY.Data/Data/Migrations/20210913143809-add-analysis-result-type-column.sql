/* Up Script */
ALTER TABLE [AnalysisResult]
ADD [Type] varchar(255) NOT NULL,
CONSTRAINT [DF_AnalysisResult_Type]  DEFAULT ('UNK') FOR [Type];

/* Down Script */
ALTER TABLE [AnalysisResult]
DROP CONSTRAINT [DF_AnalysisResult_Type],
COLUMN [Type];