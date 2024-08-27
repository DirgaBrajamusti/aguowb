IF OBJECT_ID('Report_BARGE_LOADING') IS NULL
    EXEC('CREATE PROCEDURE Report_BARGE_LOADING AS SET NOCOUNT ON;')
GO

alter procedure [dbo].[Report_BARGE_LOADING] @p_company varchar(32), @p_sheet varchar(32), @p_date varchar(10)
as
begin
    select [JOB_Number]
        ,[ID_Number]
        ,[Service_Trip_Number]
        ,[Date_Sampling]
        ,[Date_Report]
        ,[Tonnage]
        ,[Name]
        ,[Destination]
        ,[Temperature]
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
    FROM UPLOAD_BARGE_LOADING
    where Company = @p_company
    and CONVERT(varchar(10),CreatedOn,120) = @p_date
    and Sheet = @p_sheet
    order by JOB_Number, ID_Number
end