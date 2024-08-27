/* Up Script */
ALTER TABLE [Permission]
ADD [IsAcknowledge] bit NOT NULL,
[IsApprove] bit NOT NULL,
[IsEmail] bit NOT NULL,
CONSTRAINT [DF__Permission__IsAcknowledge]  DEFAULT (0) FOR [IsAcknowledge],
CONSTRAINT [DF__Permission__IsApprove]  DEFAULT (0) FOR [IsApprove],
CONSTRAINT [DF__Permission__IsEmail]  DEFAULT (0) FOR [IsEmail];

/* Down Script */
ALTER TABLE [Permission]
DROP CONSTRAINT [DF__Permission__IsAcknowledge],
CONSTRAINT [DF__Permission__IsApprove],
CONSTRAINT [DF__Permission__IsEmail],
COLUMN [IsAcknowledge], 
COLUMN [IsApprove],
COLUMN [IsEmail];