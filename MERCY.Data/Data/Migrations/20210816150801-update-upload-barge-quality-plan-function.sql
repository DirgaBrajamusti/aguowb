/* Up script. */
ALTER procedure [dbo].[UPLOAD_ACTUAL_Barge_Quality_Plan] @p_file integer
as
begin
    declare @vCount integer = 0
    declare @vCount2 integer = 0
    declare @vStatus varchar(32) = ''
    declare @vMessage varchar(8000) = ''
    
    declare @vSite int = 0
    
    declare @vNow datetime = GetDate()
    
    exec UPLOAD_ACTUAL_Barge_Quality_Plan_Header @p_file
    
    -- ambil Nilai: Site
    select top 1 @vSite = Site
    from UPLOAD_Barge_Quality_Plan_Header
    order by RecordId desc
    
    BEGIN TRY  
        -- for Status: New
        insert into UPLOAD_Barge_Quality_Plan 
        (
            [TEMPORARY]
            , [Header]
            , Site
            , [Sheet]
            , [CreatedBy]
            , [CreatedOn]
            , CreatedOn_Date_Only
            , Barge_Ton_plan
            , Barge_Ton_actual
            , TM_plan
            , TM_actual
            , M_plan
            , M_actual
            , ASH_plan
            , ASH_actual
            , TS_plan
            , TS_actual
            , CV_ADB_plan
            , CV_ADB_actual
            , CV_AR_plan
            , CV_AR_actual
            , Product
            , TugName
            , BargeName
        )
        select
            t.[RecordId]
            , t.Header
            , @vSite
            , h.Sheet
            , t.[CreatedBy]
            , @vNow
            , t.CreatedOn_Date_Only
            , dbo.To_Decimal(t.Barge_Ton_plan, -1.0)
            , dbo.To_Decimal(t.Barge_Ton_actual, -1.0)
            , dbo.To_Decimal(t.TM_plan, -1.0)
            , dbo.To_Decimal(t.TM_actual, -1.0)
            , dbo.To_Decimal(t.M_plan, -1.0)
            , dbo.To_Decimal(t.M_actual, -1.0)
            , dbo.To_Decimal(t.ASH_plan, -1.0)
            , dbo.To_Decimal(t.ASH_actual, -1.0)
            , dbo.To_Decimal(t.TS_plan, -1.0)
            , dbo.To_Decimal(t.TS_actual, -1.0)
            , dbo.To_Decimal_0(t.CV_ADB_plan, -1.0)
            , dbo.To_Decimal_0(t.CV_ADB_actual, -1.0)
            , dbo.To_Decimal_0(t.CV_AR_plan, -1.0)
            , dbo.To_Decimal_0(t.CV_AR_actual, -1.0)
            , t.Product
            , t.TugName
            , t.BargeName
        from TEMPORARY_Barge_Quality_Plan t inner join TEMPORARY_Barge_Quality_Plan_Header h on t.Header = h.RecordId
        where 
            h.File_Physical = @p_file
            and t.[Status] = 'New'
            and t.[TugName] + '-' + t.[BargeName] not in (select [TugName] + '-' + [BargeName] from UPLOAD_Barge_Quality_Plan where [TugName] <> 'TBN' AND [BargeName] <> 'TBN' )

        -- for Status: Edit
        -- use "SQL Server UPDATE JOIN", https://www.sqlservertutorial.net/sql-server-basics/sql-server-update-join/

        UPDATE 
            UPLOAD_Barge_Quality_Plan
        SET 
            UPLOAD_Barge_Quality_Plan.[TEMPORARY] = t2.[RecordId]
            --,UPLOAD_Barge_Quality_Plan.[Header] = @header_id
            -- * Not Created
            --,UPLOAD_Barge_Quality_Plan.[CreatedBy] = t2.[CreatedBy]
            --,UPLOAD_Barge_Quality_Plan.[CreatedOn] = t2.[CreatedOn]
            --,UPLOAD_Barge_Quality_Plan.[CreatedOn_Date_Only] = t2.[CreatedOn_Date_Only]
            -- * but Modified
            , UPLOAD_Barge_Quality_Plan.Sheet = h.Sheet 
            , UPLOAD_Barge_Quality_Plan.[LastModifiedBy] = t2.[CreatedBy]
            , UPLOAD_Barge_Quality_Plan.[LastModifiedOn] = @vNow
            , UPLOAD_Barge_Quality_Plan.Barge_Ton_plan = dbo.To_Decimal(t2.Barge_Ton_plan, -1.0)
            , UPLOAD_Barge_Quality_Plan.Barge_Ton_actual = dbo.To_Decimal(t2.Barge_Ton_actual, -1.0)
            , UPLOAD_Barge_Quality_Plan.TM_plan = dbo.To_Decimal(t2.TM_plan, -1.0)
            , UPLOAD_Barge_Quality_Plan.TM_actual = dbo.To_Decimal(t2.TM_actual, -1.0)
            , UPLOAD_Barge_Quality_Plan.M_plan = dbo.To_Decimal(t2.M_plan, -1.0)
            , UPLOAD_Barge_Quality_Plan.M_actual = dbo.To_Decimal(t2.M_actual, -1.0)
            , UPLOAD_Barge_Quality_Plan.ASH_plan = dbo.To_Decimal(t2.ASH_plan, -1.0)
            , UPLOAD_Barge_Quality_Plan.ASH_actual = dbo.To_Decimal(t2.ASH_actual, -1.0)
            , UPLOAD_Barge_Quality_Plan.TS_plan = dbo.To_Decimal(t2.TS_plan, -1.0)
            , UPLOAD_Barge_Quality_Plan.TS_actual = dbo.To_Decimal(t2.TS_actual, -1.0)
            , UPLOAD_Barge_Quality_Plan.CV_ADB_plan = dbo.To_Decimal(t2.CV_ADB_plan, -1.0)
            , UPLOAD_Barge_Quality_Plan.CV_ADB_actual = dbo.To_Decimal(t2.CV_ADB_actual, -1.0)
            , UPLOAD_Barge_Quality_Plan.CV_AR_plan = dbo.To_Decimal(t2.CV_AR_plan, -1.0)
            , UPLOAD_Barge_Quality_Plan.CV_AR_actual = dbo.To_Decimal(t2.CV_AR_actual, -1.0)
            , UPLOAD_Barge_Quality_Plan.Product = t2.Product
            , UPLOAD_Barge_Quality_Plan.TugName = t2.TugName
            , UPLOAD_Barge_Quality_Plan.BargeName = t2.BargeName
        FROM 
            UPLOAD_Barge_Quality_Plan t1
                INNER JOIN TEMPORARY_Barge_Quality_Plan t2 ON 
                        t1.CreatedOn_Date_Only = t2.CreatedOn_Date_Only
                        and t1.TugName = t2.TugName
                        and t1.BargeName = t2.BargeName
                INNER JOIN TEMPORARY_Barge_Quality_Plan_Header h on t2.Header = h.RecordId
        WHERE
            h.File_Physical = @p_file
            and t2.[Status] = 'Update'

        select @vCount = Count(RecordId)
        from UPLOAD_Barge_Quality_Plan
        where (CreatedOn = @vNow) or (LastModifiedOn = @vNow)

        select @vCount2 = Count(t2.RecordId)
        FROM  TEMPORARY_Barge_Quality_Plan t2
            INNER JOIN TEMPORARY_Barge_Quality_Plan_Header h on t2.Header = h.RecordId
        WHERE h.File_Physical = @p_file

        select @vStatus = 'Ok'
        select @vMessage = 'UPLOAD_ACTUAL_Barge_Quality_Plan'
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

