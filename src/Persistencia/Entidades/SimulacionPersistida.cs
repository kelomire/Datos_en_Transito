namespace Persistencia.Entidades;

public sealed class SimulacionPersistida
{
    public int Id { get; set; }
    public string Origen { get; set; } = string.Empty;
    public string Destino { get; set; } = string.Empty;
    public int TamanioBytes { get; set; }
    public int Ttl { get; set; }
    public int TtlFinal { get; set; }
    public int LatenciaTotalMs { get; set; }
    public bool Entregado { get; set; }
    public bool Descartado { get; set; }
    public List<PasoRecorridoPersistido> Recorrido { get; set; } = [];
}

public sealed class PasoRecorridoPersistido
{
    public int DispositivoId { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string Tipo { get; set; } = string.Empty;
    public int LatenciaAplicadaMs { get; set; }
    public int TtlRestante { get; set; }
}