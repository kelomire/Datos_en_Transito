namespace Persistencia.Entidades;

public class Firewall : DispositivoRed, IFiltrador
{
    public Firewall(int id, string nombre, int latencia = 8)
        : base(id, nombre, latencia) { }

    public bool PuedeProcesar(PaqueteRed paquete)
    {
        return !paquete.IpOrigen.StartsWith("192.0.2.");
    }

    public override ResultadoProcesamiento Procesar(PaqueteRed paquete)
    {
        if (!PuedeProcesar(paquete))
            paquete.Descartado = true;
        else
            paquete.Ttl--;

        if (paquete.Ttl <= 0)
            paquete.Descartado = true;

        return new ResultadoProcesamiento
        {
            DispositivoId = Id,
            TipoDispositivo = nameof(Firewall),
            Latencia = Latencia,
            TtlFinal = paquete.Ttl,
            Entregado = paquete.Entregado,
            Descartado = paquete.Descartado
        };
    }
}
