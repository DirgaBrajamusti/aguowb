IF OBJECT_ID('UPLOAD_ACTUAL_GEOLOGY_EXPLORASI_Header') IS NULL
    EXEC('CREATE PROCEDURE UPLOAD_ACTUAL_GEOLOGY_EXPLORASI_Header AS SET NOCOUNT ON;')
GO

alter procedure [dbo].[UPLOAD_ACTUAL_GEOLOGY_EXPLORASI_Header] @p_file integer
as
begin
    declare @vNow datetime = GetDate()

    BEGIN TRY  
        -- for Status: New
        insert into UPLOAD_Geology_Explorasi_Header 
        (
            File_Physical
            , [TEMPORARY]
            , Company
            , CreatedBy
            , CreatedOn
            , Date_Detail
            , Job_No
            , Report_To
            , Date_Received
            , Nomor
            , CreatedOn_Date_Only
            , CreatedOn_Year_Only
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
            , Date_Received
            , Nomor
            , CreatedOn_Date_Only
            , CreatedOn_Year_Only
        from TEMPORARY_Geology_Explorasi_Header
        where 
            File_Physical = @p_file
    END TRY  
    BEGIN CATCH  
    END CATCH
end
