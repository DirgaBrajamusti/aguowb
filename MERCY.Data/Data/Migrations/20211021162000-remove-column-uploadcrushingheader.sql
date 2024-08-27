/*Script Up*/
ALTER TABLE dbo.UPLOAD_CRUSHING_PLANT_Header DROP COLUMN 
DateReported,
ApprovedBy;

/*ScriptDown */
ALTER TABLE dbo.UPLOAD_CRUSHING_PLANT_Header ADD
	DateReported datetime NULL,
	ApprovedBy varbinary(100) NULL;