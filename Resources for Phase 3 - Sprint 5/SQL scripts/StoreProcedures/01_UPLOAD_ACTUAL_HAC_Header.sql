IF OBJECT_ID('UPLOAD_ACTUAL_HAC_Header') IS NULL
    EXEC('CREATE PROCEDURE UPLOAD_ACTUAL_HAC_Header AS SET NOCOUNT ON;')
GO

alter procedure [dbo].[UPLOAD_ACTUAL_HAC_Header] @p_file integer
as
begin
    BEGIN TRY  
        insert into UPLOAD_HAC_Header 
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
        from TEMPORARY_HAC_Header
        where 
            File_Physical = @p_file
    END TRY  
    BEGIN CATCH
    END CATCH
end
