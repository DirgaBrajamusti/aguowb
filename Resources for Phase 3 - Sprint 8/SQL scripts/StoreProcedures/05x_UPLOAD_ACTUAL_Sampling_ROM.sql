IF OBJECT_ID('UPLOAD_ACTUAL_Sampling_ROM') IS NULL
    EXEC('CREATE PROCEDURE UPLOAD_ACTUAL_Sampling_ROM AS SET NOCOUNT ON;')
GO

alter procedure [dbo].[UPLOAD_ACTUAL_Sampling_ROM] @p_file integer
as
begin
    declare @vCount integer = 0
    declare @vCount2 integer = 0
    declare @vStatus varchar(32) = ''
    declare @vMessage varchar(8000) = ''
    
    declare @vNow datetime = GetDate()
    declare @leco_Stamp varchar(255) = ''
    
    exec UPLOAD_ACTUAL_Sampling_ROM_Header @p_file
    
    BEGIN TRY  
        -- for Status: New
        insert into UPLOAD_Sampling_ROM 
        (
          [TEMPORARY]
          ,[Header]
          ,Company
          ,Sheet
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
          , CreatedOn_Date_Only
          , CreatedOn_Year_Only
          , LECO_Stamp
        )
        select
          t.[RecordId]
          , -1 -- we don't use this Column anymore [use SheetName]
          ,t.Company
          ,t.Sheet
          ,t.[CreatedBy]
          ,@vNow
          ,t.[Date_Request]
          ,t.[Date_Sampling]
          ,t.[Day_work]
          ,t.[LOT]
          ,t.[Lab_ID]
          ,dbo.To_Decimal(t.TM, -1.0)
          ,dbo.To_Decimal(t.M, -1.0)
          ,dbo.To_Decimal(t.ASH, -1.0)
          ,dbo.To_Decimal(t.TS, -1.0)
          ,CONVERT(int, t.[CV])
          ,t.[Remark]
          ,t.[Seam]
          , t.CreatedOn_Date_Only
            , t.CreatedOn_Year_Only
            , ''
        from TEMPORARY_Sampling_ROM t
        where 
            t.[Status] = 'New'
            and t.Header in (select RecordId from TEMPORARY_Sampling_ROM_Header where File_Physical = @p_file)

        -- for Status: Edit
        -- use "SQL Server UPDATE JOIN", https://www.sqlservertutorial.net/sql-server-basics/sql-server-update-join/

        UPDATE 
            UPLOAD_Sampling_ROM
        SET 
            UPLOAD_Sampling_ROM.[TEMPORARY] = t2.[RecordId]
            
            -- * Not Created
            --,UPLOAD_Sampling_ROM.CreatedBy = t2.CreatedBy
            --,UPLOAD_Sampling_ROM.CreatedOn = t2.CreatedOn
            
            -- * but Modified
            ,UPLOAD_Sampling_ROM.LastModifiedBy = t2.CreatedBy
            ,UPLOAD_Sampling_ROM.LastModifiedOn = @vNow
            
            -- Handle others columns
            ,UPLOAD_Sampling_ROM.Company = t2.Company
            ,UPLOAD_Sampling_ROM.Sheet = t2.Sheet
            ,UPLOAD_Sampling_ROM.[Date_Request] = t2.[Date_Request]
            ,UPLOAD_Sampling_ROM.[Date_Sampling] = t2.[Date_Sampling]
            ,UPLOAD_Sampling_ROM.[Day_work] = t2.[Day_work]
            ,UPLOAD_Sampling_ROM.[LOT] = t2.[LOT]
            ,UPLOAD_Sampling_ROM.[Lab_ID] = t2.[Lab_ID]
            ,UPLOAD_Sampling_ROM.[TM] = dbo.To_Decimal(t2.TM, -1.0)
			,UPLOAD_Sampling_ROM.[M] = dbo.To_Decimal(t2.M, -1.0)
			,UPLOAD_Sampling_ROM.[ASH] = dbo.To_Decimal(t2.ASH, -1.0)
			,UPLOAD_Sampling_ROM.[TS] = dbo.To_Decimal(t2.TS, -1.0)
            ,UPLOAD_Sampling_ROM.[CV] = CONVERT(int, t2.[CV])
            ,UPLOAD_Sampling_ROM.[Remark] = t2.[Remark]
            ,UPLOAD_Sampling_ROM.[Seam] = t2.[Seam]
            , UPLOAD_Sampling_ROM.CreatedOn_Date_Only = t2.CreatedOn_Date_Only
            , UPLOAD_Sampling_ROM.CreatedOn_Year_Only = t2.CreatedOn_Year_Only
            , UPLOAD_Sampling_ROM.LECO_Stamp = ''
        FROM 
            UPLOAD_Sampling_ROM t1
                INNER JOIN TEMPORARY_Sampling_ROM t2 ON (t1.Lab_ID = t2.Lab_ID and t1.Company = t2.Company and t1.CreatedOn_Year_Only = t2.CreatedOn_Year_Only)
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
        
        exec PROCESS_LECO_for_Sampling_ROM @vNow, @leco_Stamp output
        
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
    select @vCount as cCount, @vCount2 as cCount2, @vStatus as cStatus, @vMessage as cMessage, @leco_Stamp as leco_Stamp
end
