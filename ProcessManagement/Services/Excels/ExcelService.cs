using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Mvc;
using NPOI.HSSF.UserModel;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using ProcessManagement.Models.KHO_TPHAM;
using ProcessManagement.Models.TienDoGCs;
using ProcessManagement.Services.SQLServer;
using System.ComponentModel;
using System.Drawing.Imaging;
using System.Drawing;
using System.Reflection;
using ProcessManagement.Services.QRCodes;
using System.IO;
using Microsoft.CodeAnalysis;
using System.Collections.Generic;

namespace ProcessManagement.Services.Excels
{
    public static class ExcelService
    {
        public static string ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
        public static long MaxFileSize = 1024L * 1024L * 1024L * 2L;
        private static SQLServerServices SQLServerServices = new SQLServerServices();

        public class ExcelCell
        {
            public string CellName { get; set; } = string.Empty;
            public string Row { get; set; } = string.Empty;
            public string Col { get; set; } = string.Empty;
            public string CellValue { get; set; } = string.Empty;
            public Type ValueType { get; set; } = typeof(string);
        }

        private static async Task<string> InitialExportExcelFile(IBrowserFile selectedFile)
        {
            if (selectedFile != null)
            {
                var fileName = selectedFile.Name;

                var extension = Path.GetExtension(fileName);

                //var newFileName = $"Export_{fileName}{extension}";

                var directoryPath = Path.Combine("wwwroot", "ExportExcellFile");

                var fullPath = Path.Combine(directoryPath, fileName);

                Directory.CreateDirectory(directoryPath);

                using var fileStream = new FileStream(fullPath, FileMode.Create, FileAccess.Write, FileShare.ReadWrite);

                await selectedFile.OpenReadStream(50000000).CopyToAsync(fileStream);

                return fullPath;
            }
            return string.Empty;
        }

