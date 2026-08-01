using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MiApi.Data;
using MiApi.DTOs;
using MiApi.Models;

namespace MiApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RepartidoresController : ControllerBase
    {
        private readonly MyMDbContext _context;

        public RepartidoresController(MyMDbContext context)
        {
            _context = context;
        }

        // GET: api/repartidores
        // GET: api/repartidores?estado=Disponible
        // GET: api/repartidores?buscar=Juan
        [HttpGet]
        public async Task<ActionResult<IEnumerable<RepartidorRespuestaDto>>>
            ObtenerRepartidores(
                [FromQuery] string? estado,
                [FromQuery] string? buscar)
        {
            var consulta = _context.Repartidores
                .AsNoTracking()
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(estado))
            {
                var estadoBuscado = estado.Trim();

                consulta = consulta.Where(repartidor =>
                    repartidor.Estado == estadoBuscado);
            }

            if (!string.IsNullOrWhiteSpace(buscar))
            {
                var texto = buscar.Trim();

                consulta = consulta.Where(repartidor =>
                    repartidor.Usuario.Nombre.Contains(texto) ||
                    (
                        repartidor.Usuario.Telefono != null &&
                        repartidor.Usuario.Telefono.Contains(texto)
                    ));
            }

            var repartidores = await consulta
                .OrderBy(repartidor => repartidor.Usuario.Nombre)
                .Select(repartidor => new RepartidorRespuestaDto
                {
                    IdRepartidor = repartidor.IdRepartidor,
                    IdUsuario = repartidor.IdUsuario,
                    Nombre = repartidor.Usuario.Nombre,
                    Telefono = repartidor.Usuario.Telefono,
                    Estado = repartidor.Estado,

                    CantidadPedidos = repartidor.Pedidos.Count(),

                    PedidosPendientes = repartidor.Pedidos.Count(
                        pedido => pedido.Estado != "Entregado" &&
                                  pedido.Estado != "Cancelado"),

                    PedidosEntregados = repartidor.Pedidos.Count(
                        pedido => pedido.Estado == "Entregado")
                })
                .ToListAsync();

            return Ok(repartidores);
        }

        // GET: api/repartidores/5
        [HttpGet("{id:int}")]
        public async Task<ActionResult<RepartidorRespuestaDto>>
            ObtenerRepartidorPorId(int id)
        {
            var repartidor = await ObtenerRepartidorDto(id);

            if (repartidor is null)
            {
                return NotFound(new
                {
                    mensaje =
                        $"No existe el repartidor con ID {id}."
                });
            }

            return Ok(repartidor);
        }

        // GET: api/repartidores/5/detalle
        [HttpGet("{id:int}/detalle")]
        public async Task<ActionResult>
            ObtenerRepartidorConPedidos(int id)
        {
            var repartidor = await _context.Repartidores
                .AsNoTracking()
                .Where(repartidor =>
                    repartidor.IdRepartidor == id)
                .Select(repartidor => new
                {
                    repartidor.IdRepartidor,
                    repartidor.IdUsuario,
                    repartidor.Estado,

                    Usuario = new
                    {
                        repartidor.Usuario.IdUsuario,
                        repartidor.Usuario.Nombre,
                        repartidor.Usuario.Email,
                        repartidor.Usuario.Telefono
                    },

                    Pedidos = repartidor.Pedidos
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
                                pedido.Restaurante.Nombre,

                            pedido.IdDireccionEntrega
                        })
                        .ToList()
                })
                .FirstOrDefaultAsync();

            if (repartidor is null)
            {
                return NotFound(new
                {
                    mensaje =
                        $"No existe el repartidor con ID {id}."
                });
            }

            return Ok(repartidor);
        }

        // GET: api/repartidores/disponibles
        [HttpGet("disponibles")]
        public async Task<ActionResult<IEnumerable<RepartidorRespuestaDto>>>
            ObtenerRepartidoresDisponibles()
        {
            var repartidores = await _context.Repartidores
                .AsNoTracking()
                .Where(repartidor =>
                    repartidor.Estado == "Disponible")
                .OrderBy(repartidor =>
                    repartidor.Usuario.Nombre)
                .Select(repartidor => new RepartidorRespuestaDto
                {
                    IdRepartidor = repartidor.IdRepartidor,
                    IdUsuario = repartidor.IdUsuario,
                    Nombre = repartidor.Usuario.Nombre,
                    Telefono = repartidor.Usuario.Telefono,
                    Estado = repartidor.Estado,

                    CantidadPedidos =
                        repartidor.Pedidos.Count(),

                    PedidosPendientes =
                        repartidor.Pedidos.Count(pedido =>
                            pedido.Estado != "Entregado" &&
                            pedido.Estado != "Cancelado"),

                    PedidosEntregados =
                        repartidor.Pedidos.Count(pedido =>
                            pedido.Estado == "Entregado")
                })
                .ToListAsync();

            return Ok(repartidores);
        }

        // GET: api/repartidores/usuario/3
        [HttpGet("usuario/{idUsuario:int}")]
        public async Task<ActionResult<RepartidorRespuestaDto>>
            ObtenerRepartidorPorUsuario(int idUsuario)
        {
            var repartidor = await _context.Repartidores
                .AsNoTracking()
                .Where(repartidor =>
                    repartidor.IdUsuario == idUsuario)
                .Select(repartidor => new RepartidorRespuestaDto
                {
                    IdRepartidor = repartidor.IdRepartidor,
                    IdUsuario = repartidor.IdUsuario,
                    Nombre = repartidor.Usuario.Nombre,
                    Telefono = repartidor.Usuario.Telefono,
                    Estado = repartidor.Estado,

                    CantidadPedidos =
                        repartidor.Pedidos.Count(),

                    PedidosPendientes =
                        repartidor.Pedidos.Count(pedido =>
                            pedido.Estado != "Entregado" &&
                            pedido.Estado != "Cancelado"),

                    PedidosEntregados =
                        repartidor.Pedidos.Count(pedido =>
                            pedido.Estado == "Entregado")
                })
                .FirstOrDefaultAsync();

            if (repartidor is null)
            {
                return NotFound(new
                {
                    mensaje =
                        $"El usuario con ID {idUsuario} no está " +
                        "registrado como repartidor."
                });
            }

            return Ok(repartidor);
        }

        // POST: api/repartidores
        [HttpPost]
        public async Task<ActionResult<RepartidorRespuestaDto>>
            CrearRepartidor(CrearRepartidorDto dto)
        {
            var usuario = await _context.Usuarios
                .FirstOrDefaultAsync(usuario =>
                    usuario.IdUsuario == dto.IdUsuario);

            if (usuario is null)
            {
                return BadRequest(new
                {
                    mensaje =
                        $"No existe el usuario con ID {dto.IdUsuario}."
                });
            }

            var yaEsRepartidor = await _context.Repartidores
                .AnyAsync(repartidor =>
                    repartidor.IdUsuario == dto.IdUsuario);

            if (yaEsRepartidor)
            {
                return Conflict(new
                {
                    mensaje =
                        "El usuario ya está registrado como repartidor."
                });
            }

            /*
             * Opcional:
             * Si tu modelo Usuarios tiene una propiedad TipoUsuario,
             * puedes validar o modificar su tipo aquí.
             *
             * usuario.TipoUsuario = "Repartidor";
             */

            var nuevoRepartidor = new Repartidor
            {
                IdUsuario = dto.IdUsuario,
                Estado = "Disponible"
            };

            _context.Repartidores.Add(nuevoRepartidor);
            await _context.SaveChangesAsync();

            var repartidorCreado = await ObtenerRepartidorDto(
                nuevoRepartidor.IdRepartidor);

            return CreatedAtAction(
                nameof(ObtenerRepartidorPorId),
                new { id = nuevoRepartidor.IdRepartidor },
                repartidorCreado
            );
        }

        // PUT: api/repartidores/5
        [HttpPut("{id:int}")]
        public async Task<ActionResult<RepartidorRespuestaDto>>
            ActualizarRepartidor(
                int id,
                ActualizarRepartidorDto dto)
        {
            var repartidor = await _context.Repartidores
                .FirstOrDefaultAsync(repartidor =>
                    repartidor.IdRepartidor == id);

            if (repartidor is null)
            {
                return NotFound(new
                {
                    mensaje =
                        $"No existe el repartidor con ID {id}."
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

            var usuarioAsignado = await _context.Repartidores
                .AnyAsync(otroRepartidor =>
                    otroRepartidor.IdRepartidor != id &&
                    otroRepartidor.IdUsuario == dto.IdUsuario);

            if (usuarioAsignado)
            {
                return Conflict(new
                {
                    mensaje =
                        "El usuario ya está asignado a otro repartidor."
                });
            }

            var estadoValido = ObtenerEstadoValido(dto.Estado);

            if (estadoValido is null)
            {
                return BadRequest(new
                {
                    mensaje = "El estado indicado no es válido.",
                    estadosPermitidos = EstadosPermitidos
                });
            }

            repartidor.IdUsuario = dto.IdUsuario;
            repartidor.Estado = estadoValido;

            await _context.SaveChangesAsync();

            return Ok(await ObtenerRepartidorDto(id));
        }

        // PATCH: api/repartidores/5/estado
        [HttpPatch("{id:int}/estado")]
        public async Task<ActionResult>
            CambiarEstado(
                int id,
                CambiarEstadoRepartidorDto dto)
        {
            var repartidor = await _context.Repartidores
                .FirstOrDefaultAsync(repartidor =>
                    repartidor.IdRepartidor == id);

            if (repartidor is null)
            {
                return NotFound(new
                {
                    mensaje =
                        $"No existe el repartidor con ID {id}."
                });
            }

            var nuevoEstado = ObtenerEstadoValido(dto.Estado);

            if (nuevoEstado is null)
            {
                return BadRequest(new
                {
                    mensaje = "El estado indicado no es válido.",
                    estadosPermitidos = EstadosPermitidos
                });
            }

            if (nuevoEstado == "Disponible")
            {
                var tienePedidosActivos =
                    await _context.Pedidos.AnyAsync(pedido =>
                        pedido.IdRepartidor == id &&
                        pedido.Estado != "Entregado" &&
                        pedido.Estado != "Cancelado");

                if (tienePedidosActivos)
                {
                    return Conflict(new
                    {
                        mensaje =
                            "El repartidor tiene pedidos activos y " +
                            "no puede marcarse como disponible."
                    });
                }
            }

            repartidor.Estado = nuevoEstado;

            await _context.SaveChangesAsync();

            return Ok(new
            {
                mensaje =
                    "Estado del repartidor actualizado correctamente.",
                repartidor.IdRepartidor,
                repartidor.Estado
            });
        }

        // PATCH: api/repartidores/5/disponible
        [HttpPatch("{id:int}/disponible")]
        public async Task<ActionResult>
            MarcarDisponible(int id)
        {
            var repartidor = await _context.Repartidores
                .FirstOrDefaultAsync(repartidor =>
                    repartidor.IdRepartidor == id);

            if (repartidor is null)
            {
                return NotFound(new
                {
                    mensaje =
                        $"No existe el repartidor con ID {id}."
                });
            }

            var tienePedidosActivos =
                await _context.Pedidos.AnyAsync(pedido =>
                    pedido.IdRepartidor == id &&
                    pedido.Estado != "Entregado" &&
                    pedido.Estado != "Cancelado");

            if (tienePedidosActivos)
            {
                return Conflict(new
                {
                    mensaje =
                        "El repartidor todavía tiene pedidos activos."
                });
            }

            repartidor.Estado = "Disponible";

            await _context.SaveChangesAsync();

            return Ok(new
            {
                mensaje =
                    "El repartidor ahora está disponible.",
                repartidor.IdRepartidor,
                repartidor.Estado
            });
        }

        // PATCH: api/repartidores/5/ocupado
        [HttpPatch("{id:int}/ocupado")]
        public async Task<ActionResult>
            MarcarOcupado(int id)
        {
            var repartidor = await _context.Repartidores
                .FirstOrDefaultAsync(repartidor =>
                    repartidor.IdRepartidor == id);

            if (repartidor is null)
            {
                return NotFound(new
                {
                    mensaje =
                        $"No existe el repartidor con ID {id}."
                });
            }

            repartidor.Estado = "Ocupado";

            await _context.SaveChangesAsync();

            return Ok(new
            {
                mensaje =
                    "El repartidor fue marcado como ocupado.",
                repartidor.IdRepartidor,
                repartidor.Estado
            });
        }

        // DELETE: api/repartidores/5
        [HttpDelete("{id:int}")]
        public async Task<ActionResult>
            EliminarRepartidor(int id)
        {
            var repartidor = await _context.Repartidores
                .FirstOrDefaultAsync(repartidor =>
                    repartidor.IdRepartidor == id);

            if (repartidor is null)
            {
                return NotFound(new
                {
                    mensaje =
                        $"No existe el repartidor con ID {id}."
                });
            }

            var tienePedidosActivos =
                await _context.Pedidos.AnyAsync(pedido =>
                    pedido.IdRepartidor == id &&
                    pedido.Estado != "Entregado" &&
                    pedido.Estado != "Cancelado");

            if (tienePedidosActivos)
            {
                return Conflict(new
                {
                    mensaje =
                        "No se puede eliminar el repartidor porque " +
                        "tiene pedidos activos."
                });
            }

            var tienePedidos = await _context.Pedidos
                .AnyAsync(pedido =>
                    pedido.IdRepartidor == id);

            if (tienePedidos)
            {
                /*
                 * Como IdRepartidor en Pedido es nullable,
                 * se elimina la asignación de los pedidos históricos.
                 */
                var pedidos = await _context.Pedidos
                    .Where(pedido =>
                        pedido.IdRepartidor == id)
                    .ToListAsync();

                foreach (var pedido in pedidos)
                {
                    pedido.IdRepartidor = null;
                }
            }

            _context.Repartidores.Remove(repartidor);

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                return Conflict(new
                {
                    mensaje =
                        "No se pudo eliminar el repartidor porque " +
                        "tiene registros relacionados."
                });
            }

            return Ok(new
            {
                mensaje =
                    "Repartidor eliminado correctamente."
            });
        }

        private async Task<RepartidorRespuestaDto?>
            ObtenerRepartidorDto(int idRepartidor)
        {
            return await _context.Repartidores
                .AsNoTracking()
                .Where(repartidor =>
                    repartidor.IdRepartidor == idRepartidor)
                .Select(repartidor =>
                    new RepartidorRespuestaDto
                    {
                        IdRepartidor =
                            repartidor.IdRepartidor,

                        IdUsuario =
                            repartidor.IdUsuario,

                        Nombre =
                            repartidor.Usuario.Nombre,

                        Telefono =
                            repartidor.Usuario.Telefono,

                        Estado =
                            repartidor.Estado,

                        CantidadPedidos =
                            repartidor.Pedidos.Count(),

                        PedidosPendientes =
                            repartidor.Pedidos.Count(pedido =>
                                pedido.Estado != "Entregado" &&
                                pedido.Estado != "Cancelado"),

                        PedidosEntregados =
                            repartidor.Pedidos.Count(pedido =>
                                pedido.Estado == "Entregado")
                    })
                .FirstOrDefaultAsync();
        }

        private static readonly string[] EstadosPermitidos =
        {
            "Disponible",
            "Ocupado",
            "Inactivo"
        };

        private static string? ObtenerEstadoValido(string estado)
        {
            if (string.IsNullOrWhiteSpace(estado))
            {
                return null;
            }

            return EstadosPermitidos.FirstOrDefault(
                estadoPermitido =>
                    estadoPermitido.Equals(
                        estado.Trim(),
                        StringComparison.OrdinalIgnoreCase));
        }
    }
}