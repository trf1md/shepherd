namespace ShepherdEplan.Models
{
    public sealed class StdMatInfoModel
    {
        public string Sap { get; set; } = string.Empty;

        public string? Status { get; set; }          // Standard / NonStandard / Blocked / Warning...
        public string? Stock { get; set; }         // V1 / Undefined...
        public string? Category { get; set; }        // Cables, Panels, Servos...
        public string? Description { get; set; }
        public string? Comments { get; set; }
        public string? Creator { get; set; }
    }
}
