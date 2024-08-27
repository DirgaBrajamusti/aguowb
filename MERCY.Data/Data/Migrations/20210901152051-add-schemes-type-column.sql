/* Up script. */
ALTER TABLE Scheme
ADD [Type] [varchar](50) NULL;

/* Down script. */
ALTER TABLE Scheme
DROP COLUMN [Type];
