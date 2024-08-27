using System;
using System.Linq;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;

namespace MERCY.Web.BackEnd.Helpers
{
    public class ExcelProcessing
    {
        public static string GetDate(string p_str, string p_format, string p_default)
        {
            string result = p_default;

            p_str = p_str.Trim();

            if (string.IsNullOrEmpty(p_str))
            {
                if (p_format.ToUpper().Contains("HH")) result = string.Empty;
                return result;
            }

            try
            {
                double.TryParse(p_str, out var cellDouble);

                var theDate = DateTime.FromOADate(cellDouble);
                result = theDate.ToString(p_format);
            }
            catch {}

            return result;
        }

        public static string GetDate(string p_str, string p_format)
        {
            string p_default = "null";

            return GetDate(p_str, p_format, p_default);
        }
    
        public static string GetNumeric(string p_str)
        {
            string result = "0";

            p_str = p_str.Trim();

            if ( ! string.IsNullOrEmpty(p_str)) result = p_str;

            return result;
        }

        public static string GetCellValue(WorkbookPart wbPart, WorksheetPart wsPart, string addressName)
        {
            string result = string.Empty;

            try
            {
                // Use its Worksheet property to get a reference to the cell 
                // whose address matches the address you supplied.
                Cell theCell = wsPart.Worksheet.Descendants<Cell>().Where(c => c.CellReference == addressName).FirstOrDefault();

                // If the cell does not exist, return an empty string.
                if (theCell != null)
                {
                    if (theCell.InnerText.Length > 0)
                    {
                        result = theCell.InnerText;
                        if (theCell.DataType != null)
                        {
                            switch (theCell.DataType.Value)
                            {
                                case CellValues.SharedString:

                                    // For shared strings, look up the value in the
                                    // shared strings table.
                                    var stringTable = wbPart.GetPartsOfType<SharedStringTablePart>().FirstOrDefault();

                                    // If the shared string table is missing, something 
                                    // is wrong. Return the index that is in
                                    // the cell. Otherwise, look up the correct text in 
                                    // the table.
                                    if (stringTable != null)
                                    {
                                        result = stringTable.SharedStringTable.ElementAt(int.Parse(result)).InnerText;
                                    }
                                    break;

                                case CellValues.Boolean:
                                    switch (result)
                                    {
                                        case "0":
                                            result = "FALSE";
                                            break;
                                        default:
                                            result = "TRUE";
                                            break;
                                    }
                                    break;
                            }
                        }
                    }
                }
            }
            catch {}

            return result.Trim();
        }

        // Retrieve the value of a cell, given a file name, sheet name, 
        // and address name.
        public static string GetCellValue(string fileName, string sheetName, string addressName)
        {
            string value = null;

            // Open the spreadsheet document for read-only access.
            using (SpreadsheetDocument document = SpreadsheetDocument.Open(fileName, false))
            {
                // Retrieve a reference to the workbook part.
                WorkbookPart wbPart = document.WorkbookPart;

                // Find the sheet with the supplied name, and then use that 
                // Sheet object to retrieve a reference to the first worksheet.
                Sheet theSheet = wbPart.Workbook.Descendants<Sheet>().Where(s => s.Name == sheetName).FirstOrDefault();

                // Throw an exception if there is no sheet.
                if (theSheet == null)
                {
                    throw new ArgumentException("sheetName");
                }

                // Retrieve a reference to the worksheet part.
                WorksheetPart wsPart = (WorksheetPart)(wbPart.GetPartById(theSheet.Id));

                // Use its Worksheet property to get a reference to the cell 
                // whose address matches the address you supplied.
                Cell theCell = wsPart.Worksheet.Descendants<Cell>().Where(c => c.CellReference == addressName).FirstOrDefault();

                // If the cell does not exist, return an empty string.
                if (theCell.InnerText.Length > 0)
                {
                    value = theCell.InnerText;

                    // If the cell represents an integer number, you are done. 
                    // For dates, this code returns the serialized value that 
                    // represents the date. The code handles strings and 
                    // Booleans individually. For shared strings, the code 
                    // looks up the corresponding value in the shared string 
                    // table. For Booleans, the code converts the value into 
                    // the words TRUE or FALSE.
                    if (theCell.DataType != null)
                    {
                        switch (theCell.DataType.Value)
                        {
                            case CellValues.SharedString:

                                // For shared strings, look up the value in the
                                // shared strings table.
                                var stringTable = wbPart.GetPartsOfType<SharedStringTablePart>().FirstOrDefault();

                                // If the shared string table is missing, something 
                                // is wrong. Return the index that is in
                                // the cell. Otherwise, look up the correct text in 
                                // the table.
                                if (stringTable != null)
                                {
                                    value = stringTable.SharedStringTable.ElementAt(int.Parse(value)).InnerText;
                                }
                                break;

                            case CellValues.Boolean:
                                switch (value)
                                {
                                    case "0":
                                        value = "FALSE";
                                        break;
                                    default:
                                        value = "TRUE";
                                        break;
                                }
                                break;
                        }
                    }
                }
            }

            return value;
        }
    }
}