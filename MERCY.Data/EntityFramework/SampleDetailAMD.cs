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
    
    public partial class SampleDetailAMD
    {
        public int Id { get; set; }
        public string GeoPrefix { get; set; }
        public string SampleId { get; set; }
        public int Shift { get; set; }
        public int Sequence { get; set; }
        public System.DateTime DateSampleStart { get; set; }
        public System.DateTime DateSampleEnd { get; set; }
        public System.DateTime Receive { get; set; }
        public decimal MassSampleReceived { get; set; }
        public decimal TS { get; set; }
        public decimal ANC { get; set; }
        public decimal NAG { get; set; }
        public string NAGPH45 { get; set; }
        public string NAGPH70 { get; set; }
        public string NAGType { get; set; }
        public string Remark { get; set; }
        public int Sample_Id { get; set; }
        public int CreatedBy { get; set; }
        public System.DateTime CreatedOn { get; set; }
        public Nullable<int> LastModifiedBy { get; set; }
        public Nullable<System.DateTime> LastModifiedOn { get; set; }
        public string Location { get; set; }
        public string LaboratoryId { get; set; }
        public string SampleType { get; set; }
    
        public virtual Sample Sample { get; set; }
    }
}
