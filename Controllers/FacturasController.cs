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

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var facturas = await _context.Facturas.Include(f => f.Cliente).ToListAsync();
            return Ok(facturas);
        }

        [HttpPost]
        public async Task<IActionResult> PostFactura([FromBody] Factura factura)
        {
            Console.WriteLine("== Factura recibida ==");
            Console.WriteLine($"Cliente: {factura.ClienteCedula}");
            Console.WriteLine($"Fecha: {factura.Fecha}");
            Console.WriteLine($"EstadoPago: {factura.EstadoPago}");
            Console.WriteLine($"ArchivoNombre: {factura.ArchivoNombre}");

            if (!ModelState.IsValid)
            {
                foreach (var modelState in ModelState)
                {
                    Console.WriteLine($"Key: {modelState.Key}");
                    foreach (var error in modelState.Value.Errors)
                    {
                        Console.WriteLine($"  Error: {error.ErrorMessage}");
                    }
                }

                return BadRequest(ModelState);
            }

            _context.Facturas.Add(factura);
            await _context.SaveChangesAsync();
            return Ok(factura);
        }

    }
}
