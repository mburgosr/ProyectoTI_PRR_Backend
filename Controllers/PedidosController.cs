using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProyectoTI_PRR_Backend.Models;

namespace ProyectoTI_PRR_Backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PedidosController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public PedidosController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/Pedidos
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Pedido>>> GetPedidos()
        {
            return await _context.Pedidos
                .Include(p => p.Cotizacion)
                .Include(p => p.Cliente)
                .Include(p => p.Factura)
                .OrderByDescending(p => p.Fecha)
                .ToListAsync();
        }

        // GET: api/Pedidos/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Pedido>> GetPedido(int id)
        {
            var pedido = await _context.Pedidos
                .Include(p => p.Cotizacion)
                .Include(p => p.Cliente)
                .Include(p => p.Factura)
                .FirstOrDefaultAsync(p => p.PedidoId == id);

            return pedido == null ? NotFound() : Ok(pedido);
        }

        // POST: api/Pedidos
        [HttpPost]
        public async Task<IActionResult> CreatePedido([FromBody] Pedido pedido)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (string.IsNullOrEmpty(pedido.NumeroPedido))
            {
                pedido.NumeroPedido = await GenerateNextPedidoNumber();
            }

            if (string.IsNullOrEmpty(pedido.EstadoEntrega)) pedido.EstadoEntrega = "Pendiente";
            if (string.IsNullOrEmpty(pedido.EstadoPago)) pedido.EstadoPago = "Pendiente";
            if (string.IsNullOrEmpty(pedido.EstadoPedido)) pedido.EstadoPedido = "Abierto";

            try
            {
                _context.Pedidos.Add(pedido);
                await _context.SaveChangesAsync();

                // ---CREAR FACTURA---
                var newFactura = new Factura
                {
                    NumeroFactura = await GenerateNextFacturaNumber(),
                    PedidoId = pedido.PedidoId,
                    Fecha = DateTime.Now,
                    ClienteCedula = pedido.ClienteCedula,
                    EstadoPago = pedido.EstadoPago,
                    Archivo = null,
                    Observaciones = $"Factura del Pedido {pedido.NumeroPedido}"
                };

                _context.Facturas.Add(newFactura);
                await _context.SaveChangesAsync();

                pedido.FacturaId = newFactura.IdFactura;
                _context.Entry(pedido).State = EntityState.Modified;
                await _context.SaveChangesAsync();

                return CreatedAtAction(nameof(GetPedido), new { id = pedido.PedidoId }, pedido);
            }
            catch (Exception ex)
            {
                var innerExceptionMessage = ex.InnerException?.Message ?? ex.Message;
                return StatusCode(500, $"Error al guardar el pedido y crear la factura: {innerExceptionMessage}");
            }
        }

        // PUT: api/Pedidos/5
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdatePedido(int id, [FromBody] Pedido pedido)
        {
            if (id != pedido.PedidoId)
            {
                return BadRequest("El ID del pedido no coincide.");
            }

            var existingPedido = await _context.Pedidos.AsNoTracking().FirstOrDefaultAsync(p => p.PedidoId == id);
            if (existingPedido == null)
            {
                return NotFound();
            }

            try
            {
                _context.Entry(pedido).State = EntityState.Modified;
                await _context.SaveChangesAsync();
                return NoContent();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_context.Pedidos.Any(e => e.PedidoId == id))
                {
                    return NotFound();
                }
                throw;
            }
            catch (Exception ex)
            {
                var innerExceptionMessage = ex.InnerException?.Message ?? ex.Message;
                return StatusCode(500, $"Error al actualizar el pedido: {innerExceptionMessage}");
            }
        }

        // DELETE: api/Pedidos/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeletePedido(int id)
        {
            var pedido = await _context.Pedidos.FindAsync(id);
            if (pedido == null)
            {
                return NotFound();
            }

            // borrar factura asociada si existe
            // var factura = await _context.Facturas.FirstOrDefaultAsync(f => f.PedidoId == id);
            // if (factura != null) {
            //    _context.Facturas.Remove(factura);
            // }

            _context.Pedidos.Remove(pedido);
            await _context.SaveChangesAsync();
            return NoContent();
        }

        private async Task<string> GenerateNextPedidoNumber()
        {
            var today = DateTime.Today;
            var prefix = $"PED{today.Year}{today.Month:D2}{today.Day:D2}";

            var lastPedido = await _context.Pedidos
                .Where(p => p.NumeroPedido.StartsWith(prefix))
                .OrderByDescending(p => p.NumeroPedido)
                .Select(p => p.NumeroPedido)
                .FirstOrDefaultAsync();

            int sequence = 1;
            if (lastPedido != null)
            {
                var lastSequenceStr = lastPedido.Substring(prefix.Length);
                if (int.TryParse(lastSequenceStr, out int lastSequence))
                {
                    sequence = lastSequence + 1;
                }
            }
            return $"{prefix}{sequence:D4}";
        }

        private async Task<string> GenerateNextFacturaNumber()
        {
            var today = DateTime.Today;
            var prefix = $"FAC{today.Year}{today.Month:D2}{today.Day:D2}";

            var lastFactura = await _context.Facturas
                .Where(f => f.NumeroFactura.StartsWith(prefix))
                .OrderByDescending(f => f.NumeroFactura)
                .Select(f => f.NumeroFactura)
                .FirstOrDefaultAsync();

            int sequence = 1;
            if (lastFactura != null)
            {
                var lastSequenceStr = lastFactura.Substring(prefix.Length);
                if (int.TryParse(lastSequenceStr, out int lastSequence))
                {
                    sequence = lastSequence + 1;
                }
            }
            return $"{prefix}{sequence:D4}";
        }
    }
}
