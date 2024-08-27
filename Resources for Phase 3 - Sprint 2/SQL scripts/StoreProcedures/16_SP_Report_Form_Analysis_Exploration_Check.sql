IF OBJECT_ID('Report_Form_Analysis_Exploration_Check') IS NULL
    EXEC('CREATE PROCEDURE Report_Form_Analysis_Exploration_Check AS SET NOCOUNT ON;')
GO

alter procedure [dbo].[Report_Form_Analysis_Exploration_Check](@p_analysisRequest int)
as
begin
    select count(RecordId) as Count2
    from UPLOAD_Geology_Explorasi
    where Lab_ID in
    (
        select LabId from AnalysisRequest_Detail
        where AnalysisRequest = @p_analysisRequest
    )
end;