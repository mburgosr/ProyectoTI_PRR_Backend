using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProyectoTI_PRR_Backend.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ProyectoTI_PRR_Backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ReporteDiarioController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public ReporteDiarioController(ApplicationDbContext context)
        {
            _context = context;
        }

        // Obtener todos los reportes con sus detalles
        [HttpGet]
        public async Task<ActionResult<IEnumerable<ReporteDiario>>> Get()
        {
            try
            {
                var reportes = await _context.ReportesDiarios
                    .Include(r => r.Detalles)
                    .ToListAsync();

                return Ok(reportes);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message, stack = ex.StackTrace });
            }
        }


        [HttpPost]
        public async Task<IActionResult> Create([FromBody] ReporteDiario reporte)
        {
            try
            {
                if (reporte == null || reporte.Detalles == null || !reporte.Detalles.Any())
                    return BadRequest("El reporte o los detalles no son válidos.");

                // Prevenir referencias circulares innecesarias
                foreach (var detalle in reporte.Detalles)
                {
                    detalle.ReporteDiario = null;
                }

                _context.ReportesDiarios.Add(reporte);
                await _context.SaveChangesAsync();

                return Ok(reporte);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message, stack = ex.StackTrace });
            }
        }
        // Eliminar un reporte y sus detalles por ID
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var reporte = await _context.ReportesDiarios
                .Include(r => r.Detalles)
                .FirstOrDefaultAsync(r => r.ReporteDiario_Id == id);

            if (reporte == null)
                return NotFound();

            _context.DetallesReporte.RemoveRange(reporte.Detalles);
            _context.ReportesDiarios.Remove(reporte);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