        public async static Task<List<ExcelCell>> GetParallelListAddressExcelValue(string filePath, string sheetName, List<ExcelCell> listAddress)
        {
            if (string.IsNullOrWhiteSpace(filePath) || string.IsNullOrWhiteSpace(sheetName))
            {
                return new();
            }

            using (var stream = File.Open(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            {
                var extension = Path.GetExtension(filePath);

                IWorkbook? workbook = null;

                if (extension == ".xls")
                {
                    workbook = new HSSFWorkbook(stream);
                }
                else if (extension == ".xlsx")
                {
                    workbook = new XSSFWorkbook(stream);
                }

                ISheet? worksheet = workbook?.GetSheet(sheetName);

                var excelValues = new List<ExcelCell>(listAddress.Count);

                if (workbook != null && worksheet != null)
                {
                    var tasks = new List<Task>();

                    foreach (var cellitem in listAddress)
                    {
                        tasks.Add(Task.Run(() =>
                        {
                            int columnIndex = ColumnLetterToIndex(cellitem.Col);
                            IRow row = worksheet.GetRow(int.Parse(cellitem.Row) - 1);
                            ICell? cell = row?.GetCell(columnIndex - 1);

                            if (cell != null)
                            {
                                if (cell.CellType == CellType.Formula)
                                {
                                    IFormulaEvaluator evaluator = workbook.GetCreationHelper().CreateFormulaEvaluator();
                                    CellValue cellValue = evaluator.Evaluate(cell);

                                    switch (cellValue.CellType)
                                    {
                                        case CellType.Numeric:
                                            cellitem.CellValue = cellValue.NumberValue.ToString();
                                            break;
                                        case CellType.String:
                                            cellitem.CellValue = cellValue.StringValue;
                                            break;
                                        case CellType.Boolean:
                                            cellitem.CellValue = cellValue.BooleanValue.ToString();
                                            break;
                                        default:
                                            cellitem.CellValue = string.Empty;
                                            break;
                                    }
                                }
                                else
                                {
                                    cellitem.CellValue = cell?.ToString() ?? string.Empty;
                                }
                            }
                            else
                            {
                                cellitem.CellValue = string.Empty;
                            }

                            lock (excelValues)
                            {
                                excelValues.Add(cellitem);
                            }
                        }));
                    }

                    await Task.WhenAll(tasks);

                }

                return excelValues;
            }
        }

        private static List<ExcelCell> GetListAddressExcelValue(string filePath, string sheetName, List<ExcelCell> listAddress)
        {
            if (filePath.Trim() == string.Empty || sheetName.Trim() == string.Empty) { return new(); }

            using (FileStream fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read))
            {
                List<ExcelCell> excelValues = new();

                var extension = Path.GetExtension(filePath);

                ISheet? worksheet = null;

                IWorkbook? workbook = null;

                if (extension == ".xls")
                {
                    workbook = new HSSFWorkbook(fileStream);
                }
                else if (extension == ".xlsx")
                {
                    workbook = new XSSFWorkbook(fileStream);

                }

                worksheet = workbook?.GetSheet(sheetName);

                if (worksheet != null && workbook != null)
                {
                    foreach (var cellitem in listAddress)
                    {
                        // Convert the column identifier to column index
                        int columnIndex = ColumnLetterToIndex(cellitem.Col);

                        // Get the cell based on the Excel row and column index
                        IRow row = worksheet.GetRow(int.Parse(cellitem.Row) - 1);

                        ICell cell = row.GetCell(columnIndex - 1);

                        if (cell != null)
                        {
                            // Check if the cell contains a formula
                            if (cell.CellType == CellType.Formula)
                            {
                                // Evaluate the cell to get the calculated value
                                IFormulaEvaluator evaluator = workbook.GetCreationHelper().CreateFormulaEvaluator();
                                CellValue cellValue = evaluator.Evaluate(cell);

                                // Get the value based on the cell type
                                switch (cellValue.CellType)
                                {
                                    case CellType.Numeric:
                                        cellitem.CellValue = cellValue.NumberValue.ToString();
                                        break;
                                    case CellType.String:
                                        cellitem.CellValue = cellValue.StringValue;
                                        break;
                                    case CellType.Boolean:
                                        cellitem.CellValue = cellValue.BooleanValue.ToString();
                                        break;
                                    default:
                                        cellitem.CellValue = string.Empty;
                                        break;
                                }
                            }
                            else
                            {
                                // Get the cell value as string
                                cellitem.CellValue = cell?.ToString() ?? string.Empty;
                            }
                        }
                        else
                        {
                            cellitem.CellValue = string.Empty;
                        }


                        excelValues.Add(cellitem);
                    }

                }

                return excelValues;
            }
        }

        private static int ColumnLetterToIndex(string columnLetter)
        {
            string upperColumnLetter = columnLetter.ToUpper();

            int result = 0;

            foreach (char c in upperColumnLetter)
            {
                result *= 26;

                result += (c - 'A' + 1);
            }

            return result;
        }

        #region TienDoGC Excell Services
        public static async Task<FileContentResult?> GenerateExcelData_TienDoGC(TienDoGC tienDoGC)
        {
            if (tienDoGC.DSachTienDoRows.Count == 0) return null;

            int headerRow = TienDoGCRow.ExcellAddress.HeaderRow;

            int stepjumping = TienDoGCRow.ExcellAddress.RowJumStep;

            List<ExcelCell> excelDatas = new();

            // Generate data TienDoGC Header 

            var tdgcProperties = tienDoGC.GetPropertiesValues().Where(pro => pro != null && pro.AlowDatabase);

            if (tdgcProperties != null && tdgcProperties.Any())
            {
                // Add header
                ExcelCell cellhead = new()
                {
                    CellName = TienDoGC.DBName.ExcellTitle,
                    Col = TienDoGC.ExcellAddress.ColumnAddress[TienDoGC.DBName.ExcellTitle],
                    Row = TienDoGC.ExcellAddress.RowAddress[TienDoGC.DBName.ExcellTitle],
                    ValueType = typeof(string),
                    CellValue = $"THEO DÕI TIẾN ĐỘ KẾ HOẠCH {tienDoGC.TenCongDoan.ToUpper()} {tienDoGC.MaSanPham.ToUpper()}"
                };

                excelDatas.Add(cellhead);

                // Another
                foreach (var property in tdgcProperties)
                {
                    if (property.DBName != null && TienDoGC.ExcellAddress.ColumnAddress.ContainsKey(property.DBName))
                    {
                        ExcelCell cell = new()
                        {
                            CellName = property.DBName,
                            Col = TienDoGC.ExcellAddress.ColumnAddress[property.DBName],
                            Row = TienDoGC.ExcellAddress.RowAddress[property.DBName],
                            ValueType = property.Type ?? typeof(string),
                            CellValue = property.Value?.ToString() ?? string.Empty
                        };

                        excelDatas.Add(cell);
                    }
                }
            }

            // Generate data TienDoGCRow list

            foreach (var tdgcrow in tienDoGC.DSachTienDoRows)
            {
                headerRow += stepjumping;

                var tdgcRowExceldatas = GenerateExcelData_TienDoGCRow(tdgcrow, headerRow);

                excelDatas.AddRange(tdgcRowExceldatas);
            }

            string excelPath = TienDoGCRow.ExcellAddress.ExportPath;

            string excelWritePath = TienDoGCRow.ExcellAddress.ExportLocation;

            string excelSheetName = tienDoGC.TenCongDoan;

            // Create a new workbook for writing
            IWorkbook workbook;

            string extension = Path.GetExtension(excelPath);

            using (FileStream fileStream = new FileStream(excelPath, FileMode.Open, FileAccess.Read))
            {
                if (extension == ".xls")
                {
                    workbook = new HSSFWorkbook(fileStream);
                }
                else if (extension == ".xlsx")
                {
                    workbook = new XSSFWorkbook(fileStream);
                }
                else
                {
                    throw new ArgumentException("Unsupported file extension");
                }
            }

            ISheet worksheet = workbook.GetSheetAt(0); // FirstSheet

            if (worksheet == null) { return null; }

            worksheet.Workbook.SetSheetName(0, excelSheetName); // Asign SheetName

            foreach (var location in excelDatas)
            {
                int columnIndex = ColumnLetterToIndex(location.Col);

                IRow row = worksheet.GetRow(int.Parse(location.Row) - 1) ?? worksheet.CreateRow(int.Parse(location.Row) - 1);

                ICell worksheetCell = row.GetCell(columnIndex - 1) ?? row.CreateCell(columnIndex - 1);

                if (location != null)
                {
                    if (location.ValueType == typeof(string))
                    {
                        string value = location.CellValue;
                        worksheetCell.SetCellType(CellType.String);
                        worksheetCell.SetCellValue(value);
                    }
                    else if (location.ValueType == typeof(int) || location.ValueType == typeof(double))
                    {
                        if (int.TryParse(location.CellValue, out int vl))
                        {
                            worksheetCell.SetCellType(CellType.Numeric);
                            worksheetCell.SetCellValue(vl);
                        }
                        else
                        {
                            string value = location.CellValue;
                            worksheetCell.SetCellType(CellType.String);
                            worksheetCell.SetCellValue(value);
                        }
                    }
                    else if (location.ValueType == typeof(DateTime))
                    {
                        if (DateTime.TryParse(location.CellValue, out DateTime dt))
                        {
                            worksheetCell.SetCellType(CellType.Numeric);
                            worksheetCell.SetCellValue(dt);
                        }
                    }
                }
            }

            using (FileStream writeStream = new FileStream(excelWritePath, FileMode.Create, FileAccess.Write))
            {
                workbook.Write(writeStream);
            }

            using (FileStream readStream = new FileStream(excelWritePath, FileMode.Open, FileAccess.Read))
            {
                byte[] bytes = await File.ReadAllBytesAsync(excelWritePath);

                var file = new FileContentResult(bytes, ContentType);

                file.FileDownloadName = $"TienDoGiaCong_{tienDoGC.TenCongDoan.Replace(" ", "_")}{extension}";

                return file;
            }
        }

        private static List<ExcelCell> GenerateExcelData_TienDoGCRow(TienDoGCRow tdgcrow, int targetRow)
        {
            List<ExcelCell> exceldatas = new();

            var tdgcRowProperties = tdgcrow.GetPropertiesValues().Where(pro => pro != null && pro.AlowDatabase);

            if (tdgcRowProperties != null && tdgcRowProperties.Any())
            {
                foreach (var property in tdgcRowProperties)
                {
                    if (property.DBName != null && TienDoGCRow.ExcellAddress.ColumnAddress.ContainsKey(property.DBName))
                    {
                        ExcelCell cell = new()
                        {
                            CellName = property.DBName,
                            Col = TienDoGCRow.ExcellAddress.ColumnAddress[property.DBName],
                            Row = targetRow.ToString(),
                            ValueType = property.Type ?? typeof(string),
                            CellValue = property.Value?.ToString() ?? string.Empty
                        };

                        string convertVlue = GetColumnValueByID(cell.CellValue, cell.CellName);

                        if (!string.IsNullOrEmpty(convertVlue))
                        {
                            cell.CellValue = convertVlue;
                        }

                        exceldatas.Add(cell);
                    }
                }
            }

            return exceldatas;
        }

        // Convert ID to stringValue
        private static string GetColumnValueByID(object? id, string colName)
        {
            string value = string.Empty;

            if (colName == TienDoGCRow.DBName.SPID)
            {
                value = SQLServerServices.GetMaSanphamByID(id);
            }
            else if (colName == TienDoGCRow.DBName.NCID)
            {
                value = SQLServerServices.GetNguyenCongByID(id);
            }
            else if (colName == TienDoGCRow.DBName.MMID)
            {
                value = SQLServerServices.GetMaMayMocbyID(id);
            }
            else if (colName == TienDoGCRow.DBName.NVIDs)
            {
                string ids = id?.ToString()?.Trim() ?? string.Empty;

                if (!string.IsNullOrEmpty(ids))
                {
                    List<string> nvids = ids.Split(",").ToList();

                    value = string.Join(", ", nvids
                            .Select(nvid => (SQLServerServices.GetNhanVienbyID(nvid).GetThongTinNhanVienByName("Tên nhân viên")).GiaTri.Value?.ToString() ?? string.Empty)
                            .Where(name => !string.IsNullOrEmpty(name)))
                            .Trim(',');
                }
            }

            return value;
        }
        #endregion

        #region MQLThungTP Excell Services
        public static async Task<FileContentResult?> GenerateExcelData_MQLThungTP(ThungTPham thungTPham)
        {
            if (thungTPham.MaQuanLyThung.Value == null) return null;

            List<ExcelCell> excelDatas = new();

            // Generate data ThungTPham

            var targetSP = SQLServerServices.GetSanpham(int.TryParse(thungTPham.SPID.Value?.ToString(), out int spid) ? spid : 0, false);

            // TenSP
            ExcelCell tenspCell = new()
            {
                CellName = ThungTPham.ExcellAddress.TENSP,
                Col = ThungTPham.ExcellAddress.ColumnAddress[ThungTPham.ExcellAddress.TENSP],
                Row = ThungTPham.ExcellAddress.RowAddress[ThungTPham.ExcellAddress.TENSP],
                ValueType = typeof(string),
                CellValue = $"{targetSP.SP_TenSanPham.Value}"
            };
            excelDatas.Add(tenspCell);

            // MaSP
            ExcelCell maspCell = new()
            {
                CellName = ThungTPham.ExcellAddress.MASP,
                Col = ThungTPham.ExcellAddress.ColumnAddress[ThungTPham.ExcellAddress.MASP],
                Row = ThungTPham.ExcellAddress.RowAddress[ThungTPham.ExcellAddress.MASP],
                ValueType = typeof(string),
                CellValue = $"{targetSP.SP_MaSP.Value}"
            };
            excelDatas.Add(maspCell);

            // Soluong
            ExcelCell soluongCell = new()
            {
                CellName = ThungTPham.ExcellAddress.SOLUONG,
                Col = ThungTPham.ExcellAddress.ColumnAddress[ThungTPham.ExcellAddress.SOLUONG],
                Row = ThungTPham.ExcellAddress.RowAddress[ThungTPham.ExcellAddress.SOLUONG],
                ValueType = typeof(string),
                CellValue = $"{thungTPham.SoLuong.Value} (PCS)"
            };
            excelDatas.Add(soluongCell);

            // Soluong
            ExcelCell MQLCell = new()
            {
                CellName = ThungTPham.ExcellAddress.MQLTHUNG,
                Col = ThungTPham.ExcellAddress.ColumnAddress[ThungTPham.ExcellAddress.MQLTHUNG],
                Row = ThungTPham.ExcellAddress.RowAddress[ThungTPham.ExcellAddress.MQLTHUNG],
                ValueType = typeof(string),
                CellValue = $"{thungTPham.MaQuanLyThung.Value}"
            };
            excelDatas.Add(MQLCell);

            // Excell file
            string excelPath = ThungTPham.ExcellAddress.ExportPath;

            string excelWritePath = ThungTPham.ExcellAddress.ExportLocation;

            string excelSheetName = $"{thungTPham.MaQuanLyThung.Value}";

            // Create a new workbook for writing
            IWorkbook workbook;

            string extension = Path.GetExtension(excelPath);

            using (FileStream fileStream = new FileStream(excelPath, FileMode.Open, FileAccess.Read))
            {
                if (extension == ".xls")
                {
                    workbook = new HSSFWorkbook(fileStream);
                }
                else if (extension == ".xlsx")
                {
                    workbook = new XSSFWorkbook(fileStream);
                }
                else
                {
                    throw new ArgumentException("Unsupported file extension");
                }
            }

            ISheet worksheet = workbook.GetSheetAt(0); // FirstSheet

            if (worksheet == null) { return null; }

            worksheet.Workbook.SetSheetName(0, excelSheetName); // Asign SheetName

            foreach (var location in excelDatas)
            {
                int columnIndex = ColumnLetterToIndex(location.Col);

                IRow row = worksheet.GetRow(int.Parse(location.Row) - 1) ?? worksheet.CreateRow(int.Parse(location.Row) - 1);

                ICell worksheetCell = row.GetCell(columnIndex - 1) ?? row.CreateCell(columnIndex - 1);

                if (location != null)
                {
                    if (location.ValueType == typeof(string))
                    {
                        string value = location.CellValue;
                        worksheetCell.SetCellType(CellType.String);
                        worksheetCell.SetCellValue(value);
                    }
                    else if (location.ValueType == typeof(int) || location.ValueType == typeof(double))
                    {
                        if (int.TryParse(location.CellValue, out int vl))
                        {
                            worksheetCell.SetCellType(CellType.Numeric);
                            worksheetCell.SetCellValue(vl);
                        }
                        else
                        {
                            string value = location.CellValue;
                            worksheetCell.SetCellType(CellType.String);
                            worksheetCell.SetCellValue(value);
                        }
                    }
                    else if (location.ValueType == typeof(DateTime))
                    {
                        if (DateTime.TryParse(location.CellValue, out DateTime dt))
                        {
                            worksheetCell.SetCellType(CellType.Numeric);
                            worksheetCell.SetCellValue(dt);
                        }
                    }
                }
            }

            //// Insert Image for QR
            QRCodeServices qrCodeServices = new();

            byte[] imageBytes = qrCodeServices.GenerateQRCodeImage($"{thungTPham.MaQuanLyThung.Value}");

            // Create an Excel picture
            int pictureIndex = workbook.AddPicture(imageBytes, PictureType.PNG);

            IDrawing drawing = worksheet.CreateDrawingPatriarch();

            // Parameters for CreateAnchor: dx1, dy1, dx2, dy2, col1, row1, col2, row2
            IClientAnchor anchor = drawing.CreateAnchor(0, 0, 0, 0, 1, 1, 2, 2);

            //dx1: Horizontal offset within the first cell(0 - 1023)
            //dy1: Vertical offset within the first cell(0 - 255)
            //dx2: Horizontal offset within the last cell(0 - 1023)
            //dy2: Vertical offset within the last cell(0 - 255)
            //col1: First column(0 - based, B = 1)
            //row1: First row(0 - based, 2 = 1)
            //col2: Last column
            //row2: Last row

            // Create the picture
            drawing.CreatePicture(anchor, pictureIndex);
            ////

            using (FileStream writeStream = new FileStream(excelWritePath, FileMode.Create, FileAccess.Write))
            {
                workbook.Write(writeStream);
            }

            using (FileStream readStream = new FileStream(excelWritePath, FileMode.Open, FileAccess.Read))
            {
                byte[] bytes = await File.ReadAllBytesAsync(excelWritePath);

                var file = new FileContentResult(bytes, ContentType);

                file.FileDownloadName = $"MQLThung_[{thungTPham.MaQuanLyThung.Value}]{extension}";

                return file;
            }

        }
        #endregion
    }
}
