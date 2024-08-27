create procedure [dbo].[Report_GEOLOGY_PIT_Header] @p_company varchar(32), @p_date varchar(10)
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
    where Company = @p_company
        and CONVERT(varchar,CreatedOn,103) = @p_date
	order by recordid desc
end