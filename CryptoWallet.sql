CREATE DATABASE CryptoWallet;
GO

USE CryptoWallet;
GO

-- ------------------------------------
-- TABLA: Cryptos
-- Catalogo de criptomonedas disponibles.
-- "Code" es el codigo que usa la API de CriptoYa
-- (Debe coincidir exactamente para que las consultas funcionen)
-- ------------------------------------

CREATE TABLE Cryptos (
Id INT PRIMARY KEY IDENTITY(1,1),
Code NVARCHAR(20) NOT NULL UNIQUE, --'btc','eth','usdc'
Nombre NVARCHAR(100) NOT NULL, --'Bitcoin','Ethereum'
Simbolo NVARCHAR(10) NOT NULL, --'BTC','ETH'
UrlIcono NVARCHAR(300) NULL,
Color NVARCHAR(7) NOT NULL DEFAULT '#6c63ff',
EstaActivo BIT NOT NULL DEFAULT 1,
CreadoEn DATETIME2 NOT NULL DEFAULT GETUTCDATE() 
)

-- ------------------------------------
-- TABLA: Transacciones
-- Cada Fila representa una compra o venta
-- Monto y TipoDeCambio los calcula el backend
--consultando CriptoYa, nunca los ingresa el usuario
-- ------------------------------------

CREATE TABLE Transacciones (
Id INT PRIMARY KEY IDENTITY(1,1),
CriptoMonedaId INT NOT NULL REFERENCES Cryptos(Id),
Accion NVARCHAR (10) NOT NULL CHECK (Accion IN ('compra', 'venta')),
CantidadCripto DECIMAL (18, 8) NOT NULL CHECK (CantidadCripto > 0),
Monto DECIMAL (18, 2) NOT NULL,
TipoDeCambio DECIMAL (18, 2) NOT NULL,
Exchange NVARCHAR (50) NOT NULL DEFAULT 'satoshitango',
FechaTransaccion DATETIME2 NOT NULL,
CreadoEn DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
ActualizadoEn DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
)

-- ── Tabla de usuarios ─────────────────────────────────────

CREATE TABLE Usuarios (
Id INT PRIMARY KEY IDENTITY(1,1),
Nombre NVARCHAR(100) NOT NULL,
Email NVARCHAR(200) NOT NULL,
PasswordHash NVARCHAR(500) NOT NULL, -- nunca guardamos la contraseña en texto
CreadoEn DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
ActualizadoEn DATETIME2 NOT NULL DEFAULT GETUTCDATE()
)

-- ── Tabla de métodos de pago ──────────────────────────────
-- Máximo 3 por usuario — lo validamos en el backend

CREATE TABLE MetodosPago(
Id INT PRIMARY KEY IDENTITY(1,1),
UsuarioId INT NOT NULL REFERENCES Usuarios(Id) ON DELETE CASCADE,
Tipo NVARCHAR(20) NOT NULL CHECK (Tipo IN ('debito', 'credito', 'transferencia')),
-- Para tarjetas: guardamos solo los últimos 4 dígitos — nunca el número completo
UltimosDigitos NVARCHAR(4) NULL,
-- Para transferencia: alias o CBU
Alias NVARCHAR(100) NULL,
CBU NVARCHAR(22) NULL,
Banco NVARCHAR(100) NULL,
EsPrincipal BIT NOT NULL DEFAULT 0,
CreadoEn DATETIME2 NOT NULL DEFAULT GETUTCDATE()
)

-- ------------------------------------
-- INDICES
-- Aceleran las consultas mas frecuentes 
-- ------------------------------------

CREATE INDEX IX_Transacciones_CriptoMonedaId ON Transacciones(CriptoMonedaId)
CREATE INDEX IX_Transacciones_Accion ON Transacciones(Accion)
CREATE INDEX IX_Transacciones_Fecha ON Transacciones(FechaTransaccion DESC)
CREATE INDEX IX_MetodosPago_UsuariosId ON MetodosPago(UsuarioId)
CREATE INDEX IX_Transacciones_UsuarioId ON Transacciones(UsuarioId);
GO

-- ------------------------------------
-- DATOS INICIALES - 20 Criptomonedas reales 
-- Codigos verificados contra la API de CriptoYa
-- ------------------------------------

