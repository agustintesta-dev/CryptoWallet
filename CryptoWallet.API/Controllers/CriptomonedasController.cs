using Microsoft.AspNetCore.Mvc;
using CryptoWallet.API.Repositories;
using Microsoft.AspNetCore.Authorization;

namespace CryptoWallet.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CriptomonedasController : ControllerBase
    {
        private readonly CriptomonedaRepository _repo;
        
        public CriptomonedasController(CriptomonedaRepository repo)
        {
            _repo = repo;
        }

        [HttpGet]
        public async Task<IActionResult> ObtenerTodas()
        {
            var criptos = await _repo.ObtenerTodasAsync();
            return Ok(criptos);
        }
    }
}
