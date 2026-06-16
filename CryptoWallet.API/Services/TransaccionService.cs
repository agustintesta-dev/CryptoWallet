// Services/TransaccionService.cs
// ============================================
// Contiene toda la lógica de negocio.
// No sabe nada de HTTP ni de SQL —
// solo aplica las reglas de la aplicación.
// ============================================
using CryptoWallet.API.Models;
using CryptoWallet.API.Models.DTOs;
using CryptoWallet.API.Repositories;

namespace CryptoWallet.API.Services
{
    public class TransaccionService
    {
        private readonly TransaccionesRepository _transaccionRepo;
        private readonly CriptomonedaRepository _criptoRepo;
        private readonly CryptoYaService _cryptoYa;

        public TransaccionService(
        TransaccionesRepository transaccionRepo,
        CriptomonedaRepository criptoRepo,
        CryptoYaService cryptoYa)
        {
            _transaccionRepo = transaccionRepo;
            _criptoRepo = criptoRepo;
            _cryptoYa = cryptoYa;
        }

        public async Task<TransaccionRespuestaDto> CrearAsync(CrearTransaccionDto dto, int usuarioId)
        {
            // ── Paso 1: Verificar que la cripto existe ────────
            var cripto = await _criptoRepo.ObtenerPorCodigoAsync(dto.CodigoCripto.ToLower());
            if (cripto == null)
                throw new InvalidOperationException(
                    $"La criptomoneda '{dto.CodigoCripto}' no existe en el sistema.");

            // ── Paso 2: Validar saldo si es una venta ─────────
            // Esta es la regla de negocio más importante —
            // no podés vender lo que no tenés.
            if (dto.Accion == "venta")
            {
                var saldoDisponible = await _transaccionRepo.ObtenerBalanceAsync(cripto.Id, usuarioId);
                if (dto.CantidadCripto > saldoDisponible)
                    throw new InvalidOperationException(
                        $"Saldo insuficiente. Tenes {saldoDisponible} {cripto.Simbolo} " +
                        $"y estas intentando vender {dto.CantidadCripto}.");
            }

            // ── Paso 3: Consultar precio actual a CriptoYa ────
            var precioActual = await _cryptoYa.ObtenerPrecioAsync(dto.CodigoCripto.ToLower());

            // ── Paso 4: Calcular monto total en ARS ───────────
            var montoTotal = dto.CantidadCripto * precioActual;

            // ── Paso 5: Armar la entidad y guardar ────────────
            var transaccion = new Transaccion
            {
                CriptoMonedaId = cripto.Id,
                UsuarioId = usuarioId,
                Accion = dto.Accion,
                CantidadCripto = dto.CantidadCripto,
                Monto = montoTotal,
                TipoDeCambio = precioActual,
                Exchange = "satoshitango",
                FechaTransaccion = dto.FechaTransaccion
            };

            var nuevoId = await _transaccionRepo.CrearAsync(transaccion, usuarioId);

            // ── Paso 6: Devolver la transacción completa ──────
            // Buscamos por Id para traer el JOIN con la cripto
            return await _transaccionRepo.ObtenerPorIdAsync(nuevoId, usuarioId)
                ?? throw new Exception("Error al recuperar la transaccion creada.");
        }

        // ── Previsualizar impacto de edición ──────────────────
        // Se llama ANTES de confirmar — muestra al usuario
        // cuánto va a cambiar el monto con la nueva cantidad
        public async Task<PreviewEdicionDto> PreviewEdicionAsync(int id, int usuarioId, decimal nuevaCantidad)
        {
            var transaccion = await _transaccionRepo.ObtenerPorIdAsync(id, usuarioId) 
                ?? throw new InvalidOperationException("Transacción no encontrada.");

            // Consultamos el precio actual a CriptoYa
            var precioActual = await _cryptoYa.ObtenerPrecioAsync(transaccion.CodigoCripto);
            var montoNuevo = nuevaCantidad * precioActual;

            return new PreviewEdicionDto
            {
                CantidadAnterior = transaccion.CantidadCripto,
                CantidadNueva = nuevaCantidad,
                MontoAnterior = transaccion.Monto,
                MontoNuevo = montoNuevo,
                PrecioActual = precioActual,
                Diferencia = montoNuevo > transaccion.Monto ? "aumenta" : "disminuye"
            };
        }

        // ── Confirmar edición con recálculo ───────────────────
        // Se llama DESPUÉS de que el usuario confirmó en el modal
        public async Task EditarConRecalculoAsync(int id, int usuarioId, EditartransaccionesSimpleDto dto)
        {
            var transaccion = await _transaccionRepo.ObtenerPorIdAsync(id, usuarioId)
                ?? throw new InvalidOperationException("Transacción no encontrada");

            // Validar saldo si es venta y se aumenta la cantidad
            if (dto.CantidadCripto.HasValue && transaccion.Accion == "venta")
            {
                var saldoDisponible = await _transaccionRepo.ObtenerBalanceAsync(
                    transaccion.CriptoMonedaId, usuarioId);

                var saldoEfectivo = saldoDisponible + transaccion.CantidadCripto;

                if (dto.CantidadCripto.Value > saldoEfectivo)
                {
                    throw new InvalidOperationException(
                        $"Saldo insuficiente. Disponible: {saldoDisponible}," +
                        $"intentas vender: {dto.CantidadCripto.Value}.");
                }

            }

            var dtoCompleto = new EditarTransaccionDto
            {
                FechaTransaccion = dto.FechaTransaccion
            };

            // Si cambió la cantidad, recalculamos el monto con precio actual
            if (dto.CantidadCripto.HasValue)
            {
                var precioActual = await _cryptoYa.ObtenerPrecioAsync(transaccion.CodigoCripto);
                dtoCompleto.CantidadCripto = dto.CantidadCripto.Value;
                dtoCompleto.Monto = dto.CantidadCripto.Value * precioActual;
            }

            await _transaccionRepo.EditarAsync(id, usuarioId, dtoCompleto);
        }
    }
}
