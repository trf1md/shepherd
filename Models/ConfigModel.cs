namespace ShepherdEplan.Models
{
    public sealed class ConfigModel
    {
        public string? EplanSapFilePath { get; set; }
        public string? StandardExcelFilePath { get; set; }
        public string? ApiBaseUrl { get; set; }
        public string? LogFilePath { get; set; }
    }
}
