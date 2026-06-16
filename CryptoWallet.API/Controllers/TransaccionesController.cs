// Controllers/TransaccionesController.cs
// ============================================
// Recibe las peticiones HTTP de Vue.js
// y devuelve respuestas JSON.
// No tiene lógica de negocio — solo delega
// al Service y traduce el resultado a HTTP.
// ============================================
using Microsoft.AspNetCore.Mvc;
using CryptoWallet.API.Models.DTOs;
using CryptoWallet.API.Repositories;
using CryptoWallet.API.Services;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace CryptoWallet.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]    // → /api/transacciones
    [Authorize]
    public class TransaccionesController : ControllerBase
    {
        private readonly TransaccionesRepository _repo;
        private readonly TransaccionService _service;

        public TransaccionesController(
            TransaccionesRepository repo,
            TransaccionService service)
        {
            _repo = repo;
            _service = service;
        }

        private int ObtenerUsuarioId() =>
            int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

        // GET /api/transacciones
        [HttpGet]
        public async Task<ActionResult<IEnumerable<TransaccionRespuestaDto>>> ObtenerTodos(
            [FromQuery] int pagina = 1,
            [FromQuery] int tamañoPagina = 20,
            [FromQuery] string? codigoCripto = null,
            [FromQuery] string? accion = null,
            [FromQuery] DateTime? fechaDesde = null,
            [FromQuery] DateTime? fechaHasta = null)
        {
            var resultado = await _repo.ObtenerTodasAsync(
                ObtenerUsuarioId(), pagina, tamañoPagina, codigoCripto, accion, fechaDesde, fechaHasta);
            return Ok(resultado);   // 200 OK + lista en JSON
        }

        // GET /api/transacciones/5
        [HttpGet("{id:int}")]
        public async Task<ActionResult<TransaccionRespuestaDto>> ObtenerPorId(int id)
        {
            var transaccion = await _repo.ObtenerPorIdAsync(id, ObtenerUsuarioId());
            if (transaccion == null)
                return NotFound(new { mensaje = $"Transaccion #{id} no encontrada" });

            return Ok(transaccion);
        }

        // POST /api/transacciones
        [HttpPost]
        public async Task<ActionResult<TransaccionRespuestaDto>> Crear(
            [FromBody] CrearTransaccionDto dto)
        {
            try
            {
                var creada = await _service.CrearAsync(dto, ObtenerUsuarioId());

                // 201 Created — le dice al cliente que el recurso fue creado
                // e incluye la URL donde puede encontrarlo

                return CreatedAtAction(
                    nameof(ObtenerPorId),
                    new { id = creada.Id },
                    creada);
            }
            catch (InvalidOperationException ex)
            {
                // Error de negocio — saldo insuficiente, cripto inexistente, etc.
                return BadRequest(new { mensaje = ex.Message });   
            }
            catch (Exception ex)
            {
                // Error inesperado — lo logueamos y devolvemos 500
                return StatusCode(500, new { mensaje = ex.Message });
            }
        }

        // GET /api/transacciones/5/preview?cantidad=0.002
        // Previsualiza el impacto antes de confirmar la edición
        [HttpGet("{id:int}/preview")]
        public async Task<ActionResult<PreviewEdicionDto>> PreviewEdicion(int id, [FromQuery] decimal cantidad)
        {
            try
            {
                var preview = await _service.PreviewEdicionAsync(id, ObtenerUsuarioId(), cantidad);
                return Ok(preview);
            }
            catch (InvalidOperationException ex)
            {
                return NotFound(new { mensaje = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { mensaje = ex.Message });
            }
        }


        [HttpPatch("{id:int}")]
        public async Task<IActionResult> Editar(int id, [FromBody] EditartransaccionesSimpleDto dto)
        {
            try
            {
                await _service.EditarConRecalculoAsync(id, ObtenerUsuarioId(), dto);
                return NoContent();
            }
            catch (InvalidOperationException ex) 
            {
                return NotFound(new { mensaje = ex.Message});
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { mensaje = ex.Message });
            }
        }

        // DELETE /api/transacciones/5
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Eliminar(int id)
        {
            var usuarioId = ObtenerUsuarioId();
            var eliminada = await _repo.EliminarAsync(id, usuarioId);
            if (!eliminada)
                return NotFound(new { mensaje = $"Transaccion #{id} no encontrada." });

            return NoContent();     // 204
        }
    }
}
