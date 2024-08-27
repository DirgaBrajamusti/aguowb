/* Up script. */
ALTER TABLE Sample_Detail_Loading ALTER COLUMN Customer varchar(50) NULL

/*Down script. */
ALTER TABLE Sample_Detail_Loading ALTER COLUMN Customer varchar(50) NOT NULL