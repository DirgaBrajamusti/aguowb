/* Up Script*/
ALTER PROCEDURE [dbo].[Report_BARGE_LOADING] @p_company VARCHAR(32), 
                                             @p_sheet   VARCHAR(32), 
                                             @p_date    VARCHAR(10), 
                                             @p_dateTo  VARCHAR(10)
AS
    BEGIN
        DECLARE @dateFrom VARCHAR(10);
        DECLARE @dateTo VARCHAR(10);
        DECLARE @TM_Average NUMERIC(10, 2);
        DECLARE @ASH_Average NUMERIC(10, 2);
        DECLARE @TS_Average NUMERIC(10, 2);
        DECLARE @CV_Average NUMERIC(10, 2);
        --
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
        --get Average value
        SELECT @TM_Average = ROUND(SUM(TM), 2), 
               @ASH_Average = ROUND(SUM([ASH_adb]), 2), 
               @TS_Average = ROUND(SUM([TS_adb]), 2), 
               @CV_Average = ROUND(SUM(CV_adb), 2)
        FROM
        (
            SELECT Sheet, 
                   [Tonnage] AS [Tonnage], 
                   [Tonnage] * ([TM] / (SUM([Tonnage]) OVER(PARTITION BY Sheet))) [TM], 
                   [Tonnage] * ([ASH_adb] / (SUM([Tonnage]) OVER(PARTITION BY Sheet))) [ASH_adb], 
                   [Tonnage] * ([TS_adb] / (SUM([Tonnage]) OVER(PARTITION BY Sheet))) [TS_adb], 
                   [Tonnage] * (CONVERT(DECIMAL(10, 0), [CV_adb]) / (SUM([Tonnage]) OVER(PARTITION BY Sheet))) CV_adb
            FROM UPLOAD_BARGE_LOADING
            WHERE Company = @p_company
                  AND CONVERT(VARCHAR(10), CreatedOn, 120) >= @dateFrom
                  AND CONVERT(VARCHAR(10), CreatedOn, 120) <= @dateTo
        		  AND (Sheet LIKE '%' + @p_sheet OR @p_sheet = 'all')
        ) x;
        --
        --
        SELECT [JOB_Number], 
               [ID_Number], 
               [Service_Trip_Number], 
               [Date_Sampling], 
               [Date_Report], 
               [Tonnage], 
               [Name], 
               [Destination], 
               [Temperature], 
               [TM], 
               [M], 
               [ASH_adb], 
               [ASH_arb], 
               [VM_adb], 
               [VM_arb], 
               [FC_adb], 
               [FC_arb], 
               [TS_adb], 
               [TS_arb], 
               CONVERT(DECIMAL(10, 0), [CV_adb]) AS CV_adb, 
               CONVERT(DECIMAL(10, 0), [CV_db]) AS CV_db, 
               CONVERT(DECIMAL(10, 0), [CV_arb]) AS CV_arb, 
               CONVERT(DECIMAL(10, 0), [CV_daf]) AS CV_daf, 
               CONVERT(DECIMAL(10, 0), [CV_ad_15]) AS CV_ad_15, 
               CONVERT(DECIMAL(10, 0), [CV_ad_16]) AS CV_ad_16, 
               CONVERT(DECIMAL(10, 0), [CV_ad_17]) AS CV_ad_17, 
               ROW_NUMBER() OVER(
               ORDER BY ID_Number) row_num, 
               ASH_adb AS ASH_Actual, 
               ISNULL(ASH_Plan, 0) AS ASH_Plan, 
               @ASH_Average AS ASH_Average, 
               TS_adb AS TS_Actual, 
               ISNULL(TS_Plan, 0) AS TS_Plan, 
               @TS_Average AS TS_Average, 
               [TM] AS TM_Actual, 
               ISNULL(TM_Plan, 0) AS TM_Plan, 
               @TM_Average AS TM_Average, 
               CONVERT(DECIMAL(10, 0), [CV_adb]) AS CV_Actual, 
               ISNULL(CV_Plan, 0) AS CV_Plan, 
               @CV_Average AS CV_Average, 
               [Remark]
        FROM UPLOAD_BARGE_LOADING
        WHERE Company = @p_company
              AND CONVERT(VARCHAR(10), CreatedOn, 120) >= @dateFrom
              AND CONVERT(VARCHAR(10), CreatedOn, 120) <= @dateTo
    		  AND (Sheet LIKE '%' + @p_sheet OR @p_sheet = 'all')
        --ORDER BY JOB_Number, ID_Number
        --req sprint4
        ORDER BY ID_Number;
    END;
