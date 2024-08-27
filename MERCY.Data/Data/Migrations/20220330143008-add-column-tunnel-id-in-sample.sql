/*Script Up*/
ALTER TABLE [Sample]
ADD [TunnelId] int NULL,
CONSTRAINT [FK_Sample_TunnelId] FOREIGN KEY([TunnelId])
REFERENCES [dbo].[Tunnel] ([TunnelId]);

/*ScriptDown */
ALTER TABLE [Sample]
DROP COLUMN [TunnelId];