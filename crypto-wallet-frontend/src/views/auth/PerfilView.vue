src/views/auth/PerfilView.vue
<template>
    <div class="animar-entrada">

        <div class="pagina-header">
            <div>
                <h2 class="pagina-titulo">Mi Perfil</h2>
                <p class="texto-secundario">Tus datos y métodos de pago</p>
            </div>
        </div>

        <!-- Datos del usuario -->
        <div class="card seccion-card">
            <h3 class="seccion-titulo">Información personal</h3>
            <div class="perfil-datos">
                <div class="perfil-avatar">{{ inicial }}</div>
                <div>
                    <p class="perfil-nombre">{{ authStore.usuario?.nombre }}</p>
                    <p class="texto-secundario" style="font-size: var(--texto-sm);">
                       ✉️ {{ authStore.usuario?.email }}
                    </p>
                </div>
            </div>
        </div>

        <!-- Métodos de pago -->
        <div class="card seccion-card">
            <div class="seccion-header">
                <h3 class="seccion-titulo">Métodos de pago</h3>
                <button
                    v-if="metodos.length < 3"
                    @click="mostrarFormulario = !mostrarFormulario"
                    class="btn btn-primario"
                    style="padding: 0.4rem 1rem; font-size: 0.8rem;"
                >
                    + Agregar
                </button>
                <span v-else class="texto-muted" style="font-size: 0.8rem;">
                    Máximo 3 métodos
                </span>
            </div>

            <!-- Formulario para agregar -->
            <div v-if="mostrarFormulario" class="formulario-metodo">
                <div class="form-grupo">
                    <label class="form-label">Tipo</label>
                    <select v-model="nuevoMetodo.tipo" class="form-control">
                        <option value="">Seleccioná...</option>
                        <option value="debito">Tarjeta de débito</option>
                        <option value="credito">Tarjeta de crédito</option>
                        <option value="transferencia">Transferencia bancaria</option>
                    </select>
                </div>

                <!-- Campos para tarjeta -->
                <template v-if="nuevoMetodo.tipo === 'debito' || nuevoMetodo.tipo === 'credito'">
                    <div class="form-grupo">
                        <label class="form-label">Número de tarjeta</label>
                        <input
                            v-model="nuevoMetodo.numeroCompleto" 
                            type="text"
                            maxlength="19"
                            placeholder="1234 5678 9012 3456"
                            class="form-control"
                            @input="formatearNumeroTarjeta"
                        />
                        <!-- Mostramos los últimos 4 en tiempo real -->
                        <p v-if="nuevoMetodo.numeroCompleto.length >= 4" class="hint-tarjeta">
                            Se guardaran solo los últimos 4 dígitos:
                            <strong>{{ ultimosCuatro }}</strong>
                        </p>
                    </div>
                    <div class="form-grupo">
                        <label class="form-label">Banco</label>
                        <input
                            v-model="nuevoMetodo.banco" 
                            type="text" 
                            placeholder="Ej: Galicia, Santander..."
                            class="form-control"
                        />
                    </div>
                </template>

                <!-- Campos para transferencia -->
                <template v-if="nuevoMetodo.tipo === 'transferencia'">
                    <!-- Selector Alias O CBU — no ambos -->
                    <div class="tipo-transferencia">
                        <button
                            class="tipo-btn"
                            :class="{ 'activo': modoTransferencia === 'alias' }"
                            @click="modoTransferencia = 'alias'"
                        >
                            Alias
                        </button>
                        <button
                            class="tipo-btn"
                            :class="{ 'activo': modoTransferencia === 'cbu' }"
                            @click="modoTransferencia = 'cbu'"
                        >
                            CBU
                        </button>
                    </div>

                    <div v-if="modoTransferencia === 'alias'" class="form-grupo">
                        <label class="form-label">Alias</label>
                        <input 
                            v-model="nuevoMetodo.alias" 
                            type="text" 
                            placeholder="mi.alias.banco"
                            class="form-control"
                        />
                    </div>

                    <div v-if="modoTransferencia === 'cbu'" class="form-grupo">
                        <label class="form-label">CBU</label>
                        <input 
                            v-model="nuevoMetodo.cbu" 
                            type="text"
                            maxlength="22"
                            placeholder="22 digitos"
                            class="form-control"
                        />
                    </div>

                    <div class="form-grupo">
                        <label class="form-label">Banco</label>
                        <input 
                            v-model="nuevoMetodo.banco" 
                            type="text"
                            placeholder="Ej: Galicia, Santander..."
                            class="form-control"
                        />
                    </div>
                </template> 

                <div class="flex gap-sm" style="margin-top: 1rem; justify-content: flex-end;">
                    <button @click="cancelarFormulario" class="btn btn-ghost"> 
                        Cancelar
                    </button>
                    <button @click="agregarMetodo" class="btn btn-primario" :disabled="guardando">
                        {{ guardando ? 'Guardando...' : 'Guardar' }}
                    </button>
                </div>

                <div v-if="errorFormulario" class="banner-error" style="margin-top: 0.5rem;">
                    ⚠️ {{ errorFormulario }}
                </div>
            </div>

            <!-- Lista de métodos -->
            <div v-if="cargandoMetodos" class="texto-secundario" style="padding: 1rem 0;">
                Cargando métodos de pago...
            </div>

            <div v-else-if="metodos.length === 0" class="estado-vacio-pequeno">
                No tenés métodos de pago registrados
            </div>

            <div v-else class="metodos-lista">
                <div v-for="metodo in metodos" :key="metodo.id" class="metodo-item">
                    <div class="metodo-icono">
                        {{ metodo.tipo === 'debito' ? '💳' : metodo.tipo === 'credito' ? '💰' : '🏦' }}
                    </div>
                    <div class="metodo-info">
                        <p class="metodo-nombre">
                            {{ metodo.tipo === 'transferencia'
                                ? `Transferencia - ${metodo.alias || metodo.cbu}`
                                : `${metodo.tipo === 'debito' ? 'Débito' : 'Crédito'} •••• ${metodo.ultimosDigitos}`}}
                        </p>
                        <p class="texto-muted" style="font-size: 0.75rem;">{{ metodo.banco }}</p>
                    </div>
                    <span v-if="metodo.esPrincipal" class="badge badge-compra">Principal</span>
                    <button @click="eliminarMetodo(metodo.id)" class="btn btn-ghost" style="padding: 0.3rem 0.6rem; color: var(--peligro);">
                        🗑️
                    </button>
                </div>
            </div>

        </div>

        <!-- Modal confirmar eliminar método de pago -->
        <div v-if="metodoPagoAEliminar" class="modal-overlay" @click.self="metodoPagoAEliminar = null">
            <div class="modal" style="max-width: 400px;">
                <h3>¿Eliminás este método de pago?</h3>
                <p class="texto-secundario" style="margin: 1rem 0;"> 
                    Esta acción no se puede deshacer.
                </p>
                <div class="flex gap-sm" style="justify-content: flex-end;">
                    <button @click="metodoPagoAEliminar = null" class="btn btn-ghost">Cancelar</button>
                    <button @click="confirmarEliminarMetodo" class="btn btn-peligro">Eliminar</button>
                </div>
            </div>
        </div>

    </div>
