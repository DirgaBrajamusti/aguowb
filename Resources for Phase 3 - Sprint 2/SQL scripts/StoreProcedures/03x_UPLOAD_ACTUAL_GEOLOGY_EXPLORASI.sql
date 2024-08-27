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
    declare @header_id bigint
    
    declare @vCompany varchar(32) = ''
    
    declare @vNow datetime = GetDate()
    
    exec UPLOAD_ACTUAL_GEOLOGY_EXPLORASI_Header @p_file
    
    select top 1 @header_id = RecordId, @vCompany = Company
    from UPLOAD_Geology_Explorasi_Header
    order by RecordId desc
    
    BEGIN TRY  
        -- for Status: New
        insert into UPLOAD_Geology_Explorasi 
        (
            [TEMPORARY]
            ,[Header]
            ,Company
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
            ,@header_id
            ,@vCompany
            ,t.[CreatedBy]
            ,@vNow
            ,t.[Sample_ID]
            ,t.[SampleType]
            ,t.[Lab_ID]
            ,CONVERT(decimal(10,2), t.[Mass_Spl])
            ,CONVERT(decimal(10,2), t.[TM])
            ,CONVERT(decimal(10,2), t.[M])
            ,CONVERT(decimal(10,2), t.[VM])
            ,CONVERT(decimal(10,2), t.[Ash])
            ,t.[FC]
            ,CONVERT(decimal(10,2), t.[TS])
            ,CONVERT(decimal(10,0), t.[Cal_ad])
            ,t.[Cal_db]
            ,t.[Cal_ar]
            ,t.[Cal_daf]
            ,CONVERT(decimal(10,2), t.[RD])
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
            --,UPLOAD_Geology_Explorasi.[Header] = @header_id
            -- * Not Created
            --,UPLOAD_Geology_Explorasi.[CreatedBy] = t2.[CreatedBy]
            --,UPLOAD_Geology_Explorasi.[CreatedOn] = t2.[CreatedOn]
            -- * but Modified
            ,UPLOAD_Geology_Explorasi.[LastModifiedBy] = t2.[CreatedBy]
            ,UPLOAD_Geology_Explorasi.[LastModifiedOn] = @vNow
            ,UPLOAD_Geology_Explorasi.[Sample_ID] = t2.[Sample_ID]
            ,UPLOAD_Geology_Explorasi.[SampleType] = t2.[SampleType]
            ,UPLOAD_Geology_Explorasi.[Lab_ID] = t2.[Lab_ID]
            ,UPLOAD_Geology_Explorasi.[Mass_Spl] = CONVERT(decimal(10,2), t2.[Mass_Spl])
            ,UPLOAD_Geology_Explorasi.[TM] = CONVERT(decimal(10,2), t2.[TM])
            ,UPLOAD_Geology_Explorasi.[M] = CONVERT(decimal(10,2), t2.[M])
            ,UPLOAD_Geology_Explorasi.[VM] = CONVERT(decimal(10,2), t2.[VM])
            ,UPLOAD_Geology_Explorasi.[Ash] = CONVERT(decimal(10,2), t2.[Ash])
            ,UPLOAD_Geology_Explorasi.[FC] = t2.[FC]
            ,UPLOAD_Geology_Explorasi.[TS] = CONVERT(decimal(10,2), t2.[TS])
            ,UPLOAD_Geology_Explorasi.[Cal_ad] = CONVERT(decimal(10,0), t2.[Cal_ad])
            ,UPLOAD_Geology_Explorasi.[Cal_db] = t2.[Cal_db]
            ,UPLOAD_Geology_Explorasi.[Cal_ar] = t2.[Cal_ar]
            ,UPLOAD_Geology_Explorasi.[Cal_daf] = t2.[Cal_daf]
            ,UPLOAD_Geology_Explorasi.[RD] = CONVERT(decimal(10,2), t2.[RD])
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
