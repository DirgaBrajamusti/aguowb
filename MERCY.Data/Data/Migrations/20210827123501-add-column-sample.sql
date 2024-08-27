ALTER TABLE Sample
ADD [Stage] [varchar](30) NOT NULL DEFAULT '0',
[DeletedOn] [datetime] NULL,
[DeletedBy] [int] NULL