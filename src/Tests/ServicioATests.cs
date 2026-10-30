using System;
using Persistencia.Entidades;
using Xunit;

namespace Tests;

public class ServicioATests
{
    private static PaqueteRed Paquete(int bytes = 500, int ttl = 5)
        => new("10.0.0.1", "10.0.0.2", bytes, ttl);

    [Fact]
    public void Switch_Continua_SinModificarTtl()
    {
        var p = Paquete(ttl: 5);
        var r = new Switch(1, "sw", 1).Procesar(p);
        Assert.False(r.Descartado);
        Assert.Equal(5, r.TtlFinal);
        Assert.Equal(5, p.Ttl);
    }

    [Fact]
    public void Router_ReduceTtlEnUno()
    {
        var p = Paquete(ttl: 5);
        var r = new Router(1, "r", 2).Procesar(p);
        Assert.False(r.Descartado);
        Assert.Equal(4, r.TtlFinal);
    }

    [Fact]
    public void Router_DescartaCuandoTtlLlegaACero()
    {
        var p = Paquete(ttl: 1);
        var r = new Router(1, "r", 2).Procesar(p);
        Assert.True(r.Descartado);
        Assert.Equal(0, r.TtlFinal);
    }

    [Theory]
    [InlineData(1500, false)]
    [InlineData(1501, true)]
    public void Firewall_DescartaMasDe1500Bytes(int bytes, bool descartado)
    {
        var r = new Firewall(1, "fw", 1).Procesar(Paquete(bytes: bytes));
        Assert.Equal(descartado, r.Descartado);
    }

    [Fact]
    public void Firewall_NoModificaElTtl()
    {
        var r = new Firewall(1, "fw", 1).Procesar(Paquete(ttl: 5));
        Assert.Equal(5, r.TtlFinal);
    }

    [Theory]
    [InlineData(0, 5)]
    [InlineData(-1, 5)]
    [InlineData(100, 0)]
    [InlineData(100, -3)]
    public void Paquete_RechazaTamanoOTtlNoPositivos(int bytes, int ttl)
        => Assert.Throws<ArgumentException>(() => new PaqueteRed("10.0.0.1", "10.0.0.2", bytes, ttl));
}
