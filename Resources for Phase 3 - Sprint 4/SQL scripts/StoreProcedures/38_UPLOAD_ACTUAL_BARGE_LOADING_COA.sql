IF OBJECT_ID('UPLOAD_ACTUAL_BARGE_LOADING_COA') IS NULL
    EXEC('CREATE PROCEDURE UPLOAD_ACTUAL_BARGE_LOADING_COA AS SET NOCOUNT ON;')
GO

alter procedure [dbo].[UPLOAD_ACTUAL_BARGE_LOADING_COA] @p_file integer
as
begin
    declare @vCount integer = 0
    declare @vCount2 integer = 0
    declare @vStatus varchar(32) = ''
    declare @vMessage varchar(8000) = ''
    
    declare @vNow datetime = GetDate()
    
    BEGIN TRY  
        -- for Status: New
        insert into UPLOAD_BARGE_LOADING_COA 
        (
            [TEMPORARY]
            ,[Sheet]
            ,[Job_Number]
            ,[Lab_ID]
            ,[Port_of_Loading]
            ,[Port_Destination]
            ,[Service_Trip_No]
            ,[Tug_Boat]
            ,[Quantity_Draft_Survey]
            ,[Loading_Date]
            ,[Report_Date]
            ,[Report_To]
            ,[Total_Moisture]
            ,[Moisture]
            ,[Ash]
            ,[Volatile_Matter]
            ,[Fixed_Carbon]
            ,[Total_Sulfur]
            ,[Gross_Caloric_adb]
            ,[Gross_Caloric_db]
            ,[Gross_Caloric_ar]
            ,[Gross_Caloric_daf]
            ,[Report_Location_Date]
            ,[Report_Creator]
            ,[Position]
            ,[Company]
            ,[CreatedBy]
            ,[CreatedOn]
            ,[LastModifiedBy]
            ,[LastModifiedOn]
        )
        select
            RecordId
            ,[Sheet]
            ,[Job_Number]
            ,[Lab_ID]
            ,[Port_of_Loading]
            ,[Port_Destination]
            ,[Service_Trip_No]
            ,[Tug_Boat]
            ,[Quantity_Draft_Survey]
            ,[Loading_Date]
            ,[Report_Date]
            ,[Report_To]
            ,[Total_Moisture]
            ,[Moisture]
            ,[Ash]
            ,[Volatile_Matter]
            ,[Fixed_Carbon]
            ,[Total_Sulfur]
            ,[Gross_Caloric_adb]
            ,[Gross_Caloric_db]
            ,[Gross_Caloric_ar]
            ,[Gross_Caloric_daf]
            ,[Report_Location_Date]
            ,[Report_Creator]
            ,[Position]
            ,[Company]
            ,[CreatedBy]
            ,[CreatedOn]
            ,[LastModifiedBy]
            ,[LastModifiedOn]
        from TEMPORARY_BARGE_LOADING_COA
        where 
            File_Physical = @p_file
            and [Status] = 'New'

        select @vStatus = 'Ok'
        select @vMessage = 'UPLOAD_ACTUAL_BARGE_LOADING_COA'
    END TRY  
    BEGIN CATCH  
         select @vCount = 0
         select @vCount2 = 0
         select @vStatus = 'Error'
         select @vMessage = ERROR_MESSAGE()
    END CATCH  

    -- tampilkan "Hasil Eksekusi"
    -- Tidak usah ditampilkan
    --select @vCount as cCount, @vCount2 as cCount2, @vStatus as cStatus, @vMessage as cMessage
end
