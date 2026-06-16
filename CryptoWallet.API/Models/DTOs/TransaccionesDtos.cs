// Models/DTOs/TransaccionDtos.cs
// ============================================
// DTOs — definen qué datos entran y salen de la API.
// Separados del Model para no exponer datos internos
// y para validar exactamente lo que necesitamos.
// ============================================
using System.ComponentModel.DataAnnotations;

namespace CryptoWallet.API.Models.DTOs
{
    // ── Lo que recibimos de Vue.js al CREAR ──────────────
    // Solo los datos que el usuario puede ingresar.
    // Monto y TipoDeCambio NO vienen — los calcula el backend.
    public class CrearTransaccionDto
    {
        [Required(ErrorMessage = "El codigo de criptomoneda es obligatorio")]
        public string CodigoCripto { get; set; } = string.Empty;

        [Required(ErrorMessage = "La accion es obligatoria")]
        [RegularExpression("^(compra|venta)$",
            ErrorMessage = "La accion debe ser 'compra' o 'venta'")]
        public string Accion { get; set; } = string.Empty;

        [Required(ErrorMessage = "La cantidad es obligatoria")]
        [Range(0.00000001, double.MaxValue,
            ErrorMessage = "La cantidad debe ser mayor a 0")]
        public decimal CantidadCripto { get; set; }

        [Required(ErrorMessage = "La fecha es obligatoria")]
        public DateTime FechaTransaccion { get; set; }
    }

    // ── Lo que recibimos de Vue.js al EDITAR ─────────────
    // Todo opcional — solo actualizamos lo que llega.
    // El ? en cada tipo significa que es nullable (opcional).

    public class EditarTransaccionDto
    {
        public decimal? CantidadCripto { get; set; }
        public decimal? Monto { get; set; }
        public string? Accion { get; set; }
        public DateTime? FechaTransaccion { get; set; }
    }

    // ── Lo que devolvemos a Vue.js ────────────────────────
    // Incluye datos de la cripto (del JOIN con Criptomonedas)
    // que el Model solo no tiene.

    public class TransaccionRespuestaDto
    {
        public int Id { get; set; }
        public int CriptoMonedaId { get; set; }
        public string CodigoCripto { get; set; } = string.Empty;
        public string NombreCripto { get; set; } = string.Empty;
        public string SimboloCripto { get; set; } = string.Empty;
        public string? UrlIconoCripto { get; set; }
        public string ColorCripto { get; set; } = string.Empty;
        public string Accion { get; set; } = string.Empty;
        public decimal CantidadCripto { get; set; }
        public decimal Monto { get; set; }
        public decimal TipoDeCambio { get; set; }
        public string Exchange { get; set; } = string.Empty;
        public DateTime FechaTransaccion { get; set; }
        public DateTime CreadoEn { get; set; }
    }

    // DTO para previsualizar el impacto antes de confirmar la edición

    public class PreviewEdicionDto
    {
        public decimal CantidadAnterior { get; set; }
        public decimal CantidadNueva { get; set; }
        public decimal MontoAnterior { get; set; }
        public decimal MontoNuevo { get; set; }
        public decimal PrecioActual { get; set; }
        public string Diferencia { get; set; } = string.Empty;  // "aumenta" o "disminuye"
    }

    // DTO simplificado para edición — sin Monto manual

    public class EditartransaccionesSimpleDto
    {
        [Range(0.00000001, double.MaxValue,
            ErrorMessage = "La cantidad debe ser mayor a 0")]
        public decimal? CantidadCripto { set; get; }
        public DateTime? FechaTransaccion { get; set; }
    }

    public class TransaccionesPaginadasDtos
    {
        public IEnumerable<TransaccionRespuestaDto> Items { get; set; } = new List<TransaccionRespuestaDto>();
        public int Total { get; set; }  
        public int Pagina { get; set; }
        public int TamañoPagina { get; set; }
        public int TotalPaginas => (int)Math.Ceiling((double)Total / TamañoPagina);
    }
}
