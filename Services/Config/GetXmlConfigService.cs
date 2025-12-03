using ShepherdEplan.Models;
using ShepherdEplan.Services.Common;
using System.Xml.Linq;

namespace ShepherdEplan.Services.Config
{
    public sealed class GetXmlConfigService
    {
        private readonly LogService? _logger;

        public GetXmlConfigService(LogService? logger = null)
        {
            _logger = logger;
        }

        public ConfigModel Load(string filePath)
        {
            if (!File.Exists(filePath))
                throw new FileNotFoundException($"Config file not found: {filePath}");

            try
            {
                var doc = XDocument.Load(filePath);
                var root = doc.Root ?? throw new InvalidDataException("Config XML root element missing.");

                string? Get(string name) => root.Element(name)?.Value?.Trim();

                return new ConfigModel
                {
                    EplanSapFilePath = Get(nameof(ConfigModel.EplanSapFilePath)),
                    StandardExcelFilePath = Get(nameof(ConfigModel.StandardExcelFilePath)),
                    ApiBaseUrl = Get(nameof(ConfigModel.ApiBaseUrl)),
                    LogFilePath = Get(nameof(ConfigModel.LogFilePath))
                };
            }
            catch (Exception ex)
            {
                _logger?.Error("Error loading config.xml", ex);
                throw;
            }
        }
    }
}
