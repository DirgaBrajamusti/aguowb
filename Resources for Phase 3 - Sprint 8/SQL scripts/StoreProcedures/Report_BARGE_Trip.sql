if OBJECT_ID('Report_BARGE_Trip') IS NULL
    EXEC('CREATE PROCEDURE Report_BARGE_Trip AS SET NOCOUNT ON;')
GO

alter procedure [dbo].[Report_BARGE_Trip] @p_site int = 0, @p_company varchar(32) = '', @p_dateTime DateTime = ''
as
begin
    set NOCOUNT on;
    
    -- "Sanitation" for parameter
    set @p_dateTime = IsNull(@p_dateTime, GetDate())

    if @p_dateTime = cast('' as DateTime)
    begin
        set @p_dateTime = GetDate()
    end

    declare @Year int = Year(@p_dateTime)
    
    -- Delete "Temporary Table": if necessary
    begin try  
        if OBJECT_ID('tempdb..#T_abc') IS NOT NULL
        begin
            drop table #T_abc
        end
    end try  
    begin catch  
        -- handle exception
    end catch  
    
    -- 1. Buat "Template" berdasarkan data selama 1 Tahun
    select distinct (TugName + ' / ' + Barge_Name) as TB
        , @Year                      as [Year]
        , 0                          as Year_Count
        , Convert(decimal(10, 2), 0) as Year_Belt
        , Convert(decimal(10, 2), 0) as Year_Draft
        , 0                          as January_Count   -- 1
        , Convert(decimal(10, 2), 0) as January_Belt    -- 1
        , Convert(decimal(10, 2), 0) as January_Draft   -- 1
        , 0                          as February_Count  -- 2
        , Convert(decimal(10, 2), 0) as February_Belt   -- 2
        , Convert(decimal(10, 2), 0) as February_Draft  -- 2
        , 0                          as March_Count     -- 3
        , Convert(decimal(10, 2), 0) as March_Belt      -- 3
        , Convert(decimal(10, 2), 0) as March_Draft     -- 3
        , 0                          as April_Count     -- 4
        , Convert(decimal(10, 2), 0) as April_Belt      -- 4
        , Convert(decimal(10, 2), 0) as April_Draft     -- 4
        , 0                          as May_Count       -- 5
        , Convert(decimal(10, 2), 0) as May_Belt        -- 5
        , Convert(decimal(10, 2), 0) as May_Draft       -- 5
        , 0                          as June_Count      -- 6
        , Convert(decimal(10, 2), 0) as June_Belt       -- 6
        , Convert(decimal(10, 2), 0) as June_Draft      -- 6
        , 0                          as July_Count      -- 7
        , Convert(decimal(10, 2), 0) as July_Belt       -- 7
        , Convert(decimal(10, 2), 0) as July_Draft      -- 7
        , 0                          as August_Count    -- 8
        , Convert(decimal(10, 2), 0) as August_Belt     -- 8
        , Convert(decimal(10, 2), 0) as August_Draft    -- 8
        , 0                          as September_Count -- 9
        , Convert(decimal(10, 2), 0) as September_Belt  -- 9
        , Convert(decimal(10, 2), 0) as September_Draft -- 9
        , 0                          as October_Count   -- 10
        , Convert(decimal(10, 2), 0) as October_Belt    -- 10
        , Convert(decimal(10, 2), 0) as October_Draft   -- 10
        , 0                          as November_Count  -- 11
        , Convert(decimal(10, 2), 0) as November_Belt   -- 11
        , Convert(decimal(10, 2), 0) as November_Draft  -- 11
        , 0                          as December_Count  -- 12
        , Convert(decimal(10, 2), 0) as December_Belt   -- 12
        , Convert(decimal(10, 2), 0) as December_Draft  -- 12
    into #T_abc
    from Loading_Actual
    where Year(CreatedOn) = @Year
    
    -- 2. Cursor untuk "menghitung"
    declare @actual_Id bigint
    declare @tug_Barge varchar(255)
    declare @month_ int
    
    declare @sum_Belt decimal(10, 2)
    declare @sum_Draft decimal(10, 2)
    
    declare data_cursor CURSOR for   
        select RecordId, (TugName + ' / ' + Barge_Name), Month(CreatedOn)
        from Loading_Actual
        where Year(CreatedOn) = @Year
      
    open data_cursor  
      
    fetch next from data_cursor
    into @actual_Id, @tug_Barge, @month_
      
    while @@FETCH_STATUS = 0  
    begin
        -- 3. Logic melakukan "perhitungan"
        
        -- nilai Belt dan Draft
        
        -- reset
        set @sum_Belt = 0.00;
        set @sum_Draft = 0.00;
            
        select
            @sum_Belt = Sum(IsNull(Belt, 0))
            , @sum_Draft = Sum(IsNull(Draft, 0))
        from Loading_Actual_Cargo_Loaded
        where ActualId = @actual_Id
        
        set @sum_Belt = IsNull(@sum_Belt, 0.00)
        set @sum_Draft = IsNull(@sum_Draft, 0.00)
        
        -- nilai semuanya
        update #T_abc
        set
            -- nilai Count
            January_Count = January_Count       + (case @month_ when 1 then 1 else 0 end)
            , February_Count = February_Count   + (case @month_ when 2 then 1 else 0 end)
            , March_Count = March_Count         + (case @month_ when 3 then 1 else 0 end)
            , April_Count = April_Count         + (case @month_ when 4 then 1 else 0 end)
            , May_Count = May_Count             + (case @month_ when 5 then 1 else 0 end)
            , June_Count = June_Count           + (case @month_ when 6 then 1 else 0 end)
            , July_Count = July_Count           + (case @month_ when 7 then 1 else 0 end)
            , August_Count = August_Count       + (case @month_ when 8 then 1 else 0 end)
            , September_Count = September_Count + (case @month_ when 9 then 1 else 0 end)
            , October_Count = October_Count     + (case @month_ when 10 then 1 else 0 end)
            , November_Count = November_Count   + (case @month_ when 11 then 1 else 0 end)
            , December_Count = December_Count   + (case @month_ when 12 then 1 else 0 end)
            , Year_Count = Year_Count + 1
            
            -- nilai Belt
            , January_Belt = January_Belt       + (case @month_ when 1 then @sum_Belt else 0 end)
            , February_Belt = February_Belt     + (case @month_ when 2 then @sum_Belt else 0 end)
            , March_Belt = March_Belt           + (case @month_ when 3 then @sum_Belt else 0 end)
            , April_Belt = April_Belt           + (case @month_ when 4 then @sum_Belt else 0 end)
            , May_Belt = May_Belt               + (case @month_ when 5 then @sum_Belt else 0 end)
            , June_Belt = June_Belt             + (case @month_ when 6 then @sum_Belt else 0 end)
            , July_Belt = July_Belt             + (case @month_ when 7 then @sum_Belt else 0 end)
            , August_Belt = August_Belt         + (case @month_ when 8 then @sum_Belt else 0 end)
            , September_Belt = September_Belt   + (case @month_ when 9 then @sum_Belt else 0 end)
            , October_Belt = October_Belt       + (case @month_ when 10 then @sum_Belt else 0 end)
            , November_Belt = November_Belt     + (case @month_ when 11 then @sum_Belt else 0 end)
            , December_Belt = December_Belt     + (case @month_ when 12 then @sum_Belt else 0 end)
            , Year_Belt = Year_Belt + @sum_Belt
            
            -- nilai Draft
            , January_Draft = January_Draft     + (case @month_ when 1 then @sum_Draft else 0 end)
            , February_Draft = February_Draft   + (case @month_ when 2 then @sum_Draft else 0 end)
            , March_Draft = March_Draft         + (case @month_ when 3 then @sum_Draft else 0 end)
            , April_Draft = April_Draft         + (case @month_ when 4 then @sum_Draft else 0 end)
            , May_Draft = May_Draft             + (case @month_ when 5 then @sum_Draft else 0 end)
            , June_Draft = June_Draft           + (case @month_ when 6 then @sum_Draft else 0 end)
            , July_Draft = July_Draft           + (case @month_ when 7 then @sum_Draft else 0 end)
            , August_Draft = August_Draft       + (case @month_ when 8 then @sum_Draft else 0 end)
            , September_Draft = September_Draft + (case @month_ when 9 then @sum_Draft else 0 end)
            , October_Draft = October_Draft     + (case @month_ when 10 then @sum_Draft else 0 end)
            , November_Draft = November_Draft   + (case @month_ when 11 then @sum_Draft else 0 end)
            , December_Draft = December_Draft   + (case @month_ when 12 then @sum_Draft else 0 end)
            , Year_Draft = Year_Draft + @sum_Draft
            
        where TB = @tug_Barge
        
        -- record berikutnya
        fetch next from data_cursor
        into @actual_Id, @tug_Barge, @month_
    end   
    close data_cursor;  
    deallocate data_cursor;
    
    -- 4. Display data from "Temporary Table"
    select *
    from #T_abc
    order by TB
end
go