ALTER procedure [dbo].[UPLOAD_ACTUAL_Barge_Quality_Plan_Header] @p_file integer
as
begin
    DECLARE @vNow datetime = GetDate();
    DECLARE @isExist INT = 0;
	DECLARE @filePhysical BIGINT = 0;
	DECLARE @recordId BIGINT = 0;
	DECLARE @site INT = 0;
	DECLARE @sheet varchar(255) = '';
	DECLARE @tripNo varchar(255) = '';
	DECLARE @vessel varchar(255) = '';
	DECLARE @buyer varchar(255) = '';
	DECLARE @laycanFrom datetime = GetDate();
	DECLARE @laycanTo datetime = GetDate();
	DECLARE @createdBy INT = 0;
	DECLARE @createdOnDateOnly varchar(10) = '';
    
    BEGIN TRY
    	SELECT @isExist = COUNT(upload.RecordId)
	    from TEMPORARY_Barge_Quality_Plan_Header temp
	    join UPLOAD_Barge_Quality_Plan_Header upload on temp.TripNo = upload.TripNo AND temp.CreatedOn_Date_Only = upload.CreatedOn_Date_Only 
	    where upload.File_Physical = @p_file;
       
	   	IF @isExist = 0
	    BEGIN
	        insert into UPLOAD_Barge_Quality_Plan_Header
	        (
	            File_Physical
	            , [TEMPORARY]
	            , Site
	            , Sheet
	            , TripNo
	            , Vessel
	            , Buyer
	            , Laycan_From
	            , Laycan_To
	            , CreatedBy
	            , CreatedOn
	            , CreatedOn_Date_Only
	        )
	        select
	            File_Physical
	            , RecordId
	            , Site
	            , Sheet
	            , TripNo
	            , Vessel
	            , Buyer
	            , CONVERT(datetime, Laycan_From)
	            , CONVERT(datetime, Laycan_To)
	            , CreatedBy
	            , @vNow
	            , CreatedOn_Date_Only
	        from TEMPORARY_Barge_Quality_Plan_Header
	        where 
	            File_Physical = @p_file
	    END
	    ELSE
		BEGIN
    	
    		select
	            @filePhysical = File_Physical,
	            @recordId = RecordId,
	            @site = Site,
				@sheet = Sheet,
	            @tripNo = TripNo,
	            @vessel = Vessel,
	            @buyer = Buyer,
	            @laycanFrom = CONVERT(datetime, Laycan_From),
	            @laycanTo = CONVERT(datetime, Laycan_To),
	            @createdBy = CreatedBy,
	            @createdOnDateOnly = CreatedOn_Date_Only
	        from TEMPORARY_Barge_Quality_Plan_Header
	        where File_Physical = @p_file

		    UPDATE UPLOAD_Barge_Quality_Plan_Header
		    SET 
		    	File_Physical = @filePhysical,
		    	[TEMPORARY] = @recordId,
	            Site = @site,
	            Sheet = @sheet,
	            TripNo = @tripNo,
	            Vessel = @vessel,
	            Buyer = @buyer,
	            Laycan_From = @laycanFrom,
	            Laycan_To = @laycanTo,
	            CreatedBy = @createdBy,
	            CreatedOn = @vNow,
	            CreatedOn_Date_Only = @createdOnDateOnly
		    	
		END
    END TRY  
    BEGIN CATCH
    END CATCH
