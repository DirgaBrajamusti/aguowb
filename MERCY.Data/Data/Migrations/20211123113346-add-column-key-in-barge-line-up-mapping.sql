/*Script Up*/
ALTER TABLE BargeLineUpMapping ADD [DataKey] varchar (20) NULL

/*ScriptDown */
ALTER TABLE BargeLineUpMapping DROP COLUMN
[DataKey];