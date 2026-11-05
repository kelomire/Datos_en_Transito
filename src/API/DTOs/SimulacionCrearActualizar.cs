namespace API.DTOs;

public class SimulacionCrearActualizar
{
    public string? Origen { get; set; }
    public string? Destino { get; set; }
    public int TamanioBytes { get; set; }
    public int Ttl { get; set; }
    public List<int> DispositivoIds { get; set; } = new();
}

