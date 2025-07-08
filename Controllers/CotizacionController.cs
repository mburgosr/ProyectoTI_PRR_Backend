using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProyectoTI_PRR_Backend.Models;

namespace ProyectoTI_PRR_Backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CotizacionController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public CotizacionController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Cotizacion>>> Get()
        {
            try
            {
                var cotizaciones = await _context.Cotizaciones
                    .Include(c => c.Materiales)
                    .ToListAsync();

                return Ok(cotizaciones);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] Cotizacion cotizacion)
        {
            try
            {
                if (cotizacion == null || cotizacion.Materiales == null || !cotizacion.Materiales.Any())
                    return BadRequest("La cotización o los materiales no son válidos.");

                foreach (var mat in cotizacion.Materiales)
                {
                    mat.Cotizacion = null;
                }

                _context.Cotizaciones.Add(cotizacion);
                await _context.SaveChangesAsync();

                return Ok(new { mensaje = "Cotización creada", id = cotizacion.CotizacionId });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] Cotizacion cotizacion)
        {
            var existente = await _context.Cotizaciones
                .Include(c => c.Materiales)
                .FirstOrDefaultAsync(c => c.CotizacionId == id);

            if (existente == null)
                return NotFound();

            existente.Fecha = cotizacion.Fecha;
            existente.ClienteCedula = cotizacion.ClienteCedula;
            existente.SubTotal = cotizacion.SubTotal;
            existente.IVA = cotizacion.IVA;
            existente.Total = cotizacion.Total;
            existente.NumeroCot = cotizacion.NumeroCot;

            _context.CotizacionMateriales.RemoveRange(existente.Materiales);

            foreach (var mat in cotizacion.Materiales)
            {
                mat.CotizacionId = id;
                _context.CotizacionMateriales.Add(mat);
            }

            await _context.SaveChangesAsync();
            return Ok(new { mensaje = "Cotización actualizada" });
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var cot = await _context.Cotizaciones
                .Include(c => c.Materiales)
                .FirstOrDefaultAsync(c => c.CotizacionId == id);

            if (cot == null)
                return NotFound();

            _context.CotizacionMateriales.RemoveRange(cot.Materiales);
            _context.Cotizaciones.Remove(cot);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
