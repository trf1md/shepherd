namespace ShepherdEplan.Models
{
    /// <summary>
    /// 
    /// </summary>
    public sealed class ConfigModel
    {
        /// <summary>
        /// Eplan sapt txt file (material list) path
        /// </summary>
        public string? EplanSapFilePath { get; set; }
        public string? StandardExcelFilePath { get; set; }
        public string? ApiBaseUrl { get; set; }
        public string? LogFilePath { get; set; }
    }

}


