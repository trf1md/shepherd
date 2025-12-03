using System.Net.Http.Json;
using ShepherdEplan.Models;

namespace ShepherdEplan.Services.SAP
{
    public sealed class GetSapMaterialInfoService
    {
        private readonly HttpClient _http;

        public GetSapMaterialInfoService(HttpClient httpClient)
        {
            _http = httpClient;
        }

        public async Task<SapMatInfoModel?> LoadFromApiAsync(string apiBaseUrl, string sap)
        {
            if (string.IsNullOrWhiteSpace(apiBaseUrl))
                throw new ArgumentException("ApiBaseUrl no puede ser null ni vacío.");

            string url = $"{apiBaseUrl.TrimEnd('/')}/MaterialInfo/MaterialFullInfo/{sap}";

            try
            {
                var dto = await _http.GetFromJsonAsync<ApiResponseDto>(url);
                if (dto == null)
                    return null;

                return Map(dto);
            }
            catch
            {
                return null;
            }
        }

        private static SapMatInfoModel Map(ApiResponseDto dto)
        {
            return new SapMatInfoModel
            {
                Sap = dto.sap ?? string.Empty,

                Description1 = dto.description,
                Description2 = dto.eDescription1,
                Description3 = dto.eDescription2,

                Note = dto.eNote,

                Provider = dto.eProvider,
                ProviderRef = dto.eProviderRef,

                Status = dto.localENGStatus?.ToString(),
                LocalEngStatus = dto.localENGStatus?.ToString(),
                LocalStockStatus = dto.localStockStatus?.ToString(),

                Category = dto.category,
                Creator = dto.creatorInfo,

                Depth = dto.eDepth,
                Height = dto.eHeight,
                Width = dto.eWidth,

                UpdateInfoDateTime = dto.updateInfoDateTime
            };
        }

        // DTO interno basado EXACTAMENTE en la estructura que me pasaste
        private sealed class ApiResponseDto
        {
            public string? sap { get; set; }
            public string? description { get; set; }
            public string? commentsInfo { get; set; }
            public int? localENGStatus { get; set; }
            public int? localStockStatus { get; set; }
            public string? category { get; set; }
            public string? creatorInfo { get; set; }
            public string? eDescription1 { get; set; }
            public string? eDescription2 { get; set; }
            public string? eDescription3 { get; set; }
            public string? eNote { get; set; }
            public string? eProvider { get; set; }
            public string? eProviderRef { get; set; }
            public double? eDepth { get; set; }
            public double? eHeight { get; set; }
            public double? eWidth { get; set; }
            public DateTime? updateInfoDateTime { get; set; }
        }
    }
}
