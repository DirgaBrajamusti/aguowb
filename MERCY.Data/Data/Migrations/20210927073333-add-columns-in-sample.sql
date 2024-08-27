/* Up Script*/
Alter Table dbo.Sample
ADD mm_status [varchar](50) NULL,
mm_fetched_at datetime NULL,
mm_remark [varchar](max) NULL;

/*Down Script*/
Alter Table dbo.Sample
DROP COLUMN mm_status,
COLUMN mm_fetched_at,
COLUMN mm_remark;
