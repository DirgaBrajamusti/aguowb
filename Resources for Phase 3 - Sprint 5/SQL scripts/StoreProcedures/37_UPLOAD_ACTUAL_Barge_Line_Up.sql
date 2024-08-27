IF OBJECT_ID('UPLOAD_ACTUAL_Barge_Line_Up') IS NULL
    EXEC('CREATE PROCEDURE UPLOAD_ACTUAL_Barge_Line_Up AS SET NOCOUNT ON;')
GO

alter procedure [dbo].[UPLOAD_ACTUAL_Barge_Line_Up] @p_file integer
as
begin
    declare @vCount integer = 0
    declare @vCount2 integer = 0
    declare @vStatus varchar(32) = ''
    declare @vMessage varchar(8000) = ''
    
    declare @vNow datetime = GetDate()
    
    BEGIN TRY  
        -- for Status: New
        insert into UPLOAD_Barge_Line_Up 
        (
            [TEMPORARY]
            , [CreatedBy]
		    , [CreatedOn]
            , CreatedOn_Date_Only
            , Site
            , Sheet
            , EstimateStartLoading
            , Port_of_Loading
            , VesselName
            , TripID
            , TugBoat
            , Barge
            , Product
            , Capacity
            , FinalDestinantion
            , Remark
        )
        select
            t.[RecordId]
            , t.[CreatedBy]
            , @vNow
            , CreatedOn_Date_Only
            , Site
            , Sheet
            , CONVERT(datetime, EstimateStartLoading)
            , Port_of_Loading
            , VesselName
            , TripID
            , TugBoat
            , Barge
            , Product
            , dbo.To_Integer(t.Capacity, -1.0)
            , FinalDestinantion
            , Remark
        from TEMPORARY_Barge_Line_Up t
        where 
            File_Physical = @p_file
            and [Status] = 'New'
            and t.[TripID] not in (select [TripID] from UPLOAD_Barge_Line_Up)

        -- for Status: Edit
        -- use "SQL Server UPDATE JOIN", https://www.sqlservertutorial.net/sql-server-basics/sql-server-update-join/

        UPDATE 
            UPLOAD_Barge_Line_Up
        SET 
            UPLOAD_Barge_Line_Up.[TEMPORARY] = t2.[RecordId]
            -- * Not Created
            --,UPLOAD_Barge_Line_Up.[CreatedBy] = t2.[CreatedBy]
            --,UPLOAD_Barge_Line_Up.[CreatedOn] = t2.[CreatedOn]
            --,UPLOAD_Barge_Line_Up.[CreatedOn_Date_Only] = t2.[CreatedOn_Date_Only]
            
            -- * but Modified
            , UPLOAD_Barge_Line_Up.[LastModifiedBy] = t2.[CreatedBy]
            , UPLOAD_Barge_Line_Up.[LastModifiedOn] = @vNow
            , UPLOAD_Barge_Line_Up.Site = t2.Site
            , UPLOAD_Barge_Line_Up.Sheet = t2.Sheet
            , UPLOAD_Barge_Line_Up.EstimateStartLoading = CONVERT(datetime, t2.EstimateStartLoading)
            , UPLOAD_Barge_Line_Up.Port_of_Loading = t2.Port_of_Loading
            , UPLOAD_Barge_Line_Up.VesselName = t2.VesselName
            , UPLOAD_Barge_Line_Up.TripID = t2.TripID
            , UPLOAD_Barge_Line_Up.TugBoat = t2.TugBoat
            , UPLOAD_Barge_Line_Up.Barge = t2.Barge
            , UPLOAD_Barge_Line_Up.Product = t2.Product
            , UPLOAD_Barge_Line_Up.Capacity = dbo.To_Integer(t2.Capacity, -1.0)
            , UPLOAD_Barge_Line_Up.FinalDestinantion = t2.FinalDestinantion
            , UPLOAD_Barge_Line_Up.Remark = t2.Remark
        FROM 
            UPLOAD_Barge_Line_Up t1
                INNER JOIN TEMPORARY_Barge_Line_Up t2 ON 
                    t1.CreatedOn_Date_Only = t2.CreatedOn_Date_Only
                    and t1.TripID = t2.TripID
        WHERE
            t2.File_Physical = @p_file
            and t2.[Status] = 'Update'

        select @vCount = Count(RecordId)
        from UPLOAD_Barge_Line_Up
        where (CreatedOn = @vNow) or (LastModifiedOn = @vNow)
        
        select @vCount2 = Count(t2.RecordId)
		from TEMPORARY_Barge_Line_Up t2
        where t2.File_Physical = @p_file
        
        select @vStatus = 'Ok'
        select @vMessage = 'UPLOAD_ACTUAL_Barge_Line_Up'
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
