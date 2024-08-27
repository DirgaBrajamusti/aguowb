create procedure [dbo].[Report_Form_Analysis_Pit_Check](@p_analysisRequest int)
as
begin
	select count(RecordId) as Count2
	from UPLOAD_Geology_Pit_Monitoring
	where Lab_ID in
	(
		select LabId from AnalysisRequest_Detail
		where AnalysisRequest = @p_analysisRequest
	)
end;