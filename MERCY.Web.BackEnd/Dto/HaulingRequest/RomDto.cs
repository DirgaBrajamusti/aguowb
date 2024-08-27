using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MERCY.Web.BackEnd.Dto.HaulingRequest
{
    public class RomDto
    {
        public string RomName { get; set; }
        public decimal Ton { get; set; }
        public int Shift { get; set; }
    }
}