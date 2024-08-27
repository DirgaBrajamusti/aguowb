create procedure [dbo].[Report_BARGE_LOADING_Header] @p_company varchar(32), @p_sheet varchar(32), @p_date varchar(10)
as
begin
	SELECT top 1
		Date_Detail as DATE_1
		, '' as DATE_2
		, Job_No
		, Report_To
		, Method1
		, Method2
		, Method3
		, Method4
	from UPLOAD_BARGE_LOADING_Header
    where Company = @p_company
        and Sheet = @p_sheet
        and CONVERT(varchar,CreatedOn,103) = @p_date
	order by RecordId desc
end