create procedure [dbo].[UPLOAD_ACTUAL_GEOLOGY_EXPLORASI_Header] @p_file integer
as
begin
	declare @vNow datetime = GetDate()

	BEGIN TRY  
		-- for Status: New
		insert into UPLOAD_Geology_Explorasi_Header 
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
		from TEMPORARY_Geology_Explorasi_Header
		where 
			File_Physical = @p_file
	END TRY  
	BEGIN CATCH  
	END CATCH
end
