IF OBJECT_ID('UPLOAD_ACTUAL_Barge_LineUp_VLU') IS NULL
    EXEC('CREATE PROCEDURE UPLOAD_ACTUAL_Barge_LineUp_VLU AS SET NOCOUNT ON;')
GO

alter procedure [dbo].[UPLOAD_ACTUAL_Barge_LineUp_VLU] @p_file integer
as
begin
    declare @vCount integer = 0
    declare @vCount2 integer = 0
    declare @vStatus varchar(32) = ''
    declare @vMessage varchar(8000) = ''
    
    declare @vNow datetime = GetDate()
        
    BEGIN TRY  
        -- for Status: New
        insert into UPLOAD_VLU
        (
          [TEMPORARY]
          ,[Site]
          ,[Sheet]
          ,[Month]
          ,[Shipper]
          ,[NO]
          ,[VESSEL]
          ,[LAYCAN_From]
          ,[LAYCAN_To]
          ,[DESTINATION]
          ,[BUYER]
          ,[ETA]
          ,[ATA]
          ,[Commenced_Loading]
          ,[Completed_Loading]
          ,[PORT]
          ,[DEM]
          ,[STATUS]
          ,[CreatedBy]
          ,[CreatedOn]
          ,CreatedOn_Date_Only
        )
        select
          t.[RecordId]
          ,[Site]
          ,[Sheet]
          ,[Month]
          ,[Shipper]
          ,[NO]
          ,[VESSEL]
          , CONVERT(datetime, Laycan_From)
          , CONVERT(datetime, Laycan_To)
          ,[DESTINATION]
          ,[BUYER]
          ,CONVERT(datetime, ETA)
          ,CONVERT(datetime, ATA)
          ,CONVERT(datetime, Commenced_Loading)
          ,CONVERT(datetime, Completed_Loading)
          ,[PORT]
          ,dbo.To_Decimal_0(t.DEM, -1.0)
          ,[STATUS]
          ,[CreatedBy]
          ,@vNow
          ,CreatedOn_Date_Only
        from TEMPORARY_VLU t
        where 
            t.File_Physical = @p_file
            and t.[Status_Upload] = 'New'

        -- for Status: Edit
        -- use "SQL Server UPDATE JOIN", https://www.sqlservertutorial.net/sql-server-basics/sql-server-update-join/

        UPDATE 
            UPLOAD_VLU
        SET 
            UPLOAD_VLU.[TEMPORARY] = t2.[RecordId]
            
            -- * Not Created
            --,UPLOAD_VLU.CreatedBy = t2.CreatedBy
            --,UPLOAD_VLU.CreatedOn = t2.CreatedOn
            --,UPLOAD_VLU.CreatedOn_Date_Only = t2.CreatedOn_Date_Only
            
            -- * but Modified
            ,UPLOAD_VLU.LastModifiedBy = t2.CreatedBy
            ,UPLOAD_VLU.LastModifiedOn = @vNow
            
            -- Handle others columns
            ,UPLOAD_VLU.Site = t2.Site
            ,UPLOAD_VLU.Sheet = t2.Sheet
            ,UPLOAD_VLU.Month = t2.Month
            ,UPLOAD_VLU.Shipper = t2.Shipper
            ,UPLOAD_VLU.NO = t2.NO
            ,UPLOAD_VLU.VESSEL = t2.VESSEL
            ,UPLOAD_VLU.LAYCAN_From = CONVERT(datetime, t2.LAYCAN_From)
            ,UPLOAD_VLU.LAYCAN_To = CONVERT(datetime, t2.LAYCAN_To)
            ,UPLOAD_VLU.DESTINATION = t2.DESTINATION
            ,UPLOAD_VLU.BUYER = t2.BUYER
            ,UPLOAD_VLU.ETA = t2.ETA
            ,UPLOAD_VLU.ATA = t2.ATA
            ,UPLOAD_VLU.Commenced_Loading = CONVERT(datetime, t2.Commenced_Loading)
            ,UPLOAD_VLU.Completed_Loading = CONVERT(datetime, t2.Completed_Loading)
            ,UPLOAD_VLU.PORT = t2.PORT
            ,UPLOAD_VLU.DEM = dbo.To_Decimal_0(t2.DEM, -1.0)
            ,UPLOAD_VLU.STATUS = t2.STATUS
        FROM 
            UPLOAD_VLU t1
                INNER JOIN TEMPORARY_VLU t2 ON
                    t1.CreatedOn_Date_Only = t2.CreatedOn_Date_Only
                    and t1.VESSEL = t2.VESSEL
                    and t1.LAYCAN_From = t2.LAYCAN_From
                    and t1.LAYCAN_To = t2.LAYCAN_To
        WHERE
            t2.File_Physical = @p_file
            and t2.[Status_Upload] = 'Update'

        select @vCount = Count(RecordId)
        from UPLOAD_VLU
        where (CreatedOn = @vNow) or (LastModifiedOn = @vNow)

        select @vCount2 = Count(t2.RecordId)
        FROM  TEMPORARY_VLU t2
        WHERE t2.File_Physical = @p_file

        select @vStatus = 'Ok'
        select @vMessage = 'UPLOAD_ACTUAL_Barge_LineUp_VLU'
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
