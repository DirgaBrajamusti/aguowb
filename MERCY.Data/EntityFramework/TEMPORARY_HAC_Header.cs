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
    
    public partial class TEMPORARY_HAC_Header
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public TEMPORARY_HAC_Header()
        {
            this.TEMPORARY_HAC = new HashSet<TEMPORARY_HAC>();
        }
    
        public long RecordId { get; set; }
        public long File_Physical { get; set; }
        public string Company { get; set; }
        public string Sheet { get; set; }
        public int CreatedBy { get; set; }
        public System.DateTime CreatedOn { get; set; }
        public string Date_Detail { get; set; }
        public string Job_No { get; set; }
        public string Report_To { get; set; }
        public string Method1 { get; set; }
        public string Method2 { get; set; }
        public string Method3 { get; set; }
        public string Method4 { get; set; }
        public string CreatedOn_Date_Only { get; set; }
        public int CreatedOn_Year_Only { get; set; }
    
        public virtual TEMPORARY_File TEMPORARY_File { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<TEMPORARY_HAC> TEMPORARY_HAC { get; set; }
    }
}