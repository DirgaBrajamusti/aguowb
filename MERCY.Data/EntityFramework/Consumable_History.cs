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
    
    public partial class Consumable_History
    {
        public long RecordId { get; set; }
        public long Consumable_Data { get; set; }
        public int Instrument { get; set; }
        public string PartName { get; set; }
        public string PartNumber { get; set; }
        public string MSDS_Code { get; set; }
        public int CurrentQuantity { get; set; }
        public int MinimumQuantity { get; set; }
        public int InputQuantity { get; set; }
        public int OutputQuantity { get; set; }
        public int UnitType { get; set; }
        public int ReoderDays { get; set; }
        public double Price { get; set; }
        public string NotesFile { get; set; }
        public string NotesFile2 { get; set; }
        public string PictureFile { get; set; }
        public string PictureFile2 { get; set; }
        public string Remark { get; set; }
        public int CreatedBy { get; set; }
        public System.DateTime CreatedOn { get; set; }
        public int LastModifiedBy { get; set; }
        public Nullable<System.DateTime> LastModifiedOn { get; set; }
        public string Description { get; set; }
    }
}
