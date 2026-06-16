<!-- src/views/NuevaTransaccionView.vue -->
<template>
    <div class="animar-entrada">

        <!-- Encabezado -->
        <div class="pagina-header">
            <div>
                <h2 class="pagina-titulo">Nueva Transacción</h2>
                <p class="texto-secundario">Registrá una compra o venta de criptomonedas</p>
            </div>
            <RouterLink to="/transacciones" class="btn btn-ghost">
                ← Volver
            </RouterLink>
        </div>

        <!-- Formulario -->
        <div class="card form-card">
            <div class="tipo-selector">
                <button
                    class="tipo-btn"
                    :class="{ 'tipo-btn-activo tipo-btn-compra': form.accion === 'compra' }"
                    @click="form.accion = 'compra'"
                >
                    📈 Compra
                </button>
                <button 
                    class="tipo-btn"
                    :class="{ 'tipo-btn-activo tipo-btn-venta': form.accion === 'venta' }"
                    @click="form.accion = 'venta'"
                >
                    📉 Venta
                </button>
            </div>
            <div class="form-grid">
                <!-- Criptomoneda -->
                <div class="form-grupo">
                    <label class="form-label">Criptomoneda</label>
                    <select 
                        v-model="form.codigoCripto"
                        class="form-control"
                        :class="{ 'es-error': errores.codigoCripto }"
                    >
                        <option value="">Seleccioná una cripto...</option>
                        <option
                            v-for="cripto in store.criptos"
                            :key="cripto.code" 
                            :value="cripto.code"
                        >
                            {{ cripto.nombre }} ({{ cripto.simbolo }})
                        </option>
                    </select>
                    <span v-if="errores.codigoCripto" class="form-error">
                        {{ errores.codigoCripto }}
                    </span>
                </div>

                <!-- Cantidad -->
                <div class="form-grupo">
                    <label class="form-label">Cantidad</label>
                    <input 
                        v-model.number="form.cantidadCripto"
                        type="number"
                        step="0.00000001"
                        min="0.00000001"
                        placeholder="Ej: 0.001"
                        inputmode="decimal"
                        class="form-control"
                        :class="{'es-error': errores.cantidadCripto }"
                    />
                    <span v-if="errores.cantidadCripto" class="form-error">
                        {{ errores.cantidadCripto }}
                    </span>
                </div>

                <!-- Fecha -->
                <div class="form-grupo">
                    <label class="form-label">Fecha y hora</label>
                    <input
                        v-model="form.fechaTransaccion"
                        type="datetime-local"
                        class="form-control"
                        :class="{ 'es-error': errores.fechaTransaccion }" 
                    />
                    <span v-if="errores.fechaTransaccion" class="form-error">
                        {{ errores.fechaTransaccion }}
                    </span>
                </div>

            </div>

            <!-- Preview del precio -->
            <div v-if="form.codigoCripto" class="precio-preview">
                <p class="texto-secundario">
                    El precio se calculará automaticamente desde
                    <strong>CriptoYa</strong> al momento de guardar.
                </p>
            </div>

            <!-- Error del servidor -->
            <div v-if="errorServidor" class="banner-error" style="margin-top: 1rem">
                ⚠️ {{ errorServidor }}
            </div>

            <!-- Botones -->
            <div class="form-acciones">
                <RouterLink to="/transacciones" class="btn btn-ghost">
                    Cancelar
                </RouterLink>
                <button
                    @click="guardar"
                    class="btn"
                    :class="form.accion === 'compra' ? 'btn-exito' : 'btn-peligro'"
                    :disabled="guardando"
                >   
                    {{ guardando ? 'Guardando...' : `Registrar ${form.accion}` }}
                </button>
            </div>

        </div>
    </div>
</template>

