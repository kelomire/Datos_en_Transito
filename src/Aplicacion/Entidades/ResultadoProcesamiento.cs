namespace Aplicacion.Entidades;
public class ResultadoProcesamiento
{
    public int DispositivoId { get; set; }
    public string TipoDispositivo { get; set; } = "";
    public int Latencia { get; set; }
    public int TtlFinal { get; set; }
    public bool Entregado { get; set; }
    public bool Descartado { get; set; }
}
