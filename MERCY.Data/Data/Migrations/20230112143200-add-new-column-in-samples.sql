/* Up Script */

ALTER TABLE dbo.Sample
ADD [ValidatedBy] [int] NULL,
	[ValidatedDate] [datetime] NULL;

/* Down Script */
ALTER TABLE dbo.Sample
DROP COLUMN [ValidatedBy], [ValidatedDate];