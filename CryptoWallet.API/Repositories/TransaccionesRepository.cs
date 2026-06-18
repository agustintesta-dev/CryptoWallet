// Repositories/TransaccionRepository.cs
// ============================================
// Maneja todo el acceso a la tabla Transacciones.
// Cada método hace UNA cosa — ni más ni menos.
// ============================================
using Dapper;
using Microsoft.Data.SqlClient;
using CryptoWallet.API.Models;
using CryptoWallet.API.Models.DTOs;

namespace CryptoWallet.API.Repositories
{
    public class TransaccionesRepository
    {
        private readonly string _cadenaConexion;

        public TransaccionesRepository(IConfiguration config)
        {
            _cadenaConexion = config.GetConnectionString("Conexion")
                ?? throw new InvalidOperationException("Cadena de conexion no configurada");
        }

        private SqlConnection CrearConexion() => new SqlConnection(_cadenaConexion);

        // ── Obtener todas las transacciones ──────────────────
        // Hacemos JOIN con Criptomonedas para traer el nombre,
        // símbolo y color sin tener que hacer otra consulta aparte
        public async Task<TransaccionesPaginadasDtos> ObtenerTodasAsync(
            int usuarioId, 
            int pagina = 1, 
            int tamañoPagina = 20,
            string? codigoCripto = null,
            string? accion = null,
            DateTime? fechaDesde = null,
            DateTime? fechaHasta = null) 
        {
            using var conexion = CrearConexion();

            var condiciones = new List<string> { "t.UsuarioId = @UsuarioId" };
            if (!string.IsNullOrEmpty(codigoCripto)) condiciones.Add("c.Code = @CodigoCripto");
            if (!string.IsNullOrEmpty(accion)) condiciones.Add("t.Accion = @Accion");
            if (fechaDesde.HasValue) condiciones.Add("t.FechaTransaccion >= @FechaDesde");
            if (fechaHasta.HasValue) condiciones.Add("t.FechaTransaccion <= @FechaHasta");

            var where = string.Join(" AND ", condiciones);

            var parametros = new DynamicParameters();
            parametros.Add("UsuarioId", usuarioId);
            parametros.Add("CodigoCripto", codigoCripto);
            parametros.Add("Accion", accion);
            parametros.Add("FechaDesde", fechaDesde);
            parametros.Add("FechaHasta", fechaHasta);
            parametros.Add("Offset", (pagina -1) * tamañoPagina);
            parametros.Add("TamañoPagina", tamañoPagina);

            var sqlTotal = $@"
                SELECT COUNT(*) FROM Transacciones t
                INNER JOIN Cryptos c ON t.CriptoMonedaId = c.Id WHERE {where}";
            var total = await conexion.ExecuteScalarAsync<int>(sqlTotal, parametros);

            var sql = $@"
            SELECT 
                t.Id,
                c.Code AS CodigoCripto,
                c.Nombre AS NombreCripto,
                c.Simbolo AS SimboloCripto,
                c.UrlIcono AS UrlIconoCripto,
                c.Color AS ColorCripto,
                t.Accion,
                t.CantidadCripto,
                t.Monto,
                t.TipoDeCambio,
                t.Exchange,
                t.FechaTransaccion,
                t.CreadoEn
            FROM Transacciones t
            INNER JOIN Cryptos c ON t.CriptoMonedaId = c.Id
            WHERE {where}
            ORDER BY t.FechaTransaccion DESC
            OFFSET @Offset ROWS FETCH NEXT @TamañoPagina ROWS ONLY";

            var items = await conexion.QueryAsync<TransaccionRespuestaDto>(sql, parametros);

            return new TransaccionesPaginadasDtos
            {
                Items = items,
                Total = total,
                Pagina = pagina,
                TamañoPagina = tamañoPagina
            };
        }

        // ── Obtener una transacción por Id ───────────────────
        public async Task<TransaccionRespuestaDto?> ObtenerPorIdAsync(int id, int usuarioId)
        {
            using var conexion = CrearConexion();

            const string sql = @"
            SELECT 
                t.Id,
                t.CriptoMonedaId,
                c.Code AS CodigoCripto,
                c.Nombre AS NombreCripto,
                c.Simbolo AS SimboloCripto,
                c.UrlIcono AS UrlIconoCripto,
                c.Color AS ColorCripto,
                t.Accion,
                t.CantidadCripto,
                t.Monto,
                t.TipoDeCambio,
                t.Exchange,
                t.FechaTransaccion,
                t.CreadoEn
            FROM Transacciones t
            INNER JOIN Cryptos c ON t.CriptoMonedaId = c.Id
            WHERE t.Id = @Id AND t.UsuarioId = @UsuarioId";

            return await conexion.QueryFirstOrDefaultAsync<TransaccionRespuestaDto>(sql, new { Id = id, UsuarioId = usuarioId });
        }

