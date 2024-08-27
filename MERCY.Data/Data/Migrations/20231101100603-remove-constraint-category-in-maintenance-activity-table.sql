/* Up Script*/
-- Step 1: Get the constraint name
DECLARE @ConstraintName NVARCHAR(MAX);
SELECT @ConstraintName = constraint_name
FROM information_schema.constraint_column_usage
WHERE table_name = 'MaintenanceActivity'
AND column_name = 'Category';

-- Step 2: Drop the constraint
DECLARE @sql NVARCHAR(MAX);
SET @sql = 'ALTER TABLE MaintenanceActivity DROP CONSTRAINT ' + @ConstraintName;
EXEC sp_executesql @sql;

/*Down Script*/
ALTER TABLE MaintenanceActivity
ADD CONSTRAINT UQ_MaintenanceActivity_Category UNIQUE (Category);