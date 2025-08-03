// FacturasController.cs
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProyectoTI_PRR_Backend.Models;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;

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
                    .ThenInclude(p => p.Cotizacion) // Incluir Cotizacion a través de Pedido
                .Include(f => f.Cliente)
                .Include(f => f.Pagos) // NUEVO: Incluir los pagos
                .ToListAsync();
        }

        // GET: api/Facturas/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Factura>> GetFactura(int id)
        {
            var factura = await _context.Facturas
                .Include(f => f.Pedido)
                    .ThenInclude(p => p.Cotizacion) // NUEVO: Incluir Cotizacion para obtener el total
                .Include(f => f.Cliente)
                .Include(f => f.Pagos) // NUEVO: Incluir los pagos
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

            // Asegurarse de que Pagos sea null o vacío para que EF no intente insertar pagos al crear la factura principal
            // Los pagos se añadirán/editarán en el método PUT si es necesario
            factura.Pagos = null;

            try
            {
                _context.Facturas.Add(factura);
                await _context.SaveChangesAsync();

                var pedidoToUpdate = await _context.Pedidos.FirstOrDefaultAsync(p => p.PedidoId == factura.PedidoId);
                if (pedidoToUpdate != null)
                {
                    pedidoToUpdate.EstadoPago = factura.EstadoPago; // Debería ser "Pendiente" al crear la factura

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

                // Recargar la factura para incluir los pagos si se añadieran en este mismo request (aunque la lógica actual los añade vía PUT)
                // Opcional: Puedes devolver la factura recién creada sin pagos si no se esperan inmediatamente
                // return CreatedAtAction(nameof(GetFactura), new { id = factura.IdFactura }, factura);
                return Ok(factura); // Devolver la factura creada
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

            // Obtener la factura existente con sus pagos y el pedido/cotización
            var existingFactura = await _context.Facturas
                .Include(f => f.Pagos) // Asegurarse de cargar los pagos existentes
                .Include(f => f.Pedido)
                    .ThenInclude(p => p.Cotizacion)
                .AsNoTracking() // No rastrear para evitar conflictos al adjuntar
                .FirstOrDefaultAsync(f => f.IdFactura == id);

            if (existingFactura == null)
            {
                return NotFound();
            }

            var originalEstadoPagoFactura = existingFactura.EstadoPago;
            var originalPagos = existingFactura.Pagos?.ToList() ?? new List<Pago>();
            var incomingPagos = factura.Pagos?.ToList() ?? new List<Pago>();

            // Limpiar la colección de Pagos de la factura entrante para que EF no intente procesarlos directamente
            // La gestión de pagos se hará manualmente a continuación
            factura.Pagos = null;

            _context.Entry(factura).State = EntityState.Modified; // Marcar la factura principal como modificada

            try
            {
                // --- Gestión de Pagos ---
                // Eliminar pagos que no están en la lista entrante
                foreach (var oldPago in originalPagos)
                {
                    if (!incomingPagos.Any(p => p.IdPago == oldPago.IdPago && p.IdPago != 0))
                    {
                        _context.Entry(oldPago).State = EntityState.Deleted;
                    }
                }

                // Añadir o actualizar pagos
                foreach (var newPago in incomingPagos)
                {
                    newPago.FacturaId = factura.IdFactura; // Asegurarse que el FacturaId esté asignado
                    if (newPago.IdPago == 0) // Es un nuevo pago
                    {
                        _context.Entry(newPago).State = EntityState.Added;
                    }
                    else // Es un pago existente
                    {
                        _context.Entry(newPago).State = EntityState.Modified;
                    }
                }
                // --- Fin Gestión de Pagos ---

                await _context.SaveChangesAsync(); // Guardar cambios de factura y pagos

                // --- Recalcular Estado de Pago de la Factura y Pedido ---
                // Necesitamos el total de la cotización para determinar si la factura está "Cancelada"
                var cotizacionTotal = existingFactura.Pedido?.Cotizacion?.Total ?? 0;
                var totalPagosRealizados = _context.Pagos.Where(p => p.FacturaId == factura.IdFactura).Sum(p => (decimal?)p.Monto) ?? 0;

                string newEstadoPagoFactura;
                if (totalPagosRealizados >= cotizacionTotal && cotizacionTotal > 0)
                {
                    newEstadoPagoFactura = "Cancelado";
                }
                else
                {
                    newEstadoPagoFactura = "Pendiente";
                }

                // Si el estado de pago de la factura ha cambiado, actualizar la factura y el pedido
                if (originalEstadoPagoFactura != newEstadoPagoFactura)
                {
                    factura.EstadoPago = newEstadoPagoFactura; // Actualizar el estado de pago de la factura
                    _context.Entry(factura).State = EntityState.Modified; // Marcar como modificado para guardar el nuevo estado
                    await _context.SaveChangesAsync(); // Guardar el estado de pago actualizado de la factura

                    // Actualizar el estado de pago del pedido asociado
                    var pedidoToUpdate = await _context.Pedidos.FirstOrDefaultAsync(p => p.PedidoId == factura.PedidoId);
                    if (pedidoToUpdate != null)
                    {
                        pedidoToUpdate.EstadoPago = factura.EstadoPago; // Sincronizar con el estado de la factura

                        // Re-evaluar el EstadoPedido basado en el nuevo EstadoPago y el EstadoEntrega existente
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
                }
                // --- Fin Recalcular Estado de Pago ---

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
            var factura = await _context.Facturas.Include(f => f.Pagos).FirstOrDefaultAsync(f => f.IdFactura == id);
            if (factura == null)
            {
                return NotFound();
            }

            try
            {
                // Eliminar pagos asociados primero para evitar errores de FK
                if (factura.Pagos != null && factura.Pagos.Any())
                {
                    _context.Pagos.RemoveRange(factura.Pagos);
                }

                _context.Facturas.Remove(factura);
                await _context.SaveChangesAsync();
                return NoContent();
            }
            catch (Exception ex)
            {
                var innerExceptionMessage = ex.InnerException?.Message ?? ex.Message;
                return StatusCode(500, $"Error al eliminar la factura: {innerExceptionMessage}");
            }
        }

        // Helper para generar el número de factura secuencial
        private async Task<string> GenerateNextFacturaNumber()
        {
            var today = DateTime.Today;
            var prefix = $"FAC{today.Year}{today.Month:D2}{today.Day:D2}";

            var existingNumbers = await _context.Facturas
                .Where(f => f.NumeroFactura.StartsWith(prefix))
                .Select(f => f.NumeroFactura)
                .ToListAsync();

            int maxSequence = 0;

            foreach (var num in existingNumbers)
            {
                if (num.Length > prefix.Length && int.TryParse(num.Substring(prefix.Length), out int currentSequence))
                {
                    if (currentSequence > maxSequence)
                    {
                        maxSequence = currentSequence;
                    }
                }
            }

            int nextSequence = maxSequence + 1;
            return $"{prefix}{nextSequence:D4}";
        }
    }
}
