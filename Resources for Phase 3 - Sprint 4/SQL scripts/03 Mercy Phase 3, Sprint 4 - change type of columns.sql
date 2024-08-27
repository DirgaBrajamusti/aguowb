ALTER TABLE AnalysisRequest_Detail DROP CONSTRAINT DF__AnalysisRe__From__0A9D95DB;
go

alter table AnalysisRequest_Detail alter Column [From] NUMERIC(10,2) NOT NULL;
Go

ALTER TABLE AnalysisRequest_Detail DROP CONSTRAINT DF__AnalysisRequ__To__0B91BA14;
go

alter table AnalysisRequest_Detail alter Column [To] NUMERIC(10,2) NOT NULL ;
Go
