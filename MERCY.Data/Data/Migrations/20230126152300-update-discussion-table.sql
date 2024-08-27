/* Up Script */
ALTER TABLE dbo.Discussion
ALTER COLUMN [Remark] text;

/* Down Script */
ALTER TABLE dbo.Discussion
ALTER COLUMN [Remark] VARCHAR(255);