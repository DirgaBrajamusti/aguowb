IF OBJECT_ID('Report_Barging_Loading_Performance') IS NULL
    EXEC('CREATE PROCEDURE Report_Barging_Loading_Performance AS SET NOCOUNT ON;')
GO

ALTER PROCEDURE [dbo].[Report_Barging_Loading_Performance] @p_site   VARCHAR(32), 
                                                           @p_period date
AS
    BEGIN
        --
	select z.* 
		  , case when [bp_gross_mt_hour]=0 then 0 else  [coal_loaded_draft_survey] / [bp_gross_mt_hour] end as bp_net_mt_hour
	from (
    select y.* 
		  , case when [bp_gross_operation_hours]=0 then 0 else  [coal_loaded_draft_survey] / [bp_gross_operation_hours] end as [bp_gross_mt_hour]
	from
	(
    select x.* 
		  , ([delay_loading_preparation] + [delay_waiting_cargo] +[delay_breakdown] + [delay_total_in_loading] + [delay_waiting_document]) as [delay_total]
		  , ([opr_actual_load] + [delay_total_in_loading]) - [delay_change_shift]  as [bp_gross_operation_hours]
	
	from (
    select t.*
		  ,([delay_waiting_loading] + [delay_waiting_cargo_analysis] + [delay_breakdown_conveyor] + [delay_change_shift] + [delay_pm_scm] + [delay_other]) as [delay_total_in_loading] 
		  , [opr_actual_load] as [bp_net_operation_hours]
		  , case when [opr_actual_load]=0 then 0 else  ([coal_loaded_draft_survey]/ [opr_actual_load]) end as [opr_avg_loading_capacity]
		  , case when coal_loaded_belt_scale=0 then 0 else ([coal_loaded_draft_survey] - [coal_loaded_belt_scale]) / coal_loaded_belt_scale end  [percent]
		  --, [coal_loaded_draft_survey]
	 from (
	SELECT ROW_NUMBER() OVER(
				   ORDER BY [No_Ref_Report]) [seq_no]
		  ,[No_Ref_Report] as [ref_no] 
		  ,[No_Services_Trip]
		  ,[Route] as [route_destination] 
		  ,[load_type]
		  ,[TugName] [tugboat_name]
		  ,[barge_name]
		  ,[barge_size]
		  ,'-' [coal_load_plan]
		  ,format(t.Arrival_Time,'yyyy-MM-dd HH:mm:ss') [arrival_ts]
		  ,format(t.Initial_Draft,'yyyy-MM-dd HH:mm:ss') [request_berthing_ts]
		  ,format(t.Berthed_Time,'yyyy-MM-dd HH:mm:ss') [berthed_ts]
		  ,format(t.Commenced_Loading,'yyyy-MM-dd HH:mm:ss')  [comm_loading_ts]
		  ,format(t.Completed_Loading,'yyyy-MM-dd HH:mm:ss')  [comp_loading_ts]
		  ,format(t.Unberthing,'yyyy-MM-dd HH:mm:ss')  [unberthed_ts]
		  ,format(t.Departure,'yyyy-MM-dd HH:mm:ss')  [departure_ts]
		  ,t.Delay_Cause_of_Barge_Changing [delay_cause]
		  ,0 [hcv_mls]
		  ,0 [lcv_ls]
		  ,0 [hcv_ls]
		  ,0 [lcv_hs]
		  ,0 [hcv_hs]
		  ,0 [hcm_ms_smi]
		  ,0 [coal_loaded_time]
		  ,0 [coal_loaded_freight]
		  ,0 [coal_loaded_direct_barge]
		  ,t.Coal_Quality_Spec [final_quality]
		  ,t2.coal_loaded_Draft [coal_loaded_draft_survey]
		  ,t2.coal_loaded_belt [coal_loaded_belt_scale]
		  --,([coal_loaded_draft_survey] - [coal_loaded_belt_scale]) / coal_loaded_belt_scale   [percent]
		  ,t.Surveyor_Name [surveyor_name]
		  ,0 [coal_blending_tn1]
		  ,0 [coal_blending_tn2]
		  ,0 [coal_blending_tn3]
		  ,0 [coal_blending_tn4]
		  ,0 [coal_blending_tn5]
		  ,0 [coal_blending_tn6]
		  ,0 [request_cv]
		  ,0 [request_ts]
		  ,TN1_Belt [actual_belt_scale_tn1]
		  ,TN2_Belt [actual_belt_scale_tn2]
		  ,TN3_Belt [actual_belt_scale_tn3]
		  ,TN4_Belt [actual_belt_scale_tn4]
		  ,TN5_Belt [actual_belt_scale_tn5]
		  ,TN6_Belt [actual_belt_scale_tn6]
		  ,round(cast(datediff(minute, t.Initial_Draft, t.Anchor_Up) as float) / 60,2) [opr_inital_draft]
		  ,round(cast(datediff(minute, Anchor_Up, Berthed_Time) as float) / 60,2) [opr_sailing_to_berth]
		  ,round(cast(datediff(minute, Commenced_Loading, Completed_Loading) as float) / 60,2) [opr_actual_load]
		  ,round(cast(datediff(minute, Completed_Loading, Unberthing) as float) / 60,2) [opr_final_draft]
		  --,'-' [opr_avg_loading_capacity]
		  ,round(cast(datediff(minute, Arrival_Time, Initial_Draft) as float) / 60,2) [delay_waiting_berthing]
		  ,0 [delay_waiting_barge]
		  ,0 [delay_loading_preparation]
		  ,0 [delay_waiting_cargo] --diexcelnya 0 tidak ada formula
		  ,0 [delay_breakdown] --diexcelnya 0  tidak ada formula
		  ,0 [delay_waiting_loading] --diexcelnya 0  tidak ada formula
		  ,0 [delay_waiting_cargo_analysis] --diexcelnya 0  tidak ada formula
		  ,0 [delay_breakdown_conveyor] --diexcelnya 0  tidak ada formula
		  ,0 [delay_change_shift] --tidak ada inputan untuk over shift
		  ,0 [delay_pm_scm] --diexcelnya 0  tidak ada formula
		  ,0 [delay_other] --diexcelnya 0  tidak ada formula
		  --,([delay_waiting_loading] + [delay_waiting_cargo_analysis] + [delay_breakdown_conveyor] + [delay_change_shift] + [delay_pm_scm] + [delay_other]) [delay_total_in_loading] 
		  ,round(cast(datediff(minute, Completed_Loading, Unberthing) as float) / 60,2) [delay_waiting_document]
		  --,([delay_loading_preparation] + [delay_waiting_cargo] +[delay_breakdown] + [delay_total_in_loading] + [delay_waiting_document]) [delay_total]
		  ,round(cast(datediff(minute, t.Arrival_Time, t.Unberthing) as float) / 60,2) [barge_port_stay_hour]
		  ,round(cast(datediff(minute, t.Arrival_Time, t.Unberthing) as float) / 60,2) / 24 [barge_port_stay_days]
		  ,round(cast(datediff(minute, t.Berthed_Time, t.Unberthing) as float) / 60,2) [berth_occupancy_hour]
		  --,'-' [bp_gross_operation_hours]
		  --,'-' [bp_net_operation_hours]
		  --,'-' [bp_gross_mt_hour]
		  --,'-' [bp_net_mt_hour]
		  ,t.Water_Level_During_Loading [water_level_during_load]
		  ,0 [coal_temperatur]
		  ,t.Daily_Water_Level [daily_water_level]
		  ,t.Weather_Condition [weather_condition]
		  ,'-' [date_summary]
		  ,'-' [no_of_pile]


			FROM [dbo].[Loading_Actual] t
			left join (
				SELECT ActualId
					  ,sum(case when t.name='TN1' then [Belt] else 0 end) as TN1_Belt
					  ,sum(case when t.name='TN2' then [Belt] else 0 end) as TN2_Belt
					  ,sum(case when t.name='TN3' then [Belt] else 0 end) as TN3_Belt
					  ,sum(case when t.name='TN4' then [Belt] else 0 end) as TN4_Belt
					  ,sum(case when t.name='TN5' then [Belt] else 0 end) as TN5_Belt
					  ,sum(case when t.name='TN6' then [Belt] else 0 end) as TN6_Belt
					  ,sum(case when t.name='TN1' then Draft else 0 end) as TN1_Draft
					  ,sum([Belt]) as coal_loaded_belt
					  ,sum(Draft) as coal_loaded_Draft
				  FROM [dbo].[Loading_Actual] b
				  join [dbo].[Loading_Actual_Cargo_Loaded] a on b.RecordId=a.ActualId
				  join [dbo].tunnel t on a.TunnelId=t.TunnelId
				WHERE siteid = @p_site
				  AND year(Initial_Draft) = year(@p_period)
				  and month(Initial_Draft) = month(@p_period)
			  group by ActualId			
			) t2 on t.RecordId = t2.ActualId
			WHERE siteid = @p_site
				  AND year(Initial_Draft) = year(@p_period)
				  and month(Initial_Draft) = month(@p_period)
			) t 
			) x
			) y 
			) z order by [seq_no];
    END;