</template>

<script setup>
    import { ref, reactive, computed, onMounted } from 'vue'
    import { useAuthStore } from '@/stores/auth';
    import { authApi } from '@/services/api';
    import { useToastStore } from '@/stores/toast'

    const authStore = useAuthStore()
    const toastStore = useToastStore()
    const inicial = computed(() =>
        authStore.usuario?.nombre?.charAt(0).toUpperCase() || '?'
    )

    const metodos = ref([])
    const cargandoMetodos = ref(false)
    const mostrarFormulario = ref(false)
    const guardando = ref(false)
    const errorFormulario = ref('')
    const modoTransferencia = ref('alias')
    const metodoPagoAEliminar = ref(null)

    const nuevoMetodo = reactive({
        tipo: '',
        numeroCompleto: '',
        alias: '',
        cbu: '',
        banco: '',
        esPrincipal: false
    })

    // Computed que extrae los últimos 4 dígitos del número completo
    const ultimosCuatro = computed(() => {
        const soloNumeros = nuevoMetodo.numeroCompleto.replace(/\D/g, '')
        return soloNumeros.slice(-4)
    })

    // Formatea el número con espacios cada 4 dígitos: 1234 5678 9012 3456
    const formatearNumeroTarjeta = () => {
        const soloNumeros = nuevoMetodo.numeroCompleto.replace(/\D/g, '').slice(0, 16)
        nuevoMetodo.numeroCompleto = soloNumeros.match(/.{1,4}/g)?.join(' ') || soloNumeros
    }

    const limpiarCampos = () => {
        nuevoMetodo.numeroCompleto = '',
        nuevoMetodo.banco = '',
        nuevoMetodo.alias = '',
        nuevoMetodo.cbu = '',
        modoTransferencia.value = 'alias'
    }

    const cancelarFormulario = () => {
        mostrarFormulario.value = false
        limpiarCampos()
        errorFormulario.value = ''
    } 

    const cargarMetodos = async () => {
        cargandoMetodos.value = true
        try {
            const { data } = await authApi.obtenerMetodosPago()
            metodos.value = data
        } catch (error) {
            console.error('Error al cargar metodos:', error)
        } finally {
            cargandoMetodos.value = false
        }
    }

    const agregarMetodo = async () => {
        errorFormulario.value = ''

        if (!nuevoMetodo.tipo) {
            errorFormulario.value = 'Seleccioná un tipo de método de pago'
            return 
        }

        // Validaciones según el tipo
        if(nuevoMetodo.tipo === 'debito' || nuevoMetodo.tipo === 'credito') {
            const soloNumeros = nuevoMetodo.numeroCompleto.replace(/\D/g, '')
            if (soloNumeros.length < 16 ) {
                errorFormulario.value = 'El numero de tarjeta debe tener 16 digitos'
                return
            } 
        } 

        if (nuevoMetodo.tipo === 'transferencia') {
            if (modoTransferencia.value === 'alias' && !nuevoMetodo.alias) {
                errorFormulario.value = 'Ingresa el alias'
                return
            }

            if (modoTransferencia.value === 'cbu' && nuevoMetodo.cbu.length !== 22) {
                errorFormulario.value = 'El CBU debe tener exactamente 22 digitos'
                return
            }
        }

        guardando.value = true
        errorFormulario.value = ''
        try {
            await authApi.agregarMetodoPago({
                tipo: nuevoMetodo.tipo,
                ultimosDigitos: ultimosCuatro.value || null,
                alias: nuevoMetodo.alias || null,
                cbu: nuevoMetodo.cbu || null,
                banco: nuevoMetodo.banco || null,
                esPrincipal: nuevoMetodo.esPrincipal
            })
            await cargarMetodos()
            cancelarFormulario()
            toastStore.mostrar('Metodo de pago agregado correctamente', 'exito')
        } catch (error) {
            errorFormulario.value = error.message
        } finally {
            guardando.value = false
        }
    }

    const eliminarMetodo = (id) => {
        metodoPagoAEliminar.value = id
    }

    const confirmarEliminarMetodo = async () => {
        try {
            await authApi.eliminarMetodoPago(metodoPagoAEliminar.value)
            metodos.value = metodos.value.filter(m => m.id !== metodoPagoAEliminar.value)
            metodoPagoAEliminar.value = null
        } catch (error) {
            errorFormulario.value = error.message
            metodoPagoAEliminar.value = null
        }
    }

    onMounted(cargarMetodos)

