IF OBJECT_ID('Report_Form_Sampling_Header') IS NULL
    EXEC('CREATE PROCEDURE Report_Form_Sampling_Header AS SET NOCOUNT ON;')
GO

alter procedure [dbo].[Report_Form_Sampling_Header](@p_samplingRequest int)
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
    from UPLOAD_Sampling_ROM_Header
    where RecordId in
    (
        select Header
        from UPLOAD_Sampling_ROM
        where Lab_ID in
        (
            select LabId from SamplingRequest_Lab
            where SamplingRequest = @p_samplingRequest
        )
    )
    order by RecordId desc
end