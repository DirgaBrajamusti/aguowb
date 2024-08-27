/* Up Script*/

UPDATE dbo.CompanyClientProjectScheme
	SET IsRequired = 1
	WHERE CompanyCode = 'IMM' AND ClientId = 10 and ProjectId = 24 and SchemeId = 28; -- Flow Test - Flow Test (TM)

UPDATE dbo.CompanyClientProjectScheme
	SET IsRequired = 0
	WHERE
		-- Geology - Preproduction (Test Quality)
		(CompanyCode = 'IMM' AND ClientId = 4 and ProjectId = 9 and SchemeId = 1) OR -- AA CaO
		(CompanyCode = 'IMM' AND ClientId = 4 and ProjectId = 9 and SchemeId = 18) OR -- AA Na2O
		(CompanyCode = 'IMM' AND ClientId = 4 and ProjectId = 9 and SchemeId = 23) OR -- RD
		
		-- Round Robin - Proficiency Test
		(CompanyCode = 'IMM' AND ClientId = 5 and ProjectId = 17 and SchemeId = 9) OR -- FM
		(CompanyCode = 'IMM' AND ClientId = 5 and ProjectId = 17 and SchemeId = 24) OR -- RM
		(CompanyCode = 'IMM' AND ClientId = 5 and ProjectId = 17 and SchemeId = 28) OR -- TM

		-- Check - Daily Check
		(CompanyCode = 'IMM' AND ClientId = 8 and ProjectId = 22 and SchemeId = 9) OR -- FM
		(CompanyCode = 'IMM' AND ClientId = 8 and ProjectId = 22 and SchemeId = 23) OR -- RD
		(CompanyCode = 'IMM' AND ClientId = 8 and ProjectId = 22 and SchemeId = 24) OR -- RM
		(CompanyCode = 'IMM' AND ClientId = 8 and ProjectId = 22 and SchemeId = 28) OR -- TM

		-- Flow Test - Flow Test
		(CompanyCode = 'IMM' AND ClientId = 10 and ProjectId = 24 and SchemeId = 28); -- SIZE

/* Down Script*/

UPDATE dbo.CompanyClientProjectScheme
	SET IsRequired = 0
	WHERE CompanyCode = 'IMM' AND ClientId = 10 and ProjectId = 24 and SchemeId = 28; -- Flow Test - Flow Test (TM)

UPDATE dbo.CompanyClientProjectScheme
	SET IsRequired = 1
	WHERE
		-- Geology - Preproduction (Test Quality)
		(CompanyCode = 'IMM' AND ClientId = 4 and ProjectId = 9 and SchemeId = 1) OR -- AA CaO
		(CompanyCode = 'IMM' AND ClientId = 4 and ProjectId = 9 and SchemeId = 18) OR -- AA Na2O
		(CompanyCode = 'IMM' AND ClientId = 4 and ProjectId = 9 and SchemeId = 23) OR -- RD
		
		-- Round Robin - Proficiency Test
		(CompanyCode = 'IMM' AND ClientId = 5 and ProjectId = 17 and SchemeId = 9) OR -- FM
		(CompanyCode = 'IMM' AND ClientId = 5 and ProjectId = 17 and SchemeId = 24) OR -- RM
		(CompanyCode = 'IMM' AND ClientId = 5 and ProjectId = 17 and SchemeId = 28) OR -- TM

		-- Check - Daily Check
		(CompanyCode = 'IMM' AND ClientId = 8 and ProjectId = 22 and SchemeId = 9) OR -- FM
		(CompanyCode = 'IMM' AND ClientId = 8 and ProjectId = 22 and SchemeId = 23) OR -- RD
		(CompanyCode = 'IMM' AND ClientId = 8 and ProjectId = 22 and SchemeId = 24) OR -- RM
		(CompanyCode = 'IMM' AND ClientId = 8 and ProjectId = 22 and SchemeId = 28) OR -- TM

		-- Flow Test - Flow Test
		(CompanyCode = 'IMM' AND ClientId = 10 and ProjectId = 24 and SchemeId = 28); -- SIZE