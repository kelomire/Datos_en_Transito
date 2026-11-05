using Persistencia.Entidades;
using Xunit;

namespace Tests
{
    public class ServicioBTests
    {
        [Fact]
        public void Simulacion_SumaLatenciaMsDeTodosLosDispositivos()
        {
            var paquete = new PaqueteRed("10.0.0.1", "10.0.0.2", 100, 10);
            var dispositivos = new List<DispositivoRed>
            {
                new Router(1, "d1", 1),
                new Router(2, "d2", 3),
                new Router(3, "d3", 5)
            };

            var simulacion = new Simulacion(paquete, dispositivos);
            simulacion.Ejecutar();

            Assert.Equal(9, simulacion.latenciaMsTotal);
        }

        [Fact]
        public void Simulacion_SeDetieneAlDescartarYSumaLatenciaMsHastaAhi()
        {
            var paquete = new PaqueteRed("10.0.0.1", "10.0.0.2", 50, 2);
            var dispositivos = new List<DispositivoRed>
            {
                new Router(1, "d1", 1),
                new Router(2, "d2", 3),
                new Router(3, "d3", 5)
            };

            var simulacion = new Simulacion(paquete, dispositivos);
            simulacion.Ejecutar();

            Assert.Equal(4, simulacion.latenciaMsTotal);
        }
    }
}
