// src/stores/auth.js
// ============================================
// Store de autenticación — maneja el estado
// del usuario logueado en toda la app.
// ============================================

import {defineStore} from 'pinia'
import { authApi } from '@/services/api'

export const useAuthStore = defineStore('auth', {
    state: () => ({
        // Intentamos recuperar el usuario del localStorage
        // para que la sesión persista al recargar la página
        usuario: JSON.parse(localStorage.getItem('usuario') || 'null' ),
        token: localStorage.getItem('token') || null,
        cargando: false,
        error: null
    }),

    getters: {
        // ¿Está logueado?
        estaAutenticado: (state) => !!state.token,

        // Nombre del usuario para mostrarlo en la UI
        nombreUsuario: (state) => state.usuario?.nombre || '' 
    },

    actions: {
        async login(datos) {
            this.cargando = true
            this.error = null
            try {
                const { data } = await authApi.login(datos)
                this._guardarSesion(data)
            } catch (error) {
                this.error = error.message
                throw error
            } finally {
                this.cargando = false
            }
        },

        async registro(datos) {
            this.cargando = true
            this.error = null
            try {
                const { data } = await authApi.registro(datos)
                this._guardarSesion(data)
            } catch (error) {
                this.error = error.message
                throw error
            } finally {
                this.cargando = false
            }
        },

        logout() {
            this.usuario = null
            this.token = null
            localStorage.removeItem('token')
            localStorage.removeItem('usuario')
        },

        // Método interno — guarda la sesión en el store y en localStorage
        _guardarSesion(data) {
            this.token = data.token
            this.usuario = { id: data.id, nombre: data.nombre, email: data.email }
            localStorage.setItem('token', data.token)
            localStorage.setItem('usuario', JSON.stringify(this.usuario))
        }
    }
})