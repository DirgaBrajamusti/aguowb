IF OBJECT_ID('Report_Barging_Loading_Performance') IS NULL
    EXEC('CREATE PROCEDURE Report_Barging_Loading_Performance AS SET NOCOUNT ON;')
GO

ALTER PROCEDURE [dbo].[Report_Barging_Loading_Performance] @p_site   VARCHAR(32), 
                                                           @p_period VARCHAR(10)
AS
    BEGIN
        SELECT ROW_NUMBER() OVER(
               ORDER BY [No_Ref_Report]) [seq_no], 
               [No_Ref_Report], 
               [No_Services_Trip], 
               [Route], 
               [load_type], 
               [TugName] [tugboat_name], 
               [barge_name], 
               [barge_size], 
               0 [coal_load_plan], 
               format(t.Arrival_Time,'yyyy-MM-dd HH:mm:ss') Arrival_Time
        FROM [dbo].[Loading_Actual] t
        WHERE siteid = @p_site
              AND year(Arrival_Time) = year(@p_period)
			  and month(Arrival_Time) = month(@p_period);
    END;