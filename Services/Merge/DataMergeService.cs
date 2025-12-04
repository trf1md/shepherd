using System.Diagnostics;
using ShepherdEplan.Services.SAP;
using ShepherdEplan.Services.Standard;
using ShepherdEplan.Models;
using ShepherdEplan.Services.Eplan;
using ShepherdEplan.Services.Images;

namespace ShepherdEplan.Services.Merge
{
    public sealed class DataMergeService
    {
        private readonly GetEplanMatInfoService _eplanService;
        private readonly GetStdMatInfoService _stdService;
        private readonly GetSapMaterialInfoService _sapService;
        private readonly GetImageService _imageService;

        public DataMergeService(
            GetEplanMatInfoService eplan,
            GetStdMatInfoService std,
            GetSapMaterialInfoService sap,
            GetImageService image)
        {
            _eplanService = eplan;
            _stdService = std;
            _sapService = sap;
            _imageService = image;
        }

        public async Task<List<MaterialModel>> BuildMaterialListAsync(
            string eplanFilePath,
            string excelPath,
            string apiBaseUrl)
        {
            var result = new List<MaterialModel>();

            Debug.WriteLine("[MERGE] Paso 1: Cargando datos EPLAN...");

            // 1. Load EPLAN data (run on background thread to avoid blocking)
            var eplanList = await Task.Run(() => _eplanService.LoadEplanMaterials(eplanFilePath));
            Debug.WriteLine($"[MERGE] EPLAN cargado: {eplanList.Count} materiales");

            Debug.WriteLine("[MERGE] Paso 2: Cargando Excel completo UNA SOLA VEZ...");

            // 2. Load Excel ONCE and build a dictionary (CRITICAL FIX!)
            var excelLookup = await Task.Run(() => _stdService.LoadAllMaterialsFromExcel(excelPath));
            Debug.WriteLine($"[MERGE] Excel cargado: {excelLookup.Count} materiales en diccionario");

            Debug.WriteLine("[MERGE] Paso 3: Procesando cada material...");

            int processedCount = 0;
            foreach (var ep in eplanList)
            {
                processedCount++;
                if (processedCount % 10 == 0)
                {
                    Debug.WriteLine($"[MERGE] Procesados {processedCount}/{eplanList.Count}...");
                }

                // 3. Lookup in Excel dictionary (no file I/O!)
                excelLookup.TryGetValue(ep.Sap, out var std);

                // 4. API SAP (already async)
                SapMatInfoModel? sap = null;
                try
                {
                    sap = await _sapService.LoadFromApiAsync(apiBaseUrl, ep.Sap);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"[MERGE] Error API para SAP {ep.Sap}: {ex.Message}");
                }

                // 5. Image (already async)
                byte[]? imageBytes = null;
                try
                {
                    imageBytes = await _imageService.LoadImageAsync(apiBaseUrl, ep.Sap);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"[MERGE] Error imagen para SAP {ep.Sap}: {ex.Message}");
                }

                // 6. Create final model
                var mat = new MaterialModel
                {
                    ImageBytes = imageBytes,
                    Location = ep.Location,
                    Group = ep.Group,
                    Sap = ep.Sap,
                    Units = ep.Units,
                    Quantity = ep.Units,
                    Comments = std?.Comments,
                    Description = std?.Description,
                    Category = std?.Category,
                    Status = std?.Status,
                    Stock = std?.Stock,
                    Creator = std?.Creator,
                    Provider = sap?.Provider,
                    ProviderRef = sap?.ProviderRef
                };

                result.Add(mat);
            }

            Debug.WriteLine($"[MERGE] ✓ Completado: {result.Count} materiales procesados");
            return result;
        }
    }
}