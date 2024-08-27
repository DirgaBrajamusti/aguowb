
CREATE OR ALTER procedure [dbo].[Report_GEOLOGY_PIT] @p_company varchar(32), @p_date varchar(10)
as
begin
    SELECT 
      [Sample_ID]
      ,[SampleType]
      ,[Lab_ID]
      ,[Mass_Spl]
      ,[TM]
      ,[M]
      ,[VM]
      ,[Ash]
      ,[FC]
      ,[TS]
      ,CONVERT(decimal(10,0), [Cal_ad]) as Cal_ad
      ,CONVERT(decimal(10,0), [Cal_db]) as Cal_db
      ,CONVERT(decimal(10,0), [Cal_ar]) as Cal_ar
      ,CONVERT(decimal(10,0), [Cal_daf]) as Cal_daf
      ,[RD]
      ,[DateAnalysis]
      ,[DateReceived]
      ,[ThicknessFrom]
      ,[ThicknessTo]
      ,[Na2O]
      ,[CaO]
  FROM [UPLOAD_Geology_Pit_Monitoring]
  where Company = @p_company
    and CONVERT(varchar(10),CreatedOn,120) = @p_date
    --order by Sample_ID, SampleType, Lab_ID
	--req sprint4
	order by Lab_ID;

end



CREATE OR ALTER procedure [dbo].[Report_GEOLOGY_PIT_Header] @p_company varchar(32), @p_date varchar(10), @p_tittle varchar(50)
as
begin
    SELECT top 1
        Date_Detail as DATE_1
        , '' as DATE_2
        , JOB_NO
        , Nomor as ReportNo
        , @p_tittle as Tittle
        , CreatedOn as DateReport
        , Report_To
        , Date_Received as Date_Receive_sample
        , Nomor as HEADER_NO
    from UPLOAD_Geology_Pit_Monitoring_Header
    where Company = @p_company
        and CONVERT(varchar(10),CreatedOn,120) = @p_date
    order by recordid desc
end




CREATE OR ALTER procedure [dbo].[Report_GEOLOGY_EXPLORASI] @p_company varchar(32), @p_date varchar(10)
as
begin
    SELECT 
      [Sample_ID]
      ,[SampleType]
      ,[Lab_ID]
      ,[Mass_Spl]
      ,[TM]
      ,[M]
      ,[VM]
      ,[Ash]
      ,[FC]
      ,[TS]
      ,CONVERT(decimal(10,0), [Cal_ad]) as Cal_ad
      ,CONVERT(decimal(10,0), [Cal_db]) as Cal_db
      ,CONVERT(decimal(10,0), [Cal_ar]) as Cal_ar
      ,CONVERT(decimal(10,0), [Cal_daf]) as Cal_daf
      ,[RD]
      ,[DateAnalysis]
      ,[DateReceived]
      ,[ThicknessFrom]
      ,[ThicknessTo]
      ,[Na2O]
      ,[CaO]
  FROM [UPLOAD_Geology_Explorasi]
  where Company = @p_company
    and CONVERT(varchar(10),CreatedOn,120) = @p_date
    --order by Sample_ID, SampleType, Lab_ID
  --req sprint4
  order by Lab_ID;
end



CREATE OR ALTER procedure [dbo].[Report_GEOLOGY_EXPLORASI_Header] @p_company varchar(32), @p_date varchar(10), @p_tittle varchar(50)
as
begin
    SELECT top 1
        Date_Detail as DATE_1
        , '' as DATE_2
        , JOB_NO
        , Nomor as ReportNo
        , @p_tittle as Tittle
        , CreatedOn as DateReport
        , Report_To
        , Date_Received as Date_Receive_sample
        , Nomor as HEADER_NO
    from UPLOAD_Geology_Explorasi_Header
    where Company = @p_company
        and CONVERT(varchar(10),CreatedOn,120) = @p_date
    order by RecordId desc
end