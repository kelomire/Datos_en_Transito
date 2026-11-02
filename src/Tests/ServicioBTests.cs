using Persistencia;
using Aplicacion;
using Xunit;

namespace Aplicacion
{
    public class ServicioBTests
    {
        private readonly Repositorio repositorio;

        public ServicioB(Repositorio repositorio)
        {
            this.repositorio = repositorio;
        }

        [Fact]
        public SimulacionResultado Ejecutar(SimulacionDatos datos)
        {
            // ✅ Declarada UNA SOLA VEZ — FUERA del bucle
            int latenciaTotalMs = 0;
            int ttlActual = datos.Ttl;
            string estado = "Completada";
            var recorrido = new List<PasoRecorrido>();

            foreach (var idDisp in datos.DispositivoIds)
            {
                var dispositivo = repositorio.Dispositivos
                    .FirstOrDefault(d => d.Id == idDisp);

                if (dispositivo == null)
                    continue;

                // ✅ ORDEN CORRECTO: PRIMERO sumar
                latenciaTotalMs += dispositivo.LatenciaMs;

                // DESPUÉS restar TTL
                ttlActual--;

                recorrido.Add(new PasoRecorrido
                {
                    Orden = recorrido.Count + 1,
                    DispositivoId = idDisp,
                    LatenciaAplicadaMs = dispositivo.LatenciaMs,
                    TtlRestante = ttlActual
                });

                if (ttlActual <= 0)
                {
                    estado = "Interrumpida por TTL agotado";
                    ttlActual = 0;
                    break;
                }
            }

            return new SimulacionResultado
            {
                Id = System.Guid.NewGuid().ToString(),
                Estado = estado,
                TtlFinal = ttlActual,
                LatenciaTotalMs = latenciaTotalMs, // ✅ Valor calculado
                Recorrido = recorrido
            };
        }
       
    }

    public class SimulacionDatos
    {
        public string Origen { get; set; }
        public string Destino { get; set; }
        public int TamanioBytes { get; set; }
        public int Ttl { get; set; }
        public List<string> DispositivoIds { get; set; } = new();
    }

    public class SimulacionResultado
    {
        public string Id { get; set; }
        public string Estado { get; set; }
        public int TtlFinal { get; set; }
        public int LatenciaTotalMs { get; set; }
        public List<PasoRecorrido> Recorrido { get; set; } = new();
    }

    public class PasoRecorrido
    {
        public int Orden { get; set; }
        public string DispositivoId { get; set; }
        public int LatenciaAplicadaMs { get; set; }
        public int TtlRestante { get; set; }
    }
}
