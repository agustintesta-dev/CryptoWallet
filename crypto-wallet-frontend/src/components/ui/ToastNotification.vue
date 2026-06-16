<!-- src/components/ui/ToastNotification.vue -->
<template>
    <Transition name="toast">
        <div v-if="toast.visible" class="toast" :class="`toast-${toast.tipo}`">
            <span class="toast-icono">{{ toast.tipo === 'exito' ? '✅' : toast.tipo === 'error' ? '❌' : 'ℹ️' }}</span>
            <span class="toast-mensaje">{{ toast.mensaje }}</span>
            <button class="toast-cerrar" @click="toast.cerrar()">✕</button>
        </div>
    </Transition>
</template>

<script setup>
    import { useToastStore } from '@/stores/toast'
    const toast = useToastStore()
</script>

<style scoped>
    .toast { 
        position: fixed;
        bottom: var(--espacio-xl);
        right: var(--espacio-xl);
        display: flex;
        padding: var(--espacio-md);
        border-radius: var(--border);
        align-items: center;
        gap: var(--espacio-md);
        box-shadow: var(--sombra-lg);
        z-index: 999;
        font-size: var(--texto-sm);
        font-weight: 500;
        max-width: 360px;
    }

    .toast-exito {
        background: rgba(0, 212, 170, 0.15);
        border: 1px solid var(--exito);
        color: var(--exito);
    }

    .toast-error {
        background: rgba(255, 77, 109, 0.15);
        border: 1px solid var(--peligro);
        color: var(--peligro);
    }

    .toast-info {
        background: rgba(76, 201, 240, 0.15);
        border: 1px solid var(--info);
        color: var(--info);
    }

    .toast-icono {font-size: 1rem; flex-shrink: 0; }
    .toast-mensaje { flex: 1; }

    .toast-cerrar {
        background: none;
        border: none;
        cursor: pointer;
        color: inherit;
        opacity: 0.6;
        font-size: 0.9rem;
        padding: 0;
        flex-shrink: 0;
    }

    .toast-cerrar:hover { opacity: 1; } 

    .toast-enter-active, .toast-leave-active {
        transition: opacity 0.3s, transform 0.3s;
    }

    .toast-enter-from, .toast-leave-to {
        opacity: 0;
        transform: translateY(20px);
    }

</style>