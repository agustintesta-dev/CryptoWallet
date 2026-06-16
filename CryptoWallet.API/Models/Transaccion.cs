// Models/Transaccion.cs
// ============================================
// Representa exactamente la tabla Transacciones.
// CriptomonedaId es la clave foránea que conecta
// con la tabla Criptomonedas — igual que en la DB.
// ============================================

namespace CryptoWallet.API.Models
{
    public class Transaccion
    {
        public int Id { get; set; }

        public int CriptoMonedaId { get; set; }

        public int UsuarioId { get; set; }

        public string Accion {  get; set; } = string.Empty;     // 'compra' o 'venta'

        public decimal CantidadCripto { get; set; }

        public decimal Monto { get; set; }      // Calculado desde CriptoYa

        public decimal TipoDeCambio { get; set; }   // Precio unitario al momento

        public string Exchange { get; set; } = "satoshitango";

        public DateTime FechaTransaccion {  get; set; }

        public DateTime CreadoEn {  get; set; }

        public DateTime ActualizadoEn {  get; set; }

        // Propiedad de navegación — no está en la DB,
        // se completa cuando hacemos JOIN con Criptomonedas

    }
}
