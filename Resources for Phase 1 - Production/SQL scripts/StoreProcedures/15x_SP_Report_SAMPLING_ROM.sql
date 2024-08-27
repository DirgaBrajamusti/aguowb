create procedure [dbo].[Report_SAMPLING_ROM] @p_company varchar(32), @p_date varchar(10)
as
begin
	SELECT
	  RecordId
	  ,Header
	  ,CreatedBy
	  ,CreatedOn
      ,Date_Request
	  ,Date_Sampling
	  ,Day_work
	  ,LOT
	  ,Lab_ID
	  ,TM
	  ,M
	  ,ASH
	  ,TS
	  ,CV
	  ,Remark
	  ,Seam
	FROM
	  UPLOAD_Sampling_ROM
    where Header in
    (
        SELECT RecordId
        from UPLOAD_Sampling_ROM_Header
        where Company = @p_company
            and CONVERT(varchar,CreatedOn,103) = @p_date
    )
    and CONVERT(varchar,CreatedOn,103) = @p_date
    order by RecordId
end