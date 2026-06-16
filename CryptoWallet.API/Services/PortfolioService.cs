// Services/PortfolioService.cs
// ============================================
// Calcula el estado actual de la cartera
// combinando datos de la DB con precios
// en tiempo real de CriptoYa.
// ============================================
using CryptoWallet.API.Models.DTOs;
using CryptoWallet.API.Repositories;
using Microsoft.Data.SqlClient;

namespace CryptoWallet.API.Services
{
    public class PortfolioService
    {
        private readonly TransaccionesRepository _transaccionRepo;
        private readonly CryptoYaService _cryptoYa;

        public PortfolioService(TransaccionesRepository transaccionRepo, CryptoYaService cryptoYa)
        {
            _transaccionRepo = transaccionRepo;
            _cryptoYa = cryptoYa;
        }

        public async Task<PortFolioDto> ObtenerPortfolioActualAsync(int usuarioId)
        {
            var resumen = (await _transaccionRepo.ObtenerResumenPortfolioAsync(usuarioId)).ToList();

            var tareasPrecio = resumen.Select(async item =>
            {
                decimal precioActual;
                try
                {
                    precioActual = await _cryptoYa.ObtenerPrecioAsync((string)item.CodigoCripto);
                }
                catch
                {
                    precioActual = 0;
                }
                return (item, precioActual);
            });

            var resultados = await Task.WhenAll(tareasPrecio);

            var tenencias = resultados.Select(r =>
            {
                var (item, precioActual) = r;
                decimal cantidad = (decimal)item.CantidadNeta;
                decimal invertido = (decimal)item.MontoNeto;
                decimal valorActual = cantidad * precioActual;
                decimal ganPerdida = valorActual - invertido;
                decimal porcentaje = invertido > 0 ? (ganPerdida / invertido) * 100 : 0;

                return new PortfolioItemDto
                {
                    CodigoCripto = item.CodigoCripto,
                    NombreCripto = item.NombreCripto,
                    SimboloCripto = item.SimboloCripto,
                    UrlIcono = item.UrlIcono,
                    Color = item.Color,
                    Cantidad = cantidad,
                    PrecioActualARS = precioActual,
                    ValorActualARS = valorActual,
                    InvertidoARS = invertido,
                    GananciaPerdida = ganPerdida,
                    PorcentajeGananciaPerdida = porcentaje
                };
            }).ToList();

            var valorTotal = tenencias.Sum(t => t.ValorActualARS);
            var totalInvertido = tenencias.Sum(t => t.InvertidoARS);
            var ganPerdidaTotal = valorTotal - totalInvertido;
            var porcentajeTotal = totalInvertido > 0 ? (ganPerdidaTotal / totalInvertido) * 100 : 0;

            return new PortFolioDto
            {
                Tenencias = tenencias,
                ValorTotalARS = valorTotal,
                TotalInvertidoARS = totalInvertido,
                GananciaPerdida = ganPerdidaTotal,
                PorcentajeGananciaPerdida = porcentajeTotal
            };
        }

        public async Task<IEnumerable<HistorialPortfolioDto>> ObtenerHistorialAsync(int usuarioId)
        {
            // El service no toca la DB directamente.
            // Le pide el dato al repositorio que es quien sabe hablar con SQL.
            return await _transaccionRepo.ObtenerHistorialAsync(usuarioId);
        }
    }
}
