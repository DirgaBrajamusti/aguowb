/* Up script. */
ALTER TABLE Tunnel
ADD Approval varchar(50), EffectiveDate DateTime;

UPDATE Tunnel SET EffectiveDate = LastModifiedOn;

/* Down script. */
ALTER TABLE Tunnel
DROP COLUMN [Approval], [EffectiveDate];
