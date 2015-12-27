using System;
using System.Collections.ObjectModel;
using System.IO;
using LuckyRotorPlate.Model;
using NPOI.HPSF;
using NPOI.HSSF.UserModel;

namespace LuckyRotorPlate.DataService
{
    /// <summary>
    /// Excel 读类
    /// </summary>
    class XlsHelper
    {
        public static ObservableCollection<RealThing> GetRealThings(String fileName, int startRow, int startCol)
        {
            ObservableCollection<RealThing> things = new ObservableCollection<RealThing>();
            HSSFWorkbook hssfworkbook;

            using (FileStream file = new FileStream(fileName, FileMode.Open, FileAccess.Read))
            {
                hssfworkbook = new HSSFWorkbook(file);
            }
            HSSFSheet sheet = hssfworkbook.GetSheetAt(0);
            System.Collections.IEnumerator rows = sheet.GetRowEnumerator();

            while (rows.MoveNext())
            {
                HSSFRow row = (HSSFRow)rows.Current;
                if (row.RowNum < startRow)
                {
                    continue;
                }

                HSSFCell cell;
                cell = row.GetCell(startCol);
                String name = null != cell ? cell.ToString() : "";
                cell = row.GetCell(startCol + 1);
                double value = null != cell ? Convert.ToDouble(cell.ToString()) : 1;
                cell = row.GetCell(startCol + 2);
                String remark = null != cell ? cell.ToString() : "";
                things.Add(new RealThing(name, value, remark));
            }

            return things;
        }

        public static void SaveRealThings(String fileName, ObservableCollection<RealThing> things, String sheetName)
        {
            HSSFWorkbook  hssfworkbook = new HSSFWorkbook();

            //create a entry of DocumentSummaryInformation
            DocumentSummaryInformation dsi = PropertySetFactory.CreateDocumentSummaryInformation();
            dsi.Company = "SDCJ";
            hssfworkbook.DocumentSummaryInformation = dsi;

            //create a entry of SummaryInformation
            SummaryInformation si = PropertySetFactory.CreateSummaryInformation();
            si.Subject = "抽奖结果";
            hssfworkbook.SummaryInformation = si;

            HSSFSheet sheet1 = hssfworkbook.CreateSheet(sheetName);
            sheet1.SetColumnWidth(0, 20 * 256);
            sheet1.SetColumnWidth(1, 20 * 256);
            sheet1.SetColumnWidth(3, 20 * 256);
            HSSFRow row;
            HSSFCell cell;
            //列标题
            HSSFCellStyle headerStyle = setBorderStyle(hssfworkbook); 
            HSSFFont font = hssfworkbook.CreateFont();            
            font.FontHeightInPoints = 12;
            font.Boldweight = HSSFFont.BOLDWEIGHT_BOLD;
            headerStyle.SetFont(font);
            headerStyle.Alignment = HSSFCellStyle.ALIGN_CENTER;
            row = sheet1.CreateRow(0);
            cell = row.CreateCell(0);
            cell.CellStyle = headerStyle;
            cell.SetCellValue("抽奖项目");
            cell = row.CreateCell(1);
            cell.CellStyle = headerStyle;
            cell.SetCellValue("中奖项目");
            cell = row.CreateCell(2);
            cell.CellStyle = headerStyle;
            cell.SetCellValue("数量");
            cell = row.CreateCell(3);
            cell.CellStyle = headerStyle;
            //内容
            HSSFCellStyle txtStyle = setBorderStyle(hssfworkbook);
            for (int rowIndex = 1; rowIndex <= things.Count; rowIndex++)
            {
                row = sheet1.CreateRow(rowIndex);
                cell = row.CreateCell(0);
                cell.CellStyle = txtStyle;
                cell.SetCellValue(String.Format("{0}", things[rowIndex - 1].Name));
                cell = row.CreateCell(1);
                cell.CellStyle = txtStyle;
                cell.SetCellValue(String.Format("{0}", things[rowIndex - 1].ObtainedGift));
                cell = row.CreateCell(2);
                cell.CellStyle = txtStyle;
                cell.SetCellValue(String.Format("{0}", things[rowIndex - 1].Value));
                cell = row.CreateCell(3);
                cell.CellStyle = txtStyle;
            }

            //Write the stream data of workbook to the root directory
            FileStream file = new FileStream(fileName, FileMode.Create);
            hssfworkbook.Write(file);
            file.Close();

        }

        private static HSSFCellStyle setBorderStyle(HSSFWorkbook hssfworkbook)
        {
            //设置边框
            HSSFCellStyle style = hssfworkbook.CreateCellStyle();
            style.BorderBottom = HSSFCellStyle.BORDER_THIN;
            style.BorderLeft = HSSFCellStyle.BORDER_THIN;
            style.BorderRight = HSSFCellStyle.BORDER_THIN;
            style.BorderTop = HSSFCellStyle.BORDER_THIN;
            return style;
        }
    }
}
