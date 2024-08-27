create procedure [dbo].[UPLOAD_ACTUAL_BARGE_LOADING_Header] @p_file integer
as
begin
	BEGIN TRY  
		insert into UPLOAD_BARGE_LOADING_Header
		(
            File_Physical
            , [TEMPORARY]
            , Company
            , Sheet
            , CreatedBy
            , CreatedOn
            , Date_Detail
            , Job_No
            , Report_To
            , Method1
            , Method2
            , Method3
            , Method4
		)
		select
            File_Physical
            , RecordId
            , Company
            , Sheet
            , CreatedBy
            , CreatedOn
            , Date_Detail
            , Job_No
            , Report_To
            , Method1
            , Method2
            , Method3
            , Method4
		from TEMPORARY_BARGE_LOADING_Header
		where 
			File_Physical = @p_file
	END TRY  
	BEGIN CATCH
	END CATCH  
end