GO;

ALTER PROCEDURE [dbo].[Report_CRUSHING_PLANT] @p_company VARCHAR(32), 
                                              @p_sheet   VARCHAR(32), 
                                              @p_date    VARCHAR(10), 
                                              @p_dateTo  VARCHAR(10)
AS
    BEGIN
        DECLARE @dateFrom VARCHAR(10);
        DECLARE @dateTo VARCHAR(10);
        DECLARE @TM_Average NUMERIC(10, 2);
        DECLARE @ASH_Average NUMERIC(10, 2);
        DECLARE @TS_Average NUMERIC(10, 2);
        DECLARE @CV_Average NUMERIC(10, 2);
        --
        --
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
        --get Average value
        SELECT @TM_Average = ROUND(SUM(TM), 2), 
               @ASH_Average = ROUND(SUM([ASH_adb]), 2), 
               @TS_Average = ROUND(SUM([TS_adb]), 2), 
               @CV_Average = ROUND(SUM(CV_adb), 2)
        FROM
        (
            SELECT Sheet, 
                   [Tonnage] AS [Tonnage], 
                   [Tonnage] * ([TM] / (SUM([Tonnage]) OVER(PARTITION BY Sheet))) [TM], 
                   [Tonnage] * ([ASH_adb] / (SUM([Tonnage]) OVER(PARTITION BY Sheet))) [ASH_adb], 
                   [Tonnage] * ([TS_adb] / (SUM([Tonnage]) OVER(PARTITION BY Sheet))) [TS_adb], 
                   [Tonnage] * (CONVERT(DECIMAL(10, 0), [CV_adb]) / (SUM([Tonnage]) OVER(PARTITION BY Sheet))) CV_adb
            FROM [UPLOAD_CRUSHING_PLANT]
            WHERE Company = @p_company
                  AND CONVERT(VARCHAR(10), CreatedOn, 120) >= @dateFrom
                  AND CONVERT(VARCHAR(10), CreatedOn, 120) <= @dateTo
                  AND (Sheet LIKE '%' + @p_sheet OR @p_sheet = 'all')
        ) x;
        --
        --
        SELECT [Date_Production], 
               [Shift_Work], 
               [Tonnage], 
               [Sample_ID], 
               [TM], 
               [M], 
               [ASH_adb], 
               [ASH_arb], 
               [VM_adb], 
               [VM_arb], 
               [FC_adb], 
               [FC_arb], 
               [TS_adb], 
               [TS_arb], 
               CONVERT(DECIMAL(10, 0), [CV_adb]) AS CV_adb, 
               CONVERT(DECIMAL(10, 0), [CV_db]) AS CV_db, 
               CONVERT(DECIMAL(10, 0), [CV_arb]) AS CV_arb, 
               CONVERT(DECIMAL(10, 0), [CV_daf]) AS CV_daf, 
               CONVERT(DECIMAL(10, 0), [CV_ad_15]) AS CV_ad_15, 
               CONVERT(DECIMAL(10, 0), [CV_ad_16]) AS CV_ad_16, 
               CONVERT(DECIMAL(10, 0), [CV_ad_17]) AS CV_ad_17, 
               ROW_NUMBER() OVER(
               ORDER BY [Sample_ID]) row_num, 
               ASH_adb AS ASH_Actual, 
               ISNULL(ASH_Plan, 0) AS ASH_Plan, 
               @ASH_Average AS ASH_Average, 
               TS_adb AS TS_Actual, 
               ISNULL(TS_Plan, 0) AS TS_Plan, 
               @TS_Average AS TS_Average, 
               [TM] AS TM_Actual, 
               ISNULL(TM_Plan, 0) AS TM_Plan, 
               @TM_Average AS TM_Average, 
               CONVERT(DECIMAL(10, 0), [CV_adb]) AS CV_Actual, 
               ISNULL(CV_Plan, 0) AS CV_Plan, 
               @CV_Average AS CV_Average, 
               [Remark]
        FROM [UPLOAD_CRUSHING_PLANT]
        WHERE Company = @p_company
              AND CONVERT(VARCHAR(10), CreatedOn, 120) >= @dateFrom
              AND CONVERT(VARCHAR(10), CreatedOn, 120) <= @dateTo
              AND (Sheet LIKE '%' + @p_sheet OR @p_sheet = 'all')
        --ORDER BY Date_Production, Shift_Work
        --req sprint4
        ORDER BY [Sample_ID];
    END;
