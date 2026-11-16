namespace Aplicacion.Modelos;

public sealed class SimulacionResultado
{
    public int Id { get; set; }
    public string Origen { get; set; } = string.Empty;
    public string Destino { get; set; } = string.Empty;
    public int TamanioBytes { get; set; }
    public int Ttl { get; set; }
    public string Estado { get; set; } = string.Empty;
    public int TtlFinal { get; set; }
    public int LatenciaTotalMs { get; set; }
    public List<PasoSimulacionResultado> Recorrido { get; set; } = [];
}

public sealed class PasoSimulacionResultado
{
    public int Orden { get; set; }
    public int DispositivoId { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string Tipo { get; set; } = string.Empty;
    public int LatenciaAplicadaMs { get; set; }
    public int TtlRestante { get; set; }
}

public sealed class ResumenSimulacionResultado
{
    public int Id { get; set; }
    public string Estado { get; set; } = string.Empty;
    public int TtlFinal { get; set; }
    public int LatenciaTotalMs { get; set; }
}