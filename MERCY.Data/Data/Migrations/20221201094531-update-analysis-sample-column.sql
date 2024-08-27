/* Up Script*/
ALTER TABLE AnalysisRequest
ADD 
 Moisture BIT DEFAULT 0 NOT NULL,
 DensityWet BIT DEFAULT 0 NOT NULL,
 DensityDry BIT DEFAULT 0 NOT NULL,
 UCS BIT DEFAULT 0 NOT NULL,
 DST BIT DEFAULT 0 NOT NULL,
 GPa BIT DEFAULT 0 NOT NULL,
 PoissionRatio BIT DEFAULT 0 NOT NULL,
 KPa BIT DEFAULT 0 NOT NULL,
 CUD BIT DEFAULT 0 NOT NULL,
 EstimatedEndDate DATE;

ALTER TABLE AnalysisRequest_Detail 
ADD RockType VARCHAR(32);

/*Drop Script*/
ALTER TABLE AnalysisRequest 
DROP COLUMN
	Moisture, 
    DensityWet, 
    DensityDry, 
    UCS, 
    DST, 
    GPa, 
    PoissionRatio, 
    KPa, 
    CUD, 
    EstimatedEndDate;

ALTER TABLE AnalysisRequest_Detail 
DROP COLUMN
	RockType;