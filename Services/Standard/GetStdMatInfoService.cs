using ClosedXML.Excel;
using ShepherdEplan.Models;

namespace ShepherdEplan.Services.Standard
{
    public sealed class GetStdMatInfoService
    {
        public StdMatInfoModel? LoadMaterialFromExcel(string excelPath, string sap)
        {
            if (!File.Exists(excelPath))
                throw new FileNotFoundException($"El archivo Excel no existe: {excelPath}");

            using var workbook = new XLWorkbook(excelPath);

            // Recorremos TODAS las hojas del Excel
            foreach (var sheet in workbook.Worksheets)
            {
                // Buscamos la columna ue contiene el SAP (siempre es la 1ª según tu definición)
                var rows = sheet.RangeUsed()?.RowsUsed();
                if (rows == null) continue;

                // Saltamos la cabecera (fila 1)
                foreach (var row in rows.Skip(1))
                {
                    string? excelSap = row.Cell(1).GetString().Trim();

                    if (!string.Equals(excelSap, sap, StringComparison.OrdinalIgnoreCase))
                        continue;

                    // Match encontrado → creamos el modelo
                    var model = new StdMatInfoModel
                    {
                        Sap = sap,
                        Status = row.Cell(2).GetString().Trim(),     // Columna 2
                        Comments = row.Cell(3).GetString().Trim(),   // Columna 3
                        Stock = row.Cell(4).GetString().Trim(),      // Columna 4
                        Description = row.Cell(5).GetString().Trim(),// Columna 5
                        Creator = row.Cell(7).GetString().Trim(),    // Columna 7
                        Category = sheet.Name                        // EL NOMBRE DE LA HOJA ES LA CATEGORY
                    };

                    return model;
                }
            }

            return null; // No encontrado en ninguna hoja
        }
    }
}
