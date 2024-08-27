//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace MERCY.Data.EntityFramework
{
    using System;
    using System.Collections.Generic;
    
    public partial class TEMPORARY_BARGE_LOADING_COA
    {
        public long RecordId { get; set; }
        public long File_Physical { get; set; }
        public string Status { get; set; }
        public string Sheet { get; set; }
        public string Job_Number { get; set; }
        public string Lab_ID { get; set; }
        public string Port_of_Loading { get; set; }
        public string Port_Destination { get; set; }
        public string Service_Trip_No { get; set; }
        public string Tug_Boat { get; set; }
        public string Quantity_Draft_Survey { get; set; }
        public string Loading_Date { get; set; }
        public string Report_Date { get; set; }
        public string Report_To { get; set; }
        public string Total_Moisture { get; set; }
        public string Moisture { get; set; }
        public string Ash { get; set; }
        public string Volatile_Matter { get; set; }
        public string Fixed_Carbon { get; set; }
        public string Total_Sulfur { get; set; }
        public string Gross_Caloric_adb { get; set; }
        public string Gross_Caloric_db { get; set; }
        public string Gross_Caloric_ar { get; set; }
        public string Gross_Caloric_daf { get; set; }
        public string Report_Location_Date { get; set; }
        public string Report_Creator { get; set; }
        public string Position { get; set; }
        public string Company { get; set; }
        public int CreatedBy { get; set; }
        public System.DateTime CreatedOn { get; set; }
        public int LastModifiedBy { get; set; }
        public Nullable<System.DateTime> LastModifiedOn { get; set; }
        public bool Total_Moisture_isvalid { get; set; }
        public bool Moisture_isvalid { get; set; }
        public bool Ash_isvalid { get; set; }
        public bool Volatile_Matter_isvalid { get; set; }
        public bool Fixed_Carbon_isvalid { get; set; }
        public bool Total_Sulfur_isvalid { get; set; }
        public bool Gross_Caloric_adb_isvalid { get; set; }
        public bool Gross_Caloric_db_isvalid { get; set; }
        public bool Gross_Caloric_ar_isvalid { get; set; }
        public bool Gross_Caloric_daf_isvalid { get; set; }
        public string CreatedOn_Date_Only { get; set; }
        public int CreatedOn_Year_Only { get; set; }
    }
}