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
    
    public partial class PortionBlending_Details
    {
        public long RecordId { get; set; }
        public long PortionBlending { get; set; }
        public long ROMQuality_RecordId { get; set; }
        public string block { get; set; }
        public string ROM_Name { get; set; }
        public int ROM_ID { get; set; }
        public double CV { get; set; }
        public double TS { get; set; }
        public double ASH { get; set; }
        public double IM { get; set; }
        public double TM { get; set; }
        public decimal Ton { get; set; }
        public double Portion { get; set; }
    
        public virtual PortionBlending PortionBlending1 { get; set; }
    }
}