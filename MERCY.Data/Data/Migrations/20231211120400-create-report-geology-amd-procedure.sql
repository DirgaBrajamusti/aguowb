/* Up script. */
CREATE PROCEDURE [dbo].[Report_GEOLOGY_AMD] @p_company varchar(32), @p_date varchar(10)
AS 
BEGIN
	SELECT 
		[RecordId],
		[SampleId],
		[LaboratoryId],
		[Sample_Id],
		[SampleType],
		[MassSampleReceived],
		[TS],
		[ANC],
		[MPA],
		[NAPP],
		[NAG],
		[NAGPH45],
		[NAGPH70],
		[NAGType],
		[DateReceived]
	FROM [UPLOAD_Geology_AMD]
	WHERE CompanyCode = @p_company
		AND CONVERT(varchar(10), CreatedOn ,120) = @p_date
		ORDER BY SampleId
END

/*Down script. */
DROP PROCEDURE [dbo].[Report_GEOLOGY_AMD];
