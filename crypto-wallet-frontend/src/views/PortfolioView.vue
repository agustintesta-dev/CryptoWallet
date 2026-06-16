<!-- src/views/PortfolioView.vue -->
<template>
    <div class="animar-entrada">

        <div class="pagina-header">
            <div>
                <h2 class="pagina-titulo">Mi Cartera</h2>
                <p class="texto-secundario">Estado actual de tus criptomonedas en tiempo real</p>
            </div>
            <button @click="actualizar" class="btn btn-ghost" :disabled="cargando">
                <span :class="{ spinner: cargando}">↻</span>
                Actualizar precios
            </button>
        </div>

        <!-- Loader -->
        <div v-if="cargando" class="loading-grid">
            <div v-for="i in 3" :key="i" class="skeleton" style="height: 110px;"></div>
        </div>

        <!-- Error -->
        <div v-else-if="error" class="banner-error">⚠️ {{ error }}</div>

        <template v-else>

            <!-- Cards de totales -->
            <div class="totales-grid">
                <div class="card total-card">
                    <p class="total-label">Valor total</p>
                    <p class="total-valor">{{ formatearARS(portfolio.valorTotalARS) }}</p>
                </div>
                <div class="card total-card">
                    <p class="total-label">Valor invertido</p>
                    <p class="total-valor">{{ formatearARS(portfolio.totalInvertidoARS) }}</p>
                </div>
                <div class="card total-card">
                    <p class="total-label">Ganancia / Pérdida</p>
                    <p
                        class="total-valor"
                        :class="portfolio.gananciaPerdida >= 0 ? 'texto-exito' : 'texto-peligro'"
                    >
                        {{ portfolio.gananciaPerdida >= 0 ? '+' : '' }}
                        {{ formatearARS(portfolio.gananciaPerdida) }}
                    </p>
                    <p
                        class="total-porcentaje"
                        :class="portfolio.gananciaPerdida >= 0 ? 'texto-exito' : 'texto-peligro'"
                    >   
                        {{ portfolio.porcentajeGananciaPerdida?.toFixed(2) }}%
                    </p>
                </div>
            </div>

            <!-- Sin tenencias -->
            <div v-if="!portfolio.tenencias?.length" class="card estado-vacio">
                <p style="font-size: 3rem; margin-bottom: 1rem;">📊</p>
                <h3>No tenés criptomonedas en cartera</h3>
                <p class="texto-secundario" style="margin: 0.5rem 0 1.5rem;">
                    Registrá una compra para ver tu cartera aqui
                </p>
                <RouterLink to="/transacciones/nueva" class="btn btn-primario">
                    + Registrar compra
                </RouterLink>
            </div>

            <!-- Tabla de tenencias -->
            <div v-else class="card">
                <h3 class="seccion-titulo" style="margin-bottom: 1rem;">
                    Detalle de posiciones
                </h3>
                <div class="tabla-wrapper"> 
                    <table class="tabla">
                        <thead>
                            <tr>
                                <th>Criptomoneda</th>
                                <th>Cantidad</th>
                                <th>Precio actual</th>
                                <th>Valor actual</th>
                                <th>Invertido</th>
                                <th>G/P</th>
                                <th>%</th>
                            </tr>
                        </thead>
                        <tbody>
                            <tr v-for="item in portfolio.tenencias" :key="item.codigoCripto">
                                <td>
                                    <div class="cripto-celda">
                                        <img 
                                            :src="item.urlIcono" 
                                            :alt="item.simboloCripto"
                                            class="cripto-icono"
                                            @error="(e) => e.target.style.display = 'none'"
                                        />
                                        <div>
                                            <p class="semibold">{{ item.nombreCripto }}</p>
                                            <p class="texto-muted" style="font-size: 0.75rem;">
                                                {{ item.simboloCripto }}
                                            </p>
                                        </div>
                                    </div>
                                </td>
                                <td>{{ formatearCantidad(item.cantidad) }}</td>
                                <td>
                                    <span v-if="item.precioActualARS === 0" class="texto-muted" style="font-size: 0.75rem;">
                                        ⚠️ Sin precio
                                    </span>
                                    <span v-else>{{ formatearARS(item.precioActualARS) }}</span>
                                </td>
                                <td class="semibold">{{ formatearARS(item.valorActualARS) }}</td>
                                <td>{{ formatearARS(item.invertidoARS) }}</td>
                                <td :class="item.gananciaPerdida >= 0 ? 'texto-exito' : 'texto-peligro'">
                                    {{ item.gananciaPerdida >= 0 ? '+' : ''}}
                                    {{ formatearARS(item.gananciaPerdida) }}
                                </td>
                                <td>
                                    <span
                                        class="porcentaje-badge"
                                        :class="item.gananciaPerdida >= 0 ? 'porcentaje-exito' : 'porcentaje-peligro'"
                                    >
                                        {{ item.gananciaPerdida >= 0 ? '+' : '' }}
                                        {{ item.porcentajeGananciaPerdida?.toFixed(2) }}%
                                    </span>
                                </td>
                            </tr>
                        </tbody>
                    </table>
                </div>

                <!-- Timestamp de la última actualización -->
                <p class="ultima-actualizacion">
                    Ultima actualizacion: {{ horaActualizacion }}
                </p>
            </div>

        </template>
    </div>
