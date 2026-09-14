namespace Aplicacion.Entidades;
public abstract class DispositivoRed
{
    public int Id { get; set; }
    public string Nombre { get; set; }
    public int Latencia { get; protected set; }

    protected DispositivoRed(int id, string nombre, int latencia)
    {
        if (string.IsNullOrWhiteSpace(nombre)) throw new ArgumentException("Nombre inválido");
        if (latencia < 0) throw new ArgumentException("Latencia inválida");

        Id = id;
        Nombre = nombre;
        Latencia = latencia;
    }

#pragma warning disable CS0436 // El tipo entra en conflicto con un tipo importado
    public abstract ResultadoProcesamiento Procesar(PaqueteRed paquete);
#pragma warning restore CS0436 // El tipo entra en conflicto con un tipo importado
}
