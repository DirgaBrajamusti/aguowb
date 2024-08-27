IF OBJECT_ID('Report_Form_Sampling') IS NULL
    EXEC('CREATE PROCEDURE Report_Form_Sampling AS SET NOCOUNT ON;')
GO

alter procedure [dbo].[Report_Form_Sampling](@p_samplingRequest int)
as
begin
    select
        RecordId
        ,[TEMPORARY]
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
    from UPLOAD_Sampling_ROM
    where Lab_ID in
    (
        select LabId from SamplingRequest_Lab
        where SamplingRequest = @p_samplingRequest
    )
    order by RecordId
end;