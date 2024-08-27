/* Up Script */
ALTER TABLE dbo.Sample_Detail_General
DROP COLUMN Customer, Vessel, Surveyor, ShipmentNumber;

ALTER TABLE dbo.Sample_Detail_General
ADD TripNumber varchar(50) NOT NULL;

/*Down Script*/

ALTER TABLE dbo.Sample_Detail_General
ADD Customer varchar(50) NOT NULL,
	Vessel varchar(50) NOT NULL,
	Surveyor varchar(50) NOT NULL,
	ShipmentNumber varchar(50) NOT NULL;

ALTER TABLE dbo.Sample_Detail_General
DROP COLUMN TripNumber;