# 🪙 CryptoWallet

Aplicación web para gestionar tu cartera de criptomonedas en pesos argentinos (ARS). Registrá tus compras y ventas, y seguí el valor de tu portfolio en tiempo real con precios obtenidos directamente de los exchanges locales.

---

## 📋 Requisitos previos

Antes de comenzar, asegurate de tener instalado:

| Herramienta | Versión mínima | Descarga |
|-------------|---------------|---------|
| Node.js | 20.19.0 o superior | https://nodejs.org |
| .NET SDK | 10.0 | https://dotnet.microsoft.com/download |
| SQL Server | Express o superior | https://www.microsoft.com/sql-server |
| Git | Cualquiera | https://git-scm.com |

---

## 🗄️ 1. Restaurar la base de datos

Abrí **SQL Server Management Studio (SSMS)** o cualquier cliente de SQL Server y ejecutá el script incluido en la raíz del proyecto.

```sql
-- Desde SSMS: Archivo → Abrir → CryptoWallet.sql
-- O desde la terminal:
sqlcmd -S .\SQLEXPRESS -E -i CryptoWallet.sql
```

El script crea automáticamente:
- La base de datos `CryptoWallet`
- Las tablas `Usuarios`, `Cryptos`, `Transacciones` y `MetodosPago`
- Los índices necesarios para las consultas más frecuentes
- **20 criptomonedas** precargadas con sus íconos y colores (Bitcoin, Ethereum, Solana, etc.)

Una vez ejecutado, verificá que las tablas se crearon correctamente:

```sql
USE CryptoWallet;
SELECT Id, Code, Simbolo, Nombre FROM Cryptos ORDER BY Id;
```

Deberías ver las 20 criptos listadas.

---

## ⚙️ 2. Configurar y ejecutar el Backend

### 2.1 Configurar la cadena de conexión

Abrí el archivo `CryptoWallet/CryptoWallet.API/appsettings.json` y ajustá la cadena de conexión según tu instancia de SQL Server:

```json
{
  "ConnectionStrings": {
    "Conexion": "Server=TU_SERVIDOR\\SQLEXPRESS;Database=CryptoWallet;Trusted_Connection=True;TrustServerCertificate=True"
  },
  "Jwt": {
    "Clave": "CryptoWallet2026ClaveSecretaSegura123",
    "Emisor": "CryptoWalletAPI",
    "Audiencia": "CryptoWalletFrontend",
    "ExpiracionHoras": 24
  }
}
```

> **Nota:** Reemplazá `TU_SERVIDOR` con el nombre de tu instancia. Si usás SQL Server Express local, generalmente es `.\SQLEXPRESS` o `localhost\SQLEXPRESS`.

### 2.2 Restaurar paquetes NuGet y ejecutar

```bash
# Navegá a la carpeta del backend
cd CryptoWallet/CryptoWallet.API

# Restaurar dependencias
dotnet restore

# Ejecutar en modo desarrollo
dotnet run
```

La API quedará disponible en:

```
https://localhost:7004
http://localhost:5004
```

Podés verificar que funciona abriendo Swagger en el browser:

```
https://localhost:7004/swagger
```

Desde ahí podés probar todos los endpoints directamente, incluyendo los protegidos con JWT.

---

## 🖥️ 3. Configurar y ejecutar el Frontend

### 3.1 Instalar dependencias

```bash
# Navegá a la carpeta del frontend
cd CryptoWallet/crypto-wallet-frontend

# Instalar paquetes de Node
npm install
```

### 3.2 Verificar la URL del backend

Abrí `src/services/api.js` y confirmá que la `baseURL` apunta a tu backend:

```javascript
const api = axios.create({
  baseURL: 'https://localhost:7004/api',  // ← debe coincidir con el puerto del backend
  timeout: 10000,
})
```

### 3.3 Ejecutar en modo desarrollo

```bash
npm run dev
```

La aplicación quedará disponible en:

```
http://localhost:5173
```

Abrí esa URL en el browser y ya podés registrarte y empezar a usar CryptoWallet.

---

## 🚀 Orden de inicio recomendado

Para que todo funcione correctamente, iniciá los servicios en este orden:

```
1️⃣  SQL Server   →   debe estar corriendo antes de iniciar el backend
2️⃣  Backend      →   dotnet run (esperar a que aparezca "Now listening on...")
3️⃣  Frontend     →   npm run dev
```

