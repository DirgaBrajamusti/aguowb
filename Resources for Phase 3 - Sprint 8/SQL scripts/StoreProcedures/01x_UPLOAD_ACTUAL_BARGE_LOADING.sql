IF OBJECT_ID('UPLOAD_ACTUAL_BARGE_LOADING') IS NULL
    EXEC('CREATE PROCEDURE UPLOAD_ACTUAL_BARGE_LOADING AS SET NOCOUNT ON;')
GO

alter procedure [dbo].[UPLOAD_ACTUAL_BARGE_LOADING] @p_file integer
as
begin
    declare @vCount integer = 0
    declare @vCount2 integer = 0
    declare @vStatus varchar(32) = ''
    declare @vMessage varchar(8000) = ''
    
    declare @vNow datetime = GetDate()
    declare @leco_Stamp varchar(255) = ''
    
    exec UPLOAD_ACTUAL_BARGE_LOADING_Header @p_file
    
    -- real code
    BEGIN TRY  
        -- for Status: New
        insert into UPLOAD_BARGE_LOADING 
        (
            [TEMPORARY]
            ,[Header]
            ,Company
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
            , CV_Plan
            , TM_Plan
            , ASH_Plan
            , TS_Plan
            , CreatedOn_Date_Only
            , CreatedOn_Year_Only
            , LECO_Stamp
        )
        select
            t.[RecordId]
            , -1 -- we don't use this Column anymore [use SheetName]
            ,t.Company
            ,t.[CreatedBy]
            ,@vNow
            ,t.Sheet
            ,t.[JOB_Number]
            ,t.[ID_Number]
            ,t.[Service_Trip_Number]
            ,t.[Date_Sampling]
            ,t.[Date_Report]
            ,dbo.To_Decimal_3(t.Tonnage, -1.0)
            ,t.[Name]
            ,t.[Destination]
            ,dbo.To_Decimal(t.Temperature, -1.0)
            ,dbo.To_Decimal(t.TM, -1.0)
            ,dbo.To_Decimal(t.M, -1.0)
            ,dbo.To_Decimal(t.ASH_adb, -1.0)
            ,t.[ASH_arb]
            ,dbo.To_Decimal(t.VM_adb, -1.0)
            ,t.[VM_arb]
            ,t.[FC_adb]
            ,t.[FC_arb]
            ,dbo.To_Decimal(t.TS_adb, -1.0)
            ,t.[TS_arb]
            ,dbo.To_Decimal(t.CV_adb, -1.0)
            , t.CV_db
            , t.CV_arb
            , t.CV_daf
            , t.CV_ad_15
            , t.CV_ad_16
            , t.CV_ad_17
            ,t.[Remark]
            ,dbo.To_Decimal(t.CV_Plan, -1.0)
            ,dbo.To_Decimal(t.TM_Plan, -1.0)
            ,dbo.To_Decimal(t.ASH_Plan, -1.0)
            ,dbo.To_Decimal(t.TS_Plan, -1.0)
            , t.CreatedOn_Date_Only
            , t.CreatedOn_Year_Only
            , ''
        from TEMPORARY_BARGE_LOADING t
        where
            t.[Status] = 'New'
            and t.Header in (select RecordId from TEMPORARY_BARGE_LOADING_Header where File_Physical = @p_file)

        -- for Status: Edit
        -- use "SQL Server UPDATE JOIN", https://www.sqlservertutorial.net/sql-server-basics/sql-server-update-join/

        UPDATE 
            UPLOAD_BARGE_LOADING
        SET 
            UPLOAD_BARGE_LOADING.[TEMPORARY] = t2.[RecordId]
            
            --- * Not Created
            --,UPLOAD_BARGE_LOADING.CreatedBy = t2.CreatedBy
            --,UPLOAD_BARGE_LOADING.CreatedOn = t2.CreatedOn
            
            -- * but Modified
            ,UPLOAD_BARGE_LOADING.LastModifiedBy = t2.CreatedBy
            ,UPLOAD_BARGE_LOADING.LastModifiedOn = @vNow
            
            -- Handle others columns
            ,UPLOAD_BARGE_LOADING.Company = t2.Company
            ,UPLOAD_BARGE_LOADING.Sheet = t2.Sheet
            ,UPLOAD_BARGE_LOADING.[JOB_Number] = t2.[JOB_Number]
            ,UPLOAD_BARGE_LOADING.[ID_Number] = t2.[ID_Number]
            ,UPLOAD_BARGE_LOADING.[Service_Trip_Number] = t2.[Service_Trip_Number]
            ,UPLOAD_BARGE_LOADING.[Date_Sampling] = t2.[Date_Sampling]
            ,UPLOAD_BARGE_LOADING.[Date_Report] = t2.[Date_Report]
            ,UPLOAD_BARGE_LOADING.[Tonnage] = dbo.To_Decimal_3(t2.Tonnage, -1.0)
            ,UPLOAD_BARGE_LOADING.[Name] = t2.[Name]
            ,UPLOAD_BARGE_LOADING.[Destination] = t2.[Destination]
            ,UPLOAD_BARGE_LOADING.[Temperature] = dbo.To_Decimal(t2.Temperature, -1.0)
            ,UPLOAD_BARGE_LOADING.[TM] = dbo.To_Decimal(t2.TM, -1.0)
            ,UPLOAD_BARGE_LOADING.[M] = dbo.To_Decimal(t2.M, -1.0)
            ,UPLOAD_BARGE_LOADING.[ASH_adb] = dbo.To_Decimal(t2.ASH_adb, -1.0)
            ,UPLOAD_BARGE_LOADING.[ASH_arb] = t2.[ASH_arb]
            ,UPLOAD_BARGE_LOADING.[VM_adb] = dbo.To_Decimal(t2.VM_adb, -1.0)
            ,UPLOAD_BARGE_LOADING.[VM_arb] = t2.[VM_arb]
            ,UPLOAD_BARGE_LOADING.[FC_adb] = t2.[FC_adb]
            ,UPLOAD_BARGE_LOADING.[FC_arb] = t2.[FC_arb]
            ,UPLOAD_BARGE_LOADING.[TS_adb] = dbo.To_Decimal(t2.TS_adb, -1.0)
            ,UPLOAD_BARGE_LOADING.[TS_arb] = t2.[TS_arb]
            , UPLOAD_BARGE_LOADING.CV_adb = dbo.To_Decimal(t2.CV_adb, -1.0)
            , UPLOAD_BARGE_LOADING.CV_db = t2.CV_db
            , UPLOAD_BARGE_LOADING.CV_arb = t2.CV_arb
            , UPLOAD_BARGE_LOADING.CV_daf = t2.CV_daf
            , UPLOAD_BARGE_LOADING.CV_ad_15 = t2.CV_ad_15
            , UPLOAD_BARGE_LOADING.CV_ad_16 = t2.CV_ad_16
            , UPLOAD_BARGE_LOADING.CV_ad_17 = t2.CV_ad_17
            ,UPLOAD_BARGE_LOADING.[Remark] = t2.[Remark]
            ,UPLOAD_BARGE_LOADING.CV_Plan = dbo.To_Decimal(t2.CV_Plan, -1.0)
            ,UPLOAD_BARGE_LOADING.TM_Plan = dbo.To_Decimal(t2.TM_Plan, -1.0)
            ,UPLOAD_BARGE_LOADING.ASH_Plan = dbo.To_Decimal(t2.ASH_Plan, -1.0)
            ,UPLOAD_BARGE_LOADING.TS_Plan = dbo.To_Decimal(t2.TS_Plan, -1.0)
            , UPLOAD_BARGE_LOADING.CreatedOn_Date_Only = t2.CreatedOn_Date_Only
            , UPLOAD_BARGE_LOADING.CreatedOn_Year_Only = t2.CreatedOn_Year_Only
            , UPLOAD_BARGE_LOADING.LECO_Stamp = ''
        FROM 
            UPLOAD_BARGE_LOADING t1
                INNER JOIN TEMPORARY_BARGE_LOADING t2 ON (t1.ID_Number = t2.ID_Number and t1.Company = t2.Company and t1.CreatedOn_Year_Only = t2.CreatedOn_Year_Only)
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
        
        exec PROCESS_LECO_for_BARGE_LOADING @vNow, @leco_Stamp output
        
        select @vStatus = 'Ok'
        select @vMessage = 'UPLOAD_ACTUAL_BARGE_LOADING'
    END TRY  
    BEGIN CATCH  
         select @vCount = 0
         select @vCount2 = 0
         select @vStatus = 'Error'
         select @vMessage = ERROR_MESSAGE()
    END CATCH  
    
    exec UPLOAD_ACTUAL_BARGE_LOADING_COA @p_file
    
    -- tampilkan "Hasil Eksekusi"
    select @vCount as cCount, @vCount2 as cCount2, @vStatus as cStatus, @vMessage as cMessage, @leco_Stamp as leco_Stamp
end