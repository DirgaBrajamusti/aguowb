IF OBJECT_ID('To_Decimal_3') IS NULL
    EXEC('CREATE function dbo.To_Decimal_3() RETURNS decimal(10,3) as begin return -1.00; end;')
GO

alter function dbo.To_Decimal_3(@p_value varchar(255), @p_default decimal(10,3))
RETURNS decimal(10,3)
as
begin
    declare @_result decimal(10,3) = 0.00;
    declare @vMessage varchar(8000)
    
    if (Lower(@p_value) = 'n/a')
    begin
        select @_result = -1.00
    end
    else
    begin
        select @_result = TRY_CONVERT(decimal(10,3), @p_value)
    end
    
    if (@_result is null)
    begin
        select @_result = @p_default
    end
    
    return @_result;
end