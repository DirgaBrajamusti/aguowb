/*Up Script*/

ALTER TABLE dbo.Scheme ADD Details varchar(MAX) NULL;

/*DownScript*/
ALTER TABLE dbo.Scheme
DROP COLUMN Details;