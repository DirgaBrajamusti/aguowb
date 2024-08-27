
using MERCY.Data.EntityFramework;

namespace MERCY.Web.BackEnd.Models
{
    public class Model_View_SamplingRequest : SamplingRequest
    {
        public string CreatedOn_Str { get; set; }
        public string CreatedBy_Str { get; set; }
        public string RequestDate_Str { get; set; }
        public string RequestDate_Str2 { get; set; }
        public string Requestor { get; set; }
        public string Department { get; set; }
        public string Email { get; set; }
        public string Location { get; set; }
    }
}