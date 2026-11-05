namespace API.Models;

public class Dispositivo
{
    public int Id { get; set; }
    public string? Nombre { get; set; }
    public string? Tipo { get; set; }
    public int LatenciaMs { get; set; }
}