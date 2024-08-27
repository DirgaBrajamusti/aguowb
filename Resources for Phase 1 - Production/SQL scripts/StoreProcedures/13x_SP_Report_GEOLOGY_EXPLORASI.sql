create procedure [dbo].[Report_GEOLOGY_EXPLORASI] @p_company varchar(32), @p_date varchar(10)
as
begin
	SELECT 
      [Sample_ID]
      ,[SampleType]
      ,[Lab_ID]
      ,[Mass_Spl]
      ,[TM]
      ,[M]
      ,[VM]
      ,[Ash]
      ,[FC]
      ,[TS]
      ,CONVERT(decimal(10,0), [Cal_ad]) as Cal_ad
      ,CONVERT(decimal(10,0), [Cal_db]) as Cal_db
      ,CONVERT(decimal(10,0), [Cal_ar]) as Cal_ar
      ,CONVERT(decimal(10,0), [Cal_daf]) as Cal_daf
      ,[RD]
  FROM [UPLOAD_Geology_Explorasi]
  where Header in
    (
        SELECT RecordId
        from UPLOAD_Geology_Explorasi_Header
        where Company = @p_company
            and CONVERT(varchar,CreatedOn,103) = @p_date
    )
    and CONVERT(varchar,CreatedOn,103) = @p_date
    order by RecordId
end