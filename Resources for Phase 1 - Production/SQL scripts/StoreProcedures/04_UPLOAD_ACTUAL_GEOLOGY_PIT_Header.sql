create procedure [dbo].[UPLOAD_ACTUAL_GEOLOGY_PIT_Header] @p_file integer
as
begin
	BEGIN TRY  
		insert into UPLOAD_Geology_Pit_Monitoring_Header 
		(
            File_Physical
            , [TEMPORARY]
            , Company 
            , CreatedBy 
            , CreatedOn 
            , Date_Detail 
            , Job_No 
            , Report_To 
            , Date_Received
            , Nomor 
		)
		select
            File_Physical
            , RecordId
            , Company 
            , CreatedBy 
            , CreatedOn 
            , Date_Detail 
            , Job_No 
            , Report_To 
            , Date_Received
            , Nomor 
		from TEMPORARY_Geology_Pit_Monitoring_Header
		where 
			File_Physical = @p_file
	END TRY  
	BEGIN CATCH
	END CATCH
end