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
    
    public partial class Fuel_Rob
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public Fuel_Rob()
        {
            this.Loading_Actual_Fuel_Consumption = new HashSet<Loading_Actual_Fuel_Consumption>();
        }
    
        public int RecordId { get; set; }
        public string Fuel_Rob_Name { get; set; }
    
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Loading_Actual_Fuel_Consumption> Loading_Actual_Fuel_Consumption { get; set; }
    }
}