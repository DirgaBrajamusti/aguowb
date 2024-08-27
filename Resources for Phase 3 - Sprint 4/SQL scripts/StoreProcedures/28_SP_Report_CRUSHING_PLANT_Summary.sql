IF OBJECT_ID('Report_CRUSHING_PLANT_Summary') IS NULL
    EXEC('CREATE PROCEDURE Report_CRUSHING_PLANT_Summary AS SET NOCOUNT ON;')
GO

ALTER PROCEDURE [dbo].[Report_CRUSHING_PLANT_Summary] @p_company VARCHAR(32), 
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
        SELECT Sheet, 
               SUM([Tonnage]) AS [Tonnage], 
               ROUND(SUM(TM), 2) AS TM, 
               ROUND(SUM([M]), 2) [M], 
               ROUND(SUM([ASH_adb]), 2) [ASH_adb], 
               ROUND(SUM([ASH_arb]), 2) [ASH_arb], 
               ROUND(SUM([VM_adb]), 2) [VM_adb], 
               ROUND(SUM([VM_arb]), 2) [VM_arb], 
               ROUND(SUM([FC_adb]), 2) [FC_adb], 
               ROUND(SUM([FC_arb]), 2) [FC_arb], 
               ROUND(SUM([TS_adb]), 2) [TS_adb], 
               ROUND(SUM([TS_arb]), 2) [TS_arb], 
               ROUND(SUM(CV_adb), 2) CV_adb, 
               ROUND(SUM(CV_db), 2) CV_db, 
               ROUND(SUM(CV_adb), 2) CV_adb, 
               ROUND(SUM(CV_arb), 2) CV_arb, 
               ROUND(SUM(CV_daf), 2) CV_daf, 
               ROUND(SUM(CV_ad_15), 2) CV_ad_15, 
               ROUND(SUM(CV_ad_16), 2) CV_ad_16, 
               ROUND(SUM(CV_ad_17), 2) CV_ad_17, 
               '' AS Remark
        FROM
        (
            SELECT Sheet, 
                   [Tonnage] AS [Tonnage], 
                   [Tonnage] * ([TM] / (SUM([Tonnage]) OVER(PARTITION BY Sheet))) [TM], 
                   [Tonnage] * (M / (SUM([Tonnage]) OVER(PARTITION BY Sheet))) [M], 
                   [Tonnage] * ([ASH_adb] / (SUM([Tonnage]) OVER(PARTITION BY Sheet))) [ASH_adb], 
                   [Tonnage] * ([ASH_arb] / (SUM([Tonnage]) OVER(PARTITION BY Sheet))) [ASH_arb], 
                   [Tonnage] * ([VM_adb] / (SUM([Tonnage]) OVER(PARTITION BY Sheet))) [VM_adb], 
                   [Tonnage] * ([VM_arb] / (SUM([Tonnage]) OVER(PARTITION BY Sheet))) [VM_arb], 
                   [Tonnage] * ([FC_adb] / (SUM([Tonnage]) OVER(PARTITION BY Sheet))) [FC_adb], 
                   [Tonnage] * ([FC_arb] / (SUM([Tonnage]) OVER(PARTITION BY Sheet))) [FC_arb], 
                   [Tonnage] * ([TS_adb] / (SUM([Tonnage]) OVER(PARTITION BY Sheet))) [TS_adb], 
                   [Tonnage] * ([TS_arb] / (SUM([Tonnage]) OVER(PARTITION BY Sheet))) [TS_arb], 
                   [Tonnage] * (CONVERT(DECIMAL(10, 0), [CV_adb]) / (SUM([Tonnage]) OVER(PARTITION BY Sheet))) CV_adb, 
                   [Tonnage] * (CONVERT(DECIMAL(10, 0), CV_db) / (SUM([Tonnage]) OVER(PARTITION BY Sheet))) CV_db, 
                   [Tonnage] * (CONVERT(DECIMAL(10, 0), CV_arb) / (SUM([Tonnage]) OVER(PARTITION BY Sheet))) CV_arb, 
                   [Tonnage] * (CONVERT(DECIMAL(10, 0), CV_daf) / (SUM([Tonnage]) OVER(PARTITION BY Sheet))) CV_daf, 
                   [Tonnage] * (CONVERT(DECIMAL(10, 0), CV_ad_15) / (SUM([Tonnage]) OVER(PARTITION BY Sheet))) CV_ad_15, 
                   [Tonnage] * (CONVERT(DECIMAL(10, 0), CV_ad_16) / (SUM([Tonnage]) OVER(PARTITION BY Sheet))) CV_ad_16, 
                   [Tonnage] * (CONVERT(DECIMAL(10, 0), CV_ad_17) / (SUM([Tonnage]) OVER(PARTITION BY Sheet))) CV_ad_17
            FROM [UPLOAD_CRUSHING_PLANT]
            WHERE Company = @p_company
                  AND CONVERT(VARCHAR(10), CreatedOn, 120) >= @dateFrom
                  AND CONVERT(VARCHAR(10), CreatedOn, 120) <= @dateTo
        ) x
        GROUP BY Sheet
        ORDER BY Sheet;
    END;
GO
