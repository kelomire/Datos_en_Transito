using Persistencia;   // ✅ Repositorio está acá
using Aplicacion;      // ✅ ServicioB está acá
using Xunit;

namespace Tests
{
    public class ServicioBTests
    {
        [Fact]
        public void Simulacion_SumalatenciaMsDeTodosLosDispositivos()
        {
            var repositorio = new Repositorio();
            repositorio.Dispositivos.Add(new Dispositivo { Id = "d1", LatenciaMs = 1 });
            repositorio.Dispositivos.Add(new Dispositivo { Id = "d2", LatenciaMs = 3 });
            repositorio.Dispositivos.Add(new Dispositivo { Id = "d3", LatenciaMs = 5 });

            var servicio = new ServicioB(repositorio);
            var datos = new SimulacionDatos
            {
                Origen = "A", Destino = "B",
                TamanioBytes = 100,
                Ttl = 10,
                DispositivoIds = new List<string> { "d1", "d2", "d3" }
            };

            var resultado = servicio.Ejecutar(datos);

            // Esperado: 1+3+5=9
            Assert.Equal(9, resultado.LatenciaTotalMs);
        }

        [Fact]
        public void Simulacion_SeDetieneAlDescartarYSumalatenciaMsHastaAhi()
        {
            var repositorio = new Repositorio();
            var servicio = new ServicioB(repositorio);
            var datos = new SimulacionDatos
            {
                Origen = "A", Destino = "B",
                TamanioBytes = 50,
                Ttl = 2,
                DispositivoIds = new List<string> { "d1", "d2", "d3" }
            };

            var resultado = servicio.Ejecutar(datos);

            // Esperado: 1+3 = 4 → se detiene antes del tercero
            Assert.Equal(4, resultado.LatenciaTotalMs);
        }

        [Fact]
        public void Simulacion_DescartaPorTtlEnVariosRouters()
        {
            var repositorio = new Repositorio();
            repositorio.Dispositivos.Add(new Dispositivo { Id = "r1", LatenciaMs = 2 });
            repositorio.Dispositivos.Add(new Dispositivo { Id = "r2", LatenciaMs = 50 });

            var servicio = new ServicioB(repositorio);
            var datos = new SimulacionDatos
            {
                Origen = "O", Destino = "D",
                TamanioBytes = 200,
                Ttl = 1,
                DispositivoIds = new List<string> { "r1", "r2" }
            };

            var resultado = servicio.Ejecutar(datos);

            // Esperado: solo r1 → 2
            Assert.Equal(2, resultado.LatenciaTotalMs);
        }
    }
}