GO;


/*Down Script*/
ALTER PROCEDURE [dbo].[Report_BARGE_LOADING] @p_company VARCHAR(32), 
                                             @p_sheet   VARCHAR(32), 
                                             @p_date    VARCHAR(10), 
                                             @p_dateTo  VARCHAR(10)
AS
    BEGIN
        DECLARE @dateFrom VARCHAR(10);
        DECLARE @dateTo VARCHAR(10);
        DECLARE @TM_Average NUMERIC(10, 2);
        DECLARE @ASH_Average NUMERIC(10, 2);
        DECLARE @TS_Average NUMERIC(10, 2);
        DECLARE @CV_Average NUMERIC(10, 2);
        --
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
        --get Average value
        SELECT @TM_Average = ROUND(SUM(TM), 2), 
               @ASH_Average = ROUND(SUM([ASH_adb]), 2), 
               @TS_Average = ROUND(SUM([TS_adb]), 2), 
               @CV_Average = ROUND(SUM(CV_adb), 2)
        FROM
        (
            SELECT Sheet, 
                   [Tonnage] AS [Tonnage], 
                   [Tonnage] * ([TM] / (SUM([Tonnage]) OVER(PARTITION BY Sheet))) [TM], 
                   [Tonnage] * ([ASH_adb] / (SUM([Tonnage]) OVER(PARTITION BY Sheet))) [ASH_adb], 
                   [Tonnage] * ([TS_adb] / (SUM([Tonnage]) OVER(PARTITION BY Sheet))) [TS_adb], 
                   [Tonnage] * (CONVERT(DECIMAL(10, 0), [CV_adb]) / (SUM([Tonnage]) OVER(PARTITION BY Sheet))) CV_adb
            FROM UPLOAD_BARGE_LOADING
            WHERE Company = @p_company
                  AND CONVERT(VARCHAR(10), CreatedOn, 120) >= @dateFrom
                  AND CONVERT(VARCHAR(10), CreatedOn, 120) <= @dateTo
        		  AND Sheet = @p_sheet
        ) x;
        --
        --
        SELECT [JOB_Number], 
               [ID_Number], 
               [Service_Trip_Number], 
               [Date_Sampling], 
               [Date_Report], 
               [Tonnage], 
               [Name], 
               [Destination], 
               [Temperature], 
               [TM], 
               [M], 
               [ASH_adb], 
               [ASH_arb], 
               [VM_adb], 
               [VM_arb], 
               [FC_adb], 
               [FC_arb], 
               [TS_adb], 
               [TS_arb], 
               CONVERT(DECIMAL(10, 0), [CV_adb]) AS CV_adb, 
               CONVERT(DECIMAL(10, 0), [CV_db]) AS CV_db, 
               CONVERT(DECIMAL(10, 0), [CV_arb]) AS CV_arb, 
               CONVERT(DECIMAL(10, 0), [CV_daf]) AS CV_daf, 
               CONVERT(DECIMAL(10, 0), [CV_ad_15]) AS CV_ad_15, 
               CONVERT(DECIMAL(10, 0), [CV_ad_16]) AS CV_ad_16, 
               CONVERT(DECIMAL(10, 0), [CV_ad_17]) AS CV_ad_17, 
               ROW_NUMBER() OVER(
               ORDER BY ID_Number) row_num, 
               ASH_adb AS ASH_Actual, 
               ISNULL(ASH_Plan, 0) AS ASH_Plan, 
               @ASH_Average AS ASH_Average, 
               TS_adb AS TS_Actual, 
               ISNULL(TS_Plan, 0) AS TS_Plan, 
               @TS_Average AS TS_Average, 
               [TM] AS TM_Actual, 
               ISNULL(TM_Plan, 0) AS TM_Plan, 
               @TM_Average AS TM_Average, 
               CONVERT(DECIMAL(10, 0), [CV_adb]) AS CV_Actual, 
               ISNULL(CV_Plan, 0) AS CV_Plan, 
               @CV_Average AS CV_Average, 
               [Remark]
        FROM UPLOAD_BARGE_LOADING
        WHERE Company = @p_company
              AND CONVERT(VARCHAR(10), CreatedOn, 120) >= @dateFrom
              AND CONVERT(VARCHAR(10), CreatedOn, 120) <= @dateTo
    		  AND Sheet = @p_sheet
        --ORDER BY JOB_Number, ID_Number
        --req sprint4
        ORDER BY ID_Number;
    END;
