/*Script Up*/
ALTER TABLE [dbo].[CompanyClientProjectScheme] ADD 
SchemeOrder int NOT NULL;

/*ScriptDown */
ALTER TABLE [dbo].[CompanyClientProjectScheme] DROP COLUMN
SchemeOrder;