/* Up Script*/

ALTER procedure [dbo].[Report_Form_Analysis_Pit_Header](@p_analysisRequest int)
as
begin
    SELECT top 1
        Date_Detail as DATE_1
        , '' as DATE_2
        , JOB_NO
        , Report_To
        , Date_Received as Date_Receive_sample
        , Nomor as HEADER_NO
    from UPLOAD_Geology_Pit_Monitoring_Header
    where CONCAT(Company, CreatedOn_Date_Only) IN
    (
        SELECT CONCAT(Company, CreatedOn_Date_Only)

        from UPLOAD_Geology_Pit_Monitoring
        where Lab_ID in
        (
            select LabId from AnalysisRequest_Detail
            where AnalysisRequest = @p_analysisRequest
        )
    )
    order by RecordId desc
end;

alter procedure [dbo].[Report_Form_Analysis_Exploration_Header](@p_analysisRequest int)
as
begin
    SELECT top 1
        Date_Detail as DATE_1
        , '' as DATE_2
        , JOB_NO
        , Report_To
        , Date_Received as Date_Receive_sample
        , Nomor as HEADER_NO
    from UPLOAD_Geology_Explorasi_Header
   where CONCAT(Company, CreatedOn_Date_Only) IN
    (
        SELECT CONCAT(Company, CreatedOn_Date_Only)
        from UPLOAD_Geology_Explorasi
        where Lab_ID in
        (
            select LabId from AnalysisRequest_Detail
            where AnalysisRequest = @p_analysisRequest
        )
    )
    order by RecordId desc
end;

/* Down Script*/

ALTER procedure [dbo].[Report_Form_Analysis_Pit_Header](@p_analysisRequest int)
as
begin
    SELECT top 1
        Date_Detail as DATE_1
        , '' as DATE_2
        , JOB_NO
        , Report_To
        , Date_Received as Date_Receive_sample
        , Nomor as HEADER_NO
    from UPLOAD_Geology_Pit_Monitoring_Header
    where RecordId IN
    (
        SELECT Header
        from UPLOAD_Geology_Pit_Monitoring
        where Lab_ID in
        (
            select LabId from AnalysisRequest_Detail
            where AnalysisRequest = @p_analysisRequest
        )
    )
    order by RecordId desc
end;

alter procedure [dbo].[Report_Form_Analysis_Exploration_Header](@p_analysisRequest int)
as
begin
    SELECT top 1
        Date_Detail as DATE_1
        , '' as DATE_2
        , JOB_NO
        , Report_To
        , Date_Received as Date_Receive_sample
        , Nomor as HEADER_NO
    from UPLOAD_Geology_Explorasi_Header
   where RecordId IN
    (
        SELECT Header
        from UPLOAD_Geology_Explorasi
        where Lab_ID in
        (
            select LabId from AnalysisRequest_Detail
            where AnalysisRequest = @p_analysisRequest
        )
    )
    order by RecordId desc
end;