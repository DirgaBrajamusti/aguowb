/* Up Script*/
Alter Table dbo.Session
ALTER COLUMN Menu varchar(255) NOT NULL;

/*Down Script*/
Alter Table dbo.Session
ALTER COLUMN Menu varchar(32) NOT NULL;