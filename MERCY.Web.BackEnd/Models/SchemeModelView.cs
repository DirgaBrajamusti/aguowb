using System.Collections.Generic;

namespace MERCY.Web.BackEnd.Models
{
    public class SchemeModelView
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Type { get; set; }
        public decimal? MinRepeatability { get; set; }
        public decimal? MaxRepeatability { get; set; }
        public bool IsRequired { get; set; }
        public string Details { get; set; }
        public bool IsCompleted { get; set; }
        public string AnalysisResultDetail { get; set; }
        public List<string> AnalysisResultDetails { get; set; }
        public decimal? Result { get; set; }
        public List<decimal?> Results { get; set; }
        public int AnalysisResultId { get; set; }
        
        public int? SchemeOrder { get; set; }
    }


    public class SchemeDetailModelView
    {
        public List<string> ExternalAttributes { get; set; }
        public List<SchemeDetailRuleModelView> Rules { get; set; }
        public List<SchemeDetailRuleModelView> RulesChild { get; set; }
    }


    public class SchemeDetailRuleModelView
    {
        public string Header { get; set; }
        public string Attribute { get; set; }
        public bool Input { get; set; }
        public SchemeDetailRuleFunctionModelView Fn { get; set; }
    }


    public class SchemeDetailRuleFunctionModelView
    {
        public string Arguments { get; set; }
        public string Body { get; set; }
    }
}