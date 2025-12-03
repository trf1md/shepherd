using System.Net.Http;

namespace ShepherdEplan.Services.Images
{
    public sealed class GetImageService
    {
        private readonly HttpClient _http;

        public GetImageService(HttpClient http)
        {
            _http = http;
        }

        public async Task<byte[]?> LoadImageAsync(string apiBaseUrl, string sap)
        {
            // URL de la imagen (placeholder, tú la ajustarás)
            string url = $"{apiBaseUrl.TrimEnd('/')}/MaterialInfo/image/{sap}";

            try
            {
                var bytes = await _http.GetByteArrayAsync(url);
                return bytes.Length > 0 ? bytes : null;
            }
            catch
            {
                return null;
            }
        }
    }
}
