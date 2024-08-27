
using MERCY.Data.EntityFramework;

namespace MERCY.Web.BackEnd.Models
{
    public class Files : TEMPORARY_File
    {
        public string CreatedOn_Str { get; set; }
        public string CreatedBy_Str { get; set; }
    }
}