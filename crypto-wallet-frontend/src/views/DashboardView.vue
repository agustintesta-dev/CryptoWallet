<!-- src/views/DashboardView.vue -->
<template>
    <div class="animar-entrada">

        <!-- Saludo personalizado -->
        <div class="pagina-header">
            <div>
                <h2 class="pagina-titulo">Bienvenido, {{ authStore.nombreUsuario }} 👋</h2>
                <p class="texto-secundario">Resumen de la cartera al {{ fechaHoy }}</p>
            </div>
            <RouterLink to="/transacciones/nueva" class="btn btn-primario">
                + Nueva Transacción
            </RouterLink>
        </div>

        <!-- Loader -->
        <div v-if="cargando" class="resumen-grid">
            <div v-for="i in 4" :key="i" class="skeleton" style="height: 110px;"></div>
        </div>

        <!-- Error -->
        <div v-else-if="error" class="banner-error">⚠️ {{ error }}</div>
        
        <template v-else>

            <!-- Cards de resumen -->
            <div class="resumen-grid">
                <div class="card resumen-card">
                    <p class="resumen-label"><Wallet :size="14"/> Valor total de cartera</p>
                    <p class="resumen-valor">{{ formatearARS(portfolio.valorTotalARS) }}</p>
                    <p class="texto-muted resumen-sub">A precios actuales</p>
                </div>
                <div class="card resumen-card">
                    <p class="resumen-label"><TrendingUp size="14"/> Total invertido</p>
                    <p class="resumen-valor">{{ formatearARS(portfolio.totalInvertidoARS) }}</p>
                    <p class="texto-muted resumen-sub">Suma de todas tus compras</p>
                </div>
                <div class="card resumen-card">
                    <p class="resumen-label"><BarChart2 size="14"/> Ganancia / Pérdida</p>
                    <p
                        class="resumen-valor"
                        :class="portfolio.gananciaPerdida >= 0 ? 'texto-exito' : 'texto-peligro'"
                    >
                        {{ portfolio.gananciaPerdida >= 0 ? '+' : '' }}
                        {{ formatearARS(portfolio.gananciaPerdida) }}
                    </p>

                    <p
                        class="resumen-sub"
                        :class="portfolio.gananciaPerdida >= 0 ? 'texto-exito' : 'texto-peligro'" 
                    >
                        {{ portfolio.gananciaPerdida >= 0 ? '▲' : '▼' }}
                        {{ portfolio.porcentajeGananciaPerdida?.toFixed(2) }}%
                    </p>
                </div>

                <div class="card resumen-card">
                    <p class="resumen-label"><ArrowLeftRight size="14"/> Transacciones</p>
                    <p class="resumen-valor">{{ totalTransacciones }}</p>
                    <p class="texto-muted resumen-sub">
                        {{ totalCompras }} compras · {{ totalVentas }} ventas
                    </p>
                </div>
            </div>

            <div v-if="historialPortfolio.length > 1" class="card" style="margin-bottom: 1.5rem;">
                <h3 class="seccion-titulo" style="margin-bottom: 1rem;">Evolución del portfolio</h3>
                <PortfolioLineChart :datos="historialPortfolio" />
            </div>
        
            <!-- Sin actividad -->
            <div v-if="transacciones.length === 0" class="card estado-vacio">
                <p style="font-size: 3rem; margin-bottom: 1rem;">📭</p>
                <h3>Todavía no registraste ninguna transacción</h3>
                <p class="texto-secundario" style="margin: 0.5rem 0 1.5rem;">
                    Empezá registrando tu primera compra de criptomonedas
                </p>
                <RouterLink to="/transacciones/nueva" class="btn btn-primario">
                    + Registrar primera compra
                </RouterLink>
            </div>

            <template v-else>

                <!-- Criptos en cartera -->
                <div v-if="portfolio.tenencias?.length > 0" class="card" style="margin-bottom: 1.5rem;">
                    <h3 class="seccion-titulo" style="margin-bottom: 1rem;">Criptos en cartera</h3>
                    <div class="tenencias-lista">
                        <div
                            v-for="item in portfolio.tenencias"
                            :key="item.codigoCripto"
                            class="tenencia-item"
                        >
                            <img 
                                :src="item.urlIcono"
                                :alt="item.simboloCripto"
                                class="tenencia-icono"
                                @error="(e) => e.target.style.display = 'none'"
                            />
                            <div class="tenencia-info">
                                <p class="tenencia-nombre">{{ item.nombreCripto }}</p>
                                <p class="texto-muted" style="font-size: 0.75rem;">
                                    {{ item.cantidad }} {{ item.simboloCripto }}
                                </p>
                            </div>
                            <div class="tenencia-valores">
                                <p class="tenencia-valor">{{ formatearARS(item.valorActualARS) }}</p>
                                <p
                                    class="tenencia-cambio"
                                    :class="item.gananciaPerdida >= 0 ? 'texto-exito' : 'texto-peligro'"
                                >
                                    {{ item.gananciaPerdida >= 0 ? '+' : '' }}
                                    {{ item.porcentajeGananciaPerdida?.toFixed(2) }}%
                                </p>
                            </div>
                        </div>
                    </div>
                </div>

                <!-- Últimas 5 transacciones -->
                <div class="card">
                    <div class="tabla-wrapper">
                        <div class="seccion-header-inline">
                            <h3 class="seccion-titulo">Ultimas transacciones</h3>
                            <RouterLink to="/transacciones" class="btn btn-ghost" style="font-size: 0.8rem; padding: 0.3rem 0.8rem;">
                                Ver todas →
                            </RouterLink>
                        </div>
                        <table class="tabla" style="margin-top: 1rem;">
                            <thead>
                                <tr>
                                    <th>Cripto</th>
                                    <th>Tipo</th>
                                    <th>Cantidad</th>
                                    <th>Monto</th>
                                    <th>Fecha</th>
                                </tr>
                            </thead>
                            <tbody>
                                <tr v-for="t in ultimasTransacciones" :key="t.id">
                                    <td>
                                        <div class="flex gap-sm" style="align-items: center;">
                                            <img 
                                                :src="t.urlIconoCripto" 
                                                :alt="t.simboloCripto"
                                                style="width: 24px; height: 24px; border-radius: 50%;"
                                                @error="(e) => e.target.style.display = 'none'"
                                            >
                                            <span class="semibold">{{ t.simboloCripto }}</span>
                                        </div>
                                    </td>
                                    <td>
                                        <span :class="t.accion === 'compra' ? 'badge badge-compra' : 'badge badge-venta'">
                                            {{ t.accion }}
                                        </span>
                                    </td>
                                    <td>{{ t.cantidadCripto }}</td>
                                    <td class="semibold">{{ formatearARS(t.monto) }}</td>
                                    <td class="texto-secundario">{{ formatearFecha(t.fechaTransaccion) }}</td>
                                </tr>
                            </tbody>
                        </table>
                    </div>
                </div>
            </template>

        </template>
    </div>
