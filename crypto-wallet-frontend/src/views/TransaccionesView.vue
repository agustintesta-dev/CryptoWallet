<!-- src/views/TransaccionesView.vue -->
<template>
    <div class="animar-entrada">

        <!-- Encabezado -->
        <div class="pagina-header">
            <div>
                <h2 class="pagina-titulo">Historial de Transacciones</h2>
                <p class="texto-secundario">Todas tus compras y ventas registradas</p>
            </div>
            <RouterLink to="/transacciones/nueva" class="btn btn-primario">
                + Nueva transacción
            </RouterLink>
        </div>

        <div class="filtros-barra">
            <span class="filtros-titulo">🔍 Filtrar: </span>
            <select v-model="filtros.codigoCripto" class="form-control" style="max-width: 200px;">
                <option value="">Todas las criptos</option>
                <option v-for="c in store.criptos" :key="c.code" :value="c.code">
                    {{ c.nombre }}
                </option>
            </select>

            <select v-model="filtros.accion" class="form-control" style="max-width: 200px;">
                <option value="">Compras y ventas</option>
                <option value="compra">Solo compras</option>
                <option value="venta">Solo ventas</option>
            </select>

            <input 
                v-model="filtros.fechaDesde"
                type="date"
                class="form-control"
                style="max-width: 200px;"
                placeholder="Desde"
            >

            <input 
                v-model="filtros.fechaHasta"
                type="date"
                class="form-control"
                style="max-width: 200px;"
                placeholder="Hasta"
            >

            <button @click="aplicarFiltros" class="btn btn-primario">Filtrar</button>
            <button @click="limpiarFiltros" class="btn btn-ghost">Limpiar</button>

        </div>

        <!-- Loader — se muestra mientras carga -->
        <div v-if="store.cargando" class="skeletons">
            <div v-for="i in 5" :key="i" class="skeleton" style="height: 60px;" />
        </div>

        <template v-else>
            <!-- Error -->
            <div v-if="errorModal" class="banner-error" style="margin-top: 1rem;">
                ⚠️ {{ errorModal }}
            </div>

            <!-- Sin transacciones -->
            <div v-else-if="store.transacciones.length === 0" class="estado-vacio">
                <p class="vacio-icono">📭</p>
                <h3>No hay transacciones todavía</h3>
                <p class="texto-secundario">Registrá tu primera compra para empezar</p>
                <RouterLink to="/transacciones/nueva" class="btn btn-primario">
                    + Nueva transacción
                </RouterLink>
            </div>

            <!-- Tabla de transacciones -->
            <div v-else class="card">
                <div class="tabla-wrapper">
                    <table class="tabla">
                        <thead>
                            <tr>
                                <th>Criptomoneda</th>
                                <th>Tipo</th>
                                <th>Cantidad</th>
                                <th>Monto ARS</th>
                                <th>Precio unitario</th>
                                <th>Fecha</th>
                                <th>Acciones</th>
                            </tr>
                        </thead>
                        <tbody>
                            <tr v-for="t in store.transacciones" :key="t.id">

                                <!-- Cripto con ícono y nombre -->
                                <td>
                                    <div class="cripto-celda">
                                        <img 
                                            :src="t.urlIconoCripto"
                                            :alt="t.simboloCripto"
                                            class="cripto-icono"
                                            @error="(e) => e.target.style.display = 'none'"
                                        />
                                        <div>
                                            <p class="semibold">{{ t.nombreCripto }}</p>
                                            <p class="texto-muted" style="font-size: 0.75rem;">{{ t.simboloCripto }}</p>
                                        </div>
                                    </div>
                                </td>

                                <!-- Badge compra/venta -->
                                <td>
                                    <span :class="t.accion === 'compra' ? 'badge badge-compra' : 'badge badge-venta'" >
                                        {{ t.accion }}
                                    </span>
                                </td>

                                <td> {{ t.cantidadCripto }}</td>
                                <td class="semibold"> {{ formatearARS(t.monto) }}</td>
                                <td class="texto-secundario">{{ formatearARS(t.tipoDeCambio) }}</td>
                                <td class="texto-secundario">{{ formatearFecha(t.fechaTransaccion) }}</td>

                                <!-- Acciones -->
                                <td>
                                    <div class="flex gap-sm">
                                        <RouterLink 
                                            :to="`/transacciones/${t.id}/editar`"
                                            class="btn btn-ghost"
                                            style="padding: 0.3rem 0.7rem; font-size: 0.75rem;">
                                                ✏️ Editar
                                        </RouterLink>
                                        <button
                                            @click="confirmarEliminar(t)"
                                            class="btn btn-peligro"
                                            style="padding: 0.3rem 0.7rem; font-size: 0.75rem;">
                                                🗑️ Borrar
                                        </button>
                                    </div>
                                </td>

                            </tr>
                        </tbody>
                    </table>
                </div>
            </div>
            <div v-if="store.paginacion.totalPaginas > 1" class="paginacion">
                <button 
                    class="btn btn-ghost"
                    :disabled="store.paginacion.pagina === 1"
                    @click="cambiarPagina(store.paginacion.pagina - 1)"
                >
                    ← Anterior
                </button>
                <span class="paginacion-info">
                    Página {{ store.paginacion.pagina }} de {{ store.paginacion.totalPaginas }}
                    ({{ store.paginacion.total }} transacciones)
                </span>

                <button
                    class="btn btn-ghost"
                    :disabled="store.paginacion.pagina === store.paginacion.totalPaginas"
                    @click="cambiarPagina(store.paginacion.pagina + 1 )"
                >   
                    Siguiente →
                </button>
            </div>
        </template>
        

        <!-- Modal de confirmación para borrar -->
        <div v-if="transaccionAEliminar" class="modal-overlay" @click.self="cancelarEliminar">
            <div class="modal">
                <h3>¿Confirmar el borrado?</h3>
                <p class="texto-secundario" style="margin: 1rem 0;">
                    Vas a eliminar la {{ transaccionAEliminar.accion }} de 
                    <strong>{{ transaccionAEliminar.cantidadCripto }} {{ transaccionAEliminar.simboloCripto }}</strong>
                    Esta acción no se puede deshacer.
                </p>
                <div class="flex gap-sm" style="justify-content: flex-end;">
                    <button @click="cancelarEliminar" class="btn btn-ghost">Cancelar</button>
                    <button @click="ejecutarEliminar" class="btn btn-peligro" :disabled="eliminando">
                        {{ eliminando ? 'Eliminando...' : 'Si, eliminar' }}
                    </button>
                </div>
            </div>
        </div>

    </div>
