using Datos_en_Transito.Persistencia.Entidades;
using Xunit;

namespace Tests;

// Reglas de cada dispositivo: continuidad, descarte y TTL.
public class ServicioATests
{
    [Fact]
    public void Switch_Continua_SinModificarTtl()
    {
        var r = new Switch("sw1", 1).Procesar(new PaqueteRed(500, 5));
        Assert.True(r.Continua);
        Assert.Equal(5, r.Paquete!.Ttl);
    }

    [Fact]
    public void Router_ReduceTtlEnUno()
    {
        var r = new Router("r1", 2).Procesar(new PaqueteRed(500, 5));
        Assert.True(r.Continua);
        Assert.Equal(4, r.Paquete!.Ttl);
    }

    [Fact]
    public void Router_DescartaCuandoTtlLlegaACero()
    {
        var r = new Router("r1", 2).Procesar(new PaqueteRed(500, 1));
        Assert.False(r.Continua);
        Assert.Null(r.Paquete);
    }

    [Theory]
    [InlineData(1500, true)]
    [InlineData(1501, false)]
    public void Firewall_DescartaMasDe1500Bytes(int bytes, bool continua)
        => Assert.Equal(continua, new Firewall("fw", 1).Procesar(new PaqueteRed(bytes, 5)).Continua);

    [Theory]
    [InlineData(0, 5)]
    [InlineData(-1, 5)]
    [InlineData(100, 0)]
    [InlineData(100, -3)]
    public void Paquete_RechazaTamanioOTtlNoPositivos(int bytes, int ttl)
        => Assert.Throws<ArgumentOutOfRangeException>(() => new PaqueteRed(bytes, ttl));
}