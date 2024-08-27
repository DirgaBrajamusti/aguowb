IF OBJECT_ID('Report_CRUSHING_PLANT') IS NULL
    EXEC('CREATE PROCEDURE Report_CRUSHING_PLANT AS SET NOCOUNT ON;')
GO

alter procedure [dbo].[Report_CRUSHING_PLANT] @p_company varchar(32), @p_sheet varchar(32), @p_date varchar(10), @p_dateTo varchar(10)
as
begin
    declare @dateFrom varchar(10)
    declare @dateTo  varchar(10)
    
    if (@p_date <= @p_dateTo)
    begin
        select @dateFrom = @p_date
        select @dateTo = @p_dateTo
    end
    else
    begin
        select @dateFrom = @p_dateTo
        select @dateTo = @p_date
    end
    
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
  where Company = @p_company
    and CONVERT(varchar(10),CreatedOn,120) >= @dateFrom
    and CONVERT(varchar(10),CreatedOn,120) <= @dateTo
    and Sheet = @p_sheet
    order by Date_Production, Shift_Work
end