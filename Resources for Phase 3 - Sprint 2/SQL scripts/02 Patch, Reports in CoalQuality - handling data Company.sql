ALTER TABLE UPLOAD_BARGE_LOADING ADD Company varchar(32) not null default '';
go

ALTER TABLE UPLOAD_CRUSHING_PLANT ADD Company varchar(32) not null default '';
go

ALTER TABLE UPLOAD_Geology_Explorasi ADD Company varchar(32) not null default '';
go

ALTER TABLE UPLOAD_Geology_Pit_Monitoring ADD Company varchar(32) not null default '';
go

ALTER TABLE UPLOAD_Sampling_ROM ADD Company varchar(32) not null default '';
go

-- do Update
update UPLOAD_BARGE_LOADING
    set UPLOAD_BARGE_LOADING.Company = h.Company
FROM 
    UPLOAD_BARGE_LOADING t
    INNER JOIN UPLOAD_BARGE_LOADING_Header h on t.Header = h.RecordId
go

update UPLOAD_CRUSHING_PLANT
    set UPLOAD_CRUSHING_PLANT.Company = h.Company
FROM 
    UPLOAD_CRUSHING_PLANT t
    INNER JOIN UPLOAD_CRUSHING_PLANT_Header h on t.Header = h.RecordId
go

update UPLOAD_Geology_Explorasi
    set UPLOAD_Geology_Explorasi.Company = h.Company
FROM 
    UPLOAD_Geology_Explorasi t
    INNER JOIN UPLOAD_Geology_Explorasi_Header h on t.Header = h.RecordId
go

update UPLOAD_Geology_Pit_Monitoring
    set UPLOAD_Geology_Pit_Monitoring.Company = h.Company
FROM 
    UPLOAD_Geology_Pit_Monitoring t
    INNER JOIN UPLOAD_Geology_Pit_Monitoring_Header h on t.Header = h.RecordId
go

update UPLOAD_Sampling_ROM
    set UPLOAD_Sampling_ROM.Company = h.Company
FROM 
    UPLOAD_Sampling_ROM t
    INNER JOIN UPLOAD_Sampling_ROM_Header h on t.Header = h.RecordId
go
