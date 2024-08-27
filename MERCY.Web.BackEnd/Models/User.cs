using System;

using MERCY.Data.EntityFramework;

namespace MERCY.Web.BackEnd.Models
{
    public class Model_View_User : User
    {
        public string CreatedOn_Str { get; set; }
        public DateTime LastActivity { get; set; }
        public string LastActivity_Str { get; set; }
        public string Sites { get; set; }
        public string Companies { get; set; }
        public string Groups { get; set; }
        public int RecordId { get; set; }
    }
}