namespace API
{
    public class Almacenamiento
    {
        public Dictionary<string, Dispositivo> Dispositivos { get; set; } = new();
        public Dictionary<string, Simulacion> Simulaciones { get; set; } = new();
    }

    public class Dispositivo
    {
        public string? Id { get; set; }
        public string? Nombre { get; set; }
        public string? Tipo { get; set; }
        public int LatenciaMs { get; set; }
    }
}
