using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MiApi.Data;
using MiApi.DTOs;
using MiApi.Models;

namespace MiApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PagosController : ControllerBase
    {
        private readonly MyMDbContext _context;

        private static readonly string[] MetodosPagoPermitidos =
        {
            "Efectivo",
            "Tarjeta",
            "Transferencia",
            "PayPal"
        };

        public PagosController(MyMDbContext context)
        {
            _context = context;
        }

        // GET: api/pagos
        // GET: api/pagos?metodoPago=Efectivo
        // GET: api/pagos?idUsuario=1
        // GET: api/pagos?idRestaurante=2
        [HttpGet]
        public async Task<ActionResult<IEnumerable<PagoRespuestaDto>>>
            ObtenerPagos(
                [FromQuery] string? metodoPago,
                [FromQuery] int? idUsuario,
                [FromQuery] int? idRestaurante)
        {
            var consulta = _context.Pagos
                .AsNoTracking()
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(metodoPago))
            {
                var metodo = metodoPago.Trim();

                consulta = consulta.Where(pago =>
                    pago.MetodoPago == metodo);
            }

            if (idUsuario.HasValue)
            {
                consulta = consulta.Where(pago =>
                    pago.Pedido.IdUsuario == idUsuario.Value);
            }

            if (idRestaurante.HasValue)
            {
                consulta = consulta.Where(pago =>
                    pago.Pedido.IdRestaurante == idRestaurante.Value);
            }

            var pagos = await consulta
                .OrderByDescending(pago => pago.FechaPago)
                .Select(pago => new PagoRespuestaDto
                {
                    IdPago = pago.IdPago,
                    IdPedido = pago.IdPedido,
                    NumeroPedido = pago.Pedido.NumeroPedido,
                    Monto = pago.Monto,
                    MetodoPago = pago.MetodoPago,
                    FechaPago = pago.FechaPago,
                    EstadoPedido = pago.Pedido.Estado,

                    IdUsuario = pago.Pedido.IdUsuario,
                    Usuario = pago.Pedido.Usuario.Nombre,

                    IdRestaurante = pago.Pedido.IdRestaurante,
                    Restaurante = pago.Pedido.Restaurante.Nombre
                })
                .ToListAsync();

            return Ok(pagos);
        }

        // GET: api/pagos/5
        [HttpGet("{id:int}")]
        public async Task<ActionResult<PagoRespuestaDto>>
            ObtenerPagoPorId(int id)
        {
            var pago = await ObtenerPagoDto(id);

            if (pago is null)
            {
                return NotFound(new
                {
                    mensaje = $"No existe el pago con ID {id}."
                });
            }

            return Ok(pago);
        }

        // GET: api/pagos/pedido/3
        [HttpGet("pedido/{idPedido:int}")]
        public async Task<ActionResult<PagoRespuestaDto>>
            ObtenerPagoPorPedido(int idPedido)
        {
            var pedidoExiste = await _context.Pedidos
                .AnyAsync(pedido => pedido.IdPedido == idPedido);

            if (!pedidoExiste)
            {
                return NotFound(new
                {
                    mensaje = $"No existe el pedido con ID {idPedido}."
                });
            }

            var pago = await _context.Pagos
                .AsNoTracking()
                .Where(pago => pago.IdPedido == idPedido)
                .Select(pago => new PagoRespuestaDto
                {
                    IdPago = pago.IdPago,
                    IdPedido = pago.IdPedido,
                    NumeroPedido = pago.Pedido.NumeroPedido,
                    Monto = pago.Monto,
                    MetodoPago = pago.MetodoPago,
                    FechaPago = pago.FechaPago,
                    EstadoPedido = pago.Pedido.Estado,

                    IdUsuario = pago.Pedido.IdUsuario,
                    Usuario = pago.Pedido.Usuario.Nombre,

                    IdRestaurante = pago.Pedido.IdRestaurante,
                    Restaurante = pago.Pedido.Restaurante.Nombre
                })
                .FirstOrDefaultAsync();

            if (pago is null)
            {
                return NotFound(new
                {
                    mensaje = "El pedido todavía no tiene un pago registrado."
                });
            }

            return Ok(pago);
        }

        // POST: api/pagos
        [HttpPost]
        public async Task<ActionResult<PagoRespuestaDto>>
            CrearPago(CrearPagoDto dto)
        {
            var pedido = await _context.Pedidos
                .Include(pedido => pedido.Detalles)
                .FirstOrDefaultAsync(pedido =>
                    pedido.IdPedido == dto.IdPedido);

            if (pedido is null)
            {
                return BadRequest(new
                {
                    mensaje = $"No existe el pedido con ID {dto.IdPedido}."
                });
            }

            var pagoExistente = await _context.Pagos
                .AnyAsync(pago => pago.IdPedido == dto.IdPedido);

            if (pagoExistente)
            {
                return Conflict(new
                {
                    mensaje = "El pedido ya tiene un pago registrado."
                });
            }

            if (pedido.Estado.Equals(
                    "Cancelado",
                    StringComparison.OrdinalIgnoreCase))
            {
                return BadRequest(new
                {
                    mensaje = "No se puede pagar un pedido cancelado."
                });
            }

            if (pedido.Detalles.Count == 0)
            {
                return BadRequest(new
                {
                    mensaje =
                        "No se puede registrar el pago de un pedido vacío."
                });
            }

            if (pedido.Total <= 0)
            {
                return BadRequest(new
                {
                    mensaje =
                        "El total del pedido debe ser mayor que cero."
                });
            }

            var metodoPago = ObtenerMetodoPagoValido(
                dto.MetodoPago);

            if (metodoPago is null)
            {
                return BadRequest(new
                {
                    mensaje = "El método de pago no es válido.",
                    metodosPermitidos = MetodosPagoPermitidos
                });
            }

            var nuevoPago = new Pago
            {
                IdPedido = pedido.IdPedido,

                // El monto se obtiene del pedido y no del cliente.
                Monto = pedido.Total,

                MetodoPago = metodoPago,
                FechaPago = DateTime.UtcNow
            };

            _context.Pagos.Add(nuevoPago);

            // Opcional: actualizar el estado del pedido.
            if (pedido.Estado.Equals(
                    "Pendiente",
                    StringComparison.OrdinalIgnoreCase))
            {
                pedido.Estado = "Confirmado";
            }

            await _context.SaveChangesAsync();

            var pagoCreado = await ObtenerPagoDto(
                nuevoPago.IdPago);

            return CreatedAtAction(
                nameof(ObtenerPagoPorId),
                new { id = nuevoPago.IdPago },
                pagoCreado
            );
        }

        // PUT: api/pagos/5
        [HttpPut("{id:int}")]
        public async Task<ActionResult<PagoRespuestaDto>>
            ActualizarPago(
                int id,
                ActualizarPagoDto dto)
        {
            var pago = await _context.Pagos
                .Include(pago => pago.Pedido)
                .FirstOrDefaultAsync(pago =>
                    pago.IdPago == id);

            if (pago is null)
            {
                return NotFound(new
                {
                    mensaje = $"No existe el pago con ID {id}."
                });
            }

            var metodoPago = ObtenerMetodoPagoValido(
                dto.MetodoPago);

            if (metodoPago is null)
            {
                return BadRequest(new
                {
                    mensaje = "El método de pago no es válido.",
                    metodosPermitidos = MetodosPagoPermitidos
                });
            }

            if (pago.Pedido.Estado.Equals(
                    "Cancelado",
                    StringComparison.OrdinalIgnoreCase))
            {
                return BadRequest(new
                {
                    mensaje =
                        "No se puede modificar el pago de un pedido cancelado."
                });
            }

            pago.MetodoPago = metodoPago;

            /*
             * Se sincroniza el monto con el total actual del pedido.
             * Puedes eliminar esta línea si deseas conservar el monto
             * histórico del momento del pago.
             */
            pago.Monto = pago.Pedido.Total;

            await _context.SaveChangesAsync();

            return Ok(await ObtenerPagoDto(id));
        }

        // DELETE: api/pagos/5
        [HttpDelete("{id:int}")]
        public async Task<ActionResult> EliminarPago(int id)
        {
            var pago = await _context.Pagos
                .Include(pago => pago.Pedido)
                .FirstOrDefaultAsync(pago =>
                    pago.IdPago == id);

            if (pago is null)
            {
                return NotFound(new
                {
                    mensaje = $"No existe el pago con ID {id}."
                });
            }

            if (pago.Pedido.Estado.Equals(
                    "Entregado",
                    StringComparison.OrdinalIgnoreCase))
            {
                return Conflict(new
                {
                    mensaje =
                        "No se puede eliminar el pago de un pedido entregado."
                });
            }

            _context.Pagos.Remove(pago);

            /*
             * Opcional: regresar el pedido a estado Pendiente.
             */
            if (pago.Pedido.Estado.Equals(
                    "Confirmado",
                    StringComparison.OrdinalIgnoreCase))
            {
                pago.Pedido.Estado = "Pendiente";
            }

            await _context.SaveChangesAsync();

            return Ok(new
            {
                mensaje = "Pago eliminado correctamente."
            });
        }

        private async Task<PagoRespuestaDto?>
            ObtenerPagoDto(int idPago)
        {
            return await _context.Pagos
                .AsNoTracking()
                .Where(pago => pago.IdPago == idPago)
                .Select(pago => new PagoRespuestaDto
                {
                    IdPago = pago.IdPago,
                    IdPedido = pago.IdPedido,
                    NumeroPedido = pago.Pedido.NumeroPedido,
                    Monto = pago.Monto,
                    MetodoPago = pago.MetodoPago,
                    FechaPago = pago.FechaPago,
                    EstadoPedido = pago.Pedido.Estado,

                    IdUsuario = pago.Pedido.IdUsuario,
                    Usuario = pago.Pedido.Usuario.Nombre,

                    IdRestaurante = pago.Pedido.IdRestaurante,
                    Restaurante = pago.Pedido.Restaurante.Nombre
                })
                .FirstOrDefaultAsync();
        }

        private static string? ObtenerMetodoPagoValido(
            string metodoPago)
        {
            if (string.IsNullOrWhiteSpace(metodoPago))
            {
                return null;
            }

            return MetodosPagoPermitidos.FirstOrDefault(
                metodoPermitido =>
                    metodoPermitido.Equals(
                        metodoPago.Trim(),
                        StringComparison.OrdinalIgnoreCase));
        }
    }
}