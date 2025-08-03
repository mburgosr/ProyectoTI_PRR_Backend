// PagosController.cs
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
    public class PagosController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public PagosController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/Pagos
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Pago>>> GetPagos()
        {
            return await _context.Pagos.ToListAsync();
        }

        // GET: api/Pagos/ByFactura/5
        [HttpGet("ByFactura/{facturaId}")]
        public async Task<ActionResult<IEnumerable<Pago>>> GetPagosByFacturaId(int facturaId)
        {
            return await _context.Pagos
                                 .Where(p => p.FacturaId == facturaId)
                                 .OrderBy(p => p.FechaPago) // Opcional: ordenar por fecha
                                 .ToListAsync();
        }

        // GET: api/Pagos/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Pago>> GetPago(int id)
        {
            var pago = await _context.Pagos.FindAsync(id);

            if (pago == null)
            {
                return NotFound();
            }

            return pago;
        }

        // POST: api/Pagos
        [HttpPost]
        public async Task<ActionResult<Pago>> PostPago([FromBody] Pago pago)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                _context.Pagos.Add(pago);
                await _context.SaveChangesAsync();

                return CreatedAtAction(nameof(GetPago), new { id = pago.IdPago }, pago);
            }
            catch (Exception ex)
            {
                var innerExceptionMessage = ex.InnerException?.Message ?? ex.Message;
                return StatusCode(500, $"Error al guardar el pago: {innerExceptionMessage}");
            }
        }

        // PUT: api/Pagos/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutPago(int id, [FromBody] Pago pago)
        {
            if (id != pago.IdPago)
            {
                return BadRequest("El ID del pago no coincide.");
            }

            _context.Entry(pago).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_context.Pagos.Any(e => e.IdPago == id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }
            catch (Exception ex)
            {
                var innerExceptionMessage = ex.InnerException?.Message ?? ex.Message;
                return StatusCode(500, $"Error al actualizar el pago: {innerExceptionMessage}");
            }

            return NoContent();
        }

        // DELETE: api/Pagos/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeletePago(int id)
        {
            var pago = await _context.Pagos.FindAsync(id);
            if (pago == null)
            {
                return NotFound();
            }

            try
            {
                _context.Pagos.Remove(pago);
                await _context.SaveChangesAsync();
                return NoContent();
            }
            catch (Exception ex)
            {
                var innerExceptionMessage = ex.InnerException?.Message ?? ex.Message;
                return StatusCode(500, $"Error al eliminar el pago: {innerExceptionMessage}");
            }
        }
    }
}
