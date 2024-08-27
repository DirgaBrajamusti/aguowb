IF OBJECT_ID('PROCESS_Reminder_Lab_Maintenance') IS NULL
    EXEC('CREATE PROCEDURE PROCESS_Reminder_Lab_Maintenance AS SET NOCOUNT ON;')
GO

alter procedure [dbo].[PROCESS_Reminder_Lab_Maintenance] @p_now varchar(16) = '', @p_is_Save_History int = 0
as
begin
    declare @vStatus varchar(32) = ''
    declare @vMessage varchar(8000) = ''
    declare @periode_1_str varchar(8000) = ''
    declare @periode_1 int = 0
    declare @periode_2_str varchar(8000) = ''
    declare @periode_2 int = 0
    
    declare @v_RecordId BIGINT
    declare @v_InstrumentCode varchar(255)
    declare @v_Description varchar(255)
    declare @v_NextSchedule varchar(255)
    declare @v_InstrumentName varchar(255)
    declare @v_Category varchar(255)
        
    DECLARE @table_result TABLE (
        Period int
        , RecordId BIGINT
        , InstrumentCode varchar(255)
        , Description varchar(255)
        , NextSchedule varchar(255)
        , InstrumentName varchar(255)
        , Category varchar(255)
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
        where Name = 'REMINDER_MAINTENANCE_1'
        
        select @periode_1 = Convert(int, @periode_1_str)
        
        DECLARE data_1 CURSOR FOR
        select
            m.RecordId
            , m.InstrumentCode
            , m.Description
            , Convert(varchar(16), m.NextSchedule, 120) as NextSchedule
            , i.InstrumentName
            , a.Category
        FROM LabMaintenance m
            , Instrument i
            , MaintenanceActivity a
        where
            m.IsActive = 1
            and DATEADD(day, (-1 * @periode_1), m.NextSchedule) <= @_now
            and m.Category = a.RecordId
            and m.Instrument = i.RecordId
            and m.RecordId not in
            (
                select Maintenance_Data
                from Reminder_History_Lab_Maintenance
                where 
                    (Period = @periode_1
                        and DATEADD(day, @periode_1, CreatedOn) >= @_now)
                    or
                    (CAST(CreatedOn as date) = CAST(@_now as date))
            )
            and m.RepeatCount > dbo.Lab_Maintenance_Complete_Count(m.RecordId)
        
        OPEN data_1  
  
        FETCH NEXT FROM data_1   
        INTO @v_RecordId, @v_InstrumentCode, @v_Description, @v_NextSchedule
                , @v_InstrumentName, @v_Category
          
        WHILE @@FETCH_STATUS = 0  
        BEGIN
            insert into @table_result(
                Period
                , RecordId
                , InstrumentCode, Description, NextSchedule
                , InstrumentName, Category
            )
            values(
                @periode_1
                , @v_RecordId, @v_InstrumentCode, @v_Description, @v_NextSchedule
                , @v_InstrumentName, @v_Category
            )
            
            if (@p_is_Save_History = 1)
            begin
                insert into Reminder_History_Lab_Maintenance(
                    CreatedOn
                    , Maintenance_Data
                    , Period
                )
                values(@_now, @v_RecordId, @periode_1)
            end
            
            -- get next Record
            FETCH NEXT FROM data_1   
            INTO @v_RecordId, @v_InstrumentCode, @v_Description, @v_NextSchedule
                , @v_InstrumentName, @v_Category
        END   
        CLOSE data_1;  
        DEALLOCATE data_1;
        
        SELECT TOP 1
            @periode_2_str = [Value]
        FROM [Config]
        where Name = 'REMINDER_MAINTENANCE_2'
        
        select @periode_2 = Convert(int, @periode_2_str)
        
        DECLARE data_2 CURSOR FOR
        select
            m.RecordId
            , m.InstrumentCode
            , m.Description
            , Convert(varchar(16), m.NextSchedule, 120) as NextSchedule
            , i.InstrumentName
            , a.Category
        FROM LabMaintenance m
            , Instrument i
            , MaintenanceActivity a
        where
            m.IsActive = 1
            and DATEADD(day, (-1 * @periode_2), m.NextSchedule) <= @_now
            and m.Category = a.RecordId
            and m.Instrument = i.RecordId
            and m.RecordId not in
            (
                select Maintenance_Data
                from Reminder_History_Lab_Maintenance
                where 
                    (Period = @periode_2
                        and DATEADD(day, @periode_2, CreatedOn) >= @_now)
                    or
                    (CAST(CreatedOn as date) = CAST(@_now as date))
            )
            and m.RepeatCount > dbo.Lab_Maintenance_Complete_Count(m.RecordId)
        
        OPEN data_2  
  
        FETCH NEXT FROM data_2   
        INTO @v_RecordId, @v_InstrumentCode, @v_Description, @v_NextSchedule
                , @v_InstrumentName, @v_Category
          
        WHILE @@FETCH_STATUS = 0  
        BEGIN
            if (not Exists(select 1 from @table_result where RecordId = @v_RecordId))
            begin
                insert into @table_result(
                    Period
                    , RecordId
                    , InstrumentCode, Description, NextSchedule
                    , InstrumentName, Category
                )
                values(
                    @periode_2
                    , @v_RecordId, @v_InstrumentCode, @v_Description, @v_NextSchedule
                    , @v_InstrumentName, @v_Category
                )
            end
            
            if (@p_is_Save_History = 1)
            begin
                insert into Reminder_History_Lab_Maintenance(
                    CreatedOn
                    , Maintenance_Data
                    , Period
                )
                values(@_now, @v_RecordId, @periode_2)
            end
            
            -- get next Record
            FETCH NEXT FROM data_2   
            INTO @v_RecordId, @v_InstrumentCode, @v_Description, @v_NextSchedule
                , @v_InstrumentName, @v_Category
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
        , Category
        , InstrumentName
        , InstrumentCode
        , Description
        , NextSchedule
    from @table_result
    order by Period, Category, InstrumentName, InstrumentCode
end