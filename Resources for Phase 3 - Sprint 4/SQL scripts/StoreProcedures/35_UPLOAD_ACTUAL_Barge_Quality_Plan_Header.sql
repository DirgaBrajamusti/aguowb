IF OBJECT_ID('UPLOAD_ACTUAL_Barge_Quality_Plan_Header') IS NULL
    EXEC('CREATE PROCEDURE UPLOAD_ACTUAL_Barge_Quality_Plan_Header AS SET NOCOUNT ON;')
GO

alter procedure [dbo].[UPLOAD_ACTUAL_Barge_Quality_Plan_Header] @p_file integer
as
begin
    BEGIN TRY
        insert into UPLOAD_Barge_Quality_Plan_Header
        (
            File_Physical
            , [TEMPORARY]
            , Site
            , Sheet
            , TripNo
            , Vessel
            , Buyer
            , LaycanFrom
            , LaycanTo
            , CreatedBy
            , CreatedOn
        )
        select
            File_Physical
            , RecordId
            , Site
            , Sheet
            , TripNo
            , Vessel
            , Buyer
            , LaycanFrom
            , LaycanTo
            , CreatedBy
            , CreatedOn
        from TEMPORARY_Barge_Quality_Plan_Header
        where 
            File_Physical = @p_file
    END TRY  
    BEGIN CATCH
    END CATCH
end