---

## 🛠️ Tecnologías utilizadas

### Frontend

| Tecnología | Versión | Rol |
|-----------|---------|-----|
| Vue.js | 3.5.32 | Framework principal de UI |
| Vite | 8.0.8 | Build tool y servidor de desarrollo |
| Pinia | 3.0.4 | Manejo de estado global |
| Vue Router | 5.0.4 | Navegación entre vistas |
| Axios | 1.17.0 | Cliente HTTP hacia la API |
| Chart.js | 4.5.1 | Gráfico de evolución del portfolio |
| Lucide Vue Next | 1.0.0 | Íconos |

### Backend

| Tecnología | Versión | Rol |
|-----------|---------|-----|
| ASP.NET Core | .NET 10.0 | Framework de la API REST |
| C# | 13 | Lenguaje de programación |
| Dapper | 2.1.79 | Micro-ORM para acceso a datos |
| JWT Bearer | 10.0.9 | Autenticación con tokens |
| BCrypt.Net-Next | 4.2.0 | Hash seguro de contraseñas |
| Swashbuckle | 6.9.0 | Documentación Swagger de la API |
| Microsoft.Data.SqlClient | 7.0.1 | Driver de conexión a SQL Server |

### Base de datos y servicios externos

| Tecnología | Rol |
|-----------|-----|
| SQL Server Express | Base de datos relacional |
| CriptoYa API (`criptoya.com/api`) | Precios de criptomonedas en ARS en tiempo real |

---

## ✅ Funcionalidades implementadas

### 🔐 Autenticación
- Registro de cuenta con validación de email único y contraseña mínima de 8 caracteres.
- Login con email y contraseña; devuelve un JWT con validez de 24 horas.
- Protección automática de rutas: si no hay sesión activa, redirige al login.
- Logout con limpieza de sesión local.
- Si el token expira, la app detecta el error 401 y redirige al login automáticamente.

### 💸 Transacciones
- Registrar compras y ventas de criptomonedas: el precio en ARS se obtiene en tiempo real desde CriptoYa, no se ingresa manualmente.
- Validación de saldo: no podés vender más cripto de la que tenés registrada.
- Historial paginado (20 por página) con filtros por criptomoneda, tipo de operación (compra/venta) y rango de fechas.
- Edición de transacciones con preview del impacto: antes de confirmar el cambio, la app muestra cuánto varía el monto con el precio actual del mercado.
- Eliminación de transacciones.

### 📊 Portfolio
- Resumen de cartera: valor total en ARS, total invertido, ganancia o pérdida global y porcentaje de rendimiento.
- Detalle por criptomoneda: cantidad neta acumulada, precio actual de mercado, valor en ARS y resultado individual.
- Los colores indican el estado: verde para ganancia, rojo para pérdida.
- Gráfico de líneas con la evolución histórica del valor del portfolio.

### 👤 Perfil de usuario
- Visualización de datos personales (nombre y email).
- Gestión de métodos de pago: podés agregar hasta 3 entre tarjeta de débito, crédito o datos bancarios para transferencia.
- Para tarjetas se guardan solo los últimos 4 dígitos; nunca el número completo.
- Eliminación de métodos de pago.

---

## 📁 Estructura del proyecto

```
CryptoWallet/
├── crypto-wallet-frontend/     # Aplicación Vue.js
│   ├── src/
│   │   ├── views/              # Pantallas de la app
│   │   ├── components/         # Componentes reutilizables
│   │   ├── stores/             # Estado global con Pinia
│   │   ├── services/           # Configuración de Axios
│   │   └── router/             # Rutas y guards de autenticación
│   └── package.json
│
├── CryptoWallet.API/           # Backend ASP.NET Core
│   ├── Controllers/            # Endpoints REST
│   ├── Services/               # Lógica de negocio
│   ├── Repositories/           # Acceso a base de datos
│   ├── Models/                 # Entidades y DTOs
│   └── appsettings.json        # Configuración (conexión + JWT)
│
└── CryptoWallet.sql            # Script de creación de la base de datos
```

---

## 🔗 URLs del proyecto en desarrollo

| Servicio | URL |
|---------|-----|
| Frontend (Vue.js) | http://localhost:5173 |
| Backend (API REST) | https://localhost:7004 |
| Swagger UI | https://localhost:7004/swagger |
| CriptoYa API | https://criptoya.com/api |
