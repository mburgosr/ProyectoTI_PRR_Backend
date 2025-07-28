using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProyectoTI_PRR_Backend.Models;

namespace ProyectoTI_PRR_Backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class FacturasController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public FacturasController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/Facturas
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Factura>>> GetFacturas()
        {
            return await _context.Facturas
                .Include(f => f.Pedido)
                .Include(f => f.Cliente)
                .OrderByDescending(f => f.Fecha)
                .ToListAsync();
        }

        // GET: api/Facturas/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Factura>> GetFactura(int id)
        {
            var factura = await _context.Facturas
                .Include(f => f.Pedido)
                .Include(f => f.Cliente)
                .FirstOrDefaultAsync(f => f.IdFactura == id);

            return factura == null ? NotFound() : Ok(factura);
        }

        // POST: api/Facturas
        [HttpPost]
        public async Task<IActionResult> CreateFactura([FromBody] Factura factura)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (string.IsNullOrEmpty(factura.NumeroFactura))
            {
                factura.NumeroFactura = await GenerateNextFacturaNumber();
            }

            try
            {
                _context.Facturas.Add(factura);
                await _context.SaveChangesAsync();

                var pedidoToUpdate = await _context.Pedidos.FirstOrDefaultAsync(p => p.PedidoId == factura.PedidoId);
                if (pedidoToUpdate != null)
                {
                    pedidoToUpdate.EstadoPago = factura.EstadoPago;

                    if (pedidoToUpdate.EstadoEntrega == "Entregado" && pedidoToUpdate.EstadoPago == "Cancelado")
                    {
                        pedidoToUpdate.EstadoPedido = "Cerrado";
                    }
                    else if (pedidoToUpdate.EstadoEntrega == "Cancelado")
                    {
                        pedidoToUpdate.EstadoPedido = "Cerrado";
                    }
                    else
                    {
                        pedidoToUpdate.EstadoPedido = "Abierto";
                    }

                    _context.Entry(pedidoToUpdate).State = EntityState.Modified;
                    await _context.SaveChangesAsync();
                }
                return Ok(factura);
            }
            catch (Exception ex)
            {
                var innerExceptionMessage = ex.InnerException?.Message ?? ex.Message;
                return StatusCode(500, $"Error al guardar la factura: {innerExceptionMessage}");
            }
        }

        // PUT: api/Facturas/5
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateFactura(int id, [FromBody] Factura factura)
        {
            if (id != factura.IdFactura)
            {
                return BadRequest("El ID de la factura no coincide.");
            }

            var existingFactura = await _context.Facturas.AsNoTracking().FirstOrDefaultAsync(f => f.IdFactura == id);
            if (existingFactura == null)
            {
                return NotFound();
            }

            var originalEstadoPago = existingFactura.EstadoPago;

            try
            {
                _context.Entry(factura).State = EntityState.Modified;
                await _context.SaveChangesAsync();

                if (originalEstadoPago != factura.EstadoPago)
                {
                    var pedidoToUpdate = await _context.Pedidos.FirstOrDefaultAsync(p => p.PedidoId == factura.PedidoId);

                    if (pedidoToUpdate != null)
                    {
                        pedidoToUpdate.EstadoPago = factura.EstadoPago;
                        if (pedidoToUpdate.EstadoEntrega == "Entregado" && pedidoToUpdate.EstadoPago == "Cancelado")
                        {
                            pedidoToUpdate.EstadoPedido = "Cerrado";
                        }
                        else if (pedidoToUpdate.EstadoEntrega == "Cancelado") // Si la entrega fue cancelada, el pedido también se cierra
                        {
                            pedidoToUpdate.EstadoPedido = "Cerrado";
                        }
                        else
                        {
                            pedidoToUpdate.EstadoPedido = "Abierto";
                        }
                        _context.Entry(pedidoToUpdate).State = EntityState.Modified;
                        await _context.SaveChangesAsync();
                    }
                }

                return NoContent();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_context.Facturas.Any(f => f.IdFactura == id))
                {
                    return NotFound();
                }
                throw;
            }
            catch (Exception ex)
            {
                var innerExceptionMessage = ex.InnerException?.Message ?? ex.Message;
                return StatusCode(500, $"Error al actualizar la factura: {innerExceptionMessage}");
            }
        }

        // DELETE: api/Facturas/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteFactura(int id)
        {
            var factura = await _context.Facturas.FindAsync(id);
            if (factura == null)
            {
                return NotFound();
            }

            _context.Facturas.Remove(factura);
            await _context.SaveChangesAsync();
            return NoContent();
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