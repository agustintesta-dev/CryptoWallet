<template>
    <div class="chart-wrapper">
        <canvas ref="canvasRef"></canvas>
    </div>
</template>

<script setup>
    import { ref, onMounted, watch, onUnmounted} from 'vue'
    import { Chart, LineElement, PointElement, LineController,
        CategoryScale, LinearScale, Tooltip, Filler } from 'chart.js'

    Chart.register(LineElement, PointElement, LineController, 
        CategoryScale, LinearScale, Tooltip, Filler)
    
    const props = defineProps ({
        datos: { type: Array, required: true}
    })

    const canvasRef = ref(null)
    let instanciaChart = null
    
    const crearChart = () => {
        if(instanciaChart) {
            instanciaChart.destroy()
            instanciaChart = null
        } 
        if(!canvasRef.value || !props.datos.length) return

        const ctx = canvasRef.value.getContext('2d')
        const gradient = ctx.createLinearGradient(0, 0, 0, 300)
        gradient.addColorStop(0, 'rgba(108, 99, 255, 0.3)')
        gradient.addColorStop(1, 'rgba(108, 99, 255, 0)')

        instanciaChart = new Chart(ctx, {
            type: 'line',
            data: {
                labels: props.datos.map(d => d.Fecha),
                datasets: [{
                    label: 'Capital invertido (ARS)',
                    data: props.datos.map(d => d.ValorARS),
                    borderColor: '#6c63ff',
                    backgroundColor: gradient,
                    borderWidth: 2,
                    pointRadius: 0,
                    pointHoverRadius: 4,
                    fill: true,
                    tension: 0.4
                }]
            },
            options: {
                responsive: true,
                maintainAspectRatio: false,
                plugins: {
                    legend: { display: false },
                    tooltip: {
                        callbacks: {
                            label: ctx => new Intl.NumberFormat('es-AR', {
                                style: 'currency', currency: 'ARS'
                            }).format(ctx.parsed.y)
                        }
                    }
                },
                scales: {
                    x: {
                        grid: { color: '#2a2d3e' },
                        ticks: { color: '#8b8fa8', maxTicksLimit: 6 } 
                    },
                    y: {
                        grid: { color: '#2a2d3e' },
                        ticks: {
                            color: '#8b8fa8',
                            callback: v => new Intl.NumberFormat('es-AR', {
                                notation: 'compact', currency: 'ARS', style: 'currency'
                            }).format(v)
                        }
                    }
                }
            }
        })
    }

    watch(() => props.datos, crearChart, { deep: true })
    onMounted(crearChart)
    onUnmounted(() => instanciaChart?.destroy())

</script>

<style scoped>
    .chart-wrapper {
        position: relative;
        height: 260px;
        width: 100%;
    }
</style>