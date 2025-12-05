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
            var sw = Stopwatch.StartNew();
            Debug.WriteLine("[MERGE] ═══════════════════════════════════════");
            Debug.WriteLine("[MERGE] 🚀 INICIO DE CARGA CONCURRENTE");
            Debug.WriteLine("[MERGE] ═══════════════════════════════════════");

            // ═══════════════════════════════════════════════════════════════
            // PHASE 1: CONCURRENT DATA LOADING (EPLAN + EXCEL in parallel)
            // ═══════════════════════════════════════════════════════════════
            Debug.WriteLine("[MERGE] Fase 1: Cargando EPLAN y Excel en paralelo...");

            var eplanTask = Task.Run(() =>
            {
                var eplanSw = Stopwatch.StartNew();
                var result = _eplanService.LoadEplanMaterials(eplanFilePath);
                eplanSw.Stop();
                Debug.WriteLine($"[MERGE] ✓ EPLAN completado en {eplanSw.ElapsedMilliseconds}ms: {result.Count} materiales");
                return result;
            });

            var excelTask = Task.Run(() =>
            {
                var excelSw = Stopwatch.StartNew();
                var result = _stdService.LoadAllMaterialsFromExcel(excelPath);
                excelSw.Stop();
                Debug.WriteLine($"[MERGE] ✓ Excel completado en {excelSw.ElapsedMilliseconds}ms: {result.Count} materiales");
                return result;
            });

            // Wait for both EPLAN and Excel to complete
            await Task.WhenAll(eplanTask, excelTask);

            var eplanList = await eplanTask;
            var excelLookup = await excelTask;

            Debug.WriteLine($"[MERGE] ✓ Fase 1 completada en {sw.ElapsedMilliseconds}ms");

            // ═══════════════════════════════════════════════════════════════
            // PHASE 2: PROCESS MATERIALS WITH CONCURRENT API/IMAGE LOADING
            // ═══════════════════════════════════════════════════════════════
            Debug.WriteLine("[MERGE] Fase 2: Procesando materiales con API/imágenes concurrentes...");

            var result = new List<MaterialModel>(eplanList.Count);
            var phase2Sw = Stopwatch.StartNew();

            // Process materials in batches to avoid overwhelming the API
            const int batchSize = 10; // Process 10 materials concurrently at a time
            int totalProcessed = 0;

            for (int i = 0; i < eplanList.Count; i += batchSize)
            {
                var batch = eplanList.Skip(i).Take(batchSize).ToList();

                // Create tasks for each material in the batch
                var batchTasks = batch.Select(async ep =>
                {
                    // Lookup in Excel dictionary (instant - no I/O)
                    excelLookup.TryGetValue(ep.Sap, out var std);

                    // Launch API and Image requests concurrently
                    var sapTask = LoadSapDataAsync(ep.Sap);
                    var imageTask = LoadImageDataAsync(ep.Sap);

                    await Task.WhenAll(sapTask, imageTask);

                    var sap = await sapTask;
                    var imageBytes = await imageTask;

                    // Create final model
                    return new MaterialModel
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
                }).ToList();

                // Wait for the batch to complete
                var batchResults = await Task.WhenAll(batchTasks);
                result.AddRange(batchResults);

                totalProcessed += batchResults.Length;

                if (totalProcessed % 50 == 0 || totalProcessed == eplanList.Count)
                {
                    Debug.WriteLine($"[MERGE] Progreso: {totalProcessed}/{eplanList.Count} materiales procesados...");
                }
            }

            phase2Sw.Stop();
            sw.Stop();

            Debug.WriteLine("[MERGE] ═══════════════════════════════════════");
            Debug.WriteLine($"[MERGE] ✓ Fase 2 completada en {phase2Sw.ElapsedMilliseconds}ms");
            Debug.WriteLine($"[MERGE] ✓ TOTAL COMPLETADO: {result.Count} materiales en {sw.ElapsedMilliseconds}ms");
            Debug.WriteLine($"[MERGE] ⚡ Promedio: {(double)sw.ElapsedMilliseconds / result.Count:F2}ms por material");
            Debug.WriteLine("[MERGE] ═══════════════════════════════════════");

            return result;
        }

        // Helper method for SAP API calls with error handling
        private async Task<SapMatInfoModel?> LoadSapDataAsync(string sap)
        {
            try
            {
                return await _sapService.LoadFromApiAsync("https://md0vm00162.emea.bosch.com/materials/api/", sap);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[MERGE] ⚠️ Error API para SAP {sap}: {ex.Message}");
                return null;
            }
        }

        // Helper method for Image calls with error handling
        private async Task<byte[]?> LoadImageDataAsync(string sap)
        {
            try
            {
                return await _imageService.LoadImageAsync("https://md0vm00162.emea.bosch.com/materials/api/", sap);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[MERGE] ⚠️ Error imagen para SAP {sap}: {ex.Message}");
                return null;
            }
        }
    }
}