namespace ShepherdEplan.Models
{
    public sealed class MaterialModel
    {
        // Datos EPLAN
        public string? Location { get; set; }
        public string? Group { get; set; }
        public int? Units { get; set; }

        // Identificación
        public string Sap { get; set; } = string.Empty;

        // Comentarios / descripción visibles en la tabla
        public string? Comments { get; set; }
        public string? Description { get; set; }
        public string? Category { get; set; }

        // Estado estándar / bloqueo / warning
        public string? Status { get; set; }      // texto
        public bool IsStandard { get; set; }
        public bool IsWarning { get; set; }
        public bool IsBlocked { get; set; } // Diferenciar entre blocked y forbidden
        // Blocked tiene prioridad sobre el resto de estados
        // Añ


        // Stock y proveedor
        public string? Stock { get; set; }
        public string? Provider { get; set; }
        public string? ProviderRef { get; set; }

        // Datos de proyecto
        public int? Quantity { get; set; }
        public string? Creator { get; set; }

        // Imagen
        public byte[]? ImageBytes { get; set; }
    }
}
