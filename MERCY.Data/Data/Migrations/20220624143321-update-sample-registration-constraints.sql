/* Up Script*/
ALTER TABLE SampleDetailGeneral DROP CONSTRAINT FK_SampleDetailGeneral_Sample_SampleId;
ALTER TABLE SampleDetailGeneral WITH CHECK 
	ADD CONSTRAINT FK_SampleDetailGeneral_Sample_SampleId FOREIGN KEY (Sample_Id) REFERENCES Sample(Id) ON DELETE CASCADE;
ALTER TABLE SampleDetailGeneral CHECK CONSTRAINT FK_SampleDetailGeneral_Sample_SampleId;

ALTER TABLE SampleDetailLoading DROP CONSTRAINT FK_SampleLoading_SampleId;
ALTER TABLE SampleDetailLoading WITH CHECK 
	ADD CONSTRAINT FK_SampleLoading_SampleId FOREIGN KEY (SampleId) REFERENCES Sample(Id) ON DELETE CASCADE;
ALTER TABLE SampleDetailLoading CHECK CONSTRAINT FK_SampleLoading_SampleId;

ALTER TABLE SampleScheme DROP CONSTRAINT FK_SampleScheme_SampleId;
ALTER TABLE SampleScheme WITH CHECK 
	ADD CONSTRAINT FK_SampleScheme_SampleId FOREIGN KEY (SampleId) REFERENCES Sample(Id) ON DELETE CASCADE;
ALTER TABLE SampleScheme CHECK CONSTRAINT FK_SampleScheme_SampleId;

ALTER TABLE AnalysisResult DROP CONSTRAINT FK_AnalysisResult_SampleSchemeId;
ALTER TABLE AnalysisResult WITH CHECK 
	ADD CONSTRAINT FK_AnalysisResult_SampleSchemeId FOREIGN KEY (SampleSchemeId) REFERENCES SampleScheme(Id) ON DELETE CASCADE;
ALTER TABLE AnalysisResult CHECK CONSTRAINT FK_AnalysisResult_SampleSchemeId;

ALTER TABLE AnalysisResultHistory DROP CONSTRAINT FK_AnalysisResult_AnalysisResultId;
ALTER TABLE AnalysisResultHistory WITH CHECK 
	ADD CONSTRAINT FK_AnalysisResult_AnalysisResultId FOREIGN KEY (AnalysisResultId) REFERENCES AnalysisResult(Id) ON DELETE CASCADE;
ALTER TABLE AnalysisResultHistory CHECK CONSTRAINT FK_AnalysisResult_AnalysisResultId;

/*Down Script*/
ALTER TABLE SampleDetailGeneral DROP CONSTRAINT FK_SampleDetailGeneral_Sample_SampleId;
ALTER TABLE SampleDetailGeneral WITH CHECK 
	ADD CONSTRAINT FK_SampleDetailGeneral_Sample_SampleId FOREIGN KEY (Sample_Id) REFERENCES Sample(Id);
ALTER TABLE SampleDetailGeneral CHECK CONSTRAINT FK_SampleDetailGeneral_Sample_SampleId;

ALTER TABLE SampleDetailLoading DROP CONSTRAINT FK_SampleLoading_SampleId;
ALTER TABLE SampleDetailLoading WITH CHECK 
	ADD CONSTRAINT FK_SampleLoading_SampleId FOREIGN KEY (SampleId) REFERENCES Sample(Id);
ALTER TABLE SampleDetailLoading CHECK CONSTRAINT FK_SampleLoading_SampleId;

ALTER TABLE SampleScheme DROP CONSTRAINT FK_SampleScheme_SampleId;
ALTER TABLE SampleScheme WITH CHECK 
	ADD CONSTRAINT FK_SampleScheme_SampleId FOREIGN KEY (SampleId) REFERENCES Sample(Id);
ALTER TABLE SampleScheme CHECK CONSTRAINT FK_SampleScheme_SampleId;

ALTER TABLE AnalysisResult DROP CONSTRAINT FK_AnalysisResult_SampleSchemeId;
ALTER TABLE AnalysisResult WITH CHECK 
	ADD CONSTRAINT FK_AnalysisResult_SampleSchemeId FOREIGN KEY (SampleSchemeId) REFERENCES SampleScheme(Id);
ALTER TABLE AnalysisResult CHECK CONSTRAINT FK_AnalysisResult_SampleSchemeId;

ALTER TABLE AnalysisResultHistory DROP CONSTRAINT FK_AnalysisResult_AnalysisResultId;
ALTER TABLE AnalysisResultHistory WITH CHECK 
	ADD CONSTRAINT FK_AnalysisResult_AnalysisResultId FOREIGN KEY (AnalysisResultId) REFERENCES AnalysisResult(Id);
ALTER TABLE AnalysisResultHistory CHECK CONSTRAINT FK_AnalysisResult_AnalysisResultId;
