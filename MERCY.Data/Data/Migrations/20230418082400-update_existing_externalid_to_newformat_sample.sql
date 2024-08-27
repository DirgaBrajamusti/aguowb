/* Up Script*/

UPDATE Sample
SET ExternalId = (SELECT CONCAT(LoadingNumber, ' - ', VesselName, ' - Lot ', LotNumber) From SampleDetailLoading WHERE Sample.Id = SampleDetailLoading.SampleId AND Sample.CompanyCode = 'IMM');

UPDATE SampleDetailLoading 
SET LotSamples = CONCAT(LoadingNumber, ' - ', VesselName, ' - Lot ', LotNumber)
WHERE SampleId IN (SELECT s.Id FROM Sample s WHERE s.CompanyCode = 'IMM')

/*Down Script*/

UPDATE Sample
SET ExternalId = (SELECT CONCAT(LoadingNumber, ' - Lot ', LotNumber) From SampleDetailLoading WHERE Sample.Id = SampleDetailLoading.SampleId AND Sample.CompanyCode = 'IMM');

UPDATE SampleDetailLoading 
SET LotSamples = CONCAT(LoadingNumber, ' - Lot ', LotNumber)
WHERE SampleId IN (SELECT s.Id FROM Sample s WHERE s.CompanyCode = 'IMM');