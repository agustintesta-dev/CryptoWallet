// Controllers/PortfolioController.cs
using Microsoft.AspNetCore.Mvc;
using CryptoWallet.API.Services;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace CryptoWallet.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")] // → /api/portfolio
    [Authorize]
    public class PortfolioController : ControllerBase
    {
        private readonly PortfolioService _portfolioService;
        
        public PortfolioController(PortfolioService portfolioService)
        {
            _portfolioService = portfolioService;
        }

        private int ObtenerUsuarioId() =>
            int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

        // GET /api/portfolio
        [HttpGet]
        public async Task<IActionResult> ObtenerPortfolio()
        {
            try
            {
                var portfolio = await _portfolioService.ObtenerPortfolioActualAsync(ObtenerUsuarioId());
                return Ok(portfolio);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { mensaje = ex.Message });
            }
        }

        [HttpGet("historial")]
        [Authorize]
        public async Task<IActionResult> ObtenerHistorial()
        {
            try
            {
                var historial = await _portfolioService.ObtenerHistorialAsync(ObtenerUsuarioId());
                return Ok(historial);
            } 
            catch (Exception ex)
            {
                return StatusCode(500, new { mensaje = ex.Message });
            }
            
        }
    }
}
