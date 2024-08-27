IF OBJECT_ID('PROCESS_LECO_for_BARGE_LOADING') IS NULL
    EXEC('CREATE PROCEDURE PROCESS_LECO_for_BARGE_LOADING AS SET NOCOUNT ON;')
GO

alter procedure [dbo].[PROCESS_LECO_for_BARGE_LOADING] @p_timeStamp datetime, @p_leco_Stamp varchar(255) output
as
begin
/*
BG.TCM.XXX -> Barge Loading for TCM, other info (Product, shift, etc) is blank (later on will be define from uploading lab result)
CP.TCM.XXX -> Crushing Plant for TCM, other info (Product, shift, etc) is blank (later on will be define from uploading lab result)
ROM.TCM.XXX -> ROM for TCM, other info (Pit, Seam, Location, shift, etc) is blank (later on will be define from uploading lab result)
GE.TCM.XXX -> Geology for TCM, for Geology exploration or pit monitoring type system should match the number with Lab Analysis Request data
*/

    declare @id_LECO int = null
    declare @id_Upload bigint
    declare @value_CV varchar(255)
    declare @value_Sulfur varchar(255)
    declare @value_Moisture varchar(255)
    declare @value_Volatile varchar(255)
    declare @value_Ash varchar(255)
    declare @name varchar(255)
    declare @is_Found int = 0
    
    declare @company varchar(255)
    declare @id varchar(255)
    
    select @p_leco_Stamp = Convert(varchar(20), @p_timeStamp, 120)
    
    BEGIN TRY
        DECLARE data_1 CURSOR FOR
            select RecordId, Company, ID_Number
            from UPLOAD_BARGE_LOADING
            where (CreatedOn = @p_timeStamp) or (LastModifiedOn = @p_timeStamp)
            order by RecordId
        
        OPEN data_1  
  
        FETCH NEXT FROM data_1   
        INTO @id_Upload, @company, @id
          
        WHILE @@FETCH_STATUS = 0  
        BEGIN
            -- buat Nama dengan pola: BG.TCM.XXX
            select @name = 'BG.' + @company + '.' + @id
            
            -- LECO CV
            BEGIN TRY
                -- reset
                select @id_LECO = null
                
                -- cari di Table LECO: CV
                select top 1 @id_LECO = RecordId, @value_CV = CV
                from UPLOAD_LECO_CV
                where NAME = @name
                order by RecordId desc
                
                -- jika ketemu, maka:
                -- 1. lakukan Update nilai
                -- 2. lakukan "Marking" nilai LECO_Stamp
                if (@id_LECO is not null)
                begin
                    update UPLOAD_BARGE_LOADING
                    set CV_adb = Convert(decimal(10, 2), @value_CV)
                        , LECO_Stamp = @p_leco_Stamp
                    where RecordId = @id_Upload
                    
                    select @is_Found = 1
                end
            END TRY  
            BEGIN CATCH  
                 
            END CATCH
            
            -- LECO TS
            BEGIN TRY
                -- reset
                select @id_LECO = null
                
                -- cari di Table LECO: TS
                select top 1 @id_LECO = RecordId, @value_Sulfur = Sulfur
                from UPLOAD_LECO_TS
                where Name = @name
                order by RecordId desc
                
                -- jika ketemu, maka:
                -- 1. lakukan Update nilai
                -- 2. lakukan "Marking" nilai LECO_Stamp
                if (@id_LECO is not null)
                begin
                    update UPLOAD_BARGE_LOADING
                    set TS_adb = Convert(decimal(10, 2), @value_Sulfur)
                        , LECO_Stamp = @p_leco_Stamp
                    where RecordId = @id_Upload
                    
                    select @is_Found = 1
                end
            END TRY  
            BEGIN CATCH  
                 
            END CATCH
            
            -- LECO TGA
            BEGIN TRY
                -- reset
                select @id_LECO = null
                
                -- cari di Table LECO: TGA
                select top 1 @id_LECO = RecordId
                        , @value_Moisture = Moisture, @value_Volatile = Volatile, @value_Ash = Ash
                from UPLOAD_LECO_TGA
                where NAME = @name
                order by RecordId desc
                
                -- jika ketemu, maka:
                -- 1. lakukan Update nilai
                -- 2. lakukan "Marking" nilai LECO_Stamp
                if (@id_LECO is not null)
                begin
                    update UPLOAD_BARGE_LOADING
                    set M = Convert(decimal(10, 2), @value_Moisture)
                        , ASH_adb = Convert(decimal(10, 2), @value_Ash)
                        , VM_adb = Convert(decimal(10, 2), @value_Volatile)
                        , LECO_Stamp = @p_leco_Stamp
                    where RecordId = @id_Upload
                    
                    select @is_Found = 1
                end
            END TRY  
            BEGIN CATCH  
                 
            END CATCH
            
            -- get next Record
            FETCH NEXT FROM data_1   
            INTO @id_Upload, @company, @id
        END   
        CLOSE data_1;  
        DEALLOCATE data_1;
        
    END TRY  
    BEGIN CATCH  
         
    END CATCH  

    if (@is_Found = 0)
    begin
        select @p_leco_Stamp = ''
    end
end
