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
    
    public partial class Port_Loading
    {
        public int Record_Id { get; set; }
        public string CompanyCode { get; set; }
        public string Periode { get; set; }
        public string Type { get; set; }
        public int ProductId { get; set; }
        public string ProductName { get; set; }
        public double Tonnage { get; set; }
        public double CV { get; set; }
        public decimal TS { get; set; }
        public decimal ASH { get; set; }
        public decimal IM { get; set; }
        public decimal TM { get; set; }
        public int CreatedBy { get; set; }
        public System.DateTime CreatedOn { get; set; }
        public int LastModifiedBy { get; set; }
        public Nullable<System.DateTime> LastModifiedOn { get; set; }
        public bool IsActive { get; set; }
    
        public virtual Company Company { get; set; }
    }
}