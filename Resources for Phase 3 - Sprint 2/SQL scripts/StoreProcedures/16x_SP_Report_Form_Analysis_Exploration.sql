IF OBJECT_ID('Report_Form_Analysis_Exploration') IS NULL
    EXEC('CREATE PROCEDURE Report_Form_Analysis_Exploration AS SET NOCOUNT ON;')
GO

alter procedure [dbo].[Report_Form_Analysis_Exploration](@p_analysisRequest int)
as
begin
    select
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
    from UPLOAD_Geology_Explorasi
    where Lab_ID in
    (
        select LabId from AnalysisRequest_Detail
        where AnalysisRequest = @p_analysisRequest
    )
    order by RecordId
end;