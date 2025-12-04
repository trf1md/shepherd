using System.Diagnostics;
using ClosedXML.Excel;
using ShepherdEplan.Models;

namespace ShepherdEplan.Services.Standard
{
    public sealed class GetStdMatInfoService
    {
        // OLD METHOD - still available for backward compatibility
        public StdMatInfoModel? LoadMaterialFromExcel(string excelPath, string sap)
        {
            if (!File.Exists(excelPath))
                throw new FileNotFoundException($"El archivo Excel no existe: {excelPath}");

            using var workbook = new XLWorkbook(excelPath);

            foreach (var sheet in workbook.Worksheets)
            {
                var rows = sheet.RangeUsed()?.RowsUsed();
                if (rows == null) continue;

                foreach (var row in rows.Skip(1))
                {
                    string? excelSap = row.Cell(1).GetString().Trim();

                    if (!string.Equals(excelSap, sap, StringComparison.OrdinalIgnoreCase))
                        continue;

                    var model = new StdMatInfoModel
                    {
                        Sap = sap,
                        Status = row.Cell(2).GetString().Trim(),
                        Comments = row.Cell(3).GetString().Trim(),
                        Stock = row.Cell(4).GetString().Trim(),
                        Description = row.Cell(5).GetString().Trim(),
                        Creator = row.Cell(7).GetString().Trim(),
                        Category = sheet.Name
                    };

                    return model;
                }
            }

            return null;
        }

        // NEW METHOD - Load ALL materials from Excel into a dictionary (CRITICAL FIX!)
        public Dictionary<string, StdMatInfoModel> LoadAllMaterialsFromExcel(string excelPath)
        {
            Debug.WriteLine($"[EXCEL] Abriendo archivo: {excelPath}");

            if (!File.Exists(excelPath))
                throw new FileNotFoundException($"El archivo Excel no existe: {excelPath}");

            var dictionary = new Dictionary<string, StdMatInfoModel>(StringComparer.OrdinalIgnoreCase);

            using var workbook = new XLWorkbook(excelPath);
            Debug.WriteLine($"[EXCEL] Archivo abierto, procesando {workbook.Worksheets.Count} hojas...");

            foreach (var sheet in workbook.Worksheets)
            {
                Debug.WriteLine($"[EXCEL] Procesando hoja: {sheet.Name}");

                var rows = sheet.RangeUsed()?.RowsUsed();
                if (rows == null)
                {
                    Debug.WriteLine($"[EXCEL] Hoja {sheet.Name} está vacía, saltando...");
                    continue;
                }

                int rowCount = 0;
                foreach (var row in rows.Skip(1)) // Skip header
                {
                    try
                    {
                        string? excelSap = row.Cell(1).GetString().Trim();

                        if (string.IsNullOrWhiteSpace(excelSap))
                            continue;

                        var model = new StdMatInfoModel
                        {
                            Sap = excelSap,
                            Status = row.Cell(2).GetString().Trim(),
                            Comments = row.Cell(3).GetString().Trim(),
                            Stock = row.Cell(4).GetString().Trim(),
                            Description = row.Cell(5).GetString().Trim(),
                            Creator = row.Cell(7).GetString().Trim(),
                            Category = sheet.Name
                        };

                        // Add or update in dictionary (last occurrence wins)
                        dictionary[excelSap] = model;
                        rowCount++;
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine($"[EXCEL] Error procesando fila en hoja {sheet.Name}: {ex.Message}");
                    }
                }

                Debug.WriteLine($"[EXCEL] Hoja {sheet.Name} procesada: {rowCount} materiales");
            }

            Debug.WriteLine($"[EXCEL] ✓ Diccionario completo: {dictionary.Count} materiales totales");
            return dictionary;
        }
    }
}