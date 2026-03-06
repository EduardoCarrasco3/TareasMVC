using TareasMVC.Models;

namespace TareasMVC.Servicios
{
    public interface IAlmacenadorArchivos
    {
        Task Borrar(string ruta, string contenedor);
        Task<ArchivoResultado[]> Almacenar(string contenedor, 
            IEnumerable<IFormFile> archivos);
    }
}
