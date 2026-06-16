<!-- src/views/EditarTransaccionView.vue -->
<template>
    <div class="animar-entrada">
        
        <!-- Encabezado -->
        <div class="pagina-header">
            <div>
                <h2 class="pagina-titulo">Editar Transacción</h2>
                <p class="texto-secundario">Modificá los datos de esta transacción</p>
            </div>
            <RouterLink to="/transacciones" class="btn btn-ghost">
                ← Volver
            </RouterLink>
        </div>

        <!-- Loader mientras carga la transacción -->
        <div v-if="cargando" class="card form-card">
            <div v-for="i in 3" :key="i" class="skeleton" style="height: 60px; margin-bottom: 1rem;"></div>
        </div>

        <!-- Error al cargar -->
        <div v-else-if="errorCarga" class="banner-error">
            ⚠️ {{ errorCarga }}
        </div>

        <!-- Formulario -->
        <div v-else class="card form-card">
            
            <!-- Info de la cripto — solo lectura, no se puede cambiar -->
            <div class="cripto-info">
                <img 
                    :src="transaccionOriginal?.urlIconoCripto"
                    :alt="transaccionOriginal?.simboloCripto"
                    class="cripto-icono-grande"
                    @error="(e) => e.target.style.display = 'none'"
                />
                <div>
                    <p class="cripto-nombre">{{ transaccionOriginal?.nombreCripto }}</p>
                    <p class="texto-secundario">{{ transaccionOriginal?.simboloCripto }}</p>
                </div>
                <span :class="transaccionOriginal?.accion === 'compra' ? 'badge badge-compra' : 'badge badge-venta'">
                    {{ transaccionOriginal?.accion }}
                </span>
            </div>
            
            <div class="form-grid">

                <!-- Cantidad -->
                 <div class="form-grupo">
                    <label class="form-label">Cantidad</label>
                    <input 
                        v-model.number="form.cantidadCripto"
                        type="number"
                        step="0.00000001"
                        min="0.00000001"
                        inputmode="decimal"
                        class="form-control"
                        :class="{ 'es-error': errores.cantidadCripto}"
                    />
                    <span v-if="errores.cantidadCripto" class="form-error">
                        {{ errores.cantidadCripto }}
                    </span>
                 </div>

                <!-- Fecha -->
                <div class="form-grupo">
                    <label class="form-label">Fecha y Hora</label>
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

            <!-- Botón para ver el preview ANTES de guardar -->
            <button
                @click="verPreview"
                class="btn btn-ghost"
                :disabled="calculandoPreview || !form.cantidadCripto"
                style="margin-bottom: 1rem;"
            >
                {{ calculandoPreview ? 'Calculando...' : '🔍 Ver el impacto del cambio' }}
            </button>

            <!-- Aviso sobre recálculo -->
            <div class="aviso-edicion">
                ℹ️ Si cambiás la cantidad, el monto se <strong>recalculará automaticamente</strong> con el precio actual de CriptoYa.
            </div>

            <div v-if="errorServidor" class="banner-error" style="margin-top: 1rem;">
                ⚠️ {{ errorServidor }}
            </div>

            <div class="form-acciones">
                <RouterLink to="/transacciones" class="btn btn-ghost">Cancelar</RouterLink>
                <button @click="solicitarConfirmacion" class="btn btn-primario" :disabled="guardando">
                    {{ guardando ? 'Guardando...' : 'Guardar cambios' }}
                </button>
            </div>

        </div>

        <!-- Modal de preview -->
        <div v-if="preview" class="modal-overlay" @click.self="preview = null">
            <div class="modal">
                <h3>Impacto del cambio</h3>
                <div class="preview-grid">
                    <div class="preview-item">
                        <p class="preview-label">Cantidad anterior</p>
                        <p class="preview-valor">{{ preview.cantidadAnterior }}</p>
                    </div>
                    <div class="preview-item">
                        <p class="preview-label">Cantidad nueva</p>
                        <p class="preview-valor">{{ preview.cantidadNueva }}</p>
                    </div>
                    <div class="preview-item">
                        <p class="preview-label">Monto anterior</p>
                        <p class="preview-valor">{{ formatearARS(preview.montoAnterior) }}</p>
                    </div>
                    <div class="preview-item">
                        <p class="preview-label">Monto nuevo</p>
                        <p class="preview-valor" :class="preview.diferencia === 'aumenta' ? 'texto-peligro' : 'texto-exito'">
                            {{ formatearARS(preview.montoNuevo) }}
                        </p>
                    </div>
                </div>
                <p class="preview-resumen">
                    El monto
                    <strong :class="preview.diferencia === 'aumenta' ? 'texto-peligro' : 'texto-exito'">
                    {{ preview.diferencia }}
                    </strong>
                    con el precio actual de
                    <strong>{{ formatearARS(preview.precioActual) }}</strong> por unidad.
                </p>
                <div class="flex gap-sm" style="justify-content: flex-end; margin-top: 1.5rem;">
                    <button @click="preview = null" class="btn btn-ghost">Cancelar</button>
                    <button @click="guardar" class="btn btn-primario" :disabled="guardando">
                        {{ guardando ? 'Guardando...' : 'Confirmar cambio' }}
                    </button>
                </div>
            </div> 
        </div>

        <!-- Modal de confirmación sin preview -->
        <div v-if="mostrarConfirmacion" class="modal-overlay" @click.self="mostrarConfirmacion = false">
            <div class="modal">
                <h3>Confirmar edición</h3>
                <p class="texto-secundario" style="margin: 1rem 0;">
                    La cantidad cambió. El monto se recalculará automáticamente
                    con el precio actual de CriptoYa. ¿Confirmás?
                </p>
                <div class="flex gap-sm" style="justify-content: flex-end;">
                    <button @click="mostrarConfirmacion = false" class="btn btn-ghost">Cancelar</button>
                    <button @click="guardar" class="btn btn-primario" :disabled="guardando">
                        {{ guardando ? 'Guardando...' : 'Confirmar' }}
                    </button>
                </div>
            </div>
        </div>

    </div>
