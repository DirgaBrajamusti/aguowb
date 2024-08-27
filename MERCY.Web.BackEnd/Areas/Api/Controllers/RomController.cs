using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

using System.Data;
using System.Data.SqlClient;

using MERCY.Web.BackEnd.Helpers;
using MERCY.Data.EntityFramework_BigData;

namespace MERCY.Web.BackEnd.Areas.Api.Controllers
{
    public class RomController : Controller
    {
        public JsonResult Index()
        {
            var controllerB = new ExternalDataController();
            controllerB.InitializeController(this.Request.RequestContext);

            return controllerB.GetROMs();
        }

        public JsonResult Get_ddl_All()
        {
            var controllerB = new ExternalDataController();
            controllerB.InitializeController(this.Request.RequestContext);

            return controllerB.GetROMs();
        }

        public JsonResult Get_ddl()
        {
            var controllerB = new ExternalDataController();
            controllerB.InitializeController(this.Request.RequestContext);

            return controllerB.GetROMsByCompany();
        }

        internal static bool Add(SqlConnection p_db, long p_samplingRequest, string p_roms, ref string p_message)
        {
            bool result = false;

            p_message = string.Empty;

            try
            {
                p_roms = System.Uri.UnescapeDataString(p_roms);
                p_roms = System.Uri.UnescapeDataString(p_roms);
                p_roms = System.Web.HttpUtility.HtmlDecode(p_roms);

                string[] roms = p_roms.Split(',');

                SqlCommand command = p_db.CreateCommand();
                string sql = string.Empty;
                string block = string.Empty;
                string romName = string.Empty;

                Dictionary<int, string> blocks = new Dictionary<int, string>();
                Dictionary<int, string> romNames = new Dictionary<int, string>();

                try
                {
                    using (MERCY_BigData_Ctx db = new MERCY_BigData_Ctx())
                    {
                        var dataQuery =
                                (
                                    from d in db.Mercy_ROM_Info
                                    select new
                                    {
                                        d.ROM_id
                                        , d.company_code
                                        , d.Block
                                        , d.ROM_Name
                                        , Names = (d.Block + " " + d.ROM_Name)
                                    }
                                );

                        var data = dataQuery.ToList();

                        data.ForEach(c =>
                        {
                            blocks.Add(c.ROM_id, c.Block);
                            romNames.Add(c.ROM_id, c.ROM_Name);
                        });
                    }
                }
                catch {}


                foreach (string rom in roms)
                {
                    block = string.Empty;
                    romName = string.Empty;
                    try
                    {
                        block = blocks[OurUtility.ToInt32(rom)];
                    }
                    catch {}
                    try
                    {
                        romName = romNames[OurUtility.ToInt32(rom)];
                    }
                    catch {}

                    try
                    {
                        sql = string.Format(@"insert into SamplingRequest_ROM(SamplingRequest, ROM_ID, Block, ROM_Name) values({0}, {1}, '{2}', '{3}')", p_samplingRequest, rom, block, romName);
                        command.CommandText = sql;
                        command.ExecuteNonQuery();
                    }
                    catch {}
                }

                result = true;
            }
            catch {}

            return result;
        }

        internal static bool Delete(SqlConnection p_db, long p_samplingRequest, ref string p_message)
        {
            bool result = false;

            p_message = string.Empty;

            try
            {
                SqlCommand command = p_db.CreateCommand();
                string sql = string.Format(@"delete from SamplingRequest_ROM where SamplingRequest = {0}", p_samplingRequest);
                command.CommandText = sql;
                command.ExecuteNonQuery();

                result = true;
            }
            catch {}

            return result;
        }

        public JsonResult GetROMsByBlock()
        {
            var controllerB = new ExternalDataController();
            controllerB.InitializeController(this.Request.RequestContext);

            return controllerB.GetROMsByBlock();
        }

        public JsonResult GetROMsByCompany()
        {
            var controllerB = new ExternalDataController();
            controllerB.InitializeController(this.Request.RequestContext);

            return controllerB.GetROMsByCompany();
        }
        
        public JsonResult GetROMsByCompanyBlock()
        {
            var controllerB = new ExternalDataController();
            controllerB.InitializeController(this.Request.RequestContext);

            return controllerB.GetROMsByCompanyBlock();
        }

        public JsonResult GetBlocks()
        {
            var controllerB = new ExternalDataController();
            controllerB.InitializeController(this.Request.RequestContext);

            return controllerB.GetBlocks();
        }

        public JsonResult GetBlocksByCompany()
        {
            var controllerB = new ExternalDataController();
            controllerB.InitializeController(this.Request.RequestContext);

            return controllerB.GetBlocksByCompany();
        }
    }
}