/*Script Up*/
ALTER TABLE dbo.UPLOAD_CRUSHING_PLANT_Header ADD
DateReported varchar(50) NULL,
ApprovedBy varchar(100) NULL;

/*ScriptDown */
ALTER TABLE dbo.UPLOAD_CRUSHING_PLANT_Header DROP COLUMN
DateReported,
ApprovedBy;