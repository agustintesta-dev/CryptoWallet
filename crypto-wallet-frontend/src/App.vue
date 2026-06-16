<!-- src/App.vue -->
<!-- Componente raíz — es el contenedor de toda la app -->
<!-- RouterView renderiza la vista según la URL actual -->
<template>
  <div class="app-layout"> 

    <!-- El sidebar y navbar solo aparecen si el usuario está logueado -->
    <template v-if="authStore.estaAutenticado">
      <AppSidebar :esta-abierto="sidebarAbierto" :es-mobile="esMobile" @cerrar="cerrarSidebar"/>

      <div
        v-if="sidebarAbierto && esMobile"
        class="sidebar-overlay"
        @click="sidebarAbierto = false"
      ></div>


      
      <!-- Contenido principal -->
      <div class="contenido-principal" :style="{marginLeft: sidebarAbierto && !esMobile ? 'var(--sidebar-ancho)' : '0px'}">
        
        <!-- Navbar superior -->
        <AppNavbar @toggle-sidebar="sidebarAbierto = !sidebarAbierto"/>
        
        <!-- Área de contenido — cada vista se renderiza acá -->
        <main class="area-contenido">
          <RouterView v-slot="{ Component }">
            <Transition name="pagina" mode="out-in">
              <component :is="Component" />
            </Transition>
          </RouterView>
        </main>
      </div>
    </template>

    <!-- Páginas de auth — sin sidebar ni navbar -->
    <template v-else>
      <RouterView v-slot="{ Component }">
        <Transition name="pagina" mode="out-in">
          <component :is="Component"/>
        </Transition>
      </RouterView>
    </template>

    <ToastNotification />

  </div>
</template>

<script setup>

  import { ref, computed, onMounted, onUnmounted } from 'vue'
  import AppSidebar from '@/components/layout/AppSidebar.vue';
  import AppNavbar from '@/components/layout/AppNavbar.vue';
  import { useAuthStore } from '@/stores/auth';
  import ToastNotification from '@/components/ui/ToastNotification.vue';

   
  const authStore = useAuthStore()

  // ref — hace que la variable sea reactiva.
  // Cuando cambia, Vue actualiza la pantalla automáticamente.
  const sidebarAbierto = ref(window.innerWidth > 768)

  // Detecta si es mobile para no aplicar el margen
  const anchoPantalla = ref(window.innerWidth)
  const esMobile = computed(() => anchoPantalla.value <= 768)

  const cerrarSidebar = () => {
    sidebarAbierto.value = false
  }

  const actualizarAncho = () => {
    anchoPantalla.value = window.innerWidth
    if (window.innerWidth <= 768) {
      sidebarAbierto.value = false
    }
  }
  onMounted(() => window.addEventListener('resize', actualizarAncho))
  onUnmounted(() => window.removeEventListener('resize', actualizarAncho))

</script>


<style>

  /* Transición entre páginas */
  .pagina-enter-active,
  .pagina-leave-active {
    transition: opacity 0.2s, transform 0.2s;
  }

  .pagina-enter-from {
    opacity: 0;
    transform: translateY(10px);
  }

  .pagina-leave-to {
    opacity: 0;
    transform: translateY(-10px);
  }

  .sidebar-overlay {
    position: fixed;
    inset: 0;
    background: rgba(0, 0, 0, 0.6);
    z-index: 99;
  }

</style>
