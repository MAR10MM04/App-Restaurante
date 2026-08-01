using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MiApi.Data;
using MiApi.DTOs;
using MiApi.Models;

namespace MiApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DireccionesController : ControllerBase
    {
        private readonly MyMDbContext _context;

        public DireccionesController(MyMDbContext context)
        {
            _context = context;
        }

        // GET: api/direcciones
        // GET: api/direcciones?idUsuario=1
        // GET: api/direcciones?colonia=Centro
        [HttpGet]
        public async Task<ActionResult<IEnumerable<DireccionRespuestaDto>>>
            ObtenerDirecciones(
                [FromQuery] int? idUsuario,
                [FromQuery] string? colonia)
        {
            var consulta = _context.Direcciones
                .AsNoTracking()
                .AsQueryable();

            if (idUsuario.HasValue)
            {
                consulta = consulta.Where(direccion =>
                    direccion.IdUsuario == idUsuario.Value);
            }

            if (!string.IsNullOrWhiteSpace(colonia))
            {
                var texto = colonia.Trim();

                consulta = consulta.Where(direccion =>
                    direccion.Colonia.Contains(texto));
            }

            var direcciones = await consulta
                .OrderBy(direccion => direccion.Colonia)
                .Select(direccion => new DireccionRespuestaDto
                {
                    IdDireccion = direccion.IdDireccion,
                    IdUsuario = direccion.IdUsuario,
                    Usuario = direccion.Usuario.Nombre,
                    Colonia = direccion.Colonia,
                    Latitud = direccion.Latitud,
                    Longitud = direccion.Longitud,
                    CantidadPedidos = direccion.Pedidos.Count()
                })
                .ToListAsync();

            return Ok(direcciones);
        }

        // GET: api/direcciones/5
        [HttpGet("{id:int}")]
        public async Task<ActionResult<DireccionRespuestaDto>>
            ObtenerDireccionPorId(int id)
        {
            var direccion = await ObtenerDireccionDto(id);

            if (direccion is null)
            {
                return NotFound(new
                {
                    mensaje = $"No existe la dirección con ID {id}."
                });
            }

            return Ok(direccion);
        }

        // GET: api/direcciones/usuario/3
        [HttpGet("usuario/{idUsuario:int}")]
        public async Task<ActionResult<IEnumerable<DireccionRespuestaDto>>>
            ObtenerDireccionesPorUsuario(int idUsuario)
        {
            var usuarioExiste = await _context.Usuarios
                .AnyAsync(usuario =>
                    usuario.IdUsuario == idUsuario);

            if (!usuarioExiste)
            {
                return NotFound(new
                {
                    mensaje =
                        $"No existe el usuario con ID {idUsuario}."
                });
            }

            var direcciones = await _context.Direcciones
                .AsNoTracking()
                .Where(direccion =>
                    direccion.IdUsuario == idUsuario)
                .OrderBy(direccion =>
                    direccion.Colonia)
                .Select(direccion => new DireccionRespuestaDto
                {
                    IdDireccion = direccion.IdDireccion,
                    IdUsuario = direccion.IdUsuario,
                    Usuario = direccion.Usuario.Nombre,
                    Colonia = direccion.Colonia,
                    Latitud = direccion.Latitud,
                    Longitud = direccion.Longitud,
                    CantidadPedidos = direccion.Pedidos.Count()
                })
                .ToListAsync();

            return Ok(direcciones);
        }

        // GET: api/direcciones/5/pedidos
        [HttpGet("{id:int}/pedidos")]
        public async Task<ActionResult> ObtenerDireccionConPedidos(
            int id)
        {
            var direccion = await _context.Direcciones
                .AsNoTracking()
                .Where(direccion =>
                    direccion.IdDireccion == id)
                .Select(direccion => new
                {
                    direccion.IdDireccion,
                    direccion.IdUsuario,
                    Usuario = direccion.Usuario.Nombre,
                    direccion.Colonia,
                    direccion.Latitud,
                    direccion.Longitud,

                    Pedidos = direccion.Pedidos
                        .OrderByDescending(pedido =>
                            pedido.FechaPedido)
                        .Select(pedido => new
                        {
                            pedido.IdPedido,
                            pedido.NumeroPedido,
                            pedido.FechaPedido,
                            pedido.Estado,
                            pedido.Total,
                            pedido.IdRestaurante,
                            Restaurante =
                                pedido.Restaurante.Nombre
                        })
                        .ToList()
                })
                .FirstOrDefaultAsync();

            if (direccion is null)
            {
                return NotFound(new
                {
                    mensaje = $"No existe la dirección con ID {id}."
                });
            }

            return Ok(direccion);
        }

        // POST: api/direcciones
        [HttpPost]
        public async Task<ActionResult<DireccionRespuestaDto>>
            CrearDireccion(CrearDireccionDto dto)
        {
            var usuarioExiste = await _context.Usuarios
                .AnyAsync(usuario =>
                    usuario.IdUsuario == dto.IdUsuario);

            if (!usuarioExiste)
            {
                return BadRequest(new
                {
                    mensaje =
                        $"No existe el usuario con ID {dto.IdUsuario}."
                });
            }

            var colonia = dto.Colonia.Trim();

            if (string.IsNullOrWhiteSpace(colonia))
            {
                return BadRequest(new
                {
                    mensaje = "La colonia es obligatoria."
                });
            }

            var direccionDuplicada = await _context.Direcciones
                .AnyAsync(direccion =>
                    direccion.IdUsuario == dto.IdUsuario &&
                    direccion.Colonia == colonia &&
                    direccion.Latitud == dto.Latitud &&
                    direccion.Longitud == dto.Longitud);

            if (direccionDuplicada)
            {
                return Conflict(new
                {
                    mensaje =
                        "El usuario ya tiene registrada esta dirección."
                });
            }

            var nuevaDireccion = new Direccion
            {
                IdUsuario = dto.IdUsuario,
                Colonia = colonia,
                Latitud = dto.Latitud,
                Longitud = dto.Longitud
            };

            _context.Direcciones.Add(nuevaDireccion);
            await _context.SaveChangesAsync();

            var direccionCreada = await ObtenerDireccionDto(
                nuevaDireccion.IdDireccion);

            return CreatedAtAction(
                nameof(ObtenerDireccionPorId),
                new { id = nuevaDireccion.IdDireccion },
                direccionCreada
            );
        }

        // PUT: api/direcciones/5
        [HttpPut("{id:int}")]
        public async Task<ActionResult<DireccionRespuestaDto>>
            ActualizarDireccion(
                int id,
                ActualizarDireccionDto dto)
        {
            var direccion = await _context.Direcciones
                .FirstOrDefaultAsync(direccion =>
                    direccion.IdDireccion == id);

            if (direccion is null)
            {
                return NotFound(new
                {
                    mensaje = $"No existe la dirección con ID {id}."
                });
            }

            var usuarioExiste = await _context.Usuarios
                .AnyAsync(usuario =>
                    usuario.IdUsuario == dto.IdUsuario);

            if (!usuarioExiste)
            {
                return BadRequest(new
                {
                    mensaje =
                        $"No existe el usuario con ID {dto.IdUsuario}."
                });
            }

            var colonia = dto.Colonia.Trim();

            if (string.IsNullOrWhiteSpace(colonia))
            {
                return BadRequest(new
                {
                    mensaje = "La colonia es obligatoria."
                });
            }

            var direccionDuplicada = await _context.Direcciones
                .AnyAsync(otraDireccion =>
                    otraDireccion.IdDireccion != id &&
                    otraDireccion.IdUsuario == dto.IdUsuario &&
                    otraDireccion.Colonia == colonia &&
                    otraDireccion.Latitud == dto.Latitud &&
                    otraDireccion.Longitud == dto.Longitud);

            if (direccionDuplicada)
            {
                return Conflict(new
                {
                    mensaje =
                        "El usuario ya tiene registrada otra dirección igual."
                });
            }

            direccion.IdUsuario = dto.IdUsuario;
            direccion.Colonia = colonia;
            direccion.Latitud = dto.Latitud;
            direccion.Longitud = dto.Longitud;

            await _context.SaveChangesAsync();

            return Ok(await ObtenerDireccionDto(id));
        }

        // PATCH: api/direcciones/5/coordenadas
        [HttpPatch("{id:int}/coordenadas")]
        public async Task<ActionResult<DireccionRespuestaDto>>
            ActualizarCoordenadas(
                int id,
                ActualizarCoordenadasDto dto)
        {
            var direccion = await _context.Direcciones
                .FirstOrDefaultAsync(direccion =>
                    direccion.IdDireccion == id);

            if (direccion is null)
            {
                return NotFound(new
                {
                    mensaje = $"No existe la dirección con ID {id}."
                });
            }

            direccion.Latitud = dto.Latitud;
            direccion.Longitud = dto.Longitud;

            await _context.SaveChangesAsync();

            return Ok(await ObtenerDireccionDto(id));
        }

        // DELETE: api/direcciones/5
        [HttpDelete("{id:int}")]
        public async Task<ActionResult> EliminarDireccion(int id)
        {
            var direccion = await _context.Direcciones
                .FirstOrDefaultAsync(direccion =>
                    direccion.IdDireccion == id);

            if (direccion is null)
            {
                return NotFound(new
                {
                    mensaje = $"No existe la dirección con ID {id}."
                });
            }

            var tienePedidos = await _context.Pedidos
                .AnyAsync(pedido =>
                    pedido.IdDireccionEntrega == id);

            if (tienePedidos)
            {
                return Conflict(new
                {
                    mensaje =
                        "No se puede eliminar la dirección porque está " +
                        "relacionada con uno o más pedidos."
                });
            }

            _context.Direcciones.Remove(direccion);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                mensaje = "Dirección eliminada correctamente."
            });
        }

        private async Task<DireccionRespuestaDto?>
            ObtenerDireccionDto(int idDireccion)
        {
            return await _context.Direcciones
                .AsNoTracking()
                .Where(direccion =>
                    direccion.IdDireccion == idDireccion)
                .Select(direccion => new DireccionRespuestaDto
                {
                    IdDireccion = direccion.IdDireccion,
                    IdUsuario = direccion.IdUsuario,
                    Usuario = direccion.Usuario.Nombre,
                    Colonia = direccion.Colonia,
                    Latitud = direccion.Latitud,
                    Longitud = direccion.Longitud,
                    CantidadPedidos = direccion.Pedidos.Count()
                })
                .FirstOrDefaultAsync();
        }
    }
}