        // ── Calcular balance disponible de una cripto ────────
        // Suma compras y resta ventas para saber cuánto tiene el usuario.
        // Lo usamos para validar que no venda más de lo que tiene.
        public async Task<decimal> ObtenerBalanceAsync(int criptomonedaId, int usuarioId)
        {
            using var conexion = CrearConexion();

            const string sql = @"
            SELECT ISNULL(
                SUM(CASE
                    WHEN Accion = 'compra' THEN CantidadCripto
                    WHEN Accion = 'venta' THEN -CantidadCripto
                END), 0)
            FROM Transacciones 
            WHERE CriptoMonedaId = @CriptoMonedaId AND UsuarioId = @UsuarioId";

            return await conexion.ExecuteScalarAsync<decimal>(sql, new { CriptoMonedaId = criptomonedaId, UsuarioId = usuarioId });
        }

        // ── Crear una transacción ────────────────────────────
        // Devuelve el Id generado automáticamente por SQL Server
        public async Task<int> CrearAsync(Transaccion transaccion, int usuarioId)
        {
            using var conexion = CrearConexion();
            await conexion.OpenAsync();
            using var tx = await conexion.BeginTransactionAsync(
                System.Data.IsolationLevel.Serializable);

            try
            {
                if (transaccion.Accion == "venta")
                {
                    const string sqlSaldo = @"
                        SELECT ISNULL(SUM(CASE
                            WHEN Accion = 'compra' THEN CantidadCripto
                            WHEN Accion = 'venta' THEN -CantidadCripto
                        END),0)
                        FROM Transacciones WITH (UPDLOCK, HOLDLOCK)
                        WHERE CriptoMonedaId = @CriptoMonedaId
                        AND UsuarioId = @UsuarioId";

                    var saldo = await conexion.ExecuteScalarAsync<decimal>(
                        sqlSaldo, new { CriptoMonedaId = transaccion.CriptoMonedaId, UsuarioId = transaccion.UsuarioId }, tx);

                    if (transaccion.CantidadCripto > saldo)
                        throw new InvalidOperationException(
                            $"Saldo insuficiente: disponible {saldo}");
                }

                const string sqlInsert = @"
                    INSERT INTO Transacciones
                        (CriptoMonedaId, UsuarioId, Accion, CantidadCripto, Monto, TipoDeCambio, Exchange, FechaTransaccion)
                    VALUES
                        (@CriptoMonedaId, @UsuarioId, @Accion, @CantidadCripto, @Monto, @TipoDeCambio, @Exchange, @FechaTransaccion)
                    SELECT SCOPE_IDENTITY()";

                var id = await conexion.ExecuteScalarAsync<int>(sqlInsert, transaccion, tx);
                await tx.CommitAsync();
                return id;
            }
            catch
            {
                await tx.RollbackAsync();
                throw;
            }
        }

        // ── Editar una transacción ───────────────────────────
        // Construimos el UPDATE dinámicamente según qué
        // campos llegaron en el DTO — solo actualizamos lo que cambió
        public async Task<bool> EditarAsync(int id, int usuarioId, EditarTransaccionDto dto)
        {
            using var conexion = CrearConexion();

            var clausulas = new List<string> { "ActualizadoEn = GETUTCDATE()" };
            var parametros = new DynamicParameters();
            parametros.Add("Id", id);
            parametros.Add("UsuarioId", usuarioId);

            if (dto.CantidadCripto.HasValue)
            {
                clausulas.Add("CantidadCripto = @CantidadCripto");
                parametros.Add("CantidadCripto", dto.CantidadCripto.Value);
            }

            if (dto.Monto.HasValue)
            {
                clausulas.Add("Monto = @Monto");
                parametros.Add("Monto", dto.Monto.Value);
            }

            if (dto.Accion != null)
            {
                clausulas.Add("Accion = @Accion");
                parametros.Add("Accion", dto.Accion);
            }

            if (dto.FechaTransaccion.HasValue)
            {
                clausulas.Add("FechaTransaccion = @FechaTransaccion");
                parametros.Add("FechaTransaccion", dto.FechaTransaccion.Value);
            }

            var sql = $"UPDATE Transacciones SET {string.Join(",", clausulas)} WHERE Id = @Id AND UsuarioId = @UsuarioId";
            var filasAfectadas = await conexion.ExecuteAsync(sql, parametros);
            return filasAfectadas > 0;
        }

