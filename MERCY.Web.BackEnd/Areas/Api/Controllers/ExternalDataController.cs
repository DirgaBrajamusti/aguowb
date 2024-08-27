using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using MERCY.Web.BackEnd.Models;
using MERCY.Web.BackEnd.Helpers;
using MERCY.Web.BackEnd.Security;
using Permission = MERCY.Web.BackEnd.Security.Permission;

using MERCY.Data.EntityFramework_BigData;
using MERCY.Data.EntityFramework_BigDataBEK;
using MERCY.Data.EntityFramework_PowerBI;
using MERCY.Data.EntityFramework;

namespace MERCY.Web.BackEnd.Areas.Api.Controllers
{
    public class ExternalDataController : Controller
    {
        public void InitializeController(System.Web.Routing.RequestContext context)
        {
            base.Initialize(context);
        }

        public JsonResult GetSeams()
        {
            // Check Permission based on Current Url
            UserX user = new UserX(Request);
            Permission_Item permission_Item = null;

            Permission.Check_API(Request, user, ref permission_Item);
            if (!permission_Item.Is_View)
            {
                var msg = new { Success = false, Permission = permission_Item, Message = Permission.ERROR_PERMISSION_READ, MessageDetail = string.Empty, Version = Configuration.VERSION };
                return Json(msg, JsonRequestBehavior.AllowGet);
            }

            // -- Actual code
            try
            {
                using (MERCY_BigData_Ctx db = new MERCY_BigData_Ctx())
                {
                    var dataQueryX =
                            (
                                from d in db.Mercy_Source
                                where d.seam != ""
                                orderby d.Company_code, d.pit, d.seam
                                select new
                                {
                                    d.Company_code
                                    ,
                                    d.pit
                                    ,
                                    d.seam
                                    ,
                                    Names = (d.Company_code + "__" + d.pit + "__" + d.seam)
                                }
                            );

                    var itemsX = dataQueryX.Distinct().ToList();
                    var dataQuery =
                            (
                                from d in itemsX
                                orderby d.Company_code, d.pit, d.seam
                                select d
                            );
                    var items = dataQuery.ToList();

                    var result = new { Success = true, Permission = permission_Item, Message = string.Empty, MessageDetail = string.Empty, Version = Configuration.VERSION, Items = items, Total = items.Count };
                    return Json(result, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                var result = new { Success = false, Permission = permission_Item, ex.Message, MessageDetail = string.Empty, Version = Configuration.VERSION, Items = string.Empty, Total = 0 };
                return Json(result, JsonRequestBehavior.AllowGet);
            }
        }

        public JsonResult GetSeamsByPit()
        {
            // Check Permission based on Current Url
            UserX user = new UserX(Request);
            Permission_Item permission_Item = null;

            Permission.Check_API(Request, user, ref permission_Item);
            if (!permission_Item.Is_View)
            {
                var msg = new { Success = false, Permission = permission_Item, Message = Permission.ERROR_PERMISSION_READ, MessageDetail = string.Empty, Version = Configuration.VERSION };
                return Json(msg, JsonRequestBehavior.AllowGet);
            }

            // -- Actual code
            string pit = Request["pit"];
            try
            {
                using (MERCY_BigData_Ctx db = new MERCY_BigData_Ctx())
                {
                    var dataQueryX =
                            (
                                from d in db.Mercy_Source
                                where d.seam != ""
                                        && d.pit == pit
                                orderby d.Company_code, d.pit, d.seam
                                select new
                                {
                                    d.Company_code
                                    ,
                                    d.pit
                                    ,
                                    d.seam
                                    ,
                                    Names = (d.Company_code + "__" + d.pit + "__" + d.seam)
                                }
                            );

                    var itemsX = dataQueryX.Distinct().ToList();
                    var dataQuery =
                            (
                                from d in itemsX
                                orderby d.Company_code, d.pit, d.seam
                                select d
                            );
                    var items = dataQuery.ToList();

                    var result = new { Success = true, Permission = permission_Item, Message = string.Empty, MessageDetail = string.Empty, Version = Configuration.VERSION, Items = items, Total = items.Count };
                    return Json(result, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                var result = new { Success = false, Permission = permission_Item, ex.Message, MessageDetail = string.Empty, Version = Configuration.VERSION, Items = string.Empty, Total = 0 };
                return Json(result, JsonRequestBehavior.AllowGet);
            }
        }

        public JsonResult GetSeamsByCompany()
        {
            // Check Permission based on Current Url
            UserX user = new UserX(Request);
            Permission_Item permission_Item = null;

            Permission.Check_API(Request, user, ref permission_Item);
            if (!permission_Item.Is_View)
            {
                var msg = new { Success = false, Permission = permission_Item, Message = Permission.ERROR_PERMISSION_READ, MessageDetail = string.Empty, Version = Configuration.VERSION };
                return Json(msg, JsonRequestBehavior.AllowGet);
            }

            // -- Actual code
            string company = Request["company"];
            try
            {
                var itemsX = new List<ExternalSeamModelView>();
                if (company == "TCM")
                {
                    using (MERCY_BigData_Ctx db = new MERCY_BigData_Ctx())
                    {
                        var dataQueryX =
                                (
                                    from d in db.Mercy_Source
                                    where d.seam != ""
                                            && d.Company_code == company
                                    orderby d.pit, d.seam
                                    select new ExternalSeamModelView
                                    {
                                        Company_code = d.Company_code,
                                        pit = d.pit,
                                        seam = d.seam,
                                        Names = (d.Company_code + "__" + d.pit + "__" + d.seam)
                                    }
                                );

                        itemsX = dataQueryX.Distinct().ToList();
                    }
                }
                else if (company == "BEK")
                {
                    using (MERCY_BigDataBEK_Ctx db = new MERCY_BigDataBEK_Ctx())
                    {
                        var dataQueryX =
                                (
                                    from d in db.MERCY_SOURCE_NEW
                                    where d.SEAM != ""
                                            && d.COMPANY_CODE == company
                                    orderby d.PIT
                                    select new ExternalSeamModelView
                                    {
                                        Company_code = d.COMPANY_CODE,
                                        pit = d.PIT,
                                        seam = d.SEAM,
                                        Names = (d.COMPANY_CODE + "__" + d.PIT + "__" + d.SEAM)
                                    }
                                );

                        itemsX = dataQueryX.Distinct().ToList();
                    }
                }

                var dataQuery =
                        (
                            from d in itemsX
                            orderby d.pit, d.seam
                            select d
                        );
                var items = dataQuery.ToList();

                var result = new { Success = true, Permission = permission_Item, Message = string.Empty, MessageDetail = string.Empty, Version = Configuration.VERSION, Items = items, Total = items.Count };
                return Json(result, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                var result = new { Success = false, Permission = permission_Item, ex.Message, MessageDetail = string.Empty, Version = Configuration.VERSION, Items = string.Empty, Total = 0 };
                return Json(result, JsonRequestBehavior.AllowGet);
            }
        }

        public JsonResult GetSeamsByCompanyAndPit()
        {
            // Check Permission based on Current Url
            UserX user = new UserX(Request);
            Permission_Item permission_Item = null;

            Permission.Check_API(Request, user, ref permission_Item);
            if (!permission_Item.Is_View)
            {
                var msg = new { Success = false, Permission = permission_Item, Message = Permission.ERROR_PERMISSION_READ, MessageDetail = string.Empty, Version = Configuration.VERSION };
                return Json(msg, JsonRequestBehavior.AllowGet);
            }

            // -- Actual code
            string company = Request["company"];
            string pit = Request["pit"];
            try
            {
                var itemsX = new List<ExternalSeamModelView>();
                if (company == "TCM")
                {
                    using (MERCY_BigData_Ctx db = new MERCY_BigData_Ctx())
                    {
                        var dataQueryX =
                                (
                                    from d in db.Mercy_Source
                                    where d.seam != ""
                                            && d.Company_code == company
                                            && d.pit == pit
                                    orderby d.seam
                                    select new ExternalSeamModelView
                                    {
                                        Company_code = d.Company_code,
                                        pit = d.pit,
                                        seam = d.seam,
                                        Names = (d.Company_code + "__" + d.pit + "__" + d.seam)
                                    }
                                );

                        itemsX = dataQueryX.Distinct().ToList();
                    }
                }
                else if (company == "BEK")
                {
                    using (MERCY_BigDataBEK_Ctx db = new MERCY_BigDataBEK_Ctx())
                    {
                        var dataQueryX =
                                (
                                    from d in db.MERCY_SOURCE_NEW
                                    where d.SEAM != ""
                                            && d.COMPANY_CODE == company
                                            && d.PIT == pit
                                    orderby d.PIT
                                    select new ExternalSeamModelView
                                    {
                                        Company_code = d.COMPANY_CODE,
                                        pit = d.PIT,
                                        seam = d.SEAM,
                                        Names = (d.COMPANY_CODE + "__" + d.PIT + "__" + d.SEAM)
                                    }
                                );

                        itemsX = dataQueryX.Distinct().ToList();
                    }
                }

                var dataQuery =
                        (
                            from d in itemsX
                            orderby d.seam
                            select d
                        );
                var items = dataQuery.ToList();

                var result = new { Success = true, Permission = permission_Item, Message = string.Empty, MessageDetail = string.Empty, Version = Configuration.VERSION, Items = items, Total = items.Count };
                return Json(result, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                var result = new { Success = false, Permission = permission_Item, ex.Message, MessageDetail = string.Empty, Version = Configuration.VERSION, Items = string.Empty, Total = 0 };
                return Json(result, JsonRequestBehavior.AllowGet);
            }
        }

        public JsonResult GetPits()
        {
            // Check Permission based on Current Url
            UserX user = new UserX(Request);
            Permission_Item permission_Item = null;

            Permission.Check_API(Request, user, ref permission_Item);
            if (!permission_Item.Is_View)
            {
                var msg = new { Success = false, Permission = permission_Item, Message = Permission.ERROR_PERMISSION_READ, MessageDetail = string.Empty, Version = Configuration.VERSION };
                return Json(msg, JsonRequestBehavior.AllowGet);
            }

            // -- Actual code
            try
            {
                using (MERCY_BigData_Ctx db = new MERCY_BigData_Ctx())
                {
                    var dataQueryX =
                            (
                                from d in db.Mercy_Source
                                orderby d.Company_code, d.pit
                                select new
                                {
                                    d.Company_code
                                    ,
                                    d.pit
                                    ,
                                    Names = (d.Company_code + "__" + d.pit)
                                }
                            );

                    var itemsX = dataQueryX.Distinct().ToList();
                    var dataQuery =
                            (
                                from d in itemsX
                                orderby d.Company_code, d.pit
                                select d
                            );
                    var items = dataQuery.ToList();

                    var result = new { Success = true, Permission = permission_Item, Message = string.Empty, MessageDetail = string.Empty, Version = Configuration.VERSION, Items = items, Total = items.Count };
                    return Json(result, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                var result = new { Success = false, Permission = permission_Item, ex.Message, MessageDetail = string.Empty, Version = Configuration.VERSION, Items = string.Empty, Total = 0 };
                return Json(result, JsonRequestBehavior.AllowGet);
            }
        }

        public JsonResult GetPitsByCompany()
        {
            // Check Permission based on Current Url
            UserX user = new UserX(Request);
            Permission_Item permission_Item = null;

            Permission.Check_API(Request, user, ref permission_Item);
            if (!permission_Item.Is_View)
            {
                var msg = new { Success = false, Permission = permission_Item, Message = Permission.ERROR_PERMISSION_READ, MessageDetail = string.Empty, Version = Configuration.VERSION };
                return Json(msg, JsonRequestBehavior.AllowGet);
            }

            // -- Actual code
            string company = Request["company"];

            try
            {
                var itemsX = new List<ExternalPitModelView>();
                if (company == "TCM")
                {
                    using (MERCY_BigData_Ctx db = new MERCY_BigData_Ctx())
                    {
                        var dataQueryX =
                                (
                                    from d in db.Mercy_Source
                                    where d.Company_code == company
                                    orderby d.pit
                                    select new ExternalPitModelView
                                    {
                                        Company_code = d.Company_code,
                                        pit = d.pit,
                                        Names = (d.Company_code + "__" + d.pit)
                                    }
                                );

                        itemsX = dataQueryX.Distinct().ToList();
                    }
                }
                else if (company == "BEK")
                {
                    using (MERCY_BigDataBEK_Ctx db = new MERCY_BigDataBEK_Ctx())
                    {
                        var dataQueryX =
                                (
                                    from d in db.MERCY_SOURCE_NEW
                                    where d.COMPANY_CODE == company
                                    orderby d.PIT
                                    select new ExternalPitModelView
                                    {
                                        Company_code = d.COMPANY_CODE,
                                        pit = d.PIT,
                                        Names = (d.COMPANY_CODE + "__" + d.PIT)
                                    }
                                );

                        itemsX = dataQueryX.Distinct().ToList();
                    }
                }

                var dataQuery =
                        (
                            from d in itemsX
                            orderby d.pit
                            select d
                        );
                var items = dataQuery.ToList();

                var result = new { Success = true, Permission = permission_Item, Message = string.Empty, MessageDetail = string.Empty, Version = Configuration.VERSION, Items = items, Total = items.Count };
                return Json(result, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                var result = new { Success = false, Permission = permission_Item, ex.Message, MessageDetail = string.Empty, Version = Configuration.VERSION, Items = string.Empty, Total = 0 };
                return Json(result, JsonRequestBehavior.AllowGet);
            }
        }

        public JsonResult GetROMs()
        {
            // Check Permission based on Current Url
            UserX user = new UserX(Request);
            Permission_Item permission_Item = null;

            Permission.Check_API(Request, user, ref permission_Item);
            if (!permission_Item.Is_View)
            {
                var msg = new { Success = false, Permission = permission_Item, Message = Permission.ERROR_PERMISSION_READ, MessageDetail = string.Empty, Version = Configuration.VERSION };
                return Json(msg, JsonRequestBehavior.AllowGet);
            }

            // -- Actual code
            try
            {
                using (MERCY_BigData_Ctx db = new MERCY_BigData_Ctx())
                {
                    var dataQuery =
                            (
                                from d in db.Mercy_ROM_Info
                                orderby d.company_code, d.Block, d.ROM_Name
                                select new
                                {
                                    d.ROM_id
                                    ,
                                    d.company_code
                                    ,
                                    d.Block
                                    ,
                                    d.ROM_Name
                                    ,
                                    Names = (d.Block + " " + d.ROM_Name)
                                }
                            );

                    var items = dataQuery.ToList();

                    var result = new { Success = true, Permission = permission_Item, Message = string.Empty, MessageDetail = string.Empty, Version = Configuration.VERSION, Items = items, Total = items.Count };
                    return Json(result, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                var result = new { Success = false, Permission = permission_Item, ex.Message, MessageDetail = string.Empty, Version = Configuration.VERSION, Items = string.Empty, Total = 0 };
                return Json(result, JsonRequestBehavior.AllowGet);
            }
        }

        public JsonResult GetROMsByCompany()
        {
            // Check Permission based on Current Url
            UserX user = new UserX(Request);
            Permission_Item permission_Item = null;

            Permission.Check_API(Request, user, ref permission_Item);
            if (!permission_Item.Is_View)
            {
                var msg = new { Success = false, Permission = permission_Item, Message = Permission.ERROR_PERMISSION_READ, MessageDetail = string.Empty, Version = Configuration.VERSION };
                return Json(msg, JsonRequestBehavior.AllowGet);
            }

            // -- Actual code
            string company = Request["company"];
            try
            {
                var items = new List<ExternalRomModelView>();
                if (company == "TCM")
                {
                    using (MERCY_BigData_Ctx db = new MERCY_BigData_Ctx())
                    {
                        var dataQuery =
                                (
                                    from d in db.Mercy_ROM_Info
                                    where d.company_code == company
                                    orderby d.Block, d.ROM_Name
                                    select new ExternalRomModelView
                                    {
                                        ROM_id = d.ROM_id,
                                        company_code = d.company_code,
                                        Block = d.Block,
                                        ROM_Name = d.ROM_Name,
                                        Names = (d.Block + " " + d.ROM_Name)
                                    }
                                );

                        items = dataQuery.ToList();
                    }
                }
                else if (company == "BEK")
                {
                    using (MERCY_BigDataBEK_Ctx db = new MERCY_BigDataBEK_Ctx())
                    {
                        var dataQuery =
                                (
                                    from d in db.MERCY_ROM_INFO
                                    where d.COMPANYCODE == company
                                    orderby d.BLOCK, d.ROM_NAME
                                    select new ExternalRomModelView
                                    {
                                        ROM_id = d.ID,
                                        company_code = d.COMPANYCODE,
                                        Block = d.BLOCK,
                                        ROM_Name = d.ROM_NAME,
                                        Names = (d.BLOCK + " " + d.ROM_NAME)
                                    }
                                );

                        items = dataQuery.ToList();
                    }
                }

                var result = new { Success = true, Permission = permission_Item, Message = string.Empty, MessageDetail = string.Empty, Version = Configuration.VERSION, Items = items, Total = items.Count };
                return Json(result, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                var result = new { Success = false, Permission = permission_Item, ex.Message, MessageDetail = string.Empty, Version = Configuration.VERSION, Items = string.Empty, Total = 0 };
                return Json(result, JsonRequestBehavior.AllowGet);
            }
        }

        public JsonResult GetROMsByBlock()
        {
            // Check Permission based on Current Url
            UserX user = new UserX(Request);
            Permission_Item permission_Item = null;

            Permission.Check_API(Request, user, ref permission_Item);
            if (!permission_Item.Is_View)
            {
                var msg = new { Success = false, Permission = permission_Item, Message = Permission.ERROR_PERMISSION_READ, MessageDetail = string.Empty, Version = Configuration.VERSION };
                return Json(msg, JsonRequestBehavior.AllowGet);
            }

            // -- Actual code
            string block = Request["block"];
            try
            {
                using (MERCY_BigData_Ctx db = new MERCY_BigData_Ctx())
                {
                    var dataQueryX =
                            (
                                from d in db.Mercy_ROM_Info
                                where d.Block == block
                                orderby d.ROM_Name
                                select new
                                {
                                    d.ROM_id
                                    ,
                                    d.ROM_Name
                                }
                            );

                    var itemsX = dataQueryX.Distinct().ToList();
                    var dataQuery =
                            (
                                from d in itemsX
                                orderby d.ROM_Name
                                select d
                            );
                    var items = dataQuery.ToList();

                    var result = new { Success = true, Permission = permission_Item, Message = string.Empty, MessageDetail = string.Empty, Version = Configuration.VERSION, Items = items, Total = items.Count };
                    return Json(result, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                var result = new { Success = false, Permission = permission_Item, ex.Message, MessageDetail = string.Empty, Version = Configuration.VERSION, Items = string.Empty, Total = 0 };
                return Json(result, JsonRequestBehavior.AllowGet);
            }
        }

        public JsonResult GetROMsByCompanyBlock()
        {
            // Check Permission based on Current Url
            UserX user = new UserX(Request);
            Permission_Item permission_Item = null;

            Permission.Check_API(Request, user, ref permission_Item);
            if (!permission_Item.Is_View)
            {
                var msg = new { Success = false, Permission = permission_Item, Message = Permission.ERROR_PERMISSION_READ, MessageDetail = string.Empty, Version = Configuration.VERSION };
                return Json(msg, JsonRequestBehavior.AllowGet);
            }

            // -- Actual code
            string company = Request["company"];
            string block = Request["block"];
            try
            {
                var itemsX = new List<ExternalRomSimpleModelView>();
                if (company == "TCM")
                {
                    using (MERCY_BigData_Ctx db = new MERCY_BigData_Ctx())
                    {
                        var dataQueryX =
                                (
                                    from d in db.Mercy_ROM_Info
                                    where d.company_code == company && d.Block == block
                                    orderby d.Block, d.ROM_Name
                                    select new ExternalRomSimpleModelView
                                    {
                                        ROM_id = d.ROM_id,
                                        ROM_Name = d.ROM_Name
                                    }
                                );

                        itemsX = dataQueryX.Distinct().ToList();
                    }
                }
                else if (company == "BEK")
                {
                    using (MERCY_BigDataBEK_Ctx db = new MERCY_BigDataBEK_Ctx())
                    {
                        var dataQueryX =
                                (
                                    from d in db.MERCY_ROM_INFO
                                    where d.COMPANYCODE == company && d.BLOCK == block
                                    orderby d.BLOCK, d.ROM_NAME
                                    select new ExternalRomSimpleModelView
                                    {
                                        ROM_id = d.ID,
                                        ROM_Name = d.ROM_NAME
                                    }
                                );

                        itemsX = dataQueryX.Distinct().ToList();
                    }
                }

                var dataQuery =
                        (
                            from d in itemsX
                            orderby d.ROM_Name
                            select d
                        );
                var items = dataQuery.ToList();

                var result = new { Success = true, Permission = permission_Item, Message = string.Empty, MessageDetail = string.Empty, Version = Configuration.VERSION, Items = items, Total = items.Count };
                return Json(result, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                var result = new { Success = false, Permission = permission_Item, ex.Message, MessageDetail = string.Empty, Version = Configuration.VERSION, Items = string.Empty, Total = 0 };
                return Json(result, JsonRequestBehavior.AllowGet);
            }
        }

        public JsonResult GetBlocks()
        {
            // Check Permission based on Current Url
            UserX user = new UserX(Request);
            Permission_Item permission_Item = null;

            Permission.Check_API(Request, user, ref permission_Item);
            if (!permission_Item.Is_View)
            {
                var msg = new { Success = false, Permission = permission_Item, Message = Permission.ERROR_PERMISSION_READ, MessageDetail = string.Empty, Version = Configuration.VERSION };
                return Json(msg, JsonRequestBehavior.AllowGet);
            }

            // -- Actual code
            try
            {
                using (MERCY_BigData_Ctx db = new MERCY_BigData_Ctx())
                {
                    var dataQueryX =
                            (
                                from d in db.Mercy_ROM_Info
                                orderby d.company_code, d.Block
                                select new
                                {
                                    d.company_code
                                    ,
                                    d.Block
                                }
                            );

                    var itemsX = dataQueryX.Distinct().ToList();
                    var dataQuery =
                            (
                                from d in itemsX
                                orderby d.company_code, d.Block
                                select d
                            );
                    var items = dataQuery.ToList();

                    var result = new { Success = true, Permission = permission_Item, Message = string.Empty, MessageDetail = string.Empty, Version = Configuration.VERSION, Items = items, Total = items.Count };
                    return Json(result, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                var result = new { Success = false, Permission = permission_Item, ex.Message, MessageDetail = string.Empty, Version = Configuration.VERSION, Items = string.Empty, Total = 0 };
                return Json(result, JsonRequestBehavior.AllowGet);
            }
        }

        public JsonResult GetBlocksByCompany()
        {
            // Check Permission based on Current Url
            UserX user = new UserX(Request);
            Permission_Item permission_Item = null;

            Permission.Check_API(Request, user, ref permission_Item);
            if (!permission_Item.Is_View)
            {
                var msg = new { Success = false, Permission = permission_Item, Message = Permission.ERROR_PERMISSION_READ, MessageDetail = string.Empty, Version = Configuration.VERSION };
                return Json(msg, JsonRequestBehavior.AllowGet);
            }

            // -- Actual code
            string company = Request["company"];
            try
            {
                var itemsX = new List<ExternalBlockSimpleModelView>();
                if (company == "TCM")
                {
                    using (MERCY_BigData_Ctx db = new MERCY_BigData_Ctx())
                    {
                        var dataQueryX =
                                (
                                    from d in db.Mercy_ROM_Info
                                    where d.company_code == company
                                    orderby d.company_code, d.Block
                                    select new ExternalBlockSimpleModelView
                                    {
                                        company_code = d.company_code,
                                        Block = d.Block
                                    }
                                );

                        itemsX = dataQueryX.Distinct().ToList();
                    }
                }
                else if (company == "BEK")
                {
                    using (MERCY_BigDataBEK_Ctx db = new MERCY_BigDataBEK_Ctx())
                    {
                        var dataQueryX =
                                (
                                    from d in db.MERCY_ROM_INFO
                                    where d.COMPANYCODE == company
                                    orderby d.COMPANYCODE, d.BLOCK
                                    select new ExternalBlockSimpleModelView
                                    {
                                        company_code = d.COMPANYCODE,
                                        Block = d.BLOCK
                                    }
                                );

                        itemsX = dataQueryX.Distinct().ToList();
                    }
                }

                var dataQuery =
                        (
                            from d in itemsX
                            orderby d.company_code, d.Block
                            select d
                        );
                var items = dataQuery.ToList();

                var result = new { Success = true, Permission = permission_Item, Message = string.Empty, MessageDetail = string.Empty, Version = Configuration.VERSION, Items = items, Total = items.Count };
                return Json(result, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                var result = new { Success = false, Permission = permission_Item, ex.Message, MessageDetail = string.Empty, Version = Configuration.VERSION, Items = string.Empty, Total = 0 };
                return Json(result, JsonRequestBehavior.AllowGet);
            }
        }

        public JsonResult GetROMsQuality()
        {
            // Check Permission based on Current Url
            UserX user = new UserX(Request);
            Permission_Item permission_Item = null;

            Permission.Check_API(Request, user, ref permission_Item);
            if (!permission_Item.Is_View)
            {
                var msg = new { Success = false, Permission = permission_Item, Message = Permission.ERROR_PERMISSION_READ, MessageDetail = string.Empty, Version = Configuration.VERSION };
                return Json(msg, JsonRequestBehavior.AllowGet);
            }

            // -- Actual code
            string company = Request["company"];

            try
            {
                var items = new List<Model_View_Mercy_ROM_Quality>();
                if (company == "TCM" || company == "BEK")
                {
                    using (MERCY_PowerBI_Ctx db = new MERCY_PowerBI_Ctx())
                    {
                        items =
                                (
                                    from d in db.MERCY_ROM_QUALITY_ODS
                                    where d.COMPANY_CODE == company
                                    select new Model_View_Mercy_ROM_Quality
                                    {
                                        Block = d.BLOCK,
                                        ROM_Name = d.ROM_NAME,
                                        ROM_Id = d.ROM_ID,
                                        Ton = d.TON,
                                        CV = d.CV,
                                        TS = d.TS,
                                        ASH = d.ASH,
                                        IM = d.IM,
                                        TM = d.TM,
                                        Process_Date = d.PROCESS_DATE,
                                        company_code = d.COMPANY_CODE,
                                        Names = string.Empty
                                    }
                                ).ToList();
                    }

                }
                else
                {
                    throw new Exception("Company does not have Big Data connection");
                }

                items.ForEach(c =>
                {
                    c.Process_Date_Str = OurUtility.DateFormat(c.Process_Date, "dd-MMM-yyyy HH:mm");
                    c.Ton_Str = string.Format("{0:N2}", OurUtility.Round(c.Ton, 2));
                    c.CV_Str = string.Format("{0:N0}", OurUtility.Round(c.CV, 0));
                    c.TS_Str = string.Format("{0:N2}", OurUtility.Round(c.TS, 2));
                    c.ASH_Str = string.Format("{0:N2}", OurUtility.Round(c.ASH, 2));
                    c.IM_Str = string.Format("{0:N2}", OurUtility.Round(c.IM, 2));
                    c.TM_Str = string.Format("{0:N2}", OurUtility.Round(c.TM, 2));

                    try
                    {
                        c.Names = (c.Block + " " + c.ROM_Name);
                    }
                    catch { }
                });

                List<Model_View_Mercy_ROM_Quality> sorted_Items = items.OrderBy(x => x.Names).ToList();

                var result = new { Success = true, Permission = permission_Item, Message = string.Empty, MessageDetail = string.Empty, Version = Configuration.VERSION, Items = sorted_Items, Total = sorted_Items.Count };
                return Json(result, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                var result = new { Success = false, Permission = permission_Item, ex.Message, MessageDetail = string.Empty, Version = Configuration.VERSION, Items = string.Empty, Total = 0 };
                return Json(result, JsonRequestBehavior.AllowGet);
            }
        }

        public JsonResult GetProduct()
        {
            // Check Permission based on Current Url
            UserX user = new UserX(Request);
            Permission_Item permission_Item = null;

            Permission.Check_API(Request, user, ref permission_Item);
            if (!permission_Item.Is_View)
            {
                var msg = new { Success = false, Permission = permission_Item, Message = Permission.ERROR_PERMISSION_READ, MessageDetail = string.Empty, Version = Configuration.VERSION };
                return Json(msg, JsonRequestBehavior.AllowGet);
            }

            // -- Actual code
            DateTime now = DateTime.Now;
            // -- Actual code
            string company = Request["company"];
            try
            {
                var items = new List<Model_Product>();
                if (company == "TCM" || string.IsNullOrEmpty(company))
                {
                    string search = OurUtility.ValueOf(Request, "search");
                    bool isAllSearch = string.IsNullOrEmpty(search);

                    using (MERCY_BigData_Ctx db = new MERCY_BigData_Ctx())
                    {
                        items = items.Concat(
                                (
                                    from d in db.Mercy_quality_outlook
                                    where d.FirstDate.Value.Month == now.Month
                                            && d.FirstDate.Value.Year == now.Year
                                            && (isAllSearch || d.Product_name.Contains(search))
                                    orderby d.Product_name
                                    select new Model_Product
                                    {
                                        id = d.id,
                                        Product_name = d.Product_name,
                                        CompanyCode = company,
                                        FirstDate = d.FirstDate
                                    }
                                ).Distinct().ToList()).ToList();
                    }
                }

                if (company == "BEK" || string.IsNullOrEmpty(company))
                {
                    using (MERCY_BigDataBEK_Ctx db = new MERCY_BigDataBEK_Ctx())
                    {
                        items = items.Concat(
                                (
                                    from d in db.MERCY_QUALITY_OUTLOOK
                                    where d.FirstDate.Value.Month == now.Month
                                            && d.FirstDate.Value.Year == now.Year
                                    orderby d.Product_name
                                    select new Model_Product
                                    {
                                        id = d.id,
                                        Product_name = d.Product_name,
                                        CompanyCode = company,
                                        FirstDate = d.FirstDate
                                    }
                                ).Distinct().ToList()).ToList();
                    }
                }

                if (company != "BEK" && company != "TCM")
                {
                    throw new Exception("Company does not have Big Data connection");
                }

                var result = new { Success = true, Permission = permission_Item, Message = string.Empty, MessageDetail = string.Empty, Version = Configuration.VERSION, Items = items, Total = items.Count };
                return Json(result, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                var result = new { Success = false, Permission = permission_Item, ex.Message, MessageDetail = string.Empty, Version = Configuration.VERSION, Items = string.Empty, Total = 0 };
                return Json(result, JsonRequestBehavior.AllowGet);
            }
        }

        public JsonResult GetProduct_byName()
        {
            // Check Permission based on Current Url
            UserX user = new UserX(Request);
            Permission_Item permission_Item = null;

            Permission.Check_API(Request, user, ref permission_Item);
            if (!permission_Item.Is_View)
            {
                var msg = new { Success = false, Permission = permission_Item, Message = Permission.ERROR_PERMISSION_READ, MessageDetail = string.Empty, Version = Configuration.VERSION };
                return Json(msg, JsonRequestBehavior.AllowGet);
            }

            // -- Actual code
            string productName = Request["productName"];

            try
            {
                using (MERCY_BigData_Ctx db = new MERCY_BigData_Ctx())
                {
                    var dataQueryX =
                            (
                                from d in db.Mercy_quality_outlook
                                where d.Product_name == productName
                                orderby d.id descending
                                select new Model_View_Mercy_quality_outlook
                                {
                                    id = d.id
                                    ,
                                    Product_name = d.Product_name
                                    ,
                                    FirstDate = d.FirstDate
                                    ,
                                    Year = d.Year
                                    ,
                                    months = d.months
                                    ,
                                    CV = d.CV
                                    ,
                                    TS = d.TS
                                    ,
                                    ASH = d.ash
                                    ,
                                    TM = d.TM
                                    ,
                                    IM = d.IM
                                }
                            );

                    var items = dataQueryX.ToList();

                    double cv = 0.0;

                    items.ForEach(c =>
                    {
                        cv = OurUtility.Round(c.CV, 0);
                        c.CV_Str = string.Format("{0:N0}", cv);
                        c.TS_Str = string.Format("{0:N2}", c.TS);
                        c.ASH_Str = string.Format("{0:N2}", c.ASH);
                        c.IM_Str = string.Format("{0:N2}", c.IM);
                        c.TM_Str = string.Format("{0:N2}", c.TM);
                    });

                    var item = items.Take(1);

                    var result = new { Success = true, Permission = permission_Item, Message = string.Empty, MessageDetail = string.Empty, Version = Configuration.VERSION, Item = item };
                    return Json(result, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                var result = new { Success = false, Permission = permission_Item, ex.Message, MessageDetail = string.Empty, Version = Configuration.VERSION, Items = string.Empty, Total = 0 };
                return Json(result, JsonRequestBehavior.AllowGet);
            }
        }

        public JsonResult SynchronizeProduct()
        {
            // Check Permission based on Current Url
            UserX user = new UserX(Request);
            Permission_Item permission_Item = null;

            Permission.Check_API(Request, user, ref permission_Item);
            if (!permission_Item.Is_Add)
            {
                var msg = new { Success = false, Permission = permission_Item, Message = Permission.ERROR_PERMISSION_READ, MessageDetail = string.Empty, Version = Configuration.VERSION };
                return Json(msg, JsonRequestBehavior.AllowGet);
            }

            // -- Actual code
            DateTime now = DateTime.Now;
            // -- Actual code
            string company = Request["company"];
            try
            {
                var products = new List<Model_Product>();

                var controller_Company = new CompanyController();
                controller_Company.InitializeController(this.Request.RequestContext);
                if (controller_Company.Is_from_BigData(OurUtility.ValueOf(Request, "company")))
                {
                    if (company == "TCM" || string.IsNullOrEmpty(company))
                    {

                        using (MERCY_BigData_Ctx db = new MERCY_BigData_Ctx())
                        {
                            products = products.Concat(
                                    (
                                        from d in db.Mercy_quality_outlook
                                        where d.FirstDate.Value.Month == now.Month
                                                && d.FirstDate.Value.Year == now.Year
                                        orderby d.Product_name
                                        select new Model_Product
                                        {
                                            id = d.id,
                                            Product_name = d.Product_name,
                                            CompanyCode = company,
                                            FirstDate = d.FirstDate,
                                            CV = d.CV,
                                            TS = d.TS,
                                            ASH = d.ash,
                                            IM = d.IM,
                                            TM = d.TM
                                        }
                                    ).Distinct().ToList()).ToList();
                        }
                    }

                    if (company == "BEK" || string.IsNullOrEmpty(company))
                    {
                        using (MERCY_BigDataBEK_Ctx db = new MERCY_BigDataBEK_Ctx())
                        {
                            products = products.Concat(
                                    (
                                        from d in db.MERCY_QUALITY_OUTLOOK
                                        where d.FirstDate.Value.Month == now.Month
                                                && d.FirstDate.Value.Year == now.Year
                                        orderby d.Product_name
                                        select new Model_Product
                                        {
                                            id = d.id,
                                            Product_name = d.Product_name,
                                            CompanyCode = company,
                                            FirstDate = d.FirstDate,
                                            CV = d.CV,
                                            TS = d.TS,
                                            ASH = d.ash,
                                            IM = d.IM,
                                            TM = d.TM
                                        }
                                    ).Distinct().ToList()).ToList();
                        }
                    }
                }

                using (MERCY_Ctx db = new MERCY_Ctx())
                {
                    var dataQueryX =
                            (
                                from d in db.Products
                                where d.CompanyCode == company
                                orderby d.ProductName
                                select new Model_Product
                                {
                                    id = d.ProductId,
                                    Product_name = d.ProductName
                                }
                            );

                    var localProducts = dataQueryX.Distinct().ToList();

                    var listProduct = db.Products.ToList();
                    foreach(var item in localProducts)
                    {
                        var refProduct = listProduct.Where(x => x.ProductId == item.id).FirstOrDefault();
                        var existProduct = products.Where(x => x.Product_name.Replace(".", "-").Replace(" ", "") == item.Product_name.Replace(".", "-").Replace(" ", "")).FirstOrDefault();

                        if (existProduct == null)
                        {
                            refProduct.IsActive = false;
                            refProduct.LastModifiedBy = user.UserId;
                            refProduct.LastModifiedOn = DateTime.Now;
                        }
                        else
                        {
                            refProduct.ProductName = existProduct.Product_name;
                            refProduct.CV = Convert.ToDecimal(existProduct.CV);
                            refProduct.TS = Convert.ToDecimal(existProduct.TS);
                            refProduct.ASH = Convert.ToDecimal(existProduct.ASH);
                            refProduct.IM = Convert.ToDecimal(existProduct.IM);
                            refProduct.TM = Convert.ToDecimal(existProduct.TM);
                            refProduct.LastModifiedBy = user.UserId;
                            refProduct.LastModifiedOn = DateTime.Now;
                            refProduct.IsActive = true;
                        }

                        products.Remove(existProduct);
                    }

                    foreach(var item in products)
                    {
                        var newProduct = new Product()
                        {
                            ProductName = item.Product_name,
                            CompanyCode = company,
                            CV = Convert.ToDecimal(item.CV),
                            TS = Convert.ToDecimal(item.TS),
                            ASH = Convert.ToDecimal(item.ASH),
                            IM = Convert.ToDecimal(item.IM),
                            TM = Convert.ToDecimal(item.TM),
                            IsActive = true,
                            CreatedBy = user.UserId,
                            CreatedOn = DateTime.Now
                        };
                        db.Products.Add(newProduct);
                    }
                    db.SaveChanges();
                }

                var result = new { Success = true, Permission = permission_Item, Message = BaseConstants.MESSAGE_CREATE_SUCCESS, MessageDetail = string.Empty, Version = Configuration.VERSION };
                return Json(result, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                var result = new { Success = false, Permission = permission_Item, ex.Message, MessageDetail = string.Empty, Version = Configuration.VERSION, Items = string.Empty, Total = 0 };
                return Json(result, JsonRequestBehavior.AllowGet);
            }
        }

        public JsonResult SynchronizeROM()
        {
            // Check Permission based on Current Url
            UserX user = new UserX(Request);
            Permission_Item permission_Item = null;

            Permission.Check_API(Request, user, ref permission_Item);
            if (!permission_Item.Is_Add)
            {
                var msg = new { Success = false, Permission = permission_Item, Message = Permission.ERROR_PERMISSION_READ, MessageDetail = string.Empty, Version = Configuration.VERSION };
                return Json(msg, JsonRequestBehavior.AllowGet);
            }

            // -- Actual code
            string company = Request["company"];
            try
            {
                var roms = new List<Model_View_Mercy_ROM_Quality>();

                var controller_Company = new CompanyController();
                controller_Company.InitializeController(this.Request.RequestContext);
                if (controller_Company.Is_from_BigData(OurUtility.ValueOf(Request, "company")))
                {
                    if (company == "TCM")
                    {
                        using (MERCY_BigData_Ctx db = new MERCY_BigData_Ctx())
                        {
                            roms =
                                    (
                                        from d in db.Mercy_ROM_Quality
                                        where d.company_code == company
                                        select new Model_View_Mercy_ROM_Quality
                                        {
                                            Block = d.Block,
                                            ROM_Name = d.ROM_Name,
                                            ROM_Id = d.ROM_Id,
                                            Ton = d.Ton,
                                            CV = d.CV,
                                            TS = d.TS,
                                            ASH = d.ASH,
                                            IM = d.IM,
                                            TM = d.TM,
                                            Process_Date = d.Process_Date,
                                            company_code = d.company_code,
                                            Names = d.Block + " " + d.ROM_Name
                                        }
                                    ).ToList();
                        }

                    }
                    else if (company == "BEK")
                    {
                        using (MERCY_BigDataBEK_Ctx db = new MERCY_BigDataBEK_Ctx())
                        {
                            roms =
                                    (
                                        from d in db.MERCY_ROM_QUALITY
                                        where d.COMPANY_CODE == company
                                        select new Model_View_Mercy_ROM_Quality
                                        {
                                            Block = d.BLOCK,
                                            ROM_Name = d.ROM_NAME,
                                            ROM_Id = d.ROM_ID,
                                            Ton = d.TON,
                                            CV = d.CV,
                                            TS = d.TS,
                                            ASH = d.ASH,
                                            IM = d.IM,
                                            TM = d.TM,
                                            Process_Date = d.PROCESS_DATE,
                                            company_code = d.COMPANY_CODE,
                                            Names = d.BLOCK + " " + d.ROM_NAME
                                        }
                                    ).ToList();
                        }
                    }
                }

                roms = roms.GroupBy(x => x.Names).Select(x => x.Last()).ToList();

                using (MERCY_Ctx db = new MERCY_Ctx())
                {
                    var dataQueryX =
                            (
                                from d in db.ROMs
                                join c in db.Companies on d.CompanyCode equals c.CompanyCode
                                where d.CompanyCode == company
                                orderby d.ROMName
                                select new Model_View_ROM
                                {
                                    ROMId = d.ROMId,
                                    ROMName = d.ROMName
                                }
                            );

                    var localROMs = dataQueryX.ToList();

                    var listROM = db.ROMs.ToList();
                    foreach (var item in localROMs)
                    {
                        var refROM = listROM.Where(x => x.ROMId == item.ROMId).FirstOrDefault();
                        var existROM = roms.Where(x => x.Names == item.ROMName).FirstOrDefault();

                        if (existROM == null)
                        {
                            refROM.IsActive = false;
                            refROM.LastModifiedBy = user.UserId;
                            refROM.LastModifiedOn = DateTime.Now;
                        }
                        else
                        {
                            refROM.ROMName = existROM.Names;
                            refROM.CV = Convert.ToDecimal(existROM.CV);
                            refROM.TS = Convert.ToDecimal(existROM.TS);
                            refROM.ASH = Convert.ToDecimal(existROM.ASH);
                            refROM.IM = Convert.ToDecimal(existROM.IM);
                            refROM.TM = Convert.ToDecimal(existROM.TM);
                            refROM.Ton = Convert.ToInt32(existROM.Ton);
                            refROM.LastModifiedBy = user.UserId;
                            refROM.LastModifiedOn = DateTime.Now;
                            refROM.IsActive = true;
                        }

                        roms.Remove(existROM);
                    }

                    foreach (var item in roms)
                    {
                        var newROM = new ROM()
                        {
                            ROMName = item.Names,
                            CompanyCode = company,
                            CV = Convert.ToDecimal(item.CV),
                            TS = Convert.ToDecimal(item.TS),
                            ASH = Convert.ToDecimal(item.ASH),
                            IM = Convert.ToDecimal(item.IM),
                            TM = Convert.ToDecimal(item.TM),
                            Ton = Convert.ToInt32(item.Ton),
                            IsActive = true,
                            CreatedBy = user.UserId,
                            CreatedOn = DateTime.Now
                        };
                        db.ROMs.Add(newROM);
                    }
                    db.SaveChanges();
                }

                var result = new { Success = true, Permission = permission_Item, Message = BaseConstants.MESSAGE_CREATE_SUCCESS, MessageDetail = string.Empty, Version = Configuration.VERSION };
                return Json(result, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                var result = new { Success = false, Permission = permission_Item, ex.Message, MessageDetail = string.Empty, Version = Configuration.VERSION, Items = string.Empty, Total = 0 };
                return Json(result, JsonRequestBehavior.AllowGet);
            }
        }
    }
}