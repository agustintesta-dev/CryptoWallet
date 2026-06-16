// Repositories/CriptomonedaRepository.cs
// ============================================
// Maneja todo el acceso a la tabla Criptomonedas.
// Dapper ejecuta el SQL y mapea los resultados
// automáticamente a la clase Criptomoneda.
// ============================================
using Dapper;
using Microsoft.Data.SqlClient;
using CryptoWallet.API.Models;

namespace CryptoWallet.API.Repositories
{
    public class CriptomonedaRepository
    {
        private readonly string _cadenaConexion;

        // IConfiguration nos permite leer el appsettings.json
        public CriptomonedaRepository(IConfiguration config)
        {
            _cadenaConexion = config.GetConnectionString("Conexion") ?? throw new InvalidOperationException("Cadena de conexion no configurada");
        }

        // Abre una conexión nueva cada vez que se necesita.
        // Dapper y SqlClient se encargan de cerrarla automáticamente
        // gracias al "using" que usamos en cada método.
        private SqlConnection CrearConexion() => new SqlConnection(_cadenaConexion);

        // ── Obtener todas las criptos activas ────────────────
        public async Task<IEnumerable<Crypto>> ObtenerTodasAsync()
        {
            using var conexion = CrearConexion();
            const string sql = @"
            SELECT Id, Code, Nombre, Simbolo, UrlIcono, Color, EstaActivo, CreadoEn
            FROM Cryptos
            WHERE EstaActivo = 1
            ORDER BY Nombre ASC";

            return await conexion.QueryAsync<Crypto>(sql);
        }

        // ── Obtener una cripto por su código ─────────────────
        // La usamos al crear una transacción para validar
        // que la cripto existe y obtener su Id
        public async Task<Crypto?> ObtenerPorCodigoAsync(string codigo)
        {
            using var conexion = CrearConexion();
            const string sql = @"
            SELECT Id, Code, Nombre, Simbolo, UrlIcono, Color, EstaActivo, CreadoEn
            FROM Cryptos
            WHERE Code = @Codigo AND EstaActivo = 1";

            // QueryFirstOrDefaultAsync devuelve null si no encuentra nada
            // en vez de tirar una excepción — más seguro
            return await conexion.QueryFirstOrDefaultAsync<Crypto>(sql, new {Codigo = codigo });
        }

    }
}
