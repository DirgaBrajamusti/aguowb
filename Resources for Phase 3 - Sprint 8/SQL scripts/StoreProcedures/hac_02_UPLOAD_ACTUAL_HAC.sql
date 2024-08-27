IF OBJECT_ID('UPLOAD_ACTUAL_HAC') IS NULL
    EXEC('CREATE PROCEDURE UPLOAD_ACTUAL_HAC AS SET NOCOUNT ON;')
GO

alter procedure [dbo].[UPLOAD_ACTUAL_HAC] @p_file integer
as
begin
    declare @vCount integer = 0
    declare @vCount2 integer = 0
    declare @vStatus varchar(32) = ''
    declare @vMessage varchar(8000) = ''
    
    declare @vNow datetime = GetDate()
    
    exec UPLOAD_ACTUAL_HAC_Header @p_file
    
    BEGIN TRY  
        -- for Status: New
        insert into UPLOAD_HAC
        (
          [TEMPORARY]
          ,[Header]
          ,Company
          ,Sheet
          ,[CreatedBy]
          ,[CreatedOn]
          ,[Date_Sampling]
          ,[Day_work]
          ,Tonnage
          ,[LOT]
          ,[Lab_ID]
          ,[TM]
          ,[M]
          ,[ASH]
          ,[TS]
          ,[CV]
          ,[Remark]
          , CreatedOn_Date_Only
          , CreatedOn_Year_Only
        )
        select
          t.[RecordId]
          , -1 -- we don't use this Column anymore [use SheetName]
          ,t.Company
          ,t.Sheet
          ,t.[CreatedBy]
          ,@vNow
          ,t.[Date_Sampling]
          ,t.[Day_work]
          ,dbo.To_Decimal_3(t.Tonnage, -1.0)
          ,t.[LOT]
          ,t.[Lab_ID]
          ,dbo.To_Decimal(t.TM, -1.0)
          ,dbo.To_Decimal(t.M, -1.0)
          ,dbo.To_Decimal(t.ASH, -1.0)
          ,dbo.To_Decimal(t.TS, -1.0)
          ,CONVERT(int, t.[CV])
          ,t.[Remark]
          , t.CreatedOn_Date_Only
          , t.CreatedOn_Year_Only
        from TEMPORARY_HAC t
        where 
            t.[Status] = 'New'
            and t.Header in (select RecordId from TEMPORARY_HAC_Header where File_Physical = @p_file)

        -- for Status: Edit
        -- use "SQL Server UPDATE JOIN", https://www.sqlservertutorial.net/sql-server-basics/sql-server-update-join/

        UPDATE 
            UPLOAD_HAC
        SET 
            UPLOAD_HAC.[TEMPORARY] = t2.[RecordId]
            
            -- * Not Created
            --,UPLOAD_HAC.CreatedBy = t2.CreatedBy
            --,UPLOAD_HAC.CreatedOn = t2.CreatedOn
            
            -- * but Modified
            ,UPLOAD_HAC.LastModifiedBy = t2.CreatedBy
            ,UPLOAD_HAC.LastModifiedOn = @vNow
            
            -- Handle others columns
            ,UPLOAD_HAC.Company = t2.Company
            ,UPLOAD_HAC.Sheet = t2.Sheet
            ,UPLOAD_HAC.[Date_Sampling] = t2.[Date_Sampling]
            ,UPLOAD_HAC.[Day_work] = t2.[Day_work]
            ,UPLOAD_HAC.[Tonnage] = dbo.To_Decimal_3(t2.Tonnage, -1.0)
            ,UPLOAD_HAC.[LOT] = t2.[LOT]
            ,UPLOAD_HAC.[Lab_ID] = t2.[Lab_ID]
            ,UPLOAD_HAC.[TM] = dbo.To_Decimal(t2.TM, -1.0)
			,UPLOAD_HAC.[M] = dbo.To_Decimal(t2.M, -1.0)
			,UPLOAD_HAC.[ASH] = dbo.To_Decimal(t2.ASH, -1.0)
			,UPLOAD_HAC.[TS] = dbo.To_Decimal(t2.TS, -1.0)
            ,UPLOAD_HAC.[CV] = CONVERT(int, t2.[CV])
            ,UPLOAD_HAC.[Remark] = t2.[Remark]
            , UPLOAD_HAC.CreatedOn_Date_Only = t2.CreatedOn_Date_Only
            , UPLOAD_HAC.CreatedOn_Year_Only = t2.CreatedOn_Year_Only
        FROM 
            UPLOAD_HAC t1
                INNER JOIN TEMPORARY_HAC t2 ON (t1.Lab_ID = t2.Lab_ID and t1.Company = t2.Company and t1.CreatedOn_Year_Only = t2.CreatedOn_Year_Only)
                INNER JOIN TEMPORARY_HAC_Header h on t2.Header = h.RecordId
        WHERE
            h.File_Physical = @p_file
            and t2.[Status] = 'Update'

        select @vCount = Count(RecordId)
        from UPLOAD_HAC
        where (CreatedOn = @vNow) or (LastModifiedOn = @vNow)

        select @vCount2 = Count(t2.RecordId)
        FROM  TEMPORARY_HAC t2
            INNER JOIN TEMPORARY_HAC_Header h on t2.Header = h.RecordId
        WHERE h.File_Physical = @p_file

        select @vStatus = 'Ok'
        select @vMessage = 'UPLOAD_ACTUAL_HAC'
    END TRY  
    BEGIN CATCH  
         select @vCount = 0
         select @vCount2 = 0
         select @vStatus = 'Error'
         select @vMessage = ERROR_MESSAGE()
    END CATCH  

    -- tampilkan "Hasil Eksekusi"
    select @vCount as cCount, @vCount2 as cCount2, @vStatus as cStatus, @vMessage as cMessage
end
