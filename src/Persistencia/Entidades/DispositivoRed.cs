namespace Persistencia.Entidades;
public abstract class DispositivoRed
{
    public int Id { get; set; }
    public string Nombre { get; set; }
    public int latenciaMs { get; protected set; }

    protected DispositivoRed(int id, string nombre, int latenciaMs)
    {
        if (string.IsNullOrWhiteSpace(nombre)) throw new ArgumentException("Nombre inválido");
        if (latenciaMs < 0) throw new ArgumentException("latenciaMs inválida");

        Id = id;
        Nombre = nombre;
        latenciaMs = latenciaMs;
    }

#pragma warning disable CS0436 // El tipo entra en conflicto con un tipo importado
    public abstract ResultadoProcesamiento Procesar(PaqueteRed paquete);
#pragma warning restore CS0436 // El tipo entra en conflicto con un tipo importado
}
