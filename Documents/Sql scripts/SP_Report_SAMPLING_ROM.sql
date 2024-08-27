IF OBJECT_ID('Report_SAMPLING_ROM') IS NULL
    EXEC('CREATE PROCEDURE Report_SAMPLING_ROM AS SET NOCOUNT ON;')
GO

alter procedure [dbo].[Report_SAMPLING_ROM] @p_company varchar(32), @p_date varchar(10)
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
    where Company = @p_company
    and CONVERT(varchar(10),CreatedOn,120) = @p_date
    order by Date_Sampling, LOT, Lab_ID
end