end


/* Down script. */
ALTER procedure [dbo].[UPLOAD_ACTUAL_Barge_Quality_Plan] @p_file integer
as
begin
    declare @vCount integer = 0
    declare @vCount2 integer = 0
    declare @vStatus varchar(32) = ''
    declare @vMessage varchar(8000) = ''
    
    declare @vSite int = 0
    
    declare @vNow datetime = GetDate()
    
    exec UPLOAD_ACTUAL_Barge_Quality_Plan_Header @p_file
    
    -- ambil Nilai: Site
    select top 1 @vSite = Site
    from UPLOAD_Barge_Quality_Plan_Header
    order by RecordId desc
    
    BEGIN TRY  
        -- for Status: New
        insert into UPLOAD_Barge_Quality_Plan 
        (
            [TEMPORARY]
            , [Header]
            , Site
            , [Sheet]
            , [CreatedBy]
            , [CreatedOn]
            , CreatedOn_Date_Only
            , Barge_Ton_plan
            , Barge_Ton_actual
            , TM_plan
            , TM_actual
            , M_plan
            , M_actual
            , ASH_plan
            , ASH_actual
            , TS_plan
            , TS_actual
            , CV_ADB_plan
            , CV_ADB_actual
            , CV_AR_plan
            , CV_AR_actual
            , Product
            , TugName
            , BargeName
        )
        select
            t.[RecordId]
            , t.Header
            , @vSite
            , h.Sheet
            , t.[CreatedBy]
            , @vNow
            , t.CreatedOn_Date_Only
            , dbo.To_Decimal(t.Barge_Ton_plan, -1.0)
            , dbo.To_Decimal(t.Barge_Ton_actual, -1.0)
            , dbo.To_Decimal(t.TM_plan, -1.0)
            , dbo.To_Decimal(t.TM_actual, -1.0)
            , dbo.To_Decimal(t.M_plan, -1.0)
            , dbo.To_Decimal(t.M_actual, -1.0)
            , dbo.To_Decimal(t.ASH_plan, -1.0)
            , dbo.To_Decimal(t.ASH_actual, -1.0)
            , dbo.To_Decimal(t.TS_plan, -1.0)
            , dbo.To_Decimal(t.TS_actual, -1.0)
            , dbo.To_Decimal_0(t.CV_ADB_plan, -1.0)
            , dbo.To_Decimal_0(t.CV_ADB_actual, -1.0)
            , dbo.To_Decimal_0(t.CV_AR_plan, -1.0)
            , dbo.To_Decimal_0(t.CV_AR_actual, -1.0)
            , t.Product
            , t.TugName
            , t.BargeName
        from TEMPORARY_Barge_Quality_Plan t inner join TEMPORARY_Barge_Quality_Plan_Header h on t.Header = h.RecordId
        where 
            h.File_Physical = @p_file
            and t.[Status] = 'New'
            and t.[TugName] + '-' + t.[BargeName] not in (select [TugName] + '-' + [BargeName] from UPLOAD_Barge_Quality_Plan)

        -- for Status: Edit
        -- use "SQL Server UPDATE JOIN", https://www.sqlservertutorial.net/sql-server-basics/sql-server-update-join/

        UPDATE 
            UPLOAD_Barge_Quality_Plan
        SET 
            UPLOAD_Barge_Quality_Plan.[TEMPORARY] = t2.[RecordId]
            --,UPLOAD_Barge_Quality_Plan.[Header] = @header_id
            -- * Not Created
            --,UPLOAD_Barge_Quality_Plan.[CreatedBy] = t2.[CreatedBy]
            --,UPLOAD_Barge_Quality_Plan.[CreatedOn] = t2.[CreatedOn]
            --,UPLOAD_Barge_Quality_Plan.[CreatedOn_Date_Only] = t2.[CreatedOn_Date_Only]
            -- * but Modified
            , UPLOAD_Barge_Quality_Plan.[LastModifiedBy] = t2.[CreatedBy]
            , UPLOAD_Barge_Quality_Plan.[LastModifiedOn] = @vNow
            , UPLOAD_Barge_Quality_Plan.Barge_Ton_plan = dbo.To_Decimal(t2.Barge_Ton_plan, -1.0)
            , UPLOAD_Barge_Quality_Plan.Barge_Ton_actual = dbo.To_Decimal(t2.Barge_Ton_actual, -1.0)
            , UPLOAD_Barge_Quality_Plan.TM_plan = dbo.To_Decimal(t2.TM_plan, -1.0)
            , UPLOAD_Barge_Quality_Plan.TM_actual = dbo.To_Decimal(t2.TM_actual, -1.0)
            , UPLOAD_Barge_Quality_Plan.M_plan = dbo.To_Decimal(t2.M_plan, -1.0)
            , UPLOAD_Barge_Quality_Plan.M_actual = dbo.To_Decimal(t2.M_actual, -1.0)
            , UPLOAD_Barge_Quality_Plan.ASH_plan = dbo.To_Decimal(t2.ASH_plan, -1.0)
            , UPLOAD_Barge_Quality_Plan.ASH_actual = dbo.To_Decimal(t2.ASH_actual, -1.0)
            , UPLOAD_Barge_Quality_Plan.TS_plan = dbo.To_Decimal(t2.TS_plan, -1.0)
            , UPLOAD_Barge_Quality_Plan.TS_actual = dbo.To_Decimal(t2.TS_actual, -1.0)
            , UPLOAD_Barge_Quality_Plan.CV_ADB_plan = dbo.To_Decimal(t2.CV_ADB_plan, -1.0)
            , UPLOAD_Barge_Quality_Plan.CV_ADB_actual = dbo.To_Decimal(t2.CV_ADB_actual, -1.0)
            , UPLOAD_Barge_Quality_Plan.CV_AR_plan = dbo.To_Decimal(t2.CV_AR_plan, -1.0)
            , UPLOAD_Barge_Quality_Plan.CV_AR_actual = dbo.To_Decimal(t2.CV_AR_actual, -1.0)
            , UPLOAD_Barge_Quality_Plan.Product = t2.Product
            , UPLOAD_Barge_Quality_Plan.TugName = t2.TugName
            , UPLOAD_Barge_Quality_Plan.BargeName = t2.BargeName
        FROM 
            UPLOAD_Barge_Quality_Plan t1
                INNER JOIN TEMPORARY_Barge_Quality_Plan t2 ON 
                        t1.CreatedOn_Date_Only = t2.CreatedOn_Date_Only
                        and t1.TugName = t2.TugName
                        and t1.BargeName = t2.BargeName
                INNER JOIN TEMPORARY_Barge_Quality_Plan_Header h on t2.Header = h.RecordId
        WHERE
            h.File_Physical = @p_file
            and t2.[Status] = 'Update'

        select @vCount = Count(RecordId)
        from UPLOAD_Barge_Quality_Plan
        where (CreatedOn = @vNow) or (LastModifiedOn = @vNow)

        select @vCount2 = Count(t2.RecordId)
        FROM  TEMPORARY_Barge_Quality_Plan t2
            INNER JOIN TEMPORARY_Barge_Quality_Plan_Header h on t2.Header = h.RecordId
        WHERE h.File_Physical = @p_file

        select @vStatus = 'Ok'
        select @vMessage = 'UPLOAD_ACTUAL_Barge_Quality_Plan'
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

ALTER procedure [dbo].[UPLOAD_ACTUAL_Barge_Quality_Plan_Header] @p_file integer
as
begin
    declare @vNow datetime = GetDate()
    
    BEGIN TRY
        insert into UPLOAD_Barge_Quality_Plan_Header
        (
            File_Physical
            , [TEMPORARY]
            , Site
            , Sheet
            , TripNo
            , Vessel
            , Buyer
            , Laycan_From
            , Laycan_To
            , CreatedBy
            , CreatedOn
            , CreatedOn_Date_Only
        )
        select
            File_Physical
            , RecordId
            , Site
            , Sheet
            , TripNo
            , Vessel
            , Buyer
            , CONVERT(datetime, Laycan_From)
            , CONVERT(datetime, Laycan_To)
            , CreatedBy
            , @vNow
            , CreatedOn_Date_Only
        from TEMPORARY_Barge_Quality_Plan_Header
        where 
            File_Physical = @p_file
    END TRY  
    BEGIN CATCH
    END CATCH
end
