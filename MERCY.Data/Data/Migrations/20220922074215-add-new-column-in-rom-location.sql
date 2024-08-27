/* Up Script*/
Alter Table dbo.ROMLocation
ADD Code [varchar](100) NULL;

/*Down Script*/
Alter Table dbo.ROMLocation
DROP COLUMN Code;
