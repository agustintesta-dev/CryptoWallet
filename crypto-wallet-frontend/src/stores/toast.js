// src/stores/toast.js
import { defineStore } from 'pinia'
import { ref } from 'vue'

export const useToastStore = defineStore('toast', () => {
    const mensaje = ref('')
    const tipo = ref('exito')
    const visible = ref(false)
    let temporizador = null

    const mostrar = (nuevoMensaje, nuevoTipo = 'exito', duracion = 3000) => {
        if (temporizador) clearTimeout(temporizador)
        mensaje.value = nuevoMensaje
        tipo.value = nuevoTipo
        visible.value = true
        temporizador = setTimeout(() => { visible.value = false }, duracion )
    }

    const cerrar = () => {
        visible.value = false
        if (temporizador) clearTimeout(temporizador) 
    }

    return { mensaje, tipo, visible, mostrar, cerrar }
})