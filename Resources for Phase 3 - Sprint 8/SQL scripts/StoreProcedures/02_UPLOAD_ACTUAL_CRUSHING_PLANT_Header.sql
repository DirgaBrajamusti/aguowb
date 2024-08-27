IF OBJECT_ID('UPLOAD_ACTUAL_CRUSHING_PLANT_Header') IS NULL
    EXEC('CREATE PROCEDURE UPLOAD_ACTUAL_CRUSHING_PLANT_Header AS SET NOCOUNT ON;')
GO

alter procedure [dbo].[UPLOAD_ACTUAL_CRUSHING_PLANT_Header] @p_file integer
as
begin
    BEGIN TRY  
        insert into UPLOAD_CRUSHING_PLANT_Header 
        (
            File_Physical
            , [TEMPORARY]
            , Company
            , Sheet
            , CreatedBy
            , CreatedOn
            , Date_Detail
            , Job_No
            , Report_To
            , Method1
            , Method2
            , Method3
            , Method4
            , CreatedOn_Date_Only
            , CreatedOn_Year_Only
        )
        select
            File_Physical
            , RecordId
            , Company
            , Sheet
            , CreatedBy
            , CreatedOn
            , Date_Detail
            , Job_No
            , Report_To
            , Method1
            , Method2
            , Method3
            , Method4
            , CreatedOn_Date_Only
            , CreatedOn_Year_Only
        from TEMPORARY_CRUSHING_PLANT_Header
        where 
            File_Physical = @p_file
    END TRY  
    BEGIN CATCH
    END CATCH 
end