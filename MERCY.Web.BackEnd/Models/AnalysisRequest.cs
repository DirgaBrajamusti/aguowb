
using MERCY.Data.EntityFramework;

namespace MERCY.Web.BackEnd.Models
{
    public class Model_View_AnalysisRequest : AnalysisRequest
    {
        public string CreatedOn_Str { get; set; }
        public string CreatedBy_Str { get; set; }
        public string DeliveryDate_Str { get; set; }
        public string DeliveryDate_Str2 { get; set; }
        public string EstimatedEndDate_Str { get; set; }
        public string EstimatedEndDate_Str2 { get; set; }
        public string Requestor { get; set; }
        public string Department { get; set; }
        public string Email { get; set; }
    }
}