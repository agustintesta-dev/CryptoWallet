// Controllers/AuthController.cs
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using CryptoWallet.API.Models.DTOs;
using CryptoWallet.API.Services;
using CryptoWallet.API.Repositories;

namespace CryptoWallet.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]     // → /api/auth
    public class AuthController : ControllerBase
    {
        private readonly AuthService _authService;
        private readonly UsuarioRepository _usuarioRepo;

        public AuthController(AuthService authService, UsuarioRepository usuarioRepo)
        {
            _authService = authService;
            _usuarioRepo = usuarioRepo;
        }

        // POST /api/auth/registro
        [HttpPost("registro")]
        public async Task<ActionResult<AuthRespuestaDto>> Registro([FromBody] RegistroDto dto)
        {
            try
            {
                var resultado = await _authService.RegistrarAsync(dto);
                return Ok(resultado);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { mensaje = ex.Message });
            }
        }

        // POST /api/auth/login
        [HttpPost("login")]
        public async Task<ActionResult<AuthRespuestaDto>> Login([FromBody] LoginDto dto)
        {
            try
            {
                var resultado = await _authService.LoginAsync(dto);
                return Ok(resultado);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { mensaje = ex.Message });
            }
        }

        // GET /api/auth/perfil — requiere token
        [HttpGet("perfil")]
        [Authorize]
        public async Task<IActionResult> ObtenerPerfil()
        {
            var idClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (idClaim == null) return Unauthorized();

            var usuario = await _usuarioRepo.ObtenerPorIdAsync(int.Parse(idClaim));
            if (usuario == null) return NotFound();

            return Ok(new { usuario.Id, usuario.Nombre, usuario.Email });
        }

        // GET /api/auth/metodos-pago — requiere token
        [HttpGet("metodos-pago")]
        [Authorize]
        public async Task<IActionResult> ObtenerMetodosPago()
        {
            var usuarioId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
            var metodos = await _usuarioRepo.ObtenerMetodosPagoAsync(usuarioId);
            return Ok(metodos);
        }

        // POST /api/auth/metodos-pago — requiere token
        [HttpPost("metodos-pago")]
        [Authorize]
        public async Task<IActionResult> AgregarMetodoPago([FromBody] MetodoPagoDto dto)
        {
            var usuarioId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

            // Máximo 3 métodos de pago
            var cantidad = await _usuarioRepo.ContarMetodosPagoAsync(usuarioId);
            if (cantidad >= 3)
                return BadRequest(new { mensaje = "Máximo 3 métodos de pago permitidos." });

            var id = await _usuarioRepo.AgregarMetodoPagoAsync(usuarioId, dto);
            return Ok(new { id, mensaje = "Método de pago agregado correctamente." });
        }

        // DELETE /api/auth/metodos-pago/5 — requiere token
        [HttpDelete("metodos-pago/{id:int}")]
        [Authorize]
        public async Task<IActionResult> EliminarMetodoPago(int id)
        {
            var usuarioId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
            var eliminado = await _usuarioRepo.EliminarMetodoPagoAsync(id, usuarioId);

            if (!eliminado)
                return NotFound(new { mensaje = "Método de pago no encontrado." });

            return NoContent();
        }
    }
}
