namespace CryptoWallet.API.Models
{
    public class Usuario
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string Email {  get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty;
        public DateTime CreadoEn { get; set; }
        public DateTime ActualizadoEn { get; set; }
    }

    public class MetodoPago
    {
        public int Id { get; set; }
        public int UsuarioId { get; set; }
        public string Tipo { get; set; } = string.Empty;
        public string? UltimosDigitos { get; set; }
        public string? Alias { get; set; }
        public string? CBU { get; set; }
        public string? Banco { get; set; }
        public bool EsPrincipal {  get; set; }
        public DateTime CreadoEn { get; set; }
    } 
}
