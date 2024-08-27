IF OBJECT_ID('Report_HAC') IS NULL
    EXEC('CREATE PROCEDURE Report_HAC AS SET NOCOUNT ON;')
GO

ALTER procedure [dbo].[Report_HAC] @p_company varchar(32), @p_date varchar(10), @p_dateTo varchar(10)
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
      ,Date_Sampling
      ,Day_work
      ,Tonnage
      ,LOT
      ,Lab_ID
      ,TM
      ,M
      ,ASH
      ,TS
      ,CV
      ,Remark
    FROM
      UPLOAD_HAC
    where Company = @p_company
        and CONVERT(varchar(10),CreatedOn,120) >= @dateFrom
        and CONVERT(varchar(10),CreatedOn,120) <= @dateTo
	order by Lab_ID;
end
GO
