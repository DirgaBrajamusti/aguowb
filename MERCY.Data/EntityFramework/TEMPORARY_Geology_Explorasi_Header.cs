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
    
    public partial class TEMPORARY_Geology_Explorasi_Header
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public TEMPORARY_Geology_Explorasi_Header()
        {
            this.TEMPORARY_Geology_Explorasi = new HashSet<TEMPORARY_Geology_Explorasi>();
        }
    
        public long RecordId { get; set; }
        public long File_Physical { get; set; }
        public string Company { get; set; }
        public int CreatedBy { get; set; }
        public System.DateTime CreatedOn { get; set; }
        public string Date_Detail { get; set; }
        public string Job_No { get; set; }
        public string Report_To { get; set; }
        public string Date_Received { get; set; }
        public string Nomor { get; set; }
        public string Sheet { get; set; }
        public string CreatedOn_Date_Only { get; set; }
        public int CreatedOn_Year_Only { get; set; }
    
        public virtual TEMPORARY_File TEMPORARY_File { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<TEMPORARY_Geology_Explorasi> TEMPORARY_Geology_Explorasi { get; set; }
    }
}