INSERT INTO Cryptos (Code, Nombre, Simbolo, UrlIcono, Color) VALUES

-- 5 grandes por capitalizacion de mercado
('btc', 'Bitcoin', 'BTC', 'https://cryptologos.cc/logos/bitcoin-btc-logo.png', '#F7931A'),
('eth', 'Ethereum', 'ETH', 'https://cryptologos.cc/logos/ethereum-eth-logo.png', '#627EEA'),
('usdt', 'Tether', 'USDT', 'https://cryptologos.cc/logos/tether-usdt-logo.png', '#26A17B'),
('usdc', 'Dolar Digital', 'USDC', 'https://cryptologos.cc/logos/usd-coin-usdc-logo.png', '#2775CA'),
('bnb', 'BNB', 'BNB', 'https://cryptologos.cc/logos/bnb-bnb-logo.png', '#F3BA2F'),

-- Altcoins populares
('sol', 'Solana', 'SOL', 'https://cryptologos.cc/logos/solana-sol-logo.png', '#9945FF'),
('ada', 'Cardano', 'ADA', 'https://cryptologos.cc/logos/cardano-ada-logo.png', '#0033AD'),
('xrp', 'XRP', 'XRP', 'https://cryptologos.cc/logos/xrp-xrp-logo.png', '#00AAE4'),
('dot', 'Polkadot', 'DOT', 'https://cryptologos.cc/logos/polkadot-new-dot-logo.png', '#E6007A'),
('avax', 'Avalanche', 'AVAX', 'https://cryptologos.cc/logos/avalanche-avax-logo.png', '#E84142'),

-- DeFi y ecosistema
('link', 'Chainlink', 'LINK', 'https://cryptologos.cc/logos/chainlink-link-logo.png', '#2A5ADA'),
('matic', 'Polygon', 'MATIC', 'https://cryptologos.cc/logos/polygon-matic-logo.png', '#8247E5'),
('uni', 'Uniswap', 'UNI', 'https://cryptologos.cc/logos/uniswap-uni-logo.png', '#FF007A'),
('atom', 'Cosmos', 'ATOM', 'https://cryptologos.cc/logos/cosmos-atom-logo.png', '#6F7390'),
('near', 'Protocolo NEAR', 'NEAR', 'https://cryptologos.cc/logos/near-protocol-near-logo.png', '#00C08B'),

-- Otras conocidas
('ltc', 'Litecoin', 'LTC', 'https://cryptologos.cc/logos/litecoin-ltc-logo.png', '#BFBBBB'),
('xlm', 'Stellar', 'XLM', 'https://cryptologos.cc/logos/stellar-xlm-logo.png', '#7D00FF'),
('algo', 'Algorand', 'ALGO', 'https://cryptologos.cc/logos/algorand-algo-logo.png', '#000000'),
('sandbox', 'The Sandbox', 'SAND', 'https://cryptologos.cc/logos/the-sandbox-sand-logo.png', '#00ADEF'),
('dai', 'Dai', 'DAI', 'https://cryptologos.cc/logos/multi-collateral-dai-dai-logo.png', '#F5AC37')

-- ────────────────────────────────────────────
-- VERIFICACIÓN
-- Ejecutar para confirmar que todo quedó bien
-- ────────────────────────────────────────────

SELECT Id, Code, Simbolo, Nombre, Color, EstaActivo
FROM Cryptos
ORDER BY Id

UPDATE Cryptos SET Color = '#808080' WHERE Code = 'algo'

EXEC sp_rename 'Usuarios.CreadoEN', 'CreadoEn', 'COLUMN';
SELECT COLUMN_NAME
FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_NAME = 'Usuarios';

ALTER TABLE Transacciones
ADD UsuarioId INT NULL REFERENCES Usuarios(Id);

UPDATE Transacciones SET UsuarioId = 1 WHERE UsuarioId IS NULL;

ALTER TABLE Transacciones
ALTER COLUMN UsuarioId INT NOT NULL;

SELECT 
	t.Id,
	t.UsuarioId,
	u.Nombre AS NombreUsuario,
	t.Accion,
	t.CantidadCripto
FROM Transacciones t
INNER JOIN Usuarios u ON t.UsuarioId = u.Id
ORDER BY t.Id;