IF OBJECT_ID('Report_Barging_Loading_Performance_Summary') IS NULL
    EXEC('CREATE PROCEDURE Report_Barging_Loading_Performance_Summary AS SET NOCOUNT ON;')
GO

ALTER PROCEDURE [dbo].[Report_Barging_Loading_Performance_Summary] @p_site   VARCHAR(32), 
                                          @p_period DATE
AS
    BEGIN
				--
		select * from (
		SELECT 'Plan Loading' as item_type, 
				'mt' as item_uom,
				1 as seq_no,
			   MONTH(@p_period) AS period_month, 
			   1 AS mm_01, 2 AS mm_02, 3 AS mm_03, 4 AS mm_04, 5 AS mm_05, 6 AS mm_06, 7 AS mm_07, 8 AS mm_08, 9 AS mm_09, 10 AS mm_10, 11 AS mm_11, 12 AS mm_12 
			   union
		SELECT 'Outlook Loading' as item_type, 
				'mt' as item_uom,
				2 as seq_no,
			   MONTH(@p_period) AS period_month, 
			   1 AS mm_01, 2 AS mm_02, 3 AS mm_03, 4 AS mm_04, 5 AS mm_05, 6 AS mm_06, 7 AS mm_07, 8 AS mm_08, 9 AS mm_09, 10 AS mm_10, 11 AS mm_11, 12 AS mm_12 
			   union
		SELECT 'HCV-MS SMI' as item_type, 
				'mt' as item_uom,
				3 as seq_no,
			   MONTH(@p_period) AS period_month, 
			   1 AS mm_01, 2 AS mm_02, 3 AS mm_03, 4 AS mm_04, 5 AS mm_05, 6 AS mm_06, 7 AS mm_07, 8 AS mm_08, 9 AS mm_09, 10 AS mm_10, 11 AS mm_11, 12 AS mm_12 
			   union
		SELECT 'HCV-HS' as item_type, 
				'mt' as item_uom,
				4 as seq_no,
			   MONTH(@p_period) AS period_month, 
			   1 AS mm_01, 2 AS mm_02, 3 AS mm_03, 4 AS mm_04, 5 AS mm_05, 6 AS mm_06, 7 AS mm_07, 8 AS mm_08, 9 AS mm_09, 10 AS mm_10, 11 AS mm_11, 12 AS mm_12 
			   union
		SELECT 'HCV-LS' as item_type, 
				'mt' as item_uom,
				5 as seq_no,
			   MONTH(@p_period) AS period_month, 
			   1 AS mm_01, 2 AS mm_02, 3 AS mm_03, 4 AS mm_04, 5 AS mm_05, 6 AS mm_06, 7 AS mm_07, 8 AS mm_08, 9 AS mm_09, 10 AS mm_10, 11 AS mm_11, 12 AS mm_12 
			   union
		SELECT 'Belt Scale' as item_type, 
				'mt' as item_uom,
				6 as seq_no,
			   MONTH(@p_period) AS period_month, 
			   sum(case when month(Initial_Draft)=1 then Belt else 0 end)  AS mm_01, 
			   sum(case when month(Initial_Draft)=2 then Belt else 0 end)  AS mm_02, 
			   sum(case when month(Initial_Draft)=3 then Belt else 0 end)  AS mm_03, 
			   sum(case when month(Initial_Draft)=4 then Belt else 0 end)  AS mm_04, 
			   sum(case when month(Initial_Draft)=5 then Belt else 0 end)  AS mm_05, 
			   sum(case when month(Initial_Draft)=6 then Belt else 0 end)  AS mm_06, 
			   sum(case when month(Initial_Draft)=7 then Belt else 0 end)  AS mm_07, 
			   sum(case when month(Initial_Draft)=8 then Belt else 0 end)  AS mm_08, 
			   sum(case when month(Initial_Draft)=9 then Belt else 0 end)  AS mm_09, 
			   sum(case when month(Initial_Draft)=10 then Belt else 0 end)  AS mm_10, 
			   sum(case when month(Initial_Draft)=11 then Belt else 0 end)  AS mm_11, 
			   sum(case when month(Initial_Draft)=12 then Belt else 0 end)  AS mm_12
				  FROM [dbo].[Loading_Actual] b
				  join [dbo].[Loading_Actual_Cargo_Loaded] a on b.RecordId=a.ActualId
				  join [dbo].tunnel t on a.TunnelId=t.TunnelId
				WHERE siteid = @p_site
				  AND year(Initial_Draft) = year(@p_period)
			  group by ActualId	
			   union
--
		SELECT 'Draft Survey' as item_type, 
				'mt' as item_uom,
				7 as seq_no,
			   MONTH(@p_period) AS period_month, 
			   sum(case when month(Initial_Draft)=1 then Draft else 0 end)  AS mm_01, 
			   sum(case when month(Initial_Draft)=2 then Draft else 0 end)  AS mm_02, 
			   sum(case when month(Initial_Draft)=3 then Draft else 0 end)  AS mm_03, 
			   sum(case when month(Initial_Draft)=4 then Draft else 0 end)  AS mm_04, 
			   sum(case when month(Initial_Draft)=5 then Draft else 0 end)  AS mm_05, 
			   sum(case when month(Initial_Draft)=6 then Draft else 0 end)  AS mm_06, 
			   sum(case when month(Initial_Draft)=7 then Draft else 0 end)  AS mm_07, 
			   sum(case when month(Initial_Draft)=8 then Draft else 0 end)  AS mm_08, 
			   sum(case when month(Initial_Draft)=9 then Draft else 0 end)  AS mm_09, 
			   sum(case when month(Initial_Draft)=10 then Draft else 0 end)  AS mm_10, 
			   sum(case when month(Initial_Draft)=11 then Draft else 0 end)  AS mm_11, 
			   sum(case when month(Initial_Draft)=12 then Draft else 0 end)  AS mm_12
				  FROM [dbo].[Loading_Actual] b
				  join [dbo].[Loading_Actual_Cargo_Loaded] a on b.RecordId=a.ActualId
				  join [dbo].tunnel t on a.TunnelId=t.TunnelId
				WHERE siteid = @p_site
				  AND year(Initial_Draft) = year(@p_period)
			  group by ActualId	
--			  			   
			   union
		SELECT 'HCV-MS SMI' as item_type, 
				'mt' as item_uom,
				8 as seq_no,
			   MONTH(@p_period) AS period_month, 
			   1 AS mm_01, 2 AS mm_02, 3 AS mm_03, 4 AS mm_04, 5 AS mm_05, 6 AS mm_06, 7 AS mm_07, 8 AS mm_08, 9 AS mm_09, 10 AS mm_10, 11 AS mm_11, 12 AS mm_12 
			   union
		SELECT 'HCV-HS' as item_type, 
				'mt' as item_uom,
				9 as seq_no,
			   MONTH(@p_period) AS period_month, 
			   1 AS mm_01, 2 AS mm_02, 3 AS mm_03, 4 AS mm_04, 5 AS mm_05, 6 AS mm_06, 7 AS mm_07, 8 AS mm_08, 9 AS mm_09, 10 AS mm_10, 11 AS mm_11, 12 AS mm_12 
			   union
		SELECT 'HCV-LS' as item_type, 
				'mt' as item_uom,
				10 as seq_no,
			   MONTH(@p_period) AS period_month, 
			   1 AS mm_01, 2 AS mm_02, 3 AS mm_03, 4 AS mm_04, 5 AS mm_05, 6 AS mm_06, 7 AS mm_07, 8 AS mm_08, 9 AS mm_09, 10 AS mm_10, 11 AS mm_11, 12 AS mm_12 
			   union
		SELECT 'Variance' as item_type, 
				'mt' as item_uom,
				11 as seq_no,
			   MONTH(@p_period) AS period_month, 
			   1 AS mm_01, 2 AS mm_02, 3 AS mm_03, 4 AS mm_04, 5 AS mm_05, 6 AS mm_06, 7 AS mm_07, 8 AS mm_08, 9 AS mm_09, 10 AS mm_10, 11 AS mm_11, 12 AS mm_12 
			   union
		SELECT '% Achieve' as item_type, 
				'%' as item_uom,
				12 as seq_no,
			   MONTH(@p_period) AS period_month, 
			   1 AS mm_01, 2 AS mm_02, 3 AS mm_03, 4 AS mm_04, 5 AS mm_05, 6 AS mm_06, 7 AS mm_07, 8 AS mm_08, 9 AS mm_09, 10 AS mm_10, 11 AS mm_11, 12 AS mm_12 
			   ) t order by seq_no
    END;