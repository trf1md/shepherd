using ShepherdEplan.Services.SAP;
using ShepherdEplan.Services.Standard;
using ShepherdEplan.Models;
using ShepherdEplan.Services.Eplan;
using ShepherdEplan.Services.Images;
using ShepherdEplan.Services.SAP;
using ShepherdEplan.Services.Standard;

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

            // 1. Datos EPLAN (lista base)
            var eplanList = _eplanService.LoadEplanMaterials(eplanFilePath);

            foreach (var ep in eplanList)
            {
                // 2. Excel estándar (buscar SAP en todas las hojas)
                var std = _stdService.LoadMaterialFromExcel(excelPath, ep.Sap);

                // 3. API SAP
                var sap = await _sapService.LoadFromApiAsync(apiBaseUrl, ep.Sap);

                // 4. Imagen
                byte[]? imageBytes = null;
                try
                {
                    imageBytes = await _imageService.LoadImageAsync(apiBaseUrl, ep.Sap);
                }
                catch
                {
                    imageBytes = null;
                }

                // 5. Crear el modelo final
                var mat = new MaterialModel
                {
                    // Imagen
                    ImageBytes = imageBytes,

                    // EPLAN
                    Location = ep.Location,
                    Group = ep.Group,
                    Sap = ep.Sap,
                    Units = ep.Units,
                    Quantity = ep.Units,

                    // Excel (Std)
                    Comments = std?.Comments,
                    Description = std?.Description,
                    Category = std?.Category,
                    Status = std?.Status,
                    Stock = std?.Stock,
                    Creator = std?.Creator,

                    // API (SAP)
                    Provider = sap?.Provider,
                    ProviderRef = sap?.ProviderRef
                };

                // Añadir a la lista final
                result.Add(mat);
            }

            return result;
        }
    }
}
