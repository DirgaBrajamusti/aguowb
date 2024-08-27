IF OBJECT_ID('Report_SAMPLING_ROM') IS NULL
    EXEC('CREATE PROCEDURE Report_SAMPLING_ROM AS SET NOCOUNT ON;')
GO

alter procedure [dbo].[Report_SAMPLING_ROM] @p_company varchar(32), @p_date varchar(10), @p_dateTo varchar(10)
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
    where Company = @p_company
        and CONVERT(varchar(10),CreatedOn,120) >= @dateFrom
        and CONVERT(varchar(10),CreatedOn,120) <= @dateTo
    order by Date_Sampling, LOT, Lab_ID
end