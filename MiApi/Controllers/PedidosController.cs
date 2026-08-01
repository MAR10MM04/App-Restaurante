using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MiApi.Data;
using MiApi.DTOs;
using MiApi.Models;

namespace MiApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PedidosController : ControllerBase
    {
        private readonly MyMDbContext _context;

        public PedidosController(MyMDbContext context)
        {
            _context = context;
        }

        // GET: api/pedidos
        // GET: api/pedidos?idUsuario=1
        // GET: api/pedidos?idRestaurante=2
        // GET: api/pedidos?idRepartidor=3
        // GET: api/pedidos?estado=Pendiente
        [HttpGet]
        public async Task<ActionResult<IEnumerable<PedidoRespuestaDto>>>
            ObtenerPedidos(
                [FromQuery] int? idUsuario,
                [FromQuery] int? idRestaurante,
                [FromQuery] int? idRepartidor,
                [FromQuery] string? estado)
        {
            var consulta = _context.Pedidos
                .AsNoTracking()
                .AsQueryable();

            if (idUsuario.HasValue)
            {
                consulta = consulta.Where(pedido =>
                    pedido.IdUsuario == idUsuario.Value);
            }

            if (idRestaurante.HasValue)
            {
                consulta = consulta.Where(pedido =>
                    pedido.IdRestaurante == idRestaurante.Value);
            }

            if (idRepartidor.HasValue)
            {
                consulta = consulta.Where(pedido =>
                    pedido.IdRepartidor == idRepartidor.Value);
            }

            if (!string.IsNullOrWhiteSpace(estado))
            {
                var estadoBuscado = estado.Trim();

                consulta = consulta.Where(pedido =>
                    pedido.Estado == estadoBuscado);
            }

            var pedidos = await consulta
                .OrderByDescending(pedido => pedido.FechaPedido)
                .Select(pedido => new PedidoRespuestaDto
                {
                    IdPedido = pedido.IdPedido,
                    NumeroPedido = pedido.NumeroPedido,
                    FechaPedido = pedido.FechaPedido,
                    Estado = pedido.Estado,
                    Total = pedido.Total,
                    TipoPago = pedido.TipoPago,

                    IdUsuario = pedido.IdUsuario,
                    Usuario = pedido.Usuario.Nombre,

                    IdRestaurante = pedido.IdRestaurante,
                    Restaurante = pedido.Restaurante.Nombre,

                    IdDireccionEntrega =
                        pedido.IdDireccionEntrega,

                    IdRepartidor = pedido.IdRepartidor,

                    Repartidor = pedido.Repartidor == null
                        ? null
                        : pedido.Repartidor.Usuario.Nombre,

                    CalificacionRestaurante =
                        pedido.CalificacionRestaurante,

                    CalificacionRepartidor =
                        pedido.CalificacionRepartidor,

                    CantidadDetalles = pedido.Detalles.Count(),

                    TienePago = pedido.Pago != null
                })
                .ToListAsync();

            return Ok(pedidos);
        }

        // GET: api/pedidos/5
        [HttpGet("{id:int}")]
        public async Task<ActionResult<PedidoRespuestaDto>>
            ObtenerPedidoPorId(int id)
        {
            var pedido = await ObtenerPedidoDto(id);

            if (pedido is null)
            {
                return NotFound(new
                {
                    mensaje = $"No existe el pedido con ID {id}."
                });
            }

            return Ok(pedido);
        }

        // GET: api/pedidos/5/detalle
        [HttpGet("{id:int}/detalle")]
        public async Task<ActionResult>
            ObtenerPedidoConDetalles(int id)
        {
            var pedido = await _context.Pedidos
                .AsNoTracking()
                .Where(pedido => pedido.IdPedido == id)
                .Select(pedido => new
                {
                    pedido.IdPedido,
                    pedido.NumeroPedido,
                    pedido.FechaPedido,
                    pedido.Estado,
                    pedido.Total,
                    pedido.TipoPago,
                    pedido.CalificacionRestaurante,
                    pedido.CalificacionRepartidor,

                    Usuario = new
                    {
                        pedido.Usuario.IdUsuario,
                        pedido.Usuario.Nombre,
                        pedido.Usuario.Email,
                        pedido.Usuario.Telefono
                    },

                    Restaurante = new
                    {
                        pedido.Restaurante.IdRestaurante,
                        pedido.Restaurante.Nombre,
                        pedido.Restaurante.Direccion,
                        pedido.Restaurante.Telefono
                    },

                    DireccionEntrega = new
                    {
                        pedido.DireccionEntrega.IdDireccion,
                        pedido.DireccionEntrega.Latitud,
                        pedido.DireccionEntrega.Longitud
                    },

                    Repartidor = pedido.Repartidor == null
                        ? null
                        : new
                        {
                            pedido.Repartidor.IdRepartidor,
                            pedido.Repartidor.Estado,
                            IdUsuario =
                                pedido.Repartidor.IdUsuario,
                            Nombre =
                                pedido.Repartidor.Usuario.Nombre
                        },

                    Detalles = pedido.Detalles.Select(detalle =>
                        new
                        {
                            detalle.IdDetallePedido,
                            detalle.IdProducto,
                            Producto = detalle.Producto.Nombre,
                            detalle.Cantidad,
                            detalle.PrecioUnitario,
                            detalle.Subtotal
                        }),

                    Pago = pedido.Pago == null
                        ? null
                        : new
                        {
                            pedido.Pago.IdPago,
                            pedido.Pago.Monto,
                            pedido.Pago.MetodoPago,
                            pedido.Pago.FechaPago
                        }
                })
                .FirstOrDefaultAsync();

            if (pedido is null)
            {
                return NotFound(new
                {
                    mensaje = $"No existe el pedido con ID {id}."
                });
            }

            return Ok(pedido);
        }

        // GET: api/pedidos/usuario/1
        [HttpGet("usuario/{idUsuario:int}")]
        public async Task<ActionResult<IEnumerable<PedidoRespuestaDto>>>
            ObtenerPedidosPorUsuario(int idUsuario)
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

            var pedidos = await ObtenerListaPedidos(
                pedido => pedido.IdUsuario == idUsuario);

            return Ok(pedidos);
        }

        // GET: api/pedidos/restaurante/1
        [HttpGet("restaurante/{idRestaurante:int}")]
        public async Task<ActionResult<IEnumerable<PedidoRespuestaDto>>>
            ObtenerPedidosPorRestaurante(int idRestaurante)
        {
            var restauranteExiste = await _context.Restaurantes
                .AnyAsync(restaurante =>
                    restaurante.IdRestaurante == idRestaurante);

            if (!restauranteExiste)
            {
                return NotFound(new
                {
                    mensaje =
                        $"No existe el restaurante con ID {idRestaurante}."
                });
            }

            var pedidos = await ObtenerListaPedidos(
                pedido =>
                    pedido.IdRestaurante == idRestaurante);

            return Ok(pedidos);
        }

        // GET: api/pedidos/repartidor/1
        [HttpGet("repartidor/{idRepartidor:int}")]
        public async Task<ActionResult<IEnumerable<PedidoRespuestaDto>>>
            ObtenerPedidosPorRepartidor(int idRepartidor)
        {
            var repartidorExiste = await _context.Repartidores
                .AnyAsync(repartidor =>
                    repartidor.IdRepartidor == idRepartidor);

            if (!repartidorExiste)
            {
                return NotFound(new
                {
                    mensaje =
                        $"No existe el repartidor con ID {idRepartidor}."
                });
            }

            var pedidos = await ObtenerListaPedidos(
                pedido =>
                    pedido.IdRepartidor == idRepartidor);

            return Ok(pedidos);
        }

        // POST: api/pedidos
        [HttpPost]
        public async Task<ActionResult<PedidoRespuestaDto>>
            CrearPedido(CrearPedidoDto dto)
        {
            var errorRelacion = await ValidarRelaciones(
                dto.IdUsuario,
                dto.IdRestaurante,
                dto.IdDireccionEntrega,
                dto.IdRepartidor);

            if (errorRelacion is not null)
            {
                return BadRequest(new
                {
                    mensaje = errorRelacion
                });
            }

            var numeroPedido = await GenerarNumeroPedido();

            var nuevoPedido = new Pedido
            {
                IdUsuario = dto.IdUsuario,
                IdRestaurante = dto.IdRestaurante,
                IdDireccionEntrega =
                    dto.IdDireccionEntrega,
                IdRepartidor = dto.IdRepartidor,
                NumeroPedido = numeroPedido,
                FechaPedido = DateTime.UtcNow,
                Estado = "Pendiente",
                Total = dto.Total,
                TipoPago = dto.TipoPago.Trim()
            };

            _context.Pedidos.Add(nuevoPedido);
            await _context.SaveChangesAsync();

            var pedidoCreado = await ObtenerPedidoDto(
                nuevoPedido.IdPedido);

            return CreatedAtAction(
                nameof(ObtenerPedidoPorId),
                new { id = nuevoPedido.IdPedido },
                pedidoCreado
            );
        }

        // PUT: api/pedidos/5
        [HttpPut("{id:int}")]
        public async Task<ActionResult<PedidoRespuestaDto>>
            ActualizarPedido(
                int id,
                ActualizarPedidoDto dto)
        {
            var pedido = await _context.Pedidos
                .FirstOrDefaultAsync(pedido =>
                    pedido.IdPedido == id);

            if (pedido is null)
            {
                return NotFound(new
                {
                    mensaje = $"No existe el pedido con ID {id}."
                });
            }

            var errorRelacion = await ValidarRelaciones(
                dto.IdUsuario,
                dto.IdRestaurante,
                dto.IdDireccionEntrega,
                dto.IdRepartidor);

            if (errorRelacion is not null)
            {
                return BadRequest(new
                {
                    mensaje = errorRelacion
                });
            }

            pedido.IdUsuario = dto.IdUsuario;
            pedido.IdRestaurante = dto.IdRestaurante;
            pedido.IdDireccionEntrega =
                dto.IdDireccionEntrega;
            pedido.IdRepartidor = dto.IdRepartidor;
            pedido.Estado = dto.Estado.Trim();
            pedido.Total = dto.Total;
            pedido.TipoPago = dto.TipoPago.Trim();

            await _context.SaveChangesAsync();

            var pedidoActualizado = await ObtenerPedidoDto(id);

            return Ok(pedidoActualizado);
        }

        // PATCH: api/pedidos/5/estado
        [HttpPatch("{id:int}/estado")]
        public async Task<ActionResult>
            CambiarEstado(
                int id,
                CambiarEstadoPedidoDto dto)
        {
            var pedido = await _context.Pedidos
                .FirstOrDefaultAsync(pedido =>
                    pedido.IdPedido == id);

            if (pedido is null)
            {
                return NotFound(new
                {
                    mensaje = $"No existe el pedido con ID {id}."
                });
            }

            var estadosPermitidos = new[]
            {
                "Pendiente",
                "Confirmado",
                "Preparando",
                "Listo",
                "En camino",
                "Entregado",
                "Cancelado"
            };

            var nuevoEstado = estadosPermitidos
                .FirstOrDefault(estado =>
                    estado.Equals(
                        dto.Estado.Trim(),
                        StringComparison.OrdinalIgnoreCase));

            if (nuevoEstado is null)
            {
                return BadRequest(new
                {
                    mensaje = "El estado indicado no es válido.",
                    estadosPermitidos
                });
            }

            pedido.Estado = nuevoEstado;

            await _context.SaveChangesAsync();

            return Ok(new
            {
                mensaje = "Estado actualizado correctamente.",
                pedido.IdPedido,
                pedido.NumeroPedido,
                pedido.Estado
            });
        }

        // PATCH: api/pedidos/5/repartidor
        [HttpPatch("{id:int}/repartidor")]
        public async Task<ActionResult>
            AsignarRepartidor(
                int id,
                AsignarRepartidorDto dto)
        {
            var pedido = await _context.Pedidos
                .FirstOrDefaultAsync(pedido =>
                    pedido.IdPedido == id);

            if (pedido is null)
            {
                return NotFound(new
                {
                    mensaje = $"No existe el pedido con ID {id}."
                });
            }

            var repartidor = await _context.Repartidores
                .AsNoTracking()
                .FirstOrDefaultAsync(repartidor =>
                    repartidor.IdRepartidor ==
                    dto.IdRepartidor);

            if (repartidor is null)
            {
                return BadRequest(new
                {
                    mensaje =
                        $"No existe el repartidor con ID " +
                        $"{dto.IdRepartidor}."
                });
            }

            if (!repartidor.Estado.Equals(
                    "Activo",
                    StringComparison.OrdinalIgnoreCase))
            {
                return BadRequest(new
                {
                    mensaje =
                        "El repartidor seleccionado no está activo."
                });
            }

            pedido.IdRepartidor = dto.IdRepartidor;

            await _context.SaveChangesAsync();

            return Ok(new
            {
                mensaje = "Repartidor asignado correctamente.",
                pedido.IdPedido,
                pedido.NumeroPedido,
                pedido.IdRepartidor
            });
        }

        // DELETE: api/pedidos/5/repartidor
        [HttpDelete("{id:int}/repartidor")]
        public async Task<ActionResult>
            QuitarRepartidor(int id)
        {
            var pedido = await _context.Pedidos
                .FirstOrDefaultAsync(pedido =>
                    pedido.IdPedido == id);

            if (pedido is null)
            {
                return NotFound(new
                {
                    mensaje = $"No existe el pedido con ID {id}."
                });
            }

            pedido.IdRepartidor = null;

            await _context.SaveChangesAsync();

            return Ok(new
            {
                mensaje =
                    "El repartidor fue removido del pedido.",
                pedido.IdPedido,
                pedido.NumeroPedido
            });
        }

        // PATCH: api/pedidos/5/calificacion
        [HttpPatch("{id:int}/calificacion")]
        public async Task<ActionResult>
            CalificarPedido(
                int id,
                CalificarPedidoDto dto)
        {
            var pedido = await _context.Pedidos
                .FirstOrDefaultAsync(pedido =>
                    pedido.IdPedido == id);

            if (pedido is null)
            {
                return NotFound(new
                {
                    mensaje = $"No existe el pedido con ID {id}."
                });
            }

            if (!pedido.Estado.Equals(
                    "Entregado",
                    StringComparison.OrdinalIgnoreCase))
            {
                return BadRequest(new
                {
                    mensaje =
                        "Solamente se pueden calificar pedidos entregados."
                });
            }

            if (dto.CalificacionRestaurante is null &&
                dto.CalificacionRepartidor is null)
            {
                return BadRequest(new
                {
                    mensaje =
                        "Debes proporcionar por lo menos una calificación."
                });
            }

            if (dto.CalificacionRestaurante.HasValue)
            {
                pedido.CalificacionRestaurante =
                    dto.CalificacionRestaurante.Value;
            }

            if (dto.CalificacionRepartidor.HasValue)
            {
                if (pedido.IdRepartidor is null)
                {
                    return BadRequest(new
                    {
                        mensaje =
                            "El pedido no tiene un repartidor asignado."
                    });
                }

                pedido.CalificacionRepartidor =
                    dto.CalificacionRepartidor.Value;
            }

            await _context.SaveChangesAsync();

            return Ok(new
            {
                mensaje = "Calificación guardada correctamente.",
                pedido.IdPedido,
                pedido.CalificacionRestaurante,
                pedido.CalificacionRepartidor
            });
        }

        // DELETE: api/pedidos/5
        [HttpDelete("{id:int}")]
        public async Task<ActionResult>
            EliminarPedido(int id)
        {
            var pedido = await _context.Pedidos
                .FirstOrDefaultAsync(pedido =>
                    pedido.IdPedido == id);

            if (pedido is null)
            {
                return NotFound(new
                {
                    mensaje = $"No existe el pedido con ID {id}."
                });
            }

            if (pedido.Estado.Equals(
                    "Entregado",
                    StringComparison.OrdinalIgnoreCase))
            {
                return Conflict(new
                {
                    mensaje =
                        "No se recomienda eliminar un pedido entregado."
                });
            }

            _context.Pedidos.Remove(pedido);

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                return Conflict(new
                {
                    mensaje =
                        "No fue posible eliminar el pedido porque " +
                        "tiene registros relacionados."
                });
            }

            return Ok(new
            {
                mensaje = "Pedido eliminado correctamente."
            });
        }

        private async Task<string?> ValidarRelaciones(
            int idUsuario,
            int idRestaurante,
            int idDireccion,
            int? idRepartidor)
        {
            var usuarioExiste = await _context.Usuarios
                .AnyAsync(usuario =>
                    usuario.IdUsuario == idUsuario);

            if (!usuarioExiste)
            {
                return $"No existe el usuario con ID {idUsuario}.";
            }

            var restauranteExiste = await _context.Restaurantes
                .AnyAsync(restaurante =>
                    restaurante.IdRestaurante == idRestaurante);

            if (!restauranteExiste)
            {
                return
                    $"No existe el restaurante con ID {idRestaurante}.";
            }

            var direccionExiste = await _context.Direcciones
                .AnyAsync(direccion =>
                    direccion.IdDireccion == idDireccion &&
                    direccion.IdUsuario == idUsuario);

            if (!direccionExiste)
            {
                return
                    "La dirección no existe o no pertenece al usuario.";
            }

            if (idRepartidor.HasValue)
            {
                var repartidorExiste =
                    await _context.Repartidores.AnyAsync(
                        repartidor =>
                            repartidor.IdRepartidor ==
                            idRepartidor.Value);

                if (!repartidorExiste)
                {
                    return
                        $"No existe el repartidor con ID " +
                        $"{idRepartidor.Value}.";
                }
            }

            return null;
        }

        private async Task<string> GenerarNumeroPedido()
        {
            string numeroPedido;
            bool existe;

            do
            {
                numeroPedido =
                    $"PED-{DateTime.UtcNow:yyyyMMdd}-" +
                    $"{Guid.NewGuid().ToString("N")[..8].ToUpper()}";

                existe = await _context.Pedidos
                    .AnyAsync(pedido =>
                        pedido.NumeroPedido == numeroPedido);
            }
            while (existe);

            return numeroPedido;
        }

        private async Task<PedidoRespuestaDto?>
            ObtenerPedidoDto(int idPedido)
        {
            return await _context.Pedidos
                .AsNoTracking()
                .Where(pedido =>
                    pedido.IdPedido == idPedido)
                .Select(pedido => new PedidoRespuestaDto
                {
                    IdPedido = pedido.IdPedido,
                    NumeroPedido = pedido.NumeroPedido,
                    FechaPedido = pedido.FechaPedido,
                    Estado = pedido.Estado,
                    Total = pedido.Total,
                    TipoPago = pedido.TipoPago,

                    IdUsuario = pedido.IdUsuario,
                    Usuario = pedido.Usuario.Nombre,

                    IdRestaurante = pedido.IdRestaurante,
                    Restaurante = pedido.Restaurante.Nombre,

                    IdDireccionEntrega =
                        pedido.IdDireccionEntrega,

                    IdRepartidor = pedido.IdRepartidor,

                    Repartidor = pedido.Repartidor == null
                        ? null
                        : pedido.Repartidor.Usuario.Nombre,

                    CalificacionRestaurante =
                        pedido.CalificacionRestaurante,

                    CalificacionRepartidor =
                        pedido.CalificacionRepartidor,

                    CantidadDetalles =
                        pedido.Detalles.Count(),

                    TienePago = pedido.Pago != null
                })
                .FirstOrDefaultAsync();
        }

        private async Task<List<PedidoRespuestaDto>>
            ObtenerListaPedidos(
                System.Linq.Expressions.Expression<
                    Func<Pedido, bool>> filtro)
        {
            return await _context.Pedidos
                .AsNoTracking()
                .Where(filtro)
                .OrderByDescending(pedido =>
                    pedido.FechaPedido)
                .Select(pedido => new PedidoRespuestaDto
                {
                    IdPedido = pedido.IdPedido,
                    NumeroPedido = pedido.NumeroPedido,
                    FechaPedido = pedido.FechaPedido,
                    Estado = pedido.Estado,
                    Total = pedido.Total,
                    TipoPago = pedido.TipoPago,

                    IdUsuario = pedido.IdUsuario,
                    Usuario = pedido.Usuario.Nombre,

                    IdRestaurante = pedido.IdRestaurante,
                    Restaurante = pedido.Restaurante.Nombre,

                    IdDireccionEntrega =
                        pedido.IdDireccionEntrega,

                    IdRepartidor = pedido.IdRepartidor,

                    Repartidor = pedido.Repartidor == null
                        ? null
                        : pedido.Repartidor.Usuario.Nombre,

                    CalificacionRestaurante =
                        pedido.CalificacionRestaurante,

                    CalificacionRepartidor =
                        pedido.CalificacionRepartidor,

                    CantidadDetalles =
                        pedido.Detalles.Count(),

                    TienePago = pedido.Pago != null
                })
                .ToListAsync();
        }
    }
}