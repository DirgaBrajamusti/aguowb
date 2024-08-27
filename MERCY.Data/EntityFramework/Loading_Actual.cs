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
    
    public partial class Loading_Actual
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public Loading_Actual()
        {
            this.Loading_Actual_Attachments = new HashSet<Loading_Actual_Attachments>();
            this.Loading_Actual_Barge_Survey_Condition = new HashSet<Loading_Actual_Barge_Survey_Condition>();
            this.Loading_Actual_Cargo_Loaded = new HashSet<Loading_Actual_Cargo_Loaded>();
            this.Loading_Actual_Coal_Temperature = new HashSet<Loading_Actual_Coal_Temperature>();
            this.Loading_Actual_Draft_Survey = new HashSet<Loading_Actual_Draft_Survey>();
            this.Loading_Actual_Fuel_Consumption = new HashSet<Loading_Actual_Fuel_Consumption>();
        }
    
        public long RecordId { get; set; }
        public string TugName { get; set; }
        public int RequestId { get; set; }
        public string No_Ref_Report { get; set; }
        public int SiteId { get; set; }
        public string No_Services_Trip { get; set; }
        public string Barge_Name { get; set; }
        public int Barge_Size { get; set; }
        public string Route { get; set; }
        public string Load_Type { get; set; }
        public System.DateTime Arrival_Time { get; set; }
        public System.DateTime Initial_Draft { get; set; }
        public System.DateTime Anchor_Up { get; set; }
        public System.DateTime Berthed_Time { get; set; }
        public System.DateTime Commenced_Loading { get; set; }
        public System.DateTime Completed_Loading { get; set; }
        public System.DateTime Unberthing { get; set; }
        public System.DateTime Departure { get; set; }
        public string Coal_Quality_Spec { get; set; }
        public string Delay_Cause_of_Barge_Changing { get; set; }
        public string Surveyor_Name { get; set; }
        public string Weather_Condition { get; set; }
        public decimal Water_Level_During_Loading { get; set; }
        public decimal Daily_Water_Level { get; set; }
        public decimal Water_Level_at_Jetty { get; set; }
        public int CreatedBy { get; set; }
        public System.DateTime CreatedOn { get; set; }
        public int LastModifiedBy { get; set; }
        public Nullable<System.DateTime> LastModifiedOn { get; set; }
        public string Status { get; set; }
        public string Shipment_Type { get; set; }
    
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Loading_Actual_Attachments> Loading_Actual_Attachments { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Loading_Actual_Barge_Survey_Condition> Loading_Actual_Barge_Survey_Condition { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Loading_Actual_Cargo_Loaded> Loading_Actual_Cargo_Loaded { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Loading_Actual_Coal_Temperature> Loading_Actual_Coal_Temperature { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Loading_Actual_Draft_Survey> Loading_Actual_Draft_Survey { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Loading_Actual_Fuel_Consumption> Loading_Actual_Fuel_Consumption { get; set; }
    }
}
