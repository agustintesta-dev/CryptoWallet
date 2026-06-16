// src/router/index.js
// ============================================
// El router define qué componente mostrar
// según la URL que el usuario visita.
// ============================================

import { createRouter, createWebHistory } from 'vue-router'

const router = createRouter({
  history: createWebHistory(),
  routes: [
    // ── Rutas públicas — no requieren login ───────────
    {
      path: '/login',
      name:'Login',
      component: () => import('@/views/auth/LoginView.vue'),
      meta: { titulo: 'Iniciar Sesión', publica: true }
    },
    {
      path: '/registro',
      name:'Registro',
      component: () => import('@/views/auth/RegistroView.vue'),
      meta: { titulo: 'Crear Cuenta', publica: true }
    },
    // ── Rutas protegidas — requieren login ────────────
    {
      path: '/',
      redirect: '/dashboard'  // / redirige al dashboard
    },
    {
      path: '/dashboard',
      name: 'Dashboard',
      component: () => import('@/views/DashboardView.vue'),
      meta: { titulo: 'Dashboard' }
    },
    {
      path: '/transacciones',
      name: 'Transacciones',
      component: () => import('@/views/TransaccionesView.vue'),
      meta: { titulo: 'Historial' }
    },
    {
      path: '/transacciones/nueva',
      name: 'NuevaTransaccion',
      component: () => import('@/views/NuevaTransaccionView.vue'),
      meta: { titulo: 'Nueva Transacción' }
    },
    {
      path: '/transacciones/:id/editar',
      name: 'EditarTransaccion',
      component: () => import('@/views/EditarTransaccionView.vue'),
      meta: { titulo: 'Editar Transacción' }
    },
    {
      path: '/portfolio',
      name: 'Portfolio',
      component: () => import('@/views/PortfolioView.vue'),
      meta: { titulo: 'Mi Cartera' }
    },
    {
      path:'/perfil',
      name: 'Perfil',
      component: () => import('@/views/auth/PerfilView.vue'),
      meta: { titulo: 'Mi perfil' }
    }
  ]
})

// Cambia el título de la pestaña del browser en cada navegación
router.beforeEach((to) => {
  document.title = to.meta?.titulo
    ? `${to.meta.titulo} | CryptoWallet`
    : 'CryptoWallet'

  const token = localStorage.getItem('token')

  // Si la ruta es pública y el usuario ya está logueado
  // lo mandamos al dashboard directamente
  if (to.meta.publica && token) {
    return { name: 'Dashboard' }
  }

  // Si la ruta requiere login y no hay token
  // lo mandamos al login
  if (!to.meta.publica && !token){
    return { name: 'Login' }
  }
})

router.afterEach(() => {
  window.scrollTo(0, 0)
})

export default router
