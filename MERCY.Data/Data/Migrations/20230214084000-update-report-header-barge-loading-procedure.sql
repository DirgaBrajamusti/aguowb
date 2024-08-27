﻿/* Up Script*/

ALTER procedure [dbo].[Report_BARGE_LOADING_Header] (@p_company varchar(32), @p_sheet varchar(32), @p_date varchar(10), @p_dateTo varchar(10))
as
begin
    declare @dateFrom varchar(10)
    declare @dateTo  varchar(10)
    
    if (@p_date <= @p_dateTo)
    begin
        select @dateFrom = @p_date
        select @dateTo = @p_dateTo
    end
    else
    begin
        select @dateFrom = @p_dateTo
        select @dateTo = @p_date
    end
    
    SELECT top 1
        Date_Detail as DATE_1
        , '' as DATE_2
        , Job_No
        , Report_To
        , Method1
        , Method2
        , Method3
        , Method4
    from UPLOAD_BARGE_LOADING_Header
    where Company = @p_company
        and (Sheet LIKE '%'+ @p_sheet + '%' OR @p_sheet = 'all')
        and CONVERT(varchar(10),CreatedOn,120) >= @dateFrom
        and CONVERT(varchar(10),CreatedOn,120) <= @dateTo
    order by RecordId desc
END;

/* Down Script*/

ALTER procedure [dbo].[Report_BARGE_LOADING_Header] @p_company varchar(32), @p_sheet varchar(32), @p_date varchar(10), @p_dateTo varchar(10)
as
begin
    declare @dateFrom varchar(10)
    declare @dateTo  varchar(10)
    
    if (@p_date <= @p_dateTo)
    begin
        select @dateFrom = @p_date
        select @dateTo = @p_dateTo
    end
    else
    begin
        select @dateFrom = @p_dateTo
        select @dateTo = @p_date
    end
    
    SELECT top 1
        Date_Detail as DATE_1
        , '' as DATE_2
        , Job_No
        , Report_To
        , Method1
        , Method2
        , Method3
        , Method4
    from UPLOAD_BARGE_LOADING_Header
    where Company = @p_company
        and (Sheet = @p_sheet OR @p_sheet = 'all')
        and CONVERT(varchar(10),CreatedOn,120) >= @dateFrom
        and CONVERT(varchar(10),CreatedOn,120) <= @dateTo
    order by RecordId desc
end;
