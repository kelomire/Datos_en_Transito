namespace Persistencia.Entidades;

public class Switch : DispositivoRed
{
    public Switch(int id, string nombre, int latenciaMs = 3)
        : base(id, nombre, latenciaMs) { }

    public override ResultadoProcesamiento Procesar(PaqueteRed paquete)
    {
        // El switch deja continuar al paquete sin modificar el TTL.
        return new ResultadoProcesamiento
        {
            DispositivoId = Id,
            TipoDispositivo = nameof(Switch),
            latenciaMs = latenciaMs,
            TtlFinal = paquete.Ttl,
            Entregado = paquete.Entregado,
            Descartado = paquete.Descartado
        };
    }
}
