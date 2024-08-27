create procedure [dbo].[Report_SAMPLING_ROM_Header] @p_company varchar(32), @p_date varchar(10)
as
begin
	SELECT top 1
		Date_Detail as DATE_1
		, '' as DATE_2
		, JOB_NO
		, Report_To
		, Method1
		, Method2
		, Method3
		, Method4
	from UPLOAD_Sampling_ROM_Header
    where Company = @p_company
        and CONVERT(varchar,CreatedOn,103) = @p_date
	order by RecordId desc
end