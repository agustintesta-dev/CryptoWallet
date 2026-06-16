import { defineStore } from 'pinia'
import { portfolioApi } from '@/services/api'

export const usePortfolioStore = defineStore('portfolio', {
    state: () => ({
        datos: {
            valorTotalARS : 0,
            totalInvertidoARS: 0,
            gananciaPerdida: 0,
            porcentajeGananciaPerdida: 0,
            tenencias: []

        },
        cargando: false,
        cargado: false
    }),

    actions: {
        async obtener(forzar = false) {
            if (this.cargado && !forzar ) return

            this.cargando = true
            try {
                const { data } = await portfolioApi.obtener()
                this.datos = data
                this.cargado = true
            } finally {
                this.cargado = false
            }
        },

        invalidar() {
            this.cargado = false
        }
    }
})