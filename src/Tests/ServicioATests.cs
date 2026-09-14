using Aplicacion.Entidades;
using Aplicacion.Interfaces;
using Aplicacion.Servicios;
using Xunit;

namespace Tests;

public class ServicioATests
{
    [Fact]
    public void Router_Debe_Procesar_Paquete()
    {
        var repo = new FakeSimulacionRepository();
        var service = new SimulacionService(repo);

        var paquete = new PaqueteRed(
            "10.0.0.1",
            "10.0.0.2",
            500,
            5);

        var simulacion = service.Ejecutar(
            paquete,
            new DispositivoRed[]
            {
                new Router(1, "Router1")
            });

        Assert.Single(simulacion.Resultados);
        Assert.Equal("Router", simulacion.Resultados[0].TipoDispositivo);
        Assert.Equal(10, simulacion.LatenciaTotal);
        Assert.True(paquete.Entregado);
    }

    [Fact]
    public void Varios_Dispositivos_Deben_Usar_Polimorfismo()
    {
        var repo = new FakeSimulacionRepository();
        var service = new SimulacionService(repo);

        var paquete = new PaqueteRed(
            "10.0.0.1",
            "10.0.0.2",
            100,
            10);

        var simulacion = service.Ejecutar(
            paquete,
            new DispositivoRed[]
            {
                new Router(1, "R1"),
                new Switch(2, "S1"),
                new AccessPoint(3, "AP1")
            });

        Assert.Equal(3, simulacion.Resultados.Count);
        Assert.Equal(18, simulacion.LatenciaTotal);
        Assert.True(paquete.Entregado);
    }

    private class FakeSimulacionRepository : ISimulacionRepository
    {
        private readonly List<Simulacion> _items = new();

        public void Registrar(Simulacion simulacion)
        {
            _items.Add(simulacion);
        }

        public Simulacion? ObtenerPorId(int id)
        {
            return _items.FirstOrDefault(x => x.Id == id);
        }

        public IEnumerable<Simulacion> ObtenerTodas()
        {
            return _items;
        }
    }
}
