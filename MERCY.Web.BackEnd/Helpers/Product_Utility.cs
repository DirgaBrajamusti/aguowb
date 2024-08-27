using System;
using System.Collections.Generic;
using System.Linq;

using System.Data;

using MERCY.Data.EntityFramework;

using MERCY.Web.BackEnd.Models;
using MERCY.Data.EntityFramework_BigData;

namespace MERCY.Web.BackEnd.Helpers
{
    public class Product_Utility
    {
        public static List<Model_Product> Get(string p_company)
        {
            List<Model_Product> result = new List<Model_Product>();

            bool is_DataROM_from_BI = false;

            try
            {
                using (MERCY_Ctx db = new MERCY_Ctx())
                {
                    var dataQuery =
                            (
                                from d in db.Companies
                                where d.CompanyCode == p_company
                                select new
                                {
                                    d.Is_DataROM_from_BI
                                }
                            );

                    var data = dataQuery.SingleOrDefault();
                    if (data != null)
                    {
                        is_DataROM_from_BI = data.Is_DataROM_from_BI;
                    }
                }

                if (is_DataROM_from_BI)
                {
                    DateTime now = DateTime.Now;

                    using (MERCY_BigData_Ctx db = new MERCY_BigData_Ctx())
                    {
                        var dataQueryX =
                                (
                                    from d in db.Mercy_quality_outlook
                                    where d.FirstDate.Value.Month == now.Month
                                            && d.FirstDate.Value.Year == now.Year
                                    orderby d.Product_name
                                    select new Model_Product
                                    {
                                        id = d.id
                                        , Product_name = d.Product_name
                                        , FirstDate = d.FirstDate
                                    }
                                );

                        result = dataQueryX.Distinct().ToList();
                    }
                }
                else
                {
                    using (MERCY_Ctx db = new MERCY_Ctx())
                    {
                        var dataQueryX =
                                (
                                    from d in db.Products
                                    where d.CompanyCode == p_company && d.IsActive
                                    orderby d.ProductName
                                    select new Model_Product
                                    {
                                        id = d.ProductId,
                                        Product_name = d.ProductName,
                                        FirstDate = null
                                    }
                                );

                        result = dataQueryX.Distinct().ToList();
                    }
                }
            }
            catch {}

            return result;
        }
    }
}