ALTER TABLE AnalysisRequest_History ALTER COLUMN Description varchar(8000) not null;
go

ALTER TABLE SamplingRequest_History ALTER COLUMN Description varchar(8000) not null;
go

CREATE TABLE Discussion
(
    RecordId INT NOT NULL IDENTITY(1,1),
    Page VARCHAR(255) NOT NULL Default '',
    ReferenceId BIGINT NOT NULL DEFAULT 0,
    Typeof VARCHAR(32) NOT NULL Default '', -- [Remark or File]
    Remark VARCHAR(255) NOT NULL Default '',
    Attached VARCHAR(255) NOT NULL Default '',
    Attached2 VARCHAR(255) NOT NULL Default '',
    CreatedBy INT NOT NULL DEFAULT 0,
    CreatedOn datetime NOT NULL DEFAULT GetDate(),
    CONSTRAINT PK_Discussion_RecordId PRIMARY KEY (RecordId)
);
