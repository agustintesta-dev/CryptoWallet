// Models/Crypto.cs
// ============================================
// Representa exactamente la tabla Criptomonedas
// de SQL Server. Cada propiedad = una columna.
// ============================================

namespace CryptoWallet.API.Models
{
    public class Crypto
    {
        public int Id { get; set; }

        public string Code { get; set; } = string.Empty;

        public string Nombre {  get; set;  } = string.Empty;

        public string Simbolo {  get; set; } = string.Empty;

        public string? UrlIcono {  get; set; }

        public string Color { get; set; } = "#6c63ff";

        public bool EstaActivo { get; set; }

        public DateTime CreadoEn {  get; set; }
    }
}
