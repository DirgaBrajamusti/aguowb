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
    
    public partial class Loading_Request_Loading_Plan
    {
        public long RecordId { get; set; }
        public long Request_Id { get; set; }
        public long Loading_Plan_Id { get; set; }
        public int SiteId { get; set; }
        public string Company { get; set; }
        public string Shipment_Type { get; set; }
        public string Buyer { get; set; }
        public string Vessel { get; set; }
        public string Destination { get; set; }
        public string Trip_No { get; set; }
        public string Remark { get; set; }
        public int CreatedBy { get; set; }
        public System.DateTime CreatedOn { get; set; }
        public string CreatedOn_Date_Only { get; set; }
        public int LastModifiedBy { get; set; }
        public Nullable<System.DateTime> LastModifiedOn { get; set; }
    
        public virtual Loading_Request Loading_Request { get; set; }
    }
}