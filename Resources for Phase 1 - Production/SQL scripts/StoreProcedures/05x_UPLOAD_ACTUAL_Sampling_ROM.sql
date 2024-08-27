create procedure [dbo].[UPLOAD_ACTUAL_Sampling_ROM] @p_file integer
as
begin
	declare @vCount integer = 0
	declare @vCount2 integer = 0
	declare @vStatus varchar(32) = ''
	declare @vMessage varchar(8000) = ''
    
    declare @header_id bigint
    
	declare @vNow datetime = GetDate()
    
    exec UPLOAD_ACTUAL_Sampling_ROM_Header @p_file
    
    select top 1 @header_id = RecordId
    from UPLOAD_Sampling_ROM_Header
    order by RecordId desc
    
	BEGIN TRY  
		-- for Status: New
		insert into UPLOAD_Sampling_ROM 
		(
		  [TEMPORARY]
          ,[Header]
		  ,[CreatedBy]
		  ,[CreatedOn]
		  ,[Date_Request]
          ,[Date_Sampling]
		  ,[Day_work]
		  ,[LOT]
		  ,[Lab_ID]
		  ,[TM]
		  ,[M]
		  ,[ASH]
		  ,[TS]
		  ,[CV]
		  ,[Remark]
		  ,[Seam]
		)
		select
		  t.[RecordId]
          ,@header_id
		  ,t.[CreatedBy]
		  ,@vNow
          ,t.[Date_Request]
		  ,t.[Date_Sampling]
		  ,t.[Day_work]
		  ,t.[LOT]
		  ,t.[Lab_ID]
		  ,CONVERT(decimal(10,2), t.[TM])
          ,CONVERT(decimal(10,2), t.[M])
          ,CONVERT(decimal(10,2), t.[ASH])
          ,CONVERT(decimal(10,2), t.[TS])
          ,CONVERT(int, t.[CV])
		  ,t.[Remark]
		  ,t.[Seam]
		from TEMPORARY_Sampling_ROM t inner join TEMPORARY_Sampling_ROM_Header h on t.Header = h.RecordId
		where 
			h.File_Physical = @p_file
			and t.[Status] = 'New'
			and t.[Lab_ID] not in (select [Lab_ID] from UPLOAD_Sampling_ROM)

		-- for Status: Edit
		-- use "SQL Server UPDATE JOIN", https://www.sqlservertutorial.net/sql-server-basics/sql-server-update-join/

		UPDATE 
			UPLOAD_Sampling_ROM
		SET 
			UPLOAD_Sampling_ROM.[TEMPORARY] = t2.[RecordId]
            ,UPLOAD_Sampling_ROM.[Header] = @header_id
			-- * Not Created
			--,UPLOAD_Sampling_ROM.[CreatedBy] = t2.[CreatedBy]
			--,UPLOAD_Sampling_ROM.[CreatedOn] = t2.[CreatedOn]
			-- * but Modified
			,UPLOAD_Sampling_ROM.[LastModifiedBy] = t2.[CreatedBy]
			,UPLOAD_Sampling_ROM.[LastModifiedOn] = @vNow
            ,UPLOAD_Sampling_ROM.[Date_Request] = t2.[Date_Request]
			,UPLOAD_Sampling_ROM.[Date_Sampling] = t2.[Date_Sampling]
			,UPLOAD_Sampling_ROM.[Day_work] = t2.[Day_work]
			,UPLOAD_Sampling_ROM.[LOT] = t2.[LOT]
			,UPLOAD_Sampling_ROM.[Lab_ID] = t2.[Lab_ID]
			,UPLOAD_Sampling_ROM.[TM] = CONVERT(decimal(10,2), t2.[TM])
			,UPLOAD_Sampling_ROM.[M] = CONVERT(decimal(10,2), t2.[M])
			,UPLOAD_Sampling_ROM.[ASH] = CONVERT(decimal(10,2), t2.[ASH])
			,UPLOAD_Sampling_ROM.[TS] = CONVERT(decimal(10,2), t2.[TS])
			,UPLOAD_Sampling_ROM.[CV] = CONVERT(int, t2.[CV])
			,UPLOAD_Sampling_ROM.[Remark] = t2.[Remark]
			,UPLOAD_Sampling_ROM.[Seam] = t2.[Seam]
		FROM 
			UPLOAD_Sampling_ROM t1
				INNER JOIN TEMPORARY_Sampling_ROM t2 ON t1.Lab_ID = t2.Lab_ID
				INNER JOIN TEMPORARY_Sampling_ROM_Header h on t2.Header = h.RecordId
		WHERE
			h.File_Physical = @p_file
			and t2.[Status] = 'Update'

		select @vCount = Count(RecordId)
		from UPLOAD_Sampling_ROM
		where (CreatedOn = @vNow) or (LastModifiedOn = @vNow)

		select @vCount2 = Count(t2.RecordId)
		FROM  TEMPORARY_Sampling_ROM t2
			INNER JOIN TEMPORARY_Sampling_ROM_Header h on t2.Header = h.RecordId
		WHERE h.File_Physical = @p_file

		select @vStatus = 'Ok'
		select @vMessage = 'UPLOAD_ACTUAL_Sampling_ROM'
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
