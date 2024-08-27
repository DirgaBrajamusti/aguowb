IF OBJECT_ID('PROCESS_HAULING_REQUEST_DETAIL_Tunnel_Actual') IS NULL
    EXEC('CREATE PROCEDURE PROCESS_HAULING_REQUEST_DETAIL_Tunnel_Actual AS SET NOCOUNT ON;')
GO

alter procedure [dbo].[PROCESS_HAULING_REQUEST_DETAIL_Tunnel_Actual] @p_HaulingRequest_Id bigint, @p_UserId int
as
begin
    declare @lastId bigint = 0
    
    BEGIN TRY  
        select @lastId = Max(RecordId) from Tunnel_Actual
        
        -- Actual_Tunnel
        insert into Tunnel_Actual(HaulingRequest_Reference, TunnelId, [Time], CreatedBy, CreatedOn)
        select h.RecordIdx, t.TunnelId, Convert(varchar(10), BlendingDate, 120) + ' ' + case when (h.Shift = 1) then '07:00:00' else '19:00:00' end, @p_UserId, GetDate()
        from HaulingRequest_Detail_PortionBlending h
            , Tunnel t 
        where h.HaulingRequest = @p_HaulingRequest_Id
            and h.Tunnel = t.Name
        
        -- History: Actual_Tunnel        
        insert into Tunnel_Actual_History(
                                            Tunnel_Actual_Id
                                            ,HaulingRequest_Reference
                                            ,TunnelId
                                            ,CreatedBy
                                            ,CreatedOn
                                            ,LastModifiedBy
                                            ,LastModifiedOn
                                            ,[Time]
                                            ,Remark
                                            ,[Status]
                                            , Changed_Tunnel)
        select RecordId
                ,HaulingRequest_Reference
                ,TunnelId
                ,CreatedBy
                ,CreatedOn
                ,LastModifiedBy
                ,LastModifiedOn
                ,[Time]
                ,Remark
                ,[Status]
                , Convert(varchar(32), TunnelId) + ',' + Convert(varchar(32), TunnelId)
        from Tunnel_Actual
        where RecordId > @lastId
    END TRY  
    BEGIN CATCH  
         
    END CATCH  

    -- tampilkan "Hasil Eksekusi"
    select 'Success' as cMessage
end
