IF OBJECT_ID('Report_CRUSHING_PLANT_Header') IS NULL
    EXEC('CREATE PROCEDURE Report_CRUSHING_PLANT_Header AS SET NOCOUNT ON;')
GO

alter procedure [dbo].[Report_CRUSHING_PLANT_Header] @p_company varchar(32), @p_sheet varchar(32), @p_date varchar(10)
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
    from UPLOAD_CRUSHING_PLANT_Header
    where Company = @p_company
        and Sheet = @p_sheet
        and CONVERT(varchar(10),CreatedOn,120) = @p_date
    order by RecordId desc
end