create procedure [dbo].[Report_CRUSHING_PLANT] @p_company varchar(32), @p_sheet varchar(32), @p_date varchar(10)
as
begin
	select [Date_Production]
        ,[Shift_Work]
        ,[Tonnage]
        ,[Sample_ID]
        ,[TM]
        ,[M]
        ,[ASH_adb]
        ,[ASH_arb]
        ,[VM_adb]
        ,[VM_arb]
        ,[FC_adb]
        ,[FC_arb]
        ,[TS_adb]
        ,[TS_arb]
        , CONVERT(decimal(10,0), [CV_adb]) as CV_adb 
        , CONVERT(decimal(10,0), [CV_db]) as CV_db 
        , CONVERT(decimal(10,0), [CV_arb]) as CV_arb 
        , CONVERT(decimal(10,0), [CV_daf]) as CV_daf 
        , CONVERT(decimal(10,0), [CV_ad_15]) as CV_ad_15 
        , CONVERT(decimal(10,0), [CV_ad_16]) as CV_ad_16 
        , CONVERT(decimal(10,0), [CV_ad_17]) as CV_ad_17 
        ,[Remark]
  FROM [UPLOAD_CRUSHING_PLANT]
  where Header in
    (
        SELECT RecordId
        from UPLOAD_CRUSHING_PLANT_Header
        where Company = @p_company
            --and Sheet = @p_sheet
            and CONVERT(varchar,CreatedOn,103) = @p_date
    )
    and CONVERT(varchar,CreatedOn,103) = @p_date
    and Sheet = @p_sheet
    order by RecordId
end