using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProyectoTI_PRR_Backend.Models;

namespace ProyectoTI_PRR_Backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FacturaController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _env;

        public FacturaController(ApplicationDbContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Factura>>> GetFacturas()
        {
            return await _context.Facturas.Include(f => f.Cliente).ToListAsync();
        }

        [HttpPost]
        public async Task<IActionResult> PostFactura([FromForm] Factura factura, IFormFile archivo)
        {
            if (archivo != null && archivo.Length > 0)
            {
                var extension = Path.GetExtension(archivo.FileName);
                var nombreArchivo = $"{Guid.NewGuid()}{extension}";
                var ruta = Path.Combine(_env.WebRootPath, "uploads", nombreArchivo);

                Directory.CreateDirectory(Path.GetDirectoryName(ruta));
                using (var stream = new FileStream(ruta, FileMode.Create))
                {
                    await archivo.CopyToAsync(stream);
                }

                factura.Archivo = $"uploads/{nombreArchivo}";
            }

            _context.Facturas.Add(factura);
            await _context.SaveChangesAsync();
            return Ok(factura);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Factura>> GetFactura(int id)
        {
            var factura = await _context.Facturas.Include(f => f.Cliente).FirstOrDefaultAsync(f => f.IdFactura == id);
            return factura == null ? NotFound() : Ok(factura);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> PutFactura(int id, [FromForm] Factura factura, IFormFile? archivo)
        {
            var facturaExistente = await _context.Facturas.FindAsync(id);
            if (facturaExistente == null)
                return NotFound();

            // Sincronizar estado de pago con el pedido relacionado
            var pedidoRelacionado = await _context.Pedidos.FirstOrDefaultAsync(p => p.FacturaId == facturaExistente.IdFactura);
                if (pedidoRelacionado != null)
                {
                    pedidoRelacionado.EstadoPago = factura.EstadoPago;

                    // Verificar si el pedido aún cumple las condiciones para estar Cerrado
                    if (pedidoRelacionado.EstadoEntrega == "Entregado" && pedidoRelacionado.EstadoPago == "Cancelado")
                    {
                        pedidoRelacionado.EstadoPedido = "Cerrado";
                    }
                    else if (pedidoRelacionado.EstadoEntrega == "Cancelado")
                    {
                        pedidoRelacionado.EstadoPedido = "Cerrado";
                    }
                    else
                    {
                        pedidoRelacionado.EstadoPedido = "Abierto";
                    }
                }

            facturaExistente.Fecha = factura.Fecha;
            facturaExistente.ClienteCedula = factura.ClienteCedula;
            facturaExistente.EstadoPago = factura.EstadoPago;

            if (archivo != null && archivo.Length > 0)
            {
                // Borrar archivo anterior si existe
                if (!string.IsNullOrEmpty(facturaExistente.Archivo))
                {
                    var archivoAnteriorRuta = Path.Combine(_env.WebRootPath, facturaExistente.Archivo);
                    if (System.IO.File.Exists(archivoAnteriorRuta))
                        System.IO.File.Delete(archivoAnteriorRuta);
                }

                // Guardar nuevo archivo
                var nombreArchivo = $"{Guid.NewGuid()}{Path.GetExtension(archivo.FileName)}";
                var ruta = Path.Combine(_env.WebRootPath, "uploads", nombreArchivo);
                Directory.CreateDirectory(Path.GetDirectoryName(ruta)!);
                using var stream = new FileStream(ruta, FileMode.Create);
                await archivo.CopyToAsync(stream);

                facturaExistente.Archivo = $"uploads/{nombreArchivo}";
            }

            await _context.SaveChangesAsync();
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteFactura(int id)
        {
            var factura = await _context.Facturas.FindAsync(id);
            if (factura == null) return NotFound();

            if (!string.IsNullOrEmpty(factura.Archivo))
            {
                var ruta = Path.Combine(_env.WebRootPath, factura.Archivo);
                if (System.IO.File.Exists(ruta)) System.IO.File.Delete(ruta);
            }

            _context.Facturas.Remove(factura);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}