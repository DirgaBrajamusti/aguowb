IF OBJECT_ID('Report_CRUSHING_PLANT_Header_Summary') IS NULL
    EXEC('CREATE PROCEDURE Report_CRUSHING_PLANT_Header_Summary AS SET NOCOUNT ON;')
GO


ALTER PROCEDURE [dbo].[Report_CRUSHING_PLANT_Header_Summary] @p_company VARCHAR(32), 
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
        SELECT TOP 1 Date_Detail AS DATE_1, 
                     '' AS DATE_2, 
                     JOB_NO, 
                     Report_To, 
                     Method1, 
                     Method2, 
                     Method3, 
                     Method4
        FROM UPLOAD_CRUSHING_PLANT_Header
        WHERE Company = @p_company
              AND CONVERT(VARCHAR(10), CreatedOn, 120) >= @dateFrom
              AND CONVERT(VARCHAR(10), CreatedOn, 120) <= @dateTo
        ORDER BY RecordId DESC;
    END;
GO
