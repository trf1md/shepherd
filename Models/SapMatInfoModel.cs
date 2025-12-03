namespace ShepherdEplan.Models
{
    public sealed class SapMatInfoModel
    {
        public string Sap { get; set; } = string.Empty;

        public string? Description1 { get; set; }
        public string? Description2 { get; set; }
        public string? Description3 { get; set; }

        public string? Note { get; set; }

        public string? Provider { get; set; }
        public string? ProviderRef { get; set; }

        public string? Status { get; set; }
        public string? LocalEngStatus { get; set; }
        public string? LocalStockStatus { get; set; }

        public string? Category { get; set; }
        public string? Creator { get; set; }

        public double? Depth { get; set; }
        public double? Height { get; set; }
        public double? Width { get; set; }

        public DateTime? UpdateInfoDateTime { get; set; }
    }
}
