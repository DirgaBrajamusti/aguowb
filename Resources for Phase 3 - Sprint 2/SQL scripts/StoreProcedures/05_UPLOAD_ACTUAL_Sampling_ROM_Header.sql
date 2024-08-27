IF OBJECT_ID('UPLOAD_ACTUAL_Sampling_ROM_Header') IS NULL
    EXEC('CREATE PROCEDURE UPLOAD_ACTUAL_Sampling_ROM_Header AS SET NOCOUNT ON;')
GO

alter procedure [dbo].[UPLOAD_ACTUAL_Sampling_ROM_Header] @p_file integer
as
begin
    BEGIN TRY  
        insert into UPLOAD_Sampling_ROM_Header 
        (
            File_Physical
            , [TEMPORARY]
            , Company
            , CreatedBy
            , CreatedOn
            , Date_Detail
            , Job_No
            , Report_To
            , Method1
            , Method2
            , Method3
            , Method4
        )
        select
            File_Physical
            , RecordId
            , Company
            , CreatedBy
            , CreatedOn
            , Date_Detail
            , Job_No
            , Report_To
            , Method1
            , Method2
            , Method3
            , Method4
        from TEMPORARY_Sampling_ROM_Header
        where 
            File_Physical = @p_file
    END TRY  
    BEGIN CATCH
    END CATCH
end
