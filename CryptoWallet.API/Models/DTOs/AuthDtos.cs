using System.ComponentModel.DataAnnotations;

namespace CryptoWallet.API.Models.DTOs
{
    public class RegistroDto
    {
        [Required(ErrorMessage = "El nombre es obligatorio")]
        [MinLength(3, ErrorMessage = "El nombre debe tener al menos 3 caracteres")]
        public string Nombre { get; set; } = string.Empty;

        [Required(ErrorMessage = "El email es obligatorio")]
        [EmailAddress(ErrorMessage = "El email no es valido")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "La contraseña es obligatoria")]
        [MinLength(8, ErrorMessage = "La contraseña debe tener al menos 8 caracteres")]
        public string Password { get; set; } = string.Empty;
    }

    public class LoginDto
    {
        [Required(ErrorMessage = "El email es obligatorio")]
        [EmailAddress(ErrorMessage = "El email no es valido")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "La contraseña es obligatoria")]
        public string Password { get; set; } = string.Empty;
    }

    public class AuthRespuestaDto
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Token { get; set; } = string.Empty;
    }

    public class MetodoPagoDto
    {
        [Required(ErrorMessage = "El tipo es obligatorio")]
        [RegularExpression("^(debito|credito|transferencia)$",
            ErrorMessage = "Tipo inválido")]
        public string Tipo { get; set; } = string.Empty;

        // Solo para tarjetas
        public string? UltimosDigitos { get; set; }

        // Solo para transferencia
        public string? Alias { get; set; }
        public string? CBU { get; set; }
        public string? Banco { get; set; }
        public bool EsPrincipal { get; set; }
    }

    public class MetodoPagoRespuestaDto
    {
        public int Id { get; set; }
        public string Tipo { get; set; } = string.Empty;
        public string? UltimosDigitos { get; set; }
        public string? Alias { get; set; }
        public string? Cbu { get; set; }
        public string? Banco { get; set; }
        public bool EsPrincipal { get; set; }
        public DateTime CreadoEn { get; set; }
    } 
}
