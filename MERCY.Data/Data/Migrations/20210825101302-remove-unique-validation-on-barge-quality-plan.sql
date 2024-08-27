/* Up script. */
ALTER TABLE dbo.UPLOAD_Barge_Quality_Plan
DROP CONSTRAINT CO_TEMPORARY_Barge_Quality_Plan_UNIQUE;

/* Down script. */
ALTER TABLE UPLOAD_Barge_Quality_Plan
ADD CONSTRAINT CO_TEMPORARY_Barge_Quality_Plan_UNIQUE UNIQUE (CreatedOn_Date_Only, TugName, BargeName);
