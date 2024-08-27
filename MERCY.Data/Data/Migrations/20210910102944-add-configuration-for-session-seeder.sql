/* Up script */
INSERT INTO Config (Name, Value) VALUES
	 (N'Session_Security_Key','<ask administrator for it>'),
	 (N'Session_Expired_Time_in_Minutes','5');

/* Down script */
DELETE FROM Config
WHERE Name IN (N'Session_Security_Key', N'Session_Expired_Time_in_Minutes');
