// Models/DTOs/PortfolioDtos.cs
// ============================================
// DTOs para la pantalla de análisis de cartera.
// Estos no mapean directamente a una tabla —
// son datos calculados que armamos en el backend.
// ============================================

namespace CryptoWallet.API.Models.DTOs
{
    public class PortFolioDto
    {
        // El portfolio completo que devolvemos a Vue.js
        public List<PortfolioItemDto> Tenencias { get; set; } = new();
        public decimal ValorTotalARS { get; set; }
        public decimal TotalInvertidoARS { get; set; }
        public decimal GananciaPerdida { get; set; }
        public decimal PorcentajeGananciaPerdida { get; set; }
    }

    // Una fila del portfolio — una cripto con su estado actual
    public class PortfolioItemDto
    {
        public string CodigoCripto {  get; set; } = string.Empty;
        public string NombreCripto { get; set; } = string.Empty;
        public string SimboloCripto { get; set; } = string.Empty;
        public string? UrlIcono { get; set; }
        public string Color { get; set; } = string.Empty;
        public decimal Cantidad { get; set; }   // Cuánta cripto tenemos
        public decimal PrecioActualARS { get; set; }    // Precio de CriptoYa ahora
        public decimal ValorActualARS { get; set; }    // Cantidad * PrecioActual 
        public decimal InvertidoARS { get; set; }   // Lo que pagamos en total
        public decimal GananciaPerdida { get; set; }    // ValorActual - Invertido
        public decimal PorcentajeGananciaPerdida { get; set; }
    }

    public class HistorialPortfolioDto
    {
        public string Fecha { get; set; } = "";
        public decimal ValorARS { get; set; }
    }
}
