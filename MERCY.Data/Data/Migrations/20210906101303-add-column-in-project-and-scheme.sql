/* Up Script */
ALTER TABLE [Project] ADD
[Type] varchar(20) NOT NULL,
CONSTRAINT [DF__Project__Type]  DEFAULT ('General') FOR [Type];

ALTER TABLE [Scheme] ADD
[MinRepeatability] decimal(18,2) NULL,
[MaxRepeatability] decimal(18,2) NULL;

/* Down Script */
ALTER TABLE [Project]
DROP CONSTRAINT [DF__Project__Type],
COLUMN [Type];
ALTER TABLE [Scheme]
DROP COLUMN [MinRepeatability],
COLUMN [MaxRepeatability];