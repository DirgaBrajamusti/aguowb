/*Script Up*/
ALTER TABLE [CoalInventory]
ADD [Status] varchar(20) NULL;

/*ScriptDown */
ALTER TABLE [CoalInventory]
DROP COLUMN [Status];