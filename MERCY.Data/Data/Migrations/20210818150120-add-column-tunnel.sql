/* Up script. */
ALTER TABLE Tunnel ADD [Remark] [varchar](255) NOT NULL DEFAULT '';

/* Down script. */
ALTER TABLE Tunnel
DROP COLUMN [Remark];
