<!-- src/components/layout/AppSidebar.vue -->
<template>
    <aside class="sidebar" :class="{ 'sidebar-cerrado': !estaAbierto }">

        <!-- Logo -->
        <div class="sidebar-logo">
            <span class="logo-icono">₿</span>
            <span class="logo-texto">CryptoWallet</span>
        </div>

        <!-- Navegación -->
        <nav class="sidebar-nav">
            <a
                v-for="item in menu"
                :key="item.ruta"
                class="nav-item"
                :class="{ 'nav-item-activo' : route.path === item.ruta}"
                href="#"
                @click.prevent="navegar(item.ruta)"
            >
                <span class="nav-icono">{{ item.icono }}</span>
                <span class="nav-texto">{{ item.nombre }}</span>
            </a>
        </nav>

        <!-- Footer del sidebar -->
        <div class="sidebar-footer">
            <p class="version">v1.0.0</p>
        </div>

    </aside>
</template>

<script setup>

    import { useRouter, useRoute } from 'vue-router'

    const router = useRouter()
    const route = useRoute()

    // defineProps — declara las propiedades que recibe este componente
    // desde el componente padre (App.vue)
    const props = defineProps({
        estaAbierto: {
            type: Boolean,
            default: true
        },
        esMobile: {
            type: Boolean,
            default: false
        }
    })

    const emit = defineEmits(['cerrar'])

    const navegar = (ruta) => {
        if (props.esMobile) {
            emit('cerrar')
        }
        router.push(ruta)
    }

    // Los ítems del menú — si querés agregar una sección
    // solo agregás un objeto a este array
    const menu = [
        { nombre: 'Dashboard', ruta: '/dashboard', icono: '📊' },
        { nombre: 'Transacciones', ruta: '/transacciones', icono: '📋' },
        { nombre: 'Nueva Compra', ruta: '/transacciones/nueva', icono: '➕' },
        { nombre: 'Mi Cartera', ruta: '/portfolio', icono:'💼' },
        { nombre: 'Mi perfil', ruta: '/perfil', icono: '👤'}
    ]
</script>

<style scoped>
    .sidebar {
        position: fixed;
        top: 0;
        left: 0;
        height: 100vh;
        width: var(--sidebar-ancho);
        background: var(--bg-secondary);
        border-right: 1px solid var(--border);
        display: flex;
        flex-direction: column;
        transition: transform var(--transicion);
        z-index: 100;
    }

    .sidebar-cerrado {
        transform: translateX(-100%);
    }

    /* Logo */
    .sidebar-logo {
        display: flex;
        align-items: center;
        gap: var(--espacio-md);
        padding: var(--espacio-xl) var(--espacio-lg);
        border-bottom: 1px solid var(--border);
    }

    .logo-icono {
        font-size: 1.5rem;
        background: var(--acento);
        width: 40px;
        height: 40px;
        border-radius: var(--radio-md);
        display: flex;
        align-items: center;
        justify-content: center;
    }

    .logo-texto {
        font-size: var(--texto-lg);
        font-weight: 700;
        color: var(--texto-primario);
    }

    /* Nav items */
    .sidebar-nav {
        flex: 1;
        padding: var(--espacio-lg) var(--espacio-md);
        display: flex;
        flex-direction: column;
        gap: var(--espacio-xs);
    }

    .nav-item {
        display: flex;
        align-items: center;
        gap: var(--espacio-md);
        padding: 0.75rem var(--espacio-md);
        border-radius: var(--radio-md);
        color: var(--texto-secundario);
        text-decoration: none;
        font-size: var(--texto-sm);
        font-weight: 500;
        transition: all var(--transicion-rapida);
    }
    
    .nav-item:hover{
        background: var(--bg-card);
        color: var(--texto-primario);
    }

    /* Clase que Vue Router aplica automáticamente cuando la ruta del item coincide con la URL actual  */
    .nav-item-activo {
        background: rgba(108, 99, 255, 0.15);
        color: var(--acento);
        border: 1px solid rgba(108, 99, 255, 0.3);
    }

    .nav-icono { font-size: 1.1rem; }

    /* Footer */
    .sidebar-footer {
        padding: var(--espacio-lg);
        border-top: 1px solid var(--border);
    }

    .version {
        font-size: var(--texto-xs);
        color: var(--texto-muted);
        text-align: center;
    }
</style>    