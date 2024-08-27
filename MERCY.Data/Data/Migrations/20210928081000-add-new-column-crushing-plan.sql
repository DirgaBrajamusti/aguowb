/*Up Script */

ALTER TABLE dbo.UPLOAD_CRUSHING_PLANT ADD 
	SamplingStart datetime NULL,
 	SamplingEnd datetime NULL,
 	AnalysisStart datetime NULL,
 	AnalysisEnd datetime NULL,


ALTER TABLE dbo.UPLOAD_Geology_Explorasi ADD
	Na2O numeric(18,5) NULL,
	CaO numeric(18,5) NULL;

ALTER TABLE dbo.UPLOAD_Geology_Pit_Monitoring ADD
	Na2O numeric(18,5) NULL,
	CaO numeric(18,5) NULL;

ALTER TABLE dbo.SampleDetailLoading ADD
	Size numeric(18,5) NULL;

/*Down Script*/

ALTER TABLE dbo.UPLOAD_CRUSHING_PLANT DROP COLUMN
	SamplingEnd,
	SamplingStart,
	AnalysisEnd,
	AnalysisStart;

ALTER TABLE dbo.UPLOAD_Geology_Explorasi DROP COLUMN
	Na2O,
	CaO;

ALTER TABLE dbo.UPLOAD_Geology_Explorasi DROP COLUMN
	Na2O,
	CaO;
	
ALTER TABLE dbo.SampleDetailLoading DROP COLUMN
	Size;