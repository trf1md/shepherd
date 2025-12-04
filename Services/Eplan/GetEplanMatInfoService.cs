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

                var digits = new string(body.Where(char.IsDigit).ToArray());
                if (digits.Length < 10)
                    return null;

                string sap = digits[^10..];

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