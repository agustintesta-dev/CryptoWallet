// src/stores/transacciones.js
// ============================================
// Store de Pinia — estado global de transacciones.
// Cualquier componente puede leer y modificar
// estos datos sin pasar props entre componentes.
// ============================================
import { defineStore } from 'pinia'
import { transaccionesApi, criptosApi } from '@/services/api'

export const useTransaccionesStore = defineStore('transacciones', {
    // ── Estado ────────────────────────────────────────────
    // Son los datos que persisten mientras la app está abierta
    state: () => ({
        transacciones: [],  // Lista completa
        criptos: [],
        paginacion: { total: 0, pagina: 1, tamañoPagina: 20, totalPaginas: 0},
        cargando: false,    // Para mostrar spinners
        error: null     // Para mostrar mensajes de error
    }),

    // ── Actions ───────────────────────────────────────────
    // Operaciones asíncronas que modifican el estado
    actions: {
        async obtenerTodas(pagina = 1, filtros = {}) {
            this.cargando = true
            this.error = null
            try {
                const { data } = await transaccionesApi.obtenerTodas(
                    pagina, 
                    this.paginacion.tamañoPagina,
                    filtros.codigoCripto || undefined,
                    filtros.accion || undefined,
                    filtros.fechaDesde || undefined,
                    filtros.fechaHasta || undefined,
                )
                this.transacciones = data.items
                this.paginacion = {
                    total: data.total,
                    pagina: data.pagina,
                    tamañoPagina: data.tamañoPagina,
                    totalPaginas: data.totalPaginas
                }
            } catch (error) { 
                this.error = error.message
            } finally {
                // finally siempre se ejecuta — haya error o no
                this.cargando = false
            }
        },

        async obtenerCriptos() {
            try {
                const { data } = await criptosApi.obtenerTodas()
                this.criptos = data
            } catch (error) {
                console.error('Error al cargar criptos:' ,error)
            }
        },

        async crear(datos) {
            const { data } = await transaccionesApi.crear(datos)
            // Agrega al inicio sin recargar toda la lista
            this.transacciones.unshift(data)
            return data
        },

        async editar(id, datos) {
            await transaccionesApi.editar(id, datos)
            const { data } = await transaccionesApi.obtenerPorId(id)
            const index = this.transacciones.findIndex( t => t.id === id)
            if (index !== -1) {
                this.transacciones[index] = data
            }
        },

        async eliminar(id) {
            await transaccionesApi.eliminar(id)
            // Filtra el eliminado de la lista local
            this.transacciones = this.transacciones.filter(t => t.id !== id)
        }
    }
})


