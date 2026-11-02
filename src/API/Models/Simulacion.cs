using System.Collections.Generic;

namespace API.Models
{
    public class Simulacion
    {
        public string? Id { get; set; }
        public string? IpOrigen { get; set; }
        public string? IpDestino { get; set; }
        public int TamanioBytes { get; set; }
        public int Ttl { get; set; }
        public List<string> DispositivoIds { get; set; } = new(); // Soluciona advertencia

        public string? Estado { get; set; }
        public int TtlFinal { get; set; }
        public int LatenciaTotalMs { get; set; }
        
        // ✅ Agregá esta línea que falta
        public List<PasoRecorrido> Recorrido { get; set; } = new();
    }

    // También definí esta clase acá o en su propio archivo
    public class PasoRecorrido
    {
        public int Orden { get; set; }
        public string? DispositivoId { get; set; }
        public int LatenciaAplicadaMs { get; set; }
        public int TtlRestante { get; set; }
    }
}
