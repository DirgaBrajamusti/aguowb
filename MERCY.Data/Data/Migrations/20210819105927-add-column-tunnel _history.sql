/* Up script. */
ALTER TABLE Tunnel_History
ADD [EffectiveDate] [datetime] NULL, [Remark] [varchar](255) NOT NULL DEFAULT '';

/* Down script. */
ALTER TABLE UPLOAD_Barge_Quality_Plan
DROP COLUMN [EffectiveDate], [Remark];
