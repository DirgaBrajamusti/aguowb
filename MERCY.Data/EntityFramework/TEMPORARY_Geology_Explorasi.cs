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
    
    public partial class TEMPORARY_Geology_Explorasi
    {
        public long RecordId { get; set; }
        public long Header { get; set; }
        public string Status { get; set; }
        public int CreatedBy { get; set; }
        public System.DateTime CreatedOn { get; set; }
        public string Sample_ID { get; set; }
        public string SampleType { get; set; }
        public string Lab_ID { get; set; }
        public string Mass_Spl { get; set; }
        public string TM { get; set; }
        public string M { get; set; }
        public string VM { get; set; }
        public string Ash { get; set; }
        public decimal FC { get; set; }
        public string TS { get; set; }
        public string Cal_ad { get; set; }
        public decimal Cal_db { get; set; }
        public decimal Cal_ar { get; set; }
        public decimal Cal_daf { get; set; }
        public string RD { get; set; }
        public bool Sample_ID_isvalid { get; set; }
        public bool SampleType_isvalid { get; set; }
        public bool Lab_ID_isvalid { get; set; }
        public bool Mass_Spl_isvalid { get; set; }
        public bool TM_isvalid { get; set; }
        public bool M_isvalid { get; set; }
        public bool VM_isvalid { get; set; }
        public bool Ash_isvalid { get; set; }
        public bool FC_isvalid { get; set; }
        public bool TS_isvalid { get; set; }
        public bool Cal_ad_isvalid { get; set; }
        public bool Cal_db_isvalid { get; set; }
        public bool Cal_ar_isvalid { get; set; }
        public bool Cal_daf_isvalid { get; set; }
        public bool RD_isvalid { get; set; }
        public string Company { get; set; }
        public string CreatedOn_Date_Only { get; set; }
        public int CreatedOn_Year_Only { get; set; }
        public string Sheet { get; set; }
    
        public virtual TEMPORARY_Geology_Explorasi_Header TEMPORARY_Geology_Explorasi_Header { get; set; }
    }
}