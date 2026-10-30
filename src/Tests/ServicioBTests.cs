using Datos_en_Transito.Persistencia.Entidades;
using Xunit;

namespace Tests;

// Latencia y recorrido completo de una simulación.
public class ServicioBTests
{
    [Fact]
    public void Dispositivo_RechazaLatenciaNegativa()
        => Assert.Throws<ArgumentOutOfRangeException>(() => new Switch("sw", -1));

    [Fact]
    public void Dispositivo_AceptaLatenciaCero()
        => Assert.Equal(0, new Switch("sw", 0).LatenciaMs);

    [Fact]
    public void Simulacion_SumaLatenciaDeTodosLosDispositivos()
    {
        var sim = new Simulacion(new PaqueteRed(500, 10),
            new DispositivoRed[] { new Switch("sw", 1), new Router("r", 5), new Firewall("fw", 3) });
        sim.Ejecutar();
        Assert.True(sim.Entregado);
        Assert.Equal(9, sim.LatenciaTotalMs);
        Assert.Equal(3, sim.Pasos.Count);
    }

    [Fact]
    public void Simulacion_SeDetieneAlDescartarYCuentaLaLatenciaHastaAhi()
    {
        var sim = new Simulacion(new PaqueteRed(2000, 10),
            new DispositivoRed[] { new Switch("sw", 1), new Firewall("fw", 3), new Router("r", 5) });
        sim.Ejecutar();
        Assert.False(sim.Entregado);
        Assert.Equal(4, sim.LatenciaTotalMs);
        Assert.Equal(2, sim.Pasos.Count);
    }

    [Fact]
    public void Simulacion_DescartaPorTtlEnVariosRouters()
    {
        var sim = new Simulacion(new PaqueteRed(100, 2),
            new DispositivoRed[] { new Router("r1", 1), new Router("r2", 1), new Router("r3", 1) });
        sim.Ejecutar();
        Assert.False(sim.Entregado);
        Assert.Equal(2, sim.Pasos.Count);
    }
}