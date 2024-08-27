create procedure [dbo].[PROCESS_Reminder_Lab_Consumable] @p_now varchar(16) = '', @p_is_Save_History int = 0
as
begin
	declare @vStatus varchar(32) = ''
	declare @vMessage varchar(8000) = ''
    declare @periode_1_str varchar(8000) = ''
    declare @periode_1 int = 0
    declare @periode_2_str varchar(8000) = ''
    declare @periode_2 int = 0
    
    declare @v_RecordId BIGINT
    declare @v_PartName varchar(255)
    declare @v_PartNumber varchar(255)
    declare @v_MSDS_Code varchar(255)
    declare @v_CurrentQuantity int
    declare @v_MinimumQuantity int
    declare @v_InstrumentName varchar(255)
    declare @v_UnitName varchar(255)
        
    DECLARE @table_result TABLE (
        Period int
        , RecordId BIGINT
        , PartName varchar(255)
        , PartNumber varchar(255)
        , MSDS_Code varchar(255)
        , CurrentQuantity int
        , MinimumQuantity int
        , InstrumentName varchar(255)
        , UnitName varchar(255)
    );
    
	BEGIN TRY
		declare @_now DateTime
        if (@p_now = '')
        begin
            select @_now = GetDate()
        end
        else
        begin
            select @_now = Convert(DateTime, @p_now)
        end
        
        SELECT TOP 1
            @periode_1_str = [Value]
        FROM [Config]
        where Name = 'REMINDER_CONSUMABLE_1'
        
        select @periode_1 = Convert(int, @periode_1_str)
        
        DECLARE data_1 CURSOR FOR
        select
            c.RecordId
            , c.PartName
            , c.PartNumber
            , c.MSDS_Code
            , c.CurrentQuantity
            , c.MinimumQuantity
            , i.InstrumentName
            , u.UnitName
        FROM Consumable c
            , Instrument i
            , UnitMeasurement u
        where
            c.CurrentQuantity <= (0.5 * c.MinimumQuantity)
            and c.Instrument = i.RecordId
            and c.UnitType = u.RecordId
            and c.RecordId not in
            (
                select Consumable_Data
                from Reminder_History_Lab_Consumable
                where
                    (Period = @periode_1
                        and DATEADD(day, @periode_1, CreatedOn) >= @_now)
                    or
                    (CAST(CreatedOn as date) = CAST(@_now as date))
            )
        
        OPEN data_1  
  
        FETCH NEXT FROM data_1   
        INTO @v_RecordId, @v_PartName, @v_PartNumber, @v_MSDS_Code
                , @v_CurrentQuantity, @v_MinimumQuantity, @v_InstrumentName, @v_UnitName
          
        WHILE @@FETCH_STATUS = 0  
        BEGIN
            insert into @table_result(
                Period
                , RecordId
                , PartName
                , PartNumber
                , MSDS_Code
                , CurrentQuantity
                , MinimumQuantity
                , InstrumentName
                , UnitName
            )
            values(
                @periode_1
                , @v_RecordId, @v_PartName, @v_PartNumber, @v_MSDS_Code
                , @v_CurrentQuantity, @v_MinimumQuantity, @v_InstrumentName, @v_UnitName
            )
            
            if (@p_is_Save_History = 1)
            begin
                insert into Reminder_History_Lab_Consumable(
                    CreatedOn
                    , Consumable_Data
                    , Period
                )
                values(@_now, @v_RecordId, @periode_1)
            end
            
            -- get next Record
            FETCH NEXT FROM data_1   
            INTO @v_RecordId, @v_PartName, @v_PartNumber, @v_MSDS_Code
                , @v_CurrentQuantity, @v_MinimumQuantity, @v_InstrumentName, @v_UnitName
        END   
        CLOSE data_1;  
        DEALLOCATE data_1;
        
        SELECT TOP 1
            @periode_2_str = [Value]
        FROM [Config]
        where Name = 'REMINDER_CONSUMABLE_2'
        
        select @periode_2 = Convert(int, @periode_2_str)
        
        DECLARE data_2 CURSOR FOR
        select
            c.RecordId
            , c.PartName
            , c.PartNumber
            , c.MSDS_Code
            , c.CurrentQuantity
            , c.MinimumQuantity
            , i.InstrumentName
            , u.UnitName
        FROM Consumable c
            , Instrument i
            , UnitMeasurement u
        where
            c.CurrentQuantity > (0.5 * c.MinimumQuantity)
            and c.CurrentQuantity <= (0.75 * c.MinimumQuantity)
            and c.Instrument = i.RecordId
            and c.UnitType = u.RecordId
            and c.RecordId not in
            (
                select Consumable_Data
                from Reminder_History_Lab_Consumable
                where
                    (Period = @periode_2
                        and DATEADD(day, @periode_2, CreatedOn) >= @_now)
                    or
                    (CAST(CreatedOn as date) = CAST(@_now as date))
            )
        
        OPEN data_2  
  
        FETCH NEXT FROM data_2   
        INTO @v_RecordId, @v_PartName, @v_PartNumber, @v_MSDS_Code
                , @v_CurrentQuantity, @v_MinimumQuantity, @v_InstrumentName, @v_UnitName
          
        WHILE @@FETCH_STATUS = 0  
        BEGIN
            if (not Exists(select 1 from @table_result where RecordId = @v_RecordId))
            begin
                insert into @table_result(
                    Period
                    , RecordId
                    , PartName
                    , PartNumber
                    , MSDS_Code
                    , CurrentQuantity
                    , MinimumQuantity
                    , InstrumentName
                    , UnitName
                )
                values(
                    @periode_2
                    , @v_RecordId, @v_PartName, @v_PartNumber, @v_MSDS_Code
                    , @v_CurrentQuantity, @v_MinimumQuantity, @v_InstrumentName, @v_UnitName
                )
            end
            
            if (@p_is_Save_History = 1)
            begin
                insert into Reminder_History_Lab_Consumable(
                    CreatedOn
                    , Consumable_Data
                    , Period
                )
                values(@_now, @v_RecordId, @periode_2)
            end
            
            -- get next Record
            FETCH NEXT FROM data_2   
            INTO @v_RecordId, @v_PartName, @v_PartNumber, @v_MSDS_Code
                , @v_CurrentQuantity, @v_MinimumQuantity, @v_InstrumentName, @v_UnitName
        END   
        CLOSE data_2;  
        DEALLOCATE data_2;
        
        select @vStatus = 'Ok'
	END TRY 
	BEGIN CATCH  
		 select @vStatus = 'Error'
		 select @vMessage = ERROR_MESSAGE()
	END CATCH  

	-- tampilkan "Hasil Eksekusi"
	select @vStatus as Status
        , @vMessage as Message
        , Period
        , RecordId
        , InstrumentName
        , PartName
        , PartNumber
        , MSDS_Code
        , CurrentQuantity
        , MinimumQuantity
        , UnitName
    from @table_result
    order by Period, InstrumentName, PartName, PartNumber, MSDS_Code
end