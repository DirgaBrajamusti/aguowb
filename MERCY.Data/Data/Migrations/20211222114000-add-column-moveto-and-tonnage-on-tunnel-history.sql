/* Up Script*/
Alter Table dbo.TunnelHistory
ADD [StockMovementTunnelId] [int],
[StockMovementTonnage] decimal(10,2)
CONSTRAINT FK_TunnelHistory_StockMovementTunnelId FOREIGN KEY (StockMovementTunnelId) REFERENCES dbo.Tunnel(TunnelId);

/*Down Script*/
Alter Table dbo.TunnelHistory
DROP CONSTRAINT FK_TunnelHistory_StockMovementTunnelId,
DROP COLUMN StockMovementTunnelId,
COLUMN StockMovementTonnage;