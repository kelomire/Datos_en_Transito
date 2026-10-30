using System;
using Persistencia.Entidades;
using Xunit;

namespace Tests;

public class ServicioBTests
{
    private static PaqueteRed Paquete(int bytes = 500, int ttl = 10)
        => new("10.0.0.1", "10.0.0.2", bytes, ttl);

    [Fact]
    public void Dispositivo_RechazaLatenciaNegativa()
        => Assert.Throws<ArgumentException>(() => new Switch(1, "sw", -1));

    [Fact]
    public void Dispositivo_AceptaLatenciaCero()
        => Assert.Equal(0, new Switch(1, "sw", 0).Latencia);

    [Fact]
    public void Simulacion_SumaLatenciaDeTodosLosDispositivos()
    {
        var sim = new Simulacion(Paquete(), new DispositivoRed[]
        {
            new Switch(1, "sw", 1), new Router(2, "r", 5), new Firewall(3, "fw", 3)
        });
        sim.Ejecutar();
        Assert.True(sim.Paquete.Entregado);
        Assert.Equal(9, sim.LatenciaTotal);
        Assert.Equal(3, sim.Resultados.Count);
    }

    [Fact]
    public void Simulacion_SeDetieneAlDescartarYSumaLatenciaHastaAhi()
    {
        var sim = new Simulacion(Paquete(bytes: 2000), new DispositivoRed[]
        {
            new Switch(1, "sw", 1), new Firewall(2, "fw", 3), new Router(3, "r", 5)
        });
        sim.Ejecutar();
        Assert.True(sim.Paquete.Descartado);
        Assert.False(sim.Paquete.Entregado);
        Assert.Equal(4, sim.LatenciaTotal);
        Assert.Equal(2, sim.Resultados.Count);
    }

    [Fact]
    public void Simulacion_DescartaPorTtlEnVariosRouters()
    {
        var sim = new Simulacion(Paquete(ttl: 2), new DispositivoRed[]
        {
            new Router(1, "r1", 1), new Router(2, "r2", 1), new Router(3, "r3", 1)
        });
        sim.Ejecutar();
        Assert.True(sim.Paquete.Descartado);
        Assert.Equal(2, sim.Resultados.Count);
        Assert.Equal(2, sim.LatenciaTotal);
    }
}
