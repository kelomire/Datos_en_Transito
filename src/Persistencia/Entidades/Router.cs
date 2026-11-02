namespace Persistencia.Entidades;

public class Router : DispositivoRed
{
    public Router(int id, string nombre, int latenciaMs = 10)
        : base(id, nombre, latenciaMs) { }

    public override ResultadoProcesamiento Procesar(PaqueteRed paquete)
    {
        paquete.Ttl--;

        if (paquete.Ttl <= 0)
            paquete.Descartado = true;

        return new ResultadoProcesamiento
        {
            DispositivoId = Id,
            TipoDispositivo = nameof(Router),
            latenciaMs = latenciaMs,
            TtlFinal = paquete.Ttl,
            Entregado = paquete.Entregado,
            Descartado = paquete.Descartado
        };
    }
}