        // ── Eliminar una transacción ─────────────────────────
        public async Task<bool> EliminarAsync(int id, int usuarioId)
        {
            using var conexion = CrearConexion();
            var filasAfectadas = await conexion.ExecuteAsync(
                "DELETE FROM Transacciones WHERE Id = @Id AND UsuarioId = @UsuarioId", new { Id = id, UsuarioId = usuarioId });
            return filasAfectadas > 0;
        }

        // ── Resumen para el portfolio ────────────────────────
        // Agrupa por cripto y calcula el balance neto de cada una.
        // Solo devuelve las criptos donde el balance es mayor a 0
        // (las que vendiste todo no aparecen)
        public async Task<IEnumerable<dynamic>> ObtenerResumenPortfolioAsync(int usuarioId)
        {
            using var conexion = CrearConexion();
            const string sql = @"
            SELECT 
                c.Id AS CriptoMonedaId,
                c.Code AS CodigoCripto,
                c.Nombre AS NombreCripto,
                c.Simbolo AS SimboloCripto,
                c.UrlIcono,
                c.Color,
                SUM(CASE WHEN t.Accion = 'compra' THEN t.CantidadCripto
                         ELSE -t.CantidadCripto END) AS CantidadNeta,
                SUM(CASE WHEN t.Accion = 'compra' THEN t.Monto
                         ELSE -t.Monto END) AS MontoNeto
            FROM Transacciones t
            INNER JOIN Cryptos c ON t.CriptoMonedaId = c.Id
            WHERE t.UsuarioId = @UsuarioId
            GROUP BY c.Id, c.Code, c.Nombre, c.Simbolo, c.UrlIcono, c.Color
            HAVING SUM(CASE WHEN t.Accion = 'compra' THEN t.CantidadCripto
                            ELSE -t.CantidadCripto END) > 0
            ORDER BY CantidadNeta DESC";

            return await conexion.QueryAsync(sql, new { UsuarioId = usuarioId});
        }


        // ── Historial de valor del portfolio ────────────────
        // Trae todas las transacciones del usuario ordenadas
        // de más antigua a más nueva y calcula el valor
        // acumulado en cada punto para dibujar el gráfico.
        public async Task<IEnumerable<HistorialPortfolioDto>> ObtenerHistorialAsync(int usuarioId)
        {
            using var conexion = CrearConexion();

            const string sql = @"
                SELECT
                    t.FechaTransaccion,
                    t.Accion,
                    t.Monto,
                    c.Code AS CodigoCripto
                FROM Transacciones t
                INNER JOIN Cryptos c ON t.CriptoMonedaId = c.Id
                WHERE t.UsuarioId = @UsuarioId    
                ORDER BY t.FechaTransaccion ASC";

            var transacciones = (await conexion.QueryAsync(sql, new { UsuarioId = usuarioId }));

            // Si no hay transacciones devolvemos lista vacía
            // el frontend no va a mostrar el gráfico en ese caso
            if (!transacciones.Any())
                return Enumerable.Empty<HistorialPortfolioDto>();

            var puntos = new List<HistorialPortfolioDto>();
            decimal acumulado = 0;

            foreach (var t in transacciones)
            {
                // Compra suma al valor del portfolio
                // Venta resta (vendiste cripto, bajó tu inversión registrada)
                acumulado += t.Accion == "compra"
                    ? (decimal)t.Monto
                    : -(decimal)t.Monto;

                puntos.Add(new HistorialPortfolioDto
                {
                    Fecha = ((DateTime)t.FechaTransaccion).ToString("yyyy-MM-dd"),
                    ValorARS = acumulado
                });
            }

            return puntos
                .GroupBy(p => p.Fecha)
                .Select(g => g.Last())
                .OrderBy(p => p.Fecha)
                .ToList();
        }   
    }
}
