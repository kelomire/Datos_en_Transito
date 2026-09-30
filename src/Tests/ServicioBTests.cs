using Persistencia.Entidades;
using Xunit;

namespace Tests;

public class ServicioBTests
{
    [Fact]
    public void Paquete_Descartarse()
    {
        var paquete = new PaqueteRed(
            "10.0.0.1",
            "10.0.0.2",
            100,
            1);

        var router = new Router(1, "Router");

        var resultado = router.Procesar(paquete);

        Assert.True(resultado.Descartado);
        Assert.Equal(0, resultado.TtlFinal);
    }

    [Fact]
    public void Firewall()
    {
        var paquete = new PaqueteRed(
            "192.0.2.10",
            "10.0.0.2",
            100,
            10);

        var firewall = new Firewall(1, "Firewall");

        var resultado = firewall.Procesar(paquete);

        Assert.True(resultado.Descartado);
    }

    [Fact]
    public void Paquete_Validar()
    {
        Assert.Throws<ArgumentException>(() =>
            new PaqueteRed(
                "10.0.0.1",
                "10.0.0.2",
                0,
                10));
    }

    [Fact]
    public void Paquete_TTL()
    {
        Assert.Throws<ArgumentException>(() =>
            new PaqueteRed(
                "10.0.0.1",
                "10.0.0.2",
                100,
                0));
    }
}
