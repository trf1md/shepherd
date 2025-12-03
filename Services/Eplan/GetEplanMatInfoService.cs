using ShepherdEplan.Models;

namespace ShepherdEplan.Services.Eplan
{
    public sealed class GetEplanMatInfoService
    {
        private const string DefaultPath = @"C:\temp\EPLAN-SAP.txt";

        public List<EplanMatInfoModel> LoadEplanMaterials(string? filePath = null)
        {
            filePath ??= DefaultPath;

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

            return result;
        }

        private EplanMatInfoModel? ParseLine(string line)
        {
            try
            {
                // Separación por espacios
                var parts = line.Split(' ', StringSplitOptions.RemoveEmptyEntries);

                if (parts.Length < 3)
                    return null;

                // Location (=100)
                string location = parts[0];

                // Group (+S1, +, etc.)
                string rawGroup = parts[1];
                string? group = rawGroup == "+" ? null : rawGroup;

                // Parte intermedia donde está el SAP
                string body = parts[2];

                // Último bloque (cantidad)
                string qtyString = parts.Last();
                if (!int.TryParse(qtyString, out int qty))
                    qty = 0;

                // Buscar los últimos 10 dígitos dentro del cuerpo
                var digits = new string(body.Where(char.IsDigit).ToArray());
                if (digits.Length < 10)
                    return null;

                string sap = digits[^10..]; // últimos 10 dígitos

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
            catch
            {
                return null;
            }
        }
    }
}
