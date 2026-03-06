using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TareasMVC.Entidades;
using TareasMVC.Servicios;

namespace TareasMVC.Controllers
{
    [Route("api/archivos")]
    [ApiController]
    public class ArchivosController : ControllerBase
    {
        private readonly ApplicationDbContext context;
        private readonly IAlmacenadorArchivos almacenadorArchivos;
        private readonly IServicioUsuarios servicioUsuarios;
        private readonly string contenedor = "archivos-adjuntos";

        public ArchivosController(ApplicationDbContext context,
            IAlmacenadorArchivos almacenadorArchivos,
            IServicioUsuarios servicioUsuarios)
        {
            this.context = context;
            this.almacenadorArchivos = almacenadorArchivos;
            this.servicioUsuarios = servicioUsuarios;
        }

        [HttpPost("{tareaId:int}")]
        public async Task<ActionResult<IEnumerable<ArchivoAdjunto>>> Post(int tareaId,
            [FromForm] IEnumerable<IFormFile> archivos)
        {
            var usuarioId = servicioUsuarios.ObtenerUsuarioId();

            var tarea = await context.Tareas.FirstOrDefaultAsync(t => t.Id == tareaId);

            if (tarea is null)
            {
                return NotFound();
            }

            if (tarea.UsuarioCreacionId != usuarioId)
            {
                return Forbid();
            }

            var existenArchivos = await context.ArchivoAdjuntos.AnyAsync(x => x.TareaId == tareaId);

            var ordenMayor = 0;

            if (existenArchivos)
            {
                ordenMayor = await context.ArchivoAdjuntos.Where(x => x.TareaId == tareaId)
                    .Select(x => x.Orden).MaxAsync();
            }

            var resultados = await almacenadorArchivos.Almacenar(contenedor, archivos);

            var archivosAdjuntos = resultados.Select((resultado, indice) => new ArchivoAdjunto
            {
                TareaId = tareaId,
                Url = resultado.URL,
                Titulo = resultado.Titulo,
                Orden = ++ordenMayor
            }).ToList();

            context.AddRange(archivosAdjuntos);

            await context.SaveChangesAsync();

            return archivosAdjuntos.ToList();
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Put(Guid id, [FromBody] string titulo)
        {
            var usuarioId = servicioUsuarios.ObtenerUsuarioId();

            var archivoAdjunto = await context.ArchivoAdjuntos
                .Include(a => a.Tarea)
                .FirstOrDefaultAsync(a => a.Id == id);

            if (archivoAdjunto is null)
            {
                return NotFound();
            }

            if (archivoAdjunto.Tarea.UsuarioCreacionId != usuarioId)
            {
                return Forbid();
            }

            archivoAdjunto.Titulo = titulo;
            await context.SaveChangesAsync();
            return Ok();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var usuarioId = servicioUsuarios.ObtenerUsuarioId();

            var archivoAdjunto = await context.ArchivoAdjuntos.Include(a => a.Tarea).FirstOrDefaultAsync();

            if (archivoAdjunto is null)
            {
                return NotFound();
            }

            if (archivoAdjunto.Tarea.UsuarioCreacionId != usuarioId)
            {
                return Forbid();
            }

            context.Remove(archivoAdjunto);
            await context.SaveChangesAsync();
            await almacenadorArchivos.Borrar(archivoAdjunto.Url, contenedor);
            return Ok();
        }

        [HttpPost("ordenar/{tareaId:int}")]
        public async Task<ActionResult> Ordenar(int tareaId, [FromBody] Guid[] ids)
        {
            var usuarioId = servicioUsuarios.ObtenerUsuarioId();
            var tarea = await context.Tareas.FirstOrDefaultAsync(t => t.Id == tareaId && t.UsuarioCreacionId == usuarioId);

            if (tarea is null)
            {
                return NotFound();
            }

            var archivosAdjuntos = await context.ArchivoAdjuntos.Where(x => x.TareaId == tareaId).ToListAsync();

            var archivosIds = archivosAdjuntos.Select(x => x.Id);

            var idsArchivosNoPertenecenALaTarea = ids.Except(archivosIds).ToList();

            if (idsArchivosNoPertenecenALaTarea.Any())
            {
                return BadRequest("No todos los archivos estan presentes");
            }

            var archivosDiccionario = archivosAdjuntos.ToDictionary(p => p.Id);

            for (int i = 0; i < ids.Length; i++)
            {
                var archivoId = ids[i];

                var archivo = archivosDiccionario[archivoId];

                archivo.Orden = i + 1;
            }

            await context.SaveChangesAsync();
            return Ok();

        }
    }
}
