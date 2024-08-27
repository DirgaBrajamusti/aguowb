/* Up Script */

ALTER TABLE dbo.Company
ADD [TonnageDecimalNumber] [int] NOT NULL DEFAULT 3;

/* Down Script */
ALTER TABLE dbo.Company
DROP [TonnageDecimalNumber];