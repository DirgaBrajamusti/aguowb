IF OBJECT_ID('Report_Barge_Trip') IS NULL
    EXEC('CREATE PROCEDURE Report_Barge_Trip AS SET NOCOUNT ON;')
GO

ALTER PROCEDURE [dbo].[Report_Barge_Trip] @p_site   VARCHAR(32), 
                                          @p_period VARCHAR(10)
AS
    BEGIN
        --
        SELECT [tugboat_barge], 
               SUM(CASE
                       WHEN MONTH(Arrival_Date) = MONTH(@p_period)
                       THEN Loading_Count
                       ELSE 0
                   END) AS period_count, 
               SUM(CASE
                       WHEN MONTH(Arrival_Date) = MONTH(@p_period)
                       THEN [Cargo_Loaded_Belt]
                       ELSE 0
                   END) AS [Cargo_Loaded_Belt], 
               SUM(CASE
                       WHEN MONTH(Arrival_Date) = MONTH(@p_period)
                       THEN [Cargo_Loaded_Draft]
                       ELSE 0
                   END) AS [Cargo_Loaded_Draft], 
               SUM(Cargo_Loaded_Belt) AS Total_Cargo_Loaded_Belt, 
               SUM(Cargo_Loaded_Draft) AS Total_Cargo_Loaded_Draft, 
               SUM(Loading_Count) AS Total_count
        FROM
        (
            SELECT concat([TugName], ' / ', [barge_name]) [tugboat_barge], 
                   CAST(Arrival_Time AS DATE) AS Arrival_Date, 
                   SUM(isnull(b.[Belt],0)) AS [Cargo_Loaded_Belt], 
                   SUM(isnull(b.[Draft],0)) AS [Cargo_Loaded_Draft], 
                   COUNT(1) AS loading_count
            FROM [dbo].[Loading_Actual] t
                     left JOIN [dbo].[Loading_Actual_Cargo_Loaded] b ON t.[RecordId] = b.ActualId
            WHERE siteid = @p_site
                  AND YEAR(Arrival_Time) = YEAR(@p_period)
            GROUP BY concat([TugName], ' / ', [barge_name]), 
                     CAST(Arrival_Time AS DATE)
        ) t
        GROUP BY [tugboat_barge]
        ORDER BY [tugboat_barge];
    END;