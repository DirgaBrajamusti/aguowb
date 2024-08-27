/* Up Script*/
ALTER TABLE [dbo].[AnalysisJob]
ADD [ValidatedBy] [int]  NULL,
[ApprovedBy] [int] NULL;


/*Down Script*/
ALTER TABLE [dbo].[AnalysisJob]
DROP COLUMN [ValidatedBy],
COLUMN [ApprovedBy];
