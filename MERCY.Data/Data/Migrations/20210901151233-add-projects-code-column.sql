/* Up script. */
ALTER TABLE Project
ADD [Code] [varchar](10) NULL;

/* Down script. */
ALTER TABLE Project
DROP COLUMN [Code];
