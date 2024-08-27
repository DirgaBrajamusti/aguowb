IF OBJECT_ID('UPLOAD_ACTUAL_CRUSHING_PLANT') IS NULL
    EXEC('CREATE PROCEDURE UPLOAD_ACTUAL_CRUSHING_PLANT AS SET NOCOUNT ON;')
GO

alter procedure [dbo].[UPLOAD_ACTUAL_CRUSHING_PLANT] @p_file integer
as
begin
    declare @vCount integer = 0
    declare @vCount2 integer = 0
    declare @vStatus varchar(32) = ''
    declare @vMessage varchar(8000) = ''
    declare @header_id bigint
    
    declare @vCompany varchar(32) = ''
    
    declare @vNow datetime = GetDate()
    
    exec UPLOAD_ACTUAL_CRUSHING_PLANT_Header @p_file
    
    select top 1 @header_id = RecordId, @vCompany = Company
    from UPLOAD_CRUSHING_PLANT_Header
    order by RecordId desc
    
    BEGIN TRY  
        -- for Status: New
        insert into UPLOAD_CRUSHING_PLANT 
        (
            [TEMPORARY]
            ,[Header]
            ,Company
            ,[CreatedBy]
            ,[CreatedOn]
            ,[Sheet]
            ,[Date_Production]
            ,[Shift_Work]
            ,[Tonnage]
            ,[Sample_ID]
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
        )
        select
            t.[RecordId]
            ,@header_id
            ,@vCompany
            ,t.[CreatedBy]
            ,@vNow
            ,h.Sheet
            ,t.[Date_Production]
            ,t.[Shift_Work]
            ,CONVERT(decimal(10,3), t.[Tonnage])
            ,t.[Sample_ID]
            ,CONVERT(decimal(10,2), t.[TM])
            ,CONVERT(decimal(10,2), t.[M])
            ,CONVERT(decimal(10,2), t.[ASH_adb])
            ,t.[ASH_arb]
            ,CONVERT(decimal(10,2), t.[VM_adb])
            ,t.[VM_arb]
            ,t.[FC_adb]
            ,t.[FC_arb]
            ,CONVERT(decimal(10,2), t.[TS_adb])
            ,t.[TS_arb]
            ,CONVERT(decimal(10,2), t.[CV_adb])
            , t.CV_db
            , t.CV_arb
            , t.CV_daf
            , t.CV_ad_15
            , t.CV_ad_16
            , t.CV_ad_17
            ,t.[Remark]
        from TEMPORARY_CRUSHING_PLANT t inner join TEMPORARY_CRUSHING_PLANT_Header h on t.Header = h.RecordId
        where 
            h.File_Physical = @p_file
            and t.[Status] = 'New'
            and t.[Sample_ID] not in (select [Sample_ID] from UPLOAD_CRUSHING_PLANT)

        -- for Status: Edit
        -- use "SQL Server UPDATE JOIN", https://www.sqlservertutorial.net/sql-server-basics/sql-server-update-join/

        UPDATE 
            UPLOAD_CRUSHING_PLANT
        SET 
            UPLOAD_CRUSHING_PLANT.[TEMPORARY] = t2.[RecordId]
            --,UPLOAD_CRUSHING_PLANT.[Header] = @header_id
            -- * Not Created
            --,UPLOAD_CRUSHING_PLANT.[CreatedBy] = t2.[CreatedBy]
            --,UPLOAD_CRUSHING_PLANT.[CreatedOn] = t2.[CreatedOn]
            -- * but Modified
            ,UPLOAD_CRUSHING_PLANT.[LastModifiedBy] = t2.[CreatedBy]
            ,UPLOAD_CRUSHING_PLANT.[LastModifiedOn] = @vNow
            ,UPLOAD_CRUSHING_PLANT.[Sheet] = h.Sheet
            ,UPLOAD_CRUSHING_PLANT.[Date_Production] = t2.[Date_Production]
            ,UPLOAD_CRUSHING_PLANT.[Shift_Work] = t2.[Shift_Work]
            ,UPLOAD_CRUSHING_PLANT.[Tonnage] = CONVERT(decimal(10,3), t2.[Tonnage])
            ,UPLOAD_CRUSHING_PLANT.[Sample_ID] = t2.[Sample_ID]
            ,UPLOAD_CRUSHING_PLANT.[TM] = CONVERT(decimal(10,2), t2.[TM])
            ,UPLOAD_CRUSHING_PLANT.[M] = CONVERT(decimal(10,2), t2.[M])
            ,UPLOAD_CRUSHING_PLANT.[ASH_adb] = CONVERT(decimal(10,2), t2.[ASH_adb])
            ,UPLOAD_CRUSHING_PLANT.[ASH_arb] = t2.[ASH_arb]
            ,UPLOAD_CRUSHING_PLANT.[VM_adb] = CONVERT(decimal(10,2), t2.[VM_adb])
            ,UPLOAD_CRUSHING_PLANT.[VM_arb] = t2.[VM_arb]
            ,UPLOAD_CRUSHING_PLANT.[FC_adb] = t2.[FC_adb]
            ,UPLOAD_CRUSHING_PLANT.[FC_arb] = t2.[FC_arb]
            ,UPLOAD_CRUSHING_PLANT.[TS_adb] = CONVERT(decimal(10,2), t2.[TS_adb])
            ,UPLOAD_CRUSHING_PLANT.[TS_arb] = t2.[TS_arb]
            , UPLOAD_CRUSHING_PLANT.CV_adb = CONVERT(decimal(10,2), t2.[CV_adb])
            , UPLOAD_CRUSHING_PLANT.CV_db = t2.CV_db
            , UPLOAD_CRUSHING_PLANT.CV_arb = t2.CV_arb
            , UPLOAD_CRUSHING_PLANT.CV_daf = t2.CV_daf
            , UPLOAD_CRUSHING_PLANT.CV_ad_15 = t2.CV_ad_15
            , UPLOAD_CRUSHING_PLANT.CV_ad_16 = t2.CV_ad_16
            , UPLOAD_CRUSHING_PLANT.CV_ad_17 = t2.CV_ad_17
            ,UPLOAD_CRUSHING_PLANT.[Remark] = t2.[Remark]
        FROM 
            UPLOAD_CRUSHING_PLANT t1
                INNER JOIN TEMPORARY_CRUSHING_PLANT t2 ON t1.Sample_ID = t2.Sample_ID
                INNER JOIN TEMPORARY_CRUSHING_PLANT_Header h on t2.Header = h.RecordId
        WHERE
            h.File_Physical = @p_file
            and t2.[Status] = 'Update'

        select @vCount = Count(RecordId)
        from UPLOAD_CRUSHING_PLANT
        where (CreatedOn = @vNow) or (LastModifiedOn = @vNow)

        select @vCount2 = Count(t2.RecordId)
        FROM  TEMPORARY_CRUSHING_PLANT t2
            INNER JOIN TEMPORARY_CRUSHING_PLANT_Header h on t2.Header = h.RecordId
        WHERE h.File_Physical = @p_file

        select @vStatus = 'Ok'
        select @vMessage = 'UPLOAD_ACTUAL_CRUSHING_PLANT'
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
