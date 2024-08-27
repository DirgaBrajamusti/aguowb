IF OBJECT_ID('PROCESS_HAULING_REQUEST_DETAIL') IS NULL
    EXEC('CREATE PROCEDURE PROCESS_HAULING_REQUEST_DETAIL AS SET NOCOUNT ON;')
GO

alter procedure [dbo].[PROCESS_HAULING_REQUEST_DETAIL] @p_HaulingRequest_Id bigint
as
begin
    declare @vStatus varchar(32) = ''
    declare @vMessage varchar(8000) = ''
    
    BEGIN TRY
        -- just in case, this StoreProcedure is executed more than once
        delete from HaulingRequest_Detail_ROMTransfer
        where HaulingRequest = @p_HaulingRequest_Id
        
        delete from HaulingRequest_Detail_PortionBlending_Details
        where HaulingRequest = @p_HaulingRequest_Id
        
        delete from HaulingRequest_Detail_PortionBlending
        where HaulingRequest = @p_HaulingRequest_Id
        
        -- Insert: ROM Transfer
        insert into HaulingRequest_Detail_ROMTransfer 
        (
            HaulingRequest
            , RecordId_Snapshot
            , Company
            , TransferDate
            , Shift
            , Source_ROM_ID
            , Source_Block
            , Source_ROM_Name
            , Destination_ROM_ID
            , Destination_Block
            , Destination_ROM_Name
            , Remark
            , CreatedBy
            , CreatedOn
            , LastModifiedBy
            , LastModifiedOn
            , DeletedBy
            , DeletedOn
        )
        select
            @p_HaulingRequest_Id
            , RecordId
            , Company
            , TransferDate
            , Shift
            , Source_ROM_ID
            , Source_Block
            , Source_ROM_Name
            , Destination_ROM_ID
            , Destination_Block
            , Destination_ROM_Name
            , Remark
            , CreatedBy
            , CreatedOn
            , LastModifiedBy
            , LastModifiedOn
            , DeletedBy
            , DeletedOn
        from ROMTransfer
        where 
            ROMTransfer.RecordId in
            (
                select DataId
                from HaulingRequest_Detail
                where HaulingRequest = @p_HaulingRequest_Id
                    and DataType = 'ROM_Transfer'
            )
        order by ROMTransfer.RecordId
        
        -- Insert: PortionBlending
        insert into HaulingRequest_Detail_PortionBlending
        (
            HaulingRequest
            , RecordId_Snapshot
            , Company
            , Product
            , BlendingDate
            , Shift
            , NoHauling
            , Remark
            , CV
            , TS
            , ASH
            , IM
            , TM
            , Ton
            , Hopper
            , Tunnel
            , CreatedBy
            , CreatedOn
            , LastModifiedBy
            , LastModifiedOn
            , DeletedBy
            , DeletedOn
        )
        select
            @p_HaulingRequest_Id
            , RecordId
            , Company
            , Product
            , BlendingDate
            , Shift
            , NoHauling
            , Remark
            , CV
            , TS
            , ASH
            , IM
            , TM
            , Ton
            , Hopper
            , Tunnel
            , CreatedBy
            , CreatedOn
            , LastModifiedBy
            , LastModifiedOn
            , DeletedBy
            , DeletedOn
        from PortionBlending
        where 
            PortionBlending.RecordId in
            (
                select DataId
                from HaulingRequest_Detail
                where HaulingRequest = @p_HaulingRequest_Id
                    and DataType = 'PortionBlending'
            )
        order by PortionBlending.RecordId
        
        -- Insert: PortionBlending_Details
        insert into HaulingRequest_Detail_PortionBlending_Details
        (
            HaulingRequest
            , RecordId_Snapshot
            , PortionBlending
            , ROMQuality_RecordId
            , block
            , ROM_Name
            , ROM_ID
            , CV
            , TS
            , ASH
            , IM
            , TM
            , Ton
            , Portion
        )
        select
            @p_HaulingRequest_Id
            , RecordId
            , PortionBlending
            , ROMQuality_RecordId
            , block
            , ROM_Name
            , ROM_ID
            , CV
            , TS
            , ASH
            , IM
            , TM
            , Ton
            , Portion
        from PortionBlending_Details
        where 
            PortionBlending_Details.PortionBlending in
            (
                select DataId
                from HaulingRequest_Detail
                where HaulingRequest = @p_HaulingRequest_Id
                    and DataType = 'PortionBlending'
            )
        order by PortionBlending_Details.RecordId
        
        select @vStatus = 'Ok'
        select @vMessage = 'PROCESS_HAULING_REQUEST_DETAIL'
    END TRY  
    BEGIN CATCH  
         select @vStatus = 'Error'
         select @vMessage = ERROR_MESSAGE()
    END CATCH  

    -- tampilkan "Hasil Eksekusi"
    select @vStatus as cStatus, @vMessage as cMessage
end