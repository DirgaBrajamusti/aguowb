IF OBJECT_ID('Lab_Maintenance_Complete_Count') IS NULL
    EXEC('CREATE function Lab_Maintenance_Complete_Count() RETURNS integer as begin return 1; end;')
GO

alter function [dbo].[Lab_Maintenance_Complete_Count](@p_maintenanceId bigint)
RETURNS integer
as
begin
    declare @_result int = 0;
    
    select @_result = Count(RecordId)
    from LabMaintenance_Complete
    where (LabMaintenance_Data = @p_maintenanceId)
    
    return @_result;
end