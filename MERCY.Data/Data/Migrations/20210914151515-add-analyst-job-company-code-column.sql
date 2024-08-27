/* Up Script */
ALTER TABLE [Analyst_Job]
ADD [CompanyCode] varchar(32) NOT NULL,
CONSTRAINT [DF_Analyst_Job_CompanyCode]  DEFAULT ('') FOR [CompanyCode];

/* Down Script */
ALTER TABLE [Analyst_Job]
DROP CONSTRAINT [DF_Analyst_Job_CompanyCode],
COLUMN [CompanyCode];