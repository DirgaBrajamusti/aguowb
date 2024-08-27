IF OBJECT_ID('To_Integer') IS NULL
    EXEC('CREATE function dbo.To_Integer() RETURNS int as begin return -1; end;')
GO

alter function dbo.To_Integer(@p_value varchar(255), @p_default int)
RETURNS int
as
begin
    declare @_result int = 0;
    declare @vMessage varchar(8000)
    
    if (Lower(@p_value) = 'n/a')
    begin
        select @_result = -1
    end
    else
    begin
        select @_result = TRY_CONVERT(int, @p_value)
    end
    
    if (@_result is null)
    begin
        select @_result = @p_default
    end
    
    return @_result;
end