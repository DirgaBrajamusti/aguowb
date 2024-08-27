IF OBJECT_ID('Report_Form_Sampling_Check') IS NULL
    EXEC('CREATE PROCEDURE Report_Form_Sampling_Check AS SET NOCOUNT ON;')
GO

alter procedure [dbo].[Report_Form_Sampling_Check](@p_samplingRequest int)
as
begin
    select count(Lab_ID) as Count2
    from
    (
        select upload.Lab_ID
        from SamplingRequest_Lab lab
          , UPLOAD_Sampling_ROM upload
        where lab.SamplingRequest = @p_samplingRequest
            and lab.LabId = upload.Lab_ID
            and lab.Company = upload.Company
            and lab.CreatedOn_Year_Only = upload.CreatedOn_Year_Only

        union

        select upload.Lab_ID
        from SamplingRequest_Lab lab
          , UPLOAD_HAC upload
        where lab.SamplingRequest = @p_samplingRequest
            and lab.LabId = upload.Lab_ID
            and lab.Company = upload.Company
            and lab.CreatedOn_Year_Only = upload.CreatedOn_Year_Only
    ) x
end;