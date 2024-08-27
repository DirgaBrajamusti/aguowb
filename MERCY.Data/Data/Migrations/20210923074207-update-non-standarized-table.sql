/* Up Script */
Exec sp_rename 'Tunnel_History','TunnelHistory';
Exec sp_rename 'Company_RefType','CompanyRefType';
Exec sp_rename 'Sample_Scheme','SampleScheme';
Exec sp_rename 'Company_Client_Project_Scheme','CompanyClientProjectScheme';
Exec sp_rename 'Sample_Detail_General','SampleDetailGeneral';
Exec sp_rename 'Sample_Detail_Loading','SampleDetailLoading';
Exec sp_rename 'Analyst_Job','AnalysisJob';


/* Down Script */
Exec sp_rename 'AnalysisJob','Analyst_Job';
Exec sp_rename 'SampleDetailLoading','Sample_Detail_Loading';
Exec sp_rename 'SampleDetailGeneral','Sample_Detail_General';
Exec sp_rename 'CompanyClientProjectScheme','Company_Client_Project_Scheme';
Exec sp_rename 'SampleScheme','Sample_Scheme';
Exec sp_rename 'CompanyRefType','Company_RefType';
Exec sp_rename 'TunnelHistory','Tunnel_History';
