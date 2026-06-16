<!-- src/components/layout/AppNavbar.vue -->
<template>
    <header class="navbar">
        <!-- Botón para abrir/cerrar el sidebar -->
        <button class="btn-menu" @click="$emit('toggle-sidebar')">
            ☰
        </button>

        <!-- Título de la página actual -->
        <h1 class="navbar-titulo">{{ tituloActual }}</h1>

        <!-- Acciones rápidas -->
        <div class="navbar-acciones">
            <!-- Menú del usuario -->
            <div class="usuario-menu">
                <div class="usuario-avatar" @click="toggleMenu">
                    {{ inicialUsuario }}
                </div>

                <!-- Overlay para cerrar al hacer clic afuera -->
                <div v-if="menuAbierto" class="overlay" @click="menuAbierto = false"></div>

                <!-- Dropdown -->
                <div v-if="menuAbierto" class="usuario-dropdown">
                    <!-- Header con avatar y email -->
                    <div class="dropdown-header">
                        <div class="dropdown-avatar">{{ inicialUsuario }}</div>
                        <div>
                            <p class="usuario-nombre">{{ authStore.nombreUsuario }}</p>
                            <p class="usuario-email">{{ authStore.usuario?.email }}</p>
                        </div>
                    </div>
                    <hr class="dropdown-divider">
                    <RouterLink to="/perfil" class="dropdown-item" @click="menuAbierto = false">
                        👤 Mi Perfil
                    </RouterLink>
                    <button class="dropdown-item dropdown-item-peligro" @click="cerrarSesion">
                        🚪 Cerrar sesión
                    </button>
                </div>
            </div>
        </div>

    </header>
</template>

<script setup>

    import { ref, computed } from 'vue';
    import { useRoute, useRouter } from 'vue-router';
    import { useAuthStore } from '@/stores/auth';

    // useRoute — hook de Vue Router que nos da información

    const ruta = useRoute();
    const router = useRouter();
    const authStore = useAuthStore();
    const menuAbierto = ref(false)


    // computed — valor que se recalcula automáticamente cada vez que cambia la ruta
    const tituloActual = computed(() => ruta.meta.titulo || 'CryptoWallet')

    // Primera letra del nombre para el avatar
    const inicialUsuario = computed(() => 
        authStore.nombreUsuario?.charAt(0).toUpperCase() || '?'
    )

    const toggleMenu = () => menuAbierto.value = !menuAbierto.value

    const cerrarSesion = () => {
        authStore.logout()
        menuAbierto.value = false
        router.push('/login')
    }

    // defineEmits — declara los eventos que este componente puede emitir hacia el padre
    defineEmits(['toggle-sidebar'])

</script>

<style scoped>
    
    .navbar {
        height: var(--navbar-alto);
        background: var(--bg-secondary);
        border-bottom: 1px solid var(--border);
        display: flex;
        align-items: center;
        gap: var(--espacio-xl);
        padding: 0 var(--espacio-xl);
        position: sticky;
        top: 0;
        z-index: 50;
    }

    .btn-menu {
        background: none;
        border: none;
        color: var(--texto-secundario);
        font-size: 1.25rem;
        cursor: pointer;
        padding: var(--espacio-sm);
        border-radius: var(--radio-sm);
        transition: color var(--transicion-rapida);
    }

    .btn-menu:hover {
        color: var(--texto-primario);
    }

    .navbar-titulo {
        flex: 1;
        font-size: var(--texto-xl);
        font-weight: 600;
    }

    .navbar-acciones {
        display: flex;
        align-items: center;
        gap: var(--espacio-md);
    }

    /* Avatar del usuario */
    .usuario-menu {
        position: relative;
    }

    .usuario-avatar {
        width: 36px;
        height: 36px;
        border-radius: var(--radio-full);
        background: var(--acento);
        color: white;
        display: flex;
        align-items: center;
        justify-content: center;
        font-weight: 700;
        font-size: var(--texto-sm);
        transition: opacity var(--transicion-rapida);
        user-select: none;
        cursor: pointer;
    }

    .usuario-avatar:hover { opacity: 0.85; }

    /* Overlay invisible — cierra el dropdown al clicar afuera */
    .overlay {
        position: fixed;
        inset: 0;
        z-index: 98;
    }

    .dropdown-header {
        display: flex;
        align-items: center;
        gap: var(--espacio-md);
        padding: var(--espacio-md);
        background: var(--bg-secondary);
        border-radius: var(--radio-md);
        margin-bottom: var(--espacio-xs);
    }

    .dropdown-avatar {
        width: 40px;
        height: 40px;
        border-radius: var(--radio-full);
        background: var(--acento);
        color: white;
        display: flex;
        align-items: center;
        justify-content: center;
        font-weight: 700;
        font-size: var(--texto-lg);
        flex-shrink: 0;
    }

    .usuario-email {
        font-size: var(--texto-xs);
        color: var(--texto-muted);
        margin-top: 2px;
    }

    /* Dropdown */
    .usuario-dropdown {
        position: absolute;
        top: calc(100% + 8px);
        right: 0;
        background: var(--bg-card);
        border: 1px solid var(--border);
        border-radius: var(--radio-md);
        padding: var(--espacio-sm);
        min-width: 180px;
        box-shadow: var(--sombra-lg);
        z-index: 100;
    }

    .usuario-nombre {
        padding: var(--espacio-sm) var(--espacio-md);
        font-weight: 600;
        font-size: var(--texto-sm);
        color: var(--texto-primario);
    }

    .dropdown-divider {
        border: none;
        border-top: 1px solid var(--border);
        margin: var(--espacio-xs) 0; 
    }

    .dropdown-item{
        display: flex;
        align-items: center;
        gap: var(--espacio-sm);
        padding: var(--espacio-sm) var(--espacio-md);
        border-radius: var(--radio-sm);
        color: var(--texto-secundario);
        font-size: var(--texto-sm);
        text-decoration: none;
        background: none;
        border: none;
        width: 100%;
        cursor: pointer;
        transition: background var(--transicion-rapida);
        font-family: var(--fuente);
    }

    .dropdown-item:hover { background: var(--bg-card-hover); color: var(--texto-primario); }
    .dropdown-item-peligro:hover { color: var(--peligro);}

</style>