</template>

<script setup>

    import { ref, reactive, onMounted } from "vue";
    import { useRoute, useRouter } from "vue-router";
    import { transaccionesApi } from "@/services/api";
    import { useTransaccionesStore } from "@/stores/transacciones";
    import { useToastStore } from '@/stores/toast'

    const route = useRoute();
    const router = useRouter();
    const store = useTransaccionesStore();
    const toastStore = useToastStore();
    const id = Number(route.params.id);

    const cargando = ref(true);
    const errorCarga = ref('');
    const guardando = ref(false);
    const calculandoPreview = ref(false);
    const errorServidor = ref('');
    const transaccionOriginal = ref(null);
    const preview = ref(null);
    const mostrarConfirmacion = ref(false);

    const form = reactive({ cantidadCripto: null, fechaTransaccion: '' })
    const errores = reactive({ cantidadCripto: '', fechaTransaccion: ''})

    const formatearARS = (valor) => 
        new Intl.NumberFormat('es-AR', { style: 'currency', currency: 'ARS' }).format(valor)

    const validar = () => {
        errores.cantidadCripto = ''
        errores.fechaTransaccion = ''
        errorServidor.value = ''
        let valido = true

        if (!form.cantidadCripto || form.cantidadCripto <= 0) {
            errores.cantidadCripto = 'La cantidad debe ser mayor a 0'
            valido = false
        } 
        if (!form.fechaTransaccion) {
            errores.fechaTransaccion = 'La fecha es obligatoria'
            valido = false
        }

        return valido
    }

    // Ver el preview antes de confirmar
    const verPreview = async () => {
        if (!form.cantidadCripto || form.cantidadCripto <= 0) return
        calculandoPreview.value = true
        try {
            const { data } = await transaccionesApi.previewEdicion(id, form.cantidadCripto)
            preview.value = data
        } catch (error) {
            errorServidor.value = error.message
        } finally { 
            calculandoPreview.value = false
        }
    }

    // Si el usuario hace clic en "Guardar" sin ver el preview primero
    const solicitarConfirmacion = () => {
        if (!validar()) return
        // Redondeamos a 8 decimales para evitar errores de punto flotante
        const cantidadActual = Math.round(form.cantidadCripto * 1e8) / 1e8
        const cantidadOriginal = Math.round((transaccionOriginal.value?.cantidadCripto ?? 0) * 1e8 ) / 1e8
        if (cantidadActual !== cantidadOriginal) {
            mostrarConfirmacion.value = true
        } else {
            // Si solo cambió la fecha, guardamos directo
            guardar()
        }
    }

    const guardar = async () => {
        if (!validar()) return
        guardando.value = true
        preview.value = null
        mostrarConfirmacion.value = false

        try {
            await store.editar(id, {
                cantidadCripto: form.cantidadCripto,
                fechaTransaccion: new Date(form.fechaTransaccion).toISOString()
            })
            toastStore.mostrar('¡Transacción actualizada exitosamente!', 'exito')
            router.push('/transacciones')
        } catch (error) {
            errorServidor.value = error.message
        } finally {
            guardando.value = false
        }
    }

    onMounted(async () => {
        try { 
            const { data } = await transaccionesApi.obtenerPorId(id)
            transaccionOriginal.value = data

            const fechaUTC = data.fechaTransaccion.endsWith('Z') ? data.fechaTransaccion : data.fechaTransaccion + 'Z'
            const fecha = new Date(fechaUTC)
            const fechaLocal = new Date(fecha.getTime() - fecha.getTimezoneOffset() * 60000)
                .toISOString().slice(0, 16)

            form.cantidadCripto = data.cantidadCripto
            form.fechaTransaccion = fechaLocal
        } catch {
            errorCarga.value = 'No se pudo cargar la transacción.'
        } finally {
            cargando.value = false
        }
    })

