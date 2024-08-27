create procedure [dbo].[Report_Form_Analysis_Exploration_Check](@p_analysisRequest int)
as
begin
	select count(RecordId) as Count2
	from UPLOAD_Geology_Explorasi
	where Lab_ID in
	(
		select LabId from AnalysisRequest_Detail
		where AnalysisRequest = @p_analysisRequest
	)
end;