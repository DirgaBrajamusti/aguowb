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
    
    public partial class UPLOAD_Geology_AMD_Header
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public UPLOAD_Geology_AMD_Header()
        {
            this.UPLOAD_Geology_AMD = new HashSet<UPLOAD_Geology_AMD>();
        }
    
        public int RecordId { get; set; }
        public string Date_Detail { get; set; }
        public string Job_No { get; set; }
        public string Date_Received { get; set; }
        public string Nomor { get; set; }
        public int CreatedBy { get; set; }
        public System.DateTime CreatedOn { get; set; }
        public Nullable<int> LastModifiedBy { get; set; }
        public Nullable<System.DateTime> LastModifiedOn { get; set; }
        public string Report_To { get; set; }
        public string CompanyCode { get; set; }
    
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<UPLOAD_Geology_AMD> UPLOAD_Geology_AMD { get; set; }
    }
}
