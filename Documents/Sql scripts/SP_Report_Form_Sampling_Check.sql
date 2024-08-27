IF OBJECT_ID('Report_Form_Sampling_Check') IS NULL
    EXEC('CREATE PROCEDURE Report_Form_Sampling_Check AS SET NOCOUNT ON;')
GO

alter procedure [dbo].[Report_Form_Sampling_Check](@p_samplingRequest int)
as
begin
    select count(RecordId) as Count2
    from UPLOAD_Sampling_ROM
    where Lab_ID in
    (
        select LabId from SamplingRequest_Lab
        where SamplingRequest = @p_samplingRequest
    )
end;