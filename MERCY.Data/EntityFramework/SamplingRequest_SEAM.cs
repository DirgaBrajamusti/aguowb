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
    
    public partial class SamplingRequest_SEAM
    {
        public long RecordId { get; set; }
        public long SamplingRequest { get; set; }
        public string SEAM { get; set; }
        public string COMPANY_PIT_SEAM { get; set; }
    
        public virtual SamplingRequest SamplingRequest1 { get; set; }
    }
}
