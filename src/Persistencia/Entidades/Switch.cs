namespace Persistencia.Entidades;

public class Switch : DispositivoRed
{
    public Switch(int id, string nombre, int latencia = 3)
        : base(id, nombre, latencia) { }

    public override ResultadoProcesamiento Procesar(PaqueteRed paquete)
    {
        // El switch deja continuar al paquete sin modificar el TTL.
        return new ResultadoProcesamiento
        {
            DispositivoId = Id,
            TipoDispositivo = nameof(Switch),
            Latencia = Latencia,
            TtlFinal = paquete.Ttl,
            Entregado = paquete.Entregado,
            Descartado = paquete.Descartado
        };
    }
}
