/* Up script. */
CREATE PROCEDURE [dbo].[Report_GEOLOGY_AMD_Header] @p_company varchar(32), @p_date varchar(10)
AS 
BEGIN
	SELECT 
		[RecordId],
		[CompanyCode],
		[Date_Detail],
		[Job_No],
		[Report_To],
		[Date_Received],
		[Nomor]
	FROM [UPLOAD_Geology_AMD_Header]
	WHERE CompanyCode = @p_company
		AND CONVERT(varchar(10), CreatedOn ,120) = @p_date
		ORDER BY RecordId desc
END

/*Down script. */
DROP PROCEDURE [dbo].[Report_GEOLOGY_AMD_Header];