</script>

<style scoped>
    
    .pagina-header {
        display: flex;
        justify-content: space-between;
        align-items: center;
        margin-bottom: var(--espacio-xl);
    }

    .pagina-titulo {
        font-size: var(--texto-2xl);
        font-weight: 700;
        margin-bottom: 4px;
    }

    .seccion-card { margin-bottom: var(--espacio-lg);}

    .seccion-header {
        display: flex;
        justify-content: space-between;
        align-items: center;
        margin-bottom: var(--espacio-lg);
    }

    .seccion-titulo { font-size: var(--texto-lg); font-weight: 700;}

    .perfil-datos {
        display: flex;
        align-items: center;
        gap: var(--espacio-lg);
    }

    .perfil-avatar {
        width: 56px;
        height: 56px;
        border-radius: var(--radio-full);
        background: var(--acento);
        color: white;
        display: flex;
        align-items: center;
        justify-content: center;
        font-size: var(--texto-2xl);
        font-weight: 700;
    }

    .perfil-nombre { font-size: var(--texto-lg); font-weight: 600;}

    .formulario-metodo {
        background: var(--bg-secondary);
        border-radius: var(--radio-md);
        padding: var(--espacio-lg);
        margin-bottom: var(--espacio-lg);
        display: flex;
        flex-direction: column;
        gap: var(--espacio-md);
    }

    /* Selector Alias/CBU */
    .tipo-transferencia {
        display: flex;
        gap: var(--espacio-xs);
        background: var(--bg-card);
        padding: var(--espacio-xs);
        border-radius: var(--radio-md);
    }

    .tipo-btn {
        flex: 1;
        padding: 0.5rem;
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

    .tipo-btn.activo {
        background: var(--acento);
        color: white;
    }

    /* Hint de últimos 4 dígitos */
    .hint-tarjeta {
        font-size: var(--texto-xs);
        color: var(--texto-secundario);
        margin-top: 4px;
    }

    .metodos-lista {
        display: flex;
        flex-direction: column;
        gap: var(--espacio-md);
        padding: var(--espacio-md);
        background: var(--bg-secondary);
        border-radius: var(--radio-md);
        border: 1px solid var(--border);
    }

    .metodo-item {
        display: flex;
        align-items: center;
        gap: var(--espacio-md);
        width: 100%;
        padding: var(--espacio-sm) 0;
    }

    .metodo-icono { font-size: 1.5rem; }
    .metodo-info { flex: 1;}
    .metodo-nombre { font-weight: 600; font-size: var(--texto-sm);}

    .estado-vacio-pequeno {
        text-align: center;
        padding: var(--espacio-lg);
        color: var(--texto-muted);
        font-size: var(--texto-sm);
    }

</style>