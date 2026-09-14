namespace Aplicacion.Entidades;
public class PaqueteRed
{
    public int Id { get; set; }
    public string IpOrigen { get; set; }
    public string IpDestino { get; set; }
    public int TamanoBytes { get; set; }
    public int Ttl { get; set; }
    public bool Entregado { get; set; }
    public bool Descartado { get; set; }

    public PaqueteRed(string ipOrigen, string ipDestino, int tamanoBytes, int ttl)
    {
        if (string.IsNullOrWhiteSpace(ipOrigen)) throw new ArgumentException("IP origen inválida");
        if (string.IsNullOrWhiteSpace(ipDestino)) throw new ArgumentException("IP destino inválida");
        if (tamanoBytes <= 0) throw new ArgumentException("El tamaño debe ser mayor a cero");
        if (ttl <= 0) throw new ArgumentException("TTL debe ser mayor a cero");

        IpOrigen = ipOrigen;
        IpDestino = ipDestino;
        TamanoBytes = tamanoBytes;
        Ttl = ttl;
    }
}
