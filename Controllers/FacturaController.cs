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
                .Include(f => f.Cliente)
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
            // Validar el modelo recibido del frontend
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // El NumeroFactura se genera en el backend para asegurar unicidad y secuencia
            if (string.IsNullOrEmpty(factura.NumeroFactura))
            {
                factura.NumeroFactura = await GenerateNextFacturaNumber();
            }

            try
            {
                // Al agregar 'factura', EF Core automáticamente establecerá las relaciones
                // si PedidoId y ClienteCedula corresponden a entidades existentes en la DB.
                // No es necesario cargar explícitamente Pedido o Cliente aquí si solo se envían los IDs.
                _context.Facturas.Add(factura);
                await _context.SaveChangesAsync();
                return Ok(factura);
            }
            catch (Exception ex)
            {
                // Captura la excepción interna para un mejor diagnóstico
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

            try
            {
                // Al actualizar, EF Core también manejará las relaciones por los IDs.
                _context.Entry(factura).State = EntityState.Modified;
                await _context.SaveChangesAsync();
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

        // Helper para generar el número de factura secuencial
        private async Task<string> GenerateNextFacturaNumber()
        {
            var today = DateTime.Today;
            var prefix = $"FAC{today.Year}{today.Month:D2}{today.Day:D2}";

            // Encuentra el último número de factura para hoy
            var lastFactura = await _context.Facturas
                .Where(f => f.NumeroFactura.StartsWith(prefix))
                .OrderByDescending(f => f.NumeroFactura)
                .Select(f => f.NumeroFactura)
                .FirstOrDefaultAsync();

            int sequence = 1;
            if (lastFactura != null)
            {
                // Extrae el número de secuencia del último número de factura
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