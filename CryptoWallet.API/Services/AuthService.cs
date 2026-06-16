// Services/AuthService.cs
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using CryptoWallet.API.Models;
using CryptoWallet.API.Models.DTOs;
using CryptoWallet.API.Repositories;

namespace CryptoWallet.API.Services
{
    public class AuthService
    {
        private readonly UsuarioRepository _usuarioRepo;
        private readonly IConfiguration _config;

        public AuthService(UsuarioRepository usuarioRepo, IConfiguration config)
        {
            _usuarioRepo = usuarioRepo;
            _config = config;
        }

        public async Task<AuthRespuestaDto> RegistrarAsync(RegistroDto dto)
        {
            // Normalizar antes de buscar
            var emailNormalizado = dto.Email.ToLower().Trim();

            // Verificar que el email no esté en uso
            var existe = await _usuarioRepo.ObtenerPorEmailAsync(emailNormalizado);
            if (existe != null)
                throw new InvalidOperationException("Ya existe una cuenta con ese email.");

            // Hashear la contraseña — nunca guardar en texto plano
            var hash = BCrypt.Net.BCrypt.HashPassword(dto.Password);

            var usuario = new Usuario
            {
                Nombre = dto.Nombre,
                Email = emailNormalizado,
                PasswordHash = hash
            };

            var id = await _usuarioRepo.CrearAsync(usuario);
            usuario.Id = id;

            return new AuthRespuestaDto
            {
                Id = usuario.Id,
                Nombre = usuario.Nombre,
                Email = usuario.Email,
                Token = GenerarToken(usuario)
            };
        }

        public async Task<AuthRespuestaDto> LoginAsync(LoginDto dto)
        {
            var usuario = await _usuarioRepo.ObtenerPorEmailAsync(dto.Email.ToLower().Trim());

            // Mismo mensaje para email inexistente y contraseña incorrecta
            // — no le damos pistas a posibles atacantes

            bool credencialesValidas = usuario != null
                && BCrypt.Net.BCrypt.Verify(dto.Password, usuario.PasswordHash);

            if (!credencialesValidas)
                throw new InvalidOperationException("Email o contraseña incorrectos.");

            return new AuthRespuestaDto
            {
                Id = usuario!.Id,
                Nombre = usuario.Nombre,
                Email = usuario.Email,
                Token = GenerarToken(usuario)
            };
        }

        private string GenerarToken(Usuario usuario)
        {
            var clave = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(_config["Jwt:Clave"]!));
            var credencial = new SigningCredentials(clave, SecurityAlgorithms.HmacSha256);

            // Los claims son los datos que van dentro del token
            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, usuario.Id.ToString()),
                new Claim(ClaimTypes.Name, usuario.Nombre),
                new Claim(ClaimTypes.Email, usuario.Email)
            };

            var token = new JwtSecurityToken(
                issuer: _config["Jwt:Emisor"],
                audience: _config["Jwt:Audiencia"],
                claims: claims,
                expires: DateTime.UtcNow.AddHours(
                    double.Parse(_config["Jwt:ExpiracionHoras"]!)),
                signingCredentials: credencial
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
