using System.Collections.Generic;

namespace API.DTOs
{
    public class SimulacionCrearActualizar
    {
        public string? IpOrigen { get; set; }
        public string? IpDestino { get; set; }
        public int TamanioBytes { get; set; }
        public int Ttl { get; set; }
        // ✅ Inicializá la lista vacía
        public List<string> DispositivoIds { get; set; } = new();
    }
}
