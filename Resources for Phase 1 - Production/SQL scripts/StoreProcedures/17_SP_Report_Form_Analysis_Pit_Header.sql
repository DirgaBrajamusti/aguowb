create procedure [dbo].[Report_Form_Analysis_Pit_Header](@p_analysisRequest int)
as
begin
	SELECT top 1
		Date_Detail as DATE_1
		, '' as DATE_2
		, JOB_NO
		, Report_To
		, Date_Received as Date_Receive_sample
		, Nomor as HEADER_NO
	from UPLOAD_Geology_Pit_Monitoring_Header
    where RecordId in
    (
        select Header
        from UPLOAD_Geology_Pit_Monitoring
        where Lab_ID in
        (
            select LabId from AnalysisRequest_Detail
            where AnalysisRequest = @p_analysisRequest
        )
    )
	order by RecordId desc
end