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
    
    public partial class Loading_Actual_Draft_Survey
    {
        public int RecordId { get; set; }
        public long ActualId { get; set; }
        public int Draft_Survey_Id { get; set; }
        public decimal Initial { get; set; }
        public decimal Final { get; set; }
        public int CreatedBy { get; set; }
        public System.DateTime CreatedOn { get; set; }
        public int LastModifiedBy { get; set; }
        public Nullable<System.DateTime> LastModifiedOn { get; set; }
    
        public virtual Draft_Survey Draft_Survey { get; set; }
        public virtual Loading_Actual Loading_Actual { get; set; }
    }
}