</script>

<style scoped>

    .form-card { max-width: 680px;}

    .cripto-info {
        display: flex;
        align-items: center;
        gap: var(--espacio-md);
        padding: var(--espacio-md);
        background: var(--bg-secondary);
        border-radius: var(--radio-md);
        margin-bottom: var(--espacio-xl);
    }

    .cripto-icono-grande { width: 40px; height: 40px; border-radius: var(--radio-full); }
    .cripto-nombre { font-weight: 700; font-size: var(--texto-lg); }

    .form-grid {
        display: grid;
        grid-template-columns: 1fr 1fr;
        gap: var(--espacio-lg);
        margin-bottom: var(--espacio-lg);
    }

    .form-acciones {
        display: flex;
        justify-content: flex-end;
        gap: var(--espacio-md);
        margin-top: var(--espacio-lg);
        border-top: 1px solid var(--border);
    }

    .aviso-edicion {
        background: rgba(255, 209, 102, 0.1);
        border: 1px solid var(--advertencia);
        border-radius: var(--radio-md);
        padding: var(--espacio-md);
        color: var(--advertencia);
        font-size: var(--texto-sm);
        margin-bottom: var(--espacio-md);
    }

    /* Preview grid */
    .preview-grid {
        display: grid;
        grid-template-columns:1fr 1fr;
        gap: var(--espacio-md);
        margin: var(--espacio-lg) 0;
    }

    .preview-item {
        background: var(--bg-secondary);
        border-radius: var(--radio-md);
        padding: var(--espacio-md);
    }

    .preview-label { font-size: var(--texto-xs); color: var(--texto-muted); margin-bottom: 4px;}
    .preview-valor { font-size: var(--texto-lg); font-weight: 700;}
    .preview-resumen { font-size: var(--texto-sm); color: var(--texto-secundario);}

    @media (max-width: 768px) {
        .form-grid { grid-template-columns: 1fr; }
        .preview-grid { grid-template-columns: 1fr; }
    }

</style>