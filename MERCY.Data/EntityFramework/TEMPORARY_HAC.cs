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
    
    public partial class TEMPORARY_HAC
    {
        public long RecordId { get; set; }
        public long Header { get; set; }
        public string Status { get; set; }
        public int CreatedBy { get; set; }
        public System.DateTime CreatedOn { get; set; }
        public string Date_Sampling { get; set; }
        public string Day_work { get; set; }
        public string Tonnage { get; set; }
        public string LOT { get; set; }
        public string Lab_ID { get; set; }
        public string TM { get; set; }
        public string M { get; set; }
        public string ASH { get; set; }
        public string TS { get; set; }
        public string CV { get; set; }
        public string Remark { get; set; }
        public bool Date_Sampling_isvalid { get; set; }
        public bool Day_work_isvalid { get; set; }
        public bool Tonnage_isvalid { get; set; }
        public bool LOT_isvalid { get; set; }
        public bool Lab_ID_isvalid { get; set; }
        public bool TM_isvalid { get; set; }
        public bool M_isvalid { get; set; }
        public bool ASH_isvalid { get; set; }
        public bool TS_isvalid { get; set; }
        public bool CV_isvalid { get; set; }
        public bool Remark_isvalid { get; set; }
        public string Company { get; set; }
        public string CreatedOn_Date_Only { get; set; }
        public int CreatedOn_Year_Only { get; set; }
        public string Sheet { get; set; }
    
        public virtual TEMPORARY_HAC_Header TEMPORARY_HAC_Header { get; set; }
    }
}