GO;

ALTER PROCEDURE [dbo].[Report_CRUSHING_PLANT] @p_company VARCHAR(32), 
                                              @p_sheet   VARCHAR(32), 
                                              @p_date    VARCHAR(10), 
                                              @p_dateTo  VARCHAR(10)
AS
    BEGIN
        DECLARE @dateFrom VARCHAR(10);
        DECLARE @dateTo VARCHAR(10);
        DECLARE @TM_Average NUMERIC(10, 2);
        DECLARE @ASH_Average NUMERIC(10, 2);
        DECLARE @TS_Average NUMERIC(10, 2);
        DECLARE @CV_Average NUMERIC(10, 2);
        --
        --
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
        --get Average value
        SELECT @TM_Average = ROUND(SUM(TM), 2), 
               @ASH_Average = ROUND(SUM([ASH_adb]), 2), 
               @TS_Average = ROUND(SUM([TS_adb]), 2), 
               @CV_Average = ROUND(SUM(CV_adb), 2)
        FROM
        (
            SELECT Sheet, 
                   [Tonnage] AS [Tonnage], 
                   [Tonnage] * ([TM] / (SUM([Tonnage]) OVER(PARTITION BY Sheet))) [TM], 
                   [Tonnage] * ([ASH_adb] / (SUM([Tonnage]) OVER(PARTITION BY Sheet))) [ASH_adb], 
                   [Tonnage] * ([TS_adb] / (SUM([Tonnage]) OVER(PARTITION BY Sheet))) [TS_adb], 
                   [Tonnage] * (CONVERT(DECIMAL(10, 0), [CV_adb]) / (SUM([Tonnage]) OVER(PARTITION BY Sheet))) CV_adb
            FROM [UPLOAD_CRUSHING_PLANT]
            WHERE Company = @p_company
                  AND CONVERT(VARCHAR(10), CreatedOn, 120) >= @dateFrom
                  AND CONVERT(VARCHAR(10), CreatedOn, 120) <= @dateTo
                  AND Sheet = @p_sheet
        ) x;
        --
        --
        SELECT [Date_Production], 
               [Shift_Work], 
               [Tonnage], 
               [Sample_ID], 
               [TM], 
               [M], 
               [ASH_adb], 
               [ASH_arb], 
               [VM_adb], 
               [VM_arb], 
               [FC_adb], 
               [FC_arb], 
               [TS_adb], 
               [TS_arb], 
               CONVERT(DECIMAL(10, 0), [CV_adb]) AS CV_adb, 
               CONVERT(DECIMAL(10, 0), [CV_db]) AS CV_db, 
               CONVERT(DECIMAL(10, 0), [CV_arb]) AS CV_arb, 
               CONVERT(DECIMAL(10, 0), [CV_daf]) AS CV_daf, 
               CONVERT(DECIMAL(10, 0), [CV_ad_15]) AS CV_ad_15, 
               CONVERT(DECIMAL(10, 0), [CV_ad_16]) AS CV_ad_16, 
               CONVERT(DECIMAL(10, 0), [CV_ad_17]) AS CV_ad_17, 
               ROW_NUMBER() OVER(
               ORDER BY [Sample_ID]) row_num, 
               ASH_adb AS ASH_Actual, 
               ISNULL(ASH_Plan, 0) AS ASH_Plan, 
               @ASH_Average AS ASH_Average, 
               TS_adb AS TS_Actual, 
               ISNULL(TS_Plan, 0) AS TS_Plan, 
               @TS_Average AS TS_Average, 
               [TM] AS TM_Actual, 
               ISNULL(TM_Plan, 0) AS TM_Plan, 
               @TM_Average AS TM_Average, 
               CONVERT(DECIMAL(10, 0), [CV_adb]) AS CV_Actual, 
               ISNULL(CV_Plan, 0) AS CV_Plan, 
               @CV_Average AS CV_Average, 
               [Remark]
        FROM [UPLOAD_CRUSHING_PLANT]
        WHERE Company = @p_company
              AND CONVERT(VARCHAR(10), CreatedOn, 120) >= @dateFrom
              AND CONVERT(VARCHAR(10), CreatedOn, 120) <= @dateTo
              AND Sheet = @p_sheet
        --ORDER BY Date_Production, Shift_Work
        --req sprint4
        ORDER BY [Sample_ID];
    END;
GO;
