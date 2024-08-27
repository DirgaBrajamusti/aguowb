if OBJECT_ID('Report_Manual_Barging_and_Loading_Performance') IS NULL
    EXEC('CREATE PROCEDURE Report_Manual_Barging_and_Loading_Performance AS SET NOCOUNT ON;')
GO

alter procedure [dbo].[Report_Manual_Barging_and_Loading_Performance] @p_dateTime DateTime = ''
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
    declare @Month_ int = Month(@p_dateTime)
    declare @periode varchar(255) = Convert(varchar(7), @p_dateTime, 120)
    
    declare @_Plan_TCM_Year decimal(10, 2) = 0.0
    declare @_Plan_TCM_Month_Budget decimal(10, 2) = 0.0
    declare @_Plan_TCM_Month_Outlook decimal(10, 2) = 0.0
    declare @_Plan_BEK_Year decimal(10, 2) = 0.0
    declare @_Plan_BEK_Month_Budget decimal(10, 2) = 0.0
    declare @_Plan_BEK_Month_Outlook decimal(10, 2) = 0.0

    declare @_Actual_TCM_Year decimal(10, 2) = 0.0
    declare @_Actual_TCM_Month decimal(10, 2) = 0.0
    declare @_Actual_BEK_Year decimal(10, 2) = 0.0
    declare @_Actual_BEK_Month decimal(10, 2) = 0.0

    declare @_Blending_Total decimal(10, 2) = 0.0
    declare @_Blending_MTD_TCM decimal(10, 2) = 0.0
    declare @_Blending_MTD_BEK decimal(10, 2) = 0.0
    declare @_Blending_YTD decimal(10, 2) = 0.0

    declare @_Plan_MTD decimal(10, 2) = 0.0
    declare @_Plan_YTD decimal(10, 2) = 0.0

    declare @_Actual_MTD decimal(10, 2) = 0.0
    declare @_Actual_YTD decimal(10, 2) = 0.0
    
    -- TCM: Plan, Budget
    select @_Plan_TCM_Month_Budget = Sum(Tonnage)
    from Port_Loading
    where CompanyCode = 'TCM'
        and Periode = @periode
        and [Type] = 'budget'
        
    -- TCM: Plan, Outlook
    select @_Plan_TCM_Month_Outlook = Sum(Tonnage)
    from Port_Loading
    where CompanyCode = 'TCM'
        and Periode = @periode
        and [Type] = 'outlook'
    
    -- TCM Year: Plan, Outlook
    select @_Plan_TCM_Year = Sum(Tonnage)
    from Port_Loading
    where CompanyCode = 'TCM'
        and Periode like Convert(varchar(4), @Year) + '%'
        and [Type] = 'outlook'
    
    -- BEK: Plan, Budget
    select @_Plan_BEK_Month_Budget = Sum(Tonnage)
    from Port_Loading
    where CompanyCode = 'BEK'
        and Periode = @periode
        and [Type] = 'budget'
    
    -- BEK: Plan, Outlook
    select @_Plan_BEK_Month_Outlook = Sum(Tonnage)
    from Port_Loading
    where CompanyCode = 'BEK'
        and Periode = @periode
        and [Type] = 'outlook'
  
    -- BEK Year: Plan, Outlook
    select @_Plan_BEK_Year = Sum(Tonnage)
    from Port_Loading
    where CompanyCode = 'BEK'
        and Periode like Convert(varchar(4), @Year) + '%'
        and [Type] = 'outlook'
    
    select @_Actual_TCM_Year = Sum(Draft)
    from  Loading_Actual_Cargo_Loaded
    where ActualId in
    (
      select RecordId
      from Loading_Actual
      where Year(CreatedOn) = @Year
    )
    
    select @_Actual_TCM_Month = Sum(Draft)
    from  Loading_Actual_Cargo_Loaded
    where ActualId in
    (
      select RecordId
      from Loading_Actual
      where Convert(varchar(7), CreatedOn, 120) = '2021-03'
    )
    
    set @_Plan_MTD = @_Plan_TCM_Month_Outlook + @_Plan_BEK_Month_Outlook
    set @_Plan_YTD = @_Plan_TCM_Year + @_Plan_BEK_Year
    
    set @_Actual_MTD = @_Actual_TCM_Month + @_Actual_BEK_Month
    set @_Actual_YTD = @_Actual_TCM_Year + @_Actual_BEK_Year
    
    set @_Blending_Total = 0.0
    set @_Blending_MTD_TCM = 0.0
    set @_Blending_MTD_BEK = 0.0
    set @_Blending_YTD = 0.0
        
    set @_Plan_TCM_Year = IsNull(@_Plan_TCM_Year, 0.00)
    set @_Plan_TCM_Month_Budget = IsNull(@_Plan_TCM_Month_Budget, 0.00)
    set @_Plan_TCM_Month_Outlook = IsNull(@_Plan_TCM_Month_Outlook, 0.00)
    set @_Plan_BEK_Year = IsNull(@_Plan_BEK_Year, 0.00)
    set @_Plan_BEK_Month_Budget = IsNull(@_Plan_BEK_Month_Budget, 0.00)
    set @_Plan_BEK_Month_Outlook = IsNull(@_Plan_BEK_Month_Outlook, 0.00)
    
    set @_Actual_TCM_Year = IsNull(@_Actual_TCM_Year, 0.00)
    set @_Actual_TCM_Month = IsNull(@_Actual_TCM_Month, 0.00)
    set @_Actual_BEK_Year = IsNull(@_Actual_BEK_Year, 0.00)
    set @_Actual_BEK_Month = IsNull(@_Actual_BEK_Month, 0.00)
    
    set @_Blending_Total = IsNull(@_Blending_Total, 0.00)
    set @_Blending_MTD_TCM = IsNull(@_Blending_MTD_TCM, 0.00)
    set @_Blending_MTD_BEK = IsNull(@_Blending_MTD_BEK, 0.00)
    set @_Blending_YTD = IsNull(@_Blending_YTD, 0.00)
    
    set @_Plan_MTD = IsNull(@_Plan_MTD, 0.00)
    set @_Plan_YTD = IsNull(@_Plan_YTD, 0.00)
    
    set @_Actual_MTD = IsNull(@_Actual_MTD, 0.00)
    set @_Actual_YTD = IsNull(@_Actual_YTD, 0.00)
    
    select
        @_Plan_TCM_Year as Plan_TCM_Year
        , @_Plan_TCM_Month_Budget as Plan_TCM_Month_Budget
        , @_Plan_TCM_Month_Outlook as Plan_TCM_Month_Outlook
        , @_Plan_BEK_Year as Plan_BEK_Year
        , @_Plan_BEK_Month_Budget as Plan_BEK_Month_Budget
        , @_Plan_BEK_Month_Outlook as Plan_BEK_Month_Outlook

        , @_Actual_TCM_Year as Actual_TCM_Year
        , @_Actual_TCM_Month as Actual_TCM_Month
        , @_Actual_BEK_Year as Actual_BEK_Year
        , @_Actual_BEK_Month as Actual_BEK_Month

        , @_Blending_Total as Blending_Total
        , @_Blending_MTD_TCM as Blending_MTD_TCM
        , @_Blending_MTD_BEK as Blending_MTD_BEK
        , @_Blending_YTD as Blending_YTD

        , @_Plan_MTD as Plan_MTD
        , @_Plan_YTD as Plan_YTD

        , @_Actual_MTD as Actual_MTD
        , @_Actual_YTD as Actual_YTD

end
go