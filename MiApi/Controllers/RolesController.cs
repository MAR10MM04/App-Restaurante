using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MiApi.Data;
using MiApi.DTOs;
using MiApi.Models;

namespace MiApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RolesController : ControllerBase
    {
        private readonly MyMDbContext _context;

        public RolesController(MyMDbContext context)
        {
            _context = context;
        }

        // GET: api/roles
        [HttpGet]
        public async Task<ActionResult<IEnumerable<RolRespuestaDto>>>
            ObtenerRoles()
        {
            var roles = await _context.Roles
                .AsNoTracking()
                .OrderBy(rol => rol.Nombre)
                .Select(rol => new RolRespuestaDto
                {
                    IdRol = rol.IdRol,
                    Nombre = rol.Nombre,
                    Descripcion = rol.Descripcion
                })
                .ToListAsync();

            return Ok(roles);
        }

        // GET: api/roles/1
        [HttpGet("{id:int}")]
        public async Task<ActionResult<RolRespuestaDto>>
            ObtenerRolPorId(int id)
        {
            var rol = await _context.Roles
                .AsNoTracking()
                .Where(rol => rol.IdRol == id)
                .Select(rol => new RolRespuestaDto
                {
                    IdRol = rol.IdRol,
                    Nombre = rol.Nombre,
                    Descripcion = rol.Descripcion
                })
                .FirstOrDefaultAsync();

            if (rol is null)
            {
                return NotFound(new
                {
                    mensaje = $"No existe el rol con ID {id}."
                });
            }

            return Ok(rol);
        }

        // POST: api/roles
        [HttpPost]
        public async Task<ActionResult<RolRespuestaDto>>
            CrearRol([FromBody] CrearRolDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var nombre = dto.Nombre.Trim();

            var rolExiste = await _context.Roles
                .AnyAsync(rol =>
                    rol.Nombre.ToLower() == nombre.ToLower());

            if (rolExiste)
            {
                return Conflict(new
                {
                    mensaje = $"Ya existe el rol '{nombre}'."
                });
            }

            var nuevoRol = new Rol
            {
                Nombre = nombre,
               
            };

            _context.Roles.Add(nuevoRol);
            await _context.SaveChangesAsync();

            var respuesta = new RolRespuestaDto
            {
                IdRol = nuevoRol.IdRol,
                Nombre = nuevoRol.Nombre,
                Descripcion = nuevoRol.Descripcion
            };

            return CreatedAtAction(
                nameof(ObtenerRolPorId),
                new { id = nuevoRol.IdRol },
                respuesta);
        }

        // PUT: api/roles/1
        [HttpPut("{id:int}")]
        public async Task<ActionResult<RolRespuestaDto>>
            ActualizarRol(
                int id,
                [FromBody] CrearRolDto dto)
        {
            var rol = await _context.Roles
                .FirstOrDefaultAsync(rol => rol.IdRol == id);

            if (rol is null)
            {
                return NotFound(new
                {
                    mensaje = $"No existe el rol con ID {id}."
                });
            }

            var nombre = dto.Nombre.Trim();

            var nombreEnUso = await _context.Roles
                .AnyAsync(otroRol =>
                    otroRol.IdRol != id &&
                    otroRol.Nombre.ToLower() == nombre.ToLower());

            if (nombreEnUso)
            {
                return Conflict(new
                {
                    mensaje = $"Ya existe otro rol llamado '{nombre}'."
                });
            }

            rol.Nombre = nombre;
            

            await _context.SaveChangesAsync();

            return Ok(new RolRespuestaDto
            {
                IdRol = rol.IdRol,
                Nombre = rol.Nombre,
                Descripcion = rol.Descripcion
            });
        }

        // DELETE: api/roles/1
        [HttpDelete("{id:int}")]
        public async Task<ActionResult> EliminarRol(int id)
        {
            var rol = await _context.Roles
                .FirstOrDefaultAsync(rol => rol.IdRol == id);

            if (rol is null)
            {
                return NotFound(new
                {
                    mensaje = $"No existe el rol con ID {id}."
                });
            }

            var estaAsignado = await _context.UsuariosRoles
                .AnyAsync(usuarioRol => usuarioRol.IdRol == id);

            if (estaAsignado)
            {
                return Conflict(new
                {
                    mensaje =
                        "No se puede eliminar el rol porque está asignado a usuarios."
                });
            }

            _context.Roles.Remove(rol);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                mensaje = "Rol eliminado correctamente."
            });
        }
    }
}