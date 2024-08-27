/* Up Script */

/* Use date instead of header column as relation to Sampling Header table */
/* since header column is not valid                                       */

ALTER procedure [dbo].[Report_Form_Sampling_Header](@p_samplingRequest int) 
AS
BEGIN
    WITH Sampling_ROM_Header AS 
    (
	    SELECT TOP 1
            Date_Detail AS DATE_1
            , '' AS DATE_2
            , JOB_NO
            , Report_To
            , Method1
            , Method2
            , Method3
            , Method4
        FROM UPLOAD_Sampling_ROM_Header
        WHERE CONCAT(Company, CreatedOn_Date_Only) IN
        (
            SELECT CONCAT(Company, CreatedOn_Date_Only)
            FROM UPLOAD_Sampling_ROM
            WHERE Lab_ID in
            (
                SELECT LabId FROM SamplingRequest_Lab
                WHERE SamplingRequest = @p_samplingRequest
            )
        )
        ORDER BY RecordId DESC
    ),
    Sampling_HAC_Header AS (
	    SELECT TOP 1
            Date_Detail AS DATE_1
            , '' AS DATE_2
            , JOB_NO
            , Report_To
            , Method1
            , Method2
            , Method3
            , Method4
        FROM UPLOAD_HAC_Header
        WHERE CONCAT(Company, CreatedOn_Date_Only) IN
        (
            SELECT CONCAT(Company, CreatedOn_Date_Only)
            FROM UPLOAD_HAC
            WHERE Lab_ID in
            (
                SELECT LabId FROM SamplingRequest_Lab
                WHERE SamplingRequest = @p_samplingRequest
            )
        )
        ORDER BY RecordId DESC
    )

    SELECT *
    FROM (
	    SELECT * FROM Sampling_ROM_Header
	    UNION
	    SELECT * FROM Sampling_HAC_Header
    ) AS merged_sampling_header
END;

/* Include UPLOAD_HAC into report sampling procedure */
ALTER procedure [dbo].[Report_Form_Sampling](@p_samplingRequest int)
AS
BEGIN
    WITH Sampling_ROM AS
    (
	    SELECT
            RecordId
            ,[TEMPORARY]
            ,CreatedBy
            ,CreatedOn
            ,Date_Request
            ,Date_Sampling
            ,Day_work
            ,LOT
            ,Lab_ID
            ,TM
            ,M
            ,ASH
            ,TS
            ,CV
            ,Remark
            ,Seam
        FROM UPLOAD_Sampling_ROM
        WHERE Lab_ID IN
        (
            SELECT LabId FROM SamplingRequest_Lab
            WHERE SamplingRequest = @p_samplingRequest
        )
    ),

    Sampling_HAC AS
    (
	    SELECT
            RecordId
            ,[TEMPORARY]
            ,CreatedBy
            ,CreatedOn
            ,NULL AS Date_Request
            ,Date_Sampling
            ,Day_work
            ,LOT
            ,Lab_ID
            ,TM
            ,M
            ,ASH
            ,TS
            ,CV
            ,Remark
            ,NULL AS Seam
        FROM UPLOAD_HAC
        WHERE Lab_ID IN
        (
            SELECT LabId FROM SamplingRequest_Lab
            WHERE SamplingRequest = @p_samplingRequest
        )
    )
 
    SELECT * 
    FROM (
     SELECT *
     FROM Sampling_ROM
     UNION
     SELECT * 
     FROM Sampling_HAC
    ) AS merged_sampling
    ORDER BY RecordId
END;


/* Down Script */
ALTER procedure [dbo].[Report_Form_Sampling_Header](@p_samplingRequest int)
AS
BEGIN
    SELECT top 1
        Date_Detail as DATE_1
        , '' as DATE_2
        , JOB_NO
        , Report_To
        , Method1
        , Method2
        , Method3
        , Method4
    from UPLOAD_Sampling_ROM_Header
    where RecordId in
    (
        select Header
        from UPLOAD_Sampling_ROM
        where Lab_ID in
        (
            select LabId from SamplingRequest_Lab
            where SamplingRequest = @p_samplingRequest
        )
    )
    order by RecordId desc
END;

ALTER procedure [dbo].[Report_Form_Sampling](@p_samplingRequest int)
AS
BEGIN
    select
        RecordId
        ,[TEMPORARY]
        ,CreatedBy
        ,CreatedOn
        ,Date_Request
        ,Date_Sampling
        ,Day_work
        ,LOT
        ,Lab_ID
        ,TM
        ,M
        ,ASH
        ,TS
        ,CV
        ,Remark
        ,Seam
    from UPLOAD_Sampling_ROM
    where Lab_ID in
    (
        select LabId from SamplingRequest_Lab
        where SamplingRequest = @p_samplingRequest
    )
    order by RecordId
end;
