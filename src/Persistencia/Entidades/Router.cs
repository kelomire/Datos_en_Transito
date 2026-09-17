namespace Persistencia.Entidades;

public class Router : DispositivoRed
{
    public Router(int id, string nombre, int latencia = 10)
        : base(id, nombre, latencia) { }

    public override ResultadoProcesamiento Procesar(PaqueteRed paquete)
    {
        paquete.Ttl--;

        if (paquete.Ttl <= 0)
            paquete.Descartado = true;

        return new ResultadoProcesamiento
        {
            DispositivoId = Id,
            TipoDispositivo = nameof(Router),
            Latencia = Latencia,
            TtlFinal = paquete.Ttl,
            Entregado = paquete.Entregado,
            Descartado = paquete.Descartado
        };
    }
}
