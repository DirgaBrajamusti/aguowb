//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace MERCY.Data.EntityFramework_MineMarket
{
    using System;
    using System.Collections.Generic;
    
    public partial class VW_MM_MERCY_VESSEL
    {
        public string SHIPMENT_IDENTIFIER { get; set; }
        public string VESSEL_NAME { get; set; }
        public string CUSTOMER_NAME { get; set; }
        public string CUSTOMER_LONG_NAME { get; set; }
        public string DO { get; set; }
        public Nullable<System.DateTime> ETA { get; set; }
        public Nullable<System.DateTime> ATA { get; set; }
        public string CONTRACT { get; set; }
        public string PRODUCT { get; set; }
        public Nullable<double> TON_ACTUAL { get; set; }
    }
}
