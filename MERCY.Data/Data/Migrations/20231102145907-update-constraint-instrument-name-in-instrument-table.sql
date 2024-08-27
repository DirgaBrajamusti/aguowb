/* Up Script*/
-- Step 1: Get the constraint name
DECLARE @ConstraintName NVARCHAR(MAX);
SELECT @ConstraintName = constraint_name
FROM information_schema.constraint_column_usage
WHERE table_name = 'Instrument'
AND column_name = 'InstrumentName';

-- Step 2: Drop the constraint
DECLARE @sql NVARCHAR(MAX);
SET @sql = 'ALTER TABLE Instrument DROP CONSTRAINT ' + @ConstraintName;
EXEC sp_executesql @sql;

-- Step 3: Create new constraint
ALTER TABLE dbo.Instrument
ADD CONSTRAINT UQ_Instrument_InstrumentName_SiteId_CompanyCode UNIQUE (InstrumentName, SiteId, CompanyCode);

/*Down Script*/
-- Step 1: Get the constraint name
DECLARE @ConstraintName NVARCHAR(MAX);
SELECT @ConstraintName = constraint_name
FROM information_schema.constraint_column_usage
WHERE table_name = 'Instrument'
AND column_name = 'InstrumentName';

-- Step 2: Drop the constraint
DECLARE @sql NVARCHAR(MAX);
SET @sql = 'ALTER TABLE Instrument DROP CONSTRAINT ' + @ConstraintName;
EXEC sp_executesql @sql;

-- Step 3: Create new constraint
ALTER TABLE dbo.Instrument
ADD CONSTRAINT UQ_Instrument_InstrumentName UNIQUE (InstrumentName);