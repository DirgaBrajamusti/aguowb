create procedure [dbo].[UPLOAD_ACTUAL_BARGE_LOADING] @p_file integer
as
begin
	declare @vCount integer = 0
	declare @vCount2 integer = 0
	declare @vStatus varchar(32) = ''
	declare @vMessage varchar(8000) = ''
    declare @header_id bigint
    
	declare @vNow datetime = GetDate()
    
    exec UPLOAD_ACTUAL_BARGE_LOADING_Header @p_file
    
    select top 1 @header_id = RecordId
    from UPLOAD_BARGE_LOADING_Header
    order by RecordId desc
    
	BEGIN TRY  
		-- for Status: New
		insert into UPLOAD_BARGE_LOADING 
		(
            [TEMPORARY]
            ,[Header]
            ,[CreatedBy]
            ,[CreatedOn]
			,[Sheet]
            ,[JOB_Number]
            ,[ID_Number]
            ,[Service_Trip_Number]
            ,[Date_Sampling]
            ,[Date_Report]
            ,[Tonnage]
            ,[Name]
            ,[Destination]
            ,[Temperature]
            ,[TM]
            ,[M]
            ,[ASH_adb]
            ,[ASH_arb]
            ,[VM_adb]
            ,[VM_arb]
            ,[FC_adb]
            ,[FC_arb]
            ,[TS_adb]
            ,[TS_arb]
            , CV_adb
            , CV_db
            , CV_arb
            , CV_daf
            , CV_ad_15
            , CV_ad_16
            , CV_ad_17
            ,[Remark]
		)
		select
            t.[RecordId]
            ,@header_id
            ,t.[CreatedBy]
            ,@vNow
			,h.Sheet
            ,t.[JOB_Number]
            ,t.[ID_Number]
            ,t.[Service_Trip_Number]
            ,t.[Date_Sampling]
            ,t.[Date_Report]
            ,CONVERT(decimal(10,3), t.[Tonnage])
            ,t.[Name]
            ,t.[Destination]
            ,CONVERT(decimal(10,2), t.[Temperature])
            ,CONVERT(decimal(10,2), t.[TM])
            ,CONVERT(decimal(10,2), t.[M])
            ,CONVERT(decimal(10,2), t.[ASH_adb])
            ,t.[ASH_arb]
            ,CONVERT(decimal(10,2), t.[VM_adb])
            ,t.[VM_arb]
            ,t.[FC_adb]
            ,t.[FC_arb]
            ,CONVERT(decimal(10,2), t.[TS_adb])
            ,t.[TS_arb]
            ,CONVERT(decimal(10,2), t.[CV_adb])
            , t.CV_db
            , t.CV_arb
            , t.CV_daf
            , t.CV_ad_15
            , t.CV_ad_16
            , t.CV_ad_17
            ,t.[Remark]
		from TEMPORARY_BARGE_LOADING t inner join TEMPORARY_BARGE_LOADING_Header h on t.Header = h.RecordId
		where 
			h.File_Physical = @p_file
			and t.[Status] = 'New'
			and t.[ID_Number] not in (select [ID_Number] from UPLOAD_BARGE_LOADING)

		-- for Status: Edit
		-- use "SQL Server UPDATE JOIN", https://www.sqlservertutorial.net/sql-server-basics/sql-server-update-join/

		UPDATE 
			UPLOAD_BARGE_LOADING
		SET 
			UPLOAD_BARGE_LOADING.[TEMPORARY] = t2.[RecordId]
            ,UPLOAD_BARGE_LOADING.[Header] = @header_id
			-- * Not Created
			--,UPLOAD_BARGE_LOADING.[CreatedBy] = t2.[CreatedBy]
			--,UPLOAD_BARGE_LOADING.[CreatedOn] = t2.[CreatedOn]
			-- * but Modified
			,UPLOAD_BARGE_LOADING.[LastModifiedBy] = t2.[CreatedBy]
			,UPLOAD_BARGE_LOADING.[LastModifiedOn] = @vNow
			,UPLOAD_BARGE_LOADING.[Sheet] = h.Sheet
            ,UPLOAD_BARGE_LOADING.[JOB_Number] = t2.[JOB_Number]
            ,UPLOAD_BARGE_LOADING.[ID_Number] = t2.[ID_Number]
            ,UPLOAD_BARGE_LOADING.[Service_Trip_Number] = t2.[Service_Trip_Number]
            ,UPLOAD_BARGE_LOADING.[Date_Sampling] = t2.[Date_Sampling]
            ,UPLOAD_BARGE_LOADING.[Date_Report] = t2.[Date_Report]
            ,UPLOAD_BARGE_LOADING.[Tonnage] = CONVERT(decimal(10,3), t2.[Tonnage])
            ,UPLOAD_BARGE_LOADING.[Name] = t2.[Name]
            ,UPLOAD_BARGE_LOADING.[Destination] = t2.[Destination]
            ,UPLOAD_BARGE_LOADING.[Temperature] = CONVERT(decimal(10,2), t2.[Temperature])
            ,UPLOAD_BARGE_LOADING.[TM] = CONVERT(decimal(10,2), t2.[TM])
            ,UPLOAD_BARGE_LOADING.[M] = CONVERT(decimal(10,2), t2.[M])
            ,UPLOAD_BARGE_LOADING.[ASH_adb] = CONVERT(decimal(10,2), t2.[ASH_adb])
            ,UPLOAD_BARGE_LOADING.[ASH_arb] = t2.[ASH_arb]
            ,UPLOAD_BARGE_LOADING.[VM_adb] = CONVERT(decimal(10,2), t2.[VM_adb])
            ,UPLOAD_BARGE_LOADING.[VM_arb] = t2.[VM_arb]
            ,UPLOAD_BARGE_LOADING.[FC_adb] = t2.[FC_adb]
            ,UPLOAD_BARGE_LOADING.[FC_arb] = t2.[FC_arb]
            ,UPLOAD_BARGE_LOADING.[TS_adb] = CONVERT(decimal(10,2), t2.[TS_adb])
            ,UPLOAD_BARGE_LOADING.[TS_arb] = t2.[TS_arb]
            , UPLOAD_BARGE_LOADING.CV_adb = CONVERT(decimal(10,2), t2.[CV_adb])
            , UPLOAD_BARGE_LOADING.CV_db = t2.CV_db
            , UPLOAD_BARGE_LOADING.CV_arb = t2.CV_arb
            , UPLOAD_BARGE_LOADING.CV_daf = t2.CV_daf
            , UPLOAD_BARGE_LOADING.CV_ad_15 = t2.CV_ad_15
            , UPLOAD_BARGE_LOADING.CV_ad_16 = t2.CV_ad_16
            , UPLOAD_BARGE_LOADING.CV_ad_17 = t2.CV_ad_17
            ,UPLOAD_BARGE_LOADING.[Remark] = t2.[Remark]
		FROM 
			UPLOAD_BARGE_LOADING t1
				INNER JOIN TEMPORARY_BARGE_LOADING t2 ON t1.ID_Number = t2.ID_Number
				INNER JOIN TEMPORARY_BARGE_LOADING_Header h on t2.Header = h.RecordId
		WHERE
			h.File_Physical = @p_file
			and t2.[Status] = 'Update'

		select @vCount = Count(RecordId)
		from UPLOAD_BARGE_LOADING
		where (CreatedOn = @vNow) or (LastModifiedOn = @vNow)

		select @vCount2 = Count(t2.RecordId)
		FROM  TEMPORARY_BARGE_LOADING t2
			INNER JOIN TEMPORARY_BARGE_LOADING_Header h on t2.Header = h.RecordId
		WHERE h.File_Physical = @p_file

		select @vStatus = 'Ok'
		select @vMessage = 'UPLOAD_ACTUAL_BARGE_LOADING'
	END TRY  
	BEGIN CATCH  
		 select @vCount = 0
		 select @vCount2 = 0
		 select @vStatus = 'Error'
		 select @vMessage = ERROR_MESSAGE()
	END CATCH  

	-- tampilkan "Hasil Eksekusi"
	select @vCount as cCount, @vCount2 as cCount2, @vStatus as cStatus, @vMessage as cMessage
end