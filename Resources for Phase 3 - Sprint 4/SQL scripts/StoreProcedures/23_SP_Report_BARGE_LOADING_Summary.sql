IF OBJECT_ID('Report_BARGE_LOADING_Summary') IS NULL
    EXEC('CREATE PROCEDURE Report_BARGE_LOADING_Summary AS SET NOCOUNT ON;')
GO

ALTER PROCEDURE [dbo].[Report_BARGE_LOADING_Summary] @p_company VARCHAR(32), 
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
               round(SUM(TM),2) AS TM, 
               round(SUM([M]),2) [M], 
               round(SUM([ASH_adb]),2) [ASH_adb], 
               round(SUM([ASH_arb]),2) [ASH_arb], 
               round(SUM([VM_adb]),2) [VM_adb], 
               round(SUM([VM_arb]),2) [VM_arb], 
               round(SUM([FC_adb]),2) [FC_adb], 
               round(SUM([FC_arb]),2) [FC_arb], 
               round(SUM([TS_adb]),2) [TS_adb], 
               round(SUM([TS_arb]),2) [TS_arb], 
               round(SUM(CV_adb),2) CV_adb, 
               round(SUM(CV_db),2) CV_db, 
               round(SUM(CV_adb),2) CV_adb, 
               round(SUM(CV_arb),2) CV_arb, 
               round(SUM(CV_daf),2) CV_daf, 
               round(SUM(CV_ad_15),2) CV_ad_15, 
               round(SUM(CV_ad_16),2) CV_ad_16, 
               round(SUM(CV_ad_17),2) CV_ad_17, 
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
            FROM UPLOAD_BARGE_LOADING
            WHERE Company = @p_company
                  AND CONVERT(VARCHAR(10), CreatedOn, 120) >= @dateFrom
                  AND CONVERT(VARCHAR(10), CreatedOn, 120) <= @dateTo
        ) x
        GROUP BY Sheet
        ORDER BY Sheet;
    END;
GO