</template>

<script setup>
    import { ref, computed, onMounted} from 'vue'
    import { useAuthStore  } from '@/stores/auth';
    import { transaccionesApi } from '@/services/api';
    import { usePortfolioStore } from '@/stores/portfolio';
    import { portfolioApi } from '@/services/api';
    import PortfolioLineChart from '@/components/charts/PortfolioLineChart.vue';
    import { Wallet, TrendingUp, BarChart2, ArrowLeftRight } from 'lucide-vue-next';

    const authStore = useAuthStore()
    const portfolioStore = usePortfolioStore()
    const cargando = ref(true)
    const error = ref('')
    const transacciones = ref([])
    const portfolio = computed( () => portfolioStore.datos)
    const historialPortfolio = ref([])
    

    // Fecha de hoy formateada
    const fechaHoy = new Date().toLocaleDateString('es-AR', {
        weekday: 'long', day: 'numeric', month:'long', year: 'numeric'
    })

    // Computed del resumen
    const totalTransacciones = computed(() => transacciones.value.length)
    const totalCompras = computed(() => transacciones.value.filter(t => t.accion === 'compra').length)
    const totalVentas = computed(() => transacciones.value.filter(t => t.accion === 'venta').length)
    const ultimasTransacciones = computed (() => transacciones.value.slice(0, 5))

    const formatearARS = (valor) => 
        new Intl.NumberFormat('es-AR', { style: 'currency', currency: 'ARS'}).format(valor || 0)
    
    const formatearFecha = (fecha) =>
        new Date(fecha).toLocaleDateString('es-AR', {
            day: '2-digit', month: '2-digit', year: 'numeric'
        })
    
    onMounted(async () => {
        try {
            // Cargamos transacciones y portfolio en paralelo — más rápido que secuencial
            const [resTransacciones, , resHistorial] = await Promise.all([
                transaccionesApi.obtenerTodas(),
                portfolioStore.obtener(),
                portfolioApi.obtenerHistorial()
            ])
            transacciones.value = Array.isArray(resTransacciones.data) ? resTransacciones.data : resTransacciones.data.items
            historialPortfolio.value = resHistorial.data
        } catch {
            error.value = 'No se pudo cargar la información. Verificá tu conexión.'
        } finally {
            cargando.value = false
        }
    })

</script>

<style scoped>
    .resumen-grid{
        display: grid;
        grid-template-columns: repeat(auto-fit, minmax(220px, 1fr));
        gap: var(--espacio-lg);
        margin-bottom: var(--espacio-xl);
    }

    .resumen-card {cursor: default;}

    .resumen-card:hover {
        transform: none;
        box-shadow: none;
        border-color: var(--border);
    }

    .resumen-label {
        font-size: var(--texto-sm);
        color: var(--texto-secundario);
        margin-bottom: var(--espacio-sm);
    }

    .resumen-valor {
        font-size: var(--texto-2xl);
        font-weight: 700;
        margin-bottom: 4px;
    }

    .resumen-sub{ font-size: var(--texto-sm); font-weight: 600;}

    .estado-vacio {
        text-align: center;
        padding: var(--espacio-2xl);
    }

    /* Tenencias */
    .tenencias-lista {
        display: flex;
        flex-direction: column;
        gap: var(--espacio-sm);
    }

    .tenencia-item {
        display: flex;
        align-items: center;
        gap: var(--espacio-md);
        padding: var(--espacio-md);
        background: var(--bg-secondary);
        border-radius: var(--radio-md);
        transition: background var(--transicion-rapida);
    }

    .tenencia-item:hover { background: var(--bg-card-hover); } 
    .tenencia-icono { width: 36px; height: 36px; border-radius: var(--radio-full); }
    .tenencia-info { flex: 1; }
    .tenencia-nombre { font-weight: 600; font-size: var(--texto-sm); }
    .tenencia-valores { text-align: right; }
    .tenencia-valor { font-weight: 700; font-size: var(--texto-sm); }
    .tenencia-cambio { font-size: var(--texto-xs); font-weight: 600; }

    .seccion-titulo { font-size: var(--texto-lg); font-weight: 700;}
    .seccion-header-inline {
        display: flex;
        justify-content: space-between;
        align-items: center;
    }

    .tabla-wrapper { 
        overflow-x: auto;
    }

    @media (max-width: 768px) {
        .resumen-grid { grid-template-columns: 1fr 1fr;}
    }

</style>