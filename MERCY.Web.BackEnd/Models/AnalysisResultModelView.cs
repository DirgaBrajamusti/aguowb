using System.Collections.Generic;

namespace MERCY.Web.BackEnd.Models
{
    public class AnalysisResultModelView
    {
        public string Type { get; set; }


        public string Ident { get; set; }


        public string ExtIdent { get; set; }


        public string Attributes { get; set; }


        public double Total { get; set; }


        public List<AnalysisResultModelView> Child { get; set; }
    }
}
