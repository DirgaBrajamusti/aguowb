IF OBJECT_ID('UPLOAD_ACTUAL_GEOLOGY_EXPLORASI') IS NULL
    EXEC('CREATE PROCEDURE UPLOAD_ACTUAL_GEOLOGY_EXPLORASI AS SET NOCOUNT ON;')
GO

alter procedure [dbo].[UPLOAD_ACTUAL_GEOLOGY_EXPLORASI] @p_file integer
as
begin
    declare @vCount integer = 0
    declare @vCount2 integer = 0
    declare @vStatus varchar(32) = ''
    declare @vMessage varchar(8000) = ''
    
    declare @vNow datetime = GetDate()
    
    exec UPLOAD_ACTUAL_GEOLOGY_EXPLORASI_Header @p_file
    
    BEGIN TRY  
        -- for Status: New
        insert into UPLOAD_Geology_Explorasi 
        (
            [TEMPORARY]
            ,[Header]
            ,Company
            ,Sheet
            ,[CreatedBy]
            ,[CreatedOn]
            ,[Sample_ID]
            ,[SampleType]
            ,[Lab_ID]
            ,[Mass_Spl]
            ,[TM]
            ,[M]
            ,[VM]
            ,[Ash]
            ,[FC]
            ,[TS]
            ,[Cal_ad]
            ,[Cal_db]
            ,[Cal_ar]
            ,[Cal_daf]
            ,[RD]
        )
        select
            t.[RecordId]
            , -1 -- we don't use this Column anymore [use SheetName]
            ,h.Company
            ,h.Sheet
            ,t.[CreatedBy]
            ,@vNow
            ,t.[Sample_ID]
            ,t.[SampleType]
            ,t.[Lab_ID]
            ,dbo.To_Decimal(t.Mass_Spl, -1.0)
            ,dbo.To_Decimal(t.TM, -1.0)
            ,dbo.To_Decimal(t.M, -1.0)
            ,dbo.To_Decimal(t.VM, -1.0)
            ,dbo.To_Decimal(t.Ash, -1.0)
            ,t.[FC]
            ,dbo.To_Decimal(t.TS, -1.0)
            ,dbo.To_Decimal_0(t.Cal_ad, -1.0)
            ,t.[Cal_db]
            ,t.[Cal_ar]
            ,t.[Cal_daf]
            ,dbo.To_Decimal(t.RD, -1.0)
        from TEMPORARY_Geology_Explorasi t inner join TEMPORARY_Geology_Explorasi_Header h on t.Header = h.RecordId
        where 
            h.File_Physical = @p_file
            and t.[Status] = 'New'
            and t.[Lab_ID] not in (select [Lab_ID] from UPLOAD_Geology_Explorasi)

        -- for Status: Edit
        -- use "SQL Server UPDATE JOIN", https://www.sqlservertutorial.net/sql-server-basics/sql-server-update-join/

        UPDATE 
            UPLOAD_Geology_Explorasi
        SET 
            UPLOAD_Geology_Explorasi.[TEMPORARY] = t2.[RecordId]
            
            -- * Not Created
            --,UPLOAD_Geology_Explorasi.CreatedBy = t2.CreatedBy
            --,UPLOAD_Geology_Explorasi.CreatedOn = t2.CreatedOn
            
            -- * but Modified
            ,UPLOAD_Geology_Explorasi.LastModifiedBy = t2.CreatedBy
            ,UPLOAD_Geology_Explorasi.LastModifiedOn = @vNow
            
            -- Special Case for value "Company"
            -- change value with Latest if uploaded on same Date { Today }
            ,UPLOAD_Geology_Explorasi.Company =
                    (case when Convert(varchar(10), t1.CreatedOn, 120) = Convert(varchar(10), @vNow, 120)
                          then h.Company
                          else t1.Company end)
            
            -- Handle others columns
            ,UPLOAD_Geology_Explorasi.Sheet = h.Sheet
            ,UPLOAD_Geology_Explorasi.[Sample_ID] = t2.[Sample_ID]
            ,UPLOAD_Geology_Explorasi.[SampleType] = t2.[SampleType]
            ,UPLOAD_Geology_Explorasi.[Lab_ID] = t2.[Lab_ID]
            ,UPLOAD_Geology_Explorasi.[Mass_Spl] = dbo.To_Decimal(t2.Mass_Spl, -1.0)
            ,UPLOAD_Geology_Explorasi.[TM] = dbo.To_Decimal(t2.TM, -1.0)
            ,UPLOAD_Geology_Explorasi.[M] = dbo.To_Decimal(t2.M, -1.0)
            ,UPLOAD_Geology_Explorasi.[VM] = dbo.To_Decimal(t2.VM, -1.0)
            ,UPLOAD_Geology_Explorasi.[Ash] = dbo.To_Decimal(t2.Ash, -1.0)
            ,UPLOAD_Geology_Explorasi.[FC] = t2.[FC]
            ,UPLOAD_Geology_Explorasi.[TS] = dbo.To_Decimal(t2.TS, -1.0)
            ,UPLOAD_Geology_Explorasi.[Cal_ad] = dbo.To_Decimal_0(t2.Cal_ad, -1.0)
            ,UPLOAD_Geology_Explorasi.[Cal_db] = t2.[Cal_db]
            ,UPLOAD_Geology_Explorasi.[Cal_ar] = t2.[Cal_ar]
            ,UPLOAD_Geology_Explorasi.[Cal_daf] = t2.[Cal_daf]
            ,UPLOAD_Geology_Explorasi.[RD] = dbo.To_Decimal(t2.RD, -1.0)
        FROM 
            UPLOAD_Geology_Explorasi t1
                INNER JOIN TEMPORARY_Geology_Explorasi t2 ON t1.Lab_ID = t2.Lab_ID
                INNER JOIN TEMPORARY_Geology_Explorasi_Header h on t2.Header = h.RecordId
        WHERE
            h.File_Physical = @p_file
            and t2.[Status] = 'Update'

        select @vCount = Count(RecordId)
        from UPLOAD_Geology_Explorasi
        where (CreatedOn = @vNow) or (LastModifiedOn = @vNow)

        select @vCount2 = Count(t2.RecordId)
        FROM  TEMPORARY_Geology_Explorasi t2
            INNER JOIN TEMPORARY_Geology_Explorasi_Header h on t2.Header = h.RecordId
        WHERE h.File_Physical = @p_file

        select @vStatus = 'Ok'
        select @vMessage = 'UPLOAD_ACTUAL_GEOLOGY_EXPLORASI'
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
