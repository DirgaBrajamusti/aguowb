//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace MERCY.ERD.Permissions
{
    using System;
    using System.Collections.Generic;
    
    public partial class Permission
    {
        public int PermissionId { get; set; }
        public int MenuId { get; set; }
        public int GroupId { get; set; }
        public bool IsView { get; set; }
        public bool IsAdd { get; set; }
        public bool IsDelete { get; set; }
        public bool IsUpdate { get; set; }
        public bool IsActive { get; set; }
        public int CreatedBy { get; set; }
        public System.DateTime CreatedOn { get; set; }
    
        public virtual Group Group { get; set; }
        public virtual Menu Menu { get; set; }
    }
}