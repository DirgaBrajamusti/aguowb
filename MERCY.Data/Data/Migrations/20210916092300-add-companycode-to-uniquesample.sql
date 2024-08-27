/*Up Script */
ALTER TABLE dbo.Sample DROP CONSTRAINT UN_Sample;
ALTER TABLE dbo.Sample ADD CONSTRAINT UN_Sample UNIQUE (CompanyCode,ExternalId);

/*DownScript */
ALTER TABLE dbo.Sample DROP CONSTRAINT UN_Sample;
ALTER TABLE dbo.Sample ADD CONSTRAINT UN_Sample UNIQUE (ExternalId);
