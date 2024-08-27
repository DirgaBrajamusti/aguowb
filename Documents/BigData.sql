create table dbo.Mercy_ROM_Info_t
(
    ROM_id	int not null
    , company_code	nvarchar(20)
    , Block nvarchar(200)
    , ROM_Name nvarchar(200)
    , primary key (ROM_id)
);

create table dbo.Mercy_Source_t
(
    Company_code nvarchar(3)
    , pit		nvarchar(255)
    , seam 	    nvarchar(255)
    , primary key (Company_code, pit, seam)
);

create table dbo.Mercy_ROM_Quality_t
(
    ROM_Id          int
    , company_code	nvarchar(3)
    , Process_Date  datetime
    , Block	        nvarchar(255)
    , ROM_Name	    nvarchar(255)    
    , Ton	        float
    , CV	        float
    , TS            float
    , ASH	        float
    , IM	        float
    , TM	        float
    , primary key (ROM_Id)
);

create view [dbo].[Mercy_ROM_Info]
as select * from [dbo].[Mercy_ROM_Info_t];

create view [dbo].[Mercy_ROM_Quality]
as select * from [dbo].[Mercy_ROM_Quality_t];

create view [dbo].[Mercy_Source]
as select * from [dbo].[Mercy_Source_t];

create table dbo.Mercy_Product_t
(
    id int
    , ProductName nvarchar(50)
    , primary key(id)
);

create view [dbo].[Mercy_Product]
as select * from [dbo].[Mercy_Product_t];

create table Mercy_quality_outlook_t
(
  id int
  , Product_name nvarchar(50)
  , FirstDate date
  , Year int
  , months nvarchar(3)
  , CV float
  , TS float
  , ash float
  , IM float
  , TM float
  , primary key(id)
);

create view [dbo].Mercy_quality_outlook
as select * from [dbo].Mercy_quality_outlook_t;
