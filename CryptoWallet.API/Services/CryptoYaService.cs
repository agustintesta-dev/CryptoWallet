// Services/CryptoYaService.cs
// ============================================
// Consulta precios en tiempo real desde CriptoYa.
// Toda la comunicación con la API externa
// está encapsulada acá — el resto de la app
// no sabe nada de cómo funciona CriptoYa.
// ============================================

namespace CryptoWallet.API.Services
{
    public class CryptoYaService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<CryptoYaService> _logger;
        private const string UrlBase = "https://criptoya.com/api";
        private const string ExchangePorDefecto = "satoshitango";
        
        public CryptoYaService(HttpClient httpClient, ILogger<CryptoYaService> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
        }

        // ── Obtener precio actual de una cripto ───────────────
        // Devuelve el precio en ARS desde el exchange indicado.
        // Usamos "totalAsk" porque es el precio real que pagás
        // al comprar (incluye los fees del exchange).
        public async Task<decimal> ObtenerPrecioAsync(
            string codigoCripto,
            string exchange = ExchangePorDefecto)
        {
            try
            {
                var url = $"{UrlBase}/{exchange}/{codigoCripto}/ars";
                var respuesta = await _httpClient.GetFromJsonAsync<RespuestaCryptoYa>(url);

                if (respuesta == null)
                    throw new Exception($"No se pudo obtener el precio de {codigoCripto}");

                return respuesta.TotalAsk;
            }
            catch (TaskCanceledException)
            {
                _logger.LogError("Error al consultar CriptoYa para {Codigo}", codigoCripto);
                throw new Exception(
                    $"No se pudo obtener el precio de {codigoCripto.ToUpper()}. " + 
                    $"Intenta de nuevo en unos segundos.");
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error al consultar CriptoYa pa {codigo}", codigoCripto);
                throw new Exception(
                    $"No se pudo obtener el precio de {codigoCripto.ToUpper()}." +
                    $"Intentá de nuevo en unos segundos.", ex);
            }
        }

        // Mapea el JSON que devuelve CriptoYa
        // {
        //   "ask": 5912442.48,
        //   "totalAsk": 5971566.90,   ← precio real de compra con fees
        //   "bid": 5797256.86,
        //   "totalBid": 5739284.29,   ← precio real de venta con fees
        //   "time": 1626027655
        // }
        public class RespuestaCryptoYa
        {
            public decimal Ask { get; set; }
            public decimal TotalAsk { get; set; }
            public decimal Bid { get; set; }
            public decimal TotalBid { get; set; }
            public long Time {  get; set; }
        }
    }
}
