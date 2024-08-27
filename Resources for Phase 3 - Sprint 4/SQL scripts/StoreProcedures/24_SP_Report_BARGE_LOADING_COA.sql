IF OBJECT_ID('Report_BARGE_LOADING_COA') IS NULL
    EXEC('CREATE PROCEDURE Report_BARGE_LOADING_COA AS SET NOCOUNT ON;')
GO

ALTER PROCEDURE [dbo].[Report_BARGE_LOADING_COA] @p_company VARCHAR(32), 
                                                 @p_date    VARCHAR(10), 
                                                 @p_dateTo  VARCHAR(10)
AS
    BEGIN
        DECLARE @dateFrom VARCHAR(10);
        DECLARE @dateTo VARCHAR(10);
        IF(@p_date <= @p_dateTo)
            BEGIN
                SELECT @dateFrom = @p_date;
                SELECT @dateTo = @p_dateTo;
            END;
            ELSE
            BEGIN
                SELECT @dateFrom = @p_dateTo;
                SELECT @dateTo = @p_date;
            END;
        --
        SELECT [Sheet], 
               [Job_Number], 
               [Lab_ID], 
               [Port_of_Loading], 
               [Port_Destination], 
               [Service_Trip_No], 
               [Tug_Boat], 
               [Quantity_Draft_Survey], 
               --format([Loading_Date],'MMMM dd, yyyy')
               [Loading_Date], 
               --format([Report_Date],'MMMM dd, yyyy')
               [Report_Date], 
               [Report_To], 
               [Total_Moisture], 
               [Moisture], 
               [Ash], 
               [Volatile_Matter], 
               [Fixed_Carbon], 
               [Total_Sulfur], 
               [Gross_Caloric_adb], 
               [Gross_Caloric_db], 
               [Gross_Caloric_ar], 
               [Gross_Caloric_daf], 
               [Report_Location_Date], 
               [Report_Creator], 
               Position
        FROM UPLOAD_BARGE_LOADING_COA
        WHERE Company = @p_company
              AND CONVERT(VARCHAR(10), CreatedOn, 120) >= @dateFrom
              AND CONVERT(VARCHAR(10), CreatedOn, 120) <= @dateTo;
    END;
GO
