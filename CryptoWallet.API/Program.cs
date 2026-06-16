// ============================================
// Program.cs — Punto de entrada de la aplicación
// Acá configuramos todos los servicios antes
// de que la app empiece a recibir peticiones
// ============================================
using System.Text;
using CryptoWallet.API.Repositories;
using CryptoWallet.API.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text.Json;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// ── Controladores ────────────────────────────────────────
// Le decimos a ASP.NET que vamos a usar Controllers
// y que el JSON use camelCase (estándar en JavaScript/Vue)
builder.Services.AddControllers()
    .AddJsonOptions(opciones => {
        opciones.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
    });

// ── Swagger ───────────────────────────────────────────────
// Interfaz visual para probar la API desde el browser
builder.Services.AddEndpointsApiExplorer();

// Swagger con soporte para JWT — podés probar endpoints protegidos
builder.Services.AddSwaggerGen(c => {
    c.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Description = "Ingresá el token JWT"
    });
    c.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
    {
        {
            new Microsoft.OpenApi.Models.OpenApiSecurityScheme {
                Reference = new Microsoft.OpenApi.Models.OpenApiReference {
                    Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

// ── CORS ──────────────────────────────────────────────────
// Permite que Vue.js (puerto 5173) se comunique con la API
// Sin esto el browser bloquea las peticiones del frontend
builder.Services.AddCors(opciones => {
    opciones.AddPolicy("FrontendVue", politica => {
        politica.WithOrigins("http://localhost:5173", "https://localhost:5173")
        .AllowAnyHeader()
        .AllowAnyMethod();
    });
});

// ── JWT Authentication ────────────────────────────────────
var claveJwt = builder.Configuration["Jwt:Clave"]!;
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(opciones => {
        opciones.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Emisor"],
            ValidAudience = builder.Configuration["Jwt:Audiencia"],
            IssuerSigningKey = new SymmetricSecurityKey(
                                   Encoding.UTF8.GetBytes(claveJwt))
        };
    });

builder.Services.AddAuthorization();

// ── Repositories ─────────────────────────────────────────
// AddScoped = se crea una instancia por cada petición HTTP
// y se destruye cuando la petición termina
builder.Services.AddScoped<CriptomonedaRepository>();
builder.Services.AddScoped<TransaccionesRepository>();
builder.Services.AddScoped<UsuarioRepository>();

// ── Services ─────────────────────────────────────────────
builder.Services.AddScoped<TransaccionService>();
builder.Services.AddScoped<PortfolioService>();
builder.Services.AddScoped<AuthService>();

// ── HttpClient para CryptoYa ──────────────────────────────
// AddHttpClient maneja el ciclo de vida del HttpClient
// de forma eficiente — nunca uses "new HttpClient()" directamente
builder.Services.AddHttpClient<CryptoYaService>(cliente => {
    cliente.Timeout = TimeSpan.FromSeconds(5);
});

var app = builder.Build();

// ── Middleware ────────────────────────────────────────────
// El orden acá importa — cada petición pasa por estos
// filtros en el orden en que están escritos
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors("FrontendVue"); // CORS antes de los controladores
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();
