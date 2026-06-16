// Repositories/UsuarioRepository.cs
using Dapper;
using Microsoft.Data.SqlClient;
using CryptoWallet.API.Models;
using CryptoWallet.API.Models.DTOs;


namespace CryptoWallet.API.Repositories
{
    public class UsuarioRepository
    {
        private readonly string _cadenaConexion;
        
        public UsuarioRepository(IConfiguration config)
        {
            _cadenaConexion = config.GetConnectionString("Conexion")
                ?? throw new InvalidOperationException("Cadena de conexión no configurada");
        }

        private SqlConnection CrearConexion() => new SqlConnection(_cadenaConexion);

        public async Task<Usuario?> ObtenerPorEmailAsync(string email)
        {
            using var conexion = CrearConexion();
            return await conexion.QueryFirstOrDefaultAsync<Usuario>(
                "SELECT Id, Nombre, Email, PasswordHash, CreadoEn, ActualizadoEn FROM Usuarios WHERE Email = @Email",
                new { Email = email });
        }
        
        public async Task<Usuario?> ObtenerPorIdAsync (int id)
        {
            using var conexion = CrearConexion();
            return await conexion.QueryFirstOrDefaultAsync<Usuario>(
                "SELECT Id, Nombre, Email, PasswordHash, CreadoEn, ActualizadoEn FROM Usuarios WHERE Id = @Id",
                new {  Id = id });
        }

        public async Task<int> CrearAsync(Usuario usuario)
        {
            using var conexion = CrearConexion();
            const string sql = @"
                INSERT INTO Usuarios (Nombre, Email, PasswordHash)
                VALUES (@Nombre, @Email, @PasswordHash);
                SELECT SCOPE_IDENTITY();";
            return await conexion.ExecuteScalarAsync<int>(sql, usuario);
        }

        // ── Métodos de pago ───────────────────────────────────
        public async Task<IEnumerable<MetodoPagoRespuestaDto>> ObtenerMetodosPagoAsync (int usuarioId)
        {
            using var conexion = CrearConexion();
            return await conexion.QueryAsync<MetodoPagoRespuestaDto>(
                "SELECT Id, Tipo, UltimosDigitos, Alias, CBU AS Cbu, Banco, EsPrincipal, CreadoEn FROM MetodosPago WHERE UsuarioId = @UsuarioId ORDER BY EsPrincipal DESC",
                new { UsuarioId = usuarioId });

        }

        public async Task<int> ContarMetodosPagoAsync(int usuarioId)
        {
            using var conexion = CrearConexion();
            return await conexion.ExecuteScalarAsync<int>(
                "SELECT COUNT(*) FROM MetodosPago WHERE UsuarioId = @UsuarioId",
                new { UsuarioId = usuarioId });
        }

        public async Task<int> AgregarMetodoPagoAsync(int usuarioId, MetodoPagoDto dto)
        {
            using var conexion = CrearConexion();

            // Si es principal, quitamos el principal anterior
            if (dto.EsPrincipal)
                await conexion.ExecuteAsync(
                    "UPDATE MetodosPago SET EsPrincipal = 0 WHERE UsuarioId = @UsuarioId",
                    new { UsuarioId = usuarioId });

            const string sql = @"
                INSERT INTO MetodosPago
                    (UsuarioId, Tipo, UltimosDigitos, Alias, CBU, Banco, EsPrincipal)
                VALUES
                    (@UsuarioId, @Tipo, @UltimosDigitos, @Alias, @CBU, @Banco, @EsPrincipal);
                SELECT SCOPE_IDENTITY()";

            return await conexion.ExecuteScalarAsync<int>(sql, new
            {
                UsuarioId = usuarioId,
                dto.Tipo,
                dto.UltimosDigitos,
                dto.Alias,
                dto.CBU,
                dto.Banco,
                dto.EsPrincipal
            });
        }

        public async Task<bool> EliminarMetodoPagoAsync(int id, int usuarioId)
        {
            using var conexion = CrearConexion();
            var filas = await conexion.ExecuteAsync(
                "DELETE FROM MetodosPago WHERE Id = @Id AND UsuarioId = @UsuarioId",
                new { Id = id, UsuarioId = usuarioId });
            return filas > 0;
        }
    }
}
