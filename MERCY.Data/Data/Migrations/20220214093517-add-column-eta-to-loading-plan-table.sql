/*Script Up*/
ALTER TABLE [Loading_Plan]
ADD [ETA] datetime NULL;

/*ScriptDown */
ALTER TABLE [Loading_Plan]
DROP COLUMN [ETA];