</template>

<script setup> 
    import { ref, reactive, onMounted} from 'vue';
    import { useTransaccionesStore } from '@/stores/transacciones';

    const filtros = reactive({
        codigoCripto: '',
        accion: '',
        fechaDesde: '',
        fechaHasta: ''
    })

    const aplicarFiltros = () => store.obtenerTodas(1, filtros)
    const limpiarFiltros = () => {
        filtros.codigoCripto = '',
        filtros.accion = '',
        filtros.fechaDesde = '',
        filtros.fechaHasta = ''
        store.obtenerTodas(1)
    }

    const store = useTransaccionesStore();

    // Estado local del modal
    const transaccionAEliminar = ref(null);
    const eliminando = ref(false)
    const cambiarPagina = (pagina) => store.obtenerTodas(pagina)

    // Formateadores
    const formatearARS = (valor) => 
        new Intl.NumberFormat('es-AR', { style: 'currency', currency: 'ARS'}).format(valor)

    const formatearFecha = (fecha) =>
        new Date(fecha).toLocaleDateString('es-AR', {
            day: '2-digit', month: '2-digit', year: 'numeric'
        })

    // Eliminar
    const confirmarEliminar = (transaccion) => transaccionAEliminar.value = transaccion
    const cancelarEliminar = () => {
        transaccionAEliminar.value = null
        errorModal.value = ''    
    }
    const errorModal = ref('')

    const ejecutarEliminar = async () => {
        eliminando.value = true
        try {
            await store.eliminar(transaccionAEliminar.value.id)
            cancelarEliminar()
        } catch (error) {
            errorModal.value = error.message
        } finally {
            eliminando.value = false
        }
    }

    // onMounted — se ejecuta cuando el componente aparece en pantalla
    // Es el lugar correcto para cargar datos iniciales
    onMounted(() => {
        store.obtenerCriptos()
        store.obtenerTodas(1)
    })

</script>

<style scoped>

    .tabla-wrapper { overflow-x: auto;}

    .filtros-barra {
        display: flex;
        gap: var(--espacio-md);
        flex-wrap: wrap;
        align-items: center;
        margin-top: var(--espacio-md);
        margin-bottom: var(--espacio-lg);
        padding: var(--espacio-md);
        background: var(--bg-card);
        border-radius: var(--radio-md);
        border: 1px solid var(--border);
    }

    .filtros-titulo {
        font-size: var(--texto-sm);
        font-weight: 600;
        color: var(--texto-secundario);
        white-space: nowrap;
    }

    .cripto-celda {
        display: flex;
        align-items: center;
        gap: var(--espacio-md);
    }

    .cripto-icono {
        width: 32px;
        height: 32px;
        border-radius: var(--radio-full);
    }

    .skeletons {
        display: flex;
        flex-direction: column;
        gap: var(--espacio-md);
    }

    .estado-vacio {
        text-align: center;
        padding: var(--espacio-2xl);
        color: var(--texto-secundario);
    }

    .vacio-icono {
        font-size: 3rem;
        margin-bottom: var(--espacio-md);
    }

    .paginacion {
        display: flex;
        align-items: center;
        justify-content: center;
        gap: var(--espacio-lg);
        padding-top: var(--espacio-lg);
        border-top: 1px solid var(--border);
    }

    .paginacion-info {
        font-size: var(--texto-sm);
        color: var(--texto-secundario);
    }

    @media (max-width: 480px) {
        .filtros-barra {
            flex-direction: column;
        }
        .filtros-barra select,
        .filtros-barra input {
            max-width: 100%;
            width: 100%;
        }
    }
    
</style>