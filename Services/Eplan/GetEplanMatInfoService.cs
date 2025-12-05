using System.Diagnostics;
using ShepherdEplan.Models;

namespace ShepherdEplan.Services.Eplan
{
    public sealed class GetEplanMatInfoService
    {
        private const string DefaultPath = @"C:\temp\EPLAN-SAP.txt";

        public List<EplanMatInfoModel> LoadEplanMaterials(string? filePath = null)
        {
            filePath ??= DefaultPath;

            Debug.WriteLine($"[EPLAN] Cargando archivo: {filePath}");

            if (!File.Exists(filePath))
                throw new FileNotFoundException($"No se encuentra el fichero EPLAN-SAP: {filePath}");

            var result = new List<EplanMatInfoModel>();

            foreach (var rawLine in File.ReadAllLines(filePath))
            {
                var line = rawLine.Trim();
                if (string.IsNullOrWhiteSpace(line))
                    continue;

                var model = ParseLine(line);
                if (model != null)
                    result.Add(model);
            }

            Debug.WriteLine($"[EPLAN] ✓ Cargados {result.Count} materiales");
            return result;
        }

        private EplanMatInfoModel? ParseLine(string line)
        {
            try
            {
                var parts = line.Split(' ', StringSplitOptions.RemoveEmptyEntries);

                if (parts.Length < 3)
                    return null;

                string location = parts[0];
                string rawGroup = parts[1];
                string? group = rawGroup == "+" ? null : rawGroup;
                string body = parts[2];
                string qtyString = parts.Last();

                if (!int.TryParse(qtyString, out int qty))
                    qty = 0;

                // FIX: Take last 10 characters (alphanumeric), not just digits
                if (body.Length < 10)
                {
                    Debug.WriteLine($"[EPLAN] ⚠️ Body demasiado corto ({body.Length} chars): '{body}'");
                    return null;
                }

                string sap = body[^10..]; // Last 10 characters (can include letters)

                return new EplanMatInfoModel
                {
                    Location = location,
                    Group = group,
                    Sap = sap,
                    Units = qty,
                    Of = null,
                    Project = null
                };
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[EPLAN] Error parseando línea '{line}': {ex.Message}");
                return null;
            }
        }
    }
}