<script setup>

    import { ref, reactive, onMounted } from 'vue';
    import { useRouter } from 'vue-router';
    import { useTransaccionesStore } from '@/stores/transacciones';
    import { useToastStore } from '@/stores/toast'
     
    const toastStore = useToastStore()
    const store = useTransaccionesStore()
    const router = useRouter()
    const ahora = new Date()
    const fechaActual = new Date(ahora.getTime() - ahora.getTimezoneOffset() * 60000)
        .toISOString().slice(0, 16)

    // reactive — para objetos con múltiples propiedades
    // es más cómodo que tener un ref por cada campo del form
    const form = reactive({
        accion: 'compra',
        codigoCripto: '',
        cantidadCripto: null,
        fechaTransaccion: fechaActual   //fecha actual
    })

    // Errores de validación — uno por campo
    const errores = reactive({
        codigoCripto: '',
        cantidadCripto: '',
        fechaTransaccion: ''
    })

    const guardando = ref(false)
    const errorServidor = ref('')

    // Validación del formulario
    const validar = () => {
        // Limpiamos errores anteriores
        errores.codigoCripto = ''
        errores.cantidadCripto = ''
        errores.fechaTransaccion = ''
        errorServidor.value = ''

        let esValido = true

        if (!form.codigoCripto) {
            errores.codigoCripto = 'Seleccioná una criptomoneda'
            esValido = false
        }

        if (!form.cantidadCripto || form.cantidadCripto <= 0) {
            errores.cantidadCripto = 'La cantidad debe ser mayor a 0'
            esValido = false
        }

        if (!form.fechaTransaccion) {
            errores.fechaTransaccion = 'La fecha es obligatoria'
            esValido = false
        }

        return esValido
    }

    const guardar = async () => {
        if (!validar()) return  // Si hay errores, no continúa

        guardando.value = true
        try {
            await store.crear({
                codigoCripto: form.codigoCripto,
                accion: form.accion,
                cantidadCripto: form.cantidadCripto,
                fechaTransaccion: new Date(form.fechaTransaccion).toISOString()
            })

            // Redirige al historial después de guardar
            toastStore.mostrar('¡Transacción registrada exitosamente!', 'exito')
            router.push('/transacciones')
        } catch (error) {
            errorServidor.value = error.message
        } finally {
            guardando.value = false
        }
    }

    onMounted(() => store.obtenerCriptos())
</script>

<style scoped>

    .form-card {
        max-width: 680px;
    }

    /* Selector compra/venta */
    .tipo-selector {
        display: flex;
        gap: var(--espacio-sm);
        margin-bottom: var(--espacio-xl);
        background: var(--bg-secondary);
        padding: var(--espacio-xs);
        border-radius: var(--radio-md);
    }

    .tipo-btn {
        flex: 1;
        padding:0.65rem;
        border: none;
        border-radius: var(--radio-sm);
        background: transparent;
        color: var(--texto-secundario);
        font-weight: 600;
        font-size: var(--texto-sm);
        cursor: pointer;
        transition: all var(--transicion-rapida);
        font-family: var(--fuente);
    }

    .tipo-btn-activo {
        color: white;
    }

    .tipo-btn-compra {
        background: var(--exito);
        color: #0d0f14;
    }

    .tipo-btn-venta {
        background: var(--peligro);
    }

    /* Grid del formulario */
    .form-grid {
        display: grid;
        grid-template-columns: 1fr 1fr;
        gap: var(--espacio-lg);
        margin-bottom: var(--espacio-lg);
    }

    /* La fecha ocupa toda la fila */
    .form-grid .form-grupo:last-child {
        grid-column: 1 / -1;
    }

    .precio-preview{
        background: var(--bg-secondary);
        border: 1px solid var(--border);
        border-radius: var(--radio-md);
        padding: var(--espacio-md);
        margin-bottom: var(--espacio-lg);
    }
    
    @media (max-width: 768px) {
        .form-grid { grid-template-columns: 1fr;}
        .form-grid .form-grupo:last-child { grid-column: 1; }
    }

</style>