</template>

<script setup>
    import { ref, computed, onMounted} from 'vue'
    import { usePortfolioStore } from '@/stores/portfolio'

    const portfolioStore = usePortfolioStore()
    const cargando = ref(true)
    const error = ref('')
    const portfolio = computed( () => portfolioStore.datos)

    const horaActualizacion = ref('')

    const formatearARS = (valor) => 
        new Intl.NumberFormat('es-AR', { style: 'currency', currency: 'ARS' }).format(valor || 0)

    // Para cantidades pequeñas de cripto mostramos hasta 8 decimales
    const formatearCantidad = (valor) => {
        if (!valor) return '0'
        // Si es un número muy pequeño, mostramos más decimales
        if (valor < 0.0001) return valor.toFixed(8)
        if (valor < 1) return valor.toFixed(6)
        return valor.toFixed(4)
    }

    const actualizar = async () => {
        cargando.value = true
        error.value = ''
        try {
            await portfolioStore.obtener(true)
            horaActualizacion.value = new Date().toLocaleTimeString('es-AR')
        } catch {
            error.value = 'No se pudo obtener los precios actuales. Intentá de nuevo.'
        } finally {
            cargando.value = false
        }
    }

    onMounted(async () => {
        try {
            await portfolioStore.obtener()
            horaActualizacion.value = new Date().toLocaleTimeString('es-AR')
        } catch {
            error.value = 'No se pudo obtener los precios actuales. Intentá de nuevo.'
        } finally {
            cargando.value = false
        }
        
    })

</script>

<style scoped>

    .totales-grid {
        display: grid;
        grid-template-columns: repeat(auto-fit, minmax(200px, 1fr));
        gap: var(--espacio-lg);
        margin-bottom: var(--espacio-xl);
    }

    .total-card { cursor: default; }
    .total-card:hover { transform: none; box-shadow: none; border-color: var(--border);}
    .total-label { font-size: var(--texto-sm); color: var(--texto-secundario); margin-bottom: var(--espacio-sm);}
    .total-valor { font-size: var(--texto-2xl); font-weight: 700;}
    .total-porcentaje { font-size: var(--texto-sm); font-weight: 600; margin-top: 2px; }

    .loading-grid {
        display: grid;
        grid-template-columns: repeat(3, 1fr);
        gap: var(--espacio-lg);
        margin-bottom: var(--espacio-xl);
    }

    .estado-vacio {
        text-align: center;
        padding: var(--espacio-2xl);
    }

    /* Tabla */
    .tabla-wrapper { overflow-x: auto; }
    
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

    /* Badge de porcentaje */
    .porcentaje-badge {
        display: inline-flex;
        align-items: center;
        padding: 0.2rem 0.6rem;
        border-radius: var(--radio-full);
        font-size: var(--texto-xs);
        font-weight: 700
    }

    .porcentaje-exito {
        background: rgba(0, 212, 170, 0.15);
        color: var(--exito);
    }

    .porcentaje-peligro {
        background: rgba(255, 77, 109, 0.15);
        color: var(--peligro);
    }

    .seccion-titulo { font-size: var(--texto-lg); font-weight: 700; }

    .ultima-actualizacion{
        margin-top: var(--espacio-lg);
        font-size: var(--texto-xs);
        color: var(--texto-muted);
        text-align: right;
    }

    @media (max-width: 768px) {
        .totales-grid { grid-template-columns: 1fr; }
        .loading-grid { grid-template-columns: 1